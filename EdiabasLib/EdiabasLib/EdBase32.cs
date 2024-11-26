#region Copyright (c) 2019 Atif Aziz. All rights reserved.
/*
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Text;

namespace EdiabasLib
{
    /// <summary>
    /// An implementation of Base 32 based on the
    /// <a href="https://www.crockford.com/base32.html">scheme described by
    /// Douglas Crockford</a>. It uses a symbol set of 10 digits and 22
    /// uppercase. Letter U is not allowed; letters I and L are treated the
    /// same as 1 and O is treated the same as 0.
    /// </summary>
    /// <remarks>
    /// This implementation does not support check symbols that are
    /// otherwise defined for the purpose of detecting wrong-symbol and
    /// transposed-symbol errors.
    /// </remarks>

    static class EdBase32
    {
        const string Symbols = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        public static string EncodeByteString(string input)
        {
            StringBuilder sb = new StringBuilder();
            EncodeByteString(input, sb);
            return sb.ToString();
        }

        public static void EncodeByteString(string input, StringBuilder output)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            ushort bb = 0;
            int bits = 0;

            foreach (char ch in input)
            {
                Encode(checked((byte)ch), ref bb, ref bits, output);
            }

            Flush(bb, bits, output);
        }

        public static string Encode(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return Encode(buffer, 0, buffer.Length);
        }

        public static string Encode(byte[] buffer, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            Encode(buffer, offset, length, sb);
            return sb.ToString();
        }

        public static void Encode(byte[] buffer, StringBuilder output)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            Encode(buffer, 0, buffer.Length, output);
        }

        public static void Encode(byte[] buffer, int offset, int length, StringBuilder output)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (offset + length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            ushort bb = 0;
            int bits = 0;
            for (; length > 0; offset++, length--)
            {
                Encode(buffer[offset], ref bb, ref bits, output);
            }

            Flush(bb, bits, output);
        }

        static void Encode(byte b, ref ushort bb, ref int bits, StringBuilder output)
        {
            bb |= (ushort)(b << (8 - bits));
            bits += 8;
            for (; bits >= 5; bb <<= 5, bits -= 5)
            {
                output.Append(Symbols[bb >> 11]);
            }
        }

        static void Flush(ushort bb, int bits, StringBuilder output)
        {
            if (bits > 0)
            {
                output.Append(Symbols[bb >> 11]);
            }
        }

        static readonly sbyte[] ValueBySymbol =
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, -1, -1, /*
                A   B   C   D   E   F   G   H   I   J   K   L   M   N   O   */
            -1, 10, 11, 12, 13, 14, 15, 16, 17, 1, 18, 19, 1, 20, 21, 0, /*
             P   Q   R   S   T   U   V   W   X   Y   Z                       */
            22, 23, 24, 25, 26, -1, 27, 28, 29, 30, 31, -1, -1, -1, -1, -1, /*
                 a   b   c   d   e   f   g   h   i   j   k   l   m   n   o   */
            -1, 10, 11, 12, 13, 14, 15, 16, 17, 1, 18, 19, 1, 20, 21, 0, /*
             p   q   r   s   t   u   v   w   x   y   z                       */
            22, 23, 24, 25, 26, -1, 27, 28, 29, 30, 31, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        static readonly byte[] ZeroBytes = { };

        public static byte[] Decode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length == 0)
            {
                return ZeroBytes;
            }

            int si = 0;
            int hyphens = 0;
            int i;
            while ((i = input.IndexOf('-', si)) >= 0)
            {
                hyphens++;
                si = i + 1;
            }

            byte[] buffer = new byte[(input.Length - hyphens) * 5 / 8];
            Decode(input, buffer);
            return buffer;
        }

        static void Decode(string input, byte[] buffer, int offset = 0)
        {
            ushort bb = 0;
            var bits = 0;
            foreach (var ch in input)
            {
                if (ch == '-')
                {
                    continue;
                }

                sbyte b = ValueBySymbol[ch];
                if (b < 0)
                {
                    throw new FormatException($"'{ch}' is an invalid symbol.");
                }

                bb |= (ushort)((ushort)b << (11 - bits));
                bits += 5;
                for (; bits >= 8; bits -= 8, bb <<= 8)
                {
                    buffer[offset++] = (byte)(bb >> 8);
                }
            }
        }
    }
}