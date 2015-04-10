using System;
using System.IO;
namespace Mono.Touch.Activation.Common
{
	public class Crypto
	{
		public Crypto(string certificate_file, string pvk_file, string pvk_password)
		{
		}
		public Crypto(byte[] cert_bytes, byte[] pvk_bytes, string pvk_password)
		{
		}
		public byte[] EncryptAndSign(byte[] data)
		{
			return data;
		}
		public byte[] DecryptAndVerify(byte[] data)
		{
			return data;
		}
		public byte[] Sign(byte[] data)
		{
			return data;
		}
		public byte[] Sign(Stream input)
		{
			return null;
		}
		public byte[] Verify(byte[] data)
		{
			return data;
		}
	}
}
