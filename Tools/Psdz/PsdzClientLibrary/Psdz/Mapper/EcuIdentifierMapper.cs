using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuIdentifierMapper
    {
        public static IPsdzEcuIdentifier Map(EcuIdentifierModel ecuIdentifierModel)
        {
            if (ecuIdentifierModel == null)
            {
                return null;
            }
            return new PsdzEcuIdentifier
            {
                BaseVariant = ecuIdentifierModel.BaseVariant,
                DiagnosisAddress = DiagAddressMapper.Map(ecuIdentifierModel.DiagnosisAddress)
            };
        }

        public static EcuIdentifierModel Map(IPsdzEcuIdentifier ecuIdentifier)
        {
            if (ecuIdentifier == null)
            {
                return null;
            }
            return new EcuIdentifierModel
            {
                BaseVariant = ecuIdentifier.BaseVariant,
                DiagnosisAddress = DiagAddressMapper.Map(ecuIdentifier.DiagnosisAddress)
            };
        }
    }
}