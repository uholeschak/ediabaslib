using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PowerFunctions
{
    class PowerFunc
    {
        [DllImport("coredll")]
        private static extern bool GetSystemPowerStatusEx(SYSTEM_POWER_STATUS_EX lpSystemPowerStatus,
            bool fUpdate);

        [DllImport("coredll")]
        private static extern int GetSystemPowerStatusEx2(SYSTEM_POWER_STATUS_EX2 lpSystemPowerStatus,
            uint dwLen, bool fUpdate);

        // ACLineStatus
        public const byte AC_LINE_OFFLINE = 0x00;
        public const byte AC_LINE_ONLINE = 0x01;
        public const byte AC_LINE_BACKUP_POWER = 0x02;
        public const byte AC_LINE_UNKNOWN = 0xFF;

        // BatteryFlag and BackupBatteryFlag
        public const byte BATTERY_FLAG_HIGH = 0x01;
        public const byte BATTERY_FLAG_LOW = 0x02;
        public const byte BATTERY_FLAG_CRITICAL = 0x04;
        public const byte BATTERY_FLAG_CHARGING = 0x08;
        public const byte BATTERY_FLAG_NO_BATTERY = 0x80;
        public const byte BATTERY_FLAG_UNKNOWN = 0xFF;

        // BatteryLifePercent and BackupBatteryLifePercent
        public const byte BATTERY_PERCENTAGE_UNKNOWN = 0xFF;

        // BatteryLifeTime, BatteryFullLifeTime, BackupBatteryLifeTime, and BackupBatteryFullLifeTime
        public const uint BATTERY_LIFE_UNKNOWN = 0xFFFFFFFF;

        // BatteryChemistry
        public const byte BATTERY_CHEMISTRY_ALKALINE = 0x01;
        public const byte BATTERY_CHEMISTRY_NICD = 0x02;
        public const byte BATTERY_CHEMISTRY_NIMH = 0x03;
        public const byte BATTERY_CHEMISTRY_LION = 0x04;
        public const byte BATTERY_CHEMISTRY_LIPOLY = 0x05;
        public const byte BATTERY_CHEMISTRY_UNKNOWN = 0xFF;

        [StructLayout(LayoutKind.Sequential)]
        public class SYSTEM_POWER_STATUS_EX2
        {
            public byte ACLineStatus = 0;
            public byte BatteryFlag = 0;
            public byte BatteryLifePercent = 0;
            public byte Reserved1 = 0;
            public int BatteryLifeTime = 0;
            public int BatteryFullLifeTime = 0;
            public byte Reserved2 = 0;
            public byte BackupBatteryFlag = 0;
            public byte BackupBatteryLifePercent = 0;
            public byte Reserved3 = 0;
            public int BackupBatteryLifeTime = 0;
            public int BackupBatteryFullLifeTime = 0;
            public int BatteryVoltage = 0;
            public int BatteryCurrent = 0;
            public int BatteryAverageCurrent = 0;
            public int BatteryAverageInterval = 0;
            public int BatterymAHourConsumed = 0;
            public int BatteryTemperature = 0;
            public int BackupBatteryVoltage = 0;
            public byte BatteryChemistry = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SYSTEM_POWER_STATUS_EX
        {
            public byte ACLineStatus = 0;
            public byte BatteryFlag = 0;
            public byte BatteryLifePercent = 0;
            public byte Reserved1 = 0;
            public uint BatteryLifeTime = 0;
            public uint BatteryFullLifeTime = 0;
            public byte Reserved2 = 0;
            public byte BackupBatteryFlag = 0;
            public byte BackupBatteryLifePercent = 0;
            public byte Reserved3 = 0;
            public uint BackupBatteryLifeTime = 0;
            public uint BackupBatteryFullLifeTime = 0;
        }

        public static bool GetBatteryInfo(out int lifePercent, out int current, out bool acLine)
        {
            lifePercent = -1;
            current = 0;
            acLine = false;
            SYSTEM_POWER_STATUS_EX2 status2 = new SYSTEM_POWER_STATUS_EX2();

            if (GetSystemPowerStatusEx2(status2,
                (uint)Marshal.SizeOf(status2), false) ==
                (uint)Marshal.SizeOf(status2))
            {
                if (BATTERY_PERCENTAGE_UNKNOWN != status2.BatteryLifePercent)
                {
                    lifePercent = status2.BatteryLifePercent;
                }
                current = status2.BatteryCurrent;
                acLine = status2.ACLineStatus == AC_LINE_ONLINE;
                return true;
            }
            return false;
        }
    }
}
