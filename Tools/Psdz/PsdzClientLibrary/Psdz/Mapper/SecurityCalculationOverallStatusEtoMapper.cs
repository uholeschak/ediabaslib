using BMW.Rheingold.Psdz.Model.Certificate;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    internal class SecurityCalculationOverallStatusEtoMapper : MapperBase<PsdzSecurityCalculationOverallStatus, SecurityCalculationOverallStatusEto>
    {
        protected override IDictionary<PsdzSecurityCalculationOverallStatus, SecurityCalculationOverallStatusEto> CreateMap()
        {
            return new Dictionary<PsdzSecurityCalculationOverallStatus, SecurityCalculationOverallStatusEto>
            {
                {
                    PsdzSecurityCalculationOverallStatus.Conflict,
                    SecurityCalculationOverallStatusEto.CONFLICT
                },
                {
                    PsdzSecurityCalculationOverallStatus.Detailed,
                    SecurityCalculationOverallStatusEto.DETAILED
                },
                {
                    PsdzSecurityCalculationOverallStatus.Empty,
                    SecurityCalculationOverallStatusEto.EMPTY
                },
                {
                    PsdzSecurityCalculationOverallStatus.Error,
                    SecurityCalculationOverallStatusEto.ERROR
                },
                {
                    PsdzSecurityCalculationOverallStatus.Malformed,
                    SecurityCalculationOverallStatusEto.MALFORMED
                },
                {
                    PsdzSecurityCalculationOverallStatus.Ok,
                    SecurityCalculationOverallStatusEto.OK
                },
                {
                    PsdzSecurityCalculationOverallStatus.UnknownVersion,
                    SecurityCalculationOverallStatusEto.UNKNOWN_VERSION
                },
                {
                    PsdzSecurityCalculationOverallStatus.VinMalformed,
                    SecurityCalculationOverallStatusEto.VIN_MALFORMED
                },
                {
                    PsdzSecurityCalculationOverallStatus.WrongFormat,
                    SecurityCalculationOverallStatusEto.WRONG_FORMAT
                },
                {
                    PsdzSecurityCalculationOverallStatus.InvalidFatRequest,
                    SecurityCalculationOverallStatusEto.INVALID_FAT_REQUEST
                },
                {
                    PsdzSecurityCalculationOverallStatus.RequestNotOnCertStore,
                    SecurityCalculationOverallStatusEto.REQUEST_NOT_ON_CERT_STORE
                },
                {
                    PsdzSecurityCalculationOverallStatus.InvalidEcuTypeCerts,
                    SecurityCalculationOverallStatusEto.INVALID_ECU_TYPE_CERTS
                },
                {
                    PsdzSecurityCalculationOverallStatus.InvalidSignature,
                    SecurityCalculationOverallStatusEto.INVALID_SIGNATURE
                },
                {
                    PsdzSecurityCalculationOverallStatus.OTHER_ERROR,
                    SecurityCalculationOverallStatusEto.OTHER_ERROR
                }
            };
        }
    }
}