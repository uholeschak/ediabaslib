using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Tal;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ExecutionTimeTypeMapper
    {
        public static IPsdzExecutionTime Map(ExecutionTimeTypeModel model)
        {
            return new PsdzExecutionTime
            {
                PlannedStartTime = model.PlannedStartTime,
                PlannedEndTime = model.PlannedEndTime,
                ActualStartTime = model.ActualStartTime,
                ActualEndTime = model.ActualEndTime
            };
        }

        public static ExecutionTimeTypeModel Map(IPsdzExecutionTime psdzTime)
        {
            return new ExecutionTimeTypeModel
            {
                PlannedStartTime = psdzTime.PlannedStartTime,
                PlannedEndTime = psdzTime.PlannedEndTime,
                ActualStartTime = psdzTime.ActualStartTime,
                ActualEndTime = psdzTime.ActualEndTime
            };
        }
    }
}