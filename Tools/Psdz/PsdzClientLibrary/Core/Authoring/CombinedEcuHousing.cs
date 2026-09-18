using System;
using BMW.Authoring.API;
using BMW.Authoring.Programming.API.Interface;
using BMW.Authoring.Programming.API.Models.Interfaces;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using BMW.Authoring.Programming.API.Models.Implementation;
using BMW.ISPI.TRIC.ISTA.EcuTree.Wrappers;

namespace BMW.Authoring.Programming.API.Implementation
{
    public class CombinedEcuHousing : ICombinedEcuHousing
    {
        private readonly IAuthoringModule _module;

        private readonly IVehicleLogisticsWrapper _vehicleLogisticsWrapper;

        public CombinedEcuHousing(IAuthoringModule module)
        {
            _module = module;
            _vehicleLogisticsWrapper = new VehicleLogisticsWrapper();
        }

        public CombinedEcuHousing(IAuthoringModule module, IVehicleLogisticsWrapper vehicleLogisticsWrapper)
        {
            _module = module;
            _vehicleLogisticsWrapper = vehicleLogisticsWrapper;
        }

        public ICombinedEcuHousingEntryResponse[] GetAllCombinedEcuHousingAddresses()
        {
            ICollection<ICombinedEcuHousingEntry> combinedEcuHousingTable = _vehicleLogisticsWrapper.GetCombinedEcuHousingTable(_module.Vehicle);
            if (combinedEcuHousingTable == null || !combinedEcuHousingTable.Any())
            {
                return Array.Empty<ICombinedEcuHousingEntryResponse>();
            }
            return combinedEcuHousingTable.Select((ICombinedEcuHousingEntry entry) => new CombinedEcuHousingEntryResponse
            {
                RequiredEcuAddresses = (entry.RequiredEcuAddresses?.ToArray() ?? Array.Empty<int>())
            }).ToArray();
        }

        public ICombinedEcuHousingEntryResponse GetCombinedEcuHousingAddresses(int ecuAddressToSearch)
        {
            CombinedEcuHousingEntryResponse combinedEcuHousingEntryResponse = new CombinedEcuHousingEntryResponse();
            ICollection<ICombinedEcuHousingEntry> combinedEcuHousingTable = _vehicleLogisticsWrapper.GetCombinedEcuHousingTable(_module.Vehicle);
            if (combinedEcuHousingTable == null || !combinedEcuHousingTable.Any())
            {
                return combinedEcuHousingEntryResponse;
            }
            ICombinedEcuHousingEntry combinedEcuHousingEntry = combinedEcuHousingTable.FirstOrDefault((ICombinedEcuHousingEntry entry) => entry.RequiredEcuAddresses?.Contains(ecuAddressToSearch) ?? false);
            if (combinedEcuHousingEntry == null)
            {
                return combinedEcuHousingEntryResponse;
            }
            combinedEcuHousingEntryResponse.RequiredEcuAddresses = combinedEcuHousingEntry.RequiredEcuAddresses?.ToArray() ?? Array.Empty<int>();
            return combinedEcuHousingEntryResponse;
        }
    }
}
