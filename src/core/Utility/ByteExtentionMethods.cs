using System;
using System.Collections.Generic;
using System.Text;

namespace core.Utility
{
    public static class ByteExtentionMethods
    {
        public static string ToHexString(this byte[] data)
        {
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var hashBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.

            for (int i = 0; i < data.Length; i++)
            {
                hashBuilder.Append(data[i].ToString("x2"));
            }

            return hashBuilder.ToString();
        }
    }
}
