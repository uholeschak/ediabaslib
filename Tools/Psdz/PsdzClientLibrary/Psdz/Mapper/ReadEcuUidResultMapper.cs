using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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