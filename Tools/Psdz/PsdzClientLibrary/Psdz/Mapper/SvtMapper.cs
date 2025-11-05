using System.Linq;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class SvtMapper
    {
        public static IPsdzSvt Map(SvtModel svtModel)
        {
            if (svtModel == null)
            {
                return null;
            }

            return new PsdzSvt
            {
                AsString = svtModel.AsString,
                Ecus = svtModel.Ecus?.Select(EcuMapper.Map),
                HoSignature = svtModel.HoSignature,
                HoSignatureDate = svtModel.HoSignatureDate,
                Version = svtModel.Version,
                IsValid = svtModel.IsValid,
                Vin = svtModel.Vin
            };
        }

        public static SvtModel Map(IPsdzSvt svt)
        {
            if (svt == null)
            {
                return null;
            }

            return new SvtModel
            {
                AsString = svt.AsString,
                Ecus = svt.Ecus?.Select(EcuMapper.Map).ToList(),
                HoSignature = svt.HoSignature,
                HoSignatureDate = svt.HoSignatureDate,
                Version = svt.Version,
                IsValid = svt.IsValid,
                Vin = svt.Vin
            };
        }
    }
}