using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class TargetSelectorMapper
    {
        public static IPsdzTargetSelector Map(TargetSelectorModel targetSelectorModel)
        {
            if (targetSelectorModel == null)
            {
                return null;
            }

            return new PsdzTargetSelector
            {
                VehicleInfo = targetSelectorModel.VehicleInfo,
                Project = targetSelectorModel.Project,
                Baureihenverbund = targetSelectorModel.Baureihenverbund,
                IsDirect = targetSelectorModel.IsDirect
            };
        }
    }
}