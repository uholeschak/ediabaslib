namespace PsdzClient.Psdz
{
    public enum ConnectToVehicleErrorCodes
    {
        S29Error = 0,
        Sec4DiagError = 1,
        EdiabasInitError = 2,
        IstaIsOffline = 3,
        WritePermissionError = 4,
        UnexpectedError = 5,
        MissmatchVinOperationCancelledError = 6,
        DoIpIsUsedByOtherOperationError = 7,
        NewEdiabasVersionWithoutNcarVehicle = 8,
        EdiabasPemFileNotFound = 9,
        IpbValidationFailed = 10,
        Sec4CnAuthError = 11,
        Undefined = -1,
        IcomNetworkFailure = -2
    }
}