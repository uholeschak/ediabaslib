using System;
namespace Mono.Touch.Activation.Client
{
	public class ActivationService
	{
		public string Url
		{
			get;
			set;
		}
		public LoginMessage Login(string user_name, byte[] data)
		{
			return null;
		}
		public IAsyncResult BeginLogin(string user_name, byte[] data, AsyncCallback callback, object asyncState)
		{
			return null;
		}
		public LoginMessage EndLogin(IAsyncResult asyncResult)
		{
			return null;
		}
		public void LoginAsync(string user_name, byte[] data)
		{
		}
		public void LoginAsync(string user_name, byte[] data, object userState)
		{
		}
		public ActivationResponse Activate(byte[] data)
		{
			return null;
		}
		public IAsyncResult BeginActivate(byte[] data, AsyncCallback callback, object asyncState)
		{
			return null;
		}
		public ActivationResponse EndActivate(IAsyncResult asyncResult)
		{
			return null;
		}
		public void ActivateAsync(byte[] data)
		{
		}
		public void ActivateAsync(byte[] data, object userState)
		{
		}
		public ActivationResponse Activate2(byte[] data)
		{
			return null;
		}
		public IAsyncResult BeginActivate2(byte[] data, AsyncCallback callback, object asyncState)
		{
			return null;
		}
		public ActivationResponse EndActivate2(IAsyncResult asyncResult)
		{
			return null;
		}
		public void Activate2Async(byte[] data)
		{
		}
		public void Activate2Async(byte[] data, object userState)
		{
		}
		public ActivationResponse Activate3(string product, byte[] data)
		{
			return null;
		}
		public IAsyncResult BeginActivate3(string product, byte[] data, AsyncCallback callback, object asyncState)
		{
			return null;
		}
		public ActivationResponse EndActivate3(IAsyncResult asyncResult)
		{
			return null;
		}
		public void Activate3Async(string product, byte[] data)
		{
		}
		public void Activate3Async(string product, byte[] data, object userState)
		{
		}
	}
}
