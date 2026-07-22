using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;
using PsdzClient.Core;

namespace BMW.Rheingold.Measurement.Common
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IStartMeasurementServiceServer
    {
        int ConnectAndReserveImib(IVciDevice device, IFasta2Service fasta2);
    }
}
