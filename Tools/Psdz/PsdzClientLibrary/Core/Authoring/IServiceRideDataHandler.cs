using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API.ServiceRide
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IServiceRideDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IMaintenanceSchedule ImportMaintenanceScheduleInfo();
    }
}
