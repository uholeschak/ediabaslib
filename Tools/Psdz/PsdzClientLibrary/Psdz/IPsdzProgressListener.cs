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
        // [UH] Keep operation contract for backward compatibility
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
