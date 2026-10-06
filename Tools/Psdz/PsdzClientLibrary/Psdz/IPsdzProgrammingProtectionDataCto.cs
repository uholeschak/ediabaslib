using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzProgrammingProtectionDataCto
    {
        IList<IPsdzEcuIdentifier> ProgrammingProtectionEcus { get; }

        IList<IPsdzSgbmId> SWEList { get; }

        byte[] SWEData { get; }
    }
}
