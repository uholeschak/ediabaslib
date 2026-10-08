using PsdzClient;

namespace BMW.Rheingold.Measurement.Common.Contract
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IDeviceImib
    {
        string ConfigStoragePath { get; }

        // IDeviceDmm DeviceDmm { get; }

        // IDeviceDso Dso { get; }

        // IDeviceBth Bth { get; }

        // IDeviceUsbT UsbT { get; }

        // IDeviceUsbTest UsbTest { get; }

        // IDeviceCnt Cnt { get; }

        // IDeviceSti Sti { get; }

        IDeviceGeneric VirtualDevice { get; }

        IGenericMeasurementDevice GenericMeasurementDevice { get; }

        // IDictionary<int, IEnumerable<MeasuringSensor>> CurrentConnectorToSensor { get; }

        // event DeviceImibEventHandler AvailableSensorsChanged;

        // event EventHandler<ImibExceptionEventArg> ExceptionEventHandler;

        void ShowMessage(string msg, string title);

        void HideMessage();

        bool IsSensorAvailable(string sensor);

        string Version();

        void Reset();

        void Start();

        void Stop(bool commit = true);

        void Status();

        // ushort FindConnector(MeasuringSensor sensorName);

        // JawSwitchState ClampState(MeasuringSensor sensor, Devices deviceToAsk);

        // void CalibrateChannel(int channel, Devices device);

        bool LoadPlugin(string assemblyName, bool currentDomain);

        string GetStatus();
    }
}
