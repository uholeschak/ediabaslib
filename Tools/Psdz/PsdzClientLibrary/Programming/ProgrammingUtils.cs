using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.Programming.Common
{
    public class ProgrammingUtils
    {
        private static TaCategories[] DisabledTaCategories = new TaCategories[1] { TaCategories.Unknown };

        public static readonly TaCategories[] EnabledTaCategories = (from TaCategories x in Enum.GetValues(typeof(TaCategories))
            where !DisabledTaCategories.Contains(x)
            select x).ToArray();

        internal static bool IsExpertModeEnabled => ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.ExpertMode", defaultValue: false);

        internal static bool IsFastaEnabled
        {
            get
            {
                if (ConfigSettings.getConfigStringAsBoolean("FASTAEnabled", defaultValue: true))
                {
                    return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.FASTAEnabled", defaultValue: false);
                }
                return false;
            }
        }

        public static bool IsFlashableOverMost(IEcu ecu)
        {
            if (ecu.BUS == BusType.MOST || (ecu.BUS == BusType.VIRTUAL && ecu.ID_SG_ADR == 160))
            {
                return true;
            }
            return false;
        }

        public static bool IsUsedSpecificRoutingTable(IEcu ecu)
        {
            IList<long> list = new List<long>();
            list.Add(41L);
            if (ecu != null && !list.Contains(ecu.ID_SG_ADR))
            {
                return !ecu.IsVirtualOrVirtualBusCheck();
            }
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public static bool RequestClamp15State(IProgressMonitor monitor, IEcuKom ecuKom, IVehicle vehicle)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public static bool RequestClamp30State(IProgressMonitor monitor, IEcuKom ecuKom)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public static string RetrieveNewZbNr()
        {
            throw new NotImplementedException();
        }

        public static bool WriteBn2000Istufen(IEcuKom ecuKom, string groupSgbd, string iStufeWerk, string iStufeHo, string iStufeHoBackup)
        {
            if (ecuKom == null)
            {
                throw new ArgumentNullException("ecuKom");
            }
            if (groupSgbd == null)
            {
                throw new ArgumentNullException("groupSgbd");
            }
            if (iStufeWerk == null)
            {
                throw new ArgumentNullException("iStufeWerk");
            }
            if (iStufeHo == null)
            {
                throw new ArgumentNullException("iStufeHo");
            }
            if (iStufeHoBackup == null)
            {
                throw new ArgumentNullException("iStufeHoBackup");
            }
            string text = string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}", iStufeWerk, iStufeHo, iStufeHoBackup);
            if (ecuKom.ApiJob(groupSgbd, "I_STUFE_SCHREIBEN", text, string.Empty).IsOkay())
            {
                Log.Info("ProgrammingUtils.WriteBn2000Istufen()", "Integration level ({0}) successfully written! (SGBD: '{1}')", text, groupSgbd);
                return true;
            }
            Log.Error("ProgrammingUtils.WriteBn2000Istufen()", "Failed to write integration level ({0})! (SGBD: '{1}')", text, groupSgbd);
            return false;
        }

        [PreserveSource(Cleaned = true)]
        internal static IProgrammingAction AddProgrammingActionToEcu(EcuProgrammingInfo ecu)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public static FormatedData CreateProgrammingMessageClamp30(double? voltage, BatteryEnum vehicleBattery, double clampMinValue)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public static bool CheckClamp30ForProgramming(double voltage, BatteryEnum vehicleBattery)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "VehicleProgramingProhibitionReason", Placeholder = true)]
        public static PlaceholderType CheckVehicleProgramingProhibits()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "ConnectionResult<FormatedData>", Placeholder = true)]
        public static PlaceholderType CheckConnectionLan()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true, OriginalHash = "7FC863B731110E8C4456F2073E8BE8F5")]
        public static bool IsToyota()
        {
            return false;
        }

        [PreserveSource(Cleaned = true, OriginalHash = "C222EF38D34939CDBD98728578208800")]
        private static void CheckClamp30(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }

        private static bool ProhibitionIsCircumvented()
        {
            bool defaultValue = ConfigSettings.IsLightModeActive;
            return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.RheingoldSessionController.CircumventProgramingProhibition", defaultValue);
        }

        [PreserveSource(Cleaned = true, OriginalHash = "A4E9AB03DE103323608792E045FBE180")]
        private static void CheckBatteryStatus()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "ConnectionResult<FormatedData>", Placeholder = true)]
        protected static PlaceholderType CheckConnectionWithResult(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true, OriginalHash = "F7566E35F78D5A3BF71E9D0048964DF0")]
        [Obsolete("Use the method CheckConnectionWithResult instead")]
        protected static void CheckConnection(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }

        public static bool VCIHasWIFIConnection(Vehicle vehicle)
        {
            string text = 1.ToString();
            return vehicle.VCI.NetworkType == text;
        }

        private static bool PCHasWIFIConnection(Vehicle vehicle)
        {
            return NetworkType.WLAN == vehicle.VCI.LocalAdapterNetworkType;
        }

        internal static IPsdzTalFilter CreateTalFilter(ProgrammingTaskFlags programmingTaskFlags, IPsdzObjectBuilder objectBuilder)
        {
            ISet<TaCategories> set = new HashSet<TaCategories>();
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.EnforceCoding))
            {
                set.Add(TaCategories.CdDeploy);
            }
            IPsdzTalFilter inputTalFilter = objectBuilder.DefineFilterForAllEcus(set.ToArray(), TalFilterOptions.Must, null);
            ISet<TaCategories> set2 = new HashSet<TaCategories>();
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Mount))
            {
                set2.Add(TaCategories.HwInstall);
            }
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Unmount))
            {
                set2.Add(TaCategories.HwDeinstall);
            }
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Replace))
            {
                set2.Add(TaCategories.HwInstall);
                set2.Add(TaCategories.HwDeinstall);
            }
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Flash))
            {
                set2.Add(TaCategories.BlFlash);
                set2.Add(TaCategories.SwDeploy);
                set2.Add(TaCategories.IbaDeploy);
                set2.Add(TaCategories.EcuMirrorDeploy);
                set2.Add(TaCategories.EcuActivate);
                set2.Add(TaCategories.EcuPoll);
            }
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Code))
            {
                set2.Add(TaCategories.CdDeploy);
            }
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.DataRecovery))
            {
                set2.Add(TaCategories.IdBackup);
                set2.Add(TaCategories.IdRestore);
                set2.Add(TaCategories.FscBackup);
            }
            if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Fsc))
            {
                set2.Add(TaCategories.FscDeploy);
                set2.Add(TaCategories.FscDeployPrehwd);
            }
            ISet<TaCategories> set3 = new HashSet<TaCategories>(EnabledTaCategories);
            set3.ExceptWith(set);
            set3.ExceptWith(set2);
            inputTalFilter = objectBuilder.DefineFilterForAllEcus(set3.ToArray(), TalFilterOptions.MustNot, inputTalFilter);
            Log.Info("ProgrammingUtils.CreateTalFilter()", "TALFilter: {0}", NormalizeXmlText(inputTalFilter?.AsXml));
            return inputTalFilter;
        }

        internal static IEcuJob ExecuteFinalizeJob(IEcuKom ecuKom, string ecu, string job, string argument)
        {
            IEcuJob ecuJob = ecuKom.ApiJob(ecu, job, argument, string.Empty);
            string msg = string.Format("apiJob('{0}','{1}','{2}','') - JOB_STATUS: '{3}'", ecu, job, argument, string.IsNullOrEmpty(ecuJob.getStringResult("JOB_STATUS")) ? "<None>" : ecuJob.getStringResult("JOB_STATUS"));
            if (ecuJob.IsOkay())
            {
                Log.Info("ProgrammingUtils.ExecuteFinalizeJob()", msg);
            }
            else
            {
                Log.Warning("ProgrammingUtils.ExecuteFinalizeJob()", msg);
            }
            return ecuJob;
        }

        internal static IPsdzTal FilterTalByBn2020ProgrammingActions(ILogicService logicService, EcuProgrammingInfos ecuProgrammingInfos, IPsdzTal srcTal, IPsdzObjectBuilder objectBuilder)
        {
            if (logicService == null)
            {
                throw new ArgumentNullException("logicService");
            }
            if (ecuProgrammingInfos == null)
            {
                throw new ArgumentNullException("ecuProgrammingInfos");
            }
            if (srcTal == null)
            {
                throw new ArgumentNullException("srcTal");
            }
            IPsdzTalFilter psdzTalFilter = objectBuilder.BuildTalFilter();
            foreach (IPsdzEcuIdentifier affectedEcu in srcTal.AffectedEcus)
            {
                EcuProgrammingInfo itemFromProgrammingInfos = ecuProgrammingInfos.GetItemFromProgrammingInfos(affectedEcu.DiagAddrAsInt);
                if (itemFromProgrammingInfos.Ecu.IsSmartActuator)
                {
                    continue;
                }
                if (itemFromProgrammingInfos != null)
                {
                    ISet<TaCategories> set = new HashSet<TaCategories>();
                    Dictionary<string, TalFilterOptions> smartActuatorFilter = GetSmartActuatorFilter(ecuProgrammingInfos, affectedEcu.DiagAddrAsInt);
                    int num = 0;
                    foreach (IProgrammingAction programmingAction in itemFromProgrammingInfos.ProgrammingActions)
                    {
                        if (!programmingAction.IsSelected)
                        {
                            switch (programmingAction.Type)
                            {
                                case ProgrammingActionType.Programming:
                                    set.Add(TaCategories.SwDeploy);
                                    set.Add(TaCategories.EcuMirrorDeploy);
                                    set.Add(TaCategories.EcuActivate);
                                    set.Add(TaCategories.EcuPoll);
                                    break;
                                case ProgrammingActionType.BootloaderProgramming:
                                    set.Add(TaCategories.BlFlash);
                                    break;
                                case ProgrammingActionType.Coding:
                                    set.Add(TaCategories.CdDeploy);
                                    break;
                                case ProgrammingActionType.Unmounting:
                                    set.Add(TaCategories.HwDeinstall);
                                    break;
                                case ProgrammingActionType.Mounting:
                                    set.Add(TaCategories.HwInstall);
                                    break;
                                case ProgrammingActionType.Replacement:
                                    set.Add(TaCategories.HwInstall);
                                    set.Add(TaCategories.HwDeinstall);
                                    break;
                                default:
                                    Log.Warning("ProgrammingUtils.FilterTalByBn2020ProgrammingActions()", "Type '{0}' not yet supported!", programmingAction.Type);
                                    break;
                            }
                            num++;
                        }
                    }
                    if (object.Equals(num, itemFromProgrammingInfos.ProgrammingActions.Count()))
                    {
                        psdzTalFilter = objectBuilder.DefineFilterForSelectedEcus(EnabledTaCategories, new int[1] { affectedEcu.DiagAddrAsInt }, TalFilterOptions.MustNot, psdzTalFilter, smartActuatorFilter);
                    }
                    else if (set.Count > 0)
                    {
                        psdzTalFilter = objectBuilder.DefineFilterForSelectedEcus(set.ToArray(), new int[1] { affectedEcu.DiagAddrAsInt }, TalFilterOptions.MustNot, psdzTalFilter, smartActuatorFilter);
                    }
                    else if (smartActuatorFilter.Any())
                    {
                        psdzTalFilter = objectBuilder.DefineFilterForSelectedEcus(set.ToArray(), new int[1] { affectedEcu.DiagAddrAsInt }, TalFilterOptions.Must, psdzTalFilter, smartActuatorFilter);
                    }
                }
                else
                {
                    Log.Warning("ProgrammingUtils.FilterTalByBn2020ProgrammingActions()", "ECU for identifier '{0}' could not be found!", affectedEcu.ToString());
                }
            }
            return logicService.FilterTal(srcTal, psdzTalFilter);
        }

        public static IPsdzTal FilterTalBySWEs(List<EcuFilterOnSweLevel> sweFilter, IPsdzTal curTal, IPsdzObjectBuilder objectBuilder, ILogicService logicService)
        {
            IPsdzTal result = curTal;
            try
            {
                if (sweFilter != null && sweFilter.Any())
                {
                    IPsdzTalFilter talFilter = objectBuilder.BuildTalFilter();
                    talFilter = UpdateTalFilterWithSweFilter(sweFilter, talFilter, objectBuilder, curTal.TalLines);
                    IPsdzTal psdzTal = logicService.FilterTal(curTal, talFilter);
                    if (psdzTal != null)
                    {
                        result = psdzTal;
                        Log.Info(Log.CurrentMethod(), "Updated TAL with TalFilter on SweLevel. Used TalFilter: " + talFilter.AsXml);
                    }
                    else
                    {
                        Log.Error(Log.CurrentMethod(), "TAL is null after trying to filter it on swe level - use old Tal");
                    }
                }
            }
            catch (Exception arg)
            {
                Log.Error(Log.CurrentMethod(), $"Error when trying to filter TAL on swe level - {arg}");
            }
            return result;
        }

        private static IPsdzTalFilter UpdateTalFilterWithSweFilter(List<EcuFilterOnSweLevel> ecuSweFilter, IPsdzTalFilter talFilter, IPsdzObjectBuilder objectBuilder, IEnumerable<IPsdzTalLine> talLines)
        {
            if (ecuSweFilter != null)
            {
                foreach (EcuFilterOnSweLevel item in ecuSweFilter)
                {
                    List<IPsdzTa> tAs = GetTAs(talLines, item.TaCategory);
                    foreach (ISweTalFilterOptions sweFilter in item.SweTalFilterOptions)
                    {
                        IPsdzTa psdzTa = tAs.FirstOrDefault((IPsdzTa x) => x.SgbmId.ProcessClass.ToLower().Equals(sweFilter.ProcessClass.ToLower()));
                        if (psdzTa == null)
                        {
                            Log.Info(Log.CurrentMethod(), $"No TA found for diagaddress '{item.DiagAddress}' and ProcessClass '{sweFilter.ProcessClass}'");
                        }
                        else
                        {
                            sweFilter.Ta = psdzTa;
                        }
                    }
                    talFilter = objectBuilder.DefineFilterForSWEs(item, talFilter);
                }
            }
            return talFilter;
        }

        private static List<IPsdzTa> GetTAs(IEnumerable<IPsdzTalLine> talLines, TaCategories category)
        {
            switch (category)
            {
                case TaCategories.SwDeploy:
                    return talLines.Where((IPsdzTalLine x) => !x.SwDeploy.IsEmpty).SelectMany((IPsdzTalLine x) => x.SwDeploy.Tas).ToList();
                case TaCategories.EcuActivate:
                    return talLines.Where((IPsdzTalLine x) => !x.EcuActivate.IsEmpty).SelectMany((IPsdzTalLine x) => x.EcuActivate.Tas).ToList();
                case TaCategories.EcuPoll:
                    return talLines.Where((IPsdzTalLine x) => !x.EcuPoll.IsEmpty).SelectMany((IPsdzTalLine x) => x.EcuPoll.Tas).ToList();
                case TaCategories.EcuMirrorDeploy:
                    return talLines.Where((IPsdzTalLine x) => !x.EcuMirrorDeploy.IsEmpty).SelectMany((IPsdzTalLine x) => x.EcuMirrorDeploy.Tas).ToList();
                default:
                    throw new ArgumentException("Ta Category '" + category.ToString() + "' is not supported");
            }
        }

        private static Dictionary<string, TalFilterOptions> GetSmartActuatorFilter(EcuProgrammingInfos ecuProgrammingInfos, int masterEcuID)
        {
            Dictionary<string, TalFilterOptions> dictionary = new Dictionary<string, TalFilterOptions>();
            foreach (IEcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
            {
                if (ecuProgrammingInfo.Ecu is SmartActuatorECU smartActuatorECU && smartActuatorECU != null && smartActuatorECU.SmacMasterDiagAddressAsInt == masterEcuID && ecuProgrammingInfo.ProgrammingActions.Any((IProgrammingAction x) => !x.IsSelected))
                {
                    dictionary.Add(smartActuatorECU.SmacID, TalFilterOptions.MustNot);
                }
            }
            return dictionary;
        }

        internal static IList<IKmmPlanElement> GetFilteredKmmPlanElements(IDictionary<IProgrammingAction, IKmmPlanElement> kmmProgrammingActionMap, KmmPlanElementAction filterAction)
        {
            if (kmmProgrammingActionMap == null)
            {
                throw new ArgumentNullException("kmmProgrammingActionMap");
            }
            List<IKmmPlanElement> list = new List<IKmmPlanElement>();
            foreach (KeyValuePair<IProgrammingAction, IKmmPlanElement> item in kmmProgrammingActionMap)
            {
                if (item.Key.IsSelected)
                {
                    IKmmPlanElement value = item.Value;
                    if (value != null && value.Action == filterAction)
                    {
                        list.Add(value);
                    }
                }
            }
            list.Sort();
            return list;
        }

        internal static ProgrammingActionType GetProgrammingActionType(string datafile)
        {
            if (!string.IsNullOrEmpty(datafile))
            {
                if (Regex.IsMatch(datafile, "\\.(baf|0ba)$", RegexOptions.IgnoreCase))
                {
                    return ProgrammingActionType.BootloaderProgramming;
                }
                if (Regex.IsMatch(datafile, "\\.(daf|0da)$", RegexOptions.IgnoreCase))
                {
                    return ProgrammingActionType.DataProgramming;
                }
            }
            return ProgrammingActionType.Programming;
        }

        internal static bool IsKmmPlanElementRelevant(ProgrammingTaskFlags programmingTaskFlags, IKmmPlanElement kmmPlanElement)
        {
            if (kmmPlanElement != null)
            {
                switch (kmmPlanElement.Action)
                {
                    case KmmPlanElementAction.MountEcu:
                        return (programmingTaskFlags & ProgrammingTaskFlags.Mount) == ProgrammingTaskFlags.Mount;
                    case KmmPlanElementAction.UnmountEcu:
                        return (programmingTaskFlags & ProgrammingTaskFlags.Unmount) == ProgrammingTaskFlags.Unmount;
                    case KmmPlanElementAction.ReplaceEcu:
                        return (programmingTaskFlags & ProgrammingTaskFlags.Replace) == ProgrammingTaskFlags.Replace;
                    case KmmPlanElementAction.FlashEcu:
                        return (programmingTaskFlags & ProgrammingTaskFlags.Flash) == ProgrammingTaskFlags.Flash;
                    case KmmPlanElementAction.CodeEcu:
                        return (programmingTaskFlags & ProgrammingTaskFlags.Code) == ProgrammingTaskFlags.Code;
                    case KmmPlanElementAction.ImportFsc:
                    case KmmPlanElementAction.ActivateSwt:
                    case KmmPlanElementAction.DeactivateSwt:
                        return false;
                }
            }
            return false;
        }

        internal static bool PerformFlashPostconditionsExxe(IEcu ecu, IEcuKom ecuKom, IVehicle vehicle, IProgressMonitor progressMonitor)
        {
            if (!ecuKom.Refresh(vehicle.IsDoIP))
            {
                Log.Error("ProgramBn2000EcusExxeState.PerformFlashPostconditions()", "Connection to EDIABAS could not be established!");
                return false;
            }
            if (!ActiveGatewayUtils.WriteRoutingTable(ecuKom, vehicle, null))
            {
                return false;
            }
            return RequestClamp15State(progressMonitor, ecuKom, vehicle);
        }

        internal static bool PerformFlashPreconditionsExxe(IEcu ecu, IEcuKom ecuKom, IVehicle vehicle, IProgressMonitor progressMonitor)
        {
            if (!ecuKom.Refresh(vehicle.IsDoIP))
            {
                Log.Error("ProgrammingUtils.PerformFlashPreconditionsExxe()", "Connection to EDIABAS could not be established!");
                return false;
            }
            if (!ActiveGatewayUtils.WriteRoutingTable(ecuKom, vehicle, null))
            {
                return false;
            }
            bool flag = ((ecu.ID_SG_ADR != 64) ? RequestClamp15State(progressMonitor, ecuKom, vehicle) : RequestClamp30State(progressMonitor, ecuKom));
            if (flag && IsUsedSpecificRoutingTable(ecu))
            {
                flag = ActiveGatewayUtils.WriteRoutingTable(ecuKom, vehicle, ecu.ID_SG_ADR);
            }
            return flag;
        }

        internal static ProgrammingTaskFlags RetrieveProgrammingTaskFlagsFromTasks(IEnumerable<IProgrammingTask> programmingTasks)
        {
            ProgrammingTaskFlags programmingTaskFlags = (ProgrammingTaskFlags)0;
            if (programmingTasks != null)
            {
                foreach (IProgrammingTask programmingTask in programmingTasks)
                {
                    programmingTaskFlags |= programmingTask.Flags;
                }
            }
            return programmingTaskFlags;
        }

        internal static void UpdateSingleProgrammingAction(ProgrammingEventManager eventManager, EcuProgrammingInfo ecuProgrammingInfo, ProgrammingActionType type, ProgrammingActionState state)
        {
            if (ecuProgrammingInfo.UpdateSingleProgrammingAction(type, state))
            {
                eventManager?.OnProgrammingActionStateChanged(ecuProgrammingInfo.Ecu, type, state);
            }
        }

        private static IList<string> GetProgrammingChannels(KmmFlashFlags kmmFlashFlags)
        {
            IList<string> list = new List<string>();
            if ((kmmFlashFlags & KmmFlashFlags.FlashNormal) == KmmFlashFlags.FlashNormal)
            {
                list.Add("K-LINE");
            }
            if ((kmmFlashFlags & KmmFlashFlags.FlashCAN) == KmmFlashFlags.FlashCAN)
            {
                list.Add("D-CAN");
            }
            if ((kmmFlashFlags & KmmFlashFlags.FlashMOSTControl) == KmmFlashFlags.FlashMOSTControl)
            {
                list.Add("MOST-CONTROL");
            }
            if ((kmmFlashFlags & KmmFlashFlags.FlashMOSTAsync) == KmmFlashFlags.FlashMOSTAsync)
            {
                list.Add("MOST-ASYNC");
            }
            if ((kmmFlashFlags & KmmFlashFlags.FlashMOSTSync) == KmmFlashFlags.FlashMOSTSync)
            {
                list.Add("MOST-SYNC");
            }
            if ((kmmFlashFlags & KmmFlashFlags.FlashCD) == KmmFlashFlags.FlashCD)
            {
                list.Add("CD");
            }
            if ((kmmFlashFlags & KmmFlashFlags.FlashDVD) == KmmFlashFlags.FlashDVD)
            {
                list.Add("DVD");
            }
            return list;
        }

        internal static string NormalizeXmlText(string xmlText)
        {
            if (string.IsNullOrEmpty(xmlText))
            {
                return xmlText;
            }
            return Regex.Replace(xmlText.Trim(), ">\\s+<", "><");
        }

        [PreserveSource(Added = true)]
        public static FA BuildVehicleFa(IPsdzFa faInput, string br)
        {
            if (faInput == null)
            {
                return null;
            }
            FA fa = new FA();
            fa.VERSION = faInput.FaVersion.ToString(CultureInfo.InvariantCulture);
            fa.BR = br;
            fa.LACK = faInput.Lackcode;
            fa.POLSTER = faInput.Polstercode;
            fa.TYPE = faInput.Type;
            fa.C_DATE = faInput.Zeitkriterium;
            fa.E_WORT = ((faInput.EWords != null) ? new ObservableCollection<string>(faInput.EWords) : null);
            fa.HO_WORT = ((faInput.HOWords != null) ? new ObservableCollection<string>(faInput.HOWords) : null);
            fa.SA = ((faInput.Salapas != null) ? new ObservableCollection<string>(faInput.Salapas) : null);
            return fa;
        }

        [PreserveSource(Added = true)]
        public static BMW.Rheingold.CoreFramework.Contracts.Programming.IFa BuildFa(IPsdzStandardFa faInput)
        {
            if (faInput == null)
            {
                return null;
            }
            return new VehicleOrder
            {
                FaVersion = faInput.FaVersion,
                Entwicklungsbaureihe = faInput.Entwicklungsbaureihe,
                Lackcode = faInput.Lackcode,
                Polstercode = faInput.Polstercode,
                Type = faInput.Type,
                Zeitkriterium = faInput.Zeitkriterium,
                EWords = ((faInput.EWords != null) ? new List<string>(faInput.EWords) : null),
                HOWords = ((faInput.HOWords != null) ? new List<string>(faInput.HOWords) : null),
                Salapas = ((faInput.Salapas != null) ? new List<string>(faInput.Salapas) : null)
            };
        }

        [PreserveSource(Hint = "Example call in: ABL_AUS_RETROFITPROTECTIONOFFOOTWEARACTIVATION, ABL_AUS_RETROFITSETOILINTERVALTOSA8KL: Change_FA")]
        public static bool ModifyFa(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa, List<string> faModList, bool addEntry)
        {
            if (fa == null)
            {
                return false;
            }

            foreach (string modEntry in faModList)
            {
                IList<string> faList = null;
                string item = modEntry.Trim();
                char prefix = item[0];
                string itemName = item.Substring(1);
                switch (prefix)
                {
                    case '-':
                        faList = fa.EWords;
                        break;

                    case '+':
                        faList = fa.HOWords;
                        break;

                    case '$':
                        faList = fa.Salapas;
                        break;
                }

                if (faList == null)
                {
                    return false;
                }

                if (addEntry)
                {
                    if (!faList.Contains(itemName))
                    {
                        faList.Add(itemName);
                    }
                }
                else
                {
                    if (faList.Contains(itemName))
                    {
                        faList.Remove(itemName);
                    }
                }
            }

            return true;
        }

        [PreserveSource(Hint = "From BMW.Rheingold.Programming.TherapyPlan.TherapyPlanModel.CompareFa")]
        public static string CompareFa(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa faCurrent, BMW.Rheingold.CoreFramework.Contracts.Programming.IFa faTarget)
        {
            if (faCurrent != null && faTarget != null)
            {
                List<string> faElementAdded = new List<string>();
                List<string> faElementRemoved = new List<string>();
                faElementAdded.Clear();
                faElementAdded.AddRange(from item in faTarget.EWords
                    where !faCurrent.EWords.Contains(item)
                    select item);
                faElementAdded.AddRange(from item in faTarget.HOWords
                    where !faCurrent.HOWords.Contains(item)
                    select item);
                faElementAdded.AddRange(from item in faTarget.Salapas
                    where !faCurrent.Salapas.Contains(item)
                    select item);
                faElementRemoved.Clear();
                faElementRemoved.AddRange(from item in faCurrent.EWords
                    where !faTarget.EWords.Contains(item)
                    select item);
                faElementRemoved.AddRange(from item in faCurrent.HOWords
                    where !faTarget.HOWords.Contains(item)
                    select item);
                faElementRemoved.AddRange(from item in faCurrent.Salapas
                    where !faTarget.Salapas.Contains(item)
                    select item);

                if (faElementAdded.Any() || faElementRemoved.Any())
                {
                    return string.Format(CultureInfo.InvariantCulture, "Added: {0}; Removed: {1}", faElementAdded.ToStringItems(), faElementRemoved.ToStringItems());
                }
            }

            return string.Empty;
        }

        [PreserveSource(Hint = "From CheckSoftwareAvailabilityBase")]
        public static IEnumerable<IPsdzSgbmId> RemoveCafdsCalculatedOnSCB(IEnumerable<string> cafdList, IEnumerable<IPsdzSgbmId> sweList)
        {
            List<string> list = cafdList.ToList();
            if (!list.Any())
            {
                return sweList;
            }
            IEnumerable<IPsdzSgbmId> enumerable2 = sweList.Where(x => !"CAFD".Equals(x.ProcessClass) && !list.Contains(x.Id));
            return enumerable2;
        }

        [PreserveSource(Hint = "From SecureCodingLogic")]
        public static bool CheckIfThereAreAnyNcdInTheRequest(RequestJson jsonContentObj)
        {
            EcuData[] ecuData = jsonContentObj?.calcEcuData?.ecuData;
            if (ecuData == null && jsonContentObj?.ecuData != null)
            {
                // [UH] [IGNORE] For backward compatibility with old request JSON format
                ecuData = jsonContentObj.ecuData;
            }

            if (ecuData != null)
            {
                foreach (EcuData data in ecuData)
                {
                    Log.Info(Log.CurrentMethod(), "Request for NCD Calculation created for Bltld: " + data.btld + " Cafd: " + string.Join("/", data.cafd.Select((string c) => c).ToArray()));
                }

                if (ecuData.Any())
                {
                    return true;
                }
            }
            return false;
        }

        [PreserveSource(Hint = "From SecureCodingLogic.GetCafdCalculatedInSCB")]
        public static IEnumerable<string> CafdCalculatedInSCB(RequestJson jsonContentObj)
        {
            EcuData[] ecuData = jsonContentObj?.calcEcuData?.ecuData;
            if (ecuData == null && jsonContentObj?.ecuData != null)
            {
                // [UH] [IGNORE] For backward compatibility with old request JSON format
                ecuData = jsonContentObj.ecuData;
            }
            if (ecuData != null)
            {
                return ecuData.SelectMany((EcuData a) => a.GetCafdId());
            }

            return new string[0];
        }

        [PreserveSource(Added = true)]
        public static List<IPsdzRequestNcdEto> CreateRequestNcdEtos(IPsdzCheckNcdResultEto psdzCheckNcdResultEto)
        {
            List<IPsdzRequestNcdEto> requestNcdEtos = new List<IPsdzRequestNcdEto>();
            psdzCheckNcdResultEto.DetailedNcdStatus.ForEach(delegate (IPsdzDetailedNcdInfoEto f)
            {
                requestNcdEtos.Add(new PsdzRequestNcdEto
                {
                    Btld = f.Btld,
                    Cafd = f.Cafd
                });
            });

            return requestNcdEtos;
        }

        [PreserveSource(Added = true)]
        public static TalExecutionSettings GetTalExecutionSettings(PsdzClient.Programming.ProgrammingService2 programmingService)
        {
            TalExecutionSettings talExecutionSettings = new TalExecutionSettings
            {
                Parallel = true,
                TaMaxRepeat = 1,
                UseFlaMode = true,
                UseProgrammingCounter = false,
                UseAep = false,
                ProgrammingModeSwitch = true,
                CodingModeSwitch = false,
                SecureCodingConfig = SecureCodingConfigWrapper.GetSecureCodingConfig(programmingService),
                EcusNotToSwitchProgrammingMode = null,
                EcusToPreventUdsFallback = null,
                ProgrammingProtectionDataCto = null,
                ProgrammingTokens = new List<IPsdzProgrammingTokenCto>(),
                IgnoreSignatureForProgrammingToken = true,
                ExpectedSgbmidValidationActive = false,
                ExpectedSgbmIdValidationForSmacTransferStartActive = false
            };
            return talExecutionSettings;
        }

        [PreserveSource(Added = true)]
        public static void LogTalExecutionSettings(string methodeName, TalExecutionSettings talExecutionSettings)
        {
            Log.Info(methodeName, "{0}: '{1}'", "Parallel", talExecutionSettings.Parallel);
            Log.Info(methodeName, "{0}: '{1}'", "TaMaxRepeat", talExecutionSettings.TaMaxRepeat);
            Log.Info(methodeName, "{0}: '{1}'", "UseFlaMode", talExecutionSettings.UseFlaMode);
            Log.Info(methodeName, "{0}: '{1}'", "UseProgrammingCounter", talExecutionSettings.UseProgrammingCounter);
            Log.Info(methodeName, "{0}: '{1}'", "UseAep", talExecutionSettings.UseAep);
            Log.Info(methodeName, "{0}: '{1}'", "ProgrammingModeSwitch", talExecutionSettings.ProgrammingModeSwitch);
            Log.Info(methodeName, "{0}: '{1}'", "CodingModeSwitch", talExecutionSettings.CodingModeSwitch);
            Log.Info(methodeName, "{0}: '{1}'", "HddUpdateURL", talExecutionSettings.HddUpdateURL);
            Log.Info(methodeName, "{0}: '{1}'", "IgnoreSignatureForProgrammingToken", talExecutionSettings.IgnoreSignatureForProgrammingToken);
            Log.Info(methodeName, "{0} is not null: '{1}'", "SecureCodingConfig", talExecutionSettings.SecureCodingConfig != null);
            Log.Info(methodeName, "{0} is not null: '{1}'", "EcusNotToSwitchProgrammingMode", talExecutionSettings.EcusNotToSwitchProgrammingMode != null);
            Log.Info(methodeName, "{0} is not null: '{1}'", "EcusToPreventUdsFallback", talExecutionSettings.EcusToPreventUdsFallback != null);
            Log.Info(methodeName, "{0} is not null: '{1}'", "ProgrammingProtectionDataCto", talExecutionSettings.ProgrammingProtectionDataCto != null);
            Log.Info(methodeName, "{0} is not null: '{1}'", "ProgrammingTokens", talExecutionSettings.ProgrammingTokens != null);
            SecureCodingConfigWrapper.LogSettings();
        }
    }
}
