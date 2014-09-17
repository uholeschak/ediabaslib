using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenNETCF.Net.NetworkInformation;
using OpenNETCF.IO;

namespace NetworkFunctions
{
    public class NetFunc
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetIfTable(IntPtr pIfTable, ref int pdwSize, bool bOrder);
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetIfTable(byte[] pIfTable, ref int pdwSize, bool bOrder);
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern int SetDevicePower(string pvDevice, int dwDeviceFlags, DevicePowerState DeviceState);
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern int GetDevicePower(string pvDevice, int dwDeviceFlags, ref DevicePowerState DeviceState);

        public enum DevicePowerState : int
        {
            Unspecified = -1,
            D0 = 0, // Full On: full power, full functionality 
            D1, // Low Power On: fully functional at low power/performance 
            D2, // Standby: partially powered with automatic wake 
            D3, // Sleep: partially powered with device initiated wake 
            D4, // Off: unpowered 
        }

        private const int POWER_NAME = 0x00000001;
        public const string regNdisPower = "Comm\\NdisPower";

        public sealed class NDIS : StreamInterfaceDriver
        {
            public const uint IOCTL_NDIS_BIND_ADAPTER = 0x00170032u;
            public const uint IOCTL_NDIS_REBIND_ADAPTER = 0x0017002Eu;
            public const uint IOCTL_NDIS_UNBIND_ADAPTER = 0x00170036u;
            private NDIS()
                : base("NDS0:")
            {
            }
            public static void BindInterface(string adapterName)
            {
                NDIS nDIS = new NDIS();
                nDIS.Open(System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                try
                {
                    byte[] bytes = Encoding.Unicode.GetBytes(adapterName + "\0\0");
                    nDIS.DeviceIoControl(IOCTL_NDIS_BIND_ADAPTER, bytes, null);
                }
                finally
                {
                    nDIS.Dispose();
                }
            }
            public static void UnbindInterface(string adapterName)
            {
                NDIS nDIS = new NDIS();
                nDIS.Open(System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                try
                {
                    byte[] bytes = Encoding.Unicode.GetBytes(adapterName + "\0\0");
                    nDIS.DeviceIoControl(IOCTL_NDIS_UNBIND_ADAPTER, bytes, null);
                }
                finally
                {
                    nDIS.Dispose();
                }
            }
            public static void RebindInterface(string adapterName)
            {
                NDIS nDIS = new NDIS();
                nDIS.Open(System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                try
                {
                    byte[] bytes = Encoding.Unicode.GetBytes(adapterName + "\0\0");
                    nDIS.DeviceIoControl(IOCTL_NDIS_REBIND_ADAPTER, bytes, null);
                }
                finally
                {
                    nDIS.Dispose();
                }
            }
        }

        public sealed class NDISPWR : StreamInterfaceDriver
        {
            public const uint IOCTL_NPW_SAVE_POWER_STATE = 0x00120800u;

            [StructLayout(LayoutKind.Sequential)]
            private struct NDISPWR_SAVEPOWERSTATE
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                public String pwcAdapterName;
                public DevicePowerState CePowerState;
            };

            private NDISPWR()
                : base("NPW1:")
            {
            }
            public static void SetPowerState(string adapterName, DevicePowerState powerState)
            {
                NDISPWR nDISPWR = new NDISPWR();
                nDISPWR.Open(System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                try
                {
                    NDISPWR_SAVEPOWERSTATE ndisPower;
                    ndisPower.pwcAdapterName = adapterName;
                    ndisPower.CePowerState = powerState;

                    int size = Marshal.SizeOf(ndisPower);
                    byte[] bytes = new byte[size];
                    IntPtr ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(ndisPower, ptr, true);
                    Marshal.Copy(ptr, bytes, 0, size);
                    Marshal.FreeHGlobal(ptr);

                    nDISPWR.DeviceIoControl(IOCTL_NPW_SAVE_POWER_STATE, bytes, null);
                }
                finally
                {
                    nDISPWR.Dispose();
                }
            }
        }

        internal sealed class MibIfRow
        {
            private const int MAX_INTERFACE_NAME_LEN = 512;
            private const int MAXLEN_IFDESCR = 256;
            private const int MAXLEN_PHYSADDR = 8;
            private const int NAME_OFFSET = 0;
            private const int NAME_LENGTH = 512;
            private const int INDEX_OFFSET = 512;
            private const int INDEX_LENGTH = 4;
            private const int TYPE_OFFSET = 516;
            private const int TYPE_LENGTH = 4;
            private const int MTU_OFFSET = 520;
            private const int MTU_LENGTH = 4;
            private const int SPEED_OFFSET = 524;
            private const int SPEED_LENGTH = 4;
            private const int PHYS_ADDR_LEN_OFFSET = 528;
            private const int PHYS_ADDR_LEN_LENGTH = 4;
            private const int PHYS_ADDR_OFFSET = 532;
            private const int PHYS_ADDR_LENGTH = 8;
            private const int ADMIN_STATUS_OFFSET = 540;
            private const int ADMIN_STATUS_LENGTH = 4;
            private const int OPER_STATUS_OFFSET = 544;
            private const int OPER_STATUS_LENGTH = 4;
            private const int LAST_CHANGE_OFFSET = 548;
            private const int LAST_CHANGE_LENGTH = 4;
            private const int IN_OCTETS_OFFSET = 552;
            private const int IN_OCTETS_LENGTH = 4;
            private const int IN_UCAST_OFFSET = 556;
            private const int IN_UCAST_LENGTH = 4;
            private const int IN_NUCAST_OFFSET = 560;
            private const int IN_NUCAST_LENGTH = 4;
            private const int IN_DISCARDS_OFFSET = 564;
            private const int IN_DISCARDS_LENGTH = 4;
            private const int IN_ERRORS_OFFSET = 568;
            private const int IN_ERRORS_LENGTH = 4;
            private const int IN_UNK_PROTOS_OFFSET = 572;
            private const int IN_UNK_PROTOS_LENGTH = 4;
            private const int OUT_OCTETS_OFFSET = 576;
            private const int OUT_OCTETS_LENGTH = 4;
            private const int OUT_UCAST_OFFSET = 580;
            private const int OUT_UCAST_LENGTH = 4;
            private const int OUT_NUCAST_OFFSET = 584;
            private const int OUT_NUCAST_LENGTH = 4;
            private const int OUT_DISCARDS_OFFSET = 588;
            private const int OUT_DISCARDS_LENGTH = 4;
            private const int OUT_ERRORS_OFFSET = 592;
            private const int OUT_ERRORS_LENGTH = 4;
            private const int OUT_QLEN_OFFSET = 596;
            private const int OUT_QLEN_LENGTH = 4;
            private const int DESC_LEN_OFFSET = 600;
            private const int DESC_LEN_LENGTH = 4;
            private const int DESC_OFFSET = 604;
            private const int DESC_LENGTH = 256;
            public const int Size = 860;
            private byte[] m_data;
            public string Name
            {
                get
                {
                    string @string = Encoding.Unicode.GetString(this.m_data, NAME_OFFSET, NAME_LENGTH);
                    return @string.Substring(0, @string.IndexOf('\0'));
                }
            }
            public int Index
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, INDEX_OFFSET);
                }
                internal set
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    Buffer.BlockCopy(bytes, NAME_OFFSET, this.m_data, INDEX_OFFSET, bytes.Length);
                }
            }
            public NetworkInterfaceType NetworkInterfaceType
            {
                get
                {
                    return (NetworkInterfaceType)BitConverter.ToInt32(this.m_data, TYPE_OFFSET);
                }
            }
            public int MTU
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, MTU_OFFSET);
                }
            }
            public int Speed
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, SPEED_OFFSET);
                }
            }
            private int PhysAddrLength
            {
                get
                {
                    int num = BitConverter.ToInt32(this.m_data, PHYS_ADDR_LEN_OFFSET);
                    if (num <= PHYS_ADDR_LENGTH)
                    {
                        return num;
                    }
                    return 0;
                }
            }
            public OperationalStatus OperationalStatus
            {
                get
                {
                    return (OperationalStatus)BitConverter.ToInt32(this.m_data, ADMIN_STATUS_OFFSET);
                }
            }
            public InterfaceOperationalStatus InterfaceOperationalStatus
            {
                get
                {
                    return (InterfaceOperationalStatus)BitConverter.ToInt32(this.m_data, OPER_STATUS_OFFSET);
                }
            }
            public uint LastChange
            {
                get
                {
                    return BitConverter.ToUInt32(this.m_data, LAST_CHANGE_OFFSET);
                }
            }
            public uint OctetsReceived
            {
                get
                {
                    return BitConverter.ToUInt32(this.m_data, IN_OCTETS_OFFSET);
                }
            }
            public int UnicastPacketsReceived
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, IN_UCAST_OFFSET);
                }
            }
            public int NonUnicastPacketsReceived
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, IN_NUCAST_OFFSET);
                }
            }
            public int DiscardedIncomingPackets
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, IN_DISCARDS_OFFSET);
                }
            }
            public int ErrorIncomingPackets
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, IN_ERRORS_OFFSET);
                }
            }
            public int UnknownIncomingPackets
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, IN_UNK_PROTOS_OFFSET);
                }
            }
            public uint OctetsSent
            {
                get
                {
                    return BitConverter.ToUInt32(this.m_data, OUT_OCTETS_OFFSET);
                }
            }
            public int UnicastPacketsSent
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, OUT_UCAST_OFFSET);
                }
            }
            public int NonUnicastPacketsSent
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, OUT_NUCAST_OFFSET);
                }
            }
            public int DiscardedOutgoingPackets
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, OUT_DISCARDS_OFFSET);
                }
            }
            public int ErrorOutgoingPackets
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, OUT_ERRORS_OFFSET);
                }
            }
            public int OutputQueueLength
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, OUT_QLEN_OFFSET);
                }
            }
            private int DescLength
            {
                get
                {
                    return BitConverter.ToInt32(this.m_data, DESC_LEN_OFFSET);
                }
            }
            public string Description
            {
                get
                {
                    if (this.DescLength == 0)
                    {
                        return "";
                    }
                    string arg_31_0 = Encoding.ASCII.GetString(this.m_data, DESC_OFFSET, this.DescLength);
                    char[] array = new char[1];
                    return arg_31_0.TrimEnd(array);
                }
            }
            public static implicit operator byte[](MibIfRow row)
            {
                return row.m_data;
            }
            internal MibIfRow()
            {
                this.m_data = new byte[Size];
            }
            internal unsafe MibIfRow(byte* pdata, int offset)
            {
                this.m_data = new byte[Size];
                Marshal.Copy(new IntPtr((void*)((int)pdata + offset)), this.m_data, 0, Size);
            }
        }

        public unsafe static List<string> GetEthernetNetworkInterfaces()
        {
            List<string> interfaces = new List<string>();
            int bytes = 0;
            GetIfTable(IntPtr.Zero, ref bytes, true);
            byte[] array = new byte[bytes];
            int ifTable = GetIfTable(array, ref bytes, true);
            if (ifTable != 0)
            {
                return interfaces;
            }
            fixed (byte* ptr = array)
            {
                byte* ptr2 = ptr;
                int position = 4;
                int count = *(int*)ptr2;
                for (int i = 0; i < count; i++)
                {
                    MibIfRow mibIfRow = new MibIfRow(ptr, position);
                    if (mibIfRow.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        interfaces.Add(mibIfRow.Description);
                    }
                    position += MibIfRow.Size;
                }
            }
            return interfaces;
        }

        public unsafe static List<string> GetDisabledNetworkInterfaces()
        {
            List<string> interfaces = new List<string>();
            try
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(regNdisPower);
                if (reg != null)
                {
                    string[] values = reg.GetValueNames();
                    if (values != null)
                    {
                        foreach (string value in values)
                        {
                            interfaces.Add(value);
                        }
                    }
                    reg.Close();
                }
            }
            catch
            {
            }
            return interfaces;
        }

        public static void SetDevicePower(string deviceName, DevicePowerState powerState)
        {
            SetDevicePower("{98C5250D-C29A-4985-AE5F-AFE5367E5006}\\" + deviceName, POWER_NAME, powerState);
        }
    }
}
