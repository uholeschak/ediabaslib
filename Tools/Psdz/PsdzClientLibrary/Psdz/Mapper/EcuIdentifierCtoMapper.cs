using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuIdentifierCtoMapper
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

        public static EcuIdentifierCtoModel MapCto(IPsdzEcuIdentifier psdzEcuIdentifier)
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