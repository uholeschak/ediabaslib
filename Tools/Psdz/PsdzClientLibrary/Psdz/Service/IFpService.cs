using BMW.Rheingold.Psdz.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    public interface IFpService
    {
        IPsdzFp parseXml(string pathToXml);
    }
}