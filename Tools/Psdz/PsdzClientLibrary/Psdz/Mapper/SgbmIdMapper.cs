using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class SgbmIdMapper
    {
        public static IPsdzSgbmId Map(SgbmIdModel sgbmIdModel)
        {
            if (sgbmIdModel == null)
            {
                return null;
            }

            return new PsdzSgbmId
            {
                Id = sgbmIdModel.Id,
                HexString = sgbmIdModel.HexString,
                IdAsLong = sgbmIdModel.IdAsLong,
                MainVersion = sgbmIdModel.MainVersion,
                PatchVersion = sgbmIdModel.PatchVersion,
                ProcessClass = sgbmIdModel.ProcessClass,
                SubVersion = sgbmIdModel.SubVersion
            };
        }

        public static SgbmIdModel Map(IPsdzSgbmId sgbmId)
        {
            if (sgbmId == null)
            {
                return null;
            }

            return new SgbmIdModel
            {
                Id = sgbmId.Id,
                HexString = sgbmId.HexString,
                IdAsLong = sgbmId.IdAsLong,
                MainVersion = sgbmId.MainVersion,
                PatchVersion = sgbmId.PatchVersion,
                ProcessClass = sgbmId.ProcessClass,
                SubVersion = sgbmId.SubVersion
            };
        }
    }
}