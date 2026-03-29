using PsdzRpcServer.Shared;

namespace PsdzRpcClient;

public class VehicleFunctionsEventArgs
{
    public bool Success { get; }

    public PsdzOperationType OperationType { get; }

    public VehicleFunctionsEventArgs(bool success, PsdzOperationType operationType)
    {
        Success = success;
        OperationType = operationType;
    }
}