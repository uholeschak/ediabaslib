using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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