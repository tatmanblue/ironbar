using System;
using core.Ledger;
using core.Utility;

namespace node.Ledger
{
    public class PhysicalBlock : ILedgerPhysicalBlock
    {
        protected internal PhysicalBlock()
        {
        }

        public int ParentId { get; private set; } = -1;

        public string ParentHash { get; private set; }

        public int LedgerId { get; internal set; }

        public DateTime TimeStamp { get; private set; } = DateTime.Now;

        public Nonce Nonce { get; private set; } = Nonce.New();

        public string Hash { get; private set; }

        public byte[] TransactionData { get; internal set; }

        public BlockStatus Status { get; private set; }

        public ILedgerSignBlock SignBlock { get; internal set; }

        public string ComputeHash()
        {
            throw new NotImplementedException();
        }
    }
}
