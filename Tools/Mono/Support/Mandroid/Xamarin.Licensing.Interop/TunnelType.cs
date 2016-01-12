using System;
namespace Xamarin.Licensing.Interop
{
	internal enum TunnelType
	{
		None,
		Other,
		Direct,
		Ipv6ToIpv4 = 11,
		Isatap = 13,
		Teredo,
		IpOverHttps
	}
}
