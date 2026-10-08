using RheingoldPsdzWebApi.Adapter.Contracts.Model.Obd;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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