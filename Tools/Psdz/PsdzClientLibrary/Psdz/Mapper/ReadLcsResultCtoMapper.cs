using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ReadLcsResultCtoMapper
    {
        public static IPsdzReadLcsResultCto Map(ReadLcsResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzReadLcsResultCto
            {
                EcuLcsValues = model.EcuLcsValues?.Select(EcuLcsValueCtoMapper.Map),
                Failures = model.Failures?.Select(EcuFailureResponseCtoMapper.MapCto)
            };
        }
    }
}