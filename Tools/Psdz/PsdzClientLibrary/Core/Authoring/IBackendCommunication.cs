using PsdzClient;
using PsdzClient.Core;
using System.ComponentModel;
using BMW.Authoring.API.ServiceRide;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public interface IBackendCommunication : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IServiceRideDataHandler GetServiceRideDataHandler();
    }
}
