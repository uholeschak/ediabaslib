namespace PsdzRpcClient;

public class VehicleSendEventArgs
{
    public ulong Id { get; }
    public byte[] Data { get; }

    public VehicleSendEventArgs(ulong id, byte[] data)
    {
        Id = id;
        Data = data;
    }
}