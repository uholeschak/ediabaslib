using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzProgrammingProtectionDataCto
    {
        IList<IPsdzEcuIdentifier> ProgrammingProtectionEcus { get; }

        IList<IPsdzSgbmId> SWEList { get; }

        byte[] SWEData { get; }
    }
}