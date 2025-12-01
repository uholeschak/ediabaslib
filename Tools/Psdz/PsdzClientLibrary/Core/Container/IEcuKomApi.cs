
namespace PsdzClient.Core.Container
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuKomApi
    {
        IEcuJob ApiJob(string ecu, string job, string param, string resultFilter = "", int retries = 0, bool fastaActive = true);

        IEcuJob ApiJob(string ecu, string job, string param, int retries, int millisecondsTimeout);

        IEcuJob ApiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter = "", int retries = 0);

        IEcuJob ExecuteJobOverEnet(string icomAddress, string ecu, string job, string param, bool isDopIp, string resultFilter = "", int retries = 0);

        IEcuJob ExecuteJobOverEnetActivateDHCP(string icomAddress, string ecu, string job, string param, bool isDoIP, string resultFilter = "", int retries = 0);
    }
}
