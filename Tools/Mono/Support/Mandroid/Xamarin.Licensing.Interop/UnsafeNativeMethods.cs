using System;
using System.Runtime.InteropServices;
namespace Xamarin.Licensing.Interop
{
	internal static class UnsafeNativeMethods
	{
		[DllImport("iphlpapi.dll")]
		internal static extern WinapiError GetAdaptersAddresses(AddressFamily Family, GetAdaptersAddressesFlags Flags, IntPtr Reserved, IntPtr AdapterAddresses, ref uint SizePointer);
		[DllImport("kernel32.dll")]
		internal static extern int GetVolumeInformation(string lpRootPathName, IntPtr lpVolumeNamebuffer, int nVolumeNameSize, out int lpVOlumeSerialNumber, IntPtr lpMaximumComponentLength, IntPtr lpFileSystemFlags, IntPtr lpFileSystemNamebuffer, int nFileSystemNameSize);
	}
}
