using BMW.Rheingold.Psdz.Model.Exceptions;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    public interface IHttpConfigurationService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        int GetHttpServerPort();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetNetworkEndpointSet();

        void SetHttpServerAddress(string address);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void SetHttpServerPort(int port);
    }
}
