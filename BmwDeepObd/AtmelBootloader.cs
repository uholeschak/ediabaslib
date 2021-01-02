using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using EdiabasLib;

// ReSharper disable RedundantCaseLabel

namespace BmwDeepObd
{
    public static class AtmelBootloader
    {
        // ReSharper restore InconsistentNaming
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        public static bool LoadProgramFile(string fileName, byte[] buffer, out uint usedBuffer)
        {
            ClearBuffer(buffer);
            usedBuffer = 0;

            try
            {
                uint checksum = 0;
                uint segmentAddress = 0;
                bool segmentedAdress = false;

                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                using (StreamReader srHexFile = new StreamReader(fileName))
                {
                    for (;;)
                    {
                        string line = srHexFile.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        if ((line[0] != ':') || (line.Length < 11))
                        {
                            // skip line if not hex line entry,or not minimum length ":BBAAAATTCC"
                            continue;
                        }

                        uint dataLength = Convert.ToUInt32(line.Substring(1, 2), 16);
                        checksum += dataLength;

                        uint address = Convert.ToUInt32(line.Substring(3, 4), 16);
                        checksum += address & 0xFF;
                        checksum += (address >> 8) & 0xFF;

                        uint memoryAddress = address;
                        if (segmentedAdress)
                        {
                            memoryAddress += segmentAddress;
                        }

                        uint recordType = Convert.ToUInt32(line.Substring(7, 2), 16);
                        checksum += recordType;

                        uint lineChecksum;
                        switch (recordType)
                        {
                            case 0: // data block
                                if (line.Length < (11 + (2 * dataLength)))
                                {
                                    // skip if line isn't long enough for bytecount.
                                    continue;
                                }

                                for (uint byteCount = 0; byteCount < dataLength; byteCount++)
                                {
                                    uint data = Convert.ToUInt32(line.Substring((int)(9 + (2 * byteCount)), 2), 16);
                                    checksum += data;

                                    int index = (int)(memoryAddress + byteCount);
                                    if (index >= buffer.Length)
                                    {
                                        return false;
                                    }

                                    if (buffer[index] != 0xFF)
                                    {
                                        return false;
                                    }

                                    buffer[index] = (byte) data;
                                }

                                lineChecksum = Convert.ToUInt32(line.Substring((int)(9 + (2 * dataLength)), 2), 16);
                                if (usedBuffer < memoryAddress + dataLength)
                                {
                                    usedBuffer = memoryAddress + dataLength;
                                }
                                break;

                            case 1: // end of file
                                lineChecksum = Convert.ToUInt32(line.Substring(9, 2), 16);
                                break;

                            case 2: // segment address
                                if (line.Length < (11 + (2 * dataLength)))
                                {
                                    // skip if line isn't long enough for bytecount.
                                    continue;
                                }

                                segmentedAdress = true;
                                segmentAddress = Convert.ToUInt32(line.Substring(9, 4), 16);
                                lineChecksum = Convert.ToUInt32(line.Substring(13, 2), 16);
                                break;

                            default:
                                return false;
                        }

                        checksum += lineChecksum;
                        if ((checksum & 0xFF) != 0x00)
                        {
                            // checksum invalid
                            return false;
                        }
                    }
                }

                if (usedBuffer <= 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void ClearBuffer(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0xFF;
            }
        }
    }
}
