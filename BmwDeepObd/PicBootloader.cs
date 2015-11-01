using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Android.Bluetooth;
using Java.Lang;

namespace BmwDeepObd
{
    public static class PicBootloader
    {
        // ReSharper disable InconsistentNaming
        public const byte STX = 0x0F;
        public const byte ETX = 0x04;
        public const byte DLE = 0x05;
        // ReSharper restore InconsistentNaming
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        private class Crc
        {
            private static readonly ushort[] CrcTable =
            {
                0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7,
                0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef,
                0x1231, 0x0210, 0x3273, 0x2252, 0x52b5, 0x4294, 0x72f7, 0x62d6,
                0x9339, 0x8318, 0xb37b, 0xa35a, 0xd3bd, 0xc39c, 0xf3ff, 0xe3de,
                0x2462, 0x3443, 0x0420, 0x1401, 0x64e6, 0x74c7, 0x44a4, 0x5485,
                0xa56a, 0xb54b, 0x8528, 0x9509, 0xe5ee, 0xf5cf, 0xc5ac, 0xd58d,
                0x3653, 0x2672, 0x1611, 0x0630, 0x76d7, 0x66f6, 0x5695, 0x46b4,
                0xb75b, 0xa77a, 0x9719, 0x8738, 0xf7df, 0xe7fe, 0xd79d, 0xc7bc,
                0x48c4, 0x58e5, 0x6886, 0x78a7, 0x0840, 0x1861, 0x2802, 0x3823,
                0xc9cc, 0xd9ed, 0xe98e, 0xf9af, 0x8948, 0x9969, 0xa90a, 0xb92b,
                0x5af5, 0x4ad4, 0x7ab7, 0x6a96, 0x1a71, 0x0a50, 0x3a33, 0x2a12,
                0xdbfd, 0xcbdc, 0xfbbf, 0xeb9e, 0x9b79, 0x8b58, 0xbb3b, 0xab1a,
                0x6ca6, 0x7c87, 0x4ce4, 0x5cc5, 0x2c22, 0x3c03, 0x0c60, 0x1c41,
                0xedae, 0xfd8f, 0xcdec, 0xddcd, 0xad2a, 0xbd0b, 0x8d68, 0x9d49,
                0x7e97, 0x6eb6, 0x5ed5, 0x4ef4, 0x3e13, 0x2e32, 0x1e51, 0x0e70,
                0xff9f, 0xefbe, 0xdfdd, 0xcffc, 0xbf1b, 0xaf3a, 0x9f59, 0x8f78,
                0x9188, 0x81a9, 0xb1ca, 0xa1eb, 0xd10c, 0xc12d, 0xf14e, 0xe16f,
                0x1080, 0x00a1, 0x30c2, 0x20e3, 0x5004, 0x4025, 0x7046, 0x6067,
                0x83b9, 0x9398, 0xa3fb, 0xb3da, 0xc33d, 0xd31c, 0xe37f, 0xf35e,
                0x02b1, 0x1290, 0x22f3, 0x32d2, 0x4235, 0x5214, 0x6277, 0x7256,
                0xb5ea, 0xa5cb, 0x95a8, 0x8589, 0xf56e, 0xe54f, 0xd52c, 0xc50d,
                0x34e2, 0x24c3, 0x14a0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
                0xa7db, 0xb7fa, 0x8799, 0x97b8, 0xe75f, 0xf77e, 0xc71d, 0xd73c,
                0x26d3, 0x36f2, 0x0691, 0x16b0, 0x6657, 0x7676, 0x4615, 0x5634,
                0xd94c, 0xc96d, 0xf90e, 0xe92f, 0x99c8, 0x89e9, 0xb98a, 0xa9ab,
                0x5844, 0x4865, 0x7806, 0x6827, 0x18c0, 0x08e1, 0x3882, 0x28a3,
                0xcb7d, 0xdb5c, 0xeb3f, 0xfb1e, 0x8bf9, 0x9bd8, 0xabbb, 0xbb9a,
                0x4a75, 0x5a54, 0x6a37, 0x7a16, 0x0af1, 0x1ad0, 0x2ab3, 0x3a92,
                0xfd2e, 0xed0f, 0xdd6c, 0xcd4d, 0xbdaa, 0xad8b, 0x9de8, 0x8dc9,
                0x7c26, 0x6c07, 0x5c64, 0x4c45, 0x3ca2, 0x2c83, 0x1ce0, 0x0cc1,
                0xef1f, 0xff3e, 0xcf5d, 0xdf7c, 0xaf9b, 0xbfba, 0x8fd9, 0x9ff8,
                0x6e17, 0x7e36, 0x4e55, 0x5e74, 0x2e93, 0x3eb2, 0x0ed1, 0x1ef0
            };

            private ushort _crc;

            public Crc(ushort init = 0)
            {
                _crc = init;
            }

            public byte Msb()
            {
                return (byte)(_crc >> 8);
            }

            public byte Lsb()
            {
                return (byte)_crc;
            }

            public ushort Value()
            {
                return _crc;
            }

            public void Add(byte value)
            {
                byte i = (byte)(((_crc >> 8) ^ value) & 0xFF);
                _crc = (ushort)(CrcTable[i] ^ (_crc << 8));
            }
        }

        public class BootPacket
        {
            public enum Commands
            {
                BootloaderInfo = 0x00,
                ReadFlash, ReadFlashCrc, EraseFlash, WriteFlash,
                ReadEeprom, WriteEeprom,
                WriteConfig,
                RunApplication, Reset, SetNonce,
                BulkEraseFlash = 0x0B
            };

            private readonly List<byte> _packetData = new List<byte>();

            public List<byte> PacketData
            {
                get { return _packetData; }
            }

            public int HeaderSize { get; protected set; }
            public int FooterSize { get; protected set; }

            public BootPacket()
            {
                HeaderSize = 7;
                FooterSize = 2;
            }

            private void AppendEscaped(ref List<byte> packet, byte value)
            {
                if (value == STX || value == ETX || value == DLE)
                {
                    packet.Add(DLE);
                }
                packet.Add(value);
            }

            public virtual List<byte> FramePacket()
            {
                List<byte> sendPacket = new List<byte>();
                Crc crc = new Crc();
                sendPacket.Clear();
                sendPacket.Add(STX);
                foreach (byte value in _packetData)
                {
                    AppendEscaped(ref sendPacket, value);
                    crc.Add(value);
                }

                AppendEscaped(ref sendPacket, crc.Lsb());
                AppendEscaped(ref sendPacket, crc.Msb());
                sendPacket.Add(ETX);
                return sendPacket;
            }

            public virtual void SetAddress(uint address)
            {
                while (_packetData.Count < 5)
                {
                    _packetData.Add(0);
                }
                _packetData[1] = (byte) (address & 0xFF);
                _packetData[2] = (byte) ((address & 0xFF00) >> 8);
                _packetData[3] = (byte) ((address & 0xFF0000) >> 16);
                _packetData[4] = (byte) ((address & 0xFF000000) >> 24);
            }
        }

        public class EraseFlashPacket : BootPacket
        {
            public EraseFlashPacket()
            {
                for (int i = 0; i < 6; i++)
                {
                    PacketData.Add(0);
                }
                PacketData[0] = (byte)Commands.EraseFlash;
            }
        }

        public class BootloaderInfoPacket : BootPacket
        {
            public BootloaderInfoPacket()
            {
                PacketData.Add((byte)Commands.BootloaderInfo);
            }

            public override void SetAddress(uint address)
            {
                throw new Exception("setAddress is not supported by ReadBootloaderInfo packets.");
            }
        }

        public class BulkEraseFlashPacket : BootPacket
        {
            public BulkEraseFlashPacket()
            {
                PacketData.Add((byte)Commands.BulkEraseFlash);
            }

            public override void SetAddress(uint address)
            {
                throw new Exception("setAddress is not supported by BulkEraseFlashPacket packets.");
            }
        }

        public class WriteFlashPacket : BootPacket
        {
            public WriteFlashPacket()
            {
                HeaderSize = 6;
                for (int i = 0; i < HeaderSize; i++)
                {
                    PacketData.Add(0);
                }
                PacketData[0] = (byte)Commands.WriteFlash;
            }

            public int PayloadSize()
            {
                return PacketData.Count - HeaderSize;
            }

            public byte Blocks()
            {
                return PacketData[5];
            }

            public byte SetBlocks(byte value)
            {
                return PacketData[5] = value;
            }
        }

        public class RunApplicationPacket : BootPacket
        {
            public RunApplicationPacket()
            {
                PacketData.Add((byte) Commands.RunApplication);
            }

            public override void SetAddress(uint address)
            {
                throw new Exception("setAddress is not supported by RunApplicationPacket packets.");
            }
        }

        public class Comm
        {
            public struct DeviceId
            {
                public uint Id;
                public int Revision;
            };

            public struct BootInfo
            {
                public byte MajorVersion;
                public byte MinorVersion;
                public byte FamilyId;
                public uint CommandMask;
                public uint StartBootloader;
                public uint EndBootloader;
                public uint DeviceIdent;
            };

            // ReSharper disable InconsistentNaming
            public enum ErrorCode
            {
                Success = 0, Aborted, PortDoesNotExist, InvalidSettings, CouldNotTransmit,
                RetryLimitReached, NoAcknowledgement,
                ERROR_GEN_READWRITE, ERROR_READ_TIMEOUT, ERROR_BAD_CHKSUM, ERROR_INVALID_COMMAND,
                ERROR_BLOCK_TOO_SMALL, ERROR_PACKET_TOO_BIG, ERROR_BPA_TOO_SMALL, ERROR_BPA_TOO_BIG,
                JunkInsteadOfSTX
            };
            // ReSharper restore InconsistentNaming

            private const int SyncWaitTime = 100;

            private readonly Stream _bluetoothInStream;
            private readonly Stream _bluetoothOutStream;

            public Comm(Stream bluetoothInStream, Stream bluetoothOutStream)
            {
                _bluetoothInStream = bluetoothInStream;
                _bluetoothOutStream = bluetoothOutStream;
            }

            public void ActivateBootloader()
            {
                byte[] bootCmd = { 0x82, 0xF1, 0xF1, 0xFF, 0xFF, 0x62 };
                _bluetoothOutStream.Write(bootCmd, 0, bootCmd.Length);
            }

            public ErrorCode GetPacket(ref List<byte> receivePacket, int timeout = 1000)
            {
                try
                {
                    int errorCount = 0;
                    Crc crc = new Crc();

                    // Scan for a start condition
                    int junkReceived = 0;
                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        if (!_bluetoothInStream.IsDataAvailable())
                        {
                            if ((ulong)(Stopwatch.GetTimestamp() - startTime) > (ulong)(timeout * TickResolMs))
                            {
                                return ErrorCode.ERROR_READ_TIMEOUT;
                            }
                            Thread.Sleep(10);
                            continue;
                        }

                        int value = _bluetoothInStream.ReadByte();
                        if (value == STX)
                        {
                            break;
                        }

                        junkReceived++;
                        if (junkReceived > 100)
                        {
                            return ErrorCode.JunkInsteadOfSTX;
                        }
                    }

                    // Get the data and unstuff when necessary
                    receivePacket.Clear();
                    startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        if (!_bluetoothInStream.IsDataAvailable())
                        {
                            if ((ulong)(Stopwatch.GetTimestamp() - startTime) > (ulong)(timeout * TickResolMs))
                            {
                                return ErrorCode.ERROR_READ_TIMEOUT;
                            }
                            Thread.Sleep(10);
                            continue;
                        }

                        int value = _bluetoothInStream.ReadByte();
                        if (value == DLE)
                        {
                            if (!_bluetoothInStream.IsDataAvailable())
                            {
                                if ((ulong)(Stopwatch.GetTimestamp() - startTime) > (ulong)(timeout * TickResolMs))
                                {
                                    return ErrorCode.ERROR_READ_TIMEOUT;
                                }
                                Thread.Sleep(10);
                                continue;
                            }
                            value = _bluetoothInStream.ReadByte();
                        }
                        else
                        {
                            if (value == STX)
                            {
                                errorCount++;
                                if (errorCount > 5)
                                {
                                    return ErrorCode.ERROR_READ_TIMEOUT;
                                }
                                receivePacket.Clear();
                                startTime = Stopwatch.GetTimestamp();
                                continue;
                            }

                            if (value == ETX)
                            {
                                if (receivePacket.Count < 2)
                                {
                                    return ErrorCode.ERROR_BAD_CHKSUM;
                                }

                                int size = receivePacket.Count - 2;
                                ushort packetCrc = receivePacket[size + 1];
                                packetCrc <<= 8;
                                packetCrc |= receivePacket[size];
                                // chop off the CRC, nobody above us needs that
                                receivePacket.RemoveAt(receivePacket.Count - 1);
                                receivePacket.RemoveAt(receivePacket.Count - 1);

                                if (crc.Value() != packetCrc)
                                {
                                    return ErrorCode.ERROR_BAD_CHKSUM;
                                }

                                return ErrorCode.Success;
                            }
                        }
                        receivePacket.Add((byte)value);
                        if (receivePacket.Count > 2)
                        {
                            crc.Add(receivePacket[receivePacket.Count - 3]);
                        }
                    }
                }
                catch (System.Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            ErrorCode SendPacket(List<byte> sendPacket)
            {
                try
                {
                    // send the STX
                    _bluetoothOutStream.WriteByte(sendPacket[0]);

                    // wait for the responding STX echoed back
                    long startTime = Stopwatch.GetTimestamp();
                    while (!_bluetoothInStream.IsDataAvailable())
                    {
                        if ((ulong)(Stopwatch.GetTimestamp() - startTime) > (ulong)(SyncWaitTime * 100 * TickResolMs))
                        {
                            return ErrorCode.ERROR_READ_TIMEOUT;
                        }
                        Thread.Sleep(10);
                    }

                    // now we are free to send the rest of the packet
                    List<byte> tempPacket = new List<byte>(sendPacket);
                    tempPacket.RemoveAt(0);
                    _bluetoothOutStream.Write(tempPacket.ToArray(), 0, tempPacket.Count);

                    return ErrorCode.Success;
                }
                catch (System.Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            public BootInfo ReadBootloaderInfo(int timeout = 10)
            {
                BootInfo bootInfo = new BootInfo();

                try
                {
                    while (_bluetoothInStream.IsDataAvailable())
                    {
                        _bluetoothInStream.ReadByte();
                    }
                    _bluetoothOutStream.WriteByte(STX);

                    BootloaderInfoPacket cmd = new BootloaderInfoPacket();
                    List<byte> sendPacket = cmd.FramePacket();

                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        while (!_bluetoothInStream.IsDataAvailable())
                        {
                            if (timeout <= 0)
                            {
                                return bootInfo;
                            }

                            if ((ulong)(Stopwatch.GetTimestamp() - startTime) > (ulong)(SyncWaitTime * TickResolMs))
                            {
                                _bluetoothOutStream.WriteByte(STX);
                                startTime = Stopwatch.GetTimestamp();
                                timeout--;
                            }
                            Thread.Sleep(10);
                        }

                        int value = _bluetoothInStream.ReadByte();
                        if (value == STX)
                        {
                            SendPacket(sendPacket);
                            break;
                        }
                    }
                    List<byte> response = new List<byte>();
                    ErrorCode result = GetPacket(ref response);
                    if (result != ErrorCode.Success)
                    {
                        return bootInfo;
                    }
                    if (response.Count < 10)
                    {
                        return bootInfo;
                    }
                    bootInfo.MinorVersion = response[3];
                    bootInfo.MajorVersion = response[2];
                    bootInfo.FamilyId = (byte)(response[5] & 0x0F);
                    bootInfo.CommandMask = (uint)(response[4] | ((response[5] & 0xF0) << 4));
                    bootInfo.StartBootloader = (uint)((response[6] & 0xFF) |
                                                ((response[7] & 0xFF) << 8) |
                                                ((response[8] & 0xFF) << 16) |
                                                ((response[9] & 0xFF) << 24));
                    bootInfo.EndBootloader  = bootInfo.StartBootloader;
                    bootInfo.EndBootloader += response[0];
                    bootInfo.EndBootloader += (uint)(response[1] << 8);
                    if (response.Count > 10)
                    {
                        bootInfo.DeviceIdent  = response[10];
                        bootInfo.DeviceIdent += (uint)(response[11] << 8);
                    }
                }
                catch (System.Exception)
                {
                    // ignored
                }
                return bootInfo;
            }

            public ErrorCode RunApplication()
            {
                try
                {
                    RunApplicationPacket cmd = new RunApplicationPacket();
                    List<byte> sendPacket = cmd.FramePacket();
                    ErrorCode result = SendPacket(sendPacket);
                    if (result != ErrorCode.Success)
                    {
                        return result;
                    }

                    if (!_bluetoothInStream.IsDataAvailable())
                    {
                        return ErrorCode.NoAcknowledgement;
                    }
                    int response = _bluetoothInStream.ReadByte();
                    if (response != STX)
                    {
                        return ErrorCode.NoAcknowledgement;
                    }
                    return ErrorCode.Success;
                }
                catch (System.Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            public bool EnterBootloaderMode()
            {
                try
                {
                    BootInfo bootInfo = ReadBootloaderInfo(20);
                    if (bootInfo.MajorVersion == 0 && bootInfo.MinorVersion == 0)
                    {
                        ActivateBootloader();
                        Thread.Sleep(600);
                        bootInfo = ReadBootloaderInfo();
                    }
                    if (bootInfo.MajorVersion == 0 && bootInfo.MinorVersion == 0)
                    {
                        return false;
                    }
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

        public static bool FwUpdate(BluetoothSocket bluetoothSocket)
        {
            Comm comm = new Comm(bluetoothSocket.InputStream, bluetoothSocket.OutputStream);
            if (!comm.EnterBootloaderMode())
            {
                return false;
            }
            Comm.ErrorCode errorCode = comm.RunApplication();
            if (errorCode != Comm.ErrorCode.Success)
            {
                return false;
            }
            return true;
        }
    }
}
