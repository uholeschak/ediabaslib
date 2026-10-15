using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.FeatureStatusTo;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class FeatureStatusToMapper
    {
        private static FeatureStatusEtoEnumMapper _featureStatusEtoEnumMapper = new FeatureStatusEtoEnumMapper();
        private static ValidationStatusEtoMapper _validationStatusEtoMapper = new ValidationStatusEtoMapper();
        public static IPsdzFeatureStatusTo Map(FeatureStatusToModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzFeatureStatusTo
            {
                DiagAddress = DiagAddressCtoMapper.Map(model.DiagAddress),
                FeatureId = FeatureIdCtoMapper.Map(model.FatureId),
                FeatureStatus = _featureStatusEtoEnumMapper.GetValue(model.FeatureStatus),
                ValidationStatus = _validationStatusEtoMapper.GetValue(model.ValidationStatus)
            };
        }
    }
}