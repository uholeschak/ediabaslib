using BMW.Rheingold.Psdz.Model.SecureCoding;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class CheckNcdResultEtoMapper
    {
        public static IPsdzCheckNcdResultEto Map(CheckNcdResultEtoModel model)
        {
            if (model != null)
            {
                return new PsdzCheckNcdResultEto
                {
                    DetailedNcdStatus = model.DetailedNcdStatus?.Select(DetailedNcdInfoEtoMapper.Map).ToList(),
                    isEachNcdSigned = model.EachNcdSigned
                };
            }
            return null;
        }

        public static CheckNcdResultEtoModel Map(IPsdzCheckNcdResultEto psdzCheckNcdResultEto)
        {
            if (psdzCheckNcdResultEto == null)
            {
                return null;
            }
            return new CheckNcdResultEtoModel
            {
                DetailedNcdStatus = psdzCheckNcdResultEto.DetailedNcdStatus?.Select(DetailedNcdInfoEtoMapper.Map).ToList(),
                EachNcdSigned = psdzCheckNcdResultEto.isEachNcdSigned
            };
        }
    }
}