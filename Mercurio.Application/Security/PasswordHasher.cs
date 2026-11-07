using Mercurio.Application.Interfaces;
using System.Security.Cryptography;

namespace Mercurio.Application.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public string CreateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
        }

        public string Hash(string password, string salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es obligatoria.", nameof(password));

            var saltBytes = Convert.FromBase64String(salt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string password, string salt, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(salt) || string.IsNullOrWhiteSpace(hash))
                return false;

            var computed = Hash(password, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computed),
                Convert.FromBase64String(hash));
        }
    }
}
