using System;
using System.Text;

namespace HackerInator2000;

public class ElGamalSignature : ICipher
{
    public string Name => "Эль-Гамаль (подпись)";
    public bool SupportsBreak => false;

    public string Encrypt(string input, string key)
    {
        long[] parts = ParseKey3(key);
        long p = parts[0], g = parts[1], x = parts[2];

        if (p <= 2 || g <= 1 || g >= p || x <= 0 || x >= p - 1)
            throw new ArgumentException("Некорректные параметры: p>2, 1<g<p, 0<x<p-1");

        string hashHex = ComputeHash(input);
        long h = HexToLong(hashHex) % (p - 1);
        if (h <= 0) h = 1;

        long k;
        Random rand = new Random();
        do
        {
            double range = (double)(p - 3);
            if (range <= 0) throw new ArgumentException("p слишком мало");
            k = 2 + (long)(rand.NextDouble() * range);
        } while (Gcd(k, p - 1) != 1);

        long r = ModPow(g, k, p);
        long invK = ModInverse(k, p - 1);
        long xr = (x * r) % (p - 1);
        long diff = (h - xr) % (p - 1);
        if (diff < 0) diff += p - 1;
        long s = (invK * diff) % (p - 1);
        if (s < 0) s += p - 1;

        return r.ToString() + "," + s.ToString();
    }

    public string Decrypt(string signature, string key)
    {
        int comma = signature.IndexOf(',');
        if (comma == -1) throw new ArgumentException("Подпись: r,s");
        long r = ParseLong(signature.Substring(0, comma));
        long s = ParseLong(signature.Substring(comma + 1));

        int i1 = key.IndexOf(',');
        if (i1 == -1) throw new ArgumentException("Ключ: p,g,y,текст");
        int i2 = key.IndexOf(',', i1 + 1);
        if (i2 == -1) throw new ArgumentException("Ключ: p,g,y,текст");
        int i3 = key.IndexOf(',', i2 + 1);
        if (i3 == -1) throw new ArgumentException("Ключ: p,g,y,текст");

        long p = ParseLong(key.Substring(0, i1));
        long g = ParseLong(key.Substring(i1 + 1, i2 - i1 - 1));
        long y = ParseLong(key.Substring(i2 + 1, i3 - i2 - 1));
        string originalText = key.Substring(i3 + 1);

        if (r <= 0 || r >= p || s < 0 || s >= p - 1)
            return "Некорректная подпись";

        string hashHex = ComputeHash(originalText);
        long h = HexToLong(hashHex) % (p - 1);
        if (h <= 0) h = 1;

        long left = ModPow(g, h, p);
        long right = (ModPow(y, r, p) * ModPow(r, s, p)) % p;
        if (right < 0) right += p;

        return left == right ? "Подпись ВЕРНА" : "Подпись НЕВЕРНА";
    }

    public List<string> Break(string input)
    {
        var result = new List<string>();
        result.Add("Атака — решение дискретного логарифма.");
        return result;
    }

    private long[] ParseKey3(string s)
    {
        long[] res = new long[3];
        int start = 0;
        int count = 0;
        for (int i = 0; i <= s.Length; i++)
        {
            if (i == s.Length || s[i] == ',')
            {
                if (count >= 3) throw new ArgumentException("Слишком много чисел");
                string part = s.Substring(start, i - start).Trim();
                res[count] = ParseLong(part);
                count++;
                start = i + 1;
            }
        }
        if (count != 3) throw new ArgumentException("Нужно ровно 3 числа");
        return res;
    }
    private long ParseLong(string s)
    {
        s = s.Trim();
        if (string.IsNullOrEmpty(s)) throw new ArgumentException("Пустое число");

        bool neg = s[0] == '-';
        int start = neg ? 1 : 0;
        if (start >= s.Length) throw new ArgumentException("Некорректное число");

        long val = 0;
        for (int i = start; i < s.Length; i++)
        {
            char c = s[i];
            if (c < '0' || c > '9')
                throw new ArgumentException($"Недопустимый символ '{c}' в числе");
            val = checked(val * 10 + (c - '0')); // 'checked' helps catch overflow in debug
        }
        return neg ? -val : val;
    }

    private long ModPow(long b, long e, long m)
    {
        b %= m;
        if (b < 0) b += m;
        long r = 1;
        while (e > 0)
        {
            if ((e & 1) == 1) r = (r * b) % m;
            b = (b * b) % m;
            e >>= 1;
        }
        return r;
    }

    private long Gcd(long a, long b)
    {
        a = a < 0 ? -a : a;
        b = b < 0 ? -b : b;
        while (b != 0) { long t = b; b = a % b; a = t; }
        return a;
    }

    private long ModInverse(long a, long m)
    {
        long m0 = m, y = 0, x = 1;
        if (m == 1) return 0;
        while (a > 1)
        {
            long q = a / m;
            long t = m;
            m = a % m;
            a = t;
            t = y;
            y = x - q * y;
            x = t;
        }
        return x < 0 ? x + m0 : x;
    }

    private string ComputeHash(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        uint[] hash = SHA1(bytes);
        return ToHexString(hash);
    }

    private uint[] SHA1(byte[] input)
    {
        uint h0 = 0x67452301, h1 = 0xEFCDAB89, h2 = 0x98BADCFE, h3 = 0x10325476, h4 = 0xC3D2E1F0;
        int len = input.Length, bitLen = len * 8;
        int newLen = ((len + 9) / 64 + 1) * 64;
        byte[] padded = new byte[newLen];
        for (int i = 0; i < len; i++) padded[i] = input[i];
        padded[len] = 0x80;
        for (int i = 0; i < 8; i++) padded[newLen - 8 + i] = (byte)((bitLen >> (56 - i * 8)) & 0xFF);

        for (int i = 0; i < padded.Length; i += 64)
        {
            uint[] w = new uint[80];
            for (int j = 0; j < 16; j++)
                w[j] = (uint)(padded[i + j * 4 + 0] << 24 | padded[i + j * 4 + 1] << 16 |
                               padded[i + j * 4 + 2] << 8 | padded[i + j * 4 + 3]);
            for (int j = 16; j < 80; j++)
                w[j] = LeftRotate(w[j - 3] ^ w[j - 8] ^ w[j - 14] ^ w[j - 16], 1);

            uint a = h0, b = h1, c = h2, d = h3, e = h4;
            for (int j = 0; j < 80; j++)
            {
                uint f, k;
                if (j < 20) { f = (b & c) | (~b & d); k = 0x5A827999; }
                else if (j < 40) { f = b ^ c ^ d; k = 0x6ED9EBA1; }
                else if (j < 60) { f = (b & c) | (b & d) | (c & d); k = 0x8F1BBCDC; }
                else { f = b ^ c ^ d; k = 0xCA62C1D6; }

                uint temp = LeftRotate(a, 5) + f + e + k + w[j];
                e = d; d = c; c = LeftRotate(b, 30); b = a; a = temp;
            }
            h0 += a; h1 += b; h2 += c; h3 += d; h4 += e;
        }
        return new uint[] { h0, h1, h2, h3, h4 };
    }

    private uint LeftRotate(uint x, int n) => (x << n) | (x >> (32 - n));

    private string ToHexString(uint[] hash)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
            sb.Append(hash[i].ToString("x8"));
        return sb.ToString().ToUpperInvariant();
    }

    private long HexToLong(string hex)
    {
        if (hex.Length > 16) hex = hex.Substring(hex.Length - 16);
        long v = 0;
        for (int i = 0; i < hex.Length; i++)
        {
            char c = hex[i];
            int d = (c >= '0' && c <= '9') ? c - '0' :
                    (c >= 'a' && c <= 'f') ? c - 'a' + 10 :
                    (c >= 'A' && c <= 'F') ? c - 'A' + 10 : -1;
            if (d == -1) throw new ArgumentException("HEX");
            v = (v << 4) | (uint)d;
        }
        return v;
    }
}