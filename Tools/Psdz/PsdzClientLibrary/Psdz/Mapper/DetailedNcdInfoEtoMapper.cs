using BMW.Rheingold.Psdz.Model.SecureCoding;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class DetailedNcdInfoEtoMapper
    {
        private static readonly NcdStatusEtoEnumMapper ncdStatusEtoEnumMapper = new NcdStatusEtoEnumMapper();
        public static IPsdzDetailedNcdInfoEto Map(DetailedNcdInfoEtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzDetailedNcdInfoEto
            {
                Btld = SgbmIdMapper.Map(model.Btld),
                Cafd = SgbmIdMapper.Map(model.Cafd),
                CodingVersion = model.CodingVersion,
                DiagAdresses = model.DiagAddresses?.Select(DiagAddressCtoMapper.Map).ToList(),
                NcdStatus = ncdStatusEtoEnumMapper.GetValue(model.NcdStatus)
            };
        }

        public static DetailedNcdInfoEtoModel Map(IPsdzDetailedNcdInfoEto psdzDetailedNcdInfoEto)
        {
            if (psdzDetailedNcdInfoEto == null)
            {
                return null;
            }

            return new DetailedNcdInfoEtoModel
            {
                Btld = SgbmIdMapper.Map(psdzDetailedNcdInfoEto.Btld),
                Cafd = SgbmIdMapper.Map(psdzDetailedNcdInfoEto.Cafd),
                CodingVersion = psdzDetailedNcdInfoEto.CodingVersion,
                DiagAddresses = psdzDetailedNcdInfoEto.DiagAdresses?.Select(DiagAddressCtoMapper.Map).ToList(),
                NcdStatus = ncdStatusEtoEnumMapper.GetValue(psdzDetailedNcdInfoEto.NcdStatus)
            };
        }
    }
}