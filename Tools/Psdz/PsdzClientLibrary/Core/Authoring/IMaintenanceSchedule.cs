using BMW.Authoring;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.API.ServiceRide
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IMaintenanceSchedule : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult ApiResult { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime DueServiceDate { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime TodaysServiceDate { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int DueServiceMileage { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int TodaysServiceMileage { get; }
    }
}
