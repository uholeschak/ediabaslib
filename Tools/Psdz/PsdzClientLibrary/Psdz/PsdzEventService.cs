using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
	class PsdzEventService
	{
		public PsdzEventService(Binding binding, EndpointAddress endpointAddress)
		{
			this.binding = binding;
			this.endpointAddress = endpointAddress;
			this.eventListenerClients = new Dictionary<IPsdzEventListener, PsdzEventService.PsdzEventListenerClient>();
		}

		public void AddEventListener(IPsdzEventListener psdzEventListener)
		{
			if (psdzEventListener == null)
			{
				return;
			}
			IDictionary<IPsdzEventListener, PsdzEventService.PsdzEventListenerClient> obj = this.eventListenerClients;
			lock (obj)
			{
				if (!this.eventListenerClients.ContainsKey(psdzEventListener))
				{
					PsdzEventService.PsdzEventListenerClient psdzEventListenerClient = new PsdzEventService.PsdzEventListenerClient(psdzEventListener, this.binding, this.endpointAddress);
					psdzEventListenerClient.StartListening();
					this.eventListenerClients.Add(psdzEventListener, psdzEventListenerClient);
				}
			}
		}

		public void RemoveAllEventListener()
		{
			IDictionary<IPsdzEventListener, PsdzEventService.PsdzEventListenerClient> obj = this.eventListenerClients;
			lock (obj)
			{
				while (this.eventListenerClients.Any<KeyValuePair<IPsdzEventListener, PsdzEventService.PsdzEventListenerClient>>())
				{
					IPsdzEventListener psdzEventListener = this.eventListenerClients.Keys.First<IPsdzEventListener>();
					this.RemoveEventListener(psdzEventListener);
				}
			}
		}

		public void RemoveEventListener(IPsdzEventListener psdzEventListener)
		{
			if (psdzEventListener == null)
			{
				return;
			}
			IDictionary<IPsdzEventListener, PsdzEventService.PsdzEventListenerClient> obj = this.eventListenerClients;
			lock (obj)
			{
				if (this.eventListenerClients.ContainsKey(psdzEventListener))
				{
					PsdzEventService.PsdzEventListenerClient psdzEventListenerClient = this.eventListenerClients[psdzEventListener];
					psdzEventListenerClient.StopListening();
					psdzEventListenerClient.Close();
					this.eventListenerClients.Remove(psdzEventListener);
				}
			}
		}

		private readonly Binding binding;

		private readonly EndpointAddress endpointAddress;

		private readonly IDictionary<IPsdzEventListener, PsdzEventService.PsdzEventListenerClient> eventListenerClients;

		private sealed class PsdzEventListenerClient : DuplexClientBase<IEventManagerService>
		{
			public PsdzEventListenerClient(IPsdzEventListener eventListener, Binding binding, EndpointAddress endPointAddress) : base(eventListener, binding, endPointAddress)
			{
			}

			public void StartListening()
			{
				base.Channel.StartListening();
			}

			public void StopListening()
			{
				base.Channel.StopListening();
			}
		}
	}
}
