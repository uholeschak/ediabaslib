using BMW.Authoring;
using BMW.Authoring.API;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.API.ServiceDemand
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IServiceDemandDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult SendServiceDemandDataToBackend(Dictionary<string, string> pathValuePair);

        [EditorBrowsable(EditorBrowsableState.Always)]
        void SendSpeedlinkDataToBackend();
    }
}
