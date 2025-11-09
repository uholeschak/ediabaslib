using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
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
