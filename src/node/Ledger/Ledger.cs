using System;
using System.IO;
using Microsoft.Extensions.Logging;
using core.Ledger;
using core.Security;
using core.Utility;
using Node.Interfaces;

namespace Node.Ledger;

/// <summary>
/// A ledger, which consistents of blockchain entries
/// There is at least 1 ledger, most likely multiple.  The
/// ledger with ID=1 is the master ledger.
/// 
/// A ledger consists of an index, which is the list of all
/// blocks in the ledger, and blocks, which is the data of the ledger.
///
/// A block has two parts.  The block itself and a consenus entry which
/// confirms all nodes running at the time agreed to the block.  The
/// block and the consens entry are written to separate files.
/// </summary>
public class Ledger : ILedger
{
    private readonly ILogger<Ledger> logger;
    
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
    
    private string LedgerPath => System.IO.Path.Combine(this.RootDataPath, this.Name);

    private string LedgerIndexFileName => System.IO.Path.Combine(LedgerPath, "index.txt");

    private IPhysicalBlockValidator blockValidator;

    public Ledger(ILogger<Ledger> logger, IPhysicalBlockValidator blockValidator, int id, string name, string path)
    {
        this.logger = logger;
        Id = id;
        Name = name;
        RootDataPath = path;
        this.blockValidator = blockValidator;
        Writer = new DefaultTextFileLedgerWriter(LedgerPath);
        Reader =  new DefaultTextFileLedgerReader(LedgerPath);
        Indexes = new LedgerIndexManager(Name, LedgerIndexFileName);
    }

    #region Public methods, ILedger methods
    public void InitializeStorage()
    {
        if (false == Directory.Exists(LedgerPath))
            Directory.CreateDirectory(LedgerPath);
    }
    
    /// <summary>
    /// Bootnode is starting up for the first time so it needs to create the initial block
    /// </summary>
    public void Initialize()
    {
        InitializeStorage();

        Indexes.Initialize();
        PhysicalBlock block = new PhysicalBlock(BlockStatus.System)
        {
            Id = Indexes.GetNextBlockId(),
            LedgerId = Id,
            ParentHash = HashUtility.ComputeHash(Nonce.New().ToString()),
            TransactionData = System.Text.Encoding.ASCII.GetBytes("boot ledger initialized"),
            SignBlock = new SignBlock()
        };
        
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

        if (false == File.Exists(LedgerIndexFileName))
            throw new LedgerNotFoundException(LedgerIndexFileName);

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
        catch(Exception e)
        {
            logger.LogError($"{e.Message}");
            State = LedgerState.Nonfunctional;
            throw new LedgerException(Name, e);
        }
    }

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
        
        PhysicalBlock block = new PhysicalBlock(status)
        {
            Id = blockId,
            ParentHash = parent.Hash,
            ParentId = parent.BlockId,
            LedgerId = Id,
            TransactionData = data,
            SignBlock = new SignBlock()
        };
        return AddBlock(block);
    }

    public ILedgerPhysicalBlock ReadBlock(int blockId)
    {
        // TODO replace with a Block Factory function like we did with indexes
        PhysicalBlock readBlock = Reader.GetLedgerPhysicalBlock(blockId, (data) => {
            return PhysicalBlock.FromString(data);
        }) as PhysicalBlock;
        
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
