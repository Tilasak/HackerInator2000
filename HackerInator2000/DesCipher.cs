using System.Text;

namespace HackerInator2000;

public class DesCipher : ICipher
{
    public string Name => "DES";
    public bool SupportsBreak => false;

    private static readonly int[] IP = {
        58, 50, 42, 34, 26, 18, 10, 2,
        60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6,
        64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17, 9, 1,
        59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5,
        63, 55, 47, 39, 31, 23, 15, 7
    };

    private static readonly int[] FP = {
        40, 8, 48, 16, 56, 24, 64, 32,
        39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30,
        37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28,
        35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26,
        33, 1, 41, 9, 49, 17, 57, 25
    };

    private static readonly int[] PC1 = { 
        57, 49, 41, 33, 25, 17, 9,
        1, 58, 50, 42, 34, 26, 18,
        10, 2, 59, 51, 43, 35, 27,
        19, 11, 3, 60, 52, 44, 36,
        63, 55, 47, 39, 31, 23, 15,
        7, 62, 54, 46, 38, 30, 22,
        14, 6, 61, 53, 45, 37, 29,
        21, 13, 5, 28, 20, 12, 4
    };

    private static readonly int[] PC2 = { 
        14, 17, 11, 24, 1, 5,
        3, 28, 15, 6, 21, 10,
        23, 19, 12, 4, 26, 8,
        16, 7, 27, 20, 13, 2,
        41, 52, 31, 37, 47, 55,
        30, 40, 51, 45, 33, 48,
        44, 49, 39, 56, 34, 53,
        46, 42, 50, 36, 29, 32
    };

    private static readonly int[] E = { 
        32, 1, 2, 3, 4, 5,
        4, 5, 6, 7, 8, 9,
        8, 9, 10, 11, 12, 13,
        12, 13, 14, 15, 16, 17,
        16, 17, 18, 19, 20, 21,
        20, 21, 22, 23, 24, 25,
        24, 25, 26, 27, 28, 29,
        28, 29, 30, 31, 32, 1
    };

    private static readonly int[] P = { 
        16, 7, 20, 21,
        29, 12, 28, 17,
        1, 15, 23, 26,
        5, 18, 31, 10,
        2, 8, 24, 14,
        32, 27, 3, 9,
        19, 13, 30, 6,
        22, 11, 4, 25
    };

    private static readonly byte[,] SBox = {
        { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 }
    };

    private static readonly int[] Shifts = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };

    public string Encrypt(string input, string key)
    {
        byte[] keyBytes = PrepareKey(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        int padding = (8 - (inputBytes.Length % 8)) % 8;
        byte[] padded = new byte[inputBytes.Length + padding];
        Array.Copy(inputBytes, padded, inputBytes.Length);
        for (int i = 0; i < padding; i++) padded[inputBytes.Length + i] = (byte)padding;

        List<byte> result = new();
        for (int i = 0; i < padded.Length; i += 8)
        {
            byte[] block = padded.Skip(i).Take(8).ToArray();
            byte[] encryptedBlock = EncryptBlock(block, keyBytes);
            result.AddRange(encryptedBlock);
        }

        return Convert.ToBase64String(result.ToArray());
    }

    public string Decrypt(string input, string key)
    {
        byte[] keyBytes = PrepareKey(key);
        byte[] cipherBytes = Convert.FromBase64String(input);

        List<byte> result = new();
        for (int i = 0; i < cipherBytes.Length; i += 8)
        {
            byte[] block = cipherBytes.Skip(i).Take(8).ToArray();
            byte[] decryptedBlock = DecryptBlock(block, keyBytes);
            result.AddRange(decryptedBlock);
        }

        if (result.Count > 0)
        {
            int pad = result[result.Count - 1];
            if (pad > 0 && pad <= 8)
                result.RemoveRange(result.Count - pad, pad);
        }

        return Encoding.UTF8.GetString(result.ToArray());
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

    private byte[] EncryptBlock(byte[] block, byte[] key)
    {
        return ProcessBlock(block, key, false);
    }

    private byte[] DecryptBlock(byte[] block, byte[] key)
    {
        return ProcessBlock(block, key, true);
    }

    private byte[] ProcessBlock(byte[] block, byte[] key, bool decrypt)
    {
        ulong data = BytesToUInt64(block);
        data = Permute(data, IP);

        ulong left = (data >> 32) & 0xFFFFFFFF;
        ulong right = data & 0xFFFFFFFF;

        ulong[] subkeys = GenerateSubkeys(key);

        int start = decrypt ? 15 : 0;
        int end = decrypt ? -1 : 16;
        int step = decrypt ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            ulong temp = right;
            right = left ^ Feistel(right, subkeys[i]);
            left = temp;
        }

        data = (right << 32) | left;
        data = Permute(data, FP);

        return UInt64ToBytes(data);
    }

    private ulong[] GenerateSubkeys(byte[] key)
    {
        ulong key64 = BytesToUInt64(key);
        ulong c = (key64 >> 28) & 0x0FFFFFFF;
        ulong d = key64 & 0x0FFFFFFF;

        ulong[] keys = new ulong[16];
        for (int i = 0; i < 16; i++)
        {
            c = RotateLeft(c, Shifts[i], 28);
            d = RotateLeft(d, Shifts[i], 28);
            ulong cd = (c << 28) | d;
            keys[i] = Permute(cd, PC2);
        }
        return keys;
    }

    private ulong Feistel(ulong right, ulong subkey)
    {
        ulong expanded = Permute(right, E, 32);

        expanded ^= subkey;

        ulong result = 0;
        for (int i = 0; i < 8; i++)
        {
            ulong bits = (expanded >> (42 - i * 6)) & 0x3F;
            byte row = (byte)(((bits >> 4) & 0x2) | ((bits >> 5) & 0x1));
            byte col = (byte)(bits & 0xF);
            byte val = SBox[0, col];
            result = (result << 4) | val;
        }

        return Permute(result, P, 32);
    }

    private ulong Permute(ulong input, int[] table, int inputSize = 64)
    {
        ulong output = 0;
        for (int i = 0; i < table.Length; i++)
        {
            int bitPos = table[i] - 1;
            if (bitPos < inputSize)
            {
                bool bit = ((input >> (inputSize - 1 - bitPos)) & 1) == 1;
                if (bit) output |= (1UL << (table.Length - 1 - i));
            }
        }
        return output;
    }

    private ulong RotateLeft(ulong value, int steps, int size)
    {
        steps %= size;
        return ((value << steps) | (value >> (size - steps))) & ((1UL << size) - 1);
    }

    private ulong BytesToUInt64(byte[] bytes)
    {
        ulong result = 0;
        for (int i = 0; i < 8 && i < bytes.Length; i++)
        {
            result = (result << 8) | bytes[i];
        }
        return result;
    }

    private byte[] UInt64ToBytes(ulong value)
    {
        byte[] bytes = new byte[8];
        for (int i = 7; i >= 0; i--)
        {
            bytes[i] = (byte)(value & 0xFF);
            value >>= 8;
        }
        return bytes;
    }
}