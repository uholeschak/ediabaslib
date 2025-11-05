using System.Linq;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Psdz
{
    internal static class SwtActionMapper
    {
        internal static IPsdzSwtAction Map(SwtActionModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSwtAction
            {
                SwtEcus = model.SwtEcus?.Select(SwtEcuMapper.Map)
            };
        }

        internal static SwtActionModel Map(IPsdzSwtAction model)
        {
            if (model == null)
            {
                return null;
            }

            return new SwtActionModel
            {
                SwtEcus = model.SwtEcus?.Select(SwtEcuMapper.Map).ToList()
            };
        }
    }
}