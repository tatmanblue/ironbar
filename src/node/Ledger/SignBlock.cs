using System;
using core.Ledger;
using core.Utility;

namespace node.Ledger
{
    public class SignBlock : ILedgerSignBlock
    {
        protected internal SignBlock()
        {
        }

        public Nonce Nonce { get; private set; } = Nonce.New();

        public DateTime DateStamp { get; private set; } = DateTime.Now;
    }
}
