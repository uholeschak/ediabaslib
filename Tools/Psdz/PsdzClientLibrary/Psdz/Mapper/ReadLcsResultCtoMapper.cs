using System.Linq;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mappers
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