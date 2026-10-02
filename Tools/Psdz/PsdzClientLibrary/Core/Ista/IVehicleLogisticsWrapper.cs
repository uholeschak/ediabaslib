using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Wrappers
{
    public interface IVehicleLogisticsWrapper
    {
        ICollection<ICombinedEcuHousingEntry> GetCombinedEcuHousingTable(IEcuTreeVehicle vehicle);
    }
}
