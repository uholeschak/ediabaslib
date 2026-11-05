using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

#pragma warning disable CS0618
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class PlannedSwiAction : IPlannedSwiAction
    {
        public string SwiActionName { get; set; }

        public bool IsDisabled { get; set; }

        public PlannedSwiAction(string name, bool isDisabled)
        {
            SwiActionName = name;
            IsDisabled = isDisabled;
        }

        public override string ToString()
        {
            return "SwiActionName: " + SwiActionName + ", IsDisabled: " + (IsDisabled ? "true" : "false");
        }
    }
}
