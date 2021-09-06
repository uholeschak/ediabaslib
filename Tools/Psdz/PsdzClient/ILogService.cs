using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public enum PsdzLoglevel
    {
        INFO = 1,
        FINE,
        DEBUG,
        TRACE,
        DEEP_TRACE
    }

    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface ILogService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string ClosePsdzLog();

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        void SetLogLevel(PsdzLoglevel psdzLoglevel);
    }
}
