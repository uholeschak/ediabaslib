namespace BMW.Rheingold.Psdz.Model.Tal
{
    public interface IPsdzExecutionTime
    {
        long ActualEndTime { get; }

        long ActualStartTime { get; }

        long PlannedEndTime { get; }

        long PlannedStartTime { get; }
    }
}