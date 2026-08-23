using BMW.Authoring.API.ServiceDemand;
using BMW.Authoring.API.ServiceRide;
using BMW.Authoring.API.VPS;
using BMW.Authoring.API.VTG;
using PsdzClient.Core;
using System;
using System.ComponentModel;
using BMW.Authoring.API.CalibrationValues;
using BMW.Authoring.API.Interface.BatteryService;
using BMW.Authoring.API.Interface.HighVoltageBattery;
using BMW.Authoring.API.Interface.SeamLM2Demand;
using BMW.Authoring.API.OBFCM;
using BMW.Authoring.API.TVV;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IBackendCommunication : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IRotorOffsetValues BikeEEngineRotorOffsetValues_Get(string serialNumber);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IOBFCMDataHandler GetOBFCMDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IApiResult SendCustomerSimDataToBackend(string eid, string imei, string euicc);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IVtgDataHandler GetVtgDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ITvvDataHandler GetTvvDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IServiceDemandDataHandler GetServiceDemandDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IServiceRideDataHandler GetServiceRideDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IVPSDataHandler GetVPSDataHandler();

        [Obsolete("Please use GetSeamLM2BatteryDataHandler. This can be removed in ISTA Version 4.61")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBatteryHandler GetBatteryDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ISeamLM2BatteryDataHandler GetSeamLM2BatteryDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IHighVoltageBatteryDataHandler GetHighVoltageBatteryDataHandler();
    }
}
