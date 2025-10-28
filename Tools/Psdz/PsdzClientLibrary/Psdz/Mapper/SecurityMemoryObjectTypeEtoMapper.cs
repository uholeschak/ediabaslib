using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Certificate;

namespace BMW.Rheingold.Psdz
{
    internal class SecurityMemoryObjectTypeEtoMapper : MapperBase<PsdzCertMemoryObjectType, SecurityMemoryObjectTypeEto>
    {
        protected override IDictionary<PsdzCertMemoryObjectType, SecurityMemoryObjectTypeEto> CreateMap()
        {
            return new Dictionary<PsdzCertMemoryObjectType, SecurityMemoryObjectTypeEto>
            {
                {
                    PsdzCertMemoryObjectType.Certificate,
                    SecurityMemoryObjectTypeEto.CERTIFICATE
                },
                {
                    PsdzCertMemoryObjectType.Binding,
                    SecurityMemoryObjectTypeEto.BINDING
                },
                {
                    PsdzCertMemoryObjectType.OtherBinding,
                    SecurityMemoryObjectTypeEto.OTHER_BINDING
                },
                {
                    PsdzCertMemoryObjectType.OnlineCertificatesEcu,
                    SecurityMemoryObjectTypeEto.ONLINE_CERTIFICATES_ECU
                },
                {
                    PsdzCertMemoryObjectType.OnlineBindingsEcu,
                    SecurityMemoryObjectTypeEto.ONLINE_BINDINGS_ECU
                },
                {
                    PsdzCertMemoryObjectType.SecOcKeyList,
                    SecurityMemoryObjectTypeEto.KEYLIST
                }
            };
        }
    }
}