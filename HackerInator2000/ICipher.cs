namespace HackerInator2000;

public interface ICipher
{
    string Name { get; }
    bool SupportsBreak { get; } 
    string Encrypt(string input, string key); 
    string Decrypt(string input, string key);
    List<string> Break(string input);
}