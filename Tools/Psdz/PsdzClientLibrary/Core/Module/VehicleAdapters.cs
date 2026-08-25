using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PBMW.Rheingold.CoreFramework.Contracts;
using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    internal class VehicleAdapters : IVehicleAdapters
    {
        private Vehicle vehicle;

        public VehicleAdapters(Vehicle vehicle)
        {
            this.vehicle = vehicle;
        }

        public bool IsInstalled(IVehicleAdapterLocator adapter)
        {
            if (adapter != null && vehicle != null && vehicle.InstalledAdapters.Contains(adapter.SignedId))
            {
                return true;
            }
            return false;
        }

        internal void Install(IVehicleAdapterLocator adapter)
        {
            if (adapter != null && vehicle != null && vehicle != null && !vehicle.InstalledAdapters.Contains(adapter.SignedId))
            {
                Log.Info("VehicleAdapters.Install()", "install adapter: {0} {1}", adapter.Id, adapter.Title);
                vehicle.InstalledAdapters.Add(adapter.SignedId);
            }
        }

        internal void Uninstall(IVehicleAdapterLocator adapter)
        {
            if (adapter != null && vehicle.InstalledAdapters.Contains(adapter.SignedId))
            {
                Log.Info("VehicleAdapters.Uninstall()", "uninstall adapter: {0} {1}", adapter.Id, adapter.Title);
                vehicle.InstalledAdapters.Remove(adapter.SignedId);
            }
        }
    }
}
