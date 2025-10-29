using System.Linq;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class ReadStatusResultCtoMapper
    {
        public static IPsdzReadStatusResultCto Map(ReadStatusResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzReadStatusResultCto
            {
                Failures = model.Failures?.Select(EcuFailureResponseCtoMapper.MapCto).ToList(),
                FeatureStatusSet = model.FeatureStatusSet?.Select(FeatureLongStatusCtoMapper.Map).ToList()
            };
        }

        public static ReadStatusResultCtoModel Map(IPsdzReadStatusResultCto psdzReadStatusResultCto)
        {
            if (psdzReadStatusResultCto == null)
            {
                return null;
            }
            return new ReadStatusResultCtoModel
            {
                Failures = psdzReadStatusResultCto.Failures?.Select(EcuFailureResponseCtoMapper.MapCto).ToList(),
                FeatureStatusSet = psdzReadStatusResultCto.FeatureStatusSet?.Select(FeatureLongStatusCtoMapper.Map).ToList()
            };
        }
    }
}