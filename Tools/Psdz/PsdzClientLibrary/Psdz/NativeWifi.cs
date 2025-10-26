using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BMW.Rheingold.Psdz
{
    public class NativeWifi
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_INTERFACE_INFO
        {
            public Guid InterfaceGuid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strInterfaceDescription;

            public WLAN_INTERFACE_STATE isState;
        }

        public struct WLAN_INTERFACE_INFO_LIST
        {
            public uint dwNumberOfItems;

            public uint dwIndex;

            public WLAN_INTERFACE_INFO[] InterfaceInfo;

            public WLAN_INTERFACE_INFO_LIST(IntPtr ppInterfaceList)
            {
                dwNumberOfItems = (uint)Marshal.ReadInt32(ppInterfaceList, 0);
                dwIndex = (uint)Marshal.ReadInt32(ppInterfaceList, 4);
                InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];
                for (int i = 0; i < dwNumberOfItems; i++)
                {
                    IntPtr ptr = new IntPtr(ppInterfaceList.ToInt64() + 8 + Marshal.SizeOf<WLAN_INTERFACE_INFO>() * i);
                    InterfaceInfo[i] = Marshal.PtrToStructure<WLAN_INTERFACE_INFO>(ptr);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_AVAILABLE_NETWORK
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strProfileName;

            public DOT11_SSID dot11Ssid;

            public DOT11_BSS_TYPE dot11BssType;

            public uint uNumberOfBssids;

            public bool bNetworkConnectable;

            public uint wlanNotConnectableReason;

            public uint uNumberOfPhyTypes;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public DOT11_PHY_TYPE[] dot11PhyTypes;

            public bool bMorePhyTypes;

            public uint wlanSignalQuality;

            public bool bSecurityEnabled;

            public DOT11_AUTH_ALGORITHM dot11DefaultAuthAlgorithm;

            public DOT11_CIPHER_ALGORITHM dot11DefaultCipherAlgorithm;

            public uint dwFlags;

            public uint dwReserved;
        }

        public struct WLAN_AVAILABLE_NETWORK_LIST
        {
            public uint dwNumberOfItems;

            public uint dwIndex;

            public WLAN_AVAILABLE_NETWORK[] Network;

            public WLAN_AVAILABLE_NETWORK_LIST(IntPtr ppAvailableNetworkList)
            {
                dwNumberOfItems = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 0);
                dwIndex = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 4);
                Network = new WLAN_AVAILABLE_NETWORK[dwNumberOfItems];
                for (int i = 0; i < dwNumberOfItems; i++)
                {
                    IntPtr ptr = new IntPtr(ppAvailableNetworkList.ToInt64() + 8 + Marshal.SizeOf<WLAN_AVAILABLE_NETWORK>() * i);
                    Network[i] = Marshal.PtrToStructure<WLAN_AVAILABLE_NETWORK>(ptr);
                }
            }
        }

        public struct DOT11_SSID
        {
            public uint uSSIDLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] ucSSID;

            private static Encoding _encoding = Encoding.GetEncoding(65001, EncoderFallback.ReplacementFallback, DecoderFallback.ExceptionFallback);

            public byte[] ToBytes()
            {
                return ucSSID?.Take((int)uSSIDLength).ToArray();
            }
        }

        public struct DOT11_MAC_ADDRESS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] ucDot11MacAddress;

            public byte[] ToBytes()
            {
                return ucDot11MacAddress?.ToArray();
            }

            public override string ToString()
            {
                if (ucDot11MacAddress == null)
                {
                    return null;
                }
                return BitConverter.ToString(ucDot11MacAddress).Replace('-', ':');
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_CONNECTION_ATTRIBUTES
        {
            public WLAN_INTERFACE_STATE isState;

            public WLAN_CONNECTION_MODE wlanConnectionMode;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strProfileName;

            public WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes;

            public WLAN_SECURITY_ATTRIBUTES wlanSecurityAttributes;
        }

        public struct WLAN_ASSOCIATION_ATTRIBUTES
        {
            public DOT11_SSID dot11Ssid;

            public DOT11_BSS_TYPE dot11BssType;

            public DOT11_MAC_ADDRESS dot11Bssid;

            public DOT11_PHY_TYPE dot11PhyType;

            public uint uDot11PhyIndex;

            public uint wlanSignalQuality;

            public uint ulRxRate;

            public uint ulTxRate;
        }

        public struct WLAN_SECURITY_ATTRIBUTES
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool bSecurityEnabled;

            [MarshalAs(UnmanagedType.Bool)]
            public bool bOneXEnabled;

            public DOT11_AUTH_ALGORITHM dot11AuthAlgorithm;

            public DOT11_CIPHER_ALGORITHM dot11CipherAlgorithm;
        }

        public enum WLAN_INTERFACE_STATE
        {
            wlan_interface_state_not_ready,
            wlan_interface_state_connected,
            wlan_interface_state_ad_hoc_network_formed,
            wlan_interface_state_disconnecting,
            wlan_interface_state_disconnected,
            wlan_interface_state_associating,
            wlan_interface_state_discovering,
            wlan_interface_state_authenticating
        }

        public enum WLAN_CONNECTION_MODE
        {
            wlan_connection_mode_profile,
            wlan_connection_mode_temporary_profile,
            wlan_connection_mode_discovery_secure,
            wlan_connection_mode_discovery_unsecure,
            wlan_connection_mode_auto,
            wlan_connection_mode_invalid
        }

        public enum DOT11_BSS_TYPE
        {
            dot11_BSS_type_infrastructure = 1,
            dot11_BSS_type_independent,
            dot11_BSS_type_any
        }

        public enum DOT11_PHY_TYPE : uint
        {
            dot11_phy_type_unknown = 0u,
            dot11_phy_type_any = 0u,
            dot11_phy_type_fhss = 1u,
            dot11_phy_type_dsss = 2u,
            dot11_phy_type_irbaseband = 3u,
            dot11_phy_type_ofdm = 4u,
            dot11_phy_type_hrdsss = 5u,
            dot11_phy_type_erp = 6u,
            dot11_phy_type_ht = 7u,
            dot11_phy_type_vht = 8u,
            dot11_phy_type_IHV_start = 2147483648u,
            dot11_phy_type_IHV_end = uint.MaxValue
        }

        public enum DOT11_AUTH_ALGORITHM : uint
        {
            DOT11_AUTH_ALGO_80211_OPEN = 1u,
            DOT11_AUTH_ALGO_80211_SHARED_KEY = 2u,
            DOT11_AUTH_ALGO_WPA = 3u,
            DOT11_AUTH_ALGO_WPA_PSK = 4u,
            DOT11_AUTH_ALGO_WPA_NONE = 5u,
            DOT11_AUTH_ALGO_RSNA = 6u,
            DOT11_AUTH_ALGO_RSNA_PSK = 7u,
            DOT11_AUTH_ALGO_IHV_START = 2147483648u,
            DOT11_AUTH_ALGO_IHV_END = uint.MaxValue
        }

        public enum DOT11_CIPHER_ALGORITHM : uint
        {
            DOT11_CIPHER_ALGO_NONE = 0u,
            DOT11_CIPHER_ALGO_WEP40 = 1u,
            DOT11_CIPHER_ALGO_TKIP = 2u,
            DOT11_CIPHER_ALGO_CCMP = 4u,
            DOT11_CIPHER_ALGO_WEP104 = 5u,
            DOT11_CIPHER_ALGO_WPA_USE_GROUP = 256u,
            DOT11_CIPHER_ALGO_RSN_USE_GROUP = 256u,
            DOT11_CIPHER_ALGO_WEP = 257u,
            DOT11_CIPHER_ALGO_IHV_START = 2147483648u,
            DOT11_CIPHER_ALGO_IHV_END = uint.MaxValue
        }

        public enum WLAN_INTF_OPCODE : uint
        {
            wlan_intf_opcode_autoconf_start = 0u,
            wlan_intf_opcode_autoconf_enabled = 1u,
            wlan_intf_opcode_background_scan_enabled = 2u,
            wlan_intf_opcode_media_streaming_mode = 3u,
            wlan_intf_opcode_radio_state = 4u,
            wlan_intf_opcode_bss_type = 5u,
            wlan_intf_opcode_interface_state = 6u,
            wlan_intf_opcode_current_connection = 7u,
            wlan_intf_opcode_channel_number = 8u,
            wlan_intf_opcode_supported_infrastructure_auth_cipher_pairs = 9u,
            wlan_intf_opcode_supported_adhoc_auth_cipher_pairs = 10u,
            wlan_intf_opcode_supported_country_or_region_string_list = 11u,
            wlan_intf_opcode_current_operation_mode = 12u,
            wlan_intf_opcode_supported_safe_mode = 13u,
            wlan_intf_opcode_certified_safe_mode = 14u,
            wlan_intf_opcode_hosted_network_capable = 15u,
            wlan_intf_opcode_management_frame_protection_capable = 16u,
            wlan_intf_opcode_autoconf_end = 268435455u,
            wlan_intf_opcode_msm_start = 268435712u,
            wlan_intf_opcode_statistics = 268435713u,
            wlan_intf_opcode_rssi = 268435714u,
            wlan_intf_opcode_msm_end = 536870911u,
            wlan_intf_opcode_security_start = 536936448u,
            wlan_intf_opcode_security_end = 805306367u,
            wlan_intf_opcode_ihv_start = 805306368u,
            wlan_intf_opcode_ihv_end = 1073741823u
        }

        public const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_ADHOC_PROFILES = 1u;

        public const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES = 2u;

        public const uint ERROR_SUCCESS = 0u;

        public static string WLANSignalStrength = string.Empty;

        public const string NO_WLAN_SIGNAL = "No signal for WLAN connection";

        [DllImport("Wlanapi.dll")]
        public static extern uint WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, out uint pdwNegotiatedVersion, out IntPtr phClientHandle);

        [DllImport("Wlanapi.dll")]
        public static extern uint WlanCloseHandle(IntPtr hClientHandle, IntPtr pReserved);

        [DllImport("Wlanapi.dll")]
        public static extern void WlanFreeMemory(IntPtr pMemory);

        [DllImport("Wlanapi.dll")]
        public static extern uint WlanEnumInterfaces(IntPtr hClientHandle, IntPtr pReserved, out IntPtr ppInterfaceList);

        [DllImport("Wlanapi.dll")]
        public static extern uint WlanQueryInterface(IntPtr hClientHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid, WLAN_INTF_OPCODE OpCode, IntPtr pReserved, out uint pdwDataSize, ref IntPtr ppData, IntPtr pWlanOpcodeValueType);

        public static string GetConnectedSignalStrength()
        {
            List<string> list = new List<string>();
            IntPtr phClientHandle = IntPtr.Zero;
            IntPtr ppInterfaceList = IntPtr.Zero;
            IntPtr ppData = IntPtr.Zero;
            try
            {
                if (WlanOpenHandle(2u, IntPtr.Zero, out var _, out phClientHandle) != 0)
                {
                    return null;
                }
                Log.Info("GetConnectedNetworkSsids()", "More than one connection: ", list);
                if (WlanEnumInterfaces(phClientHandle, IntPtr.Zero, out ppInterfaceList) != 0)
                {
                    return null;
                }
                Log.Info("GetConnectedNetworkSsids()", "More than one connection: ", list);
                WLAN_INTERFACE_INFO[] interfaceInfo = new WLAN_INTERFACE_INFO_LIST(ppInterfaceList).InterfaceInfo;
                for (int i = 0; i < interfaceInfo.Length; i++)
                {
                    WLAN_INTERFACE_INFO wLAN_INTERFACE_INFO = interfaceInfo[i];
                    if (WlanQueryInterface(phClientHandle, wLAN_INTERFACE_INFO.InterfaceGuid, WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection, IntPtr.Zero, out var _, ref ppData, IntPtr.Zero) == 0)
                    {
                        WLAN_CONNECTION_ATTRIBUTES wLAN_CONNECTION_ATTRIBUTES = Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(ppData);
                        if (wLAN_CONNECTION_ATTRIBUTES.isState == WLAN_INTERFACE_STATE.wlan_interface_state_connected)
                        {
                            WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes = wLAN_CONNECTION_ATTRIBUTES.wlanAssociationAttributes;
                            list.Add(wlanAssociationAttributes.wlanSignalQuality.ToString());
                        }
                    }
                }
            }
            finally
            {
                if (ppData != IntPtr.Zero)
                {
                    WlanFreeMemory(ppData);
                }
                if (ppInterfaceList != IntPtr.Zero)
                {
                    WlanFreeMemory(ppInterfaceList);
                }
                if (phClientHandle != IntPtr.Zero)
                {
                    WlanCloseHandle(phClientHandle, IntPtr.Zero);
                }
            }
            if (list.Count > 1)
            {
                Log.Info("GetConnectedNetworkSsids()", "More than one connection: ", list);
            }
            else if (list.Count == 0)
            {
                Log.Info("GetConnectedNetworkSsids()", "No internet connection: ", list);
            }
            return list.FirstOrDefault();
        }
    }
}