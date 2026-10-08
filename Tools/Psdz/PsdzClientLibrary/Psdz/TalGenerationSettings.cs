using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts
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
