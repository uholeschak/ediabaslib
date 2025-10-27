using BMW.Rheingold.Psdz.Model;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class FaMapper
    {
        public static IPsdzFa Map(FaModel faModel)
        {
            if (faModel == null)
            {
                return null;
            }
            return new PsdzFa
            {
                Vin = faModel.Vin,
                AsXml = faModel.AsXml,
                AsString = faModel.AsString,
                Entwicklungsbaureihe = faModel.Entwicklungsbaureihe,
                EWords = faModel.Ewords.ToList(),
                FaVersion = faModel.FaVersion,
                HOWords = faModel.Howords,
                Lackcode = faModel.Lackcode,
                Polstercode = faModel.Polstercode,
                Salapas = faModel.Salapas.ToList(),
                Type = faModel.Type,
                IsValid = faModel.IsValid,
                Zeitkriterium = faModel.Zeitkriterium
            };
        }

        public static FaModel Map(IPsdzFa psdzFa)
        {
            if (psdzFa == null)
            {
                return null;
            }
            return new FaModel
            {
                Vin = psdzFa.Vin,
                AsXml = psdzFa.AsXml,
                AsString = psdzFa.AsString,
                Entwicklungsbaureihe = psdzFa.Entwicklungsbaureihe,
                Ewords = psdzFa.EWords.ToList(),
                FaVersion = psdzFa.FaVersion,
                Howords = psdzFa.HOWords.ToList(),
                Lackcode = psdzFa.Lackcode,
                Polstercode = psdzFa.Polstercode,
                Salapas = psdzFa.Salapas.ToList(),
                Type = psdzFa.Type,
                IsValid = psdzFa.IsValid,
                Zeitkriterium = psdzFa.Zeitkriterium
            };
        }
    }
}