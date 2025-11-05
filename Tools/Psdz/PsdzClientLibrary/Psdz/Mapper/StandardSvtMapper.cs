using System.Linq;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class StandardSvtMapper
    {
        public static IPsdzStandardSvt Map(StandardSvtModel standardSvtModel)
        {
            if (standardSvtModel == null)
            {
                return null;
            }

            return new PsdzStandardSvt
            {
                AsString = standardSvtModel.AsString,
                Ecus = standardSvtModel.Ecus?.Select(EcuMapper.Map),
                HoSignature = standardSvtModel.HoSignature,
                HoSignatureDate = standardSvtModel.HoSignatureDate,
                Version = standardSvtModel.Version
            };
        }

        public static StandardSvtModel Map(IPsdzStandardSvt standardSvt)
        {
            if (standardSvt == null)
            {
                return null;
            }

            return new StandardSvtModel
            {
                AsString = standardSvt.AsString,
                Ecus = standardSvt.Ecus?.Select(EcuMapper.Map).ToList(),
                HoSignature = standardSvt.HoSignature,
                HoSignatureDate = standardSvt.HoSignatureDate,
                Version = standardSvt.Version
            };
        }
    }
}