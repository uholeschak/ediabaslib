using BMW.Authoring.API.ServiceDemand;
using BMW.Authoring.API.ServiceRide;
using BMW.Authoring.API.VPS;
using BMW.Authoring.API.VTG;
using PsdzClient;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public interface IBackendCommunication : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IApiResult SendCustomerSimDataToBackend(string eid, string imei, string euicc);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IVtgDataHandler GetVtgDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IServiceDemandDataHandler GetServiceDemandDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IServiceRideDataHandler GetServiceRideDataHandler();

        [EditorBrowsable(EditorBrowsableState.Advanced)] 
        IVPSDataHandler GetVPSDataHandler();
    }
}
