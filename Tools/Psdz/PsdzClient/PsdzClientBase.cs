using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	abstract class PsdzClientBase<TChannel> where TChannel : class
	{
		// Token: 0x06000010 RID: 16 RVA: 0x00002163 File Offset: 0x00000363
		protected PsdzClientBase(Binding binding, EndpointAddress remoteAddress) : this(new ChannelFactory<TChannel>(binding, remoteAddress))
		{
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002172 File Offset: 0x00000372
		protected PsdzClientBase(ChannelFactory<TChannel> channelFactory)
		{
			this.channelFactory = channelFactory;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000328C File Offset: 0x0000148C
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

		// Token: 0x06000013 RID: 19 RVA: 0x000032E8 File Offset: 0x000014E8
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

		// Token: 0x06000014 RID: 20 RVA: 0x00003338 File Offset: 0x00001538
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

		// Token: 0x06000015 RID: 21 RVA: 0x000033F4 File Offset: 0x000015F4
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

		// Token: 0x06000016 RID: 22 RVA: 0x000034B4 File Offset: 0x000016B4
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

		// Token: 0x06000017 RID: 23 RVA: 0x000034E8 File Offset: 0x000016E8
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

		// Token: 0x06000018 RID: 24 RVA: 0x00003550 File Offset: 0x00001750
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

		// Token: 0x04000009 RID: 9
		private readonly Queue<TChannel> cachedChannels = new Queue<TChannel>();

		// Token: 0x0400000A RID: 10
		private readonly ChannelFactory<TChannel> channelFactory;
	}
}
