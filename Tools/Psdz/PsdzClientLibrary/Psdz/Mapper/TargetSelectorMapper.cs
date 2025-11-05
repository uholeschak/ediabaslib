using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
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