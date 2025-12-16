using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PsdzClient.Core;

namespace PsdzClient.Utility
{
    public class ImportantLoggingItem
    {
        private static HashSet<(string Name, bool IsSuccessful)> listOfEcus = new HashSet<(string, bool)>();
        private static List<string> importantItems = new List<string>();
        private static List<string> dtcEntries = new List<string>();
        private static List<string> miscItems = new List<string>();
        private static DateTime startOfIdent = DateTime.Now;
        private static DateTime endOfFasta = DateTime.Now;
        private static List<string> backendCalls = new List<string>();
        private static List<string> vehicleTests = new List<string>();
        private static List<string> metrics = new List<string>();
        private static Dictionary<string, string> dicMessagesToLog = new Dictionary<string, string>();
        private static Dictionary<string, int> doubleIdentifiers = new Dictionary<string, int>();
        private static List<string> Sec4DiagCode = new List<string>();
        private static List<string> Sec4CNCode = new List<string>();
        private static Dictionary<TYPES, string> typeDescriptions = new Dictionary<TYPES, string>
        {
            {
                TYPES.FASTA_READOUT,
                "FASTA Readout"
            },
            {
                TYPES.VEHICLE_TEST,
                "Vehicle Test"
            },
            {
                TYPES.IDENTIFICATION,
                "Identification"
            },
            {
                TYPES.INTERNET_CONNECTION,
                "Device online status"
            },
            {
                TYPES.FAULT_CODE_MODEL,
                "New fault code model active"
            },
            {
                TYPES.ISTA_VERSION,
                "ISTA Version"
            },
            {
                TYPES.VIN,
                "Vehicle identification Number (VIN)"
            },
            {
                TYPES.NVI,
                "Status; New Vehicle Ident active"
            },
            {
                TYPES.EXPERT_MODE,
                "Expert mode enabled"
            },
            {
                TYPES.PROGRAMMING,
                "Programming"
            },
            {
                TYPES.TOTAL_IDENT_TIME,
                "Total identification time"
            },
            {
                TYPES.VEHICLE_TEST_SEQUENCES,
                "Vehicle Test Sequences"
            },
            {
                TYPES.FASTA_JOBS_AMOUNT,
                "Amount of FASTA jobs executed"
            },
            {
                TYPES.ECUS,
                string.Empty
            },
            {
                TYPES.DTC_ENTRIES,
                "DTC Entries"
            },
            {
                TYPES.BACKEND_CALLS,
                string.Empty
            },
            {
                TYPES.NONE,
                "Information"
            },
            {
                TYPES.Sec4Diag,
                "Sec4Diag"
            }
        };
        public static TimeSpan FullIdentDuration => endOfFasta - startOfIdent;
        public static TimeSpan DurationVehicleIdent { get; set; }
        public static TimeSpan DurationAblges { get; set; }
        public static TimeSpan DurationFSLesen { get; set; }
        public static TimeSpan DurationfastaReadout { get; set; }
        public static TimeSpan DurationFullTest { get; set; }

        public static void AddItemToList(string item, TYPES type)
        {
            string typeDescription = GetTypeDescription(type);
            switch (type)
            {
                case TYPES.BACKEND_CALLS:
                    backendCalls.Add(item);
                    break;
                case TYPES.VEHICLE_TEST_SEQUENCES:
                    vehicleTests.Add(typeDescription + ": " + item);
                    break;
                case TYPES.DTC_ENTRIES:
                    dtcEntries.Add(typeDescription + ": " + item);
                    break;
                case TYPES.Sec4Diag:
                    Sec4DiagCode.Add(typeDescription + ": " + item);
                    break;
                case TYPES.Sec4CN:
                    Sec4CNCode.Add(typeDescription + ": " + item);
                    break;
                default:
                    importantItems.Add(typeDescription + ": " + item);
                    break;
            }
        }

        public static void AddItemToLog(string name, bool isSuccessful)
        {
            listOfEcus.Add((name, isSuccessful));
        }

        public static void AddMetric(string entry)
        {
            metrics.Add(entry);
        }

        public static void SetStartAndEndOfVehicleProcess(bool start)
        {
            if (start)
            {
                startOfIdent = DateTime.Now;
            }
            else
            {
                endOfFasta = DateTime.Now;
            }
        }

        public static void LogVehicleSpecificInformation(Vehicle vehicle)
        {
            AddMiscItem("ISTA Version: " + Assembly.GetEntryAssembly()?.GetName().Version.ToString());
            AddMiscItem("VIN: " + vehicle.VIN17);
            AddMiscItem("Online status: " + (IsNetworkAvailable() ? "online/true" : "offline/false"));
            AddMiscItem("New VehicleIdent " + (vehicle.IsNewIdentActive ? "active." : "inactive."));
            AddMiscItem("New FaultCode model " + (vehicle.Classification.IsNewFaultMemoryActive ? "active" : "inactive"));
            TimeSpan timeSpan = endOfFasta - startOfIdent;
            AddItemToList($"Ident started at: {startOfIdent}, Fasta readout finished at: {endOfFasta}. Total time elapsed: {timeSpan}", TYPES.TOTAL_IDENT_TIME);
        }

        public static void WriteLog()
        {
            LogItems();
            ClearLists();
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

        public static bool IsNetworkAvailable()
        {
            return WebCallUtility.CheckForInternetConnection();
        }

        private static string GetTypeDescription(TYPES type)
        {
            if (!typeDescriptions.TryGetValue(type, out var value))
            {
                return string.Empty;
            }

            return value;
        }

        private static void AddMiscItem(string item)
        {
            miscItems.Add(item);
        }

        private static void LogItems()
        {
            string method = Log.CurrentMethod();
            Log.Info(method, "*************** *************** Important Informations from ISTA Application *************** ***************");
            Log.Info(method, " ");
            LogInfoList("Misc Information", miscItems, method);
            LogInfoList("Process Informations", importantItems, method);
            LogInfoList("DTCs", dtcEntries, method);
            LogInfoList("Backend calls list", backendCalls, method);
            LogEcusList(listOfEcus, method);
            LogInfoList("Vehicle Test Sequences", vehicleTests, method);
            LogInfoList("Metrics", metrics, method);
            LogMessagesDic(method);
            LogInfoList("Sec4Diag", Sec4DiagCode, method);
            LogInfoList("Sec4CN", Sec4CNCode, method);
            Log.Info(method, "*************** ***************");
        }

        private static void LogInfoList(string title, List<string> items, string method)
        {
            if (!items.Any())
            {
                return;
            }

            Log.Info(method, "*************** *************** " + title + " *************** ***************");
            Log.Info(method, " ");
            foreach (string item in items)
            {
                Log.Info(method, item);
            }

            Log.Info(method, " ");
        }

        private static void LogMessagesDic(string method)
        {
            if (!dicMessagesToLog.Any())
            {
                return;
            }

            Log.Info(method, "*************** *************** Miscellaneous *************** ***************");
            Log.Info(method, " ");
            foreach (KeyValuePair<string, string> item in dicMessagesToLog)
            {
                Log.Info(method, item.Key + ": " + item.Value);
            }

            Log.Info(method, " ");
        }

        private static void LogEcusList(HashSet<(string Name, bool IsSuccessful)> items, string method)
        {
            Log.Info(method, "*************** *************** List of ECU's and their status *************** ***************");
            Log.Info(method, " ");
            Log.Info(method, "Total amount of ECU's: {0}, of which were {1} OK and {2} not OK. OK and not OK will be 0 if only an Ident was run.", items.Count, items.Count(((string Name, bool IsSuccessful) x) => x.IsSuccessful), items.Count(((string Name, bool IsSuccessful) x) => !x.IsSuccessful));
            Log.Info(method, " ");
            foreach (var item in items)
            {
                Log.Info(method, $"ECU Name: {item.Name}: Is Successful: {item.IsSuccessful}");
            }

            Log.Info(method, " ");
        }

        private static void ClearLists()
        {
            importantItems.Clear();
            listOfEcus.Clear();
            miscItems.Clear();
            backendCalls.Clear();
            vehicleTests.Clear();
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