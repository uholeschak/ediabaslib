namespace PsdzRpcClient;

public class ConnectVehicleEventArgs
{
    public bool Success { get; }
    public string Vin { get; }

    public ConnectVehicleEventArgs(bool success, string vin)
    {
        Success = success;
        Vin = vin;
    }
}
