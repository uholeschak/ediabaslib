using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    public enum PsdzLoglevel
    {
        INFO = 1,
        FINE,
        DEBUG,
        TRACE,
        DEEP_TRACE
    }

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
