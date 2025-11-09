using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal class PsdzEventService
    {
        [PreserveSource(Removed = true)]
        private sealed class PsdzEventListenerClient : DuplexClientBase<IEventManagerService>
        {
            public PsdzEventListenerClient(IPsdzEventListener eventListener, Binding binding, EndpointAddress endPointAddress)
                : base((object)eventListener, binding, endPointAddress)
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

        private readonly Binding binding;

        private readonly EndpointAddress endpointAddress;

        private readonly IDictionary<IPsdzEventListener, PsdzEventListenerClient> eventListenerClients;

        public PsdzEventService(Binding binding, EndpointAddress endpointAddress)
        {
            this.binding = binding;
            this.endpointAddress = endpointAddress;
            eventListenerClients = new Dictionary<IPsdzEventListener, PsdzEventListenerClient>();
        }

        public void AddEventListener(IPsdzEventListener psdzEventListener)
        {
            if (psdzEventListener == null)
            {
                return;
            }
            lock (eventListenerClients)
            {
                if (!eventListenerClients.ContainsKey(psdzEventListener))
                {
                    PsdzEventListenerClient psdzEventListenerClient = new PsdzEventListenerClient(psdzEventListener, binding, endpointAddress);
                    psdzEventListenerClient.StartListening();
                    eventListenerClients.Add(psdzEventListener, psdzEventListenerClient);
                }
            }
        }

        public void RemoveAllEventListener()
        {
            lock (eventListenerClients)
            {
                while (eventListenerClients.Any())
                {
                    IPsdzEventListener psdzEventListener = eventListenerClients.Keys.First();
                    RemoveEventListener(psdzEventListener);
                }
            }
        }

        public void RemoveEventListener(IPsdzEventListener psdzEventListener)
        {
            if (psdzEventListener == null)
            {
                return;
            }
            lock (eventListenerClients)
            {
                if (eventListenerClients.ContainsKey(psdzEventListener))
                {
                    PsdzEventListenerClient psdzEventListenerClient = eventListenerClients[psdzEventListener];
                    psdzEventListenerClient.StopListening();
                    psdzEventListenerClient.Close();
                    eventListenerClients.Remove(psdzEventListener);
                }
            }
        }
    }
}
