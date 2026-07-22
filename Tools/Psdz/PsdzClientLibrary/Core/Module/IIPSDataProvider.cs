using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
    public interface IIPSDataProvider
    {
        DateTime? DueServiceDate { get; }

        int DueServiceMileage { get; }

        DateTime? TodaysServiceDate { get; }

        int TodaysServiceMileage { get; }

        void ImportMaintenanceScheduleInfo(string vin17);
    }
}
