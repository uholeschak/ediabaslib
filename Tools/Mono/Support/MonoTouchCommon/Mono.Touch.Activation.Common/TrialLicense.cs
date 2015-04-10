using System;
namespace Mono.Touch.Activation.Common
{
	public class TrialLicense
	{
		public UserData UserData
		{
			get
			{
				return null;
			}
		}
		public DateTime Expires
		{
			get
			{
				return DateTime.MaxValue;
			}
		}
		private TrialLicense(UserData user, DateTime expires)
		{
		}
		public static TrialLicense LoadFromFile(Crypto crypto, string filename)
		{
			return new TrialLicense(new UserData(), DateTime.MaxValue);
		}
		public static TrialLicense LoadFromBytes(Crypto crypto, byte[] bytes)
		{
			return new TrialLicense(new UserData(), DateTime.MaxValue);
		}
	}
}
