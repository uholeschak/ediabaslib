using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class SwtApplicationIdMapper
    {
        public static IPsdzSwtApplicationId Map(SwtApplicationIdModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSwtApplicationId
            {
                ApplicationNumber = model.ApplicationNumber,
                UpgradeIndex = model.UpgradeIndex
            };
        }

        public static SwtApplicationIdModel Map(IPsdzSwtApplicationId psdzSwtApplciationId)
        {
            if (psdzSwtApplciationId == null)
            {
                return null;
            }

            return new SwtApplicationIdModel
            {
                ApplicationNumber = psdzSwtApplciationId.ApplicationNumber,
                UpgradeIndex = psdzSwtApplciationId.UpgradeIndex
            };
        }
    }
}