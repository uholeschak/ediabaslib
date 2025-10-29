using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class SecurityBackendRequestIdEtoMapper
    {
        internal static IPsdzSecurityBackendRequestIdEto Map(SecurityBackendRequestIdEtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzSecurityBackendRequestIdEto
            {
                Value = model.Value
            };
        }

        internal static SecurityBackendRequestIdEtoModel Map(IPsdzSecurityBackendRequestIdEto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }
            return new SecurityBackendRequestIdEtoModel
            {
                Value = psdzObject.Value
            };
        }
    }
}