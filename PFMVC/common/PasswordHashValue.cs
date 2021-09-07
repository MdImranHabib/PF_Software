using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PFMVC.common
{
    public static class PasswordHashValue
    {
        //private const int PasswordSaltByteSize = 128 / 8;
        //private const int AesBlockByteSize = 128 / 8;
        //private const int PasswordByteSize = 256 / 8;
        //private const int PasswordIterationCount = 100;

        //private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        //private static byte[] EncryptString(string toEncrypt)
        //{
        //    string password = "SSL_PF_BC";
        //    using (var aes = Aes.Create())
        //    {
        //        var keySalt = GenerateRandomBytes(PasswordSaltByteSize);
        //        var key = GetKey(password, keySalt);
        //        var iv = GenerateRandomBytes(AesBlockByteSize);

        //        using (var encryptor = aes.CreateEncryptor(key, iv))
        //        {
        //            var plainText = Encoding.UTF8.GetBytes(toEncrypt);
        //            var cipherText = encryptor
        //                .TransformFinalBlock(plainText, 0, plainText.Length);

        //            var result = MergeArrays(keySalt, iv, cipherText);
        //            return result;
        //        }
        //    }
        //}

        //private static byte[] GetKey(string password, byte[] passwordSalt)
        //{
        //    var keyBytes = Encoding.UTF8.GetBytes(password);

        //    using (var derivator = new Rfc2898DeriveBytes(
        //        keyBytes, passwordSalt,
        //        PasswordIterationCount))
        //    {
        //        return derivator.GetBytes(PasswordByteSize);
        //    }
        //}

        //private static byte[] GenerateRandomBytes(int numberOfBytes)
        //{
        //    var randomBytes = new byte[numberOfBytes];
        //    Random.GetBytes(randomBytes);
        //    return randomBytes;
        //}

        //private static byte[] MergeArrays(params byte[][] arrays)
        //{
        //    int arrLenthSum = 0;
        //    foreach (var item in arrays)
        //    {
        //        arrLenthSum += item.Length;
        //    }
        //    var merged = new byte[arrLenthSum];
        //    var mergeIndex = 0;
        //    for (int i = 0; i < arrays.GetLength(0); i++)
        //    {
        //        arrays[i].CopyTo(merged, mergeIndex);
        //        mergeIndex += arrays[i].Length;
        //    }

        //    return merged;
        //}

        //public static string GetMd5Hash(string value)
        //{
        //    byte[] data = EncryptString(value);

        //    var sBuilder = new StringBuilder();
        //    for (var i = 0; i < data.Length; i++)
        //    {
        //        sBuilder.Append(data[i].ToString("x2"));
        //    }
        //    return sBuilder.ToString();
        //}


        //public static string DecryptToString(byte[] encryptedData)
        //{
        //    string password = "SSL_PF_BC";
        //    using (var aes = Aes.Create())
        //    {
        //        var keySalt = encryptedData.Take(PasswordSaltByteSize).ToArray();
        //        var key = GetKey(password, keySalt);
        //        var iv = encryptedData
        //            .Skip(PasswordSaltByteSize).Take(AesBlockByteSize).ToArray();
        //        var cipherText = encryptedData
        //            .Skip(PasswordSaltByteSize + AesBlockByteSize).ToArray();

        //        using (var encryptor = aes.CreateDecryptor(key, iv))
        //        {
        //            var decryptedBytes = encryptor
        //                .TransformFinalBlock(cipherText, 0, cipherText.Length);
        //            return Encoding.UTF8.GetString(decryptedBytes);
        //        }
        //    }
        //}



        public static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}