using System;
namespace Mono.Touch.Activation.Client
{
	public class LoginMessage
	{
		public string UserName
		{
			get;
			set;
		}
		public long Last
		{
			get;
			set;
		}
		public byte[] Nonce
		{
			get;
			set;
		}
	}
}
