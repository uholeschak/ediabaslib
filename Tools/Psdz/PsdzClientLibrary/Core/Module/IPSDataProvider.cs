using BMW.Rheingold.CoreFramework;
using System;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.ISTA.CoreFramework.Module
{
    [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
    public class IPSDataProvider : IIPSDataProvider
    {
        [PreserveSource(Hint = "IstaPukServiceClient", Placeholder = true)]
        private PlaceholderType pukClient;

        private DateTime? dueServiceDate;

        private DateTime? todaysServiceDate;

        private int dueServiceMileage;

        private int todaysServiceMileage;

        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        public DateTime? DueServiceDate => dueServiceDate;

        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        public int DueServiceMileage => dueServiceMileage;

        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        public DateTime? TodaysServiceDate => todaysServiceDate;

        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        public int TodaysServiceMileage => todaysServiceMileage;

        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        public IPSDataProvider()
        {
            //[-] pukClient = new IstaPukServiceClient();
        }

        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        public void ImportMaintenanceScheduleInfo(string vin17)
        {
            //[-] PukMaintenanceScheduleInfo pukMaintenanceScheduleInfo = pukClient.ImportMaintenanceScheduleInfo(vin17);
            //[-] if (pukMaintenanceScheduleInfo != null)
            //[-] {
            //[-] todaysServiceDate = pukMaintenanceScheduleInfo.TodaysServiceDate;
            //[-] todaysServiceMileage = pukMaintenanceScheduleInfo.TodaysServiceMileage;
            //[-] dueServiceDate = pukMaintenanceScheduleInfo.DueServiceDate;
            //[-] dueServiceMileage = pukMaintenanceScheduleInfo.DueServiceMileage;
            //[-] }
        }
    }
}
