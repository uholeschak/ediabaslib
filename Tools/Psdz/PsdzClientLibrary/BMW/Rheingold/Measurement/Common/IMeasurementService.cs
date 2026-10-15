using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Measurement.Common
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    [ServiceContract]
    public interface IMeasurementService
    {
        [OperationContract]
        void DisconnectImib(bool force = false, bool disconnectedByImib = false);

        [OperationContract]
        bool InitializeImib();

        [OperationContract]
        void ReleaseDmmManagement();

        // [OperationContract]
        // void CallStiFunction(StiFunction stiFunction, object value);

        // [OperationContract]
        // void DmmCall(DmmFunction funtion, object value);

        // [OperationContract]
        // void DmmSetSensor(DmmSensorData dmmSensorData);

        // [OperationContract]
        // void MeasurementCall(MeasurementFunction function, object data);

        // [OperationContract]
        // Task<object> CallDsoFunctionAsyncronAsync(DsoFunction function, object value);

        // [OperationContract]
        // object CallDsoFunction(DsoFunction function, object value);

        // [OperationContract]
        // object ImibDeviceCall(ImibDeviceFunction funtion, object value);

        [OperationContract]
        void ShowMessage(string msg, string title);

        // [OperationContract]
        // JawSwitchState ClampState(MeasuringSensor sensor, Devices deviceToAsk);

        // [OperationContract]
        // void CalibrateClamp(int dmmNo, MeasuringSensor sensor);
    }
}
