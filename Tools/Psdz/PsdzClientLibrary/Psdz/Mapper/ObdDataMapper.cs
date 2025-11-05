using BMW.Rheingold.Psdz.Model.Obd;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class ObdDataMapper
    {
        internal static IPsdzObdData Map(ObdDataModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzObdData
            {
                ObdTripleValues = model.ObdTripleValues?.Select(ObdTripleValueMapper.Map)
            };
        }

        internal static ObdDataModel Map(IPsdzObdData psdzObdData)
        {
            if (psdzObdData == null)
            {
                return null;
            }

            return new ObdDataModel
            {
                ObdTripleValues = psdzObdData.ObdTripleValues?.Select(ObdTripleValueMapper.Map).ToList()
            };
        }
    }
}