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

        public int ParentId { get; internal set; } = -1;
        public int ReferenceId { get; internal set; } = 0;

        public string ParentHash { get; internal set; } = "0";

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
            string objectString = $"{ParentId}:{Id}:{ParentHash}:{ReferenceId}:{LedgerId}:{TimeStamp.ToFileDateTime()}:{Nonce}:{transactionDataString}:{Hash}";

            return objectString;
        }

        public static PhysicalBlock FromString(string data)
        {
            string[] elements = data.Split(":");
            DateTime expectedDate;
            if (false == DateTime.TryParseExact(elements[5], "dd MMMM yyyy HHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out expectedDate))
                throw new LedgerNotValidException($"{elements[5]}");

            PhysicalBlock block = new PhysicalBlock()
            {
                ParentId = Convert.ToInt32(elements[0]),
                Id = Convert.ToInt32(elements[1]),
                ParentHash = elements[2],
                ReferenceId = Convert.ToInt32(elements[3]),
                LedgerId = Convert.ToInt32(elements[4]),
                TimeStamp = expectedDate,
                Nonce = Nonce.FromString(elements[6]),
            };

            // TODO: gotta convert transactionData

            // compare hash from string with computedHash. they should match
            if (elements[8] != block.ComputeHash())
                throw new LedgerNotValidException($"{block.Id}");

            return block;
        }
    }
}
