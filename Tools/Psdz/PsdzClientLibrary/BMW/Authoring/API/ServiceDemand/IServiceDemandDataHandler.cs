using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

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
