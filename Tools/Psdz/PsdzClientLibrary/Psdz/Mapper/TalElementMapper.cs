using BMW.Rheingold.Psdz.Model.Tal;
using System;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal class TalElementMapper
    {
        private static TaExecutionStateMapper _taExecutionStateMapper = new TaExecutionStateMapper();
        internal static TTarget Map<TTarget>(TalElementModel model)
            where TTarget : PsdzTalElement, new()
        {
            if (model == null)
            {
                return null;
            }

            return new TTarget
            {
                Id = new Guid(model.Id),
                EndTime = model.EndTime,
                StartTime = model.StartTime,
                ExecutionState = _taExecutionStateMapper.GetValue(model.ExecutionStatus),
                HasFailureCauses = model.HasFailureCauses,
                FailureCauses = model.FailureCauses?.Select(FailureCauseMapper.Map)
            };
        }

        internal static TalElementModel Map(IPsdzTalElement psdzTalElement)
        {
            if (psdzTalElement == null)
            {
                return null;
            }

            return new TalElementModel
            {
                Id = psdzTalElement.Id.ToString(),
                EndTime = psdzTalElement.EndTime,
                StartTime = psdzTalElement.StartTime,
                ExecutionStatus = _taExecutionStateMapper.GetValue(psdzTalElement.ExecutionState),
                HasFailureCauses = psdzTalElement.HasFailureCauses,
                FailureCauses = psdzTalElement.FailureCauses?.Select(FailureCauseMapper.Map).ToList()
            };
        }
    }
}