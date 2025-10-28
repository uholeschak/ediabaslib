using BMW.Rheingold.Psdz.Model.Obd;

namespace BMW.Rheingold.Psdz
{
    internal static class ObdTripleValueMapper
    {
        internal static IPsdzObdTripleValue Map(ObdTripleValueModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzObdTripleValue
            {
                CalId = model.CalId,
                ObdId = model.ObdId,
                SubCVN = model.SubCVN
            };
        }

        internal static ObdTripleValueModel Map(IPsdzObdTripleValue psdzObdTripleValue)
        {
            if (psdzObdTripleValue == null)
            {
                return null;
            }
            return new ObdTripleValueModel
            {
                CalId = psdzObdTripleValue.CalId,
                ObdId = psdzObdTripleValue.ObdId,
                SubCVN = psdzObdTripleValue.SubCVN
            };
        }
    }
}