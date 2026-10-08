using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.EcuTree;
using BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet;
using BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Wrappers
{
    public class VehicleLogisticsWrapper : IVehicleLogisticsWrapper
    {
        public ICollection<ICombinedEcuHousingEntry> GetCombinedEcuHousingTable(IEcuTreeVehicle vehicle)
        {
            return VehicleLogistics.GetCombinedEcuHousingTable(vehicle);
        }
    }
}
