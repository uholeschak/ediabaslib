using PsdzClient;
using System.ServiceModel;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface IPsdzProgressListener
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract(IsOneWay = true)]
        void BeginTask(string task);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract(IsOneWay = true)]
        void SetDuration(long milliseconds);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract(IsOneWay = true)]
        void SetElapsedTime(long milliseconds);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract(IsOneWay = true)]
        void SetFinished();
    }
}
