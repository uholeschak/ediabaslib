using BMW.Rheingold.Psdz.Model.Certificate;

namespace BMW.Rheingold.Psdz
{
    internal static class SecurityMemoryObjectEtoMapper
    {
        private static SecurityMemoryObjectSourceEtoMapper _securityMemoryObjectSourceEtoMapper = new SecurityMemoryObjectSourceEtoMapper();
        private static SecurityMemoryObjectTypeEtoMapper _securityMemoryObjectTypeEtoMapper = new SecurityMemoryObjectTypeEtoMapper();
        internal static PsdzCertMemoryObject Map(SecurityMemoryObjectEtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzCertMemoryObject
            {
                CertMemoryObjectSource = _securityMemoryObjectSourceEtoMapper.GetValue(model.Source),
                CertMemoryObjectType = _securityMemoryObjectTypeEtoMapper.GetValue(model.Type),
                Ecu = EcuIdentifierCtoMapper.Map(model.Ecu),
                SerializedCertificate = model.SerializedCertificate
            };
        }

        internal static SecurityMemoryObjectEtoModel Map(PsdzCertMemoryObject psdzCertMemoryObject)
        {
            if (psdzCertMemoryObject == null)
            {
                return null;
            }

            return new SecurityMemoryObjectEtoModel
            {
                Source = _securityMemoryObjectSourceEtoMapper.GetValue(psdzCertMemoryObject.CertMemoryObjectSource),
                Type = _securityMemoryObjectTypeEtoMapper.GetValue(psdzCertMemoryObject.CertMemoryObjectType),
                Ecu = EcuIdentifierCtoMapper.MapCto(psdzCertMemoryObject.Ecu),
                SerializedCertificate = psdzCertMemoryObject.SerializedCertificate
            };
        }
    }
}