using System;
namespace Xamarin.Licensing.Interop
{
	internal enum IfType : uint
	{
		Other = 1u,
		EthernetCsmacd = 6u,
		Iso88025TokenRing = 9u,
		Ppp = 23u,
		SoftwareLoopback,
		Atm = 37u,
		IEEE80211 = 71u,
		Tunnel = 131u,
		IEEE1394 = 144u
	}
}
