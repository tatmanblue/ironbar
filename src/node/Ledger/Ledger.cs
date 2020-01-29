using System;
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
        /// <summary>
        /// where the ledger is saved.  This is a subdirectory of Option.DataPath
        /// </summary>
        public string Path { get; private set; }
        public LedgerState State { get; private set; } = LedgerState.Nonfunctional;

        public Ledger(int id, string path)
        {
            Id = id;
            Path = path;
        }

        public void Validate()
        {
            State = LedgerState.StartingUp;

            try
            {
                State = LedgerState.Available;
            }
            catch(Exception e)
            {
                State = LedgerState.Nonfunctional;
                throw new LedgerException(Id.ToString(), e);
            }
            
        }
    }
}
