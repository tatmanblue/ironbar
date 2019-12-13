using System;

namespace node.Ledger
{
    public enum LedgerState
    {
        Nonfunctional,
        StartingUp,
        Available,
        ShuttingDown,
    }

    /// <summary>
    /// A ledger, which consistents of blockchain entries
    /// There is at least 1 ledger, most likely multiple.  The
    /// ledger with ID=1 is the master ledger.
    /// </summary>
    public class Ledger
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
    }
}
