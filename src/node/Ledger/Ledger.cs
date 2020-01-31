using System;
using System.IO;
using core.Ledger;

namespace node.Ledger
{
    /// <summary>
    /// A ledger, which consistents of blockchain entries
    /// There is at least 1 ledger, most likely multiple.  The
    /// ledger with ID=1 is the master ledger.
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
        /// </summary>
        public string Path { get; private set; }
        public LedgerState State { get; private set; } = LedgerState.Nonfunctional;

        public ILedgerWriter Writer => throw new NotImplementedException();

        public ILedgerReader Reader => throw new NotImplementedException();

        private string LedgerPath
        {
            get
            {
                return System.IO.Path.Combine(this.Path, this.Name);
            }
        }

        private string LedgerFileName
        {
            get
            {
                return System.IO.Path.Combine(LedgerPath, "data.txt");
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

            Indexes = new LedgerIndexManager(Name, LedgerIndexFileName);
        }

        public void Initialize()
        {
            if (false == Directory.Exists(LedgerPath))
                Directory.CreateDirectory(LedgerPath);

            Indexes.Initialize();
            PhysicalBlock block = new PhysicalBlock();
            block.Id = Indexes.GetNextBlockId();
            block.LedgerId = Id;
            block.TransactionData = System.Text.Encoding.ASCII.GetBytes("ledger initialized");
            block.SignBlock = new SignBlock();
            block.ComputeHash();

            LedgerIndex index = Indexes.Add(block.Hash, block.TimeStamp);

            if (block.Id != index.BlockID)
                throw new LedgerException(Name, $"ID mismatch {block.Id}/{index.BlockID}");

            // TODO: these next two steps are technically a transaction, need to determine what happens when
            // one step fails.
            System.Console.WriteLine($"saving indexes to {LedgerIndexFileName}");
            Indexes.Save();
        }

        public void Validate()
        {
            State = LedgerState.StartingUp;

            if (false == File.Exists(LedgerFileName))
                throw new LedgerNotFoundException(Name);

            try
            {
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
