using System;
namespace Xamarin.Licensing.Interop
{
#pragma warning disable 0649
    internal struct SocketAddress
	{
		public IntPtr lpSockaddr;
		public int iSockaddrLength;
	}
#pragma warning restore 0649
}
