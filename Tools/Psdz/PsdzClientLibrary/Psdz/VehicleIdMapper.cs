using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
{
    internal static class VehicleIdMapper
    {
        public static VehicleId Map(VehicleIdModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new VehicleId
            {
                Id = model.Id,
                Url = model.Url
            };
        }
    }
}