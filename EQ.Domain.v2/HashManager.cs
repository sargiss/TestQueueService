using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace EQ.Domain.v2
{
    internal static class HashManager
    {
        public static string MD5Hash(string text)
        {
            string hash = string.Empty;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bhash = md5.ComputeHash(Encoding.Default.GetBytes(text));
            if (bhash.Length > 0)
            {
                foreach (byte b in bhash)
                    hash += b.ToString("X2");
            }
            return hash;
        }

        public static string MD5Hash(byte[] data)
        {
            string hash = string.Empty;
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bhash = md5.ComputeHash(data);
            if (bhash.Length > 0)
            {
                foreach (byte b in bhash)
                    hash += b.ToString("X2");
            }
            return hash;
        }
    }
}
