using AuthPracticeLibrary;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace DatabaseSeeder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //CreatePerson();

            //HashPlaintextUserPasswords();

            CompareHashing();

        }
        private static void CompareHashing()
        {
            IHashAndSalter hashAndSalter = new PBKDF2_HashAndSalter();
            string password = "Yeet!";
            var hashSalt1 = hashAndSalter.HashAndSalt(password);

            string hash1 = hashSalt1.PasswordHashString;
            string salt1 = hashSalt1.SaltString;
            (bool isSamePassword1, _) = hashAndSalter.PasswordEqualsHash("Yeet!", hash1, salt1);

            Console.WriteLine(isSamePassword1.ToString());
        }

        private static void HashPlaintextUserPasswords()
        {
            IHashAndSalter hashAndSalter = new PBKDF2_HashAndSalter();
            MongoCRUD db = new MongoCRUD("AuthPracticeDB");

            var people = db.LoadRecords<PersonModel>("Users");

            foreach (var person in people)
            {
                string[] passwordHashSplit = person.PasswordHash.Split('.');
                if (passwordHashSplit.Length == 3)
                {
                    continue; // if there is the required "iterations.salt.passwordHash", we don't need to hash the password
                }
                person.PasswordHash = hashAndSalter.HashAndSalt(person.PasswordHash).ToDbString();
                db.UpsertRecord("Users", person.Id, person);
            }
        }

        private static void CreatePerson()
        {
            MongoCRUD db = new MongoCRUD("AuthPracticeDB");
            PersonModel dude = new PersonModel()
            {
                FirstName = "Steve",
                LastName = "Minecraft",
                DateOfBirth = new DateTime(2000, 6, 12, 0, 0, 0, DateTimeKind.Utc),
                PasswordHash = "Minecraft2",
                PersonRole = "Gamer"
            };
            db.InsertRecord("Users", dude);
        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Source for PasswordHasher and HashingOptions code: https://medium.com/dealeron-dev/storing-passwords-in-net-core-3de29a3da4d2
        public interface IPasswordHasher
        {
            string Hash(string password);

            (bool Verified, bool NeedsUpgrade) Check(string hash, string password);
        }
        public sealed class PasswordHasher : IPasswordHasher
        {
            private const int SaltSize = 16; // 128 bit 
            private const int KeySize = 32; // 256 bit

            public PasswordHasher(IOptions<HashingOptions> options)
            {
                Options = options.Value;
            }

            private HashingOptions Options { get; }

            public string Hash(string password)
            {
                using (var algorithm = new Rfc2898DeriveBytes(
                  password,
                  SaltSize,
                  Options.Iterations,
                  HashAlgorithmName.SHA512))
                {
                    var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                    var salt = Convert.ToBase64String(algorithm.Salt);

                    return $"{Options.Iterations}.{salt}.{key}";
                }
            }

            public (bool Verified, bool NeedsUpgrade) Check(string hash, string password)
            {
                var parts = hash.Split('.', 3);

                if (parts.Length != 3)
                {
                    throw new FormatException("Unexpected hash format. " +
                      "Should be formatted as `{iterations}.{salt}.{hash}`");
                }

                var iterations = Convert.ToInt32(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var key = Convert.FromBase64String(parts[2]);

                var needsUpgrade = iterations != Options.Iterations;

                using (var algorithm = new Rfc2898DeriveBytes(
                  password,
                  salt,
                  iterations,
                  HashAlgorithmName.SHA512))
                {
                    var keyToCheck = algorithm.GetBytes(KeySize);

                    var verified = keyToCheck.SequenceEqual(key);

                    return (verified, needsUpgrade);
                }
            }
        }
        public sealed class HashingOptions
        {
            public int Iterations { get; set; } = 10000;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
