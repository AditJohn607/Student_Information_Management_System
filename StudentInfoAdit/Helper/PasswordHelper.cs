using System;
using System.Security.Cryptography;
using System.Text;
namespace StudentInfoAdit.Helpers
{
    public class PasswordHelper
    {
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] combined = Encoding.UTF8.GetBytes(password + salt);

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }
     
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            return HashPassword(password, storedSalt) == storedHash;
        }
    }
}