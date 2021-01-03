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
        private const int ReadLongTimeout = 750;
        private const int SyncLoopTimeout = 3000;
        private const int SyncDelay = 100;
        private const int VerifyChunkLength = 512;
        private const int MinimumWriteBuffer = 8;

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
        private const byte DataIllegalChar = 0x13;
        private const byte DataIllegalCharShift = 0x80;
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
        private static int _failureAddress = 0;
        private static UInt16 _connectionCRC = 0x0000;

        static int FailureAddress => _failureAddress;

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

        public static bool FwUpdate(string fileName, bool programWrite = true, bool programVerify = true, bool programStart = true, string password = "Peda")
        {
            try
            {
                byte[] buffer = new byte[0x40000];
                if (!LoadProgramFile(fileName, buffer, out uint updateBufferUsed))
                {
                    return false;
                }

                if (!Connect(password, out bool oneWireMode))
                {
                    return false;
                }

                _failureAddress = 0;
                _oneWire = oneWireMode;

                bool supportsCrc = DetectSupport(CommandProgramCheckCRC);
                ResetCrc();
                bool supportsVerify = DetectSupport(CommandProgramVerify);

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

                long deviceFlashSize = ReadFlashSizeInfo();
                if (deviceFlashSize < 0)
                {
                    return false;
                }

                if (deviceFlashSize > buffer.Length)
                {
                    return false;
                }

                if (deviceFlashSize < updateBufferUsed)
                {
                    return false;
                }

                long deviceWriteBuffer = ReadBufferSizeInfo();
                if (deviceWriteBuffer < MinimumWriteBuffer)
                {
                    return false;
                }

                if (supportsCrc)
                {
                    if (!CheckCrc())
                    {
                        return false;
                    }
                }

                if (programWrite)
                {
                    if (!WriteFirmware(buffer, (int)updateBufferUsed, (int)deviceWriteBuffer))
                    {
                        return false;
                    }
                }

                if (programVerify && supportsVerify)
                {
                    if (!VerifyFirmware(buffer, (int)updateBufferUsed))
                    {
                        return false;
                    }
                }

                if (supportsCrc)
                {
                    if (!CheckCrc())
                    {
                        return false;
                    }
                }

                if (programStart)
                {
                    if (!StartFirmware())
                    {
                        return false;
                    }
                }

                Thread.Sleep(500);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SendByte(byte data)
        {
            byte[] command = {data};
            if (!EdFtdiInterface.InterfaceSendData(command, command.Length, false, 0))
            {
                return false;
            }

            CalculateCrc(data);

            return true;
        }

        public static bool SendBuffer(byte[] buffer, int length)
        {
            if (!EdFtdiInterface.InterfaceSendData(buffer, length, false, 0))
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                CalculateCrc(buffer[i]);
            }

            return true;
        }

        public static bool ReceiveBuffer(byte[] buffer, int length, int timeout)
        {
            return EdFtdiInterface.InterfaceReceiveData(buffer, 0, length, timeout, timeout, null);
        }

        public static bool PurgeInBuffer()
        {
            return EdFtdiInterface.InterfacePurgeInBuffer();
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
                    if (!SendBuffer(ResetCmd, ResetCmd.Length))
                    {
                        return false;
                    }

                    Thread.Sleep(150);
                }

                PurgeInBuffer();

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
                    if (!SendBuffer(bootData.ToArray(), bootData.Count))
                    {
                        return false;
                    }

                    startTime = Stopwatch.GetTimestamp();
                    for (;;)
                    {
                        if (ReceiveBuffer(buffer, buffer.Length, 10))
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

                PurgeInBuffer();
                Thread.Sleep(SyncDelay);

                if (!SendByte(IdentifierCommand))
                {
                    return false;
                }

                bool loopTimeout = false;
                startTime = Stopwatch.GetTimestamp();
                for (;;)
                {
                    if (ReceiveBuffer(buffer, buffer.Length, 10))
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

                PurgeInBuffer();
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
                    if (!ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
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
                    if (!SendBuffer(commandTest, commandTest.Length))
                    {
                        return false;
                    }

                    SkipOneWireBytes(2);

                    byte[] buffer = new byte[1];
                    if (ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
                    {
                        if (buffer[0] == StatusBadCommand)
                        {
                            return false;
                        }
                    }

                    byte[] commandFinish = { IdentifierCommand, CommandProgramFinish };
                    if (!SendBuffer(commandFinish, commandFinish.Length))
                    {
                        return false;
                    }

                    if (ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
                    {
                        if (buffer[0] == StatusSuccess || buffer[0] == StatusFail)
                        {
                            return true;
                        }
                    }
                }
                finally
                {
                    PurgeInBuffer();
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
                    if (!SendBuffer(commandTest, commandTest.Length))
                    {
                        return null;
                    }

                    SkipOneWireBytes(2);

                    byte[] buffer = new byte[1];
                    if (!ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
                    {
                        return null;
                    }
                    byte readLeader = buffer[0];

                    if (!ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
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
                            if (!ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
                            {
                                return null;
                            }
                            readAnswer[i] = buffer[0];
                        }

                    }

                    if (!ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
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
                    PurgeInBuffer();
                }

                return readAnswer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool CheckCrc()
        {
            try
            {
                try
                {
                    byte[] command = { IdentifierCommand, CommandProgramCheckCRC, (byte)_connectionCRC, (byte)(_connectionCRC >> 8)};
                    if (!SendBuffer(command, command.Length))
                    {
                        return false;
                    }

                    SkipOneWireBytes(4);

                    byte[] buffer = new byte[1];
                    if (!ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
                    {
                        return false;
                    }

                    if (buffer[0] != StatusSuccess)
                    {
                        //return false;
                    }
                }
                finally
                {
                    PurgeInBuffer();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool UploadData(byte targetCommand, bool waitForContinue, byte[] buffer, int bufferLength, int chunkLength)
        {
            try
            {
                try
                {
                    int chunkPosition = 0;
                    int currentChunk = 0;
                    int chunkCount = bufferLength / chunkLength;
                    int chunkLeftover = bufferLength - chunkCount * chunkLength;
                    if (chunkLeftover != 0)
                    {
                        chunkCount++;
                    }

                    byte[] commandStart = { IdentifierCommand, targetCommand };
                    if (!SendBuffer(commandStart, commandStart.Length))
                    {
                        return false;
                    }

                    SkipOneWireBytes(2);

                    byte[] recBuffer = new byte[1];
                    while (currentChunk < chunkCount)
                    {
                        List<byte> chunkData = GetBufferChunk(buffer, ref chunkPosition, chunkLength, bufferLength);
                        if (chunkData == null)
                        {
                            return false;
                        }

                        if (chunkData.Count == 0)
                        {
                            break;
                        }

                        currentChunk++;
                        _failureAddress = chunkPosition;

                        if (!SendBuffer(chunkData.ToArray(), chunkData.Count))
                        {
                            return false;
                        }

                        SkipOneWireBytes(chunkData.Count);

                        if (currentChunk == chunkCount)
                        {
                            if (chunkPosition < bufferLength)
                            {
                                chunkCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (waitForContinue)
                        {
                            if (!ReceiveBuffer(recBuffer, recBuffer.Length, ReadLongTimeout))
                            {
                                return false;
                            }

                            if (recBuffer[0] != IdentifierContinue)
                            {
                                return false;
                            }
                        }
                    }

                    byte[] commandFinish = { IdentifierCommand, CommandProgramFinish };
                    if (!SendBuffer(commandFinish, commandFinish.Length))
                    {
                        return false;
                    }

                    SkipOneWireBytes(2);

                    if (!ReceiveBuffer(recBuffer, recBuffer.Length, ReadLongTimeout))
                    {
                        return false;
                    }

                    if (recBuffer[0] != StatusSuccess)
                    {
                        return false;
                    }

                    return true;
                }
                finally
                {
                    PurgeInBuffer();
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<byte> GetBufferChunk(byte[] buffer, ref int startPosition, int chunkLength, int bufferMaxLength)
        {
            if (startPosition < 0 || startPosition > buffer.Length)
            {
                startPosition = 0;
            }

            if (startPosition >= bufferMaxLength)
            {
                startPosition = bufferMaxLength - 1;
            }

            if (chunkLength + startPosition > bufferMaxLength)
            {
                chunkLength = bufferMaxLength - startPosition;
            }

            List<byte> convertList = new List<byte>();
            for (int i = 0; i < chunkLength; i++)
            {
                if (startPosition >= bufferMaxLength)
                {
                    break;
                }

                byte data = buffer[startPosition];
                if (data == DataIllegalChar || data == IdentifierEscape)
                {
                    convertList.Add(IdentifierEscape);
                    convertList.Add((byte) (data + DataIllegalCharShift));
                }
                else
                {
                    convertList.Add(data);
                }

                startPosition++;
            }

            return convertList;
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

        public static long ReadBufferSizeInfo()
        {
            byte[] readBytes = ReadInfo(ReadBufferSize);
            if (readBytes == null || readBytes.Length != 2)
            {
                return -1;
            }

            long bufferSize = (readBytes[0] << 8) + readBytes[1];
            return bufferSize;
        }

        public static long ReadFlashSizeInfo()
        {
            byte[] readBytes = ReadInfo(ReadFlashSize);
            if (readBytes == null || readBytes.Length != 3)
            {
                return -1;
            }

            long flashSize = (readBytes[0] << 16) + (readBytes[1] << 8) + readBytes[2];
            return flashSize;
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

        public static void ResetCrc()
        {
            _connectionCRC = 0x0000;
        }

        public static void CalculateCrc(byte data)
        {
            for (int i = 0; i < 8; i++)
            {
                if (((data & 0x01) ^ (_connectionCRC & 0x0001)) != 0x0000)
                {
                    _connectionCRC >>= 1;
                    _connectionCRC ^= 0xA001;
                }
                else
                {
                    _connectionCRC >>= 1;
                }

                data >>= 1;
            }
        }

        public static bool WriteFirmware(byte[] buffer, int bufferLength, int chunkLength)
        {
            return UploadData(CommandProgramWrite, true, buffer, bufferLength, chunkLength);
        }

        public static bool VerifyFirmware(byte[] buffer, int bufferLength)
        {
            return UploadData(CommandProgramVerify, false, buffer, bufferLength, VerifyChunkLength);
        }

        public static bool StartFirmware()
        {
            try
            {
                try
                {
                    byte[] command = { IdentifierCommand, CommandProgramStart };
                    if (!SendBuffer(command, command.Length))
                    {
                        return false;
                    }

                    SkipOneWireBytes(2);

                    byte[] buffer = new byte[1];
                    if (ReceiveBuffer(buffer, buffer.Length, ReadTimeout))
                    {
                        if (buffer[0] == StatusBadCommand)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    PurgeInBuffer();
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
