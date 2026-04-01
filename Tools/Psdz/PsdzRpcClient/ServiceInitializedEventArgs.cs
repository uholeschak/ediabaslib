namespace PsdzRpcClient;

public class ServiceInitializedEventArgs
{
    public ServiceInitializedEventArgs(string hostLogDir, bool loggingInitialized)
    {
        HostLogDir = hostLogDir;
        LoggingInitialized = loggingInitialized;
    }

    public string HostLogDir { get; }
    public bool LoggingInitialized { get; }
}