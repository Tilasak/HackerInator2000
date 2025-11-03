using System.Text;

namespace HackerInator2000;

public class AdfgvxCipher : ICipher
{
    public string Name => "ADFGVX (немецкий)";
    public bool SupportsBreak => false;

    private static readonly char[,] PolybiusSquare = {
        {'A', 'B', 'C', 'D', 'E', 'F'},
        {'G', 'H', 'I', 'J', 'K', 'L'},
        {'M', 'N', 'O', 'P', 'Q', 'R'},
        {'S', 'T', 'U', 'V', 'W', 'X'},
        {'Y', 'Z', '0', '1', '2', '3'},
        {'4', '5', '6', '7', '8', '9'}
    };

    private static readonly char[] Adfgvx = { 'A', 'D', 'F', 'G', 'V', 'X' };

    public string Encrypt(string input, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым.");

        string cleanInput = CleanInput(input.ToUpperInvariant());
        if (string.IsNullOrEmpty(cleanInput))
            throw new ArgumentException("Текст должен содержать латинские буквы или цифры.");

        string keyUpper = key.ToUpperInvariant();
        int keyLen = keyUpper.Length;

        StringBuilder pairs = new();
        foreach (char c in cleanInput)
        {
            var (row, col) = FindInSquare(c);
            pairs.Append(Adfgvx[row]);
            pairs.Append(Adfgvx[col]);
        }

        var columns = new List<StringBuilder>();
        for (int i = 0; i < keyLen; i++)
            columns.Add(new StringBuilder());

        for (int i = 0; i < pairs.Length; i++)
        {
            int col = i % keyLen;
            columns[col].Append(pairs[i]);
        }

        var indexedKey = keyUpper.Select((ch, idx) => new { Char = ch, Index = idx }).ToList();
        var sortedColumns = indexedKey.OrderBy(x => x.Char).ThenBy(x => x.Index)
                                      .Select(x => columns[x.Index]).ToList();

        StringBuilder result = new();
        foreach (var col in sortedColumns)
        {
            result.Append(col);
        }

        return result.ToString();
    }

    public string Decrypt(string input, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым.");

        string cipherText = new string(input.Where(c => Adfgvx.Contains(c)).ToArray());
        if (cipherText.Length % 2 != 0)
            throw new ArgumentException("Некорректный шифртекст: длина должна быть чётной.");

        string keyUpper = key.ToUpperInvariant();
        int keyLen = keyUpper.Length;
        int totalChars = cipherText.Length;

        if (keyLen == 0)
            throw new ArgumentException("Ключ не может быть пустым.");

        int fullRows = totalChars / keyLen;
        int remainder = totalChars % keyLen;

        int[] colLengths = new int[keyLen];
        for (int i = 0; i < keyLen; i++)
        {
            colLengths[i] = fullRows + (i < remainder ? 1 : 0);
        }

        var indexedKey = keyUpper.Select((ch, idx) => new { Char = ch, Index = idx }).ToList();
        var sortedIndices = indexedKey.OrderBy(x => x.Char).ThenBy(x => x.Index)
                                      .Select(x => x.Index).ToList();

        var columns = new StringBuilder[keyLen];
        for (int i = 0; i < keyLen; i++)
            columns[i] = new StringBuilder();

        int currentIndex = 0;
        for (int i = 0; i < keyLen; i++)
        {
            int originalIndex = sortedIndices[i];
            int len = colLengths[originalIndex];
            for (int j = 0; j < len; j++)
            {
                columns[originalIndex].Append(cipherText[currentIndex++]);
            }
        }

        StringBuilder pairs = new();
        int maxLen = colLengths.Max();
        for (int row = 0; row < maxLen; row++)
        {
            for (int col = 0; col < keyLen; col++)
            {
                if (row < columns[col].Length)
                    pairs.Append(columns[col][row]);
            }
        }

        StringBuilder result = new();
        for (int i = 0; i < pairs.Length; i += 2)
        {
            char r = pairs[i];
            char c = pairs[i + 1];
            if (!TryGetCharFromPair(r, c, out char plainChar))
                throw new ArgumentException($"Некорректная пара: {r}{c}");
            result.Append(plainChar);
        }

        return result.ToString();
    }

    public List<string> Break(string input)
    {
        return new List<string> { "Взлом ADFGVX невозможен в этом приложении." };
    }

    private (int row, int col) FindInSquare(char c)
    {
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                if (PolybiusSquare[i, j] == c)
                    return (i, j);
        throw new ArgumentException($"Символ '{c}' не поддерживается.");
    }

    private bool TryGetCharFromPair(char rowChar, char colChar, out char result)
    {
        result = '\0';
        int row = Array.IndexOf(Adfgvx, rowChar);
        int col = Array.IndexOf(Adfgvx, colChar);
        if (row == -1 || col == -1) return false;
        result = PolybiusSquare[row, col];
        return true;
    }

    private string CleanInput(string input)
    {
        return new string(input.Where(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')).ToArray());
    }
}