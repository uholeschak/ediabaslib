using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using Ediabas;
using EdiabasLib;
using PsdzClient.Contracts;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PsdzClient.Psdz;

#pragma warning disable CS0169, CS0649
namespace PsdzClient.Core.Container
{
    public class ECUKom : ECUKomBase
    {
        [PreserveSource(Hint = "api modified", SuppressWarning = true)]
        private ApiInternal api;
        private const string ERROR_ECU_ZDF_REJECT = "ERROR_ECU_ZDF_REJECT";
        private const int DEFAULT_EDIABAS_TRACELEVEL = 6;
        private const int DEFAULT_IFH_TRACELEVEL = 3;
        private const int DEFAULT_SYSTEM_IFH_TRACELEVEL = 6;
        private const int DEFAULT_SYSTEM_NET_TRACELEVEL = 6;
        private const int DEFAULT_EDIABAS_TRACE_SIZE = 32767;
        private const int _diagnosticPort = 50160;
        private const int _controlPort = 50161;
        private const int _portDoIP = 50162;
        private const int _sslPort = 50163;
        private const int _sslPortDirect = 3496;
        private const int _diagnosticPortW2V = 51560;
        private const int _controlPortW2V = 51561;
        private const int _portDoIPW2V = 51562;
        private readonly string[] _sgbmIdsToCheck = new string[2]
        {
            "SWFK-0000CED0",
            "SWFK-0000CFBC"
        };
        [PreserveSource(Hint = "ServiceController sc", Placeholder = true)]
        private PlaceholderType sc;
        [PreserveSource(Hint = "ServiceControllerPermission scp", Placeholder = true)]
        private PlaceholderType scp;
        private bool serviceIsRunning;
        private bool isTestCertReqCallExecuted;
        private bool useSpecialEdiabasVersion;
        private SpecialSecurityCases detectedSpecialSecurityCase;
        private readonly IInteractionService interactionService;
        private readonly ISec4DiagHandler sec4DiagHandler;
        private readonly IFasta2Service fasta2Service;
        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public ECUKom() : this(null, new List<string>())
        {
        }

        [PreserveSource(Hint = "ediabas added, EDIABAS_MONITOR removed", SignatureModified = true)]
        public ECUKom(string app, IList<string> lang, EdiabasNet ediabas = null)
        {
            //[-] api = CreateApi();
            //[+] api = CreateApi(ediabas);
            api = CreateApi(ediabas);
            communicationMode = CommMode.Normal;
            base.jobList = new List<ECUJob>();
            base.APP = app;
            base.FromFastaConfig = false;
            base.CacheHitCounter = 0;
            base.lang = lang;
            ServiceLocator.Current.TryGetService<IInteractionService>(out interactionService);
            ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out sec4DiagHandler);
            ServiceLocator.Current.TryGetService<IFasta2Service>(out fasta2Service);
            IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient();
            try
            {
                if (istaIcsServiceClient.IsAvailable())
                {
                    useSpecialEdiabasVersion = istaIcsServiceClient.GetFeatureEnabledStatus("SpecialEdiabasVersionForNcar").IsActive;
                }
            }
            finally
            {
                ((IDisposable)istaIcsServiceClient)?.Dispose();
            }

            try
            {
            //[-] sc = new ServiceController("EDIABAS_MONITOR", Environment.MachineName);
            //[-] scp = new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, Environment.MachineName, "EDIABAS_MONITOR");
            //[-] if (ServiceController.GetServices().Any((ServiceController x) => x.ServiceName.Equals("EDIABAS_MONITOR", StringComparison.OrdinalIgnoreCase)) && sc.Status == ServiceControllerStatus.Running)
            //[-] {
            //[-] scp.Assert();
            //[-] sc.Refresh();
            //[-] serviceIsRunning = true;
            //[-] }
            }
            catch (Exception)
            {
            }
        }

        [PreserveSource(Hint = "ediabas added, using ApiInternal", SignatureModified = true)]
        private ApiInternal CreateApi(EdiabasNet ediabas)
        {
            //[-] API aPI = new API();
            //[-] if (!TimeMetricsUtility.ShouldEnableMetrics())
            //[-] {
            //[-] return aPI;
            //[-] }
            //[-] return new MetricsProxy<API>(aPI, TimeMetricsUtility.Instance.VehicleCommunicationStart, TimeMetricsUtility.Instance.VehicleCommunicationEnd).GetTransparentProxy() as API;
            //[+] return new ApiInternal(ediabas);
            return new ApiInternal(ediabas);
        }

        public override void End()
        {
            try
            {
                if (base.VCIDeviceType != VCIDeviceType.PTT)
                {
                    api.apiSetConfig("EDIABASUnload", "1");
                }

                api.apiEnd();
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.End()", exception);
            }
        }

        public override string GetEdiabasIniFilePath(string iniFilename)
        {
            return EdiabasIniFilePath(iniFilename);
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public override BoolResultObject InitVCI(IVciDevice vciDevice, bool isDoIP)
        {
            BoolResultObject result = InitVCI(vciDevice, logging: true, isDoIP);
            if (isProblemHandlingTraceRunning)
            {
                SetLogLevelToMax();
            }

            return result;
        }

        public override bool Refresh(bool isDoIP)
        {
            End();
            bool result = false;
            try
            {
                result = InitVCI(base.VCI, isDoIP).Result;
            }
            catch (Exception exception)
            {
                Log.ErrorException("ECUKom.Refresh()", exception);
            }

            return result;
        }

        public override bool ApiInitExt(string ifh, string unit, string app, string reserved)
        {
            return api.apiInitExt(ifh, unit, app, reserved);
        }

        public override void SetEcuPath(bool logging)
        {
            try
            {
                string pathString = ConfigSettings.getPathString("BMW.Rheingold.VehicleCommunication.ECUKom.EcuPath", "..\\..\\..\\Ecu\\");
                if (!string.IsNullOrEmpty(pathString))
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    if (logging)
                    {
                        Log.Info("ECUKom.SetEcuPath()", "found EcuPath config setting: {0} AppDomain.BaseDirectory: {1}", pathString, baseDirectory);
                    }

                    if (Path.IsPathRooted(pathString))
                    {
                        api.apiSetConfig("EcuPath", Path.GetFullPath(pathString));
                        if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleCommunication.ECUKom.SetEcuPathSGCLIB", defaultValue: true))
                        {
                            Environment.SetEnvironmentVariable("SGCLIB_EDIABAS_ECU_PATH", Path.GetFullPath(pathString));
                        }
                    }
                    else
                    {
                        api.apiSetConfig("EcuPath", Path.GetFullPath(Path.Combine(baseDirectory, pathString)));
                        if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleCommunication.ECUKom.SetEcuPathSGCLIB", defaultValue: true))
                        {
                            Environment.SetEnvironmentVariable("SGCLIB_EDIABAS_ECU_PATH", Path.Combine(baseDirectory, pathString));
                        }
                    }

                    api.apiGetConfig("EcuPath", out var cfgValue);
                    if (logging)
                    {
                        Log.Info("ECUKom.SetEcuPath()", "Used EcuPath: {0}", cfgValue);
                    }
                }
                else if (logging)
                {
                    Log.Info("ECUKom.SetEcuPath()", "no config for specific ecu path used; using default values from ediabas config");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ECUKom.SetEcuPath()", exception);
            }
        }

        public static string APIFormatName(int resultFormat)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            switch (resultFormat)
            {
                case 0:
                    return "Ediabas: API.APIFORMAT_CHAR <=> C#: char";
                case 1:
                    return "Ediabas: API.APIFORMAT_BYTE <=> C#: Byte";
                case 2:
                    return "Ediabas: API.APIFORMAT_INTEGER <=> C#: short";
                case 3:
                    return "Ediabas: API.APIFORMAT_WORD <=> C# ushort";
                case 4:
                    return "Ediabas: API.APIFORMAT_LONG <=> C#: int";
                case 5:
                    return "Ediabas: API.APIFORMAT_DWORD <=> C#: uint";
                case 6:
                    return "Ediabas: API.APIFORMAT_TEXT <=> C# String";
                case 8:
                    return "Ediabas: API.APIFORMAT_REAL <=> C#: double";
                case 7:
                    return "Ediabas: API.APIFORMAT_BINARY <=> C# Byte[]";
                default:
                    return "Ediabas: UNKNOWN !!!!";
            }
        }

        [PreserveSource(Hint = "ediabas added", SignatureModified = true)]
        public static ECUKomBase DeSerialize(string filename, EdiabasNet ediabas = null)
        {
            Log.Info("ECUKom.DeSerialize()", "called");
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            ECUKomBase eCUKomBase;
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(filename);
                eCUKomBase = (ECUKomBase)new XmlSerializer(typeof(ECUKom)).Deserialize(xmlTextReader);
                eCUKomBase.jobList.ForEach(delegate (ECUJob job)
                {
                    if (job?.JobResultsForSerialization != null)
                    {
                        job.JobResult = ((IEnumerable<IEcuResult>)job.JobResultsForSerialization).ToList();
                        job.JobResultsForSerialization = null;
                    }
                });
                xmlTextReader.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.DeSerialize()", exception);
                eCUKomBase = new ECUKom("Rheingold", new List<string>());
            }

            VCIDevice vCIDevice = new VCIDevice(VCIDeviceType.SIM, "SIM", filename);
            vCIDevice.Serial = filename;
            vCIDevice.IPAddress = "127.0.0.1";
            eCUKomBase.VCI = vCIDevice;
            eCUKomBase.CommunicationMode = CommMode.Simulation;
            try
            {
                if (eCUKomBase.jobList != null)
                {
                    Log.Info("ECUKom.DeSerialize()", "got {0} jobs from simulation container", eCUKomBase.jobList.Count);
                    foreach (ECUJob job in eCUKomBase.jobList)
                    {
                        job.JobName = job.JobName.ToUpper(CultureInfo.InvariantCulture);
                        job.EcuName = job.EcuName.ToUpper(CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUKom.DeSerialize()", "failed to normalize EcuName and JobName tu uppercase with exception: {0}", ex.ToString());
            }

            Log.Info("ECUKom.DeSerialize()", "successfully done");
            return eCUKomBase;
        }

        public static string EdiabasBinPath()
        {
            try
            {
                string[] array = Environment.GetEnvironmentVariable("PATH").Split(';');
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api32.dll")))
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }

                string[] array2 = array;
                foreach (string text in array2)
                {
                    if (File.Exists(Path.Combine(text, "api32.dll")))
                    {
                        return text;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.EdiabasBinPath()", exception);
            }

            return null;
        }

        public static string EdiabasIniFilePath(string iniFilename)
        {
            try
            {
                string path = EdiabasBinPath();
                if (File.Exists(Path.Combine(path, iniFilename)))
                {
                    return Path.Combine(path, iniFilename);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.EdiabasIniFilePath()", exception);
            }

            return null;
        }

        public static bool Serialize(string filename, IEcuKom ecuKom, Encoding encType)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            Log.Info("ECUKom.Serialize()", "called");
            try
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(filename, encType);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ECUKom));
                ecuKom.JobList.ForEach(delegate (IEcuJob job)
                {
                    if (job?.JobResult != null)
                    {
                        (job as ECUJob).JobResultsForSerialization = job.JobResult.Select((IEcuResult c) => c as ECUResult).ToList();
                    }
                });
                xmlSerializer.Serialize(xmlTextWriter, ecuKom);
                xmlTextWriter.Close();
                Log.Info("ECUKom.Serialize()", "successfully done");
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.Serialize()", exception);
            }

            Log.Warning("ECUKom.Serialize()", "failed when writing ECUKom data");
            return false;
        }

        public override string GetLogPath()
        {
            string result = null;
            try
            {
                //[-] result = api.apiGetConfig("TracePath");
                //[+] if (api.apiGetConfig("TracePath", out string value))
                if (api.apiGetConfig("TracePath", out string value))
                //[+] {
                {
                    //[+] result = value;
                    result = value;
                //[+] }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.getLogPath()", exception);
            }

            return result;
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public override BoolResultObject InitVCI(IVciDevice device, bool logging, bool isDoIP)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            BoolResultObject boolResultObject2 = new BoolResultObject();
            (Sec4CNAuthStates, Sec4CNVehicleGen) tuple = (Sec4CNAuthStates.DEACTIVATED, Sec4CNVehicleGen.NCAR);
            if (device == null)
            {
                Log.Warning(Log.CurrentMethod(), "failed because device was null");
                boolResultObject.SetValues(result: false, "DeviceNull", "Device was null");
                return boolResultObject;
            }

            bool isDoIP2 = device.IsDoIP;
            string pathString = ConfigSettings.getPathString("BMW.Rheingold.Logging.Directory.Current", "..\\..\\..\\logs");
            try
            {
                IstaIcsServiceClient ics = new IstaIcsServiceClient();
                bool flag = false;
                if (ConfigSettings.IsILeanActive && ics.IsAvailable())
                {
                    flag = ics.GetFeatureEnabledStatus("SpecialEdiabasVersionForNcar").IsActive;
                }

                detectedSpecialSecurityCase = SpecialSecurityCases.None;
                if (ConfigSettings.SelectedBrand != UiBrand.BMWMotorrad)
                {
                    CreateEdiabasPublicKeyIfNotExist(device);
                    if (isDoIP2 || isDoIP)
                    {
                        //[-] int id = Process.GetCurrentProcess().Id;
                        //[-] if (!flag && interactionService.RegisterAsync(new InteractionDoIpCheckModel(id)).Result.Action == InteractionButton.Yes)
                        //[-] {
                        //[-] boolResultObject.ErrorCodeInt = 7;
                        //[-] boolResultObject.ErrorMessage = "Only one NCAR session is possible at a time.";
                        //[-] boolResultObject.ErrorCode = ConnectToVehicleErrorCodes.DoIpIsUsedByOtherOperationError.ToString();
                        //[-] return boolResultObject;
                        //[-] }

                        if (!device.IsSimulation)
                        {
                            boolResultObject2 = HandleS29Authentication(device);
                        }
                        else
                        {
                            boolResultObject2.Result = true;
                        }

                        if (!boolResultObject2.Result)
                        {
                            return boolResultObject2;
                        }
                    }
                    else
                    {
                        if (flag)
                        {
                            boolResultObject.ErrorCodeInt = 8;
                            boolResultObject.ErrorMessage = "Only NCAR Vehicles are allowed with the new EDIABAS Version.";
                            boolResultObject.ErrorCode = ConnectToVehicleErrorCodes.NewEdiabasVersionWithoutNcarVehicle.ToString();
                            return boolResultObject;
                        }

                        if (ConfigSettings.IsILeanActive && ics.IsAvailable())
                        {
                            if (!ics.GetSec4DiagEnabledInBackground())
                            {
                                Log.Warning(Log.CurrentMethod(), "Sec4DiagEnbaledInBackground is false");
                            }
                            else
                            {
                                Task.Run(delegate
                                {
                                    TestSubCACall(device);
                                    if (!isTestCertReqCallExecuted && IsActiveLBPFeatureSwitchForCallCertreqProfiles(ics))
                                    {
                                        TestCertReqCall();
                                        isTestCertReqCallExecuted = true;
                                    }
                                });
                            }
                        }
                        else if (ConfigSettings.IsOssModeActive)
                        {
                            Task.Run(delegate
                            {
                                TestSubCACall(device);
                            });
                        }
                    }
                }

                boolResultObject.Result = InitializeDevice(device, logging, isDoIP, isDoIP2);
                if (api.apiErrorCode() != 0)
                {
                    Log.Warning(Log.CurrentMethod(), "failed when init IFH with : {0} / {1}", api.apiErrorCode(), api.apiErrorText());
                }

                api.apiSetConfig("TracePath", Path.GetFullPath(pathString));
                if (boolResultObject.Result)
                {
                    vci = device as VCIDevice;
                    LogDeviceInfo(logging);
                    SetEcuPath(logging);
                    tuple = HandleNonDoIpVehicleAuthentication(device, isDoIP, isDoIP2);
                }
                else
                {
                    SetErrorCode(boolResultObject);
                }

                boolResultObject = CheckNonDoIpVehicleAuthentificationState(tuple.Item1, tuple.Item2, boolResultObject, out var resultHasToBeReturned);
                if (resultHasToBeReturned)
                {
                    return boolResultObject;
                }

                if (ConfigSettings.SelectedBrand != UiBrand.BMWMotorrad && !device.IsSimulation && tuple.Item2 == Sec4CNVehicleGen.NCAR && boolResultObject.Result && boolResultObject2.Result && (isDoIP2 || isDoIP) && !CheckAuthentificationState(device))
                {
                    if (detectedSpecialSecurityCase == SpecialSecurityCases.IpbCertificatesRequired)
                    {
                        boolResultObject.SetValues(result: false, "ZgwIssue", "IPB connected without Sec4Diag");
                    }
                    else
                    {
                        boolResultObject.SetValues(result: false, 0.ToString(), "Authentication failed with Vehicle!");
                    }
                }

                return boolResultObject;
            }
            catch (Exception ex)
            {
                Log.WarningException(Log.CurrentMethod(), ex);
                boolResultObject.SetValues(result: true, "Exception", ex.Message);
                boolResultObject.ErrorCodeInt = 5;
                return boolResultObject;
            }
        }

        private BoolResultObject CheckNonDoIpVehicleAuthentificationState(Sec4CNAuthStates authentificationState, Sec4CNVehicleGen sec4CNVehicleGen, BoolResultObject result, out bool resultHasToBeReturned)
        {
            resultHasToBeReturned = false;
            switch (authentificationState)
            {
                case Sec4CNAuthStates.ACTIVATED:
                    switch (sec4CNVehicleGen)
                    {
                        case Sec4CNVehicleGen.SP18:
                            detectedSpecialSecurityCase = SpecialSecurityCases.Sec4CnTokenRequiredForSp18;
                            result.SetValues(result: false, "Sec4CN-SP18", "SFA Token for SP18 Vehicle required");
                            resultHasToBeReturned = true;
                            break;
                        case Sec4CNVehicleGen.SP21:
                            detectedSpecialSecurityCase = SpecialSecurityCases.Sec4CnTokenRequiredForSp21;
                            result.SetValues(result: false, "Sec4CN-SP21", "SFA Token for SP21 Vehicle required");
                            resultHasToBeReturned = true;
                            break;
                    }

                    break;
                case Sec4CNAuthStates.BOOTLOADER:
                case Sec4CNAuthStates.UNINITIALIZED:
                    detectedSpecialSecurityCase = SpecialSecurityCases.Sec4CnGetewayIssue;
                    result.SetValues(result: true, "Sec4CN-Gateway", "ZGW repair required");
                    break;
            }

            return result;
        }

        [PreserveSource(Cleaned = true, OriginalHash = "BA191431D3FE8C0F1F6D3D0813C723D5")]
        private bool IsActiveLBPFeatureSwitchForCallCertreqProfiles(IstaIcsServiceClient ics)
        {
            return false;
        }

        private (Sec4CNAuthStates Sec4CNAuthStates, Sec4CNVehicleGen Sec4CNVehicleGen) HandleNonDoIpVehicleAuthentication(IVciDevice device, bool isDoIP, bool slpDoIpFromIcom)
        {
            string method = "ECUKom.HandleNonDoIpVehicleAuthentication";
            Sec4CNAuthStates item = Sec4CNAuthStates.DEACTIVATED;
            Sec4CNVehicleGen item2 = Sec4CNVehicleGen.NCAR;
            if (!device.IsDoIP && !isDoIP)
            {
                IEcuJob ecuJob = ApiJob("G_ZGW", "STATUS_LESEN", "ARG;STATUS_AUTHORIZATION_STATE");
                ImportantLoggingItem.AddItemToList("SEC4CN_001", TYPES.Sec4CN);
                fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_001", LayoutGroup.D);
                if (ecuJob.IsOkay())
                {
                    long resultsAs = ecuJob.getResultsAs("STAT_AUTHORIZATION_ENABLE_STATE", 0L, 1);
                    switch (resultsAs)
                    {
                        case 218L:
                            item = Sec4CNAuthStates.DEACTIVATED;
                            Log.Info(method, "No Authentication required");
                            ImportantLoggingItem.AddItemToList("SEC4CN_003", TYPES.Sec4CN);
                            fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_003", LayoutGroup.D);
                            break;
                        case 172L:
                            Log.Info(method, "Authentication required");
                            item = Sec4CNAuthStates.ACTIVATED;
                            if (ecuJob.getResultsAs("STAT_AUTHORISATION_STATE", 0L, 1) == 49)
                            {
                                item = Sec4CNAuthStates.AUTHORIZED;
                                Log.Info(method, "Authorized for Diagnose");
                                ImportantLoggingItem.AddItemToList("SEC4CN_004", TYPES.Sec4CN);
                                fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_004", LayoutGroup.D);
                                break;
                            }

                            switch (ecuJob.getResultsAs("STAT_AUTH_MECHANISM", 0L, 1))
                            {
                                case 24L:
                                    item2 = Sec4CNVehicleGen.SP18;
                                    Log.Info(method, "SFA Token for SP18 Vehicle required");
                                    ImportantLoggingItem.AddItemToList("SEC4CN_005", TYPES.Sec4CN);
                                    fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_005", LayoutGroup.D);
                                    break;
                                case 33L:
                                    item2 = Sec4CNVehicleGen.SP21;
                                    Log.Info(method, "SFA Token for SP21 Vehicle required");
                                    ImportantLoggingItem.AddItemToList("SEC4CN_006", TYPES.Sec4CN);
                                    fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_006", LayoutGroup.D);
                                    break;
                                default:
                                    item2 = Sec4CNVehicleGen.NCAR;
                                    Log.Info(method, "Use Sec4Diag");
                                    ImportantLoggingItem.AddItemToList("SEC4CN_007", TYPES.Sec4CN);
                                    fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_007", LayoutGroup.D);
                                    break;
                            }

                            break;
                        case 189L:
                            item = Sec4CNAuthStates.BOOTLOADER;
                            Log.Info(method, "No Authentication required --> do ZGW repair");
                            ImportantLoggingItem.AddItemToList("SEC4CN_008", TYPES.Sec4CN);
                            fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_008", LayoutGroup.D);
                            break;
                        case 254L:
                            item = Sec4CNAuthStates.UNINITIALIZED;
                            Log.Info(method, "No Authentication required --> do ZGW repair");
                            ImportantLoggingItem.AddItemToList("SEC4CN_009", TYPES.Sec4CN);
                            fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_009", LayoutGroup.D);
                            break;
                        default:
                            item = Sec4CNAuthStates.DEACTIVATED;
                            Log.Warning(method, "Unknown Enable State: {0}", resultsAs);
                            ImportantLoggingItem.AddItemToList("SEC4CN_010", TYPES.Sec4CN);
                            fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_010", LayoutGroup.D);
                            break;
                    }
                }
                else
                {
                    ImportantLoggingItem.AddItemToList("SEC4CN_002", TYPES.Sec4CN);
                    fasta2Service.AddServiceCode(ServiceCodes.S4C00_InfoCode_nu_LF, "SEC4CN_010", LayoutGroup.D);
                }
            }

            return (Sec4CNAuthStates: item, Sec4CNVehicleGen: item2);
        }

        private bool InitializeDevice(IVciDevice device, bool logging, bool isDoIP, bool slpDoIpFromIcom)
        {
            switch (device.VCIType)
            {
                case VCIDeviceType.ICOM:
                    return InitializeIcomDevice(device, logging, isDoIP, slpDoIpFromIcom);
                case VCIDeviceType.ENET:
                    return InitializeEnetDevice(device);
                case VCIDeviceType.PTT:
                    return InitializePttDevice(device, logging, isDoIP);
                case VCIDeviceType.SIM:
                    return true;
                default:
                    return api.apiInit();
            }
        }

        [PreserveSource(Hint = "Modified", SignatureModified = true)]
        private bool InitializePttDevice(IVciDevice device, bool logging, bool isDoIP)
        {
            if (isDoIP)
            {
                string reserved = $"RemoteHost={device.IPAddress};selectCertificate={sec4DiagHandler.CertificateFilePathWithoutEnding};SSLPort={3496};Authentication=S29;NetworkProtocol=SSL";
                return api.apiInitExt("ENET", "_", "Rheingold", reserved);
            }

            //[-] return api.apiInitExtForPTT("RPLUS:ICOM_P:Remotehost=127.0.0.1;Port=6408", "_", "Rheingold", string.Empty, logging);
            //[+] return false;
            return false;
        }

        private bool InitializeIcomDevice(IVciDevice device, bool logging, bool isDoIP, bool slpDoIpFromIcom)
        {
            if (slpDoIpFromIcom || isDoIP)
            {
                return InitEdiabasForDoIP(device);
            }

            //[-] if (!string.IsNullOrEmpty(device.VIN) && !isDoIP)
            //[-] {
            //[-] return api.apiInitExt("RPLUS:ICOM_P:Remotehost=" + device.IPAddress + ";Port=6801", "", "", string.Empty, logging);
            //[-] }
            //[-] if (!isDoIP)
            //[-] {
            //[-] return api.apiInitExt("RPLUS:ICOM_P:Remotehost=" + device.IPAddress + ";Port=6801", "", "", string.Empty, logging);
            //[-] }
            //[-] return false;
            //[+] return api.apiInitExt("RPLUS:ICOM_P:Remotehost=" + device.IPAddress + ";Port=6801", "", "", string.Empty);
            return api.apiInitExt("RPLUS:ICOM_P:Remotehost=" + device.IPAddress + ";Port=6801", "", "", string.Empty);
        }

        private bool InitializeEnetDevice(IVciDevice device)
        {
            if (device.IsDoIP && ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service))
            {
                string reserved = $"RemoteHost={device.IPAddress};selectCertificate={service.CertificateFilePathWithoutEnding};SSLPort={3496};Authentication=S29;NetworkProtocol=SSL";
                return api.apiInitExt("ENET", "_", "Rheingold", reserved);
            }

            return api.apiInitExt("ENET", "_", "Rheingold", "RemoteHost=" + device.IPAddress + ";DiagnosticPort=6801;ControlPort=6811;Authentication=None;NetworkProtocol=TCP");
        }

        private void LogDeviceInfo(bool logging)
        {
            if (logging)
            {
                string method = Log.CurrentMethod();
                api.apiGetConfig("TracePath", out var cfgValue);
                Log.Info(method, "Ediabas TracePath is loaded: {0}", cfgValue);
                api.apiGetConfig("EdiabasVersion", out var cfgValue2);
                Log.Info(method, "Ediabas version loaded: {0}", cfgValue2);
                api.apiGetConfig("IfhVersion", out var cfgValue3);
                Log.Info(method, "IfhVersion version loaded: {0}", cfgValue3);
                api.apiGetConfig("Session", out var cfgValue4);
                Log.Info(method, "Session name: {0}", cfgValue4);
                api.apiGetConfig("Interface", out var cfgValue5);
                Log.Info(method, "Interface type loaded: {0}", cfgValue5);
            }
        }

        private void SetErrorCode(BoolResultObject result)
        {
            int num = api.apiErrorCode();
            result.ErrorCode = num.ToString();
            result.ErrorMessage = api.apiErrorText();
            if (num >= 350 && num <= 395)
            {
                result.ErrorCodeInt = 0;
            }
            else
            {
                result.ErrorCodeInt = 2;
            }
        }

        private bool CheckAuthentificationState(IVciDevice device)
        {
            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service))
            {
                IEcuJob ecuJob = ApiJob("IPB_APP2", "STATUS_LESEN", "ARG;SEC4DIAG_READ_AUTH_MODE");
                if (api.apiErrorCode() == 162)
                {
                    CheckIfGatewayChangeForNcarIsRequired(device);
                    return false;
                }

                if (ecuJob.IsOkay())
                {
                    uint? num = ecuJob.getuintResult("STAT_ROLL_MASK_WERT");
                    string stringResult = ecuJob.getStringResult("AuthenticationReturnParameter_TEXT");
                    if (num == service.RoleMaskAsInt)
                    {
                        ImportantLoggingItem.AddMessagesToLog("S29-Authentification-Ediabas", "Authentification: $" + stringResult);
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        private void CreateEdiabasPublicKeyIfNotExist(IVciDevice device)
        {
            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service) && service.CheckIfEdiabasPublicKeyExists())
            {
                return;
            }

            string reserved = string.Empty;
            switch (device.VCIType)
            {
                case VCIDeviceType.ICOM:
                    reserved = $"RemoteHost={device.IPAddress};DiagnosticPort={50160};ControlPort={50161};PortDoIP={50162};SSLPort={50163};Authentication=S29;NetworkProtocol=SSL";
                    break;
                case VCIDeviceType.ENET:
                    reserved = $"RemoteHost={device.IPAddress};SSLPort={3496};Authentication=S29;NetworkProtocol=SSL";
                    break;
                case VCIDeviceType.PTT:
                    reserved = "RPLUS:ICOM_P:remotehost=127.0.0.1;Port=6408";
                    break;
            }

            if (ApiInitExt("ENET", "_", "Rheingold", reserved))
            {
                SetEcuPath(logging: false);
                ApiJob("F01", "IDENT", string.Empty, string.Empty);
                while (api.apiState() == 0)
                {
                    SleepUtility.ThreadSleep(200, "ECUKom.CreateEdiabasPubglickeyIfNotExist - F01, IDENT");
                }

                if (api.apiErrorCode() != 162 || !CheckIfGatewayChangeForNcarIsRequired(device))
                {
                    End();
                }
            }
        }

        private bool CheckIfGatewayChangeForNcarIsRequired(IVciDevice device)
        {
            End();
            string reserved = string.Empty;
            string text = Log.CurrentMethod();
            switch (device.VCIType)
            {
                case VCIDeviceType.ICOM:
                    reserved = $"RemoteHost={device.IPAddress};DiagnosticPort={50160};ControlPort={50161};PortDoIP={50162}";
                    break;
                case VCIDeviceType.ENET:
                    reserved = "RemoteHost=" + device.IPAddress + ";";
                    break;
                case VCIDeviceType.PTT:
                    reserved = "RPLUS:ICOM_P:remotehost=127.0.0.1;Port=6408";
                    break;
            }

            if (ApiInitExt("ENET", "_", "Rheingold", reserved))
            {
                SetEcuPath(logging: false);
                IEcuJob ecuJob = ApiJob("IPB_APP2", "SVK_LESEN", "");
                while (api.apiState() == 0)
                {
                    SleepUtility.TaskDelay(200, text + " - IPB_APP2, SVK_Lesen").GetAwaiter().GetResult();
                }

                if (ecuJob.IsOkay())
                {
                    for (int i = 1; i < ecuJob.JobResultSets; i++)
                    {
                        string stringResult = ecuJob.getStringResult((ushort)i, "SGBM_ID");
                        Log.Info(text, "SGBM_ID: '" + stringResult + "'");
                        if (stringResult != null && stringResult.Length >= 13 && _sgbmIdsToCheck.Contains(stringResult.Substring(0, 13)))
                        {
                            Log.Info(text, "Searched SGBM_ID has been recognised.");
                            detectedSpecialSecurityCase = SpecialSecurityCases.IpbCertificatesRequired;
                            break;
                        }
                    }
                }
                else
                {
                    Log.Error(text, "The Job SVK_LESEN was not successful.");
                }
            }

            Log.Info(text, "detectedSpecialSecurityCase: '" + detectedSpecialSecurityCase.ToString() + "'");
            return detectedSpecialSecurityCase == SpecialSecurityCases.IpbCertificatesRequired;
        }

        private BoolResultObject HandleS29Authentication(IVciDevice device)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            Log.CurrentMethod();
            try
            {
                if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service) && ServiceLocator.Current.TryGetService<IBackendCallsWatchDog>(out var service2) && ServiceLocator.Current.TryGetService<IFasta2Service>(out var service3))
                {
                    if (service.CheckIfEdiabasPublicKeyExists())
                    {
                        service.EdiabasPublicKey = service.GetPublicKeyFromEdiabas();
                        string configString = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.Ca", string.Empty);
                        string configString2 = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa", string.Empty);
                        if (ConfigSettings.IsOssModeActive)
                        {
                            WebCallResponse<Sec4DiagResponseData> webCallResponse = RequestCertificate(device, service, service2);
                            if (webCallResponse.IsSuccessful)
                            {
                                boolResultObject.Result = webCallResponse.IsSuccessful;
                            }
                            else
                            {
                                boolResultObject.Result = false;
                                boolResultObject.StatusCode = (int)webCallResponse.HttpStatus.Value;
                                boolResultObject.ErrorMessage = webCallResponse.Error;
                                boolResultObject.ErrorCodeInt = 1;
                            }

                            return boolResultObject;
                        }

                        if (string.IsNullOrEmpty(configString) || string.IsNullOrEmpty(configString2))
                        {
                            if (!WebCallUtility.CheckForInternetConnection() && !WebCallUtility.CheckForIntranetConnection())
                            {
                                ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_004", TYPES.Sec4Diag);
                                service3.AddServiceCode(ServiceCodes.S4D03_ErrorCode_nu_LF, "ErrorCode: SEC4DIAG_Error_004", LayoutGroup.D);
                                boolResultObject.ErrorCodeInt = 3;
                                return boolResultObject;
                            }

                            ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_001", TYPES.Sec4Diag);
                            service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "ErrorCode: SEC4DIAG_001", LayoutGroup.D);
                            WebCallResponse<Sec4DiagResponseData> webCallResponse2 = RequestCaAndSubCACertificates(device, service, service2, testRun: false);
                            if (webCallResponse2.IsSuccessful)
                            {
                                ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_003", TYPES.Sec4Diag);
                                service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "ErrorCode: SEC4DIAG_003", LayoutGroup.D);
                                boolResultObject.Result = webCallResponse2.IsSuccessful;
                            }
                            else
                            {
                                ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_001", TYPES.Sec4Diag);
                                service3.AddServiceCode(ServiceCodes.S4D03_ErrorCode_nu_LF, "ErrorCode: SEC4DIAG_Error_001", LayoutGroup.D);
                                boolResultObject.Result = webCallResponse2.IsSuccessful;
                                boolResultObject.StatusCode = (int)(webCallResponse2.HttpStatus.HasValue ? webCallResponse2.HttpStatus.Value : ((HttpStatusCode)0));
                                boolResultObject.ErrorCodeInt = 1;
                                boolResultObject.ErrorMessage = webCallResponse2.Error;
                            }

                            return boolResultObject;
                        }

                        ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_003", TYPES.Sec4Diag);
                        service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_003", LayoutGroup.D);
                        X509Certificate2Collection subCaCertificate = new X509Certificate2Collection();
                        X509Certificate2Collection caCertificate = new X509Certificate2Collection();
                        Sec4DiagCertificateState sec4DiagCertificateState = service.SearchForCertificatesInWindowsStore(configString, configString2, out subCaCertificate, out caCertificate);
                        if (!WebCallUtility.CheckForInternetConnection() && !WebCallUtility.CheckForIntranetConnection() && sec4DiagCertificateState == Sec4DiagCertificateState.NotYetExpired)
                        {
                            TimeSpan subCAZertifikateRemainingTime = GetSubCAZertifikateRemainingTime();
                            interactionService.RegisterMessage(new FormatedData("Info").Localize(), new FormatedData("#Sec4Diag.OfflineButTokenStillValid", subCAZertifikateRemainingTime.Days).Localize());
                            ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_007", TYPES.Sec4Diag);
                            service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_007", LayoutGroup.D);
                            boolResultObject = service.CertificatesAreFoundAndValid(device, subCaCertificate, caCertificate);
                            return boolResultObject;
                        }

                        if (sec4DiagCertificateState == Sec4DiagCertificateState.Valid && subCaCertificate.Count == 1 && caCertificate.Count == 1)
                        {
                            ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_004", TYPES.Sec4Diag);
                            service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_004", LayoutGroup.D);
                            boolResultObject = service.CertificatesAreFoundAndValid(device, subCaCertificate, caCertificate);
                            return boolResultObject;
                        }

                        switch (sec4DiagCertificateState)
                        {
                            case Sec4DiagCertificateState.NotYetExpired:
                            {
                                ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_004", TYPES.Sec4Diag);
                                service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_004", LayoutGroup.D);
                                WebCallResponse<Sec4DiagResponseData> webCallResponse4 = RequestCaAndSubCACertificates(device, service, service2, testRun: false);
                                if (webCallResponse4.IsSuccessful)
                                {
                                    ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_005", TYPES.Sec4Diag);
                                    service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_005", LayoutGroup.D);
                                    boolResultObject.Result = webCallResponse4.IsSuccessful;
                                }
                                else
                                {
                                    TimeSpan subCAZertifikateRemainingTime2 = GetSubCAZertifikateRemainingTime();
                                    interactionService.RegisterMessage(new FormatedData("Info").Localize(), new FormatedData("#Sec4Diag.SubCaBackendErrorButTokenStillValid", subCAZertifikateRemainingTime2.Days).Localize());
                                    ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_006", TYPES.Sec4Diag);
                                    service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_006", LayoutGroup.D);
                                    boolResultObject = service.CertificatesAreFoundAndValid(device, subCaCertificate, caCertificate);
                                }

                                return boolResultObject;
                            }

                            case Sec4DiagCertificateState.Expired:
                            case Sec4DiagCertificateState.NotFound:
                            {
                                ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_002", TYPES.Sec4Diag);
                                service3.AddServiceCode(ServiceCodes.S4D03_ErrorCode_nu_LF, "ErrorCode: SEC4DIAG_Error_002", LayoutGroup.D);
                                WebCallResponse<Sec4DiagResponseData> webCallResponse3 = RequestCaAndSubCACertificates(device, service, service2, testRun: false);
                                if (webCallResponse3.IsSuccessful)
                                {
                                    ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_003", TYPES.Sec4Diag);
                                    service3.AddServiceCode(ServiceCodes.S4D02_InfoCode_nu_LF, "SEC4DIAG_003", LayoutGroup.D);
                                    boolResultObject.Result = webCallResponse3.IsSuccessful;
                                }
                                else
                                {
                                    ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_001", TYPES.Sec4Diag);
                                    service3.AddServiceCode(ServiceCodes.S4D03_ErrorCode_nu_LF, "ErrorCode: SEC4DIAG_Error_001", LayoutGroup.D);
                                    boolResultObject.Result = webCallResponse3.IsSuccessful;
                                    boolResultObject.StatusCode = (int)(webCallResponse3.HttpStatus.HasValue ? webCallResponse3.HttpStatus.Value : ((HttpStatusCode)0));
                                    boolResultObject.ErrorCodeInt = 1;
                                    boolResultObject.ErrorMessage = webCallResponse3.Error;
                                }

                                return boolResultObject;
                            }

                            default:
                                ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_003", TYPES.Sec4Diag);
                                service3.AddServiceCode(ServiceCodes.S4D03_ErrorCode_nu_LF, "ErrorCode: SEC4DIAG_Error_003", LayoutGroup.D);
                                boolResultObject.Result = false;
                                return boolResultObject;
                        }
                    }

                    boolResultObject.Result = false;
                    boolResultObject.ErrorMessage = "EDIABAS PEM File not found.";
                    boolResultObject.ErrorCode = ConnectToVehicleErrorCodes.EdiabasPemFileNotFound.ToString();
                    boolResultObject.ErrorCodeInt = 9;
                    return boolResultObject;
                }

                ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_005", TYPES.Sec4Diag);
                boolResultObject.Result = false;
                boolResultObject.ErrorMessage = "ISec4DiagHandler or IBackendCallsWatchDog not found";
                boolResultObject.ErrorCodeInt = 1;
                return boolResultObject;
            }
            catch (Exception ex)
            {
                Log.ErrorException("HandleS29Authentication", ex);
                boolResultObject.Result = false;
                boolResultObject.ErrorMessage = ex.Message;
                boolResultObject.ErrorCodeInt = 1;
                return boolResultObject;
            }
        }

        private void TestCertReqCall()
        {
            try
            {
                if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service) && ServiceLocator.Current.TryGetService<IBackendCallsWatchDog>(out var service2))
                {
                    RequestCertReqProfil(service, service2);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("TestCertReqCall", exception);
            }
        }

        private BoolResultObject TestSubCACall(IVciDevice device)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            string method = Log.CurrentMethod();
            try
            {
                if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service) && ServiceLocator.Current.TryGetService<IBackendCallsWatchDog>(out var service2))
                {
                    service.EdiabasPublicKey = service.GetPublicKeyFromEdiabas();
                    string configString = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.Ca", string.Empty);
                    string configString2 = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa", string.Empty);
                    if (ConfigSettings.IsOssModeActive)
                    {
                        WebCallResponse<Sec4DiagResponseData> webCallResponse = RequestCertificate(device, service, service2);
                        if (webCallResponse.IsSuccessful)
                        {
                            boolResultObject.Result = webCallResponse.IsSuccessful;
                        }
                        else
                        {
                            boolResultObject.Result = false;
                            boolResultObject.StatusCode = (int)webCallResponse.HttpStatus.Value;
                            boolResultObject.ErrorMessage = webCallResponse.Error;
                            boolResultObject.ErrorCodeInt = 1;
                        }

                        return boolResultObject;
                    }

                    if (string.IsNullOrEmpty(configString) || string.IsNullOrEmpty(configString2))
                    {
                        ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_001", TYPES.Sec4Diag);
                        Log.Info(method, "Code: SEC4DIAG_001");
                        WebCallResponse<Sec4DiagResponseData> webCallResponse2 = RequestCaAndSubCACertificates(device, service, service2, testRun: true);
                        if (webCallResponse2.IsSuccessful)
                        {
                            ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_003", TYPES.Sec4Diag);
                            Log.Info(method, "Code: SEC4DIAG_003");
                            boolResultObject.Result = webCallResponse2.IsSuccessful;
                        }
                        else
                        {
                            ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_001", TYPES.Sec4Diag);
                            Log.Error(method, "ErrorCode: SEC4DIAG_Error_001");
                            boolResultObject.Result = webCallResponse2.IsSuccessful;
                            boolResultObject.StatusCode = (int)(webCallResponse2.HttpStatus.HasValue ? webCallResponse2.HttpStatus.Value : ((HttpStatusCode)0));
                            boolResultObject.ErrorCodeInt = 1;
                            boolResultObject.ErrorMessage = webCallResponse2.Error;
                        }

                        return boolResultObject;
                    }

                    ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_003", TYPES.Sec4Diag);
                    Log.Info(method, "Code: SEC4DIAG_002");
                    X509Certificate2Collection subCaCertificate = new X509Certificate2Collection();
                    X509Certificate2Collection caCertificate = new X509Certificate2Collection();
                    Sec4DiagCertificateState sec4DiagCertificateState = service.SearchForCertificatesInWindowsStore(configString, configString2, out subCaCertificate, out caCertificate);
                    if (!WebCallUtility.CheckForInternetConnection() && !WebCallUtility.CheckForIntranetConnection() && sec4DiagCertificateState == Sec4DiagCertificateState.NotYetExpired)
                    {
                        boolResultObject.Result = true;
                        return boolResultObject;
                    }

                    if (sec4DiagCertificateState == Sec4DiagCertificateState.Valid && subCaCertificate.Count == 1 && caCertificate.Count == 1)
                    {
                        boolResultObject.Result = true;
                        return boolResultObject;
                    }

                    switch (sec4DiagCertificateState)
                    {
                        case Sec4DiagCertificateState.NotYetExpired:
                            boolResultObject.Result = true;
                            return boolResultObject;
                        case Sec4DiagCertificateState.Expired:
                        case Sec4DiagCertificateState.NotFound:
                        {
                            ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_002", TYPES.Sec4Diag);
                            Log.Error(method, "ErrorCode: SEC4DIAG_Error_002");
                            WebCallResponse<Sec4DiagResponseData> webCallResponse3 = RequestCaAndSubCACertificates(device, service, service2, testRun: true);
                            if (webCallResponse3.IsSuccessful)
                            {
                                ImportantLoggingItem.AddItemToList("Code: SEC4DIAG_003", TYPES.Sec4Diag);
                                Log.Info(method, "Code: SEC4DIAG_003");
                                boolResultObject.Result = webCallResponse3.IsSuccessful;
                            }
                            else
                            {
                                ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_001", TYPES.Sec4Diag);
                                Log.Info(method, "ErrorCode: SEC4DIAG_Error_001");
                                boolResultObject.Result = webCallResponse3.IsSuccessful;
                                boolResultObject.StatusCode = (int)webCallResponse3.HttpStatus.Value;
                                boolResultObject.ErrorCodeInt = 1;
                                boolResultObject.ErrorMessage = webCallResponse3.Error;
                            }

                            return boolResultObject;
                        }

                        default:
                            ImportantLoggingItem.AddItemToList("ErrorCode: SEC4DIAG_Error_003", TYPES.Sec4Diag);
                            Log.Error(method, "ErrorCode: SEC4DIAG_Error_003");
                            boolResultObject.Result = false;
                            return boolResultObject;
                    }
                }

                Log.Error("HandleS29Authentication", "ISec4DiagHandler or IBackendCallsWatchDog not found");
                boolResultObject.Result = false;
                boolResultObject.ErrorMessage = "ISec4DiagHandler or IBackendCallsWatchDog not found";
                boolResultObject.ErrorCodeInt = 1;
                return boolResultObject;
            }
            catch (Exception ex)
            {
                Log.ErrorException("HandleS29Authentication", ex);
                boolResultObject.Result = false;
                boolResultObject.ErrorMessage = ex.Message;
                boolResultObject.ErrorCodeInt = 1;
                return boolResultObject;
            }
        }

        private TimeSpan GetSubCAZertifikateRemainingTime()
        {
            DateTime dateTime = DateTime.ParseExact(sec4DiagHandler.ReadoutExpirationTime(), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime now = DateTime.Now;
            return dateTime - now;
        }

        [PreserveSource(Cleaned = true, OriginalHash = "A54CCEFE36F21D280EC1F996CDF6E24E")]
        private WebCallResponse<Sec4DiagResponseData> RequestCaAndSubCACertificates(IVciDevice device, ISec4DiagHandler sec4DiagHandler, IBackendCallsWatchDog backendCallsWatchDog, bool testRun)
        {
            throw new InvalidOperationException("IDataContext service not found.");
        }

        [PreserveSource(Cleaned = true, OriginalHash = "4D1D0C7E4C6AD0B5B12DE7429ABD37CE")]
        private WebCallResponse<bool> RequestCertReqProfil(ISec4DiagHandler sec4DiagHandler, IBackendCallsWatchDog backendCallsWatchDog)
        {
            throw new InvalidOperationException("IDataContext service not found.");
        }

        [PreserveSource(Cleaned = true, OriginalHash = "F7D287B4437241744997FAB84CF754A0")]
        private WebCallResponse<Sec4DiagResponseData> RequestCertificate(IVciDevice device, ISec4DiagHandler sec4DiagHandler, IBackendCallsWatchDog backendCallsWatchDog)
        {
            throw new InvalidOperationException("IDataContext service not found.");
        }

        private bool InitEdiabasForDoIP(IVciDevice device)
        {
            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service))
            {
                string text = "";
                text = (device.IsSimulation ? $"RemoteHost={device.IPAddress};DiagnosticPort={50160};ControlPort={50161};PortDoIP={50162}" : $"RemoteHost={device.IPAddress};DiagnosticPort={50160};ControlPort={50161};PortDoIP={50162};selectCertificate={service.CertificateFilePathWithoutEnding};SSLPort={50163};Authentication=S29;NetworkProtocol=SSL");
                return ApiInitExt("ENET", "_", "Rheingold", text);
            }

            return false;
        }

        public override (bool, ECUJob) HandleEcuAuthorizationRejection(string ecu, string jobName, string param, string resultFilter, string jobStatus, bool isRetry)
        {
            string method = Log.CurrentMethod();
            IEcuJob ecuJob = null;
            bool item = false;
            bool flag = false;
            int num = 399;
            int num2 = api.apiErrorCode();
            bool flag2 = useSpecialEdiabasVersion && num2 == num;
            if (!isRetry && (jobStatus.Equals("ERROR_ECU_ZDF_REJECT", StringComparison.InvariantCultureIgnoreCase) || flag2))
            {
                bool flag3 = CheckAuthentificationState(base.VCI);
                if (!flag3)
                {
                    Log.Info(method, "ERROR_ECU_ZDF_REJECT where sent by the ZDF");
                    item = true;
                    End();
                    flag = InitializeDevice(base.VCI, logging: true, isDoIP: true, slpDoIpFromIcom: true);
                    SetEcuPath(logging: true);
                    Log.Info(method, "Ediabas is reinitialized with status {0}", flag);
                }

                if (flag3 && flag2)
                {
                    item = true;
                    flag = true;
                }

                if (flag && flag3)
                {
                    Log.Info(method, "Ediabas reinitialized and AuthenticationState are succesfull. DiagnoseJob will be resend.");
                    ecuJob = apiJob(ecu, jobName, param, resultFilter, cacheAdding: true, isRetry: true);
                }
                else
                {
                    string text = $"ECU: {ecu}, Job: {jobName}, Argument: {param}, ResultFilter: {resultFilter}";
                    if (ServiceLocator.Current.TryGetService<IFasta2Service>(out var service))
                    {
                        service.AddServiceCode(ServiceCodes.S4D04_DiagJobRejectByZdf_nu_LF, text, LayoutGroup.D);
                    }

                    Log.Info(method, "Ediabas reinitialized or AuthenticationState is wrong. Popup with Infos will be shown.");
                    string textItem = new TextContent(new FormatedData("#Sec4Diag.ZDFReject", false, "ERROR_ECU_ZDF_REJECT"), lang).GetTextForUI(lang)[0].TextItem;
                    interactionService.RegisterMessage(FormatedData.Localize("#Note"), textItem, text);
                }
            }

            return (item, ecuJob as ECUJob);
        }

        public override void RemoveTraceLevel(string callerMember)
        {
            if (!UseConfigFileTraces())
            {
                api.apiSetConfig("ApiTrace", "0");
                api.apiSetConfig("IfhTrace", "0");
                api.apiSetConfig("SystemTraceIfh", "0");
                api.apiSetConfig("SystemTraceNet", "0");
            }

            isProblemHandlingTraceRunning = false;
        }

        public override void SetTraceLevelToMax(string callerMember)
        {
            if (!(callerMember == "run") && !UseConfigFileTraces())
            {
                if (ConfigSettings.GetFeatureEnabledStatus("LogTracingMaximum").IsActive || ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Logging.Level.Trace.Enabled", defaultValue: false))
                {
                    int configint = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Level.Trace.Ediabas", 6);
                    int configint2 = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Trace.Ediabas.Size", 32767);
                    api.apiSetConfig("ApiTrace", configint.ToString(CultureInfo.InvariantCulture));
                    api.apiSetConfig("TraceSize", configint2.ToString(CultureInfo.InvariantCulture));
                    api.apiSetConfig("IfhTrace", 3.ToString());
                    api.apiSetConfig("SystemTraceIfh", 6.ToString());
                    api.apiSetConfig("SystemTraceNet", 6.ToString());
                }

                isProblemHandlingTraceRunning = true;
            }
        }

        public override SpecialSecurityCases DetectedSpecialSecurityCase()
        {
            return detectedSpecialSecurityCase;
        }

        private bool UseConfigFileTraces()
        {
            //[-] string text = api.apiGetConfig("ApiTrace");
            //[-] string text2 = api.apiGetConfig("IfhTrace");
            //[-] string text3 = api.apiGetConfig("SystemTraceIfh");
            //[-] string text4 = api.apiGetConfig("SystemTraceNet");
            //[+] api.apiGetConfig("ApiTrace", out string text);
            api.apiGetConfig("ApiTrace", out string text);
            //[+] api.apiGetConfig("IfhTrace", out string text2);
            api.apiGetConfig("IfhTrace", out string text2);
            //[+] api.apiGetConfig("SystemTraceIfh", out string text3);
            api.apiGetConfig("SystemTraceIfh", out string text3);
            //[+] api.apiGetConfig("SystemTraceNet", out string text4);
            api.apiGetConfig("SystemTraceNet", out string text4);
            if (text != "0" || text2 != "0" || text3 != "0" || text4 != "0")
            {
                return true;
            }

            return false;
        }

        public override int getErrorCode()
        {
            return api.apiErrorCode();
        }

        public override string getErrorText()
        {
            return api.apiErrorText();
        }

        public int getState(int suspendTime)
        {
            return api.apiStateExt(suspendTime);
        }

        public override bool setConfig(string cfgName, string cfgValue)
        {
            return api.apiSetConfig(cfgName, cfgValue);
        }

        public override IEcuJob ExecuteJobOverEnetWrapper(string icomAddress, string ecu, string job, string param, bool isDoIP, string method, string resultFilter = "", int retries = 0)
        {
            string istaLogPath = GetIstaLogPath();
            if (string.IsNullOrEmpty(istaLogPath))
            {
                Log.Warning("EdiabasUtils.ExecuteJobOverEnet()", "Path to ista log cannot be found.");
                return null;
            }

            string cfgValue = (base.IsProblemHandlingTraceRunning ? "5" : "0");
            Log.Info(method, "Before ApiInitExt");
            string reserved = $"RemoteHost={icomAddress};DiagnosticPort={51560};ControlPort={51561};PortDoIP={51562};";
            bool num = ApiInitExt("ENET", "_", "Rheingold", reserved);
            api.apiSetConfig("ApiTrace", cfgValue);
            api.apiSetConfig("TracePath", Path.GetFullPath(istaLogPath));
            Log.Info(method, "After ApiInitExt");
            if (!num)
            {
                Log.Warning("EdiabasUtils.ExecuteJobOverEnet()", "Failed switching to ENET. The Job will not be executed. The EDIABAS connection will be refreshed.");
                Log.Info(method, "Before invalid refresh");
                RefreshEdiabasConnection(isDoIP);
                Log.Info(method, "After invalid refresh");
                return null;
            }

            SetEcuPath(logging: true);
            Log.Info(method, "Before ApiJob");
            IEcuJob ecuJob = ApiJob(ecu, job, param, resultFilter, retries);
            Log.Info(method, $"After ApiJob, ECode: {ecuJob.JobErrorCode}, EText: {ecuJob.JobErrorText}");
            Log.Info(method, "Before valid Refresh");
            return ecuJob;
        }

        private static string GetIstaLogPath()
        {
            string result = string.Empty;
            try
            {
                result = Path.GetFullPath(ConfigSettings.getPathString("BMW.Rheingold.Logging.Directory.Current", "..\\..\\..\\logs"));
            }
            catch (Exception ex)
            {
                Log.Warning("EdiabasUtils.GetIstaLogPath()", "Exception occurred: ", ex);
            }

            return result;
        }

        private static string GetStack()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                int num = 1;
                do
                {
                    MethodBase method = new StackFrame(num).GetMethod();
                    string name = method.DeclaringType.Name;
                    stringBuilder.Append("-> " + name + "." + method.Name + " ");
                    num++;
                }
                while (num < 10);
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }

            return stringBuilder.ToString();
        }

        public IEcuJob apiJobWaitWhenPending(string ecu, string job, string param, string resultFilter, double timeout)
        {
            IEcuJob ecuJob = null;
            bool flag = false;
            DateTime dateTime = DateTime.Now.AddMilliseconds(timeout);
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                while (!flag && dateTime > DateTime.Now)
                {
                    ecuJob = apiJob(ecu, job, param, resultFilter);
                    if (ecuJob.IsDone() && !ecuJob.IsJobState("ERROR_ECU_REQUEST_CORRECTLY_RECEIVED__RESPONSE_PENDING"))
                    {
                        return ecuJob;
                    }
                }

                return ecuJob;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.apiJobWaitWhenPending()", exception);
                ecuJob = new ECUJob();
                ecuJob.EcuName = ecu;
                ecuJob.ExecutionStartTime = DateTime.Now;
                ecuJob.ExecutionEndTime = ecuJob.ExecutionStartTime;
                ecuJob.JobName = job;
                ecuJob.JobParam = param;
                ecuJob.JobResultFilter = resultFilter;
                ecuJob.JobErrorCode = 90;
                ecuJob.JobErrorText = "SYS-0000: INTERNAL ERROR";
                ecuJob.JobResult = new List<IEcuResult>();
                AddJobInCache(ecuJob);
                return ecuJob;
            }
        }

        public bool getConfig(string cfgName, out string cfgValue)
        {
            return api.apiGetConfig(cfgName, out cfgValue);
        }

        public int getState()
        {
            return api.apiState();
        }

        public int waitJobDone(int suspendTime)
        {
            return getState(suspendTime);
        }

        public override void ExecuteEdiabasJobAndGetResults(ECUJob ecuJob, string callerMember)
        {
            string ecuName = ecuJob.EcuName;
            string jobName = ecuJob.JobName;
            string jobParam = ecuJob.JobParam;
            string jobResultFilter = ecuJob.JobResultFilter;
            int num = 0;
            string empty = string.Empty;
            SetTraceLevelToMax(callerMember);
            if (serviceIsRunning)
            {
                try
                {
                //[-] sc.ExecuteCommand(150);
                }
                catch
                {
                //[-] Log.Error("ECUKom.apiJob()", $"Ediabas monitor executeCommand failed for Command {EdiabasMonitorTrigger.apijob}");
                }
            }

            api.apiJob(ecuName, jobName, jobParam, jobResultFilter);
            while (api.apiStateExt(1000) == 0)
            {
                SleepUtility.ThreadSleep(2, "ECUKom.apiJob - " + ecuName + ", " + jobName + ", " + jobParam);
            }

            if (serviceIsRunning)
            {
                try
                {
                //[-] sc.ExecuteCommand(151);
                }
                catch
                {
                }
            }

            RemoveTraceLevel(callerMember);
            if (api.apiStateExt(1000) == 3)
            {
                num = api.apiErrorCode();
                empty = api.apiErrorText();
                ecuJob.JobErrorCode = num;
                ecuJob.JobErrorText = empty;
            }

            api.apiResultSets(out var rsets);
            if (serviceIsRunning)
            {
                try
                {
                //[-] sc.ExecuteCommand(152);
                }
                catch
                {
                }
            }

            ecuJob.JobResultSets = rsets;
            if (rsets > 0)
            {
                Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJob()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - successfully called: {4}:{5} RSets: {6}", ecuName, jobName, jobParam, jobResultFilter, ecuJob.JobErrorCode, ecuJob.JobErrorText, rsets);
                for (ushort num2 = 0; num2 <= rsets; num2++)
                {
                    if (api.apiResultNumber(out var buffer, num2))
                    {
                        for (ushort num3 = 1; num3 <= buffer; num3++)
                        {
                            ECUResult eCUResult = new ECUResult();
                            string buffer2 = string.Empty;
                            eCUResult.Set = num2;
                            if (api.apiResultName(out buffer2, num3, num2))
                            {
                                eCUResult.Name = buffer2;
                                if (api.apiResultFormat(out var buffer3, buffer2, num2))
                                {
                                    eCUResult.Format = buffer3;
                                    switch (buffer3)
                                    {
                                        case 1:
                                        {
                                            api.apiResultByte(out var buffer10, buffer2, num2);
                                            eCUResult.Value = buffer10;
                                            break;
                                        }

                                        case 0:
                                        {
                                            api.apiResultChar(out var buffer11, buffer2, num2);
                                            eCUResult.Value = buffer11;
                                            break;
                                        }

                                        case 5:
                                        {
                                            api.apiResultDWord(out var buffer12, buffer2, num2);
                                            eCUResult.Value = buffer12;
                                            break;
                                        }

                                        case 2:
                                        {
                                            api.apiResultInt(out var buffer9, buffer2, num2);
                                            eCUResult.Value = buffer9;
                                            break;
                                        }

                                        case 4:
                                        {
                                            api.apiResultLong(out var buffer6, buffer2, num2);
                                            eCUResult.Value = buffer6;
                                            break;
                                        }

                                        case 8:
                                        {
                                            api.apiResultReal(out var buffer7, buffer2, num2);
                                            eCUResult.Value = buffer7;
                                            break;
                                        }

                                        case 6:
                                        {
                                            api.apiResultText(out var buffer8, buffer2, num2, string.Empty);
                                            eCUResult.Value = buffer8;
                                            break;
                                        }

                                        case 3:
                                        {
                                            api.apiResultWord(out var buffer5, buffer2, num2);
                                            eCUResult.Value = buffer5;
                                            break;
                                        }

                                        case 7:
                                        {
                                            uint bufferLen2;
                                            if (api.apiResultBinary(out var buffer4, out var bufferLen, buffer2, num2))
                                            {
                                                if (buffer4 != null)
                                                {
                                                    Array.Resize(ref buffer4, bufferLen);
                                                }

                                                eCUResult.Value = buffer4;
                                                eCUResult.Length = bufferLen;
                                            }
                                            else if (api.apiResultBinaryExt(out buffer4, out bufferLen2, 65536u, buffer2, num2))
                                            {
                                                if (buffer4 != null)
                                                {
                                                    Array.Resize(ref buffer4, (int)bufferLen2);
                                                }

                                                eCUResult.Value = buffer4;
                                                eCUResult.Length = bufferLen2;
                                            }
                                            else
                                            {
                                                eCUResult.Value = new byte[0];
                                                eCUResult.Length = 0u;
                                            }

                                            break;
                                        }

                                        default:
                                        {
                                            api.apiResultVar(out var var);
                                            eCUResult.Value = var;
                                            break;
                                        }
                                    }
                                }

                                ecuJob.JobResult.Add(eCUResult);
                            }
                            else
                            {
                                buffer2 = string.Format(CultureInfo.InvariantCulture, "ResName unknown! Job was: {0} result index: {1} set index{2}", jobName, num3, num2);
                            }
                        }
                    }
                }
            }

            if (num != 0)
            {
                Log.Info("ECUKom.apiJob()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with apiError: {4}:{5}", ecuName, jobName, jobParam, jobResultFilter, ecuJob.JobErrorCode, ecuJob.JobErrorText);
            }
        }

        public override void ExecuteEdiabasJobAndGetResultsWithByteParams(ECUJob ecuJob, byte[] param, int paramlen, string callerMember)
        {
            string ecuName = ecuJob.EcuName;
            string jobName = ecuJob.JobName;
            string jobResultFilter = ecuJob.JobResultFilter;
            int num = 0;
            ecuJob.JobParam = FormatConverter.ByteArray2String(param, (uint)paramlen);
            SetTraceLevelToMax(callerMember);
            api.apiJobData(ecuName, jobName, param, paramlen, jobResultFilter);
            while (api.apiStateExt(1000) == 0)
            {
                SleepUtility.ThreadSleep(2, "ECUKom.apiJob - " + ecuName + ", " + jobName + ", byte[]");
            }

            RemoveTraceLevel(callerMember);
            num = (ecuJob.JobErrorCode = api.apiErrorCode());
            ecuJob.JobErrorText = api.apiErrorText();
            if (num == 0)
            {
                Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - successfully called: {4}:{5}", ecuName, jobName, param, jobResultFilter, ecuJob.JobErrorCode, ecuJob.JobErrorText);
                if (!api.apiResultSets(out var rsets))
                {
                    return;
                }

                ecuJob.JobResultSets = rsets;
                for (ushort num3 = 0; num3 <= rsets; num3++)
                {
                    if (api.apiResultNumber(out var buffer, num3))
                    {
                        for (ushort num4 = 1; num4 <= buffer; num4++)
                        {
                            ECUResult eCUResult = new ECUResult();
                            eCUResult.Set = num3;
                            if (api.apiResultName(out var buffer2, num4, num3))
                            {
                                eCUResult.Name = buffer2;
                                if (api.apiResultFormat(out var buffer3, buffer2, num3))
                                {
                                    eCUResult.Format = buffer3;
                                    switch (buffer3)
                                    {
                                        case 1:
                                        {
                                            api.apiResultByte(out var buffer10, buffer2, num3);
                                            eCUResult.Value = buffer10;
                                            break;
                                        }

                                        case 0:
                                        {
                                            api.apiResultChar(out var buffer11, buffer2, num3);
                                            eCUResult.Value = buffer11;
                                            break;
                                        }

                                        case 5:
                                        {
                                            api.apiResultDWord(out var buffer12, buffer2, num3);
                                            eCUResult.Value = buffer12;
                                            break;
                                        }

                                        case 2:
                                        {
                                            api.apiResultInt(out var buffer9, buffer2, num3);
                                            eCUResult.Value = buffer9;
                                            break;
                                        }

                                        case 4:
                                        {
                                            api.apiResultLong(out var buffer6, buffer2, num3);
                                            eCUResult.Value = buffer6;
                                            break;
                                        }

                                        case 8:
                                        {
                                            api.apiResultReal(out var buffer7, buffer2, num3);
                                            eCUResult.Value = buffer7;
                                            break;
                                        }

                                        case 6:
                                        {
                                            api.apiResultText(out var buffer8, buffer2, num3, string.Empty);
                                            eCUResult.Value = buffer8;
                                            break;
                                        }

                                        case 3:
                                        {
                                            api.apiResultWord(out var buffer5, buffer2, num3);
                                            eCUResult.Value = buffer5;
                                            break;
                                        }

                                        case 7:
                                        {
                                            uint bufferLen2;
                                            if (api.apiResultBinary(out var buffer4, out var bufferLen, buffer2, num3))
                                            {
                                                if (buffer4 != null)
                                                {
                                                    Array.Resize(ref buffer4, bufferLen);
                                                }

                                                eCUResult.Value = buffer4;
                                                eCUResult.Length = bufferLen;
                                            }
                                            else if (api.apiResultBinaryExt(out buffer4, out bufferLen2, 65536u, buffer2, num3))
                                            {
                                                if (buffer4 != null)
                                                {
                                                    Array.Resize(ref buffer4, (int)bufferLen2);
                                                }

                                                eCUResult.Value = buffer4;
                                                eCUResult.Length = bufferLen2;
                                            }
                                            else
                                            {
                                                eCUResult.Value = new byte[0];
                                                eCUResult.Length = 0u;
                                            }

                                            break;
                                        }

                                        default:
                                        {
                                            api.apiResultVar(out var var);
                                            eCUResult.Value = var;
                                            break;
                                        }
                                    }
                                }

                                ecuJob.JobResult.Add(eCUResult);
                            }
                            else
                            {
                                buffer2 = string.Format(CultureInfo.InvariantCulture, "ResName unknown! Job was: {0} result index: {1} set index{2}", jobName, num4, num3);
                            }
                        }
                    }
                }
            }
            else
            {
                Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with apiError: {4}:{5}", ecuName, jobName, param, jobResultFilter, ecuJob.JobErrorCode, ecuJob.JobErrorText);
            }
        }

        public override void EndTimeMetric(string ecu, string jobName, string param, int argsLength = -1)
        {
            TimeMetricsUtility.Instance.ApiJobEnd(ecu, jobName, param, argsLength);
        }

        public override void StartTimeMetric(string ecu, string jobName, string param, int argsLength = -1)
        {
            TimeMetricsUtility.Instance.ApiJobStart(ecu, jobName, param, argsLength);
        }
    }
}