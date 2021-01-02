using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Globalization;
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

        private const string ValidBootloader = "2.1";
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

        private static Dictionary<string, string> deviceDict = new Dictionary<string, string>
        {
            {"1E9007", "ATtiny13"},
            {"1E910A", "ATtiny2313"},
            {"1E9205", "ATmega48"},
            {"1E9206", "ATtiny45"},
            {"1E9207", "ATtiny44"},
            {"1E9208", "ATtiny461"},
            {"1E9306", "ATmega8515"},
            {"1E9307", "ATmega8"},
            {"1E9308", "ATmega8535"},
            {"1E930A", "ATmega88"},
            {"1E930B", "ATtiny85"},
            {"1E930C", "ATtiny84"},
            {"1E930D", "ATtiny861"},
            {"1E930F", "ATmega88P"},
            {"1E9403", "ATmega16"},
            {"1E9404", "ATmega162"},
            {"1E9406", "ATmega168"},
            {"1E940B", "ATmega168P"},
            {"1E9501", "ATmega323"},
            {"1E9502", "ATmega32"},
            {"1E950F", "ATmega328P"},
            {"1E9609", "ATmega644"},
            {"1E9802", "ATmega2561"},
        };

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

                bool supportCrc = DetectSupport(CommandProgramCheckCRC);
                bool supportVerify = DetectSupport(CommandProgramVerify);

                string deviceRevision = ReadRevisionInfo();
                if (string.IsNullOrEmpty(deviceRevision))
                {
                    return false;
                }

                if (string.Compare(deviceRevision, ValidBootloader, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                string deviceSignature = ReadSignatureInfo();
                if (string.IsNullOrEmpty(deviceSignature))
                {
                    return false;
                }

                string deviceName = GetDeviceName(deviceSignature);
                if (string.IsNullOrEmpty(deviceName))
                {
                    return false;
                }

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
                    if (!EdFtdiInterface.InterfaceSendData(ResetCmd, ResetCmd.Length, false, 0))
                    {
                        return false;
                    }

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
                    if (!EdFtdiInterface.InterfaceSendData(bootData.ToArray(), bootData.Count, false, 0))
                    {
                        return false;
                    }

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
                if (!EdFtdiInterface.InterfaceSendData(command, command.Length, false, 0))
                {
                    return false;
                }

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
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DetectSupport(byte commandByte)
        {
            try
            {
                try
                {
                    byte[] commandTest = { IdentifierCommand, commandByte };
                    if (!EdFtdiInterface.InterfaceSendData(commandTest, commandTest.Length, false, 0))
                    {
                        return false;
                    }

                    SkipOneWireBytes(2);

                    byte[] buffer = new byte[1];
                    if (EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        if (buffer[0] == StatusBadCommand)
                        {
                            return false;
                        }
                    }

                    byte[] commandFinish = { IdentifierCommand, CommandProgramFinish };
                    if (!EdFtdiInterface.InterfaceSendData(commandFinish, commandFinish.Length, false, 0))
                    {
                        return false;
                    }

                    if (EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        if (buffer[0] == StatusSuccess || buffer[0] == StatusFail)
                        {
                            return true;
                        }
                    }
                }
                finally
                {
                    EdFtdiInterface.InterfacePurgeInBuffer();
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[] ReadInfo(byte commandByte)
        {
            try
            {
                byte[] readAnswer = null;
                try
                {
                    byte[] commandTest = { IdentifierCommand, commandByte };
                    if (!EdFtdiInterface.InterfaceSendData(commandTest, commandTest.Length, false, 0))
                    {
                        return null;
                    }

                    SkipOneWireBytes(2);

                    byte[] buffer = new byte[1];
                    if (!EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        return null;
                    }
                    byte readLeader = buffer[0];

                    if (!EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        return null;
                    }
                    byte answerLength = buffer[0];

                    if (answerLength > 0)
                    {
                        answerLength--;
                        readAnswer = new byte[answerLength];
                        for (int i = 0; i < answerLength; i++)
                        {
                            if (!EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                            {
                                return null;
                            }
                            readAnswer[i] = buffer[0];
                        }

                    }

                    if (!EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        return null;
                    }
                    byte readTrailer = buffer[0];

                    if (readLeader != IdentifierAnswer || readTrailer != StatusSuccess)
                    {
                        return null;
                    }
                }
                finally
                {
                    EdFtdiInterface.InterfacePurgeInBuffer();
                }

                return readAnswer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ReadRevisionInfo()
        {
            byte[] readBytes = ReadInfo(ReadRevision);
            if (readBytes == null || readBytes.Length != 2)
            {
                return null;
            }

            string revision = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", readBytes[0], readBytes[1]);
            return revision;
        }

        public static string ReadSignatureInfo()
        {
            byte[] readBytes = ReadInfo(ReadSignature);
            if (readBytes == null)
            {
                return null;
            }

            string signature = BitConverter.ToString(readBytes).Replace("-", "");
            return signature;
        }

        public static string GetDeviceName(string deviceSignature)
        {
            if (string.IsNullOrEmpty(deviceSignature))
            {
                return null;
            }

            if (!deviceDict.TryGetValue(deviceSignature.ToUpperInvariant(), out string deviceName))
            {
                return null;
            }

            return deviceName;
        }

        public static bool StartFirmware()
        {
            try
            {
                try
                {
                    byte[] command = { IdentifierCommand, CommandProgramStart };
                    if (!EdFtdiInterface.InterfaceSendData(command, command.Length, false, 0))
                    {
                        return false;
                    }

                    SkipOneWireBytes(2);

                    byte[] buffer = new byte[1];
                    if (EdFtdiInterface.InterfaceReceiveData(buffer, 0, buffer.Length, ReadTimeout, ReadTimeout, null))
                    {
                        if (buffer[0] == StatusBadCommand)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    EdFtdiInterface.InterfacePurgeInBuffer();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
