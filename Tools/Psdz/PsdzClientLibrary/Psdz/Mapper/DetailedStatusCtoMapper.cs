using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class DetailedStatusCtoMapper
    {
        private static readonly TokenDetailedStatusEtoMapper tokenDetailedStatusEtoMapper = new TokenDetailedStatusEtoMapper();

        public static DetailedStatusCtoModel Map(IPsdzDetailedStatusCto psdzDetailedStatusCto)
        {
            if (psdzDetailedStatusCto == null)
            {
                return null;
            }
            return new DetailedStatusCtoModel
            {
                TokenDetailedStatusEto = tokenDetailedStatusEtoMapper.GetValue(psdzDetailedStatusCto.TokenDetailedStatusEto),
                DiagAddressCtoModel = DiagAddressMapper.MapCto(psdzDetailedStatusCto.DiagAddressCto),
                FeatureIdCtoModel = FeatureIdCtoMapper.Map(psdzDetailedStatusCto.FeatureIdCto)
            };
        }

        public static IPsdzDetailedStatusCto Map(DetailedStatusCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzDetailedStatusCto
            {
                TokenDetailedStatusEto = tokenDetailedStatusEtoMapper.GetValue(model.TokenDetailedStatusEto),
                DiagAddressCto = DiagAddressMapper.Map(model.DiagAddressCtoModel),
                FeatureIdCto = FeatureIdCtoMapper.Map(model.FeatureIdCtoModel)
            };
        }
    }
}