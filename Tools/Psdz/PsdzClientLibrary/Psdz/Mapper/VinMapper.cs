using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
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