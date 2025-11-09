using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClientLibrary;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
	abstract class PsdzClientBase<TChannel> where TChannel : class
	{
		protected PsdzClientBase(Binding binding, EndpointAddress remoteAddress) : this(new ChannelFactory<TChannel>(binding, remoteAddress))
		{
		}

		protected PsdzClientBase(ChannelFactory<TChannel> channelFactory)
		{
			this.channelFactory = channelFactory;
		}

		public void CloseCachedChannels()
		{
			Queue<TChannel> obj = this.cachedChannels;
			lock (obj)
			{
				while (this.cachedChannels.Any<TChannel>())
				{
					PsdzClientBase<TChannel>.CloseChannel(this.cachedChannels.Dequeue());
				}
			}
		}

		protected async Task<TResult> CallFunctionAsync<TResult>(Func<TChannel, Task<TResult>> func)
		{
			TChannel channel = default(TChannel);
			TResult result;
			try
			{
				channel = this.GetChannel();
				result = await func(channel);
			}
			catch (FaultException<ArgumentException> faultException)
			{
				throw faultException.Detail;
			}
			catch (FaultException<FileNotFoundException> faultException2)
			{
				throw faultException2.Detail;
			}
			catch (FaultException<PsdzRuntimeException> faultException3)
			{
				throw faultException3.Detail;
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				throw;
			}
			catch (TimeoutException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (!this.EnqueueIfOpened(channel))
				{
					PsdzClientBase<TChannel>.CloseChannel(channel);
				}
			}
			return result;
		}

		protected TResult CallFunction<TResult>(Func<TChannel, TResult> func)
		{
			TChannel tchannel = default(TChannel);
			TResult result;
			try
			{
				tchannel = this.GetChannel();
				result = func(tchannel);
			}
			catch (FaultException<ArgumentException> faultException)
			{
				throw faultException.Detail;
			}
			catch (FaultException<FileNotFoundException> faultException2)
			{
				throw faultException2.Detail;
			}
			catch (FaultException<PsdzRuntimeException> faultException3)
			{
				throw faultException3.Detail;
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				throw;
			}
			catch (TimeoutException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (!this.EnqueueIfOpened(tchannel))
				{
					PsdzClientBase<TChannel>.CloseChannel(tchannel);
				}
			}
			return result;
		}

		protected void CallMethod(Action<TChannel> func, bool cacheChannel = true)
		{
			TChannel tchannel = default(TChannel);
			try
			{
				tchannel = this.GetChannel();
				func(tchannel);
			}
			catch (FaultException<ArgumentException> faultException)
			{
				throw faultException.Detail;
			}
			catch (FaultException<FileNotFoundException> faultException2)
			{
				throw faultException2.Detail;
			}
			catch (FaultException<PsdzRuntimeException> faultException3)
			{
				throw faultException3.Detail;
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				throw;
			}
			catch (TimeoutException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (!cacheChannel || !this.EnqueueIfOpened(tchannel))
				{
					PsdzClientBase<TChannel>.CloseChannel(tchannel);
				}
			}
		}

		private static void CloseChannel(TChannel channel)
		{
			ICommunicationObject communicationObject = channel as ICommunicationObject;
			if (communicationObject == null)
			{
				return;
			}
			if (communicationObject.State == CommunicationState.Faulted)
			{
				communicationObject.Abort();
				return;
			}
			communicationObject.Close();
		}

		private bool EnqueueIfOpened(TChannel channel)
		{
			if (channel != null)
			{
				if (((ICommunicationObject)((object)channel)).State == CommunicationState.Opened)
				{
					Queue<TChannel> obj = this.cachedChannels;
					lock (obj)
					{
						this.cachedChannels.Enqueue(channel);
					}
					return true;
				}
			}
			return false;
		}

		private TChannel GetChannel()
		{
			Queue<TChannel> obj = this.cachedChannels;
			lock (obj)
			{
				if (this.cachedChannels.Any<TChannel>())
				{
					return this.cachedChannels.Dequeue();
				}
			}
			return this.channelFactory.CreateChannel();
		}

		private readonly Queue<TChannel> cachedChannels = new Queue<TChannel>();

		private readonly ChannelFactory<TChannel> channelFactory;
	}
}
