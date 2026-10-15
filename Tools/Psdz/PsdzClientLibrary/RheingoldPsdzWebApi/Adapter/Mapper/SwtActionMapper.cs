using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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