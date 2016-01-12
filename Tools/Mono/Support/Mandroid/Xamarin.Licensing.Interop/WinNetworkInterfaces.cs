using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
namespace Xamarin.Licensing.Interop
{
	internal static class WinNetworkInterfaces
	{
		internal static string[] A()
		{
			List<WinNetworkInterfaceInfo> networkInterfaces = WinNetworkInterfaces.GetNetworkInterfaces();
			List<WinNetworkInterfaceInfo> second = WinNetworkInterfaces.Extract<WinNetworkInterfaceInfo>(networkInterfaces, (WinNetworkInterfaceInfo t) => t.Description.IndexOf("usb", StringComparison.OrdinalIgnoreCase) >= 0);
			List<WinNetworkInterfaceInfo> second2 = WinNetworkInterfaces.Extract<WinNetworkInterfaceInfo>(networkInterfaces, (WinNetworkInterfaceInfo t) => t.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0);
			List<WinNetworkInterfaceInfo> first = WinNetworkInterfaces.Extract<WinNetworkInterfaceInfo>(networkInterfaces, (WinNetworkInterfaceInfo t) => t.FriendlyName.IndexOf("local", StringComparison.OrdinalIgnoreCase) >= 0);
			List<WinNetworkInterfaceInfo> second3 = WinNetworkInterfaces.Extract<WinNetworkInterfaceInfo>(networkInterfaces, (WinNetworkInterfaceInfo t) => t.FriendlyName.IndexOf("wireless", StringComparison.OrdinalIgnoreCase) >= 0);
			List<WinNetworkInterfaceInfo> second4 = WinNetworkInterfaces.Extract<WinNetworkInterfaceInfo>(networkInterfaces, (WinNetworkInterfaceInfo t) => t.Description.IndexOf("bluetooth", StringComparison.OrdinalIgnoreCase) >= 0 || t.FriendlyName.IndexOf("bluetooth", StringComparison.OrdinalIgnoreCase) >= 0);
			List<WinNetworkInterfaceInfo> second5 = networkInterfaces;
			return (
				from t in first.Concat(second5).Concat(second3).Concat(second4).Concat(second).Concat(second2)
				select t.MacAddress).ToArray<string>();
		}
		internal unsafe static List<WinNetworkInterfaceInfo> GetNetworkInterfaces()
		{
			uint cb = 0u;
			WinapiError adaptersAddresses = UnsafeNativeMethods.GetAdaptersAddresses(AddressFamily.Unspecified, GetAdaptersAddressesFlags.SkipDnsServer | GetAdaptersAddressesFlags.IncludeGateways, IntPtr.Zero, IntPtr.Zero, ref cb);
			if (adaptersAddresses != WinapiError.BufferOverflow)
			{
				throw new InvalidOperationException("Wat; GetAdaptersAddresses returned: " + adaptersAddresses);
			}
			IntPtr intPtr = Marshal.AllocHGlobal((int)cb);
			adaptersAddresses = UnsafeNativeMethods.GetAdaptersAddresses(AddressFamily.Unspecified, GetAdaptersAddressesFlags.SkipDnsServer | GetAdaptersAddressesFlags.IncludeGateways, IntPtr.Zero, intPtr, ref cb);
			if (adaptersAddresses != WinapiError.None)
			{
				throw new Exception("ERROR F88B");
			}
			List<WinNetworkInterfaceInfo> list = new List<WinNetworkInterfaceInfo>();
			try
			{
                for (IpAdapterAddresses* ptr = (IpAdapterAddresses*)((void*)intPtr); ptr != null; ptr = ptr->Next)
                {
					if (ptr->IfType == IfType.IEEE80211 || ptr->IfType == IfType.EthernetCsmacd)
					{
						if (ptr->PhysicalAddressLength >= 6u)
						{
							StringBuilder stringBuilder = new StringBuilder();
							stringBuilder.AppendFormat("{0:X2}", ptr->PhysicalAddress.FixedElementField);
							int num = 1;
							while ((long)num < (long)((ulong)ptr->PhysicalAddressLength))
							{
                                stringBuilder.AppendFormat(":{0:X2}", *(&ptr->PhysicalAddress.FixedElementField + num));
								num++;
							}
							list.Add(new WinNetworkInterfaceInfo
							{
								Description = Marshal.PtrToStringUni(ptr->Description),
								FriendlyName = Marshal.PtrToStringUni(ptr->FriendlyName),
								MacAddress = stringBuilder.ToString()
							});
						}
					}
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
			return list;
		}
		private static List<T> Extract<T>(List<T> source, Func<T, bool> predicate)
		{
			Predicate<T> match = new Predicate<T>(predicate.Invoke);
			List<T> result = source.Where(predicate).ToList<T>();
			source.RemoveAll(match);
			return result;
		}
		internal static string B()
		{
			int num;
			UnsafeNativeMethods.GetVolumeInformation("C:\\", IntPtr.Zero, 0, out num, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0);
			return string.Format("{0:x}", num);
		}
	}
}
