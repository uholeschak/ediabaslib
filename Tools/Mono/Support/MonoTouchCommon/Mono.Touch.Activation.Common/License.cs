using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
namespace Mono.Touch.Activation.Common
{
	public class License
	{
		private const int HASHES_LENGTH = 60;
		public UserData UserData
		{
			get;
			private set;
		}
		public License(UserData user)
		{
			this.UserData = user;
		}
		public static void GetSAM(string dataFile, out string sn, out string ha, out byte[] salt)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(new StringReader(dataFile));
			sn = License.GetNodeText(xmlDocument, "/plist/array/dict/array/dict/key", "serial_number");
			ha = License.GetNodeText(xmlDocument, "/plist/array/dict/array/dict/array/dict/key", "hardware_address");
			Type type = Type.GetType("Xamarin.Licensing.Certificates, " + Assembly.GetEntryAssembly().FullName);
			salt = (byte[])type.GetField("Salt").GetValue(null);
		}
		private static string GetNodeText(XmlDocument doc, string path, string key)
		{
			XmlNodeList xmlNodeList = doc.SelectNodes(path);
			if (xmlNodeList != null)
			{
				foreach (XmlNode xmlNode in xmlNodeList)
				{
					if (!(xmlNode.InnerText != key))
					{
						XmlNode nextSibling = xmlNode.NextSibling;
						if (nextSibling != null)
						{
							return (!(nextSibling.Name != "string")) ? nextSibling.InnerText : null;
						}
					}
				}
			}
			return null;
		}
		public static byte[] GetHash(HashAlgorithm alg, MemoryStream ms, byte[] b1, byte[] b2, byte[] b3)
		{
			ms.Position = 0L;
			ms.SetLength(0L);
			ms.Write(b1, 0, b1.Length);
			ms.Write(b2, 0, b2.Length);
			ms.Write(b3, 0, b3.Length);
			return alg.ComputeHash(ms.GetBuffer(), 0, (int)ms.Length);
		}
		private static UserData CreateUserDataHashes(byte[] data = null)
		{
			UserData userData = new UserData
			{
				ExpirationDate = DateTime.Now.Add(TimeSpan.FromDays(180.0))
			};
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				BinaryReader binaryReader = new BinaryReader(memoryStream);
				string text = binaryReader.ReadString().ToUpperInvariant();
				if (text != null)
				{
					if (!(text == "MA"))
					{
						if (!(text == "MT"))
						{
							if (text == "MM")
							{
								userData.ProductId = ProductId.XamarinMacEnterprisePrio;
							}
						}
						else
						{
							userData.ProductId = ProductId.MonoTouchEnterprisePrio;
						}
					}
					else
					{
						userData.ProductId = ProductId.MAEnterprisePrio;
					}
				}
				userData.H1 = binaryReader.ReadBytes(binaryReader.ReadInt32());
				userData.H2 = binaryReader.ReadBytes(binaryReader.ReadInt32());
				userData.H3 = binaryReader.ReadBytes(binaryReader.ReadInt32());
			}
			return userData;
		}
		public static License LoadFromFile(Crypto crypto, string filename)
		{
			byte[] array = null;
			using (Stream stream = File.OpenRead(filename))
			{
				if (stream.Length > 32768L)
				{
					throw new ArgumentException("File too big", "filename");
				}
				int num = (int)stream.Length;
				array = new byte[num];
				stream.Read(array, 0, num);
			}
			return new License(License.CreateUserDataHashes(array));
		}
		public static License LoadFromBytes(Crypto crypto, byte[] bytes)
		{
			return new License(License.CreateUserDataHashes(bytes));
		}
	}
}
