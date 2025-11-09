using BMW.Rheingold.Psdz.Client;
using PsdzClientLibrary;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(Removed = true)]
    internal sealed class HttpConfigurationServiceClient : PsdzDuplexClientBase<IHttpConfigurationService, IPsdzProgressListener>, IHttpConfigurationService
    {
        internal HttpConfigurationServiceClient(IPsdzProgressListener callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : base(callbackInstance, binding, remoteAddress)
        {
        }

        public int GetHttpServerPort()
        {
            return CallFunction((IHttpConfigurationService service) => service.GetHttpServerPort());
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public void SetHttpServerAddress(string address)
        {
            throw new NotImplementedException();
        }

        public void SetHttpServerPort(int port)
        {
            CallMethod(delegate (IHttpConfigurationService service)
            {
                service.SetHttpServerPort(port);
            });
        }

        public string GetNetworkEndpointSet()
        {
            return CallFunction((IHttpConfigurationService service) => service.GetNetworkEndpointSet());
        }
    }
}
