using System;
using System.Collections.Generic;
using System.Text;
using Mono.Options;
using Xamarin.Android;
//using Xamarin.Android.Tools;
using Xamarin.Licensing;

namespace Monodroid
{
	public class MainClass
	{
		public static int Main(string[] argv)
		{
			try
			{
                OptionSet optionSet = new OptionSet();
                argv = optionSet.Parse(argv).ToArray();
                PlatformActivation.AddOptions(optionSet);
                try
                {
                    optionSet.Parse(argv);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("monodroid: {0}", ex.Message);
                    Console.Error.WriteLine("See 'monodroid --help' for more information.");
                    return 1;
                }
                if (!PlatformActivation.ProcessOptions())
                {
                    return 1;
                }
            }
			catch (Exception message)
			{
                Diagnostic.WriteTo(Console.Error, message, true);
                return 1;
			}
			return 0;
		}
    }
}
