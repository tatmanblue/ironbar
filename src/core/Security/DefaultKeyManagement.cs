using System.Globalization;
using System.Numerics;


namespace core.Security
{
    /// <summary>
    /// from 
    /// https://blog.todotnet.com/2018/02/public-private-keys-and-signing/
    /// </summary>
    public class DefaultKeyManagement : IKeyManagement
    {
        public string GetPublicKeyFromPrivateKey(string privateKey)
        {

            BigInteger p = BigInteger.Parse("0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F", NumberStyles.HexNumber);
            BigInteger b = (BigInteger)7;
            BigInteger a = BigInteger.Zero;
            BigInteger Gx = BigInteger.Parse("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798", NumberStyles.HexNumber);
            BigInteger Gy = BigInteger.Parse("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8", NumberStyles.HexNumber);

            CryptoCurve curve256 = new CryptoCurve(p, a, b);
            CryptoPoint generator256 = new CryptoPoint(curve256, Gx, Gy);

            BigInteger secret = BigInteger.Parse(privateKey, NumberStyles.HexNumber);

            CryptoPoint pubkeyPoint = generator256 * secret;

            return pubkeyPoint.X.ToString("X") + pubkeyPoint.Y.ToString("X");
        }
    }
}
