using System;
namespace Xamarin.Licensing.Interop
{
	[Flags]
	internal enum GetAdaptersAddressesFlags : uint
	{
		SKipUnicast = 1u,
		SkipAnycast = 2u,
		SkipMulticast = 4u,
		SkipDnsServer = 8u,
		IncludePrefix = 16u,
		SkipFriendlyName = 32u,
		IncludeWinsInfo = 64u,
		IncludeGateways = 128u,
		IncludeAllInterfaces = 256u,
		IncludeAllCompartments = 512u,
		IncludeTunnelBindingOrder = 1024u
	}
}
