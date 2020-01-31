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
            //
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=netframework-4.8
            //
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(this.ToString()));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var hashBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    hashBuilder.Append(data[i].ToString("x2"));
                }

                this.Hash = hashBuilder.ToString();
                return this.Hash;
            }
        }

        public override string ToString()
        {
            string transactionDataString = Encoding.UTF8.GetString(TransactionData, 0, TransactionData.Length); 
            string objectString = $"{ParentId}:{Id}:{ParentHash}:{LedgerId}:{TimeStamp}:{Nonce}:{transactionDataString}";

            return objectString;
        }
    }
}
