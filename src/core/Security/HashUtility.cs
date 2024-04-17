using System.Security.Cryptography;
using System.Text;
using core.Utility;

namespace core.Security
{
    public static class HashUtility
    {
        public static string ComputeHash(string source)
        {
            //
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=netframework-4.8
            //
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                return data.ToHexString();
            }
        }
    }
}