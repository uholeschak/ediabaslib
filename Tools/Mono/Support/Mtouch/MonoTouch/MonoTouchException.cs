using System;
namespace MonoTouch
{
	public class MonoTouchException : Exception
	{
		public int Code
		{
			get;
			private set;
		}
		public bool Error
		{
			get;
			private set;
		}
		public MonoTouchException(int code, string message, params object[] args) : this(code, false, message, args)
		{
		}
		public MonoTouchException(int code, bool error, string message, params object[] args) : this(code, error, null, message, args)
		{
		}
		public MonoTouchException(int code, bool error, Exception innerException, string message, params object[] args) : base(string.Format(message, args), innerException)
		{
			this.Code = code;
			this.Error = error;
		}
		public override string ToString()
		{
			return string.Format("{0} MT{1:0000}: {2}", this.Error ? "error" : "warning", this.Code, this.Message);
		}
	}
}
