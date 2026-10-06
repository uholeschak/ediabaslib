using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;

namespace BMW.Rheingold.Programming.ProgrammingEngine
{
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public class ProgrammingSession
    {
        public IFFMDynamicResolver FFMResolver { get; set; }

        public IVehicle Vehicle { get; set; }
    }
}