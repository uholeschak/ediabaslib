using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public struct TalGenerationSettings
    {
        public IEnumerable<IPsdzDiagAddress> ECUsToSuppress;

        public IEnumerable<IPsdzDiagAddress> AllAllowedIntelligentSensors;

        public IPsdzFa FA;

        public byte[] VehicleVPC;

        public bool IsCheckProgrammingDeps;

        public bool IsFilterIntelligentSensors;

        public bool IsPreventIncosistendSwFlash;

        public bool IsUseMirrorProtocol;
    }
}
