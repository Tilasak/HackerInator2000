using System.Text;

namespace HackerInator2000;

public class HashCipher : ICipher
{
    public string Name => "Хеш (SHA-1 учебный)";
    public bool SupportsBreak => false;

    public string Encrypt(string input, string key)
    {
        return ComputeHash(input);
    }

    public string Decrypt(string input, string key)
    {
        throw new NotSupportedException("Хеширование необратимо.");
    }

    public List<string> Break(string input)
    {
        return new List<string> { "Хеш нельзя взломать, только подобрать." };
    }

    private string ComputeHash(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        uint[] hash = SHA1(bytes);
        return ToHexString(hash);
    }

    private uint[] SHA1(byte[] input)
    {
        uint h0 = 0x67452301;
        uint h1 = 0xEFCDAB89;
        uint h2 = 0x98BADCFE;
        uint h3 = 0x10325476;
        uint h4 = 0xC3D2E1F0;

        int len = input.Length;
        int bitLen = len * 8;

        int newLen = ((len + 9) / 64 + 1) * 64;
        byte[] padded = new byte[newLen];
        Array.Copy(input, padded, len);
        padded[len] = 0x80;

        for (int i = 0; i < 8; i++)
        {
            padded[newLen - 8 + i] = (byte)((bitLen >> (56 - i * 8)) & 0xFF);
        }

        for (int i = 0; i < padded.Length; i += 64)
        {
            uint[] w = new uint[80];

            for (int j = 0; j < 16; j++)
            {
                w[j] = (uint)(padded[i + j * 4 + 0] << 24 |
                               padded[i + j * 4 + 1] << 16 |
                               padded[i + j * 4 + 2] << 8 |
                               padded[i + j * 4 + 3]);
            }

            for (int j = 16; j < 80; j++)
            {
                w[j] = LeftRotate(w[j - 3] ^ w[j - 8] ^ w[j - 14] ^ w[j - 16], 1);
            }

            uint a = h0, b = h1, c = h2, d = h3, e = h4;

            for (int j = 0; j < 80; j++)
            {
                uint f, k;
                if (j < 20)
                {
                    f = (b & c) | ((~b) & d);
                    k = 0x5A827999;
                }
                else if (j < 40)
                {
                    f = b ^ c ^ d;
                    k = 0x6ED9EBA1;
                }
                else if (j < 60)
                {
                    f = (b & c) | (b & d) | (c & d);
                    k = 0x8F1BBCDC;
                }
                else
                {
                    f = b ^ c ^ d;
                    k = 0xCA62C1D6;
                }

                uint temp = LeftRotate(a, 5) + f + e + k + w[j];
                e = d;
                d = c;
                c = LeftRotate(b, 30);
                b = a;
                a = temp;
            }

            h0 += a; h1 += b; h2 += c; h3 += d; h4 += e;
        }

        return new uint[] { h0, h1, h2, h3, h4 };
    }

    private uint LeftRotate(uint x, int n)
    {
        return (x << n) | (x >> (32 - n));
    }

    private string ToHexString(uint[] hash)
    {
        StringBuilder sb = new();
        foreach (uint h in hash)
        {
            sb.Append(h.ToString("x8"));
        }
        return sb.ToString().ToUpperInvariant();
    }
}