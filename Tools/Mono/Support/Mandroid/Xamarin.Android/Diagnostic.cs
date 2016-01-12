using Mono.Cecil.Cil;
using MonoDroid.Utils;
using System;
using System.IO;
namespace Xamarin.Android
{
	internal static class Diagnostic
	{
		public static void Error(int code, SequencePoint location, string message, params object[] args)
		{
			throw new XamarinAndroidException(code, message, args)
			{
				Location = location
			};
		}
		public static void Error(int code, string message, params object[] args)
		{
			throw new XamarinAndroidException(code, message, args);
		}
		public static void Error(int code, Exception innerException, string message, params object[] args)
		{
			throw new XamarinAndroidException(code, innerException, message, args);
		}
		public static void Warning(int code, string message, params object[] args)
		{
			Console.Error.Write("mandroid: warning XA{0:0000}: ", code);
			Console.Error.WriteLine(message, args);
		}
		public static void WriteTo(TextWriter destination, Exception message, bool verbose = false)
		{
			CommandFailedException ex = message as CommandFailedException;
			if (ex != null)
			{
				if (verbose)
				{
					destination.WriteLine(ex.ToString());
				}
				destination.WriteLine(ex.VSFormattedErrorLog);
			}
			else
			{
				XamarinAndroidException ex2 = message as XamarinAndroidException;
				if (ex2 != null)
				{
					destination.WriteLine("monodroid: {0}", ex2.Message);
					if (verbose && ex2.Code < 9000)
					{
						destination.WriteLine("monodroid: {0}", ex2.ToString());
					}
				}
				else
				{
					destination.WriteLine("monodroid: error XA0000: Unexpected error - Please file a bug report at http://bugzilla.xamarin.com. Reason: {0}", (!verbose) ? message.ToString() : message.Message);
				}
			}
		}
	}
}
