using System;
using System.Security.Cryptography;

namespace core.Utility
{
    public class Nonce
    {
        public int Value { get; private set; } = 0;

        public override string ToString()
        {
            return Value.ToString();
        }

        private Nonce() { }

        private static RNGCryptoServiceProvider generator = new RNGCryptoServiceProvider();

        public static Nonce New()
        {
            byte[] dataSet = new byte[5];
            generator.GetNonZeroBytes(dataSet);

            Nonce nonce = new Nonce
            {
                Value = BitConverter.ToInt32(dataSet, 0)
            };

            return nonce;
        }

        public static Nonce FromString(string data)
        {
            Nonce nonce = new Nonce();
            nonce.Value = Convert.ToInt32(data);
            return nonce;
        }
    }
}
