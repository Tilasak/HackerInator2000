using System.Text;

namespace HackerInator2000;

public class StreamCipher : ICipher
{
    public string Name => "Поточный";
    public bool SupportsBreak => false;

    public string Encrypt(string input, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым.");

        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] gamma = GenerateGamma(inputBytes.Length, keyBytes);
        byte[] output = new byte[inputBytes.Length];

        for (int i = 0; i < inputBytes.Length; i++)
        {
            output[i] = (byte)(inputBytes[i] ^ gamma[i]);
        }

        return Convert.ToBase64String(output);
    }

    public string Decrypt(string input, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым.");

        byte[] cipherBytes = Convert.FromBase64String(input);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] gamma = GenerateGamma(cipherBytes.Length, keyBytes);
        byte[] output = new byte[cipherBytes.Length];

        for (int i = 0; i < cipherBytes.Length; i++)
        {
            output[i] = (byte)(cipherBytes[i] ^ gamma[i]);
        }

        return Encoding.UTF8.GetString(output);
    }

    public List<string> Break(string input)
    {
        return new List<string> { "Взлом поточного шифра невозможен в этом приложении." };
    }

    private byte[] GenerateGamma(int length, byte[] key)
    {
        if (length <= 0) return Array.Empty<byte>();
        if (key.Length == 0) throw new ArgumentException("Ключ не может быть пустым.");

        byte[] gamma = new byte[length];
        int keyLen = key.Length;

        for (int i = 0; i < length; i++)
        {
            gamma[i] = (byte)((key[i % keyLen] + i) % 256);
        }

        return gamma;
    }
}