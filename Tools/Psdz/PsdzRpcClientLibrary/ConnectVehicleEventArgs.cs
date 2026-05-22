namespace PsdzRpcClient;

public class ConnectVehicleEventArgs
{
    public bool Success { get; }
    public string Vin { get; }
    public bool LicenseValid { get; }

    public ConnectVehicleEventArgs(bool success, string vin, bool licenseValid)
    {
        Success = success;
        Vin = vin;
        LicenseValid = licenseValid;
    }
}
