using RheingoldPsdzWebApi.Adapter.Contracts;

namespace PsdzClient
{
    [PreserveSource(Removed = true)]
    public interface IPsdzServiceGateway
    {
        string PsdzWebServiceLogFilePath { get; }

        string PsdzLogFilePath { get; }

        void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel);

        void Shutdown();
    }
}
