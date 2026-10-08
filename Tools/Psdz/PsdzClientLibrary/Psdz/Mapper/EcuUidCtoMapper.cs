using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class EcuUidCtoMapper
    {
        public static IPsdzEcuUidCto Map(EcuUidCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzEcuUidCto
            {
                EcuUid = model.EcuUid
            };
        }

        public static EcuUidCtoModel Map(IPsdzEcuUidCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new EcuUidCtoModel
            {
                EcuUid = psdzObject.EcuUid
            };
        }
    }
}