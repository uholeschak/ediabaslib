using BMW.Rheingold.Psdz.Model.Tal;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class TalMapper
    {
        private static TalExecutionStateMapper _talExecutionStateMapper = new TalExecutionStateMapper();

        public static IPsdzTal Map(TalModel talModel)
        {
            if (talModel == null)
            {
                return null;
            }
            PsdzTal psdzTal = TalElementMapper.Map<PsdzTal>(talModel.TalElement);
            psdzTal.AsXml = talModel.AsXml;
            psdzTal.AffectedEcus = talModel.AffectedEcus?.Select(EcuIdentifierMapper.Map);
            psdzTal.InstalledEcuListIst = talModel.InstalledEcuListCurrent?.Select(EcuIdentifierMapper.Map);
            psdzTal.InstalledEcuListSoll = talModel.InstalledEcuListTarget?.Select(EcuIdentifierMapper.Map);
            psdzTal.TalExecutionState = _talExecutionStateMapper.GetValue(talModel.TalExecutionState);
            psdzTal.PsdzExecutionTime = ExecutionTimeTypeMapper.Map(talModel.ExecutionTimeType);
            psdzTal.TalLines = talModel.TalLines.Select(TalLineMapper.Map);
            return psdzTal;
        }

        public static TalModel Map(IPsdzTal psdzTal)
        {
            if (psdzTal == null)
            {
                return null;
            }
            TalElementModel talElement = TalElementMapper.Map(psdzTal);
            return new TalModel
            {
                TalElement = talElement,
                AsXml = psdzTal.AsXml,
                AffectedEcus = psdzTal.AffectedEcus?.Select(EcuIdentifierMapper.Map).ToList(),
                InstalledEcuListCurrent = psdzTal.InstalledEcuListIst?.Select(EcuIdentifierMapper.Map).ToList(),
                InstalledEcuListTarget = psdzTal.InstalledEcuListSoll?.Select(EcuIdentifierMapper.Map).ToList(),
                TalExecutionState = _talExecutionStateMapper.GetValue(psdzTal.TalExecutionState),
                ExecutionTimeType = ExecutionTimeTypeMapper.Map(psdzTal.PsdzExecutionTime),
                TalLines = psdzTal.TalLines?.Select(TalLineMapper.Map).ToList()
            };
        }
    }
}