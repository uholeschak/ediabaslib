using Mono.Touch.Activation.Common;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Mono.Touch.Activation.Client
{
	public class Activation
	{
		private Crypto crypto;
		private ActivationService service;
		public ActivationService Service
		{
			get
			{
				return this.service;
			}
		}
		public Crypto Crypto
		{
			get
			{
				return this.crypto;
			}
		}
		public Activation()
		{
		}
		public Activation(byte[] cert_bytes, byte[] pvk_bytes, string pvk_password, string activation_url) : this(cert_bytes, pvk_bytes, pvk_password)
		{
		}
		public Activation(string cert_file, string pvk_file, string pvk_password)
		{
			this.crypto = new Crypto(cert_file, pvk_file, pvk_password);
			this.Init();
		}
		public Activation(byte[] cert_bytes, byte[] pvk_bytes, string pvk_password)
		{
			this.crypto = new Crypto(cert_bytes, pvk_bytes, pvk_password);
			this.Init();
		}
		private void Init()
		{
			this.service = new ActivationService();
		}
		private byte[] GetAuthInfo()
		{
			return new byte[1];
		}
		public ActivationResponse Activate(UserData user_data)
		{
			return new ActivationResponse
			{
				ResponseCode = ActivationResponseCode.Success
			};
		}
		public ActivationResponse Activate2(UserData user_data)
		{
			return new ActivationResponse
			{
				ResponseCode = ActivationResponseCode.Success
			};
		}
		public ActivationResponse Activate3(string product, UserData user_data)
		{
			string s;
			string s2;
			byte[] array;
			License.GetSAM(user_data.DataFile, out s, out s2, out array);
			ActivationResponse result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(product);
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				byte[] bytes2 = Encoding.UTF8.GetBytes(s2);
				using (MemoryStream memoryStream2 = new MemoryStream(bytes.Length + bytes2.Length + array.Length))
				{
					using (HashAlgorithm hashAlgorithm = SHA1.Create())
					{
						byte[] hash = License.GetHash(hashAlgorithm, memoryStream2, bytes, bytes2, array);
						binaryWriter.Write(hash.Length);
						binaryWriter.Write(hash);
						hash = License.GetHash(hashAlgorithm, memoryStream2, bytes, array, bytes2);
						binaryWriter.Write(hash.Length);
						binaryWriter.Write(hash);
						hash = License.GetHash(hashAlgorithm, memoryStream2, array, bytes, bytes2);
						binaryWriter.Write(hash.Length);
						binaryWriter.Write(hash);
					}
				}
				binaryWriter.Flush();
				result = new ActivationResponse
				{
					ResponseCode = ActivationResponseCode.Success,
					License = memoryStream.ToArray()
				};
			}
			return result;
		}
	}
}
