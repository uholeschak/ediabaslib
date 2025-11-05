using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuFailureResponseCtoMapper
    {
        public static IPsdzEcuFailureResponseCto MapCto(EcuFailureResponseCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzEcuFailureResponseCto
            {
                EcuIdentifierCto = EcuIdentifierMapper.Map(model.EcuIdentifierCto),
                Cause = LocalizableMessageToMapper.Map(model.Cause)
            };
        }

        public static PsdzEcuFailureResponse Map(EcuFailureResponseCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzEcuFailureResponse
            {
                Ecu = EcuIdentifierMapper.Map(model.EcuIdentifierCto),
                Reason = model.Cause.Description
            };
        }

        public static EcuFailureResponseCtoModel MapCto(IPsdzEcuFailureResponseCto psdzEcuFailureResponseCto)
        {
            if (psdzEcuFailureResponseCto == null)
            {
                return null;
            }

            return new EcuFailureResponseCtoModel
            {
                EcuIdentifierCto = EcuIdentifierMapper.Map(psdzEcuFailureResponseCto.EcuIdentifierCto),
                Cause = LocalizableMessageToMapper.Map(psdzEcuFailureResponseCto.Cause)
            };
        }

        public static EcuFailureResponseCtoModel Map(PsdzEcuFailureResponse psdzEcuFailureResponse)
        {
            if (psdzEcuFailureResponse == null)
            {
                return null;
            }

            return new EcuFailureResponseCtoModel
            {
                EcuIdentifierCto = EcuIdentifierMapper.Map(psdzEcuFailureResponse.Ecu),
                Cause = LocalizableMessageToMapper.Map(psdzEcuFailureResponse.Reason)
            };
        }
    }
}