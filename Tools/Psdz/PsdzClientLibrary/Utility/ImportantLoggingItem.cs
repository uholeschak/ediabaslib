using System.Collections.Generic;

namespace PsdzClient.Utility
{
    public enum TYPES
    {
        FASTA_READOUT,
        IDENTIFICATION,
        FAULT_CODE_MODEL,
        VEHICLE_TEST,
        INTERNET_CONNECTION,
        ISTA_VERSION,
        VIN,
        NVI,
        EXPERT_MODE,
        PROGRAMMING,
        TOTAL_IDENT_TIME,
        VEHICLE_TEST_SEQUENCES,
        FASTA_JOBS_AMOUNT,
        ECUS,
        DTC_ENTRIES,
        BACKEND_CALLS,
        NONE,
        Sec4Diag
    }

    public class ImportantLoggingItem
    {
        private static Dictionary<string, string> dicMessagesToLog = new Dictionary<string, string>();

        private static Dictionary<string, int> doubleIdentifiers = new Dictionary<string, int>();

        public static void AddItemToList(string item, TYPES type)
        {
        }

        public static void AddMessagesToLog(string identify, string message)
        {
            if (!dicMessagesToLog.ContainsKey(identify))
            {
                dicMessagesToLog.Add(identify, message);
            }
            else
            {
                AddMessagesToLog(ExtendTheDoubleIdentifier(identify), message);
            }
        }

        private static string ExtendTheDoubleIdentifier(string identifier)
        {
            if (!doubleIdentifiers.ContainsKey(identifier))
            {
                doubleIdentifiers.Add(identifier, 0);
            }
            return $"{identifier}_{doubleIdentifiers[identifier]++}";
        }
    }
}
