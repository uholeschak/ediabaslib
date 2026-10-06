using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    public static class EcuIdentifierCtoMapper
    {
        public static IPsdzEcuIdentifier Map(EcuIdentifierCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzEcuIdentifier
            {
                BaseVariant = model.BaseVariant,
                DiagnosisAddress = DiagAddressMapper.Map(model.DiagAddress)
            };
        }

        public static EcuIdentifierCtoModel Map(IPsdzEcuIdentifier psdzEcuIdentifier)
        {
            if (psdzEcuIdentifier == null)
            {
                return null;
            }

            return new EcuIdentifierCtoModel
            {
                BaseVariant = psdzEcuIdentifier.BaseVariant,
                DiagAddress = DiagAddressMapper.MapCto(psdzEcuIdentifier.DiagnosisAddress)
            };
        }
    }
}