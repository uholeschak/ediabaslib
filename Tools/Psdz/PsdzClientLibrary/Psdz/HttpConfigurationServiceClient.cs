using System;
using BMW.Rheingold.Psdz.Client;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz
{
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

        // [UH] For backward compatibility
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
