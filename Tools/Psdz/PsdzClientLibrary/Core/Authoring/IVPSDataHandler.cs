using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.API.VPS
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IVPSDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string VPSSendVehicleDataToBackend(string vehicleData);

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult VPSStartPollingToBackend(string vehicleSessionId);

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Always)]
        void VPSVehicleSessionStatusToBackend(string vehicleData, string vehicleFeedbackType, string sessionId);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult VPSPostSendVehicleDataToBackend(string vehicleData);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult VPSGetStartPollingToBackend(string vehicleSessionId);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult VPSPutVehicleSessionStatusToBackend(string vehicleData, string vehicleFeedbackType, string sessionId);
    }
}
