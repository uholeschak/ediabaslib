using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
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