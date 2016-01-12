using Mono.Cecil.Cil;
using System;
using System.Text;
namespace Xamarin.Android
{
	internal class XamarinAndroidException : Exception
	{
		public string MessageWithoutCode
		{
			get;
			private set;
		}
		public int Code
		{
			get;
			private set;
		}
		public SequencePoint Location
		{
			get;
			set;
		}
		public string SourceFile
		{
			get
			{
				return (this.Location != null) ? this.Location.Document.Url : null;
			}
		}
		public int SourceLine
		{
			get
			{
				return (this.Location != null) ? this.Location.StartLine : 0;
			}
		}
		public XamarinAndroidException(int code, string message, params object[] args) : this(code, null, message, args)
		{
		}
		public XamarinAndroidException(int code, Exception innerException, string message, params object[] args) : base(XamarinAndroidException.GetMessage(code, message, args), innerException)
		{
			this.Code = code;
			this.MessageWithoutCode = string.Format(message, args);
		}
		private static string GetMessage(int code, string message, object[] args)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("error ");
			stringBuilder.AppendFormat("XA{0:0000}", code);
			stringBuilder.Append(": ");
			stringBuilder.AppendFormat(message, args);
			return stringBuilder.ToString();
		}
	}
}
