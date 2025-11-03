using System.Security.Cryptography;
using System.Text;

namespace HackerInator2000;

public class DesCipher : ICipher
{
    public string Name => "DES";
    public bool SupportsBreak => false; 

    public string Encrypt(string input, string key)
    {
        byte[] keyBytes = PrepareKey(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using var des = DES.Create();
        des.Key = keyBytes;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        using var encryptor = des.CreateEncryptor();
        byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string input, string key)
    {
        byte[] keyBytes = PrepareKey(key);
        byte[] inputBytes = Convert.FromBase64String(input);

        using var des = DES.Create();
        des.Key = keyBytes;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        using var decryptor = des.CreateDecryptor();
        byte[] decrypted = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        return Encoding.UTF8.GetString(decrypted);
    }

    public List<string> Break(string input)
    {
        return new List<string> { "Взлом DES невозможен в этом приложении." };
    }

    private byte[] PrepareKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым.");

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length < 8)
        {
            Array.Resize(ref keyBytes, 8);
        }
        else if (keyBytes.Length > 8)
        {
            keyBytes = keyBytes[..8];
        }
        return keyBytes;
    }
}