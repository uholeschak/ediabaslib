using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PsdzClient.Core;

public class Base32
{
    public const string Base32StandardAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public const char StandardPaddingChar = '=';

    public const string ZBase32Alphabet = "ybndrfg8ejkmcpqxot1uwisza345h769";

    private static Dictionary<string, Dictionary<string, uint>> indexes = new Dictionary<string, Dictionary<string, uint>>(2, StringComparer.Ordinal);

    private readonly string alphabet;

    private Dictionary<string, uint> index;

    private bool ignoreWhiteSpaceWhenDecoding;

    private bool isCaseSensitive;

    private char paddingChar;

    private bool usePadding;

    public bool IgnoreWhiteSpaceWhenDecoding
    {
        get
        {
            return ignoreWhiteSpaceWhenDecoding;
        }
        set
        {
            ignoreWhiteSpaceWhenDecoding = value;
        }
    }

    public bool IsCaseSensitive
    {
        get
        {
            return isCaseSensitive;
        }
        set
        {
            isCaseSensitive = value;
        }
    }

    public char PaddingChar
    {
        get
        {
            return paddingChar;
        }
        set
        {
            paddingChar = value;
        }
    }

    public bool UsePadding
    {
        get
        {
            return usePadding;
        }
        set
        {
            usePadding = value;
        }
    }

    public Base32()
        : this(padding: false, caseSensitive: false, ignoreWhiteSpaceWhenDecoding: false, "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567")
    {
    }

    public Base32(bool padding)
        : this(padding, caseSensitive: false, ignoreWhiteSpaceWhenDecoding: false, "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567")
    {
    }

    public Base32(bool padding, bool caseSensitive)
        : this(padding, caseSensitive, ignoreWhiteSpaceWhenDecoding: false, "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567")
    {
    }

    public Base32(bool padding, bool caseSensitive, bool ignoreWhiteSpaceWhenDecoding)
        : this(padding, caseSensitive, ignoreWhiteSpaceWhenDecoding, "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567")
    {
    }

    public Base32(string alternateAlphabet)
        : this(padding: false, caseSensitive: false, ignoreWhiteSpaceWhenDecoding: false, alternateAlphabet)
    {
    }

    public Base32(bool padding, bool caseSensitive, bool ignoreWhiteSpaceWhenDecoding, string alternateAlphabet)
    {
        if (alternateAlphabet.Length != 32)
        {
            throw new ArgumentException("Alphabet must be exactly 32 characters long for base 32 encoding.");
        }
        paddingChar = '=';
        usePadding = padding;
        isCaseSensitive = caseSensitive;
        this.ignoreWhiteSpaceWhenDecoding = ignoreWhiteSpaceWhenDecoding;
        alphabet = alternateAlphabet;
    }

    public static byte[] FromBase32String(string input)
    {
        return new Base32().Decode(input);
    }

    public static string ToBase32String(byte[] data)
    {
        return new Base32().Encode(data);
    }

    public byte[] Decode(string input)
    {
        if (ignoreWhiteSpaceWhenDecoding)
        {
            input = Regex.Replace(input, "\\s+", string.Empty);
        }
        if (usePadding)
        {
            if (input.Length % 8 != 0)
            {
                throw new ArgumentException("Invalid length for a base32 string with padding.");
            }
            input = input.TrimEnd(paddingChar);
        }
        EnsureAlphabetIndexed();
        MemoryStream memoryStream = new MemoryStream(Math.Max((int)Math.Ceiling((double)(input.Length * 5) / 8.0), 1));
        for (int i = 0; i < input.Length; i += 8)
        {
            int num = Math.Min(input.Length - i, 8);
            ulong num2 = 0uL;
            int num3 = (int)Math.Floor((double)num * 0.625);
            for (int j = 0; j < num; j++)
            {
                if (!index.TryGetValue(input.Substring(i + j, 1), out var value))
                {
                    throw new ArgumentException("Invalid character '" + input.Substring(i + j, 1) + "' in base32 string, valid characters are: " + alphabet);
                }
                num2 |= (ulong)value << (num3 + 1) * 8 - j * 5 - 5;
            }
            byte[] bytes = BitConverter.GetBytes(num2);
            Array.Reverse(bytes);
            memoryStream.Write(bytes, bytes.Length - (num3 + 1), num3);
        }
        return memoryStream.ToArray();
    }

    public string Encode(byte[] data)
    {
        StringBuilder stringBuilder = new StringBuilder(Math.Max((int)Math.Ceiling((double)(data.Length * 8) / 5.0), 1));
        byte[] array = new byte[8];
        byte[] array2 = new byte[8];
        for (int i = 0; i < data.Length; i += 5)
        {
            int num = Math.Min(data.Length - i, 5);
            Array.Copy(array, array2, array.Length);
            Array.Copy(data, i, array2, array2.Length - (num + 1), num);
            Array.Reverse(array2);
            ulong num2 = BitConverter.ToUInt64(array2, 0);
            for (int num3 = (num + 1) * 8 - 5; num3 > 3; num3 -= 5)
            {
                stringBuilder.Append(alphabet[(int)((num2 >> num3) & 0x1F)]);
            }
        }
        if (usePadding)
        {
            stringBuilder.Append(string.Empty.PadRight((stringBuilder.Length % 8 != 0) ? (8 - stringBuilder.Length % 8) : 0, paddingChar));
        }
        return stringBuilder.ToString();
    }

    private void EnsureAlphabetIndexed()
    {
        if (index != null)
        {
            return;
        }
        string key = (isCaseSensitive ? "S" : "I") + alphabet;
        if (!indexes.TryGetValue(key, out var value))
        {
            lock (indexes)
            {
                if (!indexes.TryGetValue(key, out value))
                {
                    value = new Dictionary<string, uint>(alphabet.Length, isCaseSensitive ? StringComparer.InvariantCulture : StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        value[alphabet.Substring(i, 1)] = (uint)i;
                    }
                    indexes.Add(key, value);
                }
            }
        }
        index = value;
    }
}
