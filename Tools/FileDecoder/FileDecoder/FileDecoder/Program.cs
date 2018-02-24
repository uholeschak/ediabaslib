using System;
using System.IO;
using System.Text;
using zlib;

namespace FileDecoder
{
    static class Program
    {
        enum ResultCode
        {
            Ok,
            Error,
            Done,
        }

        // values from code table index: 0x76, 0xC3, 0x88, 0x3E, 0x99, 0x22, 0xCA, 0x07
        private static byte[] maskMult = { 0x8F, 0x97, 0x98, 0x29, 0xFA, 0x74, 0x9C, 0x7D };

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No input file specified");
                return 1;
            }

            string fileSpec = args[0];
            string dir = Path.GetDirectoryName(fileSpec);
            string searchPattern = Path.GetFileName(fileSpec);
            if (dir == null || searchPattern == null)
            {
                Console.WriteLine("Invalid file name");
                return 1;
            }

            string typeCodeString = "USA";
            if (args.Length >= 2)
            {
                typeCodeString = args[1];
            }

            if (typeCodeString.Length != 3)
            {
                Console.WriteLine("Type code string length must be 3");
                return 1;
            }

            try
            {
                string[] files = Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    Console.WriteLine("Decrypting: {0}", file);
                    string ext = Path.GetExtension(file);
                    if (string.Compare(ext, @".rod", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        string outFile = Path.ChangeExtension(file, @".rodtxt");
                        if (!DecryptSegmentFile(file, outFile))
                        {
                            Console.WriteLine("*** Decryption failed: {0}", file);
                        }
                    }
                    else
                    {
                        string outFile = Path.ChangeExtension(file, @".lbl");
                        if (!DecryptFile(file, outFile, typeCodeString))
                        {
                            Console.WriteLine("*** Decryption failed: {0}", file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return 0;
        }

        static bool DecryptFile(string inFile, string outFile, string typeCodeString)
        {
            byte typeCode = (byte) (typeCodeString[0] + typeCodeString[1] + typeCodeString[2]);
            try
            {
                using (FileStream fsRead = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream fsWrite = new FileStream(outFile, FileMode.Create))
                    {
                        for (int line = 0; ; line++)
                        {
                            byte[] data = DecryptLine(fsRead, line, typeCode);
                            if (data == null)
                            {
                                return false;
                            }
                            if (data.Length == 0)
                            {   // end of file
                                break;
                            }
                            if (line == 0)
                            {
                                if (!IsValidText(data))
                                {
                                    Console.WriteLine("Type code invalid, trying all values");
                                    bool found = false;
                                    for (int code = 0; code < 0x100; code++)
                                    {
                                        fsRead.Seek(0, SeekOrigin.Begin);
                                        bool bValid = true;
                                        for (int j = 0; j < 4; j++)
                                        {
                                            data = DecryptLine(fsRead, j, (byte)code);
                                            if (data == null)
                                            {
                                                bValid = false;
                                                break;
                                            }
                                            if (data.Length == 0)
                                            {   // end of file
                                                break;
                                            }
                                            if (!IsValidText(data))
                                            {
#if false
                                                System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
                                                string asc = ascii.GetString(data);
                                                Console.WriteLine("Invalid: {0}", asc);
#endif
                                                bValid = false;
                                                break;
                                            }
                                        }

                                        if (bValid)
                                        {
                                            found = true;
                                            typeCode = (byte)code;
                                            Console.WriteLine("Code found: {0:X02}", typeCode);
                                            fsRead.Seek(0, SeekOrigin.Begin);
                                            data = DecryptLine(fsRead, line, typeCode);
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        Console.WriteLine("Type code not found");
                                        return false;
                                    }
                                }
                            }

                            foreach (byte value in data)
                            {
                                if (value == 0)
                                {
                                    break;
                                }
                                fsWrite.WriteByte(value);
                            }
                            fsWrite.WriteByte((byte)'\r');
                            fsWrite.WriteByte((byte)'\n');
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static bool IsValidText(byte[] data)
        {
            if (data.Length < 6)
            {
                return false;
            }
            for (int i = 0; i < 6; i++)
            {
                byte value = data[i];
                if (i == 0)
                {
                    if (!((value >= 0x30 && value <= 0x39) || (value >= 0x41 && value <= 0x5A) || (value == 0x3A)))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!((value >= 0x30 && value <= 0x39) || (value >= 0x41 && value <= 0x5A) || (value >= 0x61 && value <= 0x7A) ||
                          (value == 0x20) || (value == 0x28) || (value == 0x2C) || (value == 0x3A) || (value == 0x0D) || (value == 0x00)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static byte[] DecryptLine(FileStream fs, int line, byte typeCode)
        {
            try
            {
                int lenH = fs.ReadByte();
                int lenL = fs.ReadByte();
                if (lenH < 0 || lenL < 0)
                {
                    return new byte[0];
                }

                int dataLen = (lenH << 8) + lenL;
                int remainder = 0;
                if ((dataLen % 8) != 0)
                {
                    remainder = 8 - dataLen % 8;
                }

                int blockSize = dataLen + remainder;
                byte[] data = new byte[blockSize + 8];
                int count = fs.Read(data, 0, blockSize + 2);
                if (count < blockSize + 2)
                {
                    return null;
                }
                UInt32[] buffer = new uint[(blockSize / sizeof(UInt32)) + 2];
                for (int i = 0; i < data.Length; i += sizeof(UInt32))
                {
                    buffer[i >> 2] = BitConverter.ToUInt32(data, i);
                }

                Int32 code1 = line;
                Int32 code2 = typeCode;
                Int32 mask1 = (code1 + 2) * (code2 + 1) + (code1 + 3) * (code1 + 1) * (code2 + 2);
                Int32 mask2 = (code1 + 2) * (code2 + 1) * (code2 + 3);
                Int32 tempVal1 = code2 + 1;
                Int32 tempVal2 = code2 % (code1 + 1);
                if (tempVal2 == 0)
                {
                    tempVal2 = code1 % tempVal1;
                }
                mask2 += tempVal2;
                if (mask1 < 0xFFFF)
                {
                    mask1 = (mask1 << 16) + (code1 + 2) * (code2 + 1) * (code2 + 3) * (code1 + 4);
                }
                if (mask2 < 0xFFFF)
                {
                    mask2 = (mask2 << 16) + (code1 + 1) * (code1 + 2) * (code1 + 3);
                }

                UInt32[] mask = new UInt32[2];
                mask[0] = (UInt32)mask1;
                mask[1] = (UInt32)mask2;
                if (!DecryptBlock(mask, buffer, 2))
                {
                    return null;
                }

                byte[] result = new byte[blockSize];
                for (int i = 0; i < blockSize; i += sizeof(UInt32))
                {
                    byte[] conf = BitConverter.GetBytes(buffer[i >> 2]);
                    Array.Copy(conf, 0, result, i, conf.Length);
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        static bool DecryptSegmentFile(string inFile, string outFile)
        {
            try
            {
                using (FileStream fsRead = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream fsWrite = new FileStream(outFile, FileMode.Create))
                    {
                        for (;;)
                        {
                            ResultCode resultCode = DecryptSegment(fsRead, fsWrite);
                            if (resultCode == ResultCode.Error)
                            {
                                return false;
                            }
                            if (resultCode == ResultCode.Done)
                            {
                                break;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static ResultCode DecryptSegment(FileStream fsRead, FileStream fsWrite)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                for (;;)
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Done;
                    }
                    if (value == 0x0A && sb.Length > 0)
                    {
                        break;
                    }
                    if (value != 0x0A && value != 0x0D)
                    {
                        sb.Append((char)value);
                    }
                }
                string segmentName = sb.ToString();

                int frameLen = 0;
                for (int i = 0; i < 3; i++)
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Error;
                    }

                    frameLen <<= 8;
                    frameLen |= value;
                }

                int contentLen = 0;
                for (int i = 0; i < 3; i++)
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Error;
                    }

                    contentLen <<= 8;
                    contentLen |= value;
                }

                bool compressed = (frameLen & 0x800000) == 0;
                int dataLength = frameLen & 0x7FFFFF;
                if ((dataLength & 0x07) != 0)
                {
                    return ResultCode.Error;
                }
                int readLength = dataLength + 8;
                byte[] data = new byte[readLength];

                int count = fsRead.Read(data, 0, data.Length);
                if (count < data.Length)
                {
                    return ResultCode.Error;
                }

                UInt32[] buffer = new uint[(data.Length / sizeof(UInt32))];
                for (int i = 0; i < data.Length; i += sizeof(UInt32))
                {
                    buffer[i >> 2] = BitConverter.ToUInt32(data, i);
                }

                UInt32 mask1;
                UInt32 mask2;
                if (compressed)
                {
                    mask1 = 0x8638C6B9;
                    mask2 = 0xB57820BC;
                }
                else
                {
                    mask1 = 0x4C50936B;
                    mask2 = 0x38B8B856;
                }
                UInt32[] mask = new UInt32[2];
                mask[0] = mask1;
                mask[1] = mask2;
                if (!DecryptBlock(mask, buffer, 0))
                {
                    return ResultCode.Error;
                }

                byte[] result = new byte[dataLength];
                for (int i = 0; i < dataLength; i += sizeof(UInt32))
                {
                    byte[] conf = BitConverter.GetBytes(buffer[i >> 2]);
                    Array.Copy(conf, 0, result, i, conf.Length);
                }

                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] segmentData = ascii.GetBytes(segmentName);
                fsWrite.Write(segmentData, 0, segmentData.Length);
                fsWrite.WriteByte((byte)'\r');
                fsWrite.WriteByte((byte)'\n');

                if (!compressed)
                {
                    int writeLen = contentLen < result.Length ? contentLen : result.Length;
                    fsWrite.Write(result, 0, writeLen);
                    return ResultCode.Ok;
                }
                DecompressData(result, fsWrite, contentLen);

                return ResultCode.Ok;
            }
            catch (Exception)
            {
                return ResultCode.Error;
            }
        }

        static bool DecryptBlock(UInt32[] mask, UInt32[] buffer, int codeTable)
        {
            UInt32 code1;
            UInt32 code2;
            UInt32 code3;
            UInt32 code4;

            switch (codeTable)
            {
                case 1:
                    code1 = 0x87A32FEB;
                    code2 = 0x77539F1B;
                    code3 = 0x67030F4B;
                    code4 = 0x57B37F7B;
                    break;

                case 2:
                    code1 = 0xFA7E14D0;
                    code2 = 0x249B910E;
                    code3 = 0x2FDD6FFC;
                    code4 = 0x15834A78;
                    break;

                default:
                    code1 = 0x29B76A4;
                    code2 = 0xCB6DB50A;
                    code3 = 0x71395D29;
                    code4 = 0x0DBC09C2;
                    break;
            }

            UInt32 mask1 = mask[0];
            UInt32 mask2 = mask[1];

            int bufferSize = buffer.Length * sizeof(UInt32);
            int dataSize = bufferSize;
            dataSize &= ~0x07;
            if (dataSize <= 8)
            {
                return false;
            }
            int blockCount = ((dataSize - 9) >> 3) + 1;
            int dataPos = 0;
            for (int i = 0; i < blockCount; i++)
            {
                UInt32 data1 = buffer[dataPos + 0];
                UInt32 data2 = buffer[dataPos + 1];
                UInt32 data1Old = data1;
                UInt32 data2Old = data2;
                UInt32 codeDyn = 0xC6EF3720;

                for (int j = 0; j < 32; j++)
                {
                    data2 -= (data1 + codeDyn) ^ (code3 + 16 * data1) ^ (code4 + (data1 >> 5));
                    data1 -= (data2 + codeDyn) ^ (code1 + 16 * data2) ^ (code2 + (data2 >> 5));
                    codeDyn += 0x61C88647;
                }
                buffer[dataPos + 0] = data1 ^ mask1;
                buffer[dataPos + 1] = data2 ^ mask2;
                mask1 = data1Old;
                mask2 = data2Old;

                dataPos += 2;
            }

            return true;
        }

        static void DecompressData(byte[] inData, Stream fsout, int bytes)
        {
            using (ZOutputStream outZStream = new ZOutputStream(fsout))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream, bytes);
                outZStream.finish();
            }
        }

        static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[0x8000];
            int len;
            while (bytes > 0 && (len = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, len);
                bytes -= len;
            }
        }
    }
}
