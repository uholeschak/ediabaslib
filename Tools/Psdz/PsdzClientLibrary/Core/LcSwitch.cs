using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class LcSwitch : ILcSwitch
    {
        public string Number { get; set; }

        public string NumberText { get; set; }

        public string Value { get; set; }

        public string ValueText { get; set; }
    }
}
