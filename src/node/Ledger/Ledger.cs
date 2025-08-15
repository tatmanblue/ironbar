using System;
using System.IO;
using Microsoft.Extensions.Logging;
using core.Ledger;
using core.Security;
using core.Utility;
using Node.Interfaces;

namespace Node.Ledger;

/// <summary>
/// A ledger, which consists of blockchain entries
/// There is at least 1 ledger, most likely multiple.  The
/// ledger with ID=1 is the master ledger.  All content for one ledger is
/// written to single directory.
/// 
/// A ledger consists of an index, which is the list of all
/// blocks in the ledger, and blocks, which is the data of the ledger.
///
/// A block has two parts.  The block itself and a consensus entry which
/// confirms all nodes running at the time agreed to the block.  The
/// block and the consensus entry are written to separate files.
/// </summary>
public class Ledger : ILedger
{
    #region ILedger properties
    /// <summary>
    /// Ledger ID.  1 is master ledger.  It is controlled by the nodes
    /// with the master bootnode overall "overseer"
    /// </summary>
    public int Id { get; private set; } = 0;
    /// <summary>
    /// To support having multiple ledgers run by the same typology, each ledger
    /// is differentiated by a name (which also becomes the directory the ledger
    /// is written)
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// root directory/folder where the ledgers are saved.  
    /// </summary>
    public string RootDataPath { get; private set; }
    public LedgerState State { get; private set; } = LedgerState.Nonfunctional;

    public ILedgerWriter Writer { get; private set; }
    
    public ILedgerReader Reader { get; private set; }
    
    /// <summary>
    ///  
    /// </summary>
    public ILedgerIndexManager Indexes { get; private set; }
    #endregion
    
    private readonly ILogger<Ledger> logger;

    private string LedgerPath => System.IO.Path.Combine(this.RootDataPath, this.Name);

    private string LedgerIndexFileName => System.IO.Path.Combine(LedgerPath, "index.txt");

    private IPhysicalBlockValidator blockValidator;
    private ILedgerPhysicalBlockFactory blockFactory;
    private ILedgerSignBlockFactory signBlockFactory;
    
    public Ledger(ILogger<Ledger> logger, IPhysicalBlockValidator blockValidator,
        ILedgerReader reader, ILedgerWriter writer, ILedgerIndexFactory indexFactory, 
        ILedgerPhysicalBlockFactory blockFactory, ILedgerSignBlockFactory signBlockFactory,
        int id, string name)
    {
        this.logger = logger;
        this.blockValidator = blockValidator;
        this.blockFactory = blockFactory;
        this.signBlockFactory = signBlockFactory;
        Id = id;
        Name = name;
        Reader = reader;
        Writer = writer;
        Indexes = new LedgerIndexManager(Name, Reader, Writer, indexFactory);
    }

    #region Public methods, ILedger methods
    public void InitializeStorage()
    {
        Writer.InitializeStorage();
    }

    public void Check()
    {
        Writer.CheckStorage();
    }

    /// <summary>
    /// Bootnode is starting up for the first time so it needs to create the initial block
    /// and create the index data
    /// </summary>
    public void Initialize()
    {
        InitializeStorage();

        Indexes.Initialize();

        string message = "Initializing boot ledger";
        ILedgerPhysicalBlock block = blockFactory.Create(
            Indexes.GetNextBlockId(),
            BlockStatus.System,
            HashUtility.ComputeHash(message),
            -1,
            Id,
            System.Text.Encoding.ASCII.GetBytes(message),
            signBlockFactory.Create()
        );
        
        AddBlockInternal(block);
    }

    /// <summary>
    /// Theres 2 levels of validation
    /// BootNode needs to run a more thorough validation
    ///
    /// For now child nodes do not
    /// </summary>
    /// <exception cref="LedgerNotFoundException"></exception>
    /// <exception cref="LedgerNotValidException"></exception>
    /// <exception cref="LedgerException"></exception>
    public void Validate()
    {
        State = LedgerState.StartingUp;
        
        try
        {
            Indexes.Load();
            int totalBlocks = Reader.CountBlocks();

            // Validation Rule #1:  the number of records in the index file should match
            // the number of blocks saved on disk
            if (totalBlocks != Indexes.Count())
                throw new LedgerNotValidException($"{Reader.CountBlocks()} != {Indexes.Count()}");
            
            // Validation Rule #2:  the boot record (which is the very first record) 
            // should be valid
            ILedgerPhysicalBlock rootBlock = ReadBlock(1);

            ILedgerIndex rootIndex = Indexes.GetIndex(1);

            if (rootBlock.Hash != rootIndex.Hash)
                throw new LedgerNotValidException($"block {rootBlock.Id}");
            
            if (rootBlock.Status != BlockStatus.System)
                throw new LedgerNotValidException($"block {rootBlock.Id}");

            // if there are blocks other than initial starting block, do some validation on those
            if (1 < totalBlocks)
            {
                // Validation Rule #3: the last record should validate
                ILedgerPhysicalBlock lastBlock = ReadBlock(Indexes.GetNextBlockId() - 1);
                ValidateBlock(lastBlock);
                
                // Validation Rule #4: verify a few other blocks
                // TODO: validate some of the remaining blocks.
                // TODO: have an option that requires all blocks to be validated

            }
            
            State = LedgerState.Available;
        }
        catch(LedgerNotFoundException lnfe)
        {
            logger.LogError($"Ledger Not Found....{lnfe.Message}");
            throw;
        }
        catch(LedgerNotValidException lnve)
        {
            logger.LogError($"Ledger validation Error.  Ledger not functional. {lnve.Message}");
            State = LedgerState.Nonfunctional;
            throw;
        }
        catch(Exception e)
        {
            logger.LogError($"Unexplained Ledger Validation Error.  You may not have a valid ledger. {e.GetType().FullName}: {e.Message}");
            State = LedgerState.Nonfunctional;
            throw new LedgerException(Name, e);
        }
    }

    /// <summary>
    /// TODO this method is not used publicly
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    /// <exception cref="LedgerException"></exception>
    public ILedgerPhysicalBlock AddBlock(ILedgerPhysicalBlock block)
    {
        if (LedgerState.Available != State)
            throw new LedgerException(Name, $"Ledger state does not allow adding blocks");
        
        return AddBlockInternal(block);
    }

    public ILedgerPhysicalBlock AddBlock(byte[] data, BlockStatus status = BlockStatus.Unconfirmed)
    {
        int blockId = Indexes.GetNextBlockId();
        ILedgerIndex parent = Indexes.GetIndex(blockId - 1);
        
        ILedgerPhysicalBlock block = blockFactory.Create(
            blockId, status, parent.Hash, parent.BlockId, Id, data, signBlockFactory.Create());

        return AddBlock(block);
    }

    public ILedgerPhysicalBlock AdvanceBlock(ILedgerPhysicalBlock pb, BlockStatus status)
    {
        ValidateBlock(pb);
        
        int blockId = Indexes.GetNextBlockId();
        ILedgerIndex parent = Indexes.GetIndex(blockId - 1);
        ILedgerPhysicalBlock block = blockFactory.Create(
            blockId, status, parent.Hash, parent.BlockId, pb.Id, pb.Hash, Id, pb.TransactionData, signBlockFactory.Create());
        
        return AddBlock(block);
    }

    public ILedgerPhysicalBlock ReadBlock(int blockId)
    {
        ILedgerPhysicalBlock readBlock = Reader.GetLedgerPhysicalBlock(blockId, (data) => {
            return blockFactory.Create(data, signBlockFactory);
        });
        
        return readBlock;
    }

    public ILedgerPhysicalBlock SyncBlock(ILedgerPhysicalBlock block)
    {
        // if a block entry exists in the index, it means this is block already
        // in the system before child node started up, otherwise it was added during
        // run time.
        ILedgerIndex li = Indexes.GetIndex(block.Id);
        if (null == li)
            li = Indexes.Add(block.Hash, block.TimeStamp, block.Status);

        if (li.BlockId != block.Id)
            throw new LedgerException(Name, $"Index {block.Id}");
        
        if (li.Hash != block.Hash)
            throw new LedgerException(Name, $"Hash mismatch {block.Id}");
        
        Writer.SaveBlock(block);
        Indexes.Save();
        
        return block;
    }

    public void ValidateBlock(ILedgerPhysicalBlock block)
    {
        blockValidator.Validate(Indexes, block);
    }
    #endregion

    private ILedgerPhysicalBlock AddBlockInternal(ILedgerPhysicalBlock block)
    {
        ILedgerIndex index = Indexes.Add(block.Hash, block.TimeStamp, block.Status);

        if (block.Id != index.BlockId)
            throw new LedgerException(Name, $"ID mismatch {block.Id}/{index.BlockId} on AddBlock");

        // TODO: these next two steps are technically a transaction, need to determine what happens when
        // one step fails.
        logger.LogDebug($"saving indexes");
        // step 1 save the block
        // TODO: need rollback if Indexes.Save() fails
        Writer.SaveBlock(block);
        // step 2 save the index
        Indexes.Save();

        return block;
    }

}
