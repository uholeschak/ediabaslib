using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class CbbTlsConfiguratorServiceClient : PsdzClientBase<ICbbTlsConfiguratorService>, ICbbTlsConfiguratorService
    {
        public CbbTlsConfiguratorServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public CbbTlsConfiguratorServiceClient(ChannelFactory<ICbbTlsConfiguratorService> channelFactory)
            : base(channelFactory)
        {
        }

        public bool CreateJks(string pathToJks)
        {
            return CallFunction((ICbbTlsConfiguratorService service) => service.CreateJks(pathToJks));
        }

        public void LoadKeyAndTrustStore(string pKeyStore, string pTrustStore)
        {
            CallMethod(delegate (ICbbTlsConfiguratorService service)
            {
                service.LoadKeyAndTrustStore(pKeyStore, pTrustStore);
            });
        }

        public void UnLoadKeyAndTrustStore()
        {
            CallMethod(delegate (ICbbTlsConfiguratorService service)
            {
                service.UnLoadKeyAndTrustStore();
            });
        }

        public bool AddCertificateToJks(string pathToJks, string aliasOfCertificateInKeystore, byte[] certificate)
        {
            return CallFunction((ICbbTlsConfiguratorService service) => service.AddCertificateToJks(pathToJks, aliasOfCertificateInKeystore, certificate));
        }
    }
}
