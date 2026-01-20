using BMW.Rheingold.Psdz.Model;
using PsdzClient;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class StandardFaMapper
    {
        [PreserveSource(Hint = "Added compiler switch", SignatureModified = true)]
        public static IPsdzStandardFa Map(StandardFaModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzStandardFa
            {
                AsString = model.AsString,
                EWords = model.Ewords,
                Entwicklungsbaureihe = model.Entwicklungsbaureihe,
                FaVersion = model.FaVersion,
                HOWords = model.Howords,
                IsValid = model.IsValid,
                Lackcode = model.Lackcode,
                Polstercode = model.Polstercode,
                Salapas = model.Salapas,
                Type = model.Type,
                Zeitkriterium = model.Zeitkriterium,
//[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
                Vin = model.Vin
//[+] #endif
#endif
            };
        }

        [PreserveSource(Hint = "Added compiler switch", SignatureModified = true)]
        public static StandardFaModel Map(IPsdzStandardFa psdzStandardFa)
        {
            if (psdzStandardFa == null)
            {
                return null;
            }

            return new StandardFaModel
            {
                AsString = psdzStandardFa.AsString,
                Ewords = psdzStandardFa.EWords.ToList(),
                Entwicklungsbaureihe = psdzStandardFa.Entwicklungsbaureihe,
                FaVersion = psdzStandardFa.FaVersion,
                Howords = psdzStandardFa.HOWords.ToList(),
                IsValid = psdzStandardFa.IsValid,
                Lackcode = psdzStandardFa.Lackcode,
                Polstercode = psdzStandardFa.Polstercode,
                Salapas = psdzStandardFa.Salapas.ToList(),
                Type = psdzStandardFa.Type,
                Zeitkriterium = psdzStandardFa.Zeitkriterium,
//[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
                Vin = psdzStandardFa.Vin
//[+] #endif
#endif
            };
        }
    }
}