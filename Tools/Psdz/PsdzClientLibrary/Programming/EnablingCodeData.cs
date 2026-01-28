using PsdzClient.Core;
using PsdzClient.Programming;
using System.Globalization;

namespace PsdzClient.Programming
{
    public class EnablingCodeData
    {
        public int DiagAddrAsInt { get; set; }

        public int ApplicationNumber { get; set; }

        public int SoftwareUpdateIndex { get; set; }

        public string Title { get; set; }

        public FscState FscState { get; set; }

        [PreserveSource(Cleaned = true)]
        public static string GetEnablingCodeName(int applicationId, int upgradeIndex, Vehicle vehicle, IFFMDynamicResolver dynamicResolver)
        {
            return string.Empty;
        }
    }
}