using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzProgressListener
    {
        [OperationContract(IsOneWay = true)]
        void BeginTask(string task);

        [OperationContract(IsOneWay = true)]
        void SetDuration(long milliseconds);

        [OperationContract(IsOneWay = true)]
        void SetElapsedTime(long milliseconds);

        [OperationContract(IsOneWay = true)]
        void SetFinished();
    }
}
