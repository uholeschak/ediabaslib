using System;
using BMW.Authoring;
using BMW.Authoring.API;
using BMW.Authoring.Vehicle.Interface;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Authoring.Vehicle.Implementation
{
    public class CentralErrorMemory : ICentralErrorMemory, IHideObjectMembers
    {
        public BMW.Authoring.Vehicle.Enums.CentralErrorMemoryStatus CentralErrorMemoryStatus { get; set; }

        public List<IZfsResult> ZfsResult { get; set; }

        public List<ICemResult> CemResult { get; set; }

        [PreserveSource(Cleaned = true)]
        public BMW.Authoring.Vehicle.Enums.CentralErrorMemoryStatus DoEcuReadCentralErrorMemoryForNewGenerationVehicles(IAuthoringModule istaModule, Vehicle authVehicle)
        {
            return BMW.Authoring.Vehicle.Enums.CentralErrorMemoryStatus.UNKNOWN;
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
