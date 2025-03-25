using AdminProvider.ModeratorsManagement.Interfaces.Utillities;
using System.Security.Cryptography;
using System.Text;

namespace AdminProvider.ModeratorsManagement.Utillities
{
    public class CustomPasswordHasher : ICustomPasswordHasher<AdminEntity>
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int HashSize = 32; // 256-bit hash
        private const int Iterations = 10000; // PBKDF2 iterations

        public string HashPassword(AdminEntity user, string password)
        {
            // Generate a random salt
            byte[] salt;
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[SaltSize];
                rng.GetBytes(salt);
            }

            // Compute the hash using PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA1))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combine salt and hash into one byte array for storage
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyHashedPassword(AdminEntity user, string hashedPassword, string providedPassword)
        {
            // Convert the stored hash back into salt and hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            byte[] storedHash = new byte[HashSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // Compute the hash of the provided password with the stored salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, Iterations, HashAlgorithmName.SHA1))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);

                // Compare the computed hash with the stored hash
                for (int i = 0; i < HashSize; i++)
                {
                    if (storedHash[i] != computedHash[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
