using Konscious.Security.Cryptography;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AuthPracticeLibrary
{
    public class Argon2id_HashAndSalter : IHashAndSalter
    {

        public PasswordHashModel HashAndSalt(string password)
        {
            byte[] salt = new byte[32];
            byte[] hash = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            using (var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2id.Salt = salt;
                argon2id.Iterations = Iterations;
                argon2id.MemorySize = MemorySize; 
                argon2id.DegreeOfParallelism = DegreeOfParallelism;
                
                hash = argon2id.GetBytes(32);
            }
            return new PasswordHashModel() { 
                PasswordHash = hash, 
                Salt = salt, 
                IterationsOnHash = Iterations 
            };
        }

        public (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, byte[] hash, byte[] salt, int iterations = -1)
        {
            if (iterations <= 0)
            {
                iterations = Iterations;
            }

            byte[] hashedPassword;
            using (var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2id.Salt = salt;
                argon2id.Iterations = iterations;
                argon2id.MemorySize = MemorySize;
                argon2id.DegreeOfParallelism = DegreeOfParallelism;

                hashedPassword = argon2id.GetBytes(32);
            }

            bool passwordEqualsHash = (hashedPassword.SequenceEqual(hash));
            bool needsUpgrade = (iterations != Iterations);
            return (passwordEqualsHash, iterationsNeedsUpgrade: needsUpgrade);
        }

        public (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, string hash, string salt, int iterations = -1)
        {
            return PasswordEqualsHash(password,
                Convert.FromBase64String(hash),
                Convert.FromBase64String(salt),
                iterations);
        }
        public (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, PasswordHashModel passwordHashModel)
        {
            return PasswordEqualsHash(password,
                passwordHashModel.PasswordHash,
                passwordHashModel.Salt,
                passwordHashModel.IterationsOnHash);
        }
        public int Iterations { get; set; } = 10;
        /// <summary>
        /// Memory used (in KiB)
        /// </summary>
        public int MemorySize { get; set; } = 65536; // 2^16KiB = 64MiB
        public int DegreeOfParallelism { get; set; } = 8;
    }
}
