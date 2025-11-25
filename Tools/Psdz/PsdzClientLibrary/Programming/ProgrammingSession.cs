using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "Dummy class")]
    public class ProgrammingSession
    {
        public IFFMDynamicResolver FFMResolver { get; set; }

        public IVehicle Vehicle { get; set; }
    }
}