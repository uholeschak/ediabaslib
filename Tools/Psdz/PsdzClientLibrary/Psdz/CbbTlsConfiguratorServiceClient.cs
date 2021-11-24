using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    class CbbTlsConfiguratorServiceClient : PsdzClientBase<ICbbTlsConfiguratorService>, ICbbTlsConfiguratorService
    {
        public CbbTlsConfiguratorServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public CbbTlsConfiguratorServiceClient(ChannelFactory<ICbbTlsConfiguratorService> channelFactory) : base(channelFactory)
        {
        }

        public bool CreateJks(string pathToJks)
        {
            return base.CallFunction<bool>((ICbbTlsConfiguratorService service) => service.CreateJks(pathToJks));
        }

        public void LoadKeyAndTrustStore(string pKeyStore, string pTrustStore)
        {
            base.CallMethod(delegate (ICbbTlsConfiguratorService service)
            {
                service.LoadKeyAndTrustStore(pKeyStore, pTrustStore);
            }, true);
        }

        public void UnLoadKeyAndTrustStore()
        {
            base.CallMethod(delegate (ICbbTlsConfiguratorService service)
            {
                service.UnLoadKeyAndTrustStore();
            }, true);
        }

        public bool AddCertificateToJks(string pathToJks, string aliasOfCertificateInKeystore, byte[] certificate)
        {
            return base.CallFunction<bool>((ICbbTlsConfiguratorService service) => service.AddCertificateToJks(pathToJks, aliasOfCertificateInKeystore, certificate));
        }
    }
}
