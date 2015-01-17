#region references

using System;
using System.Text;
using System.Runtime.InteropServices;

#endregion

namespace NativeUsbLib
{
    public class UsbApi
    {
        #region "WinAPI"

        // *******************************************************************************************
        // *************************************** constants *****************************************
        // *******************************************************************************************

        #region constants

        public const uint GENERIC_READ = 0x80000000; 
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_ALL = 0x10000000;
        public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;

        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const int OPEN_EXISTING = 0x3;
        public const int INVALID_HANDLE_VALUE = -1;

        public const int USBUSER_GET_CONTROLLER_INFO_0 = 0x00000001;
        public const int USBUSER_GET_CONTROLLER_DRIVER_KEY = 0x00000002;

        public const int IOCTL_GET_HCD_DRIVERKEY_NAME = 0x220424;
        public const int IOCTL_USB_GET_ROOT_HUB_NAME = 0x220408;
        public const int IOCTL_USB_GET_NODE_INFORMATION = 0x220408;
        public const int IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX = 0x220448;
        public const int IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 0x220410;
        public const int IOCTL_USB_GET_NODE_CONNECTION_NAME = 0x220414;
        public const int IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME = 0x220420;
        public const int IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x2D1080;

        public const int USB_DEVICE_DESCRIPTOR_TYPE = 0x1;
        public const int USB_CONFIGURATION_DESCRIPTOR_TYPE = 0x2;
        public const int USB_STRING_DESCRIPTOR_TYPE = 0x3;
        public const int USB_INTERFACE_DESCRIPTOR_TYPE = 0x4;
        public const int USB_ENDPOINT_DESCRIPTOR_TYPE = 0x5;

        public const string GUID_DEVINTERFACE_HUBCONTROLLER = "3abf6f2d-71c4-462a-8a92-1e6861e6af27";
        public const int MAX_BUFFER_SIZE = 2048;
        public const int MAXIMUM_USB_STRING_LENGTH = 255;
        public const string REGSTR_KEY_USB = "USB";
        public const int REG_SZ = 1;
        public const int DIF_PROPERTYCHANGE = 0x00000012;
        public const int DICS_FLAG_GLOBAL = 0x00000001;

        public const int DIGCF_DEFAULT = 0x00000001;  // only valid with DIGCF_DEVICEINTERFACE
        public const int DIGCF_PRESENT = 0x00000002;
        public const int DIGCF_ALLCLASSES = 0x00000004;
        public const int DIGCF_PROFILE = 0x00000008;
        public const int DIGCF_DEVICEINTERFACE = 0x00000010;
 
        public const int SPDRP_DRIVER = 0x9;
        public const int SPDRP_DEVICEDESC = 0x0;

        public const int DICS_ENABLE = 0x00000001;
        public const int DICS_DISABLE = 0x00000002;

        #endregion

        // *******************************************************************************************
        // ************************************* enumerations ****************************************
        // *******************************************************************************************

        #region enumerations

        public enum UsbDeviceClass : byte
        {
            UnspecifiedDevice = 0x00,
            AudioInterface = 0x01,
            CommunicationsAndCDCControlBoth = 0x02,
            HIDInterface = 0x03,
            PhysicalInterfaceDevice = 0x5,
            ImageInterface = 0x06,
            PrinterInterface = 0x07,
            MassStorageInterface = 0x08,
            HubDevice = 0x09,
            CDCDataInterface = 0x0A,
            SmartCardInterface = 0x0B,
            ContentSecurityInterface = 0x0D,
            VidioInterface = 0x0E,
            PersonalHeathcareInterface = 0x0F,
            DiagnosticDeviceBoth = 0xDC,
            WirelessControllerInterface = 0xE0,
            MiscellaneousBoth = 0xEF,
            ApplicationSpecificInterface = 0xFE,
            VendorSpecificBoth = 0xFF
        }

        public enum HubCharacteristics : byte
        {
            GangedPowerSwitching = 0x00,
            IndividualPotPowerSwitching = 0x01,
            // to do

        }

        //original winapi USB_HUB_NODE enumeration
        //typedef enum _USB_HUB_NODE
        //{
        //    UsbHub,
        //    UsbMIParent
        //} USB_HUB_NODE;

        public enum USB_HUB_NODE
        {
            UsbHub,
            UsbMIParent
        }

        public enum USB_DESCRIPTOR_TYPE : byte
        {
            DeviceDescriptorType = 0x1,
            ConfigurationDescriptorType = 0x2,
            StringDescriptorType = 0x3,
            InterfaceDescriptorType = 0x4,
            EndpointDescriptorType = 0x5,
            HubDescriptor = 0x29
        }

        public enum USB_CONFIGURATION : byte
        {
            RemoteWakeUp = 32,
            SelfPowered = 64,
            BusPowered = 128,
            RemoteWakeUp_BusPowered = 160,
            RemoteWakeUp_SelfPowered = 96
        }

        public enum USB_TRANSFER : byte
        {
            Control = 0x0,
            Isochronous = 0x1,
            Bulk = 0x2,
            Interrupt = 0x3
        }

        //original winapi USB_CONNECTION_STATUS enumeration
        //typedef enum _USB_CONNECTION_STATUS
        //{
        //    NoDeviceConnected,
        //    DeviceConnected,
        //    DeviceFailedEnumeration,
        //    DeviceGeneralFailure,
        //    DeviceCausedOvercurrent,
        //    DeviceNotEnoughPower,
        //    DeviceNotEnoughBandwidth,
        //    DeviceHubNestedTooDeeply,
        //    DeviceInLegacyHub
        //} USB_CONNECTION_STATUS, *PUSB_CONNECTION_STATUS;

        public enum USB_CONNECTION_STATUS : int
        {
            NoDeviceConnected,
            DeviceConnected,
            DeviceFailedEnumeration,
            DeviceGeneralFailure,
            DeviceCausedOvercurrent,
            DeviceNotEnoughPower,
            DeviceNotEnoughBandwidth,
            DeviceHubNestedTooDeeply,
            DeviceInLegacyHub
        }

        //original winapi USB_DEVICE_SPEED enumeration
        //typedef enum _USB_DEVICE_SPEED
        //{
        //    UsbLowSpeed = 0,
        //    UsbFullSpeed,
        //    UsbHighSpeed
        //} USB_DEVICE_SPEED;

        public enum USB_DEVICE_SPEED : byte
        {
            UsbLowSpeed,
            UsbFullSpeed,
            UsbHighSpeed
        }

        public enum DeviceInterfaceDataFlags : uint
        {
            Unknown = 0x00000000,
            Active = 0x00000001,
            Default = 0x00000002,
            Removed = 0x00000004
        }

        public enum HubPortStatus : short
        {
            Connection = 0x0001,
            Enabled = 0x0002,
            Suspend = 0x0004,
            OverCurrent = 0x0008,
            BeingReset = 0x0010,
            Power = 0x0100,
            LowSpeed = 0x0200,
            HighSpeed = 0x0400,
            TestMode = 0x0800,
            Indicator = 0x1000, 
        // these are the bits which cause the hub port state machine to keep moving 
        //kHubPortStateChangeMask = kHubPortConnection | kHubPortEnabled | kHubPortSuspend | kHubPortOverCurrent | kHubPortBeingReset 
        }

        public enum HubStatus : byte
        {
            LocalPowerStatus = 1,
            OverCurrentIndicator = 2,
            LocalPowerStatusChange = 1,
            OverCurrentIndicatorChange = 2
        }

        public enum PortIndicatorSlectors : byte
        { 
            IndicatorAutomatic = 0,
            IndicatorAmber,
            IndicatorGreen,
            IndicatorOff
        }

        public enum PowerSwitching : byte
        {
            SupportsGangPower = 0,
            SupportsIndividualPortPower = 1,
            SetPowerOff = 0,
            SetPowerOn = 1
        }

        /// <summary>
        /// Device registry property codes
        /// </summary>
        public enum SPDRP : int
        {
            /// <summary>
            /// DeviceDesc (R/W)
            /// </summary>
            SPDRP_DEVICEDESC = 0x00000000,

            /// <summary>
            /// HardwareID (R/W)
            /// </summary>
            SPDRP_HARDWAREID = 0x00000001,

            /// <summary>
            /// CompatibleIDs (R/W)
            /// </summary>
            SPDRP_COMPATIBLEIDS = 0x00000002,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED0 = 0x00000003,

            /// <summary>
            /// Service (R/W)
            /// </summary>
            SPDRP_SERVICE = 0x00000004,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED1 = 0x00000005,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED2 = 0x00000006,

            /// <summary>
            /// Class (R--tied to ClassGUID)
            /// </summary>
            SPDRP_CLASS = 0x00000007,

            /// <summary>
            /// ClassGUID (R/W)
            /// </summary>
            SPDRP_CLASSGUID = 0x00000008,

            /// <summary>
            /// Driver (R/W)
            /// </summary>
            SPDRP_DRIVER = 0x00000009,

            /// <summary>
            /// ConfigFlags (R/W)
            /// </summary>
            SPDRP_CONFIGFLAGS = 0x0000000A,

            /// <summary>
            /// Mfg (R/W)
            /// </summary>
            SPDRP_MFG = 0x0000000B,

            /// <summary>
            /// FriendlyName (R/W)
            /// </summary>
            SPDRP_FRIENDLYNAME = 0x0000000C,

            /// <summary>
            /// LocationInformation (R/W)
            /// </summary>
            SPDRP_LOCATION_INFORMATION = 0x0000000D,

            /// <summary>
            /// PhysicalDeviceObjectName (R)
            /// </summary>
            SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,

            /// <summary>
            /// Capabilities (R)
            /// </summary>
            SPDRP_CAPABILITIES = 0x0000000F,

            /// <summary>
            /// UiNumber (R)
            /// </summary>
            SPDRP_UI_NUMBER = 0x00000010,

            /// <summary>
            /// UpperFilters (R/W)
            /// </summary>
            SPDRP_UPPERFILTERS = 0x00000011,

            /// <summary>
            /// LowerFilters (R/W)
            /// </summary>
            SPDRP_LOWERFILTERS = 0x00000012,

            /// <summary>
            /// BusTypeGUID (R)
            /// </summary>
            SPDRP_BUSTYPEGUID = 0x00000013,

            /// <summary>
            /// LegacyBusType (R)
            /// </summary>
            SPDRP_LEGACYBUSTYPE = 0x00000014,

            /// <summary>
            /// BusNumber (R)
            /// </summary>
            SPDRP_BUSNUMBER = 0x00000015,

            /// <summary>
            /// Enumerator Name (R)
            /// </summary>
            SPDRP_ENUMERATOR_NAME = 0x00000016,

            /// <summary>
            /// Security (R/W, binary form)
            /// </summary>
            SPDRP_SECURITY = 0x00000017,

            /// <summary>
            /// Security (W, SDS form)
            /// </summary>
            SPDRP_SECURITY_SDS = 0x00000018,

            /// <summary>
            /// Device Type (R/W)
            /// </summary>
            SPDRP_DEVTYPE = 0x00000019,

            /// <summary>
            /// Device is exclusive-access (R/W)
            /// </summary>
            SPDRP_EXCLUSIVE = 0x0000001A,

            /// <summary>
            /// Device Characteristics (R/W)
            /// </summary>
            SPDRP_CHARACTERISTICS = 0x0000001B,

            /// <summary>
            /// Device Address (R)
            /// </summary>
            SPDRP_ADDRESS = 0x0000001C,

            /// <summary>
            /// UiNumberDescFormat (R/W)
            /// </summary>
            SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D,

            /// <summary>
            /// Device Power Data (R)
            /// </summary>
            SPDRP_DEVICE_POWER_DATA = 0x0000001E,

            /// <summary>
            /// Removal Policy (R)
            /// </summary>
            SPDRP_REMOVAL_POLICY = 0x0000001F,

            /// <summary>
            /// Hardware Removal Policy (R)
            /// </summary>
            SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020,

            /// <summary>
            /// Removal Policy Override (RW)
            /// </summary>
            SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021,

            /// <summary>
            /// Device Install State (R)
            /// </summary>
            SPDRP_INSTALL_STATE = 0x00000022,

            /// <summary>
            /// Device Location Paths (R)
            /// </summary>
            SPDRP_LOCATION_PATHS = 0x00000023,
        }

        #endregion

        // *******************************************************************************************
        // *************************************** stuctures *****************************************
        // *******************************************************************************************

        #region structures

        //original winapi SP_CLASSINSTALL_HEADER structure
        //typedef struct _SP_CLASSINSTALL_HEADER
        //{
        //  DWORD  cbSize;
        //  DI_FUNCTION  InstallFunction;
        //} SP_CLASSINSTALL_HEADER, *PSP_CLASSINSTALL_HEADER;

        [StructLayout( LayoutKind.Sequential )]
        public struct SP_CLASSINSTALL_HEADER
        {
            public int cbSize;
            public int InstallFunction;
        }

        //original winapi SP_PROPCHANGE_PARAMS structure
        //typedef struct _SP_PROPCHANGE_PARAMS
        //{
        //  SP_CLASSINSTALL_HEADER  ClassInstallHeader;
        //  DWORD  StateChange;
        //  DWORD  Scope;
        //  DWORD  HwProfile;
        //} SP_PROPCHANGE_PARAMS, *PSP_PROPCHANGE_PARAMS;

        [StructLayout( LayoutKind.Sequential )]
        public struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public int StateChange;
            public int Scope;
            public int HwProfile;
            public void Init()
            {
                ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
            }
        }

        //original winapi SP_DEVINFO_DATA structure
        //typedef struct _SP_DEVINFO_DATA
        //{
        //  DWORD cbSize;
        //  GUID ClassGuid;
        //  DWORD DevInst;
        //  ULONG_PTR Reserved;
        //} SP_DEVINFO_DATA,  *PSP_DEVINFO_DATA;
        
        [StructLayout( LayoutKind.Sequential )]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public Int32 DevInst;
            public IntPtr Reserved;
        }

        //original winapi SP_DEVICE_INTERFACE_DATA structure
        //typedef struct _SP_DEVICE_INTERFACE_DATA
        //{
        //  DWORD cbSize;
        //  GUID InterfaceClassGuid;
        //  DWORD Flags;
        //  ULONG_PTR Reserved;
        //} SP_DEVICE_INTERFACE_DATA,  *PSP_DEVICE_INTERFACE_DATA;
        
        [StructLayout( LayoutKind.Sequential )]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public DeviceInterfaceDataFlags Flags;
            public IntPtr Reserved;
        }

        //original winapi SP_DEVICE_INTERFACE_DETAIL_DATA structure
        //typedef struct _SP_DEVICE_INTERFACE_DETAIL_DATA
        //{
        //  DWORD cbSize;
        //  TCHAR DevicePath[ANYSIZE_ARRAY];
        //} SP_DEVICE_INTERFACE_DETAIL_DATA,  *PSP_DEVICE_INTERFACE_DETAIL_DATA;
        
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE )]
            public string DevicePath;
        }

        //original winapi USB_HCD_DRIVERKEY_NAME structure
        //typedef struct _USB_HCD_DRIVERKEY_NAME
        //{
        //    ULONG ActualLength;
        //    WCHAR DriverKeyName[1];
        //} USB_HCD_DRIVERKEY_NAME, *PUSB_HCD_DRIVERKEY_NAME;
        
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct USB_HCD_DRIVERKEY_NAME
        {
            public int ActualLength;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE )]
            public string DriverKeyName;
        }

        //original winapi USB_ROOT_HUB_NAME structrue
        //typedef struct _USB_ROOT_HUB_NAME
        //{
        //    ULONG  ActualLength;
        //    WCHAR  RootHubName[1];
        //} USB_ROOT_HUB_NAME, *PUSB_ROOT_HUB_NAME;
        
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct USB_ROOT_HUB_NAME
        {
            public int ActualLength;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE )]
            public string RootHubName;
        }

        //original winapi USB_HUB_DESCRIPTOR structure
        //typedef struct _USB_HUB_DESCRIPTOR
        //{
        //    UCHAR  bDescriptorLength;
        //    UCHAR  bDescriptorType;
        //    UCHAR  bNumberOfPorts;
        //    USHORT  wHubCharacteristics;
        //    UCHAR  bPowerOnToPowerGood;
        //    UCHAR  bHubControlCurrent;
        //    UCHAR  bRemoveAndPowerMask[64];
        //} USB_HUB_DESCRIPTOR, *PUSB_HUB_DESCRIPTOR;
        
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        public struct USB_HUB_DESCRIPTOR
        {
            public byte bDescriptorLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public byte bNumberOfPorts;
            public short wHubCharacteristics;
            public byte bPowerOnToPowerGood;
            public byte bHubControlCurrent;
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 )]
            public byte[] bRemoveAndPowerMask;
        }

        //original winapi USB_HUB_INFORMATION structrure
        //typedef struct _USB_HUB_INFORMATION
        //{
        //    USB_HUB_DESCRIPTOR HubDescriptor;
        //    BOOLEAN HubIsBusPowered;
        //} USB_HUB_INFORMATION, *PUSB_HUB_INFORMATION;
        
        [StructLayout( LayoutKind.Sequential )]
        public struct USB_HUB_INFORMATION
        {
            public USB_HUB_DESCRIPTOR HubDescriptor;
            public bool HubIsBusPowered;
        }

        //original winapi USB_NODE_INFORMATION structure
        //typedef struct _USB_NODE_INFORMATION
        //{
        //    USB_HUB_NODE  NodeType;
        //    union
        //    {
        //        USB_HUB_INFORMATION  HubInformation;
        //        USB_MI_PARENT_INFORMATION  MiParentInformation;
        //    } u;
        //} USB_NODE_INFORMATION, *PUSB_NODE_INFORMATION;
        
        [StructLayout( LayoutKind.Sequential )]
        public struct USB_NODE_INFORMATION
        {
            //public int NodeType;
            public USB_HUB_NODE NodeType;
            public USB_HUB_INFORMATION HubInformation;
        }

        //original winapi USB_NODE_CONNECTION_INFORMATION_EX structrue
        //typedef struct _USB_NODE_CONNECTION_INFORMATION_EX
        //{
        //    ULONG  ConnectionIndex;
        //    USB_DEVICE_DESCRIPTOR  DeviceDescriptor;
        //    UCHAR  CurrentConfigurationValue;
        //    UCHAR  Speed;
        //    BOOLEAN  DeviceIsHub;
        //    USHORT  DeviceAddress;
        //    ULONG  NumberOfOpenPipes;
        //    USB_CONNECTION_STATUS  ConnectionStatus;
        //    USB_PIPE_INFO  PipeList[0];
        //} USB_NODE_CONNECTION_INFORMATION_EX, *PUSB_NODE_CONNECTION_INFORMATION_EX;
        
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        public struct USB_NODE_CONNECTION_INFORMATION_EX
        {
            public int ConnectionIndex;
            public USB_DEVICE_DESCRIPTOR DeviceDescriptor;
            public byte CurrentConfigurationValue;
            public USB_DEVICE_SPEED Speed;
            public byte DeviceIsHub;
            public short DeviceAddress;
            public int NumberOfOpenPipes;
            public USB_CONNECTION_STATUS ConnectionStatus;
            //public IntPtr PipeList;
        }

        //original winapi USB_DEVICE_DESCRIPTOR structrure
        //typedef struct _USB_DEVICE_DESCRIPTOR
        //{
        //    UCHAR  bLength;
        //    UCHAR  bDescriptorType;
        //    USHORT  bcdUSB;
        //    UCHAR  bDeviceClass;
        //    UCHAR  bDeviceSubClass;
        //    UCHAR  bDeviceProtocol;
        //    UCHAR  bMaxPacketSize0;
        //    USHORT  idVendor;
        //    USHORT  idProduct;
        //    USHORT  bcdDevice;
        //    UCHAR  iManufacturer;
        //    UCHAR  iProduct;
        //    UCHAR  iSerialNumber;
        //    UCHAR  bNumConfigurations;
        //} USB_DEVICE_DESCRIPTOR, *PUSB_DEVICE_DESCRIPTOR ;
        
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        public class USB_DEVICE_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public short bcdUSB;
            public UsbDeviceClass bDeviceClass;
            public byte bDeviceSubClass;
            public byte bDeviceProtocol;
            public byte bMaxPacketSize0;
            public ushort idVendor;
            public ushort idProduct;
            public short bcdDevice;
            public byte iManufacturer;
            public byte iProduct;
            public byte iSerialNumber;
            public byte bNumConfigurations;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct USB_ENDPOINT_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public byte bEndpointAddress;
            public USB_TRANSFER bmAttributes;
            public short wMaxPacketSize;
            public byte bInterval;
        }


        //original winapi USB_STRING_DESCRIPTOR structrue
        //typedef struct _USB_STRING_DESCRIPTOR
        //{
        //    UCHAR bLength;
        //    UCHAR bDescriptorType;
        //    WCHAR bString[1];
        //} USB_STRING_DESCRIPTOR, *PUSB_STRING_DESCRIPTOR;
        
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct USB_STRING_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH )]
            public string bString;
        }

        //original winapi USB_DESCRIPTOR_REQUEST structrue
        //typedef struct _USB_DESCRIPTOR_REQUEST
        //{
        //  ULONG ConnectionIndex;
        //  struct
        //  {
        //    UCHAR  bmRequest;
        //    UCHAR  bRequest;
        //    USHORT  wValue;
        //    USHORT  wIndex;
        //    USHORT  wLength;
        //  } SetupPacket;
        //  UCHAR  Data[0];
        //} USB_DESCRIPTOR_REQUEST, *PUSB_DESCRIPTOR_REQUEST
        
        [StructLayout( LayoutKind.Sequential )]
        public struct USB_SETUP_PACKET
        {
            public byte bmRequest;
            public byte bRequest;
            public short wValue;
            public short wIndex;
            public short wLength;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct USB_DESCRIPTOR_REQUEST
        {
            public int ConnectionIndex;
            public USB_SETUP_PACKET SetupPacket;
            //public byte[] Data;
        }

        //original winapi USB_NODE_CONNECTION_NAME structure
        //typedef struct _USB_NODE_CONNECTION_NAME
        //{
        //    ULONG  ConnectionIndex;
        //    ULONG  ActualLength;
        //    WCHAR  NodeName[1];
        //} USB_NODE_CONNECTION_NAME, *PUSB_NODE_CONNECTION_NAME;
        
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct USB_NODE_CONNECTION_NAME
        {
            public int ConnectionIndex;
            public int ActualLength;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE )]
            public string NodeName;
        }

        //original winapi USB_NODE_CONNECTION_DRIVERKEY_NAME structrue
        //typedef struct _USB_NODE_CONNECTION_DRIVERKEY_NAME
        //{
        //    ULONG  ConnectionIndex;
        //    ULONG  ActualLength;
        //    WCHAR  DriverKeyName[1];
        //} USB_NODE_CONNECTION_DRIVERKEY_NAME, *PUSB_NODE_CONNECTION_DRIVERKEY_NAME;
        
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
        public struct USB_NODE_CONNECTION_DRIVERKEY_NAME               // Yes, this is the same as the structure above...
        {
            public int ConnectionIndex;
            public int ActualLength;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE )]
            public string DriverKeyName;
        }

        //typedef struct _STORAGE_DEVICE_NUMBER
        //{
        //  DEVICE_TYPE  DeviceType;
        //  ULONG  DeviceNumber;
        //  ULONG  PartitionNumber;
        //} STORAGE_DEVICE_NUMBER, *PSTORAGE_DEVICE_NUMBER;

        [StructLayout( LayoutKind.Sequential )]
        public struct STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        }


        //typedef struct _USB_INTERFACE_DESCRIPTOR { 
        //  UCHAR  bLength ;
        //  UCHAR  bDescriptorType ;    
        //  UCHAR  bInterfaceNumber ;
        //  UCHAR  bAlternateSetting ;
        //  UCHAR  bNumEndpoints ;
        //  UCHAR  bInterfaceClass ;
        //  UCHAR  bInterfaceSubClass ;
        //  UCHAR  bInterfaceProtocol ;
        //  UCHAR  iInterface ;
        //} USB_INTERFACE_DESCRIPTOR, *PUSB_INTERFACE_DESCRIPTOR ;

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_INTERFACE_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public byte bInterfaceNumber;
            public byte bAlternateSetting;
            public byte bNumEndpoints;
            public byte bInterfaceClass;
            public byte bInterfaceSubClass;
            public byte bInterfaceProtocol;
            public byte Interface;
        }


        //typedef struct _USB_CONFIGURATION_DESCRIPTOR { 
        //  UCHAR  bLength ;
        //  UCHAR  bDescriptorType ;
        //  USHORT  wTotalLength ;
        //  UCHAR  bNumInterfaces ;
        //  UCHAR  bConfigurationValue;
        //  UCHAR  iConfiguration ;
        //  UCHAR  bmAttributes ;
        //  UCHAR  MaxPower ;
        //} USB_CONFIGURATION_DESCRIPTOR, *PUSB_CONFIGURATION_DESCRIPTOR ;

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_CONFIGURATION_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public short wTotalLength;
            public byte bNumInterface;
            public byte bConfigurationsValue;
            public byte iConfiguration;
            public USB_CONFIGURATION bmAttributes;
            public byte MaxPower;
        }

        //typedef struct _HID_DESCRIPTOR
        //{
        //UCHAR  bLength;
        //UCHAR  bDescriptorType;
        //USHORT  bcdHID;
        //UCHAR  bCountry;
        //UCHAR  bNumDescriptors;
        //struct _HID_DESCRIPTOR_DESC_LIST
        //{
        //UCHAR  bReportType;
        //USHORT  wReportLength;
        //} DescriptorList [1];
        //} HID_DESCRIPTOR, *PHID_DESCRIPTOR;
        
        [StructLayout(LayoutKind.Sequential)]
        public struct HID_DESCRIPTOR_DESC_LIST
        {
            public byte bReportType;
            public short wReportLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HID_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public short bcdHID;
            public byte bCountry;
            public byte bNumDescriptors;
            public HID_DESCRIPTOR_DESC_LIST hid_desclist;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA1
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public ulong Reserved;
        };

        //typedef struct RAW_ROOTPORT_PARAMETERS
        //{
            //USHORT  PortNumber;
            //USHORT  PortStatus;
        //} RAW_ROOTPORT_PARAMETERS, *PRAW_ROOTPORT_PARAMETERS;

        [StructLayout(LayoutKind.Sequential)]
        public class RAW_ROOTPORT_PARAMETERS
        {
            public ushort PortNumber;
            public ushort PortStatus;
        }

        //typedef struct USB_UNICODE_NAME
        //{
            //ULONG  Length;
            //WCHAR  String[1];
        //} USB_UNICODE_NAME, *PUSB_UNICODE_NAME;

        [StructLayout(LayoutKind.Sequential)]
        public class USB_UNICODE_NAME
        {
            public ulong Length;
            public string str;
        }

        #endregion

        // *******************************************************************************************
        // *************************************** methodes ******************************************
        // *******************************************************************************************

        #region methodes

        //original winapi SetupDiGetClassDevs methode
        //HDEVINFO SetupDiGetClassDevs( const GUID* ClassGuid, PCTSTR Enumerator, HWND hwndParent, DWORD Flags );
        [DllImport( "setupapi.dll", CharSet = CharSet.Auto )]
        public static extern IntPtr SetupDiGetClassDevs( ref Guid ClassGuid, int Enumerator, IntPtr hwndParent, int Flags ); // 1st form using a ClassGUID

        [DllImport( "setupapi.dll", CharSet = CharSet.Auto )]
        public static extern IntPtr SetupDiGetClassDevs( int ClassGuid, string Enumerator, IntPtr hwndParent, int Flags ); // 2nd form uses an Enumerator

        [DllImport("setupapi.dll")]
        internal static extern IntPtr SetupDiGetClassDevsEx(IntPtr ClassGuid, [MarshalAs(UnmanagedType.LPStr)]String enumerator, IntPtr hwndParent, Int32 Flags, IntPtr DeviceInfoSet, [MarshalAs(UnmanagedType.LPStr)]String MachineName, IntPtr Reserved);

        //original winapi SetupDiEnumDeviceInterfaces methode
        //BOOL SetupDiEnumDeviceInterfaces( HDEVINFO DeviceInfoSet, PSP_DEVINFO_DATA DeviceInfoData, const GUID* InterfaceClassGuid, DWORD MemberIndex, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData );
        [DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool SetupDiEnumDeviceInterfaces( IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData );

        //original winapi SetupDiGetDeviceInterfaceDetail methode
        //BOOL SetupDiGetDeviceInterfaceDetail( HDEVINFO DeviceInfoSet, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, PSP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, DWORD DeviceInterfaceDetailDataSize, PDWORD RequiredSize, PSP_DEVINFO_DATA DeviceInfoData );
        [DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool SetupDiGetDeviceInterfaceDetail( IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData );

        //original winapi SetupDiGetDeviceRegistryProperty methode
        //BOOL SetupDiGetDeviceRegistryProperty( HDEVINFO DeviceInfoSet, PSP_DEVINFO_DATA DeviceInfoData, DWORD Property, PDWORD PropertyRegDataType, PBYTE PropertyBuffer, DWORD PropertyBufferSize, PDWORD RequiredSize );
        [DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool SetupDiGetDeviceRegistryProperty( IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, int iProperty, ref int PropertyRegDataType, IntPtr PropertyBuffer, int PropertyBufferSize, ref int RequiredSize );

        //original winapi SetupDiEnumDeviceInfo methode
        //BOOL SetupDiEnumDeviceInfo( HDEVINFO DeviceInfoSet, DWORD MemberIndex, PSP_DEVINFO_DATA DeviceInfoData );
        [DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool SetupDiEnumDeviceInfo( IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData );

        //original winapi SetupDiDestroyDeviceInfoList methode
        //BOOL SetupDiDestroyDeviceInfoList( HDEVINFO DeviceInfoSet );
        [DllImport( "setupapi.dll", SetLastError = true )]
        public static extern bool SetupDiDestroyDeviceInfoList( IntPtr DeviceInfoSet );

        //original winapi SetupDiGetDeviceInstanceId methode
        //WINSETUPAPI BOOL WINAPI SetupDiGetDeviceInstanceId( IN HDEVINFO  DeviceInfoSet, IN PSP_DEVINFO_DATA  DeviceInfoData, OUT PTSTR  DeviceInstanceId, IN DWORD  DeviceInstanceIdSize, OUT PDWORD  RequiredSize  OPTIONAL );
        [DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool SetupDiGetDeviceInstanceId( IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, int DeviceInstanceIdSize, out int RequiredSize );

        //original winapi DeviceIoControl methode
        //BOOL DeviceIoControl( HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpInBuffer, DWORD nInBufferSize, LPVOID lpOutBuffer, DWORD nOutBufferSize, LPDWORD lpBytesReturned, LPOVERLAPPED lpOverlapped );
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool DeviceIoControl( IntPtr hDevice, int dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped );

        //original winapi CreateFile methode
        //HANDLE CreateFile( LPCTSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile );
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern IntPtr CreateFile( string lpFileName, uint dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile );

        //original winapi CloseHandle methode
        //BOOL CloseHandle( HANDLE hObject );
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool CloseHandle( IntPtr hObject );

        //original winapi SetupDiSetClassInstallParams methode
        //WINSETUPAPI BOOL WINAPI SetupDiSetClassInstallParams( IN HDEVINFO  DeviceInfoSet, IN PSP_DEVINFO_DATA  DeviceInfoData,  IN PSP_CLASSINSTALL_HEADER  ClassInstallParams,  IN DWORD  ClassInstallParamsSize );
        [DllImport( "setupapi.dll" )]
        public static extern bool SetupDiSetClassInstallParams( IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref SP_CLASSINSTALL_HEADER ClassInstallParams, int ClassInstallParamsSize );

        //original winapi SetupDiCallClassInstaller methode
        //WINSETUPAPI BOOL WINAPI SetupDiCallClassInstaller( IN DI_FUNCTION  InstallFunction, IN HDEVINFO  DeviceInfoSet, IN PSP_DEVINFO_DATA  DeviceInfoData );
        [DllImport( "setupapi.dll" )]
        public static extern bool SetupDiCallClassInstaller( int InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData );

        //original winapi IsSystemResumeAutomatic methode
        //BOOL WINAPI IsSystemResumeAutomatic(void);
        [DllImport( "kernel32.dll" )]
        public static extern bool IsSystemResumeAutomatic();

        //original winapi SetupDiGetDeviceInfoListClass methode
        //WINSETUPAPI BOOL WINAPI SetupDiGetDeviceInfoListClass( IN HDEVINFO  DeviceInfoSet, OUT LPGUID  ClassGuid );
        //[DllImport( "setupapi.dll" )]
        //public static extern bool SetupDiGetDeviceInfoListClass( IntPtr DeviceInfoSet, out Guid ClassGuid );
        
        //[DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        //public static extern bool SetupDiClassGuidsFromName( StringBuilder ClassName, out Guid[] ClassGuidList, int ClassGuidListSize, out int RequiredSize);

        [DllImport( "setupapi.dll" )]
        public static extern bool SetupDiClassGuidsFromNameA( string ClassN, ref Guid guids, UInt32 ClassNameSize, ref UInt32 ReqSize );

        //[DllImport("setupapi.dll")]
        //internal static extern Int32 SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, Int32 DeviceInstanceIdSize, ref Int32 RequiredSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenFile(string lpFileName, out IntPtr lpReOpenBuff, uint style);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint iEnumerator, int hwndParent, int Flags);

        [DllImport("Setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, UInt32 dwIndex, SP_DEVINFO_DATA1 devInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA1 DeviceInfoData, UInt32 Property, UInt32 PropertyRegDataType, StringBuilder PropertyBuffer, UInt32 PropertyBufferSize, IntPtr RequiredSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void SetLastError(int error);

        [DllImport("kernel32.dll")]
        public static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, [In] ref System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("quickusb.dll", CharSet = CharSet.Ansi)]
        public static extern int QuickUsbOpen(out IntPtr handle, string devName);

        #endregion





        //Looking for 
        // USB_SEND_RAW_COMMAND_PARAMETERS

        #endregion
    }
}