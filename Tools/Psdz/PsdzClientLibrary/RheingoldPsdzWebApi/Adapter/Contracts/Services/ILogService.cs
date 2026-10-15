using PsdzClient;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Exceptions;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface ILogService
    {
        void PrepareLoggingForCurrentThread();

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string ClosePsdzLog();

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void SetLogLevel(PsdzLoglevel psdzLoglevel);
    }
}
