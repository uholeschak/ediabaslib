using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal class ValidationStatusEtoMapper : MapperBase<PsdzValidationStatusEtoEnum, ValidationStatusEto>
    {
        protected override IDictionary<PsdzValidationStatusEtoEnum, ValidationStatusEto> CreateMap()
        {
            return new Dictionary<PsdzValidationStatusEtoEnum, ValidationStatusEto>
            {
                {
                    PsdzValidationStatusEtoEnum.E_CHECK_RUNNING,
                    ValidationStatusEto.E_CHECK_RUNNING
                },
                {
                    PsdzValidationStatusEtoEnum.E_EMPTY,
                    ValidationStatusEto.E_EMPTY
                },
                {
                    PsdzValidationStatusEtoEnum.E_ERROR,
                    ValidationStatusEto.E_ERROR
                },
                {
                    PsdzValidationStatusEtoEnum.E_FEATUREID,
                    ValidationStatusEto.E_FEATUREID
                },
                {
                    PsdzValidationStatusEtoEnum.E_MALFORMED,
                    ValidationStatusEto.E_MALFORMED
                },
                {
                    PsdzValidationStatusEtoEnum.E_OK,
                    ValidationStatusEto.E_OK
                },
                {
                    PsdzValidationStatusEtoEnum.E_OTHER,
                    ValidationStatusEto.E_OTHER
                },
                {
                    PsdzValidationStatusEtoEnum.E_SECURITY_ERROR,
                    ValidationStatusEto.E_SECURITY_ERROR
                },
                {
                    PsdzValidationStatusEtoEnum.E_TIMESTAMP,
                    ValidationStatusEto.E_TIMESTAMP
                },
                {
                    PsdzValidationStatusEtoEnum.E_UNCHECKED,
                    ValidationStatusEto.E_UNCHECKED
                },
                {
                    PsdzValidationStatusEtoEnum.E_VERSION,
                    ValidationStatusEto.E_VERSION
                },
                {
                    PsdzValidationStatusEtoEnum.E_WRONG_LINKTOID,
                    ValidationStatusEto.E_WRONG_LINKTOID
                }
            };
        }
    }
}