using BMW.ISPI.TRIC.ISTA.EcuTree.Wrappers;
using PsdzClient.Core;
using System.Collections.Generic;

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
