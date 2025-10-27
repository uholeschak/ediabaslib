using BMW.Rheingold.Psdz.Model.Tal;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class TaCategoryMapper
    {
        private static TaExecutionStateMapper _taExecutionStateMapper = new TaExecutionStateMapper();

        public static TTarget Map<TTarget>(TaCategoryModel taCategory) where TTarget : PsdzTaCategory, new()
        {
            if (taCategory == null)
            {
                return null;
            }
            return new TTarget
            {
                IsEmpty = taCategory.IsEmpty,
                ExecutionState = _taExecutionStateMapper.GetValue(taCategory.ExecutionStatus),
                Tas = (taCategory.Tas?.Select(TaMapper.Map) ?? new List<IPsdzTa>())
            };
        }

        public static TTarget Map<TTarget>(IPsdzTaCategory psdzTaCategory) where TTarget : TaCategoryModel, new()
        {
            if (psdzTaCategory == null)
            {
                return null;
            }
            return new TTarget
            {
                IsEmpty = psdzTaCategory.IsEmpty,
                ExecutionStatus = _taExecutionStateMapper.GetValue(psdzTaCategory.ExecutionState),
                Tas = (psdzTaCategory.Tas?.Select(TaMapper.Map).ToList() ?? new List<TaModel>())
            };
        }
    }
}