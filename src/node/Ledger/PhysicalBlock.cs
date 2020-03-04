using System;
using System.Security.Cryptography;
using System.Text;
using core.Ledger;
using core.Utility;

namespace node.Ledger
{
    public class PhysicalBlock : ILedgerPhysicalBlock
    {
        protected internal PhysicalBlock()
        {
        }

        public int Id { get; internal set; } = -1;

        public int ParentId { get; private set; } = -1;
        public int ReferenceId { get; } = 0;

        public string ParentHash { get; private set; } = "0";

        public int LedgerId { get; internal set; }

        public DateTime TimeStamp { get; private set; } = DateTime.Now;

        public Nonce Nonce { get; private set; } = Nonce.New();

        public string Hash { get; private set; }

        public byte[] TransactionData { get; internal set; }

        public BlockStatus Status { get; private set; }

        public ILedgerSignBlock SignBlock { get; internal set; }

        public string ComputeHash()
        {
            //
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=netframework-4.8
            //
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(this.HashString()));

                this.Hash = data.ToHexString();
                return this.Hash;
            }
        }

        /// <summary>
        /// a string representation of the object sans a hash
        /// </summary>
        /// <returns></returns>
        private string HashString()
        {
            // TODO: what about the sign block
            string transactionDataString = Encoding.UTF8.GetString(TransactionData, 0, TransactionData.Length);
            string objectString = $"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{LedgerId}:{TimeStamp}:{Nonce}:{transactionDataString}";

            return objectString;
        }

        public override string ToString()
        {
            // TODO: what about the sign block
            string transactionDataString = Encoding.UTF8.GetString(TransactionData, 0, TransactionData.Length); 
            string objectString = $"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{LedgerId}:{TimeStamp}:{Nonce}:{transactionDataString}:{Hash}";

            return objectString;
        }
    }
}
