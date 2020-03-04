using System;
using System.IO;
using core.Ledger;

namespace node.Ledger
{
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
        /// <summary>
        /// Ledger ID.  1 is master ledger.  It is controlled by the nodes
        /// with the master bootnode overall "overseer"
        /// </summary>
        public int Id { get; private set; } = 0;
        public string Name { get; private set; }
        /// <summary>
        /// where the ledger is saved.  This is a subdirectory of Option.DataPath
        /// TODO: WRONG!  this is root path. eval if this is what we really want
        /// </summary>
        public string Path { get; private set; }
        public LedgerState State { get; private set; } = LedgerState.Nonfunctional;

        public ILedgerWriter Writer { get; private set; }
        
        public ILedgerReader Reader { get; private set; }

        private string LedgerPath
        {
            get
            {
                return System.IO.Path.Combine(this.Path, this.Name);
            }
        }

        private string LedgerIndexFileName
        {
            get
            {
                return System.IO.Path.Combine(LedgerPath, "index.txt");
            }
        }

        private LedgerIndexManager Indexes { get; set; }

        public Ledger(int id, string name, string path)
        {
            Id = id;
            Name = name;
            Path = path;
            Writer = new DefaultTextFileLedgerWriter(LedgerPath);
            Reader =  new DefaultTextFileLedgerReader(LedgerPath);
            Indexes = new LedgerIndexManager(Name, LedgerIndexFileName);
        }

        public void Initialize()
        {
            if (false == Directory.Exists(LedgerPath))
                Directory.CreateDirectory(LedgerPath);

            Indexes.Initialize();
            PhysicalBlock block = new PhysicalBlock
            {
                Id = Indexes.GetNextBlockId(),
                LedgerId = Id,
                TransactionData = System.Text.Encoding.ASCII.GetBytes("ledger initialized"),
                SignBlock = new SignBlock()
            };
            block.ComputeHash();

            LedgerIndex index = Indexes.Add(block.Hash, block.TimeStamp);

            if (block.Id != index.BlockId)
                throw new LedgerException(Name, $"ID mismatch {block.Id}/{index.BlockId}");

            // TODO: these next two steps are technically a transaction, need to determine what happens when
            // one step fails.
            System.Console.WriteLine($"saving indexes to {LedgerIndexFileName}");
            // step 1 save the block
            Writer.SaveBlock(block);
            // step 2 save the index
            // TODO: move index save to Writer
            Indexes.Save();
        }

        public void Validate()
        {
            State = LedgerState.StartingUp;

            if (false == File.Exists(LedgerIndexFileName))
                throw new LedgerNotFoundException(LedgerIndexFileName);

            try
            {
                Indexes.Load();

                if (Reader.CountBlocks() != Indexes.Count())
                    throw new LedgerNotValidException($"{Reader.CountBlocks()} != {Indexes.Count()}");

                // TODO:  validate hash of some blocks.   maybe the root block and last blocks or something
                State = LedgerState.Available;
            }
            catch(Exception e)
            {
                State = LedgerState.Nonfunctional;
                throw new LedgerException(Name, e);
            }
            
        }
    }
}
