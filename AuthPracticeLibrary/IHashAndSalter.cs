namespace AuthPracticeLibrary
{
    public interface IHashAndSalter
    {
        int Iterations { get; set; }

        PasswordHashModel HashAndSalt(string password);
        (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, byte[] hash, byte[] salt, int iterations = -1);
        (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, PasswordHashModel passwordHashModel);
        (bool, bool iterationsNeedsUpgrade) PasswordEqualsHash(string password, string hash, string salt, int iterations = -1);
    }
}