using System;
namespace Mono.Touch.Activation.Common
{
	[Serializable]
	public class UserData
	{
		public byte[] H1;
		public byte[] H2;
		public byte[] H3;
		public string Field1;
		public string Field2;
		public string Name
		{
			get;
			set;
		}
		public string Email
		{
			get;
			set;
		}
		public string Company
		{
			get;
			set;
		}
		public string Phone
		{
			get;
			set;
		}
		public string ActivationCode
		{
			get;
			set;
		}
		public string DataFile
		{
			get;
			set;
		}
		public string UpgradeCode
		{
			get;
			set;
		}
		public DateTime ExpirationDate
		{
			get;
			set;
		}
		public int ProductVersion
		{
			get;
			set;
		}
		public string Opaque
		{
			get;
			set;
		}
		public ProductId ProductId
		{
			get;
			set;
		}
		public UserData()
		{
			this.ProductVersion = 5;
		}
		public UserData(UserData from, bool keep_extras)
		{
		}
		public byte[] Serialize()
		{
			return null;
		}
		public void Deserialize(byte[] bytes)
		{
		}
		public override string ToString()
		{
			return null;
		}
		public static UserData FromBytes(byte[] bytes)
		{
			return null;
		}
		public static byte[] ToBytes(UserData user_data)
		{
			return null;
		}
	}
}
