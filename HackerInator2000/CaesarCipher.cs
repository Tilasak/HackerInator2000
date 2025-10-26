using System.Text;

namespace HackerInator2000;
public class CaesarCipher : ICipher
{
    public string Name => "Цезарь (кириллица)";
    public bool SupportsBreak => true; 

    private const string CyrillicAlphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

    public string Encrypt(string input, string key)
    {
        if (!int.TryParse(key, out int k) || k <= 0)
            throw new ArgumentException("Ключ должен быть положительным числом.");
        return Transform(input, k);
    }

    public string Decrypt(string input, string key)
    {
        if (!int.TryParse(key, out int k) || k <= 0)
            throw new ArgumentException("Ключ должен быть положительным числом.");
        return Transform(input, -k);
    }

    public List<string> Break(string input)
    {
        var results = new List<string>();
        for (int i = 1; i < CyrillicAlphabet.Length; i++)
        {
            results.Add($"{i}: {Decrypt(input, i.ToString())}");
        }
        return results;
    }

    private string Transform(string input, int shift)
    {
        var result = new StringBuilder();
        foreach (char c in input)
        {
            bool isUpper = char.IsUpper(c);
            char lowerC = char.ToLower(c);
            int index = CyrillicAlphabet.IndexOf(lowerC);
            if (index != -1)
            {
                int newIndex = (index + shift + CyrillicAlphabet.Length) % CyrillicAlphabet.Length;
                char newChar = CyrillicAlphabet[newIndex];
                result.Append(isUpper ? char.ToUpper(newChar) : newChar);
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
}