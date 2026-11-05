using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Authoring;
using BMW.Authoring.API;
using BMW.Authoring.Programming.API.Interface;
using BMW.Authoring.Programming.API.Models.Implementation;
using BMW.Authoring.Programming.API.Models.Interfaces;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.RheingoldSessionController;

namespace BMW.Authoring.Programming.API.Implementation
{
    public class ProgrammingService : BMW.Authoring.Programming.API.Interface.IProgrammingService, IHideObjectMembers
    {
        private readonly IProgrammingSession _programmingSession;

        public ProgrammingService(IAuthoringModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }
            _programmingSession = ((Logic)module.IstaOperationLogic)?.ProgrammingSession;
        }

        public BMW.Authoring.Programming.API.Models.Interfaces.IIstufenTriple RequestILevel()
        {
            BMW.Rheingold.CoreFramework.Contracts.Programming.IIstufenTriple istufenTriple = _programmingSession?.ProgrammingApi?.RequestILevel();
            if (istufenTriple == null)
            {
                return null;
            }
            return new IstufenTriple(istufenTriple);
        }

        public BMW.Authoring.Programming.API.Models.Interfaces.IIstufenTriple RequestILevelFromBackup()
        {
            BMW.Rheingold.CoreFramework.Contracts.Programming.IIstufenTriple istufenTriple = _programmingSession?.ProgrammingApi?.RequestILevelFromBackup();
            if (istufenTriple == null)
            {
                return null;
            }
            return new IstufenTriple(istufenTriple);
        }

        public BMW.Authoring.Programming.API.Models.Interfaces.IPlannedSwiAction CreatePlannedSwiAction(string swiActionName, bool isDisabled)
        {
            return new PlannedSwiAction(swiActionName, isDisabled);
        }

        public bool PlanVehicleModifications(IList<BMW.Authoring.Programming.API.Models.Interfaces.IPlannedSwiAction> plannedSwiActions)
        {
            if (_programmingSession == null || plannedSwiActions == null)
            {
                return false;
            }
            List<BMW.Rheingold.CoreFramework.Contracts.Vehicle.IPlannedSwiAction> plannedSwiActions2 = plannedSwiActions.Select((BMW.Authoring.Programming.API.Models.Interfaces.IPlannedSwiAction action) => RheingoldProgammingObjectBuilder.CreatePlannedSwiAction(action.SwiActionName, action.IsDisabled)).ToList();
            return _programmingSession.PlanVehicleModifications(plannedSwiActions2)?.Result ?? false;
        }

        public IReadOnlyList<BMW.Authoring.Programming.API.Models.Interfaces.ISgbmId> GetSgbmIds(string baseVariant, int ecuAddress)
        {
            if (_programmingSession?.ProgrammingApi == null)
            {
                return Array.Empty<BMW.Authoring.Programming.API.Models.Interfaces.ISgbmId>();
            }
            IEcuIdentifier ecu = _programmingSession.ProgrammingApi.ObjectBuilder.BuildEcuIdentifier(baseVariant, ecuAddress);
            IStandardSvk currentSvk = _programmingSession.ProgrammingApi.GetCurrentSvk(ecu);
            if (currentSvk?.SgbmIds == null)
            {
                return Array.Empty<BMW.Authoring.Programming.API.Models.Interfaces.ISgbmId>();
            }
            return currentSvk.SgbmIds.Select((BMW.Rheingold.CoreFramework.Contracts.Programming.ISgbmId sgbm) => new SgbmId(sgbm)).ToArray();
        }

        public IReadOnlyList<BMW.Authoring.Programming.API.Models.Interfaces.ISgbmIdChange> GetDifferentSgbmIds(string ecu)
        {
            if (_programmingSession == null)
            {
                return null;
            }
            return _programmingSession.GetDifferentSgbmIds(ecu)?.Select((BMW.Rheingold.CoreFramework.Contracts.Programming.ISgbmIdChange change) => new SgbmIdChange(change)).ToArray();
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
