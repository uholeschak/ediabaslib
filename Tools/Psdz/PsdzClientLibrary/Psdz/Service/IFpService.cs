using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    public interface IFpService
    {
        IPsdzFp parseXml(string pathToXml);
    }
}