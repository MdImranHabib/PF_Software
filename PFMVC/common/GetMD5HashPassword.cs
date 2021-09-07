using System.Security.Cryptography;
using System.Text;

namespace PFMVC.common
{
    public static class GetMD5HashPassword
    {
        public static string GetMd5HashOld(string value)
        {
            var md5Hasher = MD5.Create();
            var AesHasher = Aes.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));

            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }



        public static string GetMd5Hash(string value)
        {
            return PasswordHashValue.EncryptString(value);
        }



        public static string GetDecrypedValue(string value)
        {
            return PasswordHashValue.DecryptString(value);
        }
    }
}