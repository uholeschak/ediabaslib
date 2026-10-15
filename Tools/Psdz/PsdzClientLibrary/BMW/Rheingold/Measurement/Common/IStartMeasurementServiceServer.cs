using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Measurement.Common.Contract;
using PsdzClient;

namespace BMW.Rheingold.Measurement.Common
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IStartMeasurementServiceServer : IStartMeasurementService, IMeasurementService
    {
        int ConnectAndReserveImib(IVciDevice device, IFasta2Service fasta2);

        IDmmManager CreateAndInititalizeDmm();

        //IDsoManager CreateAndInitializeDso();

        IDeviceImib ReserveMeasurementDevice(CallingSource callingSource = CallingSource.TestModul);
    }
}
