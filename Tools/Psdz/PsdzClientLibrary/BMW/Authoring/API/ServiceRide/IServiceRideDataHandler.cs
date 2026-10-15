using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

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
