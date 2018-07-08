using System;
using System.Collections.Generic;

namespace UdsFileReader
{
    public static class VehicleInfoVag
    {
        public class EcuAddressEntry
        {
            public EcuAddressEntry(uint address, uint tp20EcuAddr, uint tp20TesterAddr, uint isoTpEcuCanId, uint isoTpTesterCanId)
            {
                Address = address;
                Tp20EcuAddr = tp20EcuAddr;
                Tp20TesterAddr = tp20TesterAddr;
                IsoTpEcuCanId = isoTpEcuCanId;
                IsoTpTesterCanId = isoTpTesterCanId;
            }

            public uint Address { get; }

            public uint Tp20EcuAddr { get; }

            public uint Tp20TesterAddr { get; }

            public uint IsoTpEcuCanId { get; }

            public uint IsoTpTesterCanId { get; }
        }

        public static readonly EcuAddressEntry[] EcuAddressArray =
        {
            new EcuAddressEntry(0x00000001, 0x00000010, 0x00000001, 0x000007E0, 0x000007E8),
            new EcuAddressEntry(0x00000002, 0x0000001A, 0x00000002, 0x000007E1, 0x000007E9),
            new EcuAddressEntry(0x00000003, 0x00000028, 0x00000003, 0x00000713, 0x0000077D),
            new EcuAddressEntry(0x00000004, 0x00000004, 0x00000013, 0x00000751, 0x000007BB),
            new EcuAddressEntry(0x00000005, 0x000000C4, 0x00000031, 0x00000732, 0x0000079C),
            new EcuAddressEntry(0x00000006, 0x000000A5, 0x00000035, 0x0000074D, 0x000007B7),
            new EcuAddressEntry(0x00000007, 0x00000063, 0x0000003F, 0x17FC008E, 0x17FE008E),
            new EcuAddressEntry(0x00000008, 0x00000098, 0x0000002C, 0x00000746, 0x000007B0),
            new EcuAddressEntry(0x00000009, 0x000000A4, 0x00000020, 0x0000070E, 0x00000778),
            new EcuAddressEntry(0x0000000A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000000B, 0x0000009C, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000000C, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000761, 0x000007CB),
            new EcuAddressEntry(0x0000000D, 0x000000B0, 0x00000024, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000000E, 0x0000008B, 0x00000058, 0x00000770, 0x000007DA),
            new EcuAddressEntry(0x0000000F, 0x00000088, 0x0000004F, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000010, 0x00000045, 0x0000001D, 0x0000070A, 0x00000774),
            new EcuAddressEntry(0x00000011, 0x00000011, 0x00000015, 0x000007E2, 0x000007EA),
            new EcuAddressEntry(0x00000012, 0x0000001B, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000013, 0x00000029, 0x0000000B, 0x00000757, 0x000007C1),
            new EcuAddressEntry(0x00000014, 0x0000003B, 0x0000000C, 0x00000772, 0x000007DC),
            new EcuAddressEntry(0x00000015, 0x00000058, 0x00000005, 0x00000715, 0x0000077F),
            new EcuAddressEntry(0x00000016, 0x00000030, 0x0000002A, 0x0000070C, 0x00000776),
            new EcuAddressEntry(0x00000017, 0x00000061, 0x00000007, 0x00000714, 0x0000077E),
            new EcuAddressEntry(0x00000018, 0x00000099, 0x0000002F, 0x0000076A, 0x000007D4),
            new EcuAddressEntry(0x00000019, 0x00000040, 0x0000001F, 0x00000710, 0x0000077A),
            new EcuAddressEntry(0x0000001A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000001B, 0x00000035, 0x0000001E, 0x00000716, 0x00000780),
            new EcuAddressEntry(0x0000001C, 0x00000069, 0x00000040, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000001D, 0x00000044, 0x0000002B, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000001E, 0x00000085, 0x00000050, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000001F, 0x00000089, 0x0000005F, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000020, 0xFFFFFFFF, 0x00000016, 0x00000730, 0x0000079A),
            new EcuAddressEntry(0x00000021, 0x00000012, 0x00000016, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000022, 0x0000001C, 0x0000000A, 0x0000071D, 0x00000787),
            new EcuAddressEntry(0x00000023, 0x0000002A, 0x0000000D, 0x0000073B, 0x000007A5),
            new EcuAddressEntry(0x00000024, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000025, 0x000000C0, 0x00000014, 0x00000711, 0x0000077B),
            new EcuAddressEntry(0x00000026, 0x000000A0, 0x00000030, 0x0000072D, 0x00000797),
            new EcuAddressEntry(0x00000027, 0x00000064, 0x0000003E, 0x17FC0093, 0x17FE0093),
            new EcuAddressEntry(0x00000028, 0x0000009A, 0x00000045, 0x0000071A, 0x00000784),
            new EcuAddressEntry(0x00000029, 0x00000071, 0x00000038, 0x17FC0082, 0x17FE0082),
            new EcuAddressEntry(0x0000002A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000002B, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000731, 0x0000079B),
            new EcuAddressEntry(0x0000002C, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000002D, 0x00000092, 0x00000059, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000002E, 0x00000086, 0x00000054, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000002F, 0x0000008A, 0x0000004C, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000030, 0xFFFFFFFF, 0x0000003B, 0x0000072B, 0x00000795),
            new EcuAddressEntry(0x00000031, 0x00000013, 0x00000001, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000032, 0x00000018, 0x00000018, 0x0000071E, 0x00000788),
            new EcuAddressEntry(0x00000033, 0x00000033, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000034, 0x00000038, 0x00000004, 0x00000755, 0x000007BF),
            new EcuAddressEntry(0x00000035, 0x000000C1, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000036, 0x000000A1, 0x00000026, 0x0000074C, 0x000007B6),
            new EcuAddressEntry(0x00000037, 0x00000062, 0x0000005B, 0x0000076C, 0x000007D6),
            new EcuAddressEntry(0x00000038, 0x000000AF, 0x00000027, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000039, 0x00000072, 0x00000039, 0x17FC0083, 0x17FE0083),
            new EcuAddressEntry(0x0000003A, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000072F, 0x00000799),
            new EcuAddressEntry(0x0000003B, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000721, 0x0000078B),
            new EcuAddressEntry(0x0000003C, 0x0000006B, 0x0000001C, 0x0000074E, 0x000007B8),
            new EcuAddressEntry(0x0000003D, 0x000000D0, 0x0000003A, 0x0000072C, 0x00000796),
            new EcuAddressEntry(0x0000003E, 0x00000087, 0x00000062, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000003F, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000040, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000719, 0x00000783),
            new EcuAddressEntry(0x00000041, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000042, 0x000000A9, 0x00000022, 0x0000074A, 0x000007B4),
            new EcuAddressEntry(0x00000043, 0x0000002B, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000044, 0x00000031, 0x00000009, 0x00000712, 0x0000077C),
            new EcuAddressEntry(0x00000045, 0x000000C2, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000046, 0x000000A2, 0x00000021, 0x17FC008B, 0x17FE008B),
            new EcuAddressEntry(0x00000047, 0x00000081, 0x00000053, 0x0000076F, 0x000007D9),
            new EcuAddressEntry(0x00000048, 0x000000A8, 0x00000037, 0x17FC0087, 0x17FE0087),
            new EcuAddressEntry(0x00000049, 0x00000073, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000004A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000004B, 0xFFFFFFFF, 0x00000039, 0x17FC00A9, 0x17FE00A9),
            new EcuAddressEntry(0x0000004C, 0x0000003C, 0x00000008, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000004D, 0x0000008C, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000004E, 0x00000065, 0x0000005D, 0x17FC00A7, 0x17FE00A7),
            new EcuAddressEntry(0x0000004F, 0x000000B1, 0x00000028, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000050, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC0095, 0x17FE0095),
            new EcuAddressEntry(0x00000051, 0xFFFFFFFF, 0xFFFFFFFF, 0x000007E6, 0x000007EE),
            new EcuAddressEntry(0x00000052, 0x000000AA, 0x00000023, 0x0000074B, 0x000007B5),
            new EcuAddressEntry(0x00000053, 0x0000002C, 0x00000019, 0x00000752, 0x000007BC),
            new EcuAddressEntry(0x00000054, 0x00000039, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000055, 0x00000070, 0x00000006, 0x00000754, 0x000007BE),
            new EcuAddressEntry(0x00000056, 0x00000080, 0x00000052, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000057, 0x00000082, 0x00000057, 0x0000076D, 0x000007D7),
            new EcuAddressEntry(0x00000058, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000059, 0x000000C3, 0x00000041, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000005A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000005B, 0xFFFFFFFF, 0x00000044, 0x17FC0088, 0x17FE0088),
            new EcuAddressEntry(0x0000005C, 0x0000006C, 0x0000001A, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000005D, 0x00000093, 0x0000004B, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000005E, 0x00000066, 0x0000005E, 0x17FC00A8, 0x17FE00A8),
            new EcuAddressEntry(0x0000005F, 0x0000008D, 0x0000004D, 0x00000773, 0x000007DD),
            new EcuAddressEntry(0x00000060, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000061, 0x00000042, 0x00000033, 0x17FC009C, 0x17FE009C),
            new EcuAddressEntry(0x00000062, 0x000000AB, 0x00000024, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000063, 0x000000AD, 0x00000046, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000064, 0x00000020, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000065, 0x0000003A, 0x00000029, 0x0000070B, 0x00000775),
            new EcuAddressEntry(0x00000066, 0x000000A3, 0x00000036, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000067, 0x00000083, 0x0000005C, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000068, 0x000000A6, 0x00000032, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000069, 0x00000041, 0x00000043, 0x00000747, 0x000007B1),
            new EcuAddressEntry(0x0000006A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000006B, 0xFFFFFFFF, 0x00000017, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000006C, 0x0000006A, 0x00000049, 0x00000769, 0x000007D3),
            new EcuAddressEntry(0x0000006D, 0x000000B2, 0x00000034, 0x00000723, 0x0000078D),
            new EcuAddressEntry(0x0000006E, 0x00000068, 0x0000003D, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000006F, 0x000000B3, 0x0000003C, 0x00000745, 0x000007AF),
            new EcuAddressEntry(0x00000070, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000071, 0x00000043, 0x0000000E, 0x0000071D, 0x00000787),
            new EcuAddressEntry(0x00000072, 0x000000AC, 0x00000025, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000073, 0x000000AE, 0x00000047, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000074, 0x00000034, 0x0000001B, 0x17FC0080, 0x17FE0080),
            new EcuAddressEntry(0x00000075, 0x00000090, 0x00000055, 0x00000767, 0x000007D1),
            new EcuAddressEntry(0x00000076, 0x00000060, 0x0000002D, 0x0000070A, 0x00000774),
            new EcuAddressEntry(0x00000077, 0x00000091, 0x0000005A, 0x0000076B, 0x000007D5),
            new EcuAddressEntry(0x00000078, 0x000000A7, 0x00000025, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000079, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000007A, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000007B, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000007C, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000007D, 0x0000009B, 0x0000002E, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000007E, 0x00000010, 0x00000048, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000007F, 0x0000008C, 0x0000004E, 0x0000075A, 0x000007C4),
            new EcuAddressEntry(0x00000080, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000081, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000753, 0x000007BD),
            new EcuAddressEntry(0x00000082, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000071B, 0x00000785),
            new EcuAddressEntry(0x00000083, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000084, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000727, 0x00000791),
            new EcuAddressEntry(0x00000085, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000726, 0x00000790),
            new EcuAddressEntry(0x00000086, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000087, 0xFFFFFFFF, 0xFFFFFFFF, 0x000006A2, 0x00000494),
            new EcuAddressEntry(0x00000088, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000735, 0x0000079F),
            new EcuAddressEntry(0x00000089, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000736, 0x000007A0),
            new EcuAddressEntry(0x0000008A, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000737, 0x000007A1),
            new EcuAddressEntry(0x0000008B, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000756, 0x000007C0),
            new EcuAddressEntry(0x0000008C, 0xFFFFFFFF, 0xFFFFFFFF, 0x000007E5, 0x000007ED),
            new EcuAddressEntry(0x0000008D, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000738, 0x000007A2),
            new EcuAddressEntry(0x0000008E, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000758, 0x000007C2),
            new EcuAddressEntry(0x0000008F, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000075E, 0x000007C8),
            new EcuAddressEntry(0x00000090, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000075F, 0x000007C9),
            new EcuAddressEntry(0x00000091, 0xFFFFFFFF, 0x00000003, 0x000007E0, 0x000007E8),
            new EcuAddressEntry(0x00000092, 0xFFFFFFFF, 0xFFFFFFFF, 0x000007E1, 0x000007E9),
            new EcuAddressEntry(0x00000093, 0xFFFFFFFF, 0x00000004, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x00000094, 0xFFFFFFFF, 0xFFFFFFFF, 0x000006BC, 0x000004FC),
            new EcuAddressEntry(0x00000095, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000784, 0x00000785),
            new EcuAddressEntry(0x00000096, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000796, 0x00000797),
            new EcuAddressEntry(0x00000097, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000079C, 0x0000079D),
            new EcuAddressEntry(0x00000098, 0xFFFFFFFF, 0xFFFFFFFF, 0x000006B8, 0x000004F8),
            new EcuAddressEntry(0x00000099, 0xFFFFFFFF, 0xFFFFFFFF, 0x000004E0, 0x000005FF),
            new EcuAddressEntry(0x0000009A, 0xFFFFFFFF, 0x00000005, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x0000009B, 0xFFFFFFFF, 0xFFFFFFFF, 0x000006C8, 0x000004E8),
            new EcuAddressEntry(0x0000009C, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000791, 0x000004F1),
            new EcuAddressEntry(0x0000009D, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000073B, 0x000004FB),
            new EcuAddressEntry(0x0000009E, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000072E, 0x000004EE),
            new EcuAddressEntry(0x0000009F, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000739, 0x000004F9),
            new EcuAddressEntry(0x000000A0, 0xFFFFFFFF, 0xFFFFFFFF, 0x000005D6, 0x000004F6),
            new EcuAddressEntry(0x000000A1, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000064A, 0x0000068A),
            new EcuAddressEntry(0x000000A2, 0xFFFFFFFF, 0xFFFFFFFF, 0x000005D6, 0x000004F6),
            new EcuAddressEntry(0x000000A3, 0xFFFFFFFF, 0xFFFFFFFF, 0x000006AA, 0x000004C8),
            new EcuAddressEntry(0x000000A4, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000A5, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000074F, 0x000007B9),
            new EcuAddressEntry(0x000000A6, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000763, 0x000007CD),
            new EcuAddressEntry(0x000000A7, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000749, 0x000007B3),
            new EcuAddressEntry(0x000000A8, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000A9, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000071C, 0x00000786),
            new EcuAddressEntry(0x000000AA, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000AB, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000AC, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000072A, 0x00000794),
            new EcuAddressEntry(0x000000AD, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000762, 0x000007CC),
            new EcuAddressEntry(0x000000AE, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000AF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000B0, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000667, 0x000004E7),
            new EcuAddressEntry(0x000000B1, 0xFFFFFFFF, 0xFFFFFFFF, 0x000006A5, 0x000004E5),
            new EcuAddressEntry(0x000000B2, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000733, 0x000004F3),
            new EcuAddressEntry(0x000000B3, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000730, 0x000004F0),
            new EcuAddressEntry(0x000000B4, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000662, 0x000004E2),
            new EcuAddressEntry(0x000000B5, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000726, 0x000004E6),
            new EcuAddressEntry(0x000000B6, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000792, 0x00000793),
            new EcuAddressEntry(0x000000B7, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000732, 0x0000079C),
            new EcuAddressEntry(0x000000B8, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000073D, 0x000007A7),
            new EcuAddressEntry(0x000000B9, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000073C, 0x000007A6),
            new EcuAddressEntry(0x000000BA, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000072E, 0x00000798),
            new EcuAddressEntry(0x000000BB, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000073E, 0x000007A8),
            new EcuAddressEntry(0x000000BC, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000073F, 0x000007A9),
            new EcuAddressEntry(0x000000BD, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000765, 0x000007CF),
            new EcuAddressEntry(0x000000BE, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000733, 0x0000079D),
            new EcuAddressEntry(0x000000BF, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000734, 0x0000079E),
            new EcuAddressEntry(0x000000C0, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000764, 0x000007CE),
            new EcuAddressEntry(0x000000C1, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000728, 0x00000799),
            new EcuAddressEntry(0x000000C2, 0xFFFFFFFF, 0xFFFFFFFF, 0x000007E3, 0x000007EB),
            new EcuAddressEntry(0x000000C3, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000C4, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000741, 0x000007AB),
            new EcuAddressEntry(0x000000C5, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000742, 0x000007AC),
            new EcuAddressEntry(0x000000C6, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000744, 0x000007AE),
            new EcuAddressEntry(0x000000C7, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000768, 0x000007D2),
            new EcuAddressEntry(0x000000C8, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000C9, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000CA, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC0084, 0x17FE0084),
            new EcuAddressEntry(0x000000CB, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000760, 0x000007CA),
            new EcuAddressEntry(0x000000CC, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000CD, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000CE, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000CF, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC008A, 0x17FE008A),
            new EcuAddressEntry(0x000000D0, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000759, 0x000007C3),
            new EcuAddressEntry(0x000000D1, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000D2, 0xFFFFFFFF, 0xFFFFFFFF, 0x00000749, 0x000004E9),
            new EcuAddressEntry(0x000000D3, 0xFFFFFFFF, 0xFFFFFFFF, 0x0000074B, 0x000004EB),
            new EcuAddressEntry(0x000000D4, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC0091, 0x17FE0091),
            new EcuAddressEntry(0x000000D5, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC0092, 0x17FE0092),
            new EcuAddressEntry(0x000000D6, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC0096, 0x17FE0096),
            new EcuAddressEntry(0x000000D7, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC0097, 0x17FE0097),
            new EcuAddressEntry(0x000000D8, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC160A, 0x17FE160A),
            new EcuAddressEntry(0x000000D9, 0xFFFFFFFF, 0xFFFFFFFF, 0x17FC160B, 0x17FE160B),
            new EcuAddressEntry(0x000000DA, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000DB, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000DC, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000DD, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000DE, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
            new EcuAddressEntry(0x000000DF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF),
        };

        private static Dictionary<uint, EcuAddressEntry> _addressGroupDict;

        public static EcuAddressEntry GetAddressEntry(uint address)
        {
            try
            {
                if (_addressGroupDict == null)
                {
                    _addressGroupDict = new Dictionary<uint, EcuAddressEntry>();
                    foreach (EcuAddressEntry ecuAddressEntry in EcuAddressArray)
                    {
                        if (!_addressGroupDict.ContainsKey(ecuAddressEntry.Address))
                        {
                            _addressGroupDict.Add(ecuAddressEntry.Address, ecuAddressEntry);
                        }
                    }
                }

                if (_addressGroupDict.TryGetValue(address, out EcuAddressEntry ecuAddressEntryMatch))
                {
                    if (ecuAddressEntryMatch.Tp20EcuAddr <= 0x7FF && ecuAddressEntryMatch.IsoTpTesterCanId <= 0x7FF)
                    {
                        return ecuAddressEntryMatch;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
    }
}
