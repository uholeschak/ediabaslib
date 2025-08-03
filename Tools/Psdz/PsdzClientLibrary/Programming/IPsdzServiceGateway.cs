using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
    public interface IPsdzServiceGateway
    {
        string PsdzServiceLogFilePath { get; }

        string PsdzLogFilePath { get; }

        void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel);

        void Shutdown();
    }
}
