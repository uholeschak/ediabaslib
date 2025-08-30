using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    // [UH] dummy class only
    public class ProgrammingSession
    {
        public IFFMDynamicResolver FFMResolver { get; set; }

        public IVehicle Vehicle { get; set; }
    }
}