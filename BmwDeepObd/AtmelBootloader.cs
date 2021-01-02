using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading;
using EdiabasLib;

// ReSharper disable RedundantCaseLabel

namespace BmwDeepObd
{
    public static class AtmelBootloader
    {
        // ReSharper restore InconsistentNaming
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int ConnectLoopTimeout = 100;
        private const int ReadTimeout = 250;
        private const int SyncLoopTimeout = 3000;
        private const int SyncDelay = 100;

        private const byte IdentifierCommand = 0xA5;
        private const byte IdentifierEscape = 0xA5;
        private const byte IdentifierAnswer = 0xA8;
        private const byte IdentifierContinue = 0xA9;

        private const byte ReadRevision = 0x00;
        private const byte ReadBufferSize = 0x01;
        private const byte ReadSignature = 0x02;
        private const byte ReadFlashSize = 0x03;

        private const byte CommandProgramWrite = 0x04;
        private const byte CommandProgramStart = 0x05;
        private const byte CommandProgramCheckCRC = 0x06;
        private const byte CommandProgramVerify = 0x07;
        private const byte CommandProgramFinish= 0x80;

        private const byte StatusConnected = 0xA6;
        private const byte StatusBadCommand = 0xA7;
        private const byte StatusSuccess = 0xAA;
        private const byte StatusFail = 0xAB;
        private const byte DataAutobaudLeader = 0x0D;
        private const byte DataPasswordTrailer = 0xFF;
        private static byte[] ResetCmd = {0x82, 0xF1, 0xF1, 0xFF, 0xFF, 0x62};

        private static bool _oneWire = false;

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
                                    uint data = Convert.ToUInt32(line.Substring((int) (9 + (2 * byteCount)), 2), 16);
                                    checksum += data;

                                    int index = (int) (memoryAddress + byteCount);
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

                                lineChecksum = Convert.ToUInt32(line.Substring((int) (9 + (2 * dataLength)), 2), 16);
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

        public static bool FwUpdate(string fileName, string password = "Peda")
        {
            try
            {
                byte[] buffer = new byte[0x40000];
                if (!LoadProgramFile(fileName, buffer, out uint usedBuffer))
                {
                    return false;
                }

                if (!Connect(password, out bool oneWireMode))
                {
                    return false;
                }

                _oneWire = oneWireMode;

                if (!StartFirmware())
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

        public static bool SkipOneWireBytes(int count)
        {
            try
            {
                if (_oneWire)
                {
                    byte[] buffer = new byte[count];
                    if (!EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception )
            {
                return false;
            }
        }

        public static bool Connect(string password, out bool oneWireMode, int connectRetries = 50, bool sendReset = true, bool detectOneWire = true)
        {
            oneWireMode = false;
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return false;
                }

                if (sendReset)
                {
                    EdFtdiInterface.InterfaceSendData(ResetCmd, ResetCmd.Length, false, 0);
                    Thread.Sleep(150);
                }

                EdFtdiInterface.InterfacePurgeInBuffer();

                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte wireDetectChar = enc.GetBytes(password)[0];
                byte[] buffer = new byte[1];
                long startTime;
                bool connectionEstablished = false;
                for (int connectRetry = 0; connectRetry < connectRetries; connectRetry++)
                {
                    // ReSharper disable once UseObjectOrCollectionInitializer
                    List<byte> bootData = new List<byte>();
                    bootData.Add(DataAutobaudLeader);
                    bootData.AddRange(enc.GetBytes(password));
                    bootData.Add(DataPasswordTrailer);
                    EdFtdiInterface.InterfaceSendData(bootData.ToArray(), bootData.Count, false, 0);

                    startTime = Stopwatch.GetTimestamp();
                    for (;;)
                    {
                        if (EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, 10, 10, null))
                        {
                            if (detectOneWire && buffer[0] == wireDetectChar)
                            {
                                oneWireMode = true;
                            }

                            if (buffer[0] == StatusConnected)
                            {
                                connectionEstablished = true;
                            }
                        }

                        if (Stopwatch.GetTimestamp() - startTime > ConnectLoopTimeout * TickResolMs)
                        {
                            break;
                        }
                    }

                    if (connectionEstablished)
                    {
                        break;
                    }
                }

                if (!connectionEstablished)
                {
                    return false;
                }

                EdFtdiInterface.InterfacePurgeInBuffer();
                Thread.Sleep(SyncDelay);

                byte[] command = { IdentifierCommand };
                EdFtdiInterface.InterfaceSendData(command, command.Length, false, 0);

                bool loopTimeout = false;
                startTime = Stopwatch.GetTimestamp();
                for (;;)
                {
                    if (EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, 10, 10, null))
                    {
                        if (buffer[0] == StatusSuccess)
                        {
                            break;
                        }
                    }

                    if (Stopwatch.GetTimestamp() - startTime > SyncLoopTimeout * TickResolMs)
                    {
                        loopTimeout = true;
                        break;
                    }
                }

                EdFtdiInterface.InterfacePurgeInBuffer();
                if (loopTimeout)
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

        public static bool StartFirmware()
        {
            try
            {
                byte[] command = {IdentifierCommand, CommandProgramStart};
                EdFtdiInterface.InterfaceSendData(command, command.Length, false, 0);

                SkipOneWireBytes(2);

                byte[] buffer = new byte[1];
                if (EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                {
                    if (buffer[0] == StatusBadCommand)
                    {
                        return false;
                    }
                }

                EdFtdiInterface.InterfacePurgeInBuffer();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
