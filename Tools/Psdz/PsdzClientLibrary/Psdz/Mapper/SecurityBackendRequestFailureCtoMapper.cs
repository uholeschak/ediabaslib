using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class SecurityBackendRequestFailureCtoMapper
    {
        public static SecurityBackendRequestFailureCtoModel Map(IPsdzSecurityBackendRequestFailureCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }
            return new SecurityBackendRequestFailureCtoModel
            {
                Cause = LocalizableMessageToMapper.Map(psdzObject.Cause),
                Retry = psdzObject.Retry,
                Url = psdzObject.Url
            };
        }

        public static IPsdzSecurityBackendRequestFailureCto Map(SecurityBackendRequestFailureCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzSecurityBackendRequestFailureCto
            {
                Cause = LocalizableMessageToMapper.Map(model.Cause),
                Retry = model.Retry,
                Url = model.Url
            };
        }
    }
}