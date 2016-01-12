using Mono.Options;
using MonoTouch;
using System;
using Xamarin.Licensing;
internal class MTouch
{
	private static int Main(string[] args)
	{
		try
		{
			MTouch.Main2(args);
		}
		catch (Exception e)
		{
			ErrorHelper.Show(e);
		}
		return 0;
	}
	private static void Main2(string[] args)
	{
		OptionSet optionSet = new OptionSet();
		PlatformActivation.AddOptions(optionSet);
		optionSet.Parse(args);
		PlatformActivation.ProcessOptions();
	}
}
