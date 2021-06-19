using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AuthPracticeLibrary
{
    /// <summary>
    /// The iterations, salt, and resulting hash must be stored in the database separated by a period in the form "iterations.salt.passwordHash"
    /// For example: "10000.Dvsge98yPQCkqSyxgkKZZA==.DMxNfQFQxDgwX3tSIgNijOr7+uFkZqVlmdZJHBMejdM="
    /// </summary>
    public static class HashAndSalter
    {
        /// <summary>
        /// Takes a password and returns the hash (string) and the salt (byte[]).
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static (string hashedPassword, byte[] salt) HashAndSalt(string password)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // derive a 256-bit subkey (use HMACSHA512 with a certain number of iterations)
            // note: I swithced to HMACSHA512 from the docs code because a bigger number seems better
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: Iterations,
                numBytesRequested: 256 / 8));

            return (hashed, salt);
        }

        /// <summary>
        /// Hashes password using salt and the specified number of iterations and then compares it to hash.
        /// 
        /// Returns not only if the hashed password equals the hash, but also if the specified iterations needs to be upgraded to HashAndSalter.Iterations
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hash"></param>
        /// <param name="salt"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, string hash, string salt, int iterations = -1)
        {
            if (iterations <= 0)
            {
                iterations = Iterations;
            }
            byte[] byteSalt = Convert.FromBase64String(salt);

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: byteSalt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: iterations,
                numBytesRequested: 256 / 8));

            bool passwordEqualsHash = (hashedPassword == hash);
            bool needsUpgrade = (iterations != Iterations);
            return (passwordEqualsHash, iterationsNeedsUpgrade: needsUpgrade);
        }
        public static (bool, bool iterationsNeedsUpgrade) PasswordEqualsHashNormal(string password, string hash, string salt, int iterations = -1)
        {
            if (iterations <= 0)
            {
                iterations = Iterations;
            }
            byte[] byteSalt = Convert.FromBase64String(salt);

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: byteSalt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: iterations,
                numBytesRequested: 32));

            bool passwordEqualsHash = (hashedPassword == hash);
            bool needsUpgrade = (iterations != Iterations);
            return (passwordEqualsHash, iterationsNeedsUpgrade: needsUpgrade);
        }

        /// <summary>
        /// Returns the parameters combined in a string ready for being put in a database.
        /// </summary>
        /// <param name="passwordHash"></param>
        /// <param name="salt"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static string Combine(string passwordHash, byte[] salt, int iterations = -1)
        {
            if (iterations <= 0)
            {
                iterations = Iterations;
            }
            return $"{iterations}.{Convert.ToBase64String(salt)}.{passwordHash}";
        }

        public static int Iterations { get; set; } = 10000;
    }
}
