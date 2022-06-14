using AuthPracticeLibrary;

IHashAndSalter hasher = new Argon2id_HashAndSalter();

string p1 = "Password123";
string p2 = "Password123";
string p3 = "password123";
var hash1 = hasher.HashAndSalt(p1);
Console.WriteLine($"hash 1:  {hash1.ToDbString()}");
var hash2 = hasher.HashAndSalt(p2);
Console.WriteLine($"hash 2:  {hash2.ToDbString()}");
var hash3 = hasher.HashAndSalt(p3);
Console.WriteLine($"hash 3:  {hash3.ToDbString()}");

(bool isSame, _) = hasher.PasswordEqualsHash(p2, hash1);
Console.WriteLine($"p1 == p2: {isSame}");
(isSame, _) = hasher.PasswordEqualsHash(p3, hash1);
Console.WriteLine($"p1 == p3: {isSame}");