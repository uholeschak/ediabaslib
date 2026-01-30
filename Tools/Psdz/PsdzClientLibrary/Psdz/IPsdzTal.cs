using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public interface IPsdzTal : IPsdzTalElement
    {
        IEnumerable<IPsdzEcuIdentifier> AffectedEcus { get; }

        string AsXml { get; }

        IEnumerable<IPsdzEcuIdentifier> InstalledEcuListIst { get; }

        IEnumerable<IPsdzEcuIdentifier> InstalledEcuListSoll { get; }

        PsdzTalExecutionState TalExecutionState { get; }

        IEnumerable<IPsdzTalLine> TalLines { get; }

        IPsdzExecutionTime PsdzExecutionTime { get; }
    }
}
