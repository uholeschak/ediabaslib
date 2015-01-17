#region references

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

#endregion

namespace NativeUsbLib
{
    /// <summary>
    /// abstract class of al usb devices
    /// </summary>
    public abstract class Device
    {
        #region fields
        /// <summary>
        /// The parent.
        /// </summary>
        protected Device parent = null;
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Device Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// The childs.
        /// </summary>
        protected List<Device> devices = null;
        /// <summary>
        /// Gets the childs.
        /// </summary>
        /// <value>The childs.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<Device> Devices
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<Device>(devices); }
        }

        private UsbApi.USB_NODE_CONNECTION_INFORMATION_EX m_NodeConnectionInfo;
        /// <summary>
        /// Gets or sets the node connection info.
        /// </summary>
        /// <value>The node connection info.</value>
        public UsbApi.USB_NODE_CONNECTION_INFORMATION_EX NodeConnectionInfo
        {
            get { return m_NodeConnectionInfo; }
            set
            {
                m_NodeConnectionInfo = value;
                this.m_AdapterNumber = NodeConnectionInfo.ConnectionIndex;
                UsbApi.USB_CONNECTION_STATUS Status = (UsbApi.USB_CONNECTION_STATUS)NodeConnectionInfo.ConnectionStatus;
                this.m_Status = Status.ToString();
                UsbApi.USB_DEVICE_SPEED Speed = (UsbApi.USB_DEVICE_SPEED)NodeConnectionInfo.Speed;
                this.m_Speed = Speed.ToString();
                this.m_IsConnected = (NodeConnectionInfo.ConnectionStatus == (UsbApi.USB_CONNECTION_STATUS)UsbApi.USB_CONNECTION_STATUS.DeviceConnected);
                this.m_IsHub = Convert.ToBoolean(NodeConnectionInfo.DeviceIsHub);
            }
        }

        private UsbApi.USB_DEVICE_DESCRIPTOR m_DeviceDescriptor;
        /// <summary>
        /// Gets or sets the device descriptor.
        /// </summary>
        /// <value>The device descriptor.</value>
        public UsbApi.USB_DEVICE_DESCRIPTOR DeviceDescriptor
        {
            get { return m_DeviceDescriptor; }
            set { m_DeviceDescriptor = value; }
        }


        private List<UsbApi.USB_CONFIGURATION_DESCRIPTOR> m_ConfigurationDescriptor = null;
        /// <summary>
        /// Gets the configuration descriptor.
        /// </summary>
        /// <value>The configuration descriptor.</value>
        public List<UsbApi.USB_CONFIGURATION_DESCRIPTOR> ConfigurationDescriptor
        {
            get { return m_ConfigurationDescriptor; }
        }

        private List<UsbApi.USB_INTERFACE_DESCRIPTOR> m_InterfaceDescriptor = null;
        /// <summary>
        /// Gets the interface descriptor.
        /// </summary>
        /// <value>The interface descriptor.</value>
        public List<UsbApi.USB_INTERFACE_DESCRIPTOR> InterfaceDescriptor
        {
            get { return m_InterfaceDescriptor; }
        }
       
        private List<UsbApi.USB_ENDPOINT_DESCRIPTOR> m_EndpointDescriptor = null;
        /// <summary>
        /// Gets the endpoint descriptor.
        /// </summary>
        /// <value>The endpoint descriptor.</value>
        public List<UsbApi.USB_ENDPOINT_DESCRIPTOR> EndpointDescriptor
        {
            get { return m_EndpointDescriptor; }
        }

        private List<UsbApi.HID_DESCRIPTOR> m_HdiDescriptor = null;
        /// <summary>
        /// Gets the hdi descriptor.
        /// </summary>
        /// <value>The hdi descriptor.</value>
        public List<UsbApi.HID_DESCRIPTOR> HdiDescriptor
        {
            get { return m_HdiDescriptor; }
        }


        /// <summary>
        /// The device path
        /// </summary>
        protected string m_DevicePath = string.Empty;
        /// <summary>
        /// Gets the device path.
        /// </summary>
        /// <value>The device path.</value>
        public string DevicePath
        {
            get { return m_DevicePath; }
        }

        /// <summary>
        /// The driver key.
        /// </summary>
        protected string m_DriverKey = string.Empty;
        /// <summary>
        /// Gets the driver key.
        /// </summary>
        /// <value>The driver key.</value>
        public string DriverKey
        {
            get { return m_DriverKey; }
        }

        /// <summary>
        /// The name.
        /// </summary>
        protected string m_Name = string.Empty;
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// The device description
        /// </summary>
        protected string m_DeviceDescription = string.Empty;
        /// <summary>
        /// Gets the device description.
        /// </summary>
        /// <value>The device description.</value>
        public string DeviceDescription
        {
            get { return m_DeviceDescription; }
        }

        /// <summary>
        /// IsConnected
        /// </summary>
        protected bool m_IsConnected = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get { return m_IsConnected; }
            set { m_IsConnected = value; }
        }

        /// <summary>
        /// IsHub
        /// </summary>
        protected bool m_IsHub = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is hub.
        /// </summary>
        /// <value><c>true</c> if this instance is hub; otherwise, <c>false</c>.</value>
        public bool IsHub
        {
            get { return m_IsHub; }
            set { m_IsHub = value; }
        }

        /// <summary>
        /// The status.
        /// </summary>
        protected string m_Status = string.Empty;
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public string Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        /// <summary>
        /// The speed.
        /// </summary>
        protected string m_Speed = string.Empty;
        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public string Speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        /// <summary>
        /// The adapter number.
        /// </summary>
        protected int m_AdapterNumber = -1;
        /// <summary>
        /// Gets or sets the adapter number.
        /// </summary>
        /// <value>The adapter number.</value>
        public int AdapterNumber
        {
            get { return m_AdapterNumber; }
            set { m_AdapterNumber = value; }
        }

        private string m_Manufacturer = string.Empty;
        /// <summary>
        /// Gets the manufacturer.
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer
        {
            get { return m_Manufacturer; }
        }

        private string m_InstanceId = string.Empty;
        /// <summary>
        /// Gets the instance id.
        /// </summary>
        /// <value>The instance id.</value>
        public string InstanceId
        {
            get { return m_InstanceId; }
        }

        private string m_SerialNumber = string.Empty;
        /// <summary>
        /// Gets the serial number.
        /// </summary>
        /// <value>The serial number.</value>
        public string SerialNumber
        {
            get { return m_SerialNumber; }
        }

        private string m_Product = string.Empty;
        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <value>The product.</value>
        public string Product
        {
            get { return m_Product; }
        }

        #endregion

        #region constructor/destructor

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="deviceDescriptor">The device descriptor.</param>
        /// <param name="adapterNumber">The adapter number.</param>
        /// <param name="devicePath">The device path.</param>
        public Device(Device parent, UsbApi.USB_DEVICE_DESCRIPTOR deviceDescriptor, int adapterNumber, string devicePath)
        {
            this.parent = parent;
            this.m_AdapterNumber = adapterNumber;
            this.m_DeviceDescriptor = deviceDescriptor;
            this.m_DevicePath = devicePath;
            this.devices = new List<Device>();

            if (devicePath == null)
                return;

            IntPtr handel = UsbApi.CreateFile(devicePath, UsbApi.GENERIC_WRITE, UsbApi.FILE_SHARE_WRITE, IntPtr.Zero, UsbApi.OPEN_EXISTING, 0, IntPtr.Zero);
            if (handel.ToInt32() != UsbApi.INVALID_HANDLE_VALUE)
            {
                // We use this to zero fill a buffer
                string NullString = new string((char)0, UsbApi.MAX_BUFFER_SIZE / Marshal.SystemDefaultCharSize);

                int nBytesReturned = 0;
                int nBytes = UsbApi.MAX_BUFFER_SIZE;
                // build a request for string descriptor
                UsbApi.USB_DESCRIPTOR_REQUEST Request1 = new UsbApi.USB_DESCRIPTOR_REQUEST();
                Request1.ConnectionIndex = adapterNumber;// portCount;
                Request1.SetupPacket.wValue = (short)((UsbApi.USB_CONFIGURATION_DESCRIPTOR_TYPE << 8));
                Request1.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request1));
                Request1.SetupPacket.wIndex = 0x409; // Language Code
                // Geez, I wish C# had a Marshal.MemSet() method
                IntPtr ptrRequest1 = Marshal.StringToHGlobalAuto(NullString);
                Marshal.StructureToPtr(Request1, ptrRequest1, true);

                // Use an IOCTL call to request the String Descriptor
                if (UsbApi.DeviceIoControl(handel, UsbApi.IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest1, nBytes, ptrRequest1, nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    IntPtr ptr = new IntPtr(ptrRequest1.ToInt32() + Marshal.SizeOf(Request1));

                    UsbApi.USB_CONFIGURATION_DESCRIPTOR configurationDescriptor = new UsbApi.USB_CONFIGURATION_DESCRIPTOR();
                    configurationDescriptor = (UsbApi.USB_CONFIGURATION_DESCRIPTOR)Marshal.PtrToStructure(ptr, typeof(UsbApi.USB_CONFIGURATION_DESCRIPTOR));
                    if (m_ConfigurationDescriptor == null)
                    {
                        m_ConfigurationDescriptor = new List<UsbApi.USB_CONFIGURATION_DESCRIPTOR>();
                        m_ConfigurationDescriptor.Add(configurationDescriptor);
                    }
                    else
                        m_ConfigurationDescriptor.Add(configurationDescriptor);

                    int p = (int)ptr;
                    p += Marshal.SizeOf(configurationDescriptor) - 1;
                    ptr = (IntPtr)p;

                    for (int i = 0; i < configurationDescriptor.bNumInterface; i++)
                    {
                        UsbApi.USB_INTERFACE_DESCRIPTOR interfaceDescriptor = new UsbApi.USB_INTERFACE_DESCRIPTOR();
                        interfaceDescriptor = (UsbApi.USB_INTERFACE_DESCRIPTOR)Marshal.PtrToStructure(ptr, typeof(UsbApi.USB_INTERFACE_DESCRIPTOR));
                        if (m_InterfaceDescriptor == null)
                        {
                            m_InterfaceDescriptor = new List<UsbApi.USB_INTERFACE_DESCRIPTOR>();
                            m_InterfaceDescriptor.Add(interfaceDescriptor);
                        }
                        else
                            m_InterfaceDescriptor.Add(interfaceDescriptor);

                        p = (int)ptr;
                        p += Marshal.SizeOf(interfaceDescriptor);

                        if (interfaceDescriptor.bInterfaceClass == 0x03)
                        {

                            ptr = (IntPtr)p;
                            for (int k = 0; k < interfaceDescriptor.bInterfaceSubClass; k++)
                            {
                                UsbApi.HID_DESCRIPTOR hdiDescriptor = new UsbApi.HID_DESCRIPTOR();
                                hdiDescriptor = (UsbApi.HID_DESCRIPTOR)Marshal.PtrToStructure(ptr, typeof(UsbApi.HID_DESCRIPTOR));
                                if (m_HdiDescriptor == null)
                                {
                                    m_HdiDescriptor = new List<UsbApi.HID_DESCRIPTOR>();
                                    m_HdiDescriptor.Add(hdiDescriptor);
                                }
                                else
                                    m_HdiDescriptor.Add(hdiDescriptor);

                                p = (int)ptr;
                                p += Marshal.SizeOf(hdiDescriptor);
                                p--;

                            }
                        }

                        ptr = (IntPtr)p;
                        for (int j = 0; j < interfaceDescriptor.bNumEndpoints; j++)
                        {
                            UsbApi.USB_ENDPOINT_DESCRIPTOR endpointDescriptor1 = new UsbApi.USB_ENDPOINT_DESCRIPTOR();
                            endpointDescriptor1 = (UsbApi.USB_ENDPOINT_DESCRIPTOR)Marshal.PtrToStructure(ptr, typeof(UsbApi.USB_ENDPOINT_DESCRIPTOR));
                            if (m_EndpointDescriptor == null)
                            {
                                m_EndpointDescriptor = new List<UsbApi.USB_ENDPOINT_DESCRIPTOR>();
                                m_EndpointDescriptor.Add(endpointDescriptor1);
                            }
                            else
                                m_EndpointDescriptor.Add(endpointDescriptor1);

                            p = (int)ptr;
                            p += Marshal.SizeOf(endpointDescriptor1) - 1;
                            ptr = (IntPtr)p;
                        }
                    }
                }
                Marshal.FreeHGlobal(ptrRequest1);

                // The iManufacturer, iProduct and iSerialNumber entries in the
                // device descriptor are really just indexes.  So, we have to 
                // request a string descriptor to get the values for those strings.
                if (this.m_DeviceDescriptor != null && this.m_DeviceDescriptor.iManufacturer > 0)
                {
                    // Build a request for string descriptor.
                    UsbApi.USB_DESCRIPTOR_REQUEST Request = new UsbApi.USB_DESCRIPTOR_REQUEST();
                    Request.ConnectionIndex = adapterNumber;
                    Request.SetupPacket.wValue = (short)((UsbApi.USB_STRING_DESCRIPTOR_TYPE << 8) + this.m_DeviceDescriptor.iManufacturer);
                    Request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request));
                    Request.SetupPacket.wIndex = 0x409; // The language code.

                    // Geez, I wish C# had a Marshal.MemSet() method.
                    IntPtr ptrRequest = Marshal.StringToHGlobalAuto(NullString);
                    Marshal.StructureToPtr(Request, ptrRequest, true);

                    // Use an IOCTL call to request the string descriptor.
                    if (UsbApi.DeviceIoControl(handel, UsbApi.IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes, ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                    {

                        // The location of the string descriptor is immediately after
                        // the Request structure.  Because this location is not "covered"
                        // by the structure allocation, we're forced to zero out this
                        // chunk of memory by using the StringToHGlobalAuto() hack above
                        IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(Request));
                        UsbApi.USB_STRING_DESCRIPTOR StringDesc = (UsbApi.USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(ptrStringDesc, typeof(UsbApi.USB_STRING_DESCRIPTOR));
                        m_Manufacturer = StringDesc.bString;
                    }
                    Marshal.FreeHGlobal(ptrRequest);
                }

                if (this.m_DeviceDescriptor != null && this.m_DeviceDescriptor.iSerialNumber > 0)
                {

                    // Build a request for string descriptor.
                    UsbApi.USB_DESCRIPTOR_REQUEST Request = new UsbApi.USB_DESCRIPTOR_REQUEST();
                    Request.ConnectionIndex = adapterNumber;
                    Request.SetupPacket.wValue = (short)((UsbApi.USB_STRING_DESCRIPTOR_TYPE << 8) + this.m_DeviceDescriptor.iSerialNumber);
                    Request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request));
                    Request.SetupPacket.wIndex = 0x409; // The language code.

                    // Geez, I wish C# had a Marshal.MemSet() method.
                    IntPtr ptrRequest = Marshal.StringToHGlobalAuto(NullString);
                    Marshal.StructureToPtr(Request, ptrRequest, true);

                    // Use an IOCTL call to request the string descriptor
                    if (UsbApi.DeviceIoControl(handel, UsbApi.IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes, ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                    {

                        // The location of the string descriptor is immediately after the request structure.
                        IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(Request));
                        UsbApi.USB_STRING_DESCRIPTOR StringDesc = (UsbApi.USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(ptrStringDesc, typeof(UsbApi.USB_STRING_DESCRIPTOR));
                        m_SerialNumber = StringDesc.bString;
                    }
                    Marshal.FreeHGlobal(ptrRequest);
                }

                if (this.m_DeviceDescriptor != null && this.m_DeviceDescriptor.iProduct > 0)
                {

                    // Build a request for endpoint descriptor.
                    UsbApi.USB_DESCRIPTOR_REQUEST Request = new UsbApi.USB_DESCRIPTOR_REQUEST();
                    Request.ConnectionIndex = adapterNumber;
                    Request.SetupPacket.wValue = (short)((UsbApi.USB_STRING_DESCRIPTOR_TYPE << 8) + this.m_DeviceDescriptor.iProduct);
                    Request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request));
                    Request.SetupPacket.wIndex = 0x409; // The language code.

                    // Geez, I wish C# had a Marshal.MemSet() method.
                    IntPtr ptrRequest = Marshal.StringToHGlobalAuto(NullString);
                    Marshal.StructureToPtr(Request, ptrRequest, true);

                    // Use an IOCTL call to request the string descriptor.
                    if (UsbApi.DeviceIoControl(handel, UsbApi.IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes, ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                    {

                        // the location of the string descriptor is immediately after the Request structure
                        IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(Request));
                        UsbApi.USB_STRING_DESCRIPTOR StringDesc = (UsbApi.USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(ptrStringDesc, typeof(UsbApi.USB_STRING_DESCRIPTOR));
                        m_Product = StringDesc.bString;
                    }

                    Marshal.FreeHGlobal(ptrRequest);
                }

                // Get the Driver Key Name (usefull in locating a device)
                UsbApi.USB_NODE_CONNECTION_DRIVERKEY_NAME DriverKey = new UsbApi.USB_NODE_CONNECTION_DRIVERKEY_NAME();
                DriverKey.ConnectionIndex = adapterNumber;
                nBytes = Marshal.SizeOf(DriverKey);
                IntPtr ptrDriverKey = Marshal.AllocHGlobal(nBytes);
                Marshal.StructureToPtr(DriverKey, ptrDriverKey, true);

                // Use an IOCTL call to request the Driver Key Name
                if (UsbApi.DeviceIoControl(handel, UsbApi.IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, ptrDriverKey, nBytes, ptrDriverKey, nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    DriverKey = (UsbApi.USB_NODE_CONNECTION_DRIVERKEY_NAME)Marshal.PtrToStructure(ptrDriverKey, typeof(UsbApi.USB_NODE_CONNECTION_DRIVERKEY_NAME));
                    this.m_DriverKey = DriverKey.DriverKeyName;

                    // use the DriverKeyName to get the Device Description and Instance ID
                    //Name = GetDescriptionByKeyName(this.DriverKey);
                    m_DeviceDescription = GetDescriptionByKeyName(this.DriverKey);
                    m_InstanceId = GetInstanceIDByKeyName(this.DriverKey);
                }
                Marshal.FreeHGlobal(ptrDriverKey);
            }
            UsbApi.CloseHandle(handel);
        }

        #endregion

        #region destructor

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Device"/> is reclaimed by garbage collection.
        /// </summary>
        ~Device()
        {
            devices.Clear();
            devices = null;
            parent = null;
        }

        #endregion

        #endregion

        #region methodes

        #region methode GetDescriptionByKeyName

        /// <summary>
        /// Gets the name of the description by key.
        /// </summary>
        /// <param name="DriverKeyName">Name of the driver key.</param>
        /// <returns></returns>
        protected string GetDescriptionByKeyName(string DriverKeyName)
        {
            string descriptionkeyname = string.Empty;
            string DevEnum = UsbApi.REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API 
            // to generate a list of all USB devices
            IntPtr handel = UsbApi.SetupDiGetClassDevs(0, DevEnum, IntPtr.Zero, UsbApi.DIGCF_PRESENT | UsbApi.DIGCF_ALLCLASSES);
            if (handel.ToInt32() != UsbApi.INVALID_HANDLE_VALUE)
            {

                IntPtr ptr = Marshal.AllocHGlobal(UsbApi.MAX_BUFFER_SIZE);
                string keyName = string.Empty;
                bool success = true;

                for (int i = 0; success; i++)
                {

                    // Create a device interface data structure.
                    UsbApi.SP_DEVINFO_DATA deviceInterfaceData = new UsbApi.SP_DEVINFO_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

                    // Start the enumeration.
                    success = UsbApi.SetupDiEnumDeviceInfo(handel, i, ref deviceInterfaceData);
                    if (success)
                    {

                        int RequiredSize = -1;
                        int RegType = UsbApi.REG_SZ;
                        keyName = string.Empty;

                        if (UsbApi.SetupDiGetDeviceRegistryProperty(handel, ref deviceInterfaceData, UsbApi.SPDRP_DRIVER, ref RegType, ptr, UsbApi.MAX_BUFFER_SIZE, ref RequiredSize))
                            keyName = Marshal.PtrToStringAuto(ptr);

                        // Is it a match?
                        if (keyName == DriverKeyName)
                        {

                            if (UsbApi.SetupDiGetDeviceRegistryProperty(handel, ref deviceInterfaceData, UsbApi.SPDRP_DEVICEDESC, ref RegType, ptr, UsbApi.MAX_BUFFER_SIZE, ref RequiredSize))
                                descriptionkeyname = Marshal.PtrToStringAuto(ptr);

                            break;
                        }
                    }
                }

                Marshal.FreeHGlobal(ptr);
                UsbApi.SetupDiDestroyDeviceInfoList(handel);
            }

            return descriptionkeyname;
        }

        #endregion

        #region methode GetInstanceIDByKeyName

        /// <summary>
        /// Gets the name of the instance ID by key.
        /// </summary>
        /// <param name="DriverKeyName">Name of the driver key.</param>
        /// <returns></returns>
        private string GetInstanceIDByKeyName(string DriverKeyName)
        {
            string descriptionkeyname = string.Empty;
            string DevEnum = UsbApi.REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API 
            // to generate a list of all USB devices
            IntPtr handel = UsbApi.SetupDiGetClassDevs(0, DevEnum, IntPtr.Zero, UsbApi.DIGCF_PRESENT | UsbApi.DIGCF_ALLCLASSES);
            if (handel.ToInt32() != UsbApi.INVALID_HANDLE_VALUE)
            {

                IntPtr ptr = Marshal.AllocHGlobal(UsbApi.MAX_BUFFER_SIZE);
                string keyName = string.Empty;
                bool success = true;

                for (int i = 0; success; i++)
                {

                    // Create a device interface data structure.
                    UsbApi.SP_DEVINFO_DATA deviceInterfaceData = new UsbApi.SP_DEVINFO_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

                    // Start the enumeration.
                    success = UsbApi.SetupDiEnumDeviceInfo(handel, i, ref deviceInterfaceData);
                    if (success)
                    {

                        int RequiredSize = -1;
                        int RegType = UsbApi.REG_SZ;
                        keyName = string.Empty;

                        if (UsbApi.SetupDiGetDeviceRegistryProperty(handel, ref deviceInterfaceData, UsbApi.SPDRP_DRIVER, ref RegType, ptr, UsbApi.MAX_BUFFER_SIZE, ref RequiredSize))
                            keyName = Marshal.PtrToStringAuto(ptr);

                        // is it a match?
                        if (keyName == DriverKeyName)
                        {

                            int nBytes = UsbApi.MAX_BUFFER_SIZE;
                            StringBuilder sb = new StringBuilder(nBytes);
                            UsbApi.SetupDiGetDeviceInstanceId(handel, ref deviceInterfaceData, sb, nBytes, out RequiredSize);
                            descriptionkeyname = sb.ToString();
                            break;
                        }
                    }
                }

                Marshal.FreeHGlobal(ptr);
                UsbApi.SetupDiDestroyDeviceInfoList(handel);
            }

            return descriptionkeyname;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// static class to factory class to build the connected devices
    /// </summary>
    public static class DeviceFactory
    {
        /// <summary>
        /// Builds the device.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="portCount">The port count.</param>
        /// <param name="devicePath">The device path.</param>
        /// <returns>The device.</returns>
        public static Device BuildDevice(Device parent, int portCount, string devicePath)
        {
            IntPtr handel1 = IntPtr.Zero;
            Device _Device = null;

            int nBytes = -1;
            int nBytesReturned = -1;
            bool isConnected = false;

            // Open a handle to the Hub device
            handel1 = UsbApi.CreateFile(devicePath, UsbApi.GENERIC_WRITE, UsbApi.FILE_SHARE_WRITE, IntPtr.Zero, UsbApi.OPEN_EXISTING, 0, IntPtr.Zero);
            if (handel1.ToInt32() != UsbApi.INVALID_HANDLE_VALUE)
            {
                nBytes = Marshal.SizeOf(typeof(UsbApi.USB_NODE_CONNECTION_INFORMATION_EX));
                IntPtr ptrNodeConnection = Marshal.AllocHGlobal(nBytes);
                UsbApi.USB_NODE_CONNECTION_INFORMATION_EX nodeConnection = new UsbApi.USB_NODE_CONNECTION_INFORMATION_EX();
                nodeConnection.ConnectionIndex = portCount;
                Marshal.StructureToPtr(nodeConnection, ptrNodeConnection, true);

                if (UsbApi.DeviceIoControl(handel1, UsbApi.IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX, ptrNodeConnection, nBytes, ptrNodeConnection, nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    nodeConnection = (UsbApi.USB_NODE_CONNECTION_INFORMATION_EX)Marshal.PtrToStructure(ptrNodeConnection, typeof(UsbApi.USB_NODE_CONNECTION_INFORMATION_EX));
                    isConnected = (nodeConnection.ConnectionStatus == (UsbApi.USB_CONNECTION_STATUS)UsbApi.USB_CONNECTION_STATUS.DeviceConnected);
                }

                if (isConnected)
                {
                    if (nodeConnection.DeviceDescriptor.bDeviceClass == UsbApi.UsbDeviceClass.HubDevice)
                    {
                        nBytes = Marshal.SizeOf(typeof(UsbApi.USB_NODE_CONNECTION_NAME));
                        ptrNodeConnection = Marshal.AllocHGlobal(nBytes);
                        nBytesReturned = -1;
                        UsbApi.USB_NODE_CONNECTION_NAME nameConnection = new UsbApi.USB_NODE_CONNECTION_NAME();
                        Marshal.StructureToPtr(nodeConnection, ptrNodeConnection, true);

                        if (UsbApi.DeviceIoControl(handel1, UsbApi.IOCTL_USB_GET_NODE_CONNECTION_NAME, ptrNodeConnection, nBytes, ptrNodeConnection, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            nameConnection = (UsbApi.USB_NODE_CONNECTION_NAME)Marshal.PtrToStructure(ptrNodeConnection, typeof(UsbApi.USB_NODE_CONNECTION_NAME));
                            string name = @"\\?\" + nameConnection.NodeName;
                            //this.childs.Add(new UsbHub(this, name, false));
                            _Device = new UsbHub(parent, nodeConnection.DeviceDescriptor, name);
                            _Device.NodeConnectionInfo = nodeConnection;
                        }
                    }
                    else
                    {
                        //this.childs.Add(new UsbDevice(this, nodeConnection.DeviceDescriptor, portCount, devicePath));
                        _Device = new UsbDevice(parent, nodeConnection.DeviceDescriptor, portCount, devicePath);
                        _Device.NodeConnectionInfo = nodeConnection;
                    }
                }
                else
                {
                    _Device = new UsbDevice(parent, null, portCount);
                    _Device.NodeConnectionInfo = nodeConnection;
                }
                Marshal.FreeHGlobal(ptrNodeConnection);
                UsbApi.CloseHandle(handel1);
            }

            return _Device;
        }
    }
}