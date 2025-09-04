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
    public static class PicBootloader
    {
        // ReSharper disable InconsistentNaming
        public const byte STX = 0x0F;
        public const byte ETX = 0x04;
        public const byte DLE = 0x05;
        // ReSharper restore InconsistentNaming
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static List<FirmwareDetail> _firmwareList;

        private static readonly FirmwareInfo[] FirmwareInfos =
        {
            new FirmwareInfo("Type2.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type2.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type3.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type3.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type4.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type4.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type5.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type5.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type6.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type6.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type7.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type7.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type8.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type8.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type9.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18),
            new FirmwareInfo("Type9.ELM327V23.X.production.hex", 0x030C, Device.Families.PIC18, true),
            new FirmwareInfo("Type16.CanAdapterElm.X.production.hex", 0x030C, Device.Families.PIC18)
        };

        private class FirmwareInfo
        {
            public FirmwareInfo(string fileName, uint deviceId, Device.Families familyId, bool elmFirmware = false)
            {
                FileName = fileName;
                DeviceId = deviceId;
                FamilyId = familyId;
                ElmFirmware = elmFirmware;
            }

            public string FileName { get; }
            public uint DeviceId { get; }
            public Device.Families FamilyId { get; }
            public bool ElmFirmware { get; }
        }

        private class FirmwareDetail
        {
            public FirmwareDetail(FirmwareInfo firmwareInfo, uint adapterType, uint adapterVersion, bool elmFirmware)
            {
                FirmwareInfo = firmwareInfo;
                AdapterType = adapterType;
                AdapterVersion = adapterVersion;
                ElmFirmware = elmFirmware;
            }

            public FirmwareInfo FirmwareInfo { get; }
            public uint AdapterType { get; }
            public uint AdapterVersion { get; }
            public bool ElmFirmware { get; }
        }

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

        public class DeviceData
        {
            private readonly Device _device;

            // ReSharper disable InconsistentNaming
            public const int MAX_MEM = 0x8000;
            public const int MAX_EE = 0x400;
            public const int MAX_CFG = 0x10;
            public const int MAX_UID = 0x08;
            // ReSharper restore InconsistentNaming

            public DeviceData(Device newDevice)
            {
                _device = newDevice;
            }

            /*!
            * FLASH program memory.
            */
            public uint[] ProgramMemory = new uint[MAX_MEM];
            public uint[] EePromMemory = new uint[MAX_EE];
            public uint[] ConfigWords = new uint[MAX_CFG];
            public uint[] UserIDs = new uint[MAX_UID];
            public uint Osccal;
            public uint BandGap;

            public byte[,] Mac;

            public bool Encrypted;
            public uint Nonce;

            public void ClearAllData()
            {
                ClearProgramMemory(_device.BlankValue);
                ClearEePromMemory(_device.BlankValue);
                //ClearConfigWords();

                //ClearUserIDs(numIDs, idBytes, memBlankVal);
                //OSCCAL = OSCCALInit | 0xFF;
                BandGap = _device.BlankValue;
            }

            public void ClearProgramMemory(uint memBlankVal)
            {
                for (uint i = 0; i < ProgramMemory.Length; i++)
                {
                    ProgramMemory[i] = memBlankVal & _device.FlashWordMask;
                }
            }

            public void CopyProgramMemory(uint[] memory)
            {
                Array.Copy(ProgramMemory, memory, ProgramMemory.Length);
            }

            public void ClearEePromMemory(uint memBlankVal)
            {
                //init eeprom to blank
                for (uint i = 0; i < EePromMemory.Length; i++)
                {
                    EePromMemory[i] = memBlankVal; // 8-bit eeprom will just use 8 LSBs
                }
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

            public List<byte> PacketData => _packetData;

            public const int HeaderSize = 7;
            public const int FooterSize = 2;

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

            public void SetBlocks(byte blocks)
            {
                PacketData[5] = blocks;
            }
        }

        public class ReadFlashPacket : BootPacket
        {
            public ReadFlashPacket()
            {
                for (int i = 0; i < 7; i++)
                {
                    PacketData.Add(0);
                }
                PacketData[0] = (byte)Commands.ReadFlash;
            }

            public void SetBytes(ushort blocks)
            {
                PacketData[5] = (byte)(blocks & 0xFF);
                PacketData[6] = (byte)((blocks >> 8) & 0xFF);
            }
        }

        public class ReadFlashCrcPacket : BootPacket
        {
            public ReadFlashCrcPacket()
            {
                for (int i = 0; i < 7; i++)
                {
                    PacketData.Add(0);
                }
                PacketData[0] = (byte)Commands.ReadFlashCrc;
            }

            public void SetBlocks(ushort blocks)
            {
                PacketData[5] = (byte)(blocks & 0xFF);
                PacketData[6] = (byte)((blocks >> 8) & 0xFF);
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
            public new const int HeaderSize = 6;

            public WriteFlashPacket()
            {
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

        public class Device
        {
            // ReSharper disable InconsistentNaming
            public enum Families
            {
                Unknown = 0, Baseline = 1, PIC16, PIC17, PIC18, PIC24, dsPIC30, dsPIC33, PIC32
            };
            // ReSharper restore InconsistentNaming

            public int Id;
            public Families Family;
            public uint CommandMask;
            public uint AdapterType;
            public string Name;

            public int BytesPerWordEeprom;
            public int BytesPerWordFlash;
            public int BytesPerAddressFlash;   // number of bytes per address
            public uint EraseBlockSizeFlash;    // number of bytes erased at a time
            public int WriteBlockSizeFlash;    // number of bytes written at a time
            public uint FlashWordMask;
            public uint BlankValue;

            public uint StartFlash;
            public uint EndFlash;

            public uint StartBootloader;
            public uint EndBootloader;

            public uint StartDeviceId;
            public uint EndDeviceId;
            public uint DeviceIdMask;
            public uint ConfigWordMask;

            public uint StartEeprom;
            public uint EndEeprom;

            public uint StartUser;
            public uint EndUser;

            public uint StartConfig;
            public uint EndConfig;

            public uint StartGpr;
            public uint EndGpr;

            public class MemoryRange
            {
                public uint Start;
                public uint End;

                public MemoryRange Clone()
                {
                    return ((MemoryRange) MemberwiseClone());
                }
            };

            public Device()
            {
                Id = 0;
                Family = Families.Unknown;

                SetUnknown();
            }

            public void SetUnknown()
            {
                BytesPerWordEeprom = 1;
                BlankValue = 0xFFFFFFFF;
                FlashWordMask = 0xFFFFFFFF;
                ConfigWordMask = 0xFFFFFFFF;

                Name = "";
                BytesPerAddressFlash = 1;
                WriteBlockSizeFlash = 64;
                EraseBlockSizeFlash = 1024;
                StartFlash = 0;
                EndFlash = 0;
                StartBootloader = 0;
                EndBootloader = 0;
                StartEeprom = 0;
                EndEeprom = 0;
                StartUser = 0;
                EndUser = 0;
                StartConfig = 0;
                EndConfig = 0;
                StartGpr = 0;
                EndGpr = 0;

                BytesPerWordFlash = 2;
            }

            public bool HasEeprom()
            {
                if (EndEeprom > 0)
                {
                    return true;
                }
                return false;
            }

            public bool HasUserMemory()
            {
                if (EndUser > 0)
                {
                    return true;
                }
                return false;
            }

            public bool HasConfigAsFlash()
            {
                if (Family == Families.PIC16)
                {
                    return false;
                }

                if (EndConfig > 0)
                {
                    if (StartConfig < EndFlash && EndConfig <= EndFlash)
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool HasConfigAsFuses()
            {
                if (Family == Families.PIC16)
                {
                    return false;
                }

                if (EndConfig > 0)
                {
                    if (StartConfig >= EndFlash && EndConfig >= EndFlash)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool HasConfig()
            {
                return HasConfigAsFuses() || HasConfigAsFlash();
            }

            public bool HasConfigReadCommand()
            {
                if (HasConfigAsFuses() && Family != Families.PIC16)
                {
                    return true;
                }

                return false;
            }

            public bool HasEraseFlashCommand()
            {
                if (Family == Families.PIC16)
                {
                    if ((CommandMask & 1) == 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool HasEncryption()
            {
                if ((CommandMask & 0x100) != 0)
                {
                    return true;
                }

                return false;
            }

            public int MaxPacketSize()
            {
                if (Family == Families.PIC16)
                {
                    // PIC16F bootloader firmware does a dummy write of the ETX character as part
                    // of the footer, so we don't actually get to use all 96 bytes of the bank 0 GPR area.

                    // Bit Bang serial bootloaders need 5 bytes of GPR memory, so again, we lose some space.
                    return 96 - 6;
                }
                else if (Family == Families.PIC24 || Family == Families.dsPIC30 || Family == Families.dsPIC33)
                {
                    if ((EndGpr - StartGpr) > 0xFFFF)
                    {
                        return 0x7000;
                    }

                    return (int) ((EndGpr - StartGpr) - 1024);
                }
                else if (Family == Families.PIC32)
                {
                    // Calculate max packet size based on GPR address range, subtracting off 2K bytes for
                    // packet headers and bootloader static variable space. This value of 2K bytes might
                    // be smaller in reality.
                    return (int) ((EndGpr - StartGpr) - 2048);
                }
                else
                {
                    int overhead = 16;
                    if (HasEncryption())
                    {
                        overhead += 7 + 16 + 16 + 4;
                    }

                    // Calculate max packet size based on GPR address range, subtracting off 16 bytes for
                    // packet headers and bootloader static variable space. This value of 16 bytes might
                    // be smaller in reality.
                    return (int) ((EndGpr - StartGpr) - overhead);
                }
            }

            public uint FlashPointer(uint address)
            {
                switch(Family)
                {
                    case Families.PIC16:
                        return address;

                    case Families.PIC32:
                        return address - StartFlash >> 2;

                    default:
                    case Families.PIC24:
                    case Families.dsPIC30:
                    case Families.dsPIC33:
                    case Families.PIC18:
                        return address >> 1;
                }
            }

            public void IncrementFlashAddressByInstructionWord(ref uint address)
            {
                switch (Family)
                {
                    case Families.PIC16:
                        address++;
                        break;

                    default:
                    case Families.PIC24:
                    case Families.dsPIC30:
                    case Families.dsPIC33:
                    case Families.PIC18:
                        address += 2;
                        break;

                    case Families.PIC32:
                        address += 4;
                        break;
                }
            }

            public void IncrementFlashAddressByBytes(ref int address, uint bytes)
            {
                switch (Family)
                {
                    case Families.dsPIC30:
                    case Families.dsPIC33:
                    case Families.PIC24:
                        address += (int)((bytes/BytesPerWordFlash)*2);
                        break;

                    default:
                        address += (int)(bytes / BytesPerAddressFlash);
                        break;
                }
            }

            public uint FlashBytes(uint startAddress, uint endAddress)
            {
                switch (Family)
                {
                    case Families.PIC16:
                    case Families.PIC18:
                    default:
                        return (uint)((endAddress - startAddress)*BytesPerAddressFlash);

                    case Families.dsPIC30:
                    case Families.dsPIC33:
                    case Families.PIC24:
                        return ((endAddress - startAddress)/2)*3;

                    case Families.PIC32:
                        return (endAddress - startAddress);
                }
            }

            public uint FromHexAddress(uint hexAddress, out bool error)
            {
                uint flashAddress = (uint) (hexAddress/BytesPerAddressFlash);
                uint eepromAddress;
                uint configAddress;
                uint userAddress;

                switch (Family)
                {
                    case Families.PIC16:
                        eepromAddress = hexAddress - StartFlash;
                        configAddress = hexAddress - StartFlash;
                        userAddress = hexAddress - StartFlash;
                        break;

                    default:
                        eepromAddress = hexAddress;
                        configAddress = hexAddress;
                        userAddress = hexAddress;
                        break;
                }

                if (flashAddress >= StartFlash && flashAddress <= EndFlash)
                {
                    error = false;
                    return flashAddress;
                }

                if (eepromAddress >= StartEeprom && eepromAddress <= EndEeprom)
                {
                    error = false;
                    return eepromAddress;
                }

                if (configAddress >= StartConfig && configAddress <= EndConfig)
                {
                    error = false;
                    return configAddress;
                }

                if (userAddress >= StartUser && userAddress <= EndUser)
                {
                    error = false;
                    return userAddress;
                }

                error = true;
                return 0;
            }
        }

        public class DeviceWritePlanner
        {
            private readonly Device _device;

            public bool WriteConfig;

            public DeviceWritePlanner(Device newDevice)
            {
                _device = newDevice;
            }

            public void PlanFlashWrite(ref List<Device.MemoryRange> eraseList,
                                        ref List<Device.MemoryRange> writeList,
                                        uint start, uint end,
                                        uint[] data)
            {
                uint address = start;
                eraseList.Clear();
                writeList.Clear();

                while (address < end)
                {
                    Device.MemoryRange block = new Device.MemoryRange();
                    address = SkipEmptyFlashPages(address, data);
                    block.Start = address;
                    if (address >= end)
                    {
                        break;
                    }

                    address = FindEndFlashWrite(address, data);
                    if (address >= end)
                    {
                        address = end;
                    }
                    block.End = address;

                    if (_device.Family == Device.Families.PIC16 && !_device.HasEraseFlashCommand())
                    {
                        // Certain PIC16 devices (such as PIC16F882) have a peculiar automatic erase
                        // during write feature. To make that work, writes must be expanded to align
                        // with Erase Block boundaries.
                        block.Start -= (block.Start%_device.EraseBlockSizeFlash);
                        if ((block.End%_device.EraseBlockSizeFlash) != 0)
                        {
                            block.End += _device.EraseBlockSizeFlash - (block.End%_device.EraseBlockSizeFlash);
                            address = block.End;
                        }
                    }
                    writeList.Add(block);

                    address++;
                }

                if (_device.Family == Device.Families.PIC32)
                {
                    // Because PIC32 has Bulk Erase available for bootloader use,
                    // it's faster to simply erase the entire FLASH memory space
                    // than erasing specific erase blocks using an erase plan.
                    Device.MemoryRange block = new Device.MemoryRange
                    {
                        Start = _device.StartFlash,
                        End = _device.EndFlash
                    };
                    eraseList.Add(block);
                }
                else
                {
                    if (writeList.Count > 0)
                    {
                        if (_device.HasEraseFlashCommand())
                        {
                            FlashEraseList(ref eraseList, writeList, data);
                        }

                        EraseAppCheckFirst(ref eraseList);
                        WriteAppCheckLast(ref writeList);

                        if (WriteConfig)
                        {
                            EraseConfigPageLast(ref eraseList);
                            WriteConfigPageFirst(ref writeList);
                            DoNotEraseBootBlock(ref eraseList); // needed in case boot block resides on config page
                        }
                        else
                        {
                            DoNotEraseConfigPage(ref eraseList);
                        }

                        DoNotEraseInterruptVectorTable(ref eraseList);
                    }
                }

                PacketSizeWriteList(ref writeList);
            }

            public void PlanFlashErase(ref List<Device.MemoryRange> eraseList)
            {
                Device.MemoryRange block = new Device.MemoryRange
                {
                    Start = _device.StartFlash,
                    End = _device.EndFlash
                };

                eraseList.Clear();
                eraseList.Add(block);
                if (_device.Family == Device.Families.PIC32)
                {
                    return;
                }

                if (WriteConfig)
                {
                    EraseConfigPageLast(ref eraseList);
                }
                else
                {
                    DoNotEraseConfigPage(ref eraseList);
                }

                DoNotEraseInterruptVectorTable(ref eraseList);

                DoNotEraseBootBlock(ref eraseList);
            }

            public void FlashEraseList(ref List<Device.MemoryRange> eraseList, List<Device.MemoryRange> writeList, uint[] data)
            {
                int pages = (int)(_device.EndFlash/_device.EraseBlockSizeFlash);
                bool[] flashPageErased = new bool[pages + 1];

                for (int i = 0; i < flashPageErased.Length; i++)
                {
                    flashPageErased[i] = false;
                }

                foreach (Device.MemoryRange range in writeList)
                {
                    int pageStart = (int)(range.Start / _device.EraseBlockSizeFlash);
                    int pageEnd = (int)(range.End / _device.EraseBlockSizeFlash);
                    if ((range.End % _device.EraseBlockSizeFlash) != 0)
                    {
                        pageEnd++;
                    }
                    int eraseStart = -1;
                    int eraseEnd = -1;
                    for (int i = pageStart; i < pageEnd; i++)
                    {
                        if (flashPageErased[i] == false)
                        {
                            if (eraseStart == -1)
                            {
                                eraseStart = (int)(i*_device.EraseBlockSizeFlash);
                            }
                            eraseEnd = (int)((i + 1)*_device.EraseBlockSizeFlash);
                            flashPageErased[i] = true;
                        }
                    }
                    if (eraseStart != -1)
                    {
                        Device.MemoryRange block = new Device.MemoryRange
                        {
                            Start = (uint)eraseStart,
                            End = (uint)eraseEnd
                        };
                        eraseList.Insert(0, block);
                    }
                }
            }

            public void PacketSizeWriteList(ref List<Device.MemoryRange> writeList)
            {
                int maxWritePacketData = _device.MaxPacketSize();
                int bytesPerWriteBlock = _device.WriteBlockSizeFlash;

                maxWritePacketData -= WriteFlashPacket.HeaderSize;
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                maxWritePacketData -= WriteFlashPacket.FooterSize;

                if (_device.Family == Device.Families.PIC16)
                {
                    // On PIC12/PIC16, writeBlockSizeFLASH only counts the number of instruction words per block.
                    // Each instruction word requires two bytes of data to be transmitted in the command packet,
                    // so we need to double bytesPerWriteBlock here.
                    bytesPerWriteBlock <<= 1;
                }
                else if (_device.Family == Device.Families.PIC24)
                {
                    // On PIC24, writeBlockSizeFLASH only counts the two least significant bytes of the
                    // instruction word, so we need to add 1/2 more here to count that special third byte.
                    bytesPerWriteBlock = _device.WriteBlockSizeFlash + (_device.WriteBlockSizeFlash >> 1);
                }

                if (_device.HasEncryption())
                {
                    // each write block must be followed by 16 bytes of message authentication code (MAC) data
                    bytesPerWriteBlock += 16;
                }

                if (maxWritePacketData/_device.WriteBlockSizeFlash >= 256)
                {
                    // Do not allow write transactions greater than 256 blocks.
                    // AN1310 protocol only provides an 8-bit block count for Write FLASH commands.
                    maxWritePacketData = bytesPerWriteBlock*256;
                }

                int i = 0;
                while (i < writeList.Count)
                {
                    Device.MemoryRange range = writeList[i];
                    int blocks = (int)((range.End - range.Start) / _device.WriteBlockSizeFlash);
                    int bytes = blocks*bytesPerWriteBlock;

                    if (bytes > maxWritePacketData)
                    {
                        // this write is too big, split it in half and then try again
                        Device.MemoryRange firstHalf = new Device.MemoryRange
                        {
                            Start = range.Start,
                            End = (uint) ((maxWritePacketData/bytesPerWriteBlock)*_device.WriteBlockSizeFlash)
                        };
                        // Round off packet size to nearest FLASH write block size.
                        firstHalf.End -= (uint)(firstHalf.End%_device.WriteBlockSizeFlash);
                        firstHalf.End += firstHalf.Start;
                        range.Start = firstHalf.End;
                        writeList.Insert(i, firstHalf);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            public void EraseConfigPageLast(ref List<Device.MemoryRange> eraseList)
            {
                if (DoNotEraseConfigPage(ref eraseList))
                {
                    // ABORT: this device does not store config bits in FLASH or
                    // the existing eraseList does not intend to erase the config bits page anyway.
                    return;
                }

                // make config page erase very last transaction
                Device.MemoryRange configErase = new Device.MemoryRange
                {
                    Start = _device.EndFlash - _device.EraseBlockSizeFlash,
                    End = _device.EndFlash
                };
                eraseList.Add(configErase);
            }

            public bool DoNotEraseConfigPage(ref List<Device.MemoryRange> eraseList)
            {
                if (!_device.HasConfigAsFlash())
                {
                    // ABORT: this device does not store config bits in FLASH, so no worries...
                    return true;
                }

                Device.MemoryRange firstRange = eraseList[0];
                if (firstRange.End <= _device.EndFlash - _device.EraseBlockSizeFlash)
                {
                    // ABORT: not planning to erase config bit page anyway, nothing to do here.
                    return true;
                }

                firstRange.End -= _device.EraseBlockSizeFlash;
                if (firstRange.End <= firstRange.Start)
                {
                    // after taking out the config page, this write transaction has nothing left in it.
                    eraseList.RemoveAt(0);
                }

                return false;
            }

            public bool DoNotEraseInterruptVectorTable(ref List<Device.MemoryRange> eraseList)
            {
                if (_device.Family != Device.Families.PIC24)
                {
                    // ABORT: this device does not have a fixed interrupt vector table in FLASH, so no worries...
                    return true;
                }

                Device.MemoryRange firstRange = eraseList[0];
                uint endIvt = 0x200;
                if (endIvt < _device.EraseBlockSizeFlash)
                {
                    endIvt = _device.EraseBlockSizeFlash;
                }

                if (firstRange.Start >= endIvt)
                {
                    // ABORT: not planning to erase IVT anyway, nothing to do here.
                    return true;
                }

                firstRange.Start = endIvt;
                if (firstRange.End <= firstRange.Start)
                {
                    // after taking out the IVT page, this write transaction has nothing left in it.
                    eraseList.RemoveAt(0);
                }

                return false;
            }

            public void DoNotEraseBootBlock(ref List<Device.MemoryRange> eraseList)
            {
                int i = 0;
                while (i < eraseList.Count)
                {
                    Device.MemoryRange range = eraseList[i];
                    //         v Boot Block area
                    // 1. S  E   |        <- Erase transaction is before the boot block, no problem
                    // 2.    |   S  E     <- Erase transaction is after the boot block,  no problem
                    // 3.    S   E        <- Erase transaction exactly matches the boot block
                    // 4.    S   |  E     <- Erase transaction starts on the boot block and extends past the end of it
                    // 5. S  |   E        <- Erase transaction starts before the boot block, but doesn't end before hitting it
                    // 6. S  |   |  E     <- Erase transaction starts before and ends after the boot block
                    if (!((range.Start <= _device.StartBootloader && range.End <= _device.StartBootloader) || // 1. S  E   |
                          (range.Start >= _device.EndBootloader && range.End >= _device.EndBootloader))) // 2.    |   S  E
                    {
                        if (range.Start == _device.StartBootloader && range.End == _device.EndBootloader)
                        {
                            // 3.    S   E        <- Erase transaction exactly matches the boot block
                            eraseList.Remove(range); // delete the transaction and continue checking erase plan
                            continue;
                        }
                        if (range.Start == _device.StartBootloader)
                        {
                            // 4.    S   |  E     <- Erase transaction starts on the boot block and extends past the end of it
                            // modify transaction to only erase the data after the boot block
                            range.Start = _device.EndBootloader;
                        }
                        else if (range.End == _device.EndBootloader)
                        {
                            // 5. S  |   E        <- Erase transaction starts before the boot block, but doesn't end before hitting it
                            // modify transaction to only erase data up to the boot block
                            range.End = _device.StartBootloader;
                        }
                        else
                        {
                            // 6. S  |   |  E     <- Erase transaction starts before and ends after the boot block
                            // split the transaction up into two transactions.
                            // the first transaction will erase data after the boot block
                            Device.MemoryRange firstHalf = new Device.MemoryRange
                            {
                                Start = _device.EndBootloader,
                                End = range.End
                            };
                            if (firstHalf.Start < firstHalf.End)
                            {
                                eraseList.Insert(i, firstHalf);
                            }

                            // the other transaction will erase data before the boot block
                            range.End = _device.StartBootloader;
                        }
                    }
                    i++;
                }
            }

            public void WriteConfigPageFirst(ref List<Device.MemoryRange> writeList)
            {
                if (!_device.HasConfigAsFlash())
                {
                    // ABORT: this device does not store config bits in FLASH, so no worries...
                    return;
                }

                Device.MemoryRange rangeLast = writeList[writeList.Count - 1];
                if (rangeLast.End < _device.EndFlash - _device.WriteBlockSizeFlash)
                {
                    // ABORT: user is not planning to write to config bit page.
                    return;
                }

                rangeLast.End -= (uint)_device.WriteBlockSizeFlash;
                if (rangeLast.End <= rangeLast.Start)
                {
                    // after taking out the config page, this write transaction has nothing left in it.
                    writeList.RemoveAt(writeList.Count - 1);
                }

                Device.MemoryRange configWrite = new Device.MemoryRange
                {
                    Start = (uint) (_device.EndFlash - _device.WriteBlockSizeFlash),
                    End = _device.EndFlash
                };
                writeList.Insert(0, configWrite);
            }

            public uint FindEndFlashWrite(uint address, uint[] data)
            {
                while (address < _device.EndFlash)
                {
                    uint checkAddress = SkipEmptyFlashPages(address, data);
                    if (checkAddress != address)
                    {
                        // the next page is empty, we've reached the end of the area we want to write
                        return address;
                    }

                    address += (uint)_device.WriteBlockSizeFlash;
                }

                return _device.EndFlash;
            }

            public uint SkipBootloaderFlashPages(uint address)
            {
                if (address >= _device.StartBootloader && address < _device.EndBootloader)
                {
                    return _device.EndBootloader;
                }

                return address;
            }

            public uint SkipEmptyFlashPages(uint address, uint[] data)
            {
                uint readData = _device.FlashPointer(address);
                while (address < _device.EndFlash)
                {
                    uint word = data[readData++];
                    if ((word & _device.FlashWordMask) != (_device.BlankValue & _device.FlashWordMask))
                    {
                        // We found some non-empty data here. Make sure it's not the bootloader.
                        uint addressSkip = SkipBootloaderFlashPages(address);
                        if (addressSkip != address)
                        {
                            // skip over the bootloader and continue
                            address = addressSkip;
                            readData = _device.FlashPointer(address);
                            continue;
                        }
                        else
                        {
                            // align address to FLASH write page and return result.
                            address = (uint)(address - (address%_device.WriteBlockSizeFlash));
                            return address;
                        }
                    }

                    _device.IncrementFlashAddressByInstructionWord(ref address);
                }

                // couldn't find any data, return end of flash memory
                return _device.EndFlash;
            }

            public void EraseAppCheckFirst(ref List<Device.MemoryRange> eraseList)
            {
                if (_device.Family != Device.Families.PIC24)
                {
                    // ABORT: only applies to PIC24
                    return;
                }

                Device.MemoryRange range = eraseList[eraseList.Count - 1];
                if (range.Start >= _device.EndBootloader + _device.EraseBlockSizeFlash)
                {
                    // ABORT: user is not planning to erase the application check page.
                    return;
                }

                range.Start += _device.EraseBlockSizeFlash;
                if (range.Start >= range.End)
                {
                    // after taking out the app check page, this erase transaction has nothing left in it.
                    eraseList.RemoveAt(eraseList.Count - 1);
                }

                Device.MemoryRange appErase = new Device.MemoryRange
                {
                    Start = _device.EndBootloader,
                    End = _device.EndBootloader + _device.EraseBlockSizeFlash
                };
                eraseList.Insert(0, appErase);
            }

            public void WriteAppCheckLast(ref List<Device.MemoryRange> writeList)
            {
                if (_device.Family != Device.Families.PIC24)
                {
                    // ABORT: only applies to PIC24
                    return;
                }

                Device.MemoryRange firstRange = writeList[0];
                if (firstRange.Start >= _device.EndBootloader + _device.WriteBlockSizeFlash)
                {
                    // ABORT: not planning to write Application Check row anyway, nothing to do here.
                    return;
                }

                firstRange.Start += (uint)_device.WriteBlockSizeFlash;
                if (firstRange.Start >= firstRange.End)
                {
                    // after taking out the app check row, this write transaction has nothing left in it.
                    writeList.RemoveAt(0);
                }

                // make app check row write very last transaction
                Device.MemoryRange appWrite = new Device.MemoryRange
                {
                    Start = _device.EndBootloader,
                    End = (uint) (_device.EndBootloader + _device.WriteBlockSizeFlash)
                };
                writeList.Add(appWrite);
            }
        }

        public class DeviceVerifyPlanner
        {
            private readonly Device _device;
            public bool WriteConfig;
            public int MaxBlockCount;

            public DeviceVerifyPlanner(Device newDevice)
            {
                _device = newDevice;
                MaxBlockCount = -1;
            }

            public void PlanFlashVerify(ref List<Device.MemoryRange> verifyList, int start, int end)
            {
                Device.MemoryRange block = new Device.MemoryRange
                {
                    Start = (uint) start,
                    End = (uint) end
                };

                verifyList.Clear();
                verifyList.Add(block);

                if (!WriteConfig)
                {
                    DoNotVerifyConfigPage(ref verifyList);
                }

                DoNotVerifyBootBlock(ref verifyList);
                LimitVerifyBlockSize(ref verifyList);
            }

            public void DoNotVerifyConfigPage(ref List<Device.MemoryRange> verifyList)
            {
                if (!_device.HasConfigAsFlash())
                {
                    // ABORT: this device does not store config bits in FLASH, so no worries...
                    return;
                }

                Device.MemoryRange rangeFirst = verifyList[0];
                if (rangeFirst.End <= _device.EndFlash - _device.EraseBlockSizeFlash)
                {
                    // ABORT: user is not planning to verify the config bit page.
                    return;
                }

                rangeFirst.End -= _device.EraseBlockSizeFlash;
                if (rangeFirst.End <= rangeFirst.Start)
                {
                    // after taking out the config page, this transaction has nothing left in it.
                    verifyList.RemoveAt(0);
                }
            }

            public void DoNotVerifyBootBlock(ref List<Device.MemoryRange> verifyList)
            {
                int i = 0;
                while (i < verifyList.Count)
                {
                    Device.MemoryRange range = verifyList[i];
                    //     S   E
                    // S   |   E
                    //     S   |  E
                    // S   |   |  E
                    //
                    //     |   S  E
                    // S   E   |
                    if (!((range.Start <= _device.StartBootloader && range.End <= _device.StartBootloader) ||
                          (range.Start >= _device.EndBootloader && range.End >= _device.EndBootloader)))
                    {
                        // This transaction would verify over bootloader memory, which may fail if
                        // we haven't (or can't) read the device out.
                        if (range.Start == _device.StartBootloader && range.End == _device.EndBootloader)
                        {
                            verifyList.RemoveAt(i);
                            continue;
                        }
                        if (range.Start == _device.StartBootloader)
                        {
                            range.Start = _device.EndBootloader;
                        }
                        else if (range.End == _device.EndBootloader)
                        {
                            range.End = _device.StartBootloader;
                        }
                        else
                        {
                            Device.MemoryRange firstHalf = new Device.MemoryRange
                            {
                                Start = _device.EndBootloader,
                                End = range.End
                            };
                            range.End = _device.StartBootloader;
                            if (firstHalf.Start < firstHalf.End)
                            {
                                verifyList.Insert(i, firstHalf);
                            }
                        }
                    }
                    i++;
                }
            }

            // [UH] limit block count to prevent packet loss
            public void LimitVerifyBlockSize(ref List<Device.MemoryRange> verifyList)
            {
                if (MaxBlockCount <= 0)
                {
                    return;
                }
                int i = 0;
                while (i < verifyList.Count)
                {
                    Device.MemoryRange range = verifyList[i];
                    uint blockCount = (range.End - range.Start)/_device.EraseBlockSizeFlash;
                    if (blockCount > MaxBlockCount)
                    {
                        Device.MemoryRange firstHalf = new Device.MemoryRange
                        {
                            Start = range.Start,
                            End = (uint) (range.Start + (MaxBlockCount * _device.EraseBlockSizeFlash))
                        };
                        range.Start = firstHalf.End;
                        verifyList.Insert(i, firstHalf);
                    }
                    i++;
                }
            }
        }

        public class DeviceWriter
        {
            private readonly Comm _comm;
            private readonly Device _device;
            private readonly DeviceWritePlanner _writePlan;
            private bool _abortOperation;
            public bool WriteConfig;

            public DeviceWriter(Device newDevice, Comm newComm)
            {
                _device = newDevice;
                _comm = newComm;
                _writePlan = new DeviceWritePlanner(newDevice);
                _abortOperation = false;
            }

            public void AbortOperation()
            {
                _abortOperation = true;
            }

            public Comm.ErrorCode EraseFlash(List<Device.MemoryRange> eraseList)
            {
                _abortOperation = false;

                for (int i = 0; i < eraseList.Count;)
                {
                    Device.MemoryRange block = eraseList[i++];

                    // Re-consolidate consecutive erase address blocks into one request.
                    while (i < eraseList.Count)
                    {
                        uint address = eraseList[i].Start;
                        if (address != block.End)
                        {
                            break;
                        }

                        block.End = eraseList[i].End;
                        i++;
                    }

                    Comm.ErrorCode result = EraseFlash((int)block.Start, (int)block.End);
                    if (result != Comm.ErrorCode.Success)
                    {
                        return result;
                    }

                    if (_abortOperation)
                    {
                        return Comm.ErrorCode.Aborted;
                    }
                }

                return Comm.ErrorCode.Success;
            }

            public Comm.ErrorCode EraseFlash(int startAddress, int endAddress)
            {
                List<byte> sendPacket;
                List<byte> receivePacket = new List<byte>();
                int address;
                int blocks;
                Comm.ErrorCode result;
                int maxEraseBlocks = 4;

                if (_device.Family == Device.Families.PIC32)
                {
                    if (startAddress == _device.StartFlash && endAddress == _device.EndFlash)
                    {
                        // simply use a bulk erase to quickly erase the entire flash memory of this device.
                        BulkEraseFlashPacket cmd = new BulkEraseFlashPacket();
                        sendPacket = cmd.FramePacket();
                        receivePacket.Clear();
                        result = _comm.SendGetPacket(sendPacket, ref receivePacket, 5);
                        switch (result)
                        {
                            case Comm.ErrorCode.Success:
                                break;

                            case Comm.ErrorCode.NoAcknowledgement:
                                break;
                        }
                        return result;
                    }
                }

                if (_device.HasEraseFlashCommand() == false)
                {
                    maxEraseBlocks = (int)((_device.MaxPacketSize() - WriteFlashPacket.HeaderSize)/
                                     (_device.EraseBlockSizeFlash*_device.BytesPerAddressFlash));
                }

                _abortOperation = false;

                for (address = endAddress; address > startAddress; address -= (int)(_device.EraseBlockSizeFlash*blocks))
                {
                    if (address - (int) _device.EraseBlockSizeFlash*maxEraseBlocks <= startAddress)
                    {
                        blocks = (int)((address - startAddress)/_device.EraseBlockSizeFlash);
                    }
                    else
                    {
                        blocks = maxEraseBlocks;
                    }

                    if (blocks != 0)
                    {
                        if (_device.HasEraseFlashCommand())
                        {
                            EraseFlashPacket cmd = new EraseFlashPacket();
                            cmd.SetAddress((uint)(address - _device.EraseBlockSizeFlash));
                            cmd.SetBlocks((byte)blocks);
                            sendPacket = cmd.FramePacket();
                        }
                        else
                        {
                            WriteFlashPacket cmd = new WriteFlashPacket();
                            int i = address - _device.BytesPerAddressFlash*blocks;
                            cmd.SetAddress((uint)i);
                            cmd.SetBlocks((byte)(_device.EraseBlockSizeFlash*blocks/_device.WriteBlockSizeFlash));
                            uint word = _device.BlankValue & _device.FlashWordMask;
                            while (i < address)
                            {
                                cmd.PacketData.Add((byte)(word & 0xFF));
                                cmd.PacketData.Add((byte)((word >> 8) & 0xFF));
                                switch (_device.BytesPerAddressFlash)
                                {
                                    case 2:
                                        i++;
                                        break;

                                    default:
                                    case 1:
                                        i += 2;
                                        break;
                                }
                            }
                            sendPacket = cmd.FramePacket();
                        }
                        receivePacket.Clear();
                        result = _comm.SendGetPacket(sendPacket, ref receivePacket, 5);
                        switch (result)
                        {
                            case Comm.ErrorCode.Success:
                                break;

                            case Comm.ErrorCode.NoAcknowledgement:
                                return result;

                            default:
                                return result;
                        }
                    }

                    if (_abortOperation)
                    {
                        return Comm.ErrorCode.Aborted;
                    }
                }

                return Comm.ErrorCode.Success;
            }

            public Comm.ErrorCode WriteFlash(DeviceData deviceData, uint startAddress, uint endAddress)
            {
                Comm.ErrorCode result;
                List<Device.MemoryRange> writeList = new List<Device.MemoryRange>();
                List<Device.MemoryRange> eraseList = new List<Device.MemoryRange>();

                _abortOperation = false;

                _writePlan.WriteConfig = WriteConfig;
                _writePlan.PlanFlashWrite(ref eraseList, ref writeList, startAddress, endAddress, deviceData.ProgramMemory);

                if (writeList.Count == 0 && eraseList.Count == 0)
                {
                    // nothing to do, we're done.
                    return Comm.ErrorCode.Success;
                }

                if (_device.HasEraseFlashCommand())
                {
                    // erase prior to writing FLASH memory
                    foreach (Device.MemoryRange range in eraseList)
                    {
                        result = EraseFlash((int)range.Start, (int)range.End);
                        if (result != Comm.ErrorCode.Success)
                        {
                            return result;
                        }
                    }
                }

                foreach (Device.MemoryRange range in writeList)
                {
                    uint flashOffset = _device.FlashPointer(range.Start);

                    if (_abortOperation)
                    {
                        return Comm.ErrorCode.Aborted;
                    }

                    result = WriteFlashMemory(deviceData.ProgramMemory, flashOffset, range.Start, range.End, deviceData.Mac);
                    switch (result)
                    {
                        case Comm.ErrorCode.Success:
                            break;

                        case Comm.ErrorCode.NoAcknowledgement:
                            return result;

                        default:
                            return result;
                    }
                }

                return Comm.ErrorCode.Success;
            }

            public Comm.ErrorCode WriteFlashMemory(uint[] memory, uint memoryOffset, uint startAddress, uint endAddress, byte[,] macData)
            {
                int bytesPerWriteBlock = _device.WriteBlockSizeFlash;

                WriteFlashPacket cmd = new WriteFlashPacket();
                cmd.SetAddress(startAddress);
                if (_device.Family == Device.Families.PIC32)
                {
                    // need to word align the data payload for PIC32 by stuffing two dummy bytes
                    // at the beginning of each Write Flash packet.
                    cmd.PacketData.Add(0x00);
                    cmd.PacketData.Add(0x00);
                }

                for (uint j = startAddress; j < endAddress;)
                {
                    uint word = memory[memoryOffset++];
                    cmd.PacketData.Add((byte)word);
                    cmd.PacketData.Add((byte)(word >> 8));
                    switch (_device.Family)
                    {
                        case Device.Families.PIC32:
                            cmd.PacketData.Add((byte)(word >> 16));
                            cmd.PacketData.Add((byte)(word >> 24));
                            break;

                        case Device.Families.PIC24:
                            cmd.PacketData.Add((byte)(word >> 16));
                            break;
                    }
                    _device.IncrementFlashAddressByInstructionWord(ref j);
                    if (_device.HasEncryption())
                    {
                        if ((j%_device.WriteBlockSizeFlash) == 0)
                        {
                            // end of write block -- append message authentication code (MAC) data
                            long index = (j/_device.WriteBlockSizeFlash) - 1;
                            for (int i = 0; i < 16; i++)
                            {
                                cmd.PacketData.Add(macData[index, i]);
                            }
                        }
                    }
                }

                if (_device.Family == Device.Families.PIC24)
                {
                    cmd.SetBlocks((byte)((cmd.PayloadSize()/_device.BytesPerWordFlash*_device.BytesPerWordFlash)/
                                  bytesPerWriteBlock));
                }
                else
                {
                    cmd.SetBlocks((byte)(cmd.PayloadSize()/(bytesPerWriteBlock*_device.BytesPerAddressFlash)));
                }
                List<byte> sendPacket = cmd.FramePacket();

                if (_abortOperation)
                {
                    return Comm.ErrorCode.Aborted;
                }

                Comm.ErrorCode result = _comm.SendPacket(sendPacket);
                if (result == Comm.ErrorCode.Success)
                {
                    List<byte> receivePacket = new List<byte>();

                    // At really slow baud rates (19.2Kbps and below), writing an entire
                    // write packet to the device might take a really long time, so we
                    // don't want to timeout immediately when we don't get an immediate
                    // response from the device.
                    result = _comm.GetPacket(ref receivePacket, 2500);
                }

                return result;
            }
        }

        public class DeviceVerifier
        {
            private readonly Comm _comm;
            private readonly Device _device;
            private readonly DeviceVerifyPlanner _verifyPlan;
            public bool WriteConfig;
            public int MaxBlockCount;
            public List<Device.MemoryRange> EraseList = new List<Device.MemoryRange>();
            public List<Device.MemoryRange> FailList = new List<Device.MemoryRange>();

            public DeviceVerifier(Device newDevice, Comm newComm)
            {
                _device = newDevice;
                _comm = newComm;
                _verifyPlan = new DeviceVerifyPlanner(newDevice);
                MaxBlockCount = 0x80;   // [UH] limit block count to prevent packet loss
            }

            public Comm.ErrorCode VerifyFlash(uint[] memory, int startAddress, int endAddress)
            {
                int errors = 0;

                List<byte> deviceCrc = new List<byte>();
                List<byte> memoryCrc = new List<byte>();
                List<byte> emptyCrc = new List<byte>();
                List<Device.MemoryRange> verifyList = new List<Device.MemoryRange>();
                ReadFlashCrcPacket cmd = new ReadFlashCrcPacket();
                DeviceData emptyData = new DeviceData(_device);
                emptyData.ClearAllData();
                EraseList.Clear();
                FailList.Clear();

                _verifyPlan.WriteConfig = WriteConfig;
                _verifyPlan.MaxBlockCount = MaxBlockCount;
                _verifyPlan.PlanFlashVerify(ref verifyList, startAddress, endAddress);

                foreach (Device.MemoryRange range in verifyList)
                {
                    if (_device.Family == Device.Families.PIC32)
                    {
                        cmd.SetAddress(range.Start | 0x80000000);
                    }
                    else
                    {
                        cmd.SetAddress(range.Start);
                    }

                    // PIC18 device calculating CRC over 129024 bytes of data requires:
                    // 6.812s at 2MHz
                    // 13.532s at 1MHz
                    // 26.984s at 500KHz
                    // 107.673s at 125KHz
                    // 215.346s at 62.5KHz
                    // 410.611s at 32.768KHz
                    //
                    // Someday, we might want to break our block sizes down to accomodate more
                    // GUI updates when verifying a slow running device.
                    cmd.SetBlocks((ushort)((range.End - range.Start) / _device.EraseBlockSizeFlash));
                    List<byte> sendPacket = cmd.FramePacket();
                    Comm.ErrorCode result = _comm.SendPacket(sendPacket);
                    if (result != Comm.ErrorCode.Success)
                    {
                        return result;
                    }

                    deviceCrc.Clear();

                    result = _comm.GetCrcData(ref deviceCrc);
                    if (result != Comm.ErrorCode.ERROR_BAD_CHKSUM && result != Comm.ErrorCode.Success)
                    {
                        return result;
                    }

                    int expectedBytes = (int)((range.End - range.Start) / _device.EraseBlockSizeFlash * 2);
                    if (deviceCrc.Count != expectedBytes)
                    {
                        //Log.Error("Bootloader", string.Format("Expected={0}, Received={1}", expectedBytes, deviceCrc.Count));
                        return Comm.ErrorCode.ERROR_BAD_CHKSUM;
                    }

                    // now we need to compute CRC's against the HEX file data we have in memory.
                    memoryCrc.Clear();
                    emptyCrc.Clear();
                    CalculateCrc(memory, ref memoryCrc, range.Start, range.End, ref deviceCrc);
                    CalculateCrc(emptyData.ProgramMemory, ref emptyCrc, range.Start, range.End, ref deviceCrc);

                    int i = 0;
                    while (i < memoryCrc.Count && i < deviceCrc.Count)
                    {
                        int address = (int)(range.Start + (i / 2 * _device.EraseBlockSizeFlash));
                        if ((deviceCrc[i] != memoryCrc[i]) || (deviceCrc[i + 1] != memoryCrc[i + 1]))
                        {
                            Device.MemoryRange block = new Device.MemoryRange
                            {
                                Start = (uint) address,
                                End = (uint) (address + _device.EraseBlockSizeFlash)
                            };
#if false
                            ushort word1 = memoryCrc[i + 1];
                            word1 <<= 8;
                            word1 |= memoryCrc[i];
                            ushort word2 = deviceCrc[i + 1];
                            word2 <<= 8;
                            word2 |= deviceCrc[i];
#endif
                            if (memoryCrc[i] == emptyCrc[i] && memoryCrc[i + 1] == emptyCrc[i + 1])
                            {
                                EraseList.Add(block);
                            }
                            else
                            {
                                FailList.Add(block);
                            }

                            errors++;
                        }
                        i += 2;
                    }
                }

                if (errors > 0)
                {
                    return Comm.ErrorCode.ERROR_BAD_CHKSUM;
                }

                return Comm.ErrorCode.Success;
            }

            public void CalculateCrc(uint[] memory, ref List<byte> result, uint startAddress, uint endAddress, ref List<byte> deviceCrc)
            {
                uint flashOffset = _device.FlashPointer(startAddress);

                uint i = startAddress;
                int j = 0;
                Crc crc = new Crc();
                while (i < endAddress)
                {
                    uint word = memory[flashOffset++] & _device.FlashWordMask;
                    crc.Add((byte)(word & 0xFF));
                    crc.Add((byte)((word >> 8) & 0xFF));
                    if (_device.Family == Device.Families.PIC24)
                    {
                        crc.Add((byte)((word >> 16) & 0xFF));
                    }
                    else if (_device.Family == Device.Families.PIC32)
                    {
                        crc.Add((byte)((word >> 16) & 0xFF));
                        crc.Add((byte)((word >> 24) & 0xFF));
                    }

                    _device.IncrementFlashAddressByInstructionWord(ref i);

                    if ((i%_device.EraseBlockSizeFlash) == 0)
                    {
                        result.Add(crc.Lsb());
                        result.Add(crc.Msb());

                        if (deviceCrc != null)
                        {
                            word = (uint)(deviceCrc[j++] & 0xFF);
                            word |= (uint)((deviceCrc[j++] & 0xFF) << 8);
                            crc = new Crc((ushort)word);
                        }
                    }
                }
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
                public uint AdapterType;
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

            private readonly Stream _inStream;
            private readonly Stream _outStream;

            public Stream InStream => _inStream;

            public Stream OutStream => _outStream;

            public Comm(Stream inStream, Stream outStream)
            {
                _inStream = inStream;
                _outStream = outStream;
            }

            public int XferMilliseconds(int bytes)
            {
                uint bps = 38400;
                int bits = bytes * 10; // each byte is 8 bits long, plus start and stop bits
                return (int)((bits * 1000) / bps);
            }

            public void ActivateBootloader(bool elmMode)
            {
                if (elmMode)
                {
                    System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                    byte[] bootCmdElm = enc.GetBytes("\r");
                    _outStream.Write(bootCmdElm, 0, bootCmdElm.Length);
                    Thread.Sleep(500);

                    bootCmdElm = enc.GetBytes("AT@3BL2345678901\r");
                    _outStream.Write(bootCmdElm, 0, bootCmdElm.Length);

                    while (_inStream.HasData())
                    {
                        if (_inStream.ReadByteAsync() < 0)
                        {
                            break;
                        }
                    }
                    return;
                }
                byte[] bootCmd = { 0x82, 0xF1, 0xF1, 0xFF, 0xFF, 0x62 };
                _outStream.Write(bootCmd, 0, bootCmd.Length);
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
                        if (!_inStream.HasData())
                        {
                            if (Stopwatch.GetTimestamp() - startTime > timeout * TickResolMs)
                            {
                                return ErrorCode.ERROR_READ_TIMEOUT;
                            }
                            Thread.Sleep(10);
                            continue;
                        }

                        int value = _inStream.ReadByteAsync();
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
                        if (!_inStream.HasData())
                        {
                            if (Stopwatch.GetTimestamp() - startTime > timeout * TickResolMs)
                            {
                                return ErrorCode.ERROR_READ_TIMEOUT;
                            }
                            Thread.Sleep(10);
                            continue;
                        }

                        int value = _inStream.ReadByteAsync();
                        if (value == DLE)
                        {
                            while (!_inStream.HasData())
                            {
                                if (Stopwatch.GetTimestamp() - startTime > timeout * TickResolMs)
                                {
                                    return ErrorCode.ERROR_READ_TIMEOUT;
                                }
                                Thread.Sleep(10);
                            }
                            value = _inStream.ReadByteAsync();
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
                catch (Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            public ErrorCode GetCrcData(ref List<byte> receivePacket)
            {
                int timeout = 4000; // maximum ms to wait between characters received before aborting as timed out
                //QByteArray rawData;
                int errorCount = 0;

                // Scan for a start condition
                int junkReceived = 0;
                long startTime = Stopwatch.GetTimestamp();
                for (;;)
                {
                    if (!_inStream.HasData())
                    {
                        if (Stopwatch.GetTimestamp() - startTime > timeout*TickResolMs)
                        {
                            return ErrorCode.ERROR_READ_TIMEOUT;
                        }
                        Thread.Sleep(10);
                        continue;
                    }

                    int value = _inStream.ReadByteAsync();
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
                for (;;)
                {
                    if (!_inStream.HasData())
                    {
                        if (Stopwatch.GetTimestamp() - startTime > timeout*TickResolMs)
                        {
                            return ErrorCode.ERROR_READ_TIMEOUT;
                        }
                        Thread.Sleep(10);
                        continue;
                    }
                    int value = _inStream.ReadByteAsync();
                    // we got some data, don't timeout
                    startTime = Stopwatch.GetTimestamp();

                    if (value == DLE)
                    {
                        while (!_inStream.HasData())
                        {
                            if (Stopwatch.GetTimestamp() - startTime > timeout*TickResolMs)
                            {
                                return ErrorCode.ERROR_READ_TIMEOUT;
                            }
                            Thread.Sleep(10);
                        }
                        value = _inStream.ReadByteAsync();
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
                            return ErrorCode.Success;
                        }
                    }
                    receivePacket.Add((byte)value);
                }
            }

            public ErrorCode SendPacket(List<byte> sendPacket)
            {
                try
                {
                    // send the STX
                    _outStream.WriteByte(sendPacket[0]);

                    // wait for the responding STX echoed back
                    long startTime = Stopwatch.GetTimestamp();
                    while (!_inStream.HasData())
                    {
                        if (Stopwatch.GetTimestamp() - startTime > SyncWaitTime * 100 * TickResolMs)
                        {
                            return ErrorCode.ERROR_READ_TIMEOUT;
                        }
                        Thread.Sleep(10);
                    }

                    // now we are free to send the rest of the packet
                    List<byte> tempPacket = new List<byte>(sendPacket);
                    tempPacket.RemoveAt(0);
                    _outStream.Write(tempPacket.ToArray(), 0, tempPacket.Count);

                    return ErrorCode.Success;
                }
                catch (Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            public ErrorCode SendGetPacket(List<byte> sendPacket, ref List<byte> receivePacket, int retryLimit, int timeOut = 1000)
            {
                try
                {
                    int txTime = XferMilliseconds(sendPacket.Count);

                    // send the STX
                    _outStream.WriteByte(sendPacket[0]);

                    // wait for the responding STX echoed back
                    long startTime = Stopwatch.GetTimestamp();
                    while (!_inStream.HasData())
                    {
                        if (Stopwatch.GetTimestamp() - startTime > SyncWaitTime * 100 * TickResolMs)
                        {
                            return ErrorCode.ERROR_READ_TIMEOUT;
                        }
                        Thread.Sleep(10);
                    }

                    // now we are free to send the rest of the packet
                    List<byte> tempPacket = new List<byte>(sendPacket);
                    tempPacket.RemoveAt(0);
                    while (retryLimit-- >= 0)
                    {
                        _outStream.Write(tempPacket.ToArray(), 0, tempPacket.Count);

                        ErrorCode retStatus = GetPacket(ref receivePacket, txTime + timeOut);
                        switch (retStatus)
                        {
                            case ErrorCode.ERROR_READ_TIMEOUT:
                            case ErrorCode.ERROR_BAD_CHKSUM:
                                continue;

                            default:
                                return retStatus;
                        }
                    }

                    return ErrorCode.RetryLimitReached;
                }
                catch (Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            public BootInfo ReadBootloaderInfo(int timeout = 10)
            {
                BootInfo bootInfo = new BootInfo();

                try
                {
                    while (_inStream.HasData())
                    {
                        if (_inStream.ReadByteAsync() < 0)
                        {
                            break;
                        }
                    }
                    _outStream.WriteByte(STX);

                    BootloaderInfoPacket cmd = new BootloaderInfoPacket();
                    List<byte> sendPacket = cmd.FramePacket();

                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        while (!_inStream.HasData())
                        {
                            if (timeout <= 0)
                            {
                                return bootInfo;
                            }

                            if (Stopwatch.GetTimestamp() - startTime > SyncWaitTime * TickResolMs)
                            {
                                _outStream.WriteByte(STX);
                                startTime = Stopwatch.GetTimestamp();
                                timeout--;
                            }
                            Thread.Sleep(10);
                        }

                        int value = _inStream.ReadByteAsync();
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
                    if (response.Count > 12)
                    {   // [UH] added adapter type info
                        bootInfo.AdapterType = response[12];
                        bootInfo.AdapterType += (uint)(response[13] << 8);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                return bootInfo;
            }

            public ErrorCode RunApplication(bool elmFirmware)
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

                    if (!_inStream.HasData())
                    {
                        return ErrorCode.NoAcknowledgement;
                    }
                    int response = _inStream.ReadByteAsync();
                    if (response != STX)
                    {
                        return ErrorCode.NoAcknowledgement;
                    }

                    int waitTime = elmFirmware ? 3000 : 500;    // ELM firmware restarts two times
                    Thread.Sleep(waitTime);  // wait for app start
                    return ErrorCode.Success;
                }
                catch (Exception)
                {
                    return ErrorCode.CouldNotTransmit;
                }
            }

            public DeviceId ReadDeviceId(Device.Families deviceFamily)
            {
                List<byte> sendPacket;
                List<byte> response = new List<byte>();
                DeviceId result;

                ReadFlashPacket cmd = new ReadFlashPacket();

                switch (deviceFamily)
                {
                    case Device.Families.PIC32:
                        cmd.SetAddress(0xBF80F220);
                        cmd.SetBytes(1);
                        sendPacket = cmd.FramePacket();

                        if (SendGetPacket(sendPacket, ref response, 4) != ErrorCode.Success)
                        {
                            result.Id = 0;
                            result.Revision = 0;
                            return result;
                        }

                        result.Id = (uint)(((response[2] & 0x0F) << 4) | ((response[1] & 0xF0) >> 4));
                        result.Revision = (response[3] << 4) | ((response[2] & 0xF0) >> 4);
                        break;

                    case Device.Families.PIC24:
                        cmd.SetAddress(0xFF0000);
                        cmd.SetBytes(2);
                        sendPacket = cmd.FramePacket();

                        if (SendGetPacket(sendPacket, ref response, 4) != ErrorCode.Success)
                        {
                            result.Id = 0;
                            result.Revision = 0;
                            return result;
                        }

                        result.Id = (uint)(response[0] | (response[1] << 8));
                        result.Revision = response[3] | ((response[4] & 0x01) << 8);
                        break;

                    case Device.Families.PIC18:
                    default:
                        cmd.SetAddress(0x3FFFFE);
                        cmd.SetBytes(2);
                        sendPacket = cmd.FramePacket();

                        if (SendGetPacket(sendPacket, ref response, 4) != ErrorCode.Success)
                        {
                            result.Id = 0;
                            result.Revision = 0;
                            return result;
                        }

                        result.Id = (uint)(((response[1] & 0xFF) << 8 | (response[0] & 0xFF)) >> 5);
                        result.Revision = response[0] & 0x1F;
                        break;
                }

                return result;
            }
        }

        public class HexImporter
        {
            public bool HasEndOfFileRecord;    // hex file does have an end of file record
            public bool HasConfigBits;         // hex file has config bit settings
            public bool FileExceedsFlash;      // hex file records exceed device memory constraints

            public List<Device.MemoryRange> Ranges = new List<Device.MemoryRange>();
            public List<Device.MemoryRange> Rawimport = new List<Device.MemoryRange>();

            public bool ImportHexFile(Stream hexFile, DeviceData data, Device device)
            {
                try
                {
                    HasEndOfFileRecord = false;
                    FileExceedsFlash = false;

                    bool lineExceedsFlash = true;
                    uint eepromBytesPerWord = 1;
                    uint cfgBytesPerWord = 2;

                    if (device.Family == Device.Families.PIC32)
                    {
                        cfgBytesPerWord = 4;
                    }

                    uint configWords = device.EndConfig - device.StartConfig;
                    data.ClearAllData();
                    uint segmentAddress = 0;

                    Device.MemoryRange range = new Device.MemoryRange
                    {
                        Start = 0,
                        End = 0
                    };
                    Ranges.Clear();
                    Rawimport.Clear();
                    HasConfigBits = false;
                    data.Encrypted = false;
                    data.Nonce = 0;
                    data.Mac = new byte[(device.EndFlash - device.StartFlash)/device.BytesPerWordFlash + 1, 16];

                    hexFile.Position = 0;
                    using (StreamReader srHexFile = new StreamReader(hexFile))
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

                            uint byteCount = Convert.ToUInt32(line.Substring(1, 2), 16);
                            uint lineAddress = segmentAddress + Convert.ToUInt32(line.Substring(3, 4), 16);
                            int recordType = Convert.ToInt32(line.Substring(7, 2), 16);

                            if (recordType == 1) // end of file record
                            {
                                HasEndOfFileRecord = true;
                                break;
                            }
                            else
                            {
                                string lineString;
                                if (recordType == 0x43) // nonce value record
                                {
                                    data.Encrypted = true;

                                    // skip if line isn't long enough for bytecount.
                                    if (line.Length >= (11 + (2*byteCount)))
                                    {
                                        lineString = line.Substring(9, 8);
                                        data.Nonce = Convert.ToUInt32(lineString, 16);
                                    }
                                }
                                else if (recordType == 0x40) // MAC data record
                                {
                                    data.Encrypted = true;

                                    // skip if line isn't long enough for bytecount.
                                    if (line.Length >= (11 + (2*byteCount)))
                                    {
                                        lineString = line.Substring(9, 16*2);
                                        long index = lineAddress/device.WriteBlockSizeFlash;
                                        for (int x = 0; x < 16; x++)
                                        {
                                            data.Mac[index, x] = Convert.ToByte(lineString.Substring(x * 2, 2), 16);
                                        }
                                    }
                                }
                                else if ((recordType == 2) || (recordType == 4)) // Segment address
                                {
                                    // skip if line isn't long enough for bytecount.
                                    if (line.Length >= (11 + (2*byteCount)))
                                    {
                                        segmentAddress = Convert.ToUInt32(line.Substring(9, 4), 16);
                                    }

                                    if (recordType == 2)
                                    {
                                        segmentAddress <<= 4;
                                    }
                                    else
                                    {
                                        segmentAddress <<= 16;
                                    }
                                } // end if ((recordType == 2) || (recordType == 4))
                                else if (recordType == 0) // Data Record
                                {
                                    if (line.Length < (11 + (2*byteCount)))
                                    {
                                        // skip if line isn't long enough for bytecount.
                                        continue;
                                    }

                                    Device.MemoryRange rawRange = new Device.MemoryRange
                                    {
                                        Start = lineAddress,
                                        End = lineAddress + byteCount
                                    };
                                    Rawimport.Add(rawRange);

                                    uint deviceAddress = device.FromHexAddress(lineAddress, out bool error);
                                    if (error)
                                    {
                                        // don't do anything here, this address is outside of device memory space.
                                    }
                                    else if (range.Start == 0 && range.End == 0)
                                    {
                                        range.Start = deviceAddress;
                                        range.End = device.FromHexAddress(lineAddress + byteCount, out error);
                                        Ranges.Add(range.Clone());
                                    }
                                    else if (Ranges.Count > 0 && Ranges[Ranges.Count - 1].End == deviceAddress)
                                    {
                                        Ranges[Ranges.Count - 1].End = device.FromHexAddress(lineAddress + byteCount, out error);
                                    }
                                    else
                                    {
                                        range.Start = deviceAddress;
                                        range.End = device.FromHexAddress(lineAddress + byteCount, out error);
                                        Ranges.Add(range.Clone());
                                    }

                                    if (device.HasConfigAsFlash())
                                    {
                                        if ((range.Start <= device.StartConfig && range.End >= device.EndConfig) ||
                                            (range.Start >= device.StartConfig && range.Start < device.EndConfig) ||
                                            (range.End > device.StartConfig && range.End <= device.EndConfig))
                                        {
                                            HasConfigBits = true;
                                        }
                                    }

                                    for (uint lineByte = 0; lineByte < byteCount; lineByte++)
                                    {
                                        uint byteAddress = lineAddress + lineByte;
                                        uint bytePosition;
                                        if (device.Family == Device.Families.PIC24)
                                        {
                                            // compute byte position within memory word
                                            bytePosition = byteAddress%4;
                                        }
                                        else
                                        {
                                            // compute byte position within memory word
                                            bytePosition = (uint) (byteAddress%device.BytesPerWordFlash);
                                        }

                                        // get the byte value from hex file
                                        string hexByte = line.Substring((int) (9 + (2*lineByte)), 2);
                                        uint wordByte = 0xFFFFFF00 | Convert.ToUInt32(hexByte, 16);
                                        // shift the byte into its proper position in the word.
                                        for (uint shift = 0; shift < bytePosition; shift++)
                                        {
                                            wordByte <<= 8;
                                            wordByte |= 0xFF; // shift in ones.
                                        }

                                        lineExceedsFlash = true; // if not in any memory section, then error

                                        // program memory section --------------------------------------------------
                                        if (((byteAddress/device.BytesPerAddressFlash) < device.EndFlash) &&
                                            ((byteAddress/device.BytesPerAddressFlash) >= device.StartFlash))
                                        {
                                            // compute array address from hex file address # bytes per memory location
                                            uint arrayAddress;
                                            if (device.Family == Device.Families.PIC24)
                                            {
                                                arrayAddress = (byteAddress - device.StartFlash)/4;
                                            }
                                            else
                                            {
                                                arrayAddress =
                                                    (uint) ((byteAddress - device.StartFlash)/device.BytesPerWordFlash);
                                            }

                                            data.ProgramMemory[arrayAddress] &= wordByte; // add byte.
                                            lineExceedsFlash = false;
                                            //NOTE: program memory locations containing config words may get modified
                                            // by the config section below that applies the config masks.
                                        }

                                        // EE data section ---------------------------------------------------------
                                        if (device.Family == Device.Families.PIC16)
                                        {
                                            byteAddress >>= 1;
                                        }

                                        if (device.HasEeprom() && byteAddress >= device.StartEeprom)
                                        {
                                            uint eeAddress;

                                            switch (device.Family)
                                            {
                                                case Device.Families.PIC24:
                                                    eeAddress = (byteAddress >> 1) - device.StartEeprom;
                                                    if (eeAddress < device.EndEeprom - device.StartEeprom)
                                                    {
                                                        data.EePromMemory[eeAddress >> 1] &= wordByte;

                                                        lineExceedsFlash = false;
                                                    }
                                                    break;

                                                case Device.Families.PIC16:
                                                    if (byteAddress < device.EndEeprom)
                                                    {
                                                        eeAddress = (byteAddress - device.StartEeprom)/eepromBytesPerWord;
                                                        data.EePromMemory[eeAddress] &= wordByte; // add byte.
                                                        lineExceedsFlash = false;
                                                    }
                                                    break;

                                                default:
                                                case Device.Families.PIC18:
                                                    if (byteAddress < device.EndEeprom)
                                                    {
                                                        eeAddress = (byteAddress - device.StartEeprom)/eepromBytesPerWord;
                                                        int eeshift =
                                                            (int) ((bytePosition/eepromBytesPerWord)*eepromBytesPerWord);
                                                        for (int reshift = 0; reshift < eeshift; reshift++)
                                                        {
                                                            // shift byte into proper position
                                                            wordByte >>= 8;
                                                        }
                                                        data.EePromMemory[eeAddress] &= wordByte; // add byte.
                                                        lineExceedsFlash = false;
                                                    }
                                                    break;
                                            }
                                        }

                                        // Config words section ----------------------------------------------------
                                        if ((byteAddress >= device.StartConfig) && (configWords > 0))
                                        {
                                            uint configNum = (byteAddress - (device.StartConfig))/cfgBytesPerWord;
                                            if (configNum < configWords)
                                            {
                                                lineExceedsFlash = false;
                                                HasConfigBits = true;
                                                if (cfgBytesPerWord == 4)
                                                {
                                                    data.ConfigWords[configNum] &= (wordByte & 0xFFFFFFFF);
                                                }
                                                else
                                                {
                                                    data.ConfigWords[configNum] &= (wordByte & 0xFFFF);
                                                }
                                            }
                                        }
                                    } // end for (lineByte = 0; lineByte < byteCount; lineByte++)

                                    if (lineExceedsFlash)
                                    {
                                        FileExceedsFlash = true;
                                    }
                                } // end if (recordType == 0)
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

            public byte ComputeChecksum(string fileLine)
            {
                int byteCount = ParseHex(fileLine, 1, 2);
                if (fileLine.Length >= (9 + (2* byteCount)))
                { // skip if line isn't long enough for bytecount.
                    int checksum = byteCount;
                    for(uint i = 0; i < (3 + byteCount); i++)
                    {
                        checksum += ParseHex(fileLine, (int)(3 + (2*i)), 2);
                    }
                    checksum = 0 - checksum;
                    return (byte)(checksum & 0xFF);
                }

                return 0;
            }

            private int ParseHex(string characters, int offset, int length)
            {
                int integer = 0;

                for (int i = 0; i < length; i++)
                {
                    integer *= 16;
                    switch (characters[offset + i])
                    {
                        case '1':
                            integer += 1;
                            break;

                        case '2':
                            integer += 2;
                            break;

                        case '3':
                            integer += 3;
                            break;

                        case '4':
                            integer += 4;
                            break;

                        case '5':
                            integer += 5;
                            break;

                        case '6':
                            integer += 6;
                            break;

                        case '7':
                            integer += 7;
                            break;

                        case '8':
                            integer += 8;
                            break;

                        case '9':
                            integer += 9;
                            break;

                        case 'A':
                        case 'a':
                            integer += 10;
                            break;

                        case 'B':
                        case 'b':
                            integer += 11;
                            break;

                        case 'C':
                        case 'c':
                            integer += 12;
                            break;

                        case 'D':
                        case 'd':
                            integer += 13;
                            break;

                        case 'E':
                        case 'e':
                            integer += 14;
                            break;

                        case 'F':
                        case 'f':
                            integer += 15;
                            break;
                    }
                }
                return integer;
            }
        }

        public static bool LoadDevice(Device device, int deviceId, Device.Families familyId)
        {
            device.SetUnknown();
            device.Id = deviceId;
            device.Family = familyId;
            if (familyId != Device.Families.PIC18)
            {
                return false;
            }
            if (deviceId == 0x030C)
            {
                device.Name = "PIC18F25K80";
                device.WriteBlockSizeFlash = 0x40;
                device.EraseBlockSizeFlash = 0x40;
                device.StartFlash  = 0x000000;
                device.EndFlash    = 0x008000;
                device.StartEeprom = 0xF00000;
                device.EndEeprom   = 0xF00400;
                device.StartUser   = 0x200000;
                device.EndUser     = 0x200008;
                device.StartConfig = 0x300000;
                device.EndConfig   = 0x30000E;
                device.StartGpr    = 0x000000;
                device.EndGpr      = 0x000E41;
                device.BytesPerWordFlash = 2;
            }
            else
            {
                return false;
            }
            switch(device.Family)
            {
                case Device.Families.PIC16:
                    device.BytesPerAddressFlash = 2;
                    device.BytesPerWordEeprom = 1;
                    device.FlashWordMask = 0x3FFF;
                    device.ConfigWordMask = 0xFF;
                    break;

                case Device.Families.PIC24:
                    device.BytesPerAddressFlash = 2;
                    device.BytesPerWordEeprom = 2;
                    device.FlashWordMask = 0xFFFFFF;
                    device.ConfigWordMask = 0xFFFF;
                    device.WriteBlockSizeFlash *= 2;       // temporary
                    device.EraseBlockSizeFlash *= 2;
                    break;

                case Device.Families.PIC32:
                    device.FlashWordMask = 0xFFFFFFFF;
                    device.ConfigWordMask = 0xFFFFFFFF;
                    device.BytesPerAddressFlash = 1;
                    break;

                case Device.Families.PIC18:
                default:
                    device.FlashWordMask = 0xFFFF;
                    device.ConfigWordMask = 0xFF;
                    device.BytesPerAddressFlash = 1;
                    device.BytesPerWordEeprom = 1;
                    break;
            }
            return true;
        }

        public static bool EnterBootloaderMode(Device device, Comm comm, bool elmMode)
        {
            try
            {
                Comm.BootInfo bootInfo = comm.ReadBootloaderInfo(20);
                if (bootInfo.MajorVersion == 0 && bootInfo.MinorVersion == 0)
                {
                    comm.ActivateBootloader(elmMode);
                    Thread.Sleep(600);
                    bootInfo = comm.ReadBootloaderInfo();
                }
                if (bootInfo.MajorVersion == 0 && bootInfo.MinorVersion == 0)
                {
                    return false;
                }

                Comm.DeviceId deviceId;
                if (bootInfo.DeviceIdent != 0)
                {
                    deviceId.Id = bootInfo.DeviceIdent;
                    deviceId.Revision = -1;
                }
                else
                {
                    deviceId = comm.ReadDeviceId((Device.Families)bootInfo.FamilyId);
                }

                if (deviceId.Id == 0x0309)
                {   // PIC18F26K80 (Flash size 0x10000)
                    deviceId.Id = 0x030C;
                }

                if (!LoadDevice(device, (int) deviceId.Id, (Device.Families) bootInfo.FamilyId))
                {
                    return false;
                }
                device.StartBootloader = bootInfo.StartBootloader;
                device.EndBootloader = bootInfo.EndBootloader;
                device.CommandMask = bootInfo.CommandMask;
                device.AdapterType = bootInfo.AdapterType;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool WriteDevice(Device device, DeviceData deviceData, Comm comm)
        {
            DeviceWriter deviceWriter = new DeviceWriter(device, comm)
            {
                WriteConfig = true
            };
            Comm.ErrorCode errorCode = deviceWriter.WriteFlash(deviceData, device.StartFlash, device.EndFlash);
            if (errorCode != Comm.ErrorCode.Success)
            {
                return false;
            }
            DeviceVerifier deviceVerifier = new DeviceVerifier(device, comm)
            {
                WriteConfig = true
            };
            errorCode = deviceVerifier.VerifyFlash(deviceData.ProgramMemory, (int) device.StartFlash, (int) device.EndFlash);
            if (errorCode != Comm.ErrorCode.Success)
            {
                if (deviceVerifier.EraseList.Count == 0 &&
                    deviceVerifier.FailList.Count == 0)
                {   // communication error
                    return false;
                }
                if (deviceVerifier.FailList.Count != 0)
                {
                    return false;
                }
                if (deviceVerifier.EraseList.Count != 0)
                {
                    errorCode = deviceWriter.EraseFlash(deviceVerifier.EraseList);
                    if (errorCode != Comm.ErrorCode.Success)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void CreateFirmwareList()
        {
            if (_firmwareList != null)
            {
                return;
            }
            _firmwareList = new List<FirmwareDetail>();
            Device device = new Device();
            DeviceData deviceData = new DeviceData(device);
            HexImporter hexImporter = new HexImporter();
            foreach (FirmwareInfo firmwareInfo in FirmwareInfos)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    typeof(XmlToolActivity).Namespace + ".HexFiles." + firmwareInfo.FileName))
                {
                    if (stream != null)
                    {
                        if (LoadDevice(device, (int)firmwareInfo.DeviceId, firmwareInfo.FamilyId))
                        {
                            if (hexImporter.ImportHexFile(stream, deviceData, device))
                            {
                                uint adapterType;
                                if (device.BytesPerWordFlash > 1)
                                {
                                    adapterType = deviceData.ProgramMemory[(device.EndFlash - 4) / device.BytesPerWordFlash] & 0xFFFF;
                                }
                                else
                                {
                                    adapterType = (deviceData.ProgramMemory[(device.EndFlash - 4)] & 0xFF) + ((deviceData.ProgramMemory[(device.EndFlash - 3)] & 0xFF) << 8);
                                }
                                uint adapterVersion;
                                if (device.BytesPerWordFlash > 1)
                                {
                                    adapterVersion = deviceData.ProgramMemory[(device.EndFlash - 6) / device.BytesPerWordFlash] & 0xFFFF;
                                }
                                else
                                {
                                    adapterVersion = (deviceData.ProgramMemory[(device.EndFlash - 6)] & 0xFF) + ((deviceData.ProgramMemory[(device.EndFlash - 5)] & 0xFF) << 8);
                                }
                                _firmwareList.Add(new FirmwareDetail(firmwareInfo, adapterType, adapterVersion, firmwareInfo.ElmFirmware));
                            }
                        }
                    }
                }
            }
        }

        public static int GetFirmwareVersion(uint adapterType, bool elmFirmware = false)
        {
            CreateFirmwareList();
            foreach (FirmwareDetail firmwareDetail in _firmwareList)
            {
                if (firmwareDetail.AdapterType == adapterType && firmwareDetail.ElmFirmware == elmFirmware)
                {
                    return (int)firmwareDetail.AdapterVersion;
                }
            }
            return -1;
        }

        public static bool FwUpdate(Stream inStream, Stream outStream, bool elmMode = false, bool elmFirmware = false)
        {
            CreateFirmwareList();
            Comm comm = new Comm(inStream, outStream);
            Device device = new Device();
            if (!EnterBootloaderMode(device, comm, elmMode))
            {
                return false;
            }
            // get the firmware file
            Stream hexFile = (from firmwareDetail in _firmwareList
                              where firmwareDetail.AdapterType == device.AdapterType && firmwareDetail.ElmFirmware == elmFirmware
                              select Assembly.GetExecutingAssembly().GetManifestResourceStream(
                              typeof (XmlToolActivity).Namespace + ".HexFiles." + firmwareDetail.FirmwareInfo.FileName)).FirstOrDefault();
            if (hexFile == null)
            {
                comm.RunApplication(elmMode);
                return false;
            }
            try
            {
                HexImporter hexImporter = new HexImporter();
                DeviceData deviceData = new DeviceData(device);
                if (!hexImporter.ImportHexFile(hexFile, deviceData, device))
                {
                    comm.RunApplication(elmMode);
                    return false;
                }
                uint adapterType;
                if (device.BytesPerWordFlash > 1)
                {
                    adapterType = deviceData.ProgramMemory[(device.EndFlash - 4) / device.BytesPerWordFlash] & 0xFFFF;
                }
                else
                {
                    adapterType = (deviceData.ProgramMemory[(device.EndFlash - 4)] & 0xFF) + ((deviceData.ProgramMemory[(device.EndFlash - 3)] & 0xFF) << 8);
                }
                if (adapterType != device.AdapterType)
                {
                    comm.RunApplication(elmMode);
                    return false;
                }
#if false
                // flash fill test
                for (int i = 0x0800; i < deviceData.ProgramMemory.Length; i++)
                {
                    deviceData.ProgramMemory[i] = (uint)(i * 2 + i);
                }
#endif
#if false
                // flash erase test
                DeviceWritePlanner writePlan = new DeviceWritePlanner(device);
                List<Device.MemoryRange> eraseList = new List<Device.MemoryRange>();
                writePlan.PlanFlashErase(ref eraseList);
                deviceWriter.EraseFlash(eraseList);
#endif
                if (!WriteDevice(device, deviceData, comm))
                {
                    return false;
                }
                Comm.ErrorCode errorCode = comm.RunApplication(elmFirmware);
                if (errorCode != Comm.ErrorCode.Success)
                {
                    return false;
                }
            }
            finally
            {
                hexFile.Close();
                hexFile.Dispose();
            }
            return true;
        }

        public static bool IsInBooloaderMode(Stream inStream, Stream outStream)
        {
            try
            {
                Comm comm = new Comm(inStream, outStream);
                Comm.BootInfo bootInfo = comm.ReadBootloaderInfo(20);
                if (bootInfo.MajorVersion == 0 && bootInfo.MinorVersion == 0)
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
    }
}
