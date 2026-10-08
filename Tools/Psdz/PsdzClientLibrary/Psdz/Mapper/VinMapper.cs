using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class VinMapper
    {
        public static IPsdzVin Map(VinModel vinModel)
        {
            if (vinModel == null)
            {
                return null;
            }

            return new PsdzVin
            {
                Value = vinModel.Value
            };
        }

        public static VinModel Map(IPsdzVin psdzVin)
        {
            if (psdzVin == null)
            {
                return null;
            }

            return new VinModel
            {
                Value = psdzVin.Value
            };
        }
    }
}