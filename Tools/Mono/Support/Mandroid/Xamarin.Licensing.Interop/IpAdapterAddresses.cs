using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Xamarin.Licensing.Interop
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    internal struct PhysicalAddress
    {
        public byte FixedElementField;
    }

    [StructLayout(LayoutKind.Sequential, Size = 64)]
    internal struct ZoneIndices
    {
        public uint FixedElementField;
    }

    [StructLayout(LayoutKind.Sequential, Size = 130)]
    internal struct Dhcpv6ClientDuid
    {
        public byte FixedElementField;
    }

    #pragma warning disable 0649
    internal struct IpAdapterAddresses
	{
		private const int MaxAdapterAddressLength = 8;
		private const int MaxDhcpv6DuidLength = 130;
		public ulong Alignment;
        public unsafe IpAdapterAddresses* Next;
        public IntPtr AdapterName;
		public IntPtr FirstUnicastAddress;
		public IntPtr FirstAnycastAddress;
		public IntPtr FirstMulticastAddress;
		public IntPtr FirstDnsServerAddress;
		public IntPtr DnsSuffix;
		public IntPtr Description;
		public IntPtr FriendlyName;
        public PhysicalAddress PhysicalAddress;
        public uint PhysicalAddressLength;
		public IpAdapterAddressesFlags Flags;
		public uint Mtu;
		public IfType IfType;
		public IfOperStatus OperStatus;
		public uint Ipv6IfIndex;
        public ZoneIndices ZoneIndices;
		public IntPtr FirstPrefix;
		public ulong TransmitLinkSpeed;
		public ulong ReceiveLinkSpeed;
		public IntPtr FirstWinsServerAddress;
		public IntPtr FirstGatewayAddress;
		public uint Ipv4Metric;
		public uint Ipv6Metric;
		public IfLuid Luid;
		public SocketAddress Dhcpv4Server;
		public uint CompartmentId;
		public Guid NetworkGuid;
		public NetIfConnectionType ConnectionType;
		public TunnelType TunnelType;
		public SocketAddress Dhcpv6Server;
        public Dhcpv6ClientDuid Dhcpv6ClientDuid;
		public uint Dhcpv6ClientDuidLength;
		public uint Dhcpv6Iaid;
		public IntPtr FirstDnsSuffix;
	}
    #pragma warning restore 0649
}
