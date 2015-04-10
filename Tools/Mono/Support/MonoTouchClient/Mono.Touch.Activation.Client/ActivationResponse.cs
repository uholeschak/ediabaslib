using System;
namespace Mono.Touch.Activation.Client
{
	public class ActivationResponse
	{
		public ActivationResponseCode ResponseCode
		{
			get
			{
				return ActivationResponseCode.Success;
			}
			set
			{
			}
		}
		public byte[] License
		{
			get;
			set;
		}
		public string Message
		{
			get;
			set;
		}
		public string Description
		{
			get;
			set;
		}
	}
}
