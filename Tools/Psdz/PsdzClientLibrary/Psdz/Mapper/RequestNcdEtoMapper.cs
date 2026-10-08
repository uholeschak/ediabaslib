using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class RequestNcdEtoMapper
    {
        internal static IPsdzRequestNcdEto Map(RequestNcdEtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzRequestNcdEto
            {
                Btld = SgbmIdMapper.Map(model.Btld),
                Cafd = SgbmIdMapper.Map(model.Cafd)
            };
        }

        internal static RequestNcdEtoModel Map(IPsdzRequestNcdEto psdzRequestNcdEto)
        {
            if (psdzRequestNcdEto == null)
            {
                return null;
            }

            return new RequestNcdEtoModel
            {
                Btld = SgbmIdMapper.Map(psdzRequestNcdEto.Btld),
                Cafd = SgbmIdMapper.Map(psdzRequestNcdEto.Cafd)
            };
        }
    }
}