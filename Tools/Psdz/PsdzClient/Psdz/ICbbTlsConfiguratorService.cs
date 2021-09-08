using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface ICbbTlsConfiguratorService
    {
        [OperationContract]
        bool CreateJks(string pathToJks);

        [OperationContract]
        void LoadKeyAndTrustStore(string keyStore, string trustStore);

        [OperationContract]
        void UnLoadKeyAndTrustStore();

        [OperationContract]
        bool AddCertificateToJks(string pathToJks, string aliasOfCertificateInKeystore, byte[] certificate);
    }
}
