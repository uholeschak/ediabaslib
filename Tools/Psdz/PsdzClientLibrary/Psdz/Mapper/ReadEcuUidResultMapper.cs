using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class ReadEcuUidResultMapper
    {
        public static IPsdzReadEcuUidResultCto Map(ReadEcuUidResultModel model)
        {
            if (model == null)
            {
                return null;
            }

            PsdzReadEcuUidResultCto psdzReadEcuUidResultCto = new PsdzReadEcuUidResultCto
            {
                EcuUids = model.EcuUids?.ToDictionary((KeyValuePairModel<EcuIdentifierModel, EcuUidCtoModel> a) => EcuIdentifierMapper.Map(a.Key), (KeyValuePairModel<EcuIdentifierModel, EcuUidCtoModel> b) => EcuUidCtoMapper.Map(b.Value)),
                FailureResponse = model.FailureResponse?.Select(EcuFailureResponseCtoMapper.MapCto)
            };
            if (psdzReadEcuUidResultCto.EcuUids == null)
            {
                psdzReadEcuUidResultCto.EcuUids = new Dictionary<IPsdzEcuIdentifier, IPsdzEcuUidCto>();
            }

            if (psdzReadEcuUidResultCto.FailureResponse == null)
            {
                psdzReadEcuUidResultCto.FailureResponse = new List<IPsdzEcuFailureResponseCto>();
            }

            return psdzReadEcuUidResultCto;
        }
    }
}