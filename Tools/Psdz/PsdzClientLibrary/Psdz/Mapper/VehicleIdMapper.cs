
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.DomainObjects;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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