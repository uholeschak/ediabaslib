using System;
using BMW.Rheingold.Measurement.Common.Data;

namespace BMW.Rheingold.Measurement.Common.Contract
{
    public delegate void DeviceImibEventHandler(object sender, DeviceImibEventArgs eventData);

    public interface IDmmManager
    {
        DmmChannel[] Model { get; }

        DmmModel DmmModel { get; }

        event EventHandler<InfoEventArgs> InfoChanged;

        event DeviceImibEventHandler AvailableSensorsChanged;

        event EventHandler<ImibExceptionEventArg> ExceptionEventHandler;

        DmmMeasuredValueMinMax MeasuredValue(int channel);

        void ToggleHold(int dmmNo);

        void LoadModel(MeasuringConfigurationType config);

        string FunctionName(int channel);

        void Start();

        void Stop(bool commit = true);

        void ActivateMinMax(int channelNumber, bool activate);

        void SetLastSettings(bool[] selectSourceWasSuccessful);

        void SetConfiguration(MeasuringConfigurationType configuration);

        void FillConfiguration();

        void InterruptCalibration();

        void SetFunction(int dmmNo, MeasuringFunction functionName, MeasuringCoupling couplingName);

        bool IsSelectable(MeasuringSensor sensor, int dmmNo);

        void SetRange(string range, int dmmNo);

        void SetSensor(int dmmNo, MeasuringSensor sensorName);

        void SetSensor(bool modelChangedPar, int dmmNo, MeasuringSensor sensorName, string range);

        void CalibrateClamp(int dmmNo, MeasuringSensor sensor);
    }
}
