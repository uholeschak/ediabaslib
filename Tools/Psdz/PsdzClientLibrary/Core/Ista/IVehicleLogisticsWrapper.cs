using PsdzClient.Core;
using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.EcuTree;
using BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Wrappers
{
    public interface IVehicleLogisticsWrapper
    {
        ICollection<ICombinedEcuHousingEntry> GetCombinedEcuHousingTable(IEcuTreeVehicle vehicle);
    }
}
