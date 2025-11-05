using System.Linq;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
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