#region references

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

#endregion

namespace NativeUsbLib
{
    /// <summary>
    /// Usb hub
    /// </summary>
    public class UsbHub : Device
    {
        #region fields

        private int m_PortCount = -1;
        /// <summary>
        /// Gets the port count.
        /// </summary>
        /// <value>The port count.</value>
        public int PortCount
        {
            get { return m_PortCount; }
        }

        private bool m_IsBusPowered = false;
        /// <summary>
        /// Gets a value indicating whether this instance is bus powered.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is bus powered; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusPowered
        {
            get { return m_IsBusPowered; }
        }

        private bool m_IsRootHub = false;
        /// <summary>
        /// Gets a value indicating whether this instance is root hub.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is root hub; otherwise, <c>false</c>.
        /// </value>
        public bool IsRootHub
        {
            get { return m_IsRootHub; }
        }
        private UsbApi.USB_NODE_INFORMATION m_NodeInformation = new UsbApi.USB_NODE_INFORMATION();
        /// <summary>
        /// Gets or sets the node information.
        /// </summary>
        /// <value>The node information.</value>
        public UsbApi.USB_NODE_INFORMATION NodeInformation
        {
            get { return m_NodeInformation; }
            set
            {
                this.m_NodeInformation = value;
                this.m_IsBusPowered = Convert.ToBoolean(m_NodeInformation.HubInformation.HubIsBusPowered);
                this.m_PortCount = m_NodeInformation.HubInformation.HubDescriptor.bNumberOfPorts;
            }
        }

        #endregion

        #region constructor/destructor

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbHub"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="deviceDescriptor">The device descriptor.</param>
        /// <param name="devicePath">The device path.</param>
        public UsbHub(Device parent, UsbApi.USB_DEVICE_DESCRIPTOR deviceDescriptor, string devicePath)
            : base(parent, deviceDescriptor, -1, devicePath)
        {
            IntPtr handel1 = IntPtr.Zero;
            IntPtr handel2 = IntPtr.Zero;
            this.m_DeviceDescription = "Standard-USB-Hub";
            this.m_DevicePath = devicePath;

            int nBytesReturned = -1;
            int nBytes = -1;

            // Open a handle to the host controller.
            handel1 = UsbApi.CreateFile(devicePath, UsbApi.GENERIC_WRITE, UsbApi.FILE_SHARE_WRITE, IntPtr.Zero, UsbApi.OPEN_EXISTING, 0, IntPtr.Zero);
            if (handel1.ToInt32() != UsbApi.INVALID_HANDLE_VALUE)
            {

                UsbApi.USB_ROOT_HUB_NAME rootHubName = new UsbApi.USB_ROOT_HUB_NAME();
                nBytes = Marshal.SizeOf(rootHubName);
                IntPtr ptrRootHubName = Marshal.AllocHGlobal(nBytes);

                // Get the root hub name.
                if (UsbApi.DeviceIoControl(handel1, UsbApi.IOCTL_USB_GET_ROOT_HUB_NAME, ptrRootHubName, nBytes, ptrRootHubName, nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    rootHubName = (UsbApi.USB_ROOT_HUB_NAME)Marshal.PtrToStructure(ptrRootHubName, typeof(UsbApi.USB_ROOT_HUB_NAME));

                    if (rootHubName.ActualLength > 0)
                    {
                        this.m_IsRootHub = true;
                        this.m_DeviceDescription = "RootHub";
                        this.m_DevicePath = @"\\?\" + rootHubName.RootHubName;
                    }
                }

                Marshal.FreeHGlobal(ptrRootHubName);

                // TODO: Get the driver key name for the root hub.

                // Now let's open the hub (based upon the hub name we got above).
                handel2 = UsbApi.CreateFile(this.DevicePath, UsbApi.GENERIC_WRITE, UsbApi.FILE_SHARE_WRITE, IntPtr.Zero, UsbApi.OPEN_EXISTING, 0, IntPtr.Zero);
                if (handel2.ToInt32() != UsbApi.INVALID_HANDLE_VALUE)
                {

                    UsbApi.USB_NODE_INFORMATION NodeInfo = new UsbApi.USB_NODE_INFORMATION();
                    NodeInfo.NodeType = UsbApi.USB_HUB_NODE.UsbHub;
                    nBytes = Marshal.SizeOf(NodeInfo);
                    IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                    Marshal.StructureToPtr(NodeInfo, ptrNodeInfo, true);

                    // Get the hub information.
                    if (UsbApi.DeviceIoControl(handel2, UsbApi.IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes, out nBytesReturned, IntPtr.Zero))
                    {
                        this.NodeInformation = (UsbApi.USB_NODE_INFORMATION)Marshal.PtrToStructure(ptrNodeInfo, typeof(UsbApi.USB_NODE_INFORMATION));
                    }

                    Marshal.FreeHGlobal(ptrNodeInfo);
                    UsbApi.CloseHandle(handel2);
                }

                UsbApi.CloseHandle(handel1);

                for (int index = 1; index <= PortCount; index++)
                {

                    // Initialize a new port and save the port.
                    try
                    {
                        //this.childs.Add(new UsbPort(this, index, this.DevicePath));
                        this.devices.Add(DeviceFactory.BuildDevice(this, index, this.DevicePath));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
                throw new Exception("No port found!");
        }

        #endregion

        #region destructor

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="UsbHub"/> is reclaimed by garbage collection.
        /// </summary>
        ~UsbHub()
        {
        }

        #endregion

        #endregion
    }
}