using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
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
