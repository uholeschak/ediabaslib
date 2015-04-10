using System;
namespace Mono.Touch.Activation.Common
{
	public class LoginMessage
	{
		public string UserName;
		public long Last;
		public byte[] Nonce
		{
			get;
			set;
		}
		public byte[] Serialize()
		{
			return null;
		}
		public void Deserialize(byte[] bytes)
		{
		}
		public static LoginMessage FromBytes(byte[] bytes)
		{
			return new LoginMessage();
		}
		public static byte[] ToBytes(LoginMessage msg)
		{
			return null;
		}
	}
}
