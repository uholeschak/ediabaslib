using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.SecureCoding;
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
        [Flags]
        [AuthorAPI(SelectableTypeDeclaration = true)]
        public enum KmmFlashFlags
        {
            FlashNormal = 1,
            FlashCD = 2,
            FlashDVD = 4,
            FlashMOSTSync = 8,
            FlashMOSTAsync = 0x10,
            FlashMOSTControl = 0x20,
            FlashCAN = 0x40,
            FlashByteFlight = 0x80
        }

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

        // ProgrammingTaskFlags.Mount | ProgrammingTaskFlags.Unmount | ProgrammingTaskFlags.Replace | ProgrammingTaskFlags.Flash | Programming.ProgrammingTaskFlags.Code | ProgrammingTaskFlags.DataRecovery | ProgrammingTaskFlags.Fsc
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

        // Example call in: ABL_AUS_RETROFITPROTECTIONOFFOOTWEARACTIVATION, ABL_AUS_RETROFITSETOILINTERVALTOSA8KL: Change_FA
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

        // From BMW.Rheingold.Programming.TherapyPlan.TherapyPlanModel.CompareFa
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
                return ecuData.SelectMany(a => a.CafdId);
            }

            return new string[0];
        }

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
