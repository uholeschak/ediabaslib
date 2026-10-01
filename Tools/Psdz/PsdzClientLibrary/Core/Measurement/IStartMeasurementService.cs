using PsdzClient;
using PsdzClient.Core;
using System.Threading.Tasks;

namespace BMW.Rheingold.Measurement.Common
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IStartMeasurementService
    {
        // IDeviceImib Device { get; }

        bool IsConnectedToImib { get; }

        bool IsExistMeasurementPageAndIsMinimized { get; }

        bool IsFreeMeasurementAllowed { get; }

        // IStiManager StiManager { get; }

        bool CheckConnectionToImibInServiceDialog(MeasuringFunction measuringType = MeasuringFunction.None);

        bool CheckImibConnection(IProgressMonitor progressMonitor);

        // void FinishMeasurement(CallingSource callingSource);

        void InitQuickCommandMeasuringType(MeasuringFunction measuringType);

        void LoadLastSettings();

        void NavigateBack();

        // void SaveLastSettings(MeasurementTab lastSelectedMeasurementTab);

        // Task<bool> Start(IProgressMonitor current, CallingSource callingSource);
    }
}
