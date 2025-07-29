using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BmwFileReader;
using HarmonyLib;
using log4net;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using PsdzClient.Core;
using PsdzClientLibrary;

namespace PsdzClient
{
    public partial class PsdzDatabase : IDisposable
    {
        public const string DiagObjServiceRoot = "DiagnosticObjectServicefunctionRoot";
        public const string AblFilter = "ABL";

        public const string SqlTitleItemsC =
            "C.TITLE_DEDE, C.TITLE_ENGB, C.TITLE_ENUS, " +
            "C.TITLE_FR, C.TITLE_TH, C.TITLE_SV, " +
            "C.TITLE_IT, C.TITLE_ES, C.TITLE_ID, " +
            "C.TITLE_KO, C.TITLE_EL, C.TITLE_TR, " +
            "C.TITLE_ZHCN, C.TITLE_RU, C.TITLE_NL, " +
            "C.TITLE_PT, C.TITLE_ZHTW, C.TITLE_JA, " +
            "C.TITLE_CSCZ, C.TITLE_PLPL";

        public const string SqlXmlItems =
            "XML_DEDE, XML_ENGB, XML_ENUS, " +
            "XML_FR, XML_TH, XML_SV, " +
            "XML_IT, XML_ES, XML_ID, " +
            "XML_KO, XML_EL, XML_TR, " +
            "XML_ZHCN, XML_RU, XML_NL, " +
            "XML_PT, XML_ZHTW, XML_JA, " +
            "XML_CSCZ, XML_PLPL";

        public const string DiagObjectItems =
            "ID, NODECLASS, TITLEID, " + DatabaseFunctions.SqlTitleItems + ", VERSIONNUMBER, NAME, FAILUREWEIGHT, VERSTECKT, SICHERHEITSRELEVANT, CONTROLID, SORT_ORDER";

        public enum SwiRegisterEnum
        {
            SoftwareUpdateExtended,
            SoftwareUpdateAdditionalSoftware,
            SoftwareUpdateComfort,
            EcuReplacementBeforeReplacement,
            EcuReplacementAfterReplacement,
            VehicleModification,
            VehicleModificationRetrofitting,
            VehicleModificationConversion,
            VehicleModificationCodingConversion,
            VehicleModificationBackConversion,
            VehicleModificationCodingBackConversion,
            Common,
            Immediatactions
        }

        public enum SwiRegisterGroup
        {
            None,
            Software,
            HwInstall,
            HwDeinstall,
            Modification,
            Other,
        }

        public enum SwiActionSource
        {
            VarId,
            VarGroupId,
            VarPrgEcuId,
            SwiRegister,
        }

        public enum BatteryEnum
        {
            Pb,
            LFP,
            PbNew
        }

        public enum SwiActionLinkType
        {
            SwiActionDiagnosticLink,
            PRF,
            MPB,
            MHV,
            MVF,
            MVS,
            MNS,
            MNF,
            MHN,
            AUS,
            TN,
            ESK_VA,
            ESK_VF,
            ESK_VS,
            ESK_MPB,
            ESK_PRF,
            SMP,
            HDD
        }

        public class EcuTranslation
        {
            public EcuTranslation()
            {
                TextDe = string.Empty;
                TextEn = string.Empty;
                TextUs = string.Empty;
                TextFr = string.Empty;
                TextTh = string.Empty;
                TextSv = string.Empty;
                TextIt = string.Empty;
                TextEs = string.Empty;
                TextId = string.Empty;
                TextKo = string.Empty;
                TextEl = string.Empty;
                TextTr = string.Empty;
                TextZh = string.Empty;
                TextRu = string.Empty;
                TextNl = string.Empty;
                TextPt = string.Empty;
                TextJa = string.Empty;
                TextCs = string.Empty;
                TextPl = string.Empty;
            }

            public EcuTranslation(string textDe, string textEnGb, string textEnUs, string textFr, string textTh, string textSv, string textIt,
                string textEs, string textId, string textKo, string textEl, string textTr, string textZh,
                string textRu, string textNl, string textPt, string textJa, string textCs, string textPl)
            {
                TextDe = textDe;
                TextEn = textEnGb;
                TextUs = textEnUs;
                TextFr = textFr;
                TextTh = textTh;
                TextSv = textSv;
                TextIt = textIt;
                TextEs = textEs;
                TextId = textId;
                TextKo = textKo;
                TextEl = textEl;
                TextTr = textTr;
                TextZh = textZh;
                TextRu = textRu;
                TextNl = textNl;
                TextPt = textPt;
                TextJa = textJa;
                TextCs = textCs;
                TextPl = textPl;
            }

            public string GetTitle(string language, string prefix = "Text")
            {
                try
                {
                    if (string.IsNullOrEmpty(language) || language.Length < 2)
                    {
                        return string.Empty;
                    }

                    string titlePropertyName = prefix + language.ToUpperInvariant()[0] + language.ToLowerInvariant()[1];
                    Type objType = GetType();
                    PropertyInfo propertyTitle = objType.GetProperty(titlePropertyName);
                    if (propertyTitle == null)
                    {
                        titlePropertyName = prefix + "En";
                        propertyTitle = objType.GetProperty(titlePropertyName);
                    }

                    if (propertyTitle != null)
                    {
                        string result = propertyTitle.GetValue(this) as string;
                        return result ?? string.Empty;
                    }

                    return string.Empty;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            public string GetTitle(ClientContext clientContext)
            {
                return GetTitle(clientContext.Language);
            }

            public static List<string> GetLanguages()
            {
                List<string> langList = new List<string>();
                PropertyInfo[] langueProperties = typeof(EcuTranslation).GetProperties();
                foreach (PropertyInfo propertyInfo in langueProperties)
                {
                    string name = propertyInfo.Name;
                    if (name.StartsWith("Text", StringComparison.OrdinalIgnoreCase))
                    {
                        langList.Add(name.Substring(4));
                    }
                }

                return langList;
            }

            public static string GetDbLanguage(string language)
            {
                string dbLanguage = language.ToUpperInvariant();
                switch (dbLanguage)
                {
                    case "ZH":
                        return "ZHCN";

                    case "DE":
                        return "DEDE";

                    case "CS":
                        return "CSCZ";

                    case "EN":
                        return "ENGB";

                    case "US":
                        return "ENUS";

                    case "PL":
                        return "PLPL";
                }

                return dbLanguage;
            }

            public string TextDe { get; set; }
            public string TextEn { get; set; }
            public string TextUs { get; set; }
            public string TextFr { get; set; }
            public string TextTh { get; set; }
            public string TextSv { get; set; }
            public string TextIt { get; set; }
            public string TextEs { get; set; }
            public string TextId { get; set; }
            public string TextKo { get; set; }
            public string TextEl { get; set; }
            public string TextTr { get; set; }
            public string TextZh { get; set; }
            public string TextRu { get; set; }
            public string TextNl { get; set; }
            public string TextPt { get; set; }
            public string TextJa { get; set; }
            public string TextCs { get; set; }
            public string TextPl { get; set; }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string description, string sgbd, string grp)
            {
                Name = name;
                Address = address;
                Description = description;
                DetectFailure = false;
                Sgbd = sgbd;
                Grp = grp;
                EcuVar = null;
                EcuPrgVars = null;
                PsdzEcu = null;
                SwiActions = new List<SwiAction>();
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Description { get; set; }

            public bool DetectFailure { get; set; }

            public string Sgbd { get; set; }

            public string Grp { get; set; }

            public EcuVar EcuVar { get; set; }

            public List<EcuPrgVar> EcuPrgVars { get; set; }

            public IPsdzEcu PsdzEcu { get; set; }

            public List<SwiAction> SwiActions { get; set; }

            public bool HasIndividualData
            {
                get
                {
                    if (PsdzEcu != null && PsdzEcu.EcuStatusInfo != null)
                    {
                        return PsdzEcu.EcuStatusInfo.HasIndividualData;
                    }
                    return false;
                }
            }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuInfo: Name={0}, Addr={1}, Sgdb={2}, Group={3}",
                    Name, Address, Sgbd, Grp));

                string prefixChild = prefix + " ";
                if (EcuVar != null)
                {
                    sb.AppendLine();
                    sb.Append(EcuVar.ToString(language, prefixChild));
                }

                if (EcuPrgVars != null)
                {
                    foreach (EcuPrgVar ecuPrgVar in EcuPrgVars)
                    {
                        sb.AppendLine();
                        sb.Append(ecuPrgVar.ToString(language, prefixChild));
                    }
                }

                if (PsdzEcu != null)
                {
                    sb.AppendLine();
                    sb.Append(prefixChild);
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Psdz: BaseVar={0}, Var={1}, Name={2}",
                        PsdzEcu.BaseVariant, PsdzEcu.EcuVariant, PsdzEcu.BnTnName));
                }

                if (SwiActions != null)
                {
                    foreach (SwiAction swiAction in SwiActions)
                    {
                        if (swiAction.HasInfoObjects)
                        {
                            string actionText = swiAction.ToString(language, prefixChild);
                            if (!string.IsNullOrEmpty(actionText))
                            {
                                sb.AppendLine();
                                sb.Append(actionText);
                            }
                        }
                    }
                }
                return sb.ToString();
            }
        }

        public class EcuVar
        {
            public EcuVar(string id, string faultMemDelWaitTime, string name, string validFrom, string validTo, string safetyRelevant, string ecuGroupId, string sort, EcuTranslation ecuTranslation)
            {
                Id = id;
                FaultMemDelWaitTime = faultMemDelWaitTime;
                Name = name;
                ValidFrom = validFrom;
                ValidTo = validTo;
                SafetyRelevant = safetyRelevant;
                EcuGroupId = ecuGroupId;
                Sort = sort;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string FaultMemDelWaitTime { get; set; }

            public string Name { get; set; }

            public string ValidFrom { get; set; }

            public string ValidTo { get; set; }

            public string SafetyRelevant { get; set; }

            public string EcuGroupId { get; set; }

            public string Sort { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, FaultTime={1}, Name={2}, ValidFrom={3}, ValidTo={4}, SafetyRel={5}, EcuGroupId={6}, Sort={7}, Title='{8}'",
                    Id, FaultMemDelWaitTime, Name, ValidFrom, ValidTo, SafetyRelevant, EcuGroupId, Sort, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class EcuPrgVar
        {
            public EcuPrgVar(string id, string name, string flashLimit, string ecuVarId)
            {
                Id = id;
                Name = name;
                FlashLimit = flashLimit;
                EcuVarId = ecuVarId;
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public string FlashLimit { get; set; }

            public string EcuVarId { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuPrgVar: Id={0}, Name={1}, FlashLimit={2}, EcuVarId={3}",
                    Id, Name, FlashLimit, EcuVarId));
                return sb.ToString();
            }
        }

        public class EcuGroup
        {
            public EcuGroup(string id, string obdIdent, string faultMemDelIdent, string faultMemDelWaitTime, string name, string virt, string safetyRelevant, string validFrom, string validTo, string diagAddr)
            {
                Id = id;
                ObdIdent = obdIdent;
                FaultMemDelIdent = faultMemDelIdent;
                FaultMemDelWaitTime = faultMemDelWaitTime;
                Name = name;
                Virt = virt;
                SafetyRelevant = safetyRelevant;
                ValidFrom = validFrom;
                ValidTo = validTo;
                DiagAddr = diagAddr;
            }

            public string Id { get; set; }

            public string ObdIdent { get; set; }

            public string FaultMemDelIdent { get; set; }

            public string FaultMemDelWaitTime { get; set; }

            public string Name { get; set; }

            public string Virt { get; set; }

            public string SafetyRelevant { get; set; }

            public string ValidFrom { get; set; }

            public string ValidTo { get; set; }

            public string DiagAddr { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuGroup: Id={0}, ObdId={1}, FaultMemDelIdent={2}, FaultMemDelWait={3}, Name={4}, Virt={5}, Safety={6}, ValidFrom={7}, ValidTo={8}, Addr={9}",
                    Id, ObdIdent, FaultMemDelIdent, FaultMemDelWaitTime, Name, Virt, SafetyRelevant, ValidFrom, ValidTo, DiagAddr));
                return sb.ToString();
            }
        }

        public class Equipment
        {
            public Equipment(string id, string name, EcuTranslation ecuTranslation)
            {
                Id = id;
                Name = name;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, Name={1}, Title='{2}'",
                    Id, Name, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class EcuClique
        {
            public EcuClique(string id, string cliqueName, string ecuRepId, EcuTranslation ecuTranslation)
            {
                Id = id;
                CliqueName = cliqueName;
                EcuRepId = ecuRepId;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string CliqueName { get; set; }

            public string EcuRepId { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, Name={1}, RepId={2}, Title='{3}'",
                    Id, CliqueName, EcuRepId, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class EcuRefClique
        {
            public EcuRefClique(string id, string ecuCliqueId)
            {
                Id = id;
                EcuCliqueId = ecuCliqueId;
            }

            public string Id { get; set; }

            public string EcuCliqueId { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, CliqueId={1}", Id, EcuCliqueId));
                return sb.ToString();
            }
        }

        public class ProductionDate
        {
            public ProductionDate(string year, string month)
            {
                Year = year;
                Month = month;
            }

            public string Year { get; set; }

            public string Month { get; set; }

            public long GetValue()
            {
                if (string.IsNullOrEmpty(Year) || string.IsNullOrEmpty(Month))
                {
                    return 0;
                }

                if (Year.Length != 4 || Month.Length != 2)
                {
                    return 0;
                }

                string date = Year + Month;
                return date.ConvertToInt();
            }
        }

        public class CharacteristicRoots
        {
            public CharacteristicRoots(string id, string nodeClass, string motorCycSeq, string vehicleSeq, EcuTranslation ecuTranslation)
            {
                Id = id;
                NodeClass = nodeClass;
                MotorCycSeq = motorCycSeq;
                VehicleSeq = vehicleSeq;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string NodeClass { get; set; }

            public string MotorCycSeq { get; set; }

            public string VehicleSeq { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, Class={1}, MotorSeq={2}, VehicleSeq={3}, Title='{4}'",
                    Id, NodeClass, MotorCycSeq, VehicleSeq, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class Characteristics
        {
            public Characteristics(string id, string nodeClass, string titleId, string istaVisible, string staticClassVar, string staticClassVarMCycle,
                string parentId, string name, string legacyName, EcuTranslation ecuTranslation)
            {
                Id = id;
                NodeClass = nodeClass;
                TitleId = titleId;
                IstaVisible = istaVisible;
                StaticClassVar = staticClassVar;
                StaticClassVarMCycle = staticClassVarMCycle;
                ParentId = parentId;
                Name = name;
                LegacyName = legacyName;
                DriveId = string.Empty;
                RootNodeClass = string.Empty;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string NodeClass { get; set; }

            public string TitleId { get; set; }

            public string IstaVisible { get; set; }

            public string StaticClassVar { get; set; }

            public string StaticClassVarMCycle { get; set; }

            public string ParentId { get; set; }

            public string Name { get; set; }

            public string LegacyName { get; set; }

            public string DriveId { get; set; }

            public string RootNodeClass { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "Char: Id={0}, Class={1}, TitleId={2}, ParentId={3}, Name={4}, LegacyName={5}, Drive={6}, RootClass={7}, Title='{8}'",
                    Id, NodeClass, TitleId, ParentId, Name, LegacyName, DriveId, RootNodeClass, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class VinRanges
        {
            public VinRanges(string changeDate, string productionMonth, string productionYear, string releaseState, string typeKey,
                string vinBandFrom, string vinBandTo, string gearboxType, string vin17_4_7)
            {
                ChangeDate = changeDate;
                ProductionMonth = productionMonth;
                ProductionYear = productionYear;
                ReleaseState = releaseState;
                TypeKey = typeKey;
                VinBandFrom = vinBandFrom;
                VinBandTo = vinBandTo;
                GearboxType = gearboxType;
                Vin17_4_7 = vin17_4_7;
            }

            public string ChangeDate { get; set; }

            public string ProductionMonth { get; set; }

            public string ProductionYear { get; set; }

            public string ReleaseState { get; set; }

            public string TypeKey { get; set; }

            public string VinBandFrom { get; set; }

            public string VinBandTo { get; set; }

            public string GearboxType { get; set; }

            public string Vin17_4_7 { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "VinRange: Date={0}, Month={1}, Year={2}, State={3}, Key={4}, VinFrom={5}, VinTo={6}, Gear={6}, Vin17_4_7={7}",
                    ChangeDate, ProductionMonth, ProductionYear, ReleaseState, TypeKey, VinBandFrom, VinBandTo, GearboxType, Vin17_4_7));
                return sb.ToString();
            }
        }

        public class SaLaPa
        {
            public SaLaPa(string id, string name, string productType, EcuTranslation ecuTranslation)
            {
                Id = id;
                Name = name;
                ProductType = productType;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public string ProductType { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, Name={1}, ProdTyp={2}, Title='{3}'",
                    Id, Name, ProductType, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class EcuReps
        {
            public EcuReps(string id, string ecuShortcut)
            {
                Id = id;
                EcuShortcut = ecuShortcut;
            }

            public string Id { get; set; }

            public string EcuShortcut { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, EcuShortcut={1}", Id, EcuShortcut));
                return sb.ToString();
            }
        }

        public class SwiAction
        {
            public SwiAction(SwiActionSource swiSource, string id, string name, string actionCategory, string selectable, string showInPlan, string executable, string nodeClass,
                EcuTranslation ecuTranslation)
            {
                SwiSource = swiSource;
                Id = id;
                Name = name;
                ActionCategory = actionCategory;
                Selectable = selectable;
                ShowInPlan = showInPlan;
                Executable = executable;
                NodeClass = nodeClass;
                EcuTranslation = ecuTranslation;
            }

            public SwiActionSource SwiSource { get; set; }

            public string Id { get; set; }

            public string Name { get; set; }

            public string ActionCategory { get; set; }

            public string Selectable { get; set; }

            public string ShowInPlan { get; set; }

            public string Executable { get; set; }

            public string NodeClass { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public List<SwiInfoObj> SwiInfoObjs { get; set; }

            public bool HasInfoObjects => SwiInfoObjs != null && SwiInfoObjs.Count > 0;

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiAction: Id={0}, Name={1}, Category={2}, Select={3}, Show={4}, Execute={5}, Title='{6}'",
                    Id, Name, ActionCategory, Selectable, ShowInPlan, Executable, EcuTranslation.GetTitle(language)));

                string prefixChild = prefix + " ";

                if (SwiInfoObjs != null)
                {
                    foreach (SwiInfoObj swiInfoObjLinked in SwiInfoObjs)
                    {
                        sb.AppendLine();
                        sb.Append(swiInfoObjLinked.ToString(language, prefixChild));
                    }
                }
                return sb.ToString();
            }
        }

        public class SwiRegister
        {
            public SwiRegister(string id, string nodeClass, string name, string parentId, string remark, string sort, string versionNum, string identifier,
                EcuTranslation ecuTranslation)
            {
                Id = id;
                NodeClass = nodeClass;
                Name = name;
                ParentId = parentId;
                Remark = remark;
                Sort = sort;
                VersionNum = versionNum;
                Identifier = identifier;
                EcuTranslation = ecuTranslation;
                Children = null;
                SwiActions = null;
            }

            public string Id { get; set; }

            public string NodeClass { get; set; }

            public string ParentId { get; set; }

            public string Name { get; set; }

            public string Remark { get; set; }

            public string Sort { get; set; }

            public string VersionNum { get; set; }

            public string Identifier { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public List<SwiRegister> Children { get; set; }

            public List<SwiAction> SwiActions { get; set; }

            public bool HasInfoObjects
            {
                get
                {
                    if (Children != null && Children.Any(x => x.HasInfoObjects))
                    {
                        return true;
                    }

                    if (SwiActions != null && SwiActions.Any(x => x.HasInfoObjects))
                    {
                        return true;
                    }

                    return false;
                }
            }

            public string ToString(string language, string prefix = "")
            {
                if (!HasInfoObjects)
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiReg: Name={0}, Remark={1}, Sort={2}, Ver={3}, Ident={4}, Title='{5}'",
                    Name, Remark, Sort, VersionNum, Identifier, EcuTranslation.GetTitle(language)));

                string prefixChild = prefix + " ";
                if (Children != null)
                {
                    foreach (SwiRegister swiChild in Children)
                    {
                        string childText = swiChild.ToString(language, prefixChild);
                        if (!string.IsNullOrEmpty(childText))
                        {
                            sb.AppendLine();
                            sb.Append(childText);
                        }
                    }
                }

                if (SwiActions != null)
                {
                    foreach (SwiAction swiAction in SwiActions)
                    {
                        if (swiAction.HasInfoObjects)
                        {
                            string actionText = swiAction.ToString(language, prefixChild);
                            if (!string.IsNullOrEmpty(actionText))
                            {
                                sb.AppendLine();
                                sb.Append(actionText);
                            }
                        }
                    }
                }
                return sb.ToString();
            }
        }

        public class SwiInfoObj
        {
            public SwiInfoObj(SwiActionDatabaseLinkType? linkType, SwiActionLinkType? mappedLinkType, string id, string nodeClass, string assembly, string versionNum, string programType, string safetyRelevant,
                string titleId, string general, string telSrvId, string vehicleComm, string measurement, string hidden, string name, string informationType,
                string identification, string informationFormat, string siNumber, string targetILevel, string controlId,
                string infoType, string infoFormat, string docNum, string priority, string identifier, string flowXml, EcuTranslation ecuTranslation)
            {
                LinkType = linkType;
                MappedTypeLink = mappedLinkType;
                Id = id;
                NodeClass = nodeClass;
                Assembly = assembly;
                VersionNum = versionNum;
                ProgramType = programType;
                SafetyRelevant = safetyRelevant;
                TitleId = titleId;
                General = general;
                TelSrvId = telSrvId;
                VehicleComm = vehicleComm;
                Measurement = measurement;
                Hidden = hidden;
                Name = name;
                InformationType = informationType;
                Identification = identification;
                InformationFormat = informationFormat;
                SiNumber = siNumber;
                TargetILevel = targetILevel;
                ControlId = controlId;
                InfoType = infoType;
                InfoFormat = infoFormat;
                DocNum = docNum;
                Priority = priority;
                Identifier = identifier;
                FlowXml = flowXml;
                EcuTranslation = ecuTranslation;
                DiagObjectPath = null;
            }

            public enum SwiActionDatabaseLinkType
            {
                SwiActionCheckLink,
                SwiActionActionSelectionLink,
                SwiActionVehiclePostprocessingLink,
                SwiActionDiagnosticLink,
                SwiActionEcuPostprocessingLink,
                SwiActionVehiclePreparingLink,
                SwiActionActionPlanLink,
                SwiActionPreparingHintsLink,
                SwiActionEcuPreparingLink,
                SwiActionPostprocessingHintsLink,
                SwiactionEscalationPreparingGeneralLink,
                SwiactionEscalationPreparingVehicleLink,
                SwiactionEscalationPreparingEcuLink,
                SwiactionEscalationActionplanCalculationLink,
                SwiactionEscalationPreconditionCheckLink,
                SwiactionSpecialActionplanLink
            }

            public SwiActionDatabaseLinkType? LinkType { get; set; }

            public SwiActionLinkType? MappedTypeLink { get; set; }

            public string Id { get; set; }

            public string NodeClass { get; set; }

            public string Assembly { get; set; }

            public string VersionNum { get; set; }

            public string ProgramType { get; set; }

            public string SafetyRelevant { get; set; }

            public string TitleId { get; set; }

            public string General { get; set; }

            public string TelSrvId { get; set; }

            public string VehicleComm { get; set; }

            public string Measurement { get; set; }

            public string Hidden { get; set; }

            public string Name { get; set; }

            public string InformationType { get; set; }

            public string Identification { get; set; }

            public string InformationFormat { get; set; }

            public string SiNumber { get; set; }

            public string TargetILevel { get; set; }

            public string ControlId { get; set; }

            public string InfoType { get; set; }

            public string InfoFormat { get; set; }

            public string DocNum { get; set; }

            public string Priority { get; set; }

            public string Identifier { get; set; }

            public string FlowXml { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public List<SwiDiagObj> DiagObjectPath { get; set; }

            public string ModuleName => Identifier.Replace("-", "_");

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiInfoObj: LinkType={0}, Id={1}, Class={2}, PrgType={3}, InformationType={4}, Identification={5}, ILevel={6}, InfoType={7}, Identifier={8}, Flow={9}, Title='{10}'",
                    LinkType, Id, NodeClass, ProgramType, InformationType, Identification, TargetILevel, InfoType, Identifier, FlowXml, EcuTranslation.GetTitle(language)));
#if false
                if (!string.IsNullOrEmpty(FlowXml))
                {
                    string flowData = ClientContext.Database.GetFlowForInfoObj(this);
                    if (!string.IsNullOrEmpty(flowData))
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "SwiInfoObj: Flow='{0}'", flowData));
                    }
                }
#endif
                return sb.ToString();
            }

            public static SwiActionDatabaseLinkType? GetLinkType(string linkTypeId)
            {
                if (Enum.TryParse<SwiActionDatabaseLinkType>(linkTypeId, true, out SwiActionDatabaseLinkType swiActionDatabaseLinkType))
                {
                    return swiActionDatabaseLinkType;
                }

                return null;
            }
        }

        public class SwiDiagObj
        {
            public SwiDiagObj(string id, string nodeClass,
                string titleId, string versionNum, string name, string failWeight, string hidden,
                string safetyRelevant, string controlId, string sortOrder, EcuTranslation ecuTranslation)
            {
                Id = id;
                NodeClass = nodeClass;
                TitleId = titleId;
                VersionNum = versionNum;
                Name = name;
                FailWeight = failWeight;
                Hidden = hidden;
                SafetyRelevant = safetyRelevant;
                ControlId = controlId;
                SortOrder = sortOrder;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string NodeClass { get; set; }

            public string TitleId { get; set; }

            public string VersionNum { get; set; }

            public string Name { get; set; }

            public string FailWeight { get; set; }

            public string Hidden { get; set; }

            public string SafetyRelevant { get; set; }

            public string Identifier { get; set; }

            public string ControlId { get; set; }

            public string SortOrder { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public List<SwiInfoObj> InfoObjects { get; set; }

            public List<SwiDiagObj> Children { get; set; }

            public int InfoObjectsCount
            {
                get
                {
                    int infoObjectCount = 0;
                    if (Children != null)
                    {
                        foreach (SwiDiagObj swiDiagObj in Children)
                        {
                            infoObjectCount += swiDiagObj.InfoObjectsCount;
                        }
                    }

                    if (InfoObjects != null)
                    {
                        infoObjectCount += InfoObjects.Count;
                    }

                    return infoObjectCount;
                }
            }

            public List<SwiInfoObj> CompleteInfoObjects
            {
                get
                {
                    List<SwiInfoObj> completeInfoObjects = new List<SwiInfoObj>();
                    if (InfoObjects != null)
                    {
                        foreach (SwiInfoObj infoObject in InfoObjects)
                        {
                            infoObject.DiagObjectPath = new List<SwiDiagObj> { this };
                            completeInfoObjects.Add(infoObject);
                        }
                    }

                    if (Children != null)
                    {
                        foreach (SwiDiagObj swiDiagObj in Children)
                        {
                            List<SwiInfoObj> infoObjectChildren = swiDiagObj.CompleteInfoObjects;
                            foreach (SwiInfoObj infoObject in infoObjectChildren)
                            {
                                if (infoObject.DiagObjectPath != null)
                                {
                                    infoObject.DiagObjectPath.Insert(0, this);
                                }

                                completeInfoObjects.Add(infoObject);
                            }
                        }
                    }

                    return completeInfoObjects;
                }
            }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiDiagObj: Id={0}, Class={1}, TitleId={2}, Name={3}, Identification={4}, ControlId={5}, Title='{6}'",
                    Id, NodeClass, TitleId, Name, Identifier, ControlId, EcuTranslation.GetTitle(language)));

                string prefixChild = prefix + " ";
                if (InfoObjects != null)
                {
                    foreach (SwiInfoObj infoObj in InfoObjects)
                    {
                        sb.AppendLine();
                        sb.Append(infoObj.ToString(language, prefixChild));
                    }
                }

                if (Children != null && InfoObjectsCount > 0)
                {
                    foreach (SwiDiagObj childObj in Children)
                    {
                        if (childObj.InfoObjectsCount > 0)
                        {
                            sb.AppendLine();
                            sb.Append(childObj.ToString(language, prefixChild));
                        }
                    }
                }
                return sb.ToString();
            }
        }

        public class XepRule
        {
            public XepRule(string id, byte[] rule)
            {
                Id = id;
                Rule = rule;
                Reset();
            }

            public string Id { get; set; }

            public byte[] Rule { get; }

            public RuleExpression RuleExpression { get; private set; }

            public bool? RuleResult { get; private set; }

            public void Reset()
            {
                RuleExpression = null;
                ResetResult();
            }
            public void ResetResult()
            {
                RuleResult = null;
            }

            public bool EvaluateRule(Vehicle vehicle, IFFMDynamicResolver ffmResolver)
            {
                log.InfoFormat("EvaluateRule Id: {0}", Id);
                if (vehicle == null)
                {
                    log.ErrorFormat("EvaluateRule No vehicle");
                    return false;
                }

                if (RuleExpression == null)
                {
                    try
                    {
                        log.InfoFormat("EvaluateRule Create expression");
                        RuleExpression = RuleExpression.Deserialize(new MemoryStream(Rule), new NugetLogger(), vehicle);
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("EvaluateRule Exception: '{0}'", e.Message);
                    }
                }

                log.InfoFormat("EvaluateRule Expression: '{0}'", RuleExpression);
                if (!RuleResult.HasValue)
                {
                    try
                    {
                        RuleResult = RuleExpression.Evaluate(vehicle, RuleExpression, ffmResolver);
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("EvaluateRule Exception: '{0}'", e.Message);
                        return false;
                    }
                }
                log.InfoFormat("EvaluateRule Result: {0}", RuleResult.Value);
                return RuleResult.Value;
            }

            public string GetRuleString(Vehicle vehicle)
            {
                if (vehicle == null)
                {
                    log.ErrorFormat("GetRuleString No vehicle");
                    return null;
                }

                try
                {
                    RuleExpression ruleExpression = RuleExpression.Deserialize(new MemoryStream(Rule), new NugetLogger(), vehicle);
                    return ruleExpression.ToString();
                }
                catch (Exception e)
                {
                    log.ErrorFormat("GetRuleString Exception: '{0}'", e.Message);
                }

                return null;
            }

            public string GetRuleFormula(Vehicle vehicle, RuleExpression.FormulaConfig formulaConfig = null, List<string> subRuleIds = null)
            {
                if (vehicle == null)
                {
                    log.ErrorFormat("GetRuleString No vehicle");
                    return null;
                }

                try
                {
                    RuleExpression ruleExpression = RuleExpression.Deserialize(new MemoryStream(Rule), new NugetLogger(), vehicle);
                    RuleExpression.FormulaConfig formulaConfigCurrent = formulaConfig;
                    if (formulaConfigCurrent == null)
                    {
                        formulaConfigCurrent = new RuleExpression.FormulaConfig("RuleString", "RuleNum", "IsValidRuleString", "IsValidRuleNum", "IsFaultRuleValid", true, subRuleIds);
                    }
                    return ruleExpression.ToFormula(formulaConfigCurrent);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("GetRuleString Exception: '{0}'", e.Message);
                }

                return null;
            }

            public string ToString(string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                string ruleResult = RuleResult != null ? RuleResult.Value.ToString() : "-";
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "XepRule: Id={0}, Result={1}, Rule='{2}'", Id, ruleResult, RuleExpression.ToString()));
                return sb.ToString();
            }
        }

        public class BordnetsData
        {
            public BordnetsData(string infoObjId, string infoObjIdent, string docId)
            {
                InfoObjId = infoObjId;
                InfoObjIdent = infoObjIdent;
                DocId = docId;
                DocData = null;
                XepRule = null;
            }

            public string InfoObjId { get; set; }

            public string InfoObjIdent { get; set; }

            public string DocId { get; set; }

            public string DocData { get; set; }

            public XepRule XepRule { get; set; }

            public string ToString(string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "BordnetsData: InfoId={0}, InfoIdent={1}, DocId={2}", InfoObjId, InfoObjIdent, DocId));
                return sb.ToString();
            }
        }

        public class DbInfo
        {
            public DbInfo(string version, DateTime dateTime)
            {
                Version = version;
                DateTime = dateTime;
            }

            public string Version { get; set; }

            public DateTime DateTime { get; set; }

            public string ToString(string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "DbInfo: Version={0}, Date={1}", Version, DateTime));
                return sb.ToString();
            }
        }

        public class VinRangeQuerySettings
        {
            public string ProductionYearColumnName { get; }

            public string ProductionMonthColumnName { get; }

            public string TableName { get; }

            public VinRangeQuerySettings(bool isPrerelease)
            {
                ProductionMonthColumnName = (isPrerelease ? "PACKAGINGDATEMONTH" : "PRODUCTIONDATEMONTH");
                ProductionYearColumnName = (isPrerelease ? "PACKAGINGDATEYEAR" : "PRODUCTIONDATEYEAR");
                TableName = (isPrerelease ? "VINRANGES_CKDALL" : "VINRANGES");
            }
        }

        private const string TestModulesXmlFile = "TestModules.xml";
        private const string TestModulesZipFile = "TestModules.zip";
        private const string ServiceModulesXmlFile = "ServiceModules.xml";
        private const string ServiceModulesZipFile = "ServiceModules.zip";
        private const string EcuCharacteristicsXmFile = "EcuCharacteristics.xml";
        private const string EcuCharacteristicsZipFile = "EcuCharacteristics.zip";
        private const string ConfigurationContainerXMLPar = "ConfigurationContainerXML";
        private static readonly ILog log = LogManager.GetLogger(typeof(PsdzDatabase));

        // ToDo: Check on update
        public static List<decimal> EngineRootNodeClasses = new List<decimal>
        {
            40141570m, 40142338m, 40142722m, 40143106m, 40145794m, 99999999866m, 99999999868m, 99999999870m, 99999999872m, 99999999874m,
            99999999876m, 99999999878m, 99999999880m, 99999999909m, 99999999910m, 99999999918m, 99999999701m, 99999999702m, 99999999703m, 99999999704m,
            99999999705m, 99999999706m, 99999999707m, 99999999708m
        };

        private static List<string> engineRootNodeClasses = EngineRootNodeClasses.ConvertAll<string>(d => d.ToString());

        private bool _disposed;
        private string _databasePath;
        private string _databaseExtractPath;
        private string _testModulePath;
        private string _frameworkPath;
        private Harmony _harmony;
        private SqliteConnection _mDbConnection;
        private string _rootENameClassId;
        private string _typeKeyClassId;
        private string _tableForFTSSearch = string.Empty;
        private bool? _doesXMLValuePrimitiveTableHaveFTS;
        private Dictionary<string, XepRule> _xepRuleDict;
        private List<SwiDiagObj> _diagObjRootNodes;
        private HashSet<string> _diagObjRootNodeIdSet;
        public Dictionary<string, XepRule> XepRuleDict => _xepRuleDict;
        public SwiRegister SwiRegisterTree { get; private set; }
        public TestModules TestModuleStorage { get; private set; }
        public EcuCharacteristicsData EcuCharacteristicsStorage { get; private set; }
        public bool UseIsAtLeastOnePathToRootValid { get; set; }
        public static bool RestartRequired { get; private set; }

        public PsdzDatabase(string istaFolder)
        {
            if (!Directory.Exists(istaFolder))
            {
                log.ErrorFormat("PsdzDatabase: ISTA path not existing: {0}", istaFolder);
                throw new Exception(string.Format("ISTA path not existing: {0}", istaFolder));
            }

            string databaseBasePath = ConfigSettings.getPathString("BMW.Rheingold.DatabaseProvider.SQLiteConnector.DbBasePath", null);
            if (!string.IsNullOrEmpty(databaseBasePath) && Directory.Exists(databaseBasePath))
            {
                _databasePath = databaseBasePath;
            }
            else
            {
                _databasePath = Path.Combine(istaFolder, "SQLiteDBs");
            }

            if (!Directory.Exists(_databasePath))
            {
                log.ErrorFormat("PsdzDatabase: ISTA database path not existing: {0}", _databasePath);
                throw new Exception(string.Format("ISTA database path not existing: {0}", _databasePath));
            }

            _databaseExtractPath = Path.Combine(_databasePath, "Extract");
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            bool adminRequired = false;
            if (PathStartWith(_databaseExtractPath, programFiles))
            {
                adminRequired = true;
            }
            else if (PathStartWith(_databaseExtractPath, programFilesX86))
            {
                adminRequired = true;
            }

            if (adminRequired)
            {
                log.ErrorFormat("PsdzDatabase: Path rejected, requires admin rights: {0}", _databaseExtractPath);
                _databaseExtractPath = null;
            }

            if (!string.IsNullOrEmpty(_databaseExtractPath) && !Directory.Exists(_databaseExtractPath))
            {
                try
                {
                    Directory.CreateDirectory(_databaseExtractPath);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("PsdzDatabase CreateDirectory Exception: {0}, Path: {1}", e.Message, _databaseExtractPath);
                    _databaseExtractPath = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(_databaseExtractPath))
            {
                _databaseExtractPath = ConfigSettings.getPathString("BMW.Rheingold.Programming.PsdzExtractDataPath", "%ISPIDATA%\\BMW\\ISPI\\data\\TRIC\\ISTA\\Extract\\");
                try
                {
                    Directory.CreateDirectory(_databaseExtractPath);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("PsdzDatabase CreateDirectory Exception: {0}, Path: {1}", e.Message, _databaseExtractPath);
                    _databaseExtractPath = string.Empty;
                }
            }

            _testModulePath = Path.Combine(istaFolder, "Testmodule");
            if (!Directory.Exists(_testModulePath))
            {
                log.ErrorFormat("PsdzDatabase: ISTA testmodule path not existing: {0}", _testModulePath);
                throw new Exception(string.Format("ISTA testmodule path not existing: {0}", _testModulePath));
            }

            _frameworkPath = Path.Combine(istaFolder, "TesterGUI", "bin","ReleaseMod");
            if (!Directory.Exists(_frameworkPath))
            {
                _frameworkPath = Path.Combine(istaFolder, "TesterGUI", "bin", "Release");
            }

            if (!Directory.Exists(_frameworkPath))
            {
                log.ErrorFormat("PsdzDatabase: ISTA framework path not existing: {0}", _frameworkPath);
                throw new Exception(string.Format("ISTA framework path not existing: {0}", _frameworkPath));
            }

            log.InfoFormat("PsdzDatabase: ISTA framework path: {0}", _frameworkPath);

            _harmony = new Harmony("de.holeschak.PsdzClient");
            if (!SqlLoader.PatchLoader(_harmony))
            {
                log.ErrorFormat("PsdzDatabase: PatchLoader failed");
            }

            string databaseFile = Path.Combine(_databasePath, "DiagDocDb.sqlite");
            if (!File.Exists(databaseFile))
            {
                log.ErrorFormat("PsdzDatabase: Database file not existing: {0}", databaseFile);
                throw new Exception(string.Format("Database file not existing: {0}", databaseFile));
            }

            string hexKey = BitConverter.ToString(Encoding.ASCII.GetBytes(DatabaseFunctions.DatabasePassword)).Replace("-", "");
            SqliteConnectionStringBuilder sqliteConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = "file:" + databaseFile + "?cipher=rc4&hexkey=" + hexKey,
                Mode = SqliteOpenMode.ReadOnly,
            };

            _mDbConnection = new SqliteConnection(sqliteConnectionString.ConnectionString);
            _mDbConnection.Open();

            _rootENameClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"RootEBezeichnung");
            _typeKeyClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"Typschluessel");

            _xepRuleDict = null;
            _diagObjRootNodes = null;
            _diagObjRootNodeIdSet = null;
            SwiRegisterTree = null;
            UseIsAtLeastOnePathToRootValid = true;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string assemblyPath = Path.Combine(_frameworkPath, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(assemblyPath))
                {
                    return null;
                }
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if ((Exception)args.ExceptionObject is Exception e)
                {
                    log.ErrorFormat("UnhandledException: {0}", e.Message);
                }
            };
        }

        // ToDo: Check on update
        // SwiRegister -> SwiRegisterEnum, vehicle added
        private string SwiRegisterEnumerationNameConverter(SwiRegisterEnum swiRegister, Vehicle vehicle)
        {
            string arg;
            switch (swiRegister)
            {
                case SwiRegisterEnum.SoftwareUpdateExtended:
                    arg = "ERWEITERT";
                    break;
                case SwiRegisterEnum.SoftwareUpdateAdditionalSoftware:
                    arg = "ZUSATZSOFTWARE";
                    break;
                case SwiRegisterEnum.SoftwareUpdateComfort:
                    arg = "KOMFORT";
                    break;
                case SwiRegisterEnum.EcuReplacementBeforeReplacement:
                    arg = "VOR_DEM_TAUSCH";
                    break;
                case SwiRegisterEnum.EcuReplacementAfterReplacement:
                    arg = "NACH_DEM_TAUSCH";
                    break;
                case SwiRegisterEnum.VehicleModification:
                    arg = "FAHRZEUG-MODIFIKATION";
                    break;
                case SwiRegisterEnum.VehicleModificationRetrofitting:
                    arg = "NACHRUESTUNGEN";
                    break;
                case SwiRegisterEnum.VehicleModificationConversion:
                    arg = "UMRUESTUNGEN";
                    break;
                case SwiRegisterEnum.VehicleModificationCodingConversion:
                    arg = "CODIERUMRUESTUNGEN";
                    break;
                case SwiRegisterEnum.VehicleModificationBackConversion:
                    arg = "RUECKRUESTUNGEN";
                    break;
                case SwiRegisterEnum.VehicleModificationCodingBackConversion:
                    arg = "CODIERRUECKRUESTUNGEN";
                    break;
                case SwiRegisterEnum.Common:
                    arg = "ALLGEMEIN";
                    break;
                case SwiRegisterEnum.Immediatactions:
                    arg = GetImmediatactionsIdentifier(vehicle);
                    break;
                default:
                    throw new ArgumentException("Unknown SWI Register!");
            }
            return $"REG|{arg}";
        }

        private string GetImmediatactionsIdentifier(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Classification.IsMotorcycle())
            {
                return "SOFORTMASSNAHMEN_PROGRAMMIERUNG_MOTORRAD";
            }

            return "SOFORTMASSNAHMEN_PROGRAMMIERUNG_PKW";
        }


        public static SwiRegisterGroup GetSwiRegisterGroup(SwiRegisterEnum swiRegisterEnum)
        {
            switch (swiRegisterEnum)
            {
                case SwiRegisterEnum.SoftwareUpdateExtended:
                case SwiRegisterEnum.SoftwareUpdateAdditionalSoftware:
                case SwiRegisterEnum.SoftwareUpdateComfort:
                    return SwiRegisterGroup.Software;

                case SwiRegisterEnum.EcuReplacementBeforeReplacement:
                    return SwiRegisterGroup.HwDeinstall;

                case SwiRegisterEnum.EcuReplacementAfterReplacement:
                    return SwiRegisterGroup.HwInstall;

                case SwiRegisterEnum.VehicleModification:
                case SwiRegisterEnum.VehicleModificationRetrofitting:
                case SwiRegisterEnum.VehicleModificationConversion:
                case SwiRegisterEnum.VehicleModificationCodingConversion:
                case SwiRegisterEnum.VehicleModificationBackConversion:
                case SwiRegisterEnum.VehicleModificationCodingBackConversion:
                    return SwiRegisterGroup.Modification;

                case SwiRegisterEnum.Common:
                case SwiRegisterEnum.Immediatactions:
                    return SwiRegisterGroup.Other;
            }

            return SwiRegisterGroup.None;
        }

        public void ResetXepRules()
        {
            if (_xepRuleDict == null)
            {
                log.InfoFormat("ResetXepRules No rules present");
                return;
            }

            foreach (KeyValuePair<string, XepRule> keyValuePair in _xepRuleDict)
            {
                keyValuePair.Value?.Reset();
            }
        }

        public bool LinkSvtEcus(List<EcuInfo> ecuList, IPsdzSvt psdzSvt)
        {
            try
            {
                foreach (EcuInfo ecuInfo in ecuList)
                {
                    IPsdzEcu psdzEcuMatch = null; 
                    if (ecuInfo.Address >= 0)
                    {
                        foreach (IPsdzEcu psdzEcu in psdzSvt.Ecus)
                        {
                            if (psdzEcu.PrimaryKey.DiagAddrAsInt == ecuInfo.Address)
                            {
                                psdzEcuMatch = psdzEcu;
                                break;
                            }
                        }
                    }
                    ecuInfo.PsdzEcu = psdzEcuMatch;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("LinkSvtEcus Exception: '{0}'", e.Message);
                return false;
            }

            return true;
        }

        public List<LocalizedText> GetTextCollectionById(string idInfoObject, IList<string> lang = null)
        {
            log.InfoFormat("GetTextCollectionById Id: {0}", idInfoObject);

            if (string.IsNullOrEmpty(idInfoObject))
            {
                log.ErrorFormat("GetTextCollectionById No Info Object ID");
                return null;
            }

            List<LocalizedText> textList = new List<LocalizedText>();
            try
            {
                EcuTranslation xmlTranslation = null;
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, INFOOBJECT_ID, " + SqlXmlItems + " FROM XEP_REFSPTEXTCOLL WHERE (INFOOBJECT_ID = '{0}')", idInfoObject);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            xmlTranslation = GetTranslation(reader, "XML");
                            if (xmlTranslation != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (xmlTranslation == null)
                {
                    log.ErrorFormat("GetTextCollectionById No translations");
                    return null;
                }

                List<string> languages = EcuTranslation.GetLanguages();
                foreach (string language in languages)
                {
                    string langName = string.Empty;
                    if (lang != null)
                    {
                        foreach (string requestLang in lang)
                        {
                            if (language.StartsWith(requestLang, StringComparison.OrdinalIgnoreCase))
                            {
                                langName = requestLang;
                                break;
                            }
                        }
                    }
                    else
                    {
                        langName = language.ToUpperInvariant();
                    }

                    if (string.IsNullOrEmpty(langName))
                    {
                        continue;
                    }

                    string xmlId = xmlTranslation.GetTitle(language);
                    if (!string.IsNullOrEmpty(xmlId))
                    {
                        string xmlData = GetXmlValuePrimitivesById(xmlId, EcuTranslation.GetDbLanguage(language));
                        if (!string.IsNullOrEmpty(xmlData))
                        {
                            textList.Add(new LocalizedText(xmlData, langName));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTextCollectionById Exception: '{0}'", e.Message);
                return null;
            }

            return textList;
        }

        public List<LocalizedText> GetTextById(string id, IList<string> lang = null)
        {
            log.InfoFormat("GetTextById Id: {0}", id);

            if (string.IsNullOrEmpty(id))
            {
                log.ErrorFormat("GetTextById No ID");
                return null;
            }

            List<LocalizedText> textList = new List<LocalizedText>();
            try
            {
                EcuTranslation xmlTranslation = null;
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, XMLID, " + SqlXmlItems + " FROM XEP_SPINTTEXTITEMS WHERE (ID = {0})", id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            xmlTranslation = GetTranslation(reader, "XML");
                            if (xmlTranslation != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (xmlTranslation == null)
                {
                    log.ErrorFormat("GetTextById No translations");
                    return null;
                }

                List<string> languages = EcuTranslation.GetLanguages();
                foreach (string language in languages)
                {
                    string langName = string.Empty;
                    if (lang != null)
                    {
                        foreach (string requestLang in lang)
                        {
                            if (language.StartsWith(requestLang, StringComparison.OrdinalIgnoreCase))
                            {
                                langName = requestLang;
                                break;
                            }
                        }
                    }
                    else
                    {
                        langName = language.ToUpperInvariant();
                    }

                    if (string.IsNullOrEmpty(langName))
                    {
                        continue;
                    }

                    string xmlId = xmlTranslation.GetTitle(language);
                    if (!string.IsNullOrEmpty(xmlId))
                    {
                        string xmlData = GetXmlValuePrimitivesById(xmlId, EcuTranslation.GetDbLanguage(language));
                        if (!string.IsNullOrEmpty(xmlData))
                        {
                            textList.Add(new LocalizedText(xmlData, langName));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTextById Exception: '{0}'", e.Message);
                return null;
            }

            return textList;
        }

        public EcuTranslation GetSpTextItemsByControlId(string controlId)
        {
            log.InfoFormat("GetSpTextItemsByControlId Id: {0}", controlId);

            if (string.IsNullOrEmpty(controlId))
            {
                log.ErrorFormat("GetSpTextItemsByControlId No control ID");
                return null;
            }

            EcuTranslation xmlTranslation = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, XMLID, " + SqlXmlItems + " FROM XEP_SPTEXTITEMS WHERE (CONTROLID = {0})", controlId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            xmlTranslation = GetTranslation(reader, "XML");
                            if (xmlTranslation != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (xmlTranslation == null)
                {
                    log.ErrorFormat("GetSpTextItemsByControlId No translations");
                    return null;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTextCollectionById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetSpTextItemsByControlId OK");
            return xmlTranslation;
        }

        public string GetXmlValuePrimitivesById(string id, string languageExtension)
        {
            log.InfoFormat("GetXmlValuePrimitivesById Id: {0}, Lang: {1}", id, languageExtension);

            string data = GetXmlValuePrimitivesByIdSingle(id, languageExtension);
            if (string.IsNullOrEmpty(data) && string.Compare(languageExtension, "ENGB", StringComparison.OrdinalIgnoreCase) != 0)
            {
                data = GetXmlValuePrimitivesByIdSingle(id, "ENGB");
            }

            if (string.IsNullOrEmpty(data))
            {
                data = GetXmlValuePrimitivesByIdSingle(id, "OTHER");
            }

            if (string.IsNullOrEmpty(data))
            {
                data = string.Empty;
            }

            log.InfoFormat("GetXmlValuePrimitivesById OK");
            return data;
        }

        public string GetXmlValuePrimitivesByIdSingle(string id, string languageExtension)
        {
            log.InfoFormat("GetXmlValuePrimitivesByIdSingle Id: {0}, Lang: {1}", id, languageExtension);
            if (string.IsNullOrWhiteSpace(languageExtension))
            {
                log.ErrorFormat("GetXmlValuePrimitivesByIdSingle Language missing");
                return null;
            }

            string data = null;
            try
            {
                string databaseName = @"xmlvalueprimitive_" + languageExtension + @".sqlite";
                string databaseFile = Path.Combine(_databasePath, databaseName);
                if (!File.Exists(databaseFile))
                {
                    log.ErrorFormat("GetXmlValuePrimitivesByIdSingle File not found: {0}", databaseFile);
                    return null;
                }

                string comparator = this.DoesXMLValuePrimitiveTableHaveFTS ? "Match" : "=";
                SqliteConnectionStringBuilder sqliteConnectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = "file:" + databaseFile,
                    Mode = SqliteOpenMode.ReadOnly,
                };

                using (SqliteConnection mDbConnection = new SqliteConnection(sqliteConnectionString.ConnectionString))
                {
                    mDbConnection.Open();
                    string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, DATA FROM XMLVALUEPRIMITIVE WHERE (ID {0} '{1}')", comparator, id);
                    using (SqliteCommand command = mDbConnection.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data = reader["DATA"].ToString();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetXmlValuePrimitivesByIdSingle Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetXmlValuePrimitivesByIdSingle OK");
            return data;
        }

        private string TableForFTSSearch
        {
            get
            {
                if (string.IsNullOrEmpty(this._tableForFTSSearch))
                {
                    this._tableForFTSSearch = this.GetTableWithFTSModule();
                }
                return this._tableForFTSSearch;
            }
        }

        private bool DoesXMLValuePrimitiveTableHaveFTS
        {
            get
            {
                bool? flag = this._doesXMLValuePrimitiveTableHaveFTS;
                if (flag == null)
                {
                    bool? flag2 = (this._doesXMLValuePrimitiveTableHaveFTS = new bool?(this.TableForFTSSearch.Equals("xmlvalueprimitive", StringComparison.OrdinalIgnoreCase)));
                    return flag2.Value;
                }
                return flag.GetValueOrDefault();
            }
        }

        private string GetTableWithFTSModule()
        {
            try
            {
                string databaseName = @"xmlvalueprimitive_ENGB.sqlite";
                string databaseFile = Path.Combine(_databasePath, databaseName);
                if (!File.Exists(databaseFile))
                {
                    log.ErrorFormat("GetTableWithFTSModule File not found: {0}", databaseFile);
                    return null;
                }

                SqliteConnectionStringBuilder sqliteConnectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = "file:" + databaseFile,
                    Mode = SqliteOpenMode.ReadOnly,
                };

                using (SqliteConnection mDbConnection = new SqliteConnection(sqliteConnectionString.ConnectionString))
                {
                    mDbConnection.Open();
                    string text1 = string.Empty;
                    using (SqliteCommand command = mDbConnection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='xmlvalueprimitive_content'";
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                text1 = reader["name"].ToString();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(text1))
                    {
                        return "xmlvalueprimitive";
                    }

                    string text2 = string.Empty;
                    using (SqliteCommand command = mDbConnection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='fts'";
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                text2 = reader["name"].ToString();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(text2))
                    {
                        return text2;
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTableWithFTSModule Exception: '{0}'", e.Message);
                return null;
            }

            return string.Empty;
        }


        public string GetFlowForInfoObj(SwiInfoObj swiInfoObj)
        {
            log.InfoFormat("GetFlowForInfoObj Identifier: {0}, Flow={1}", swiInfoObj?.Identifier, swiInfoObj?.FlowXml);
            if (string.IsNullOrEmpty(swiInfoObj?.FlowXml))
            {
                log.WarnFormat("GetFlowForInfoObj No FlowXml for: {0}", swiInfoObj?.Identifier);
                return null;
            }

            string data = GetXmlValuePrimitivesById(swiInfoObj.FlowXml, "DEDE");
            log.InfoFormat("GetFlowForInfoObj Data: {0}", data);
            return data;
        }

        public bool IsExecutable()
        {
            log.InfoFormat("IsExecutable Start");

            try
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    log.InfoFormat("IsExecutable Executable");
                    return true;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("IsExecutable Exception: '{0}'", e.Message);
            }

            log.InfoFormat("IsExecutable No executable");
            return false;
        }

        public bool GetEcuVariants(List<EcuInfo> ecuList, Vehicle vehicle = null, IFFMDynamicResolver ffmDynamicResolver = null)
        {
            log.InfoFormat("GetEcuVariants Vehicle: {0}", vehicle != null);
            foreach (EcuInfo ecuInfo in ecuList)
            {
                ecuInfo.SwiActions.Clear();
                ecuInfo.EcuVar = GetEcuVariantByName(ecuInfo.Sgbd);
                ecuInfo.EcuPrgVars = GetEcuProgrammingVariantByName(ecuInfo.PsdzEcu?.BnTnName, vehicle, ffmDynamicResolver);

                GetSwiActionsForEcuVariant(ecuInfo);
                GetSwiActionsForEcuGroup(ecuInfo);
                if (ecuInfo.EcuPrgVars != null)
                {
                    foreach (EcuPrgVar ecuPrgVar in ecuInfo.EcuPrgVars)
                    {
                        List<SwiAction> swiActions = GetSwiActionsForEcuProgrammingVariant(ecuPrgVar.Id, vehicle, ffmDynamicResolver);
                        if (swiActions != null)
                        {
                            ecuInfo.SwiActions.AddRange(swiActions);
                        }
                    }
                }

                foreach (SwiAction swiAction in ecuInfo.SwiActions)
                {
                    swiAction.SwiInfoObjs = GetServiceProgramsForSwiAction(swiAction, vehicle, ffmDynamicResolver);
                }
            }

            log.InfoFormat("GetEcuVariants Result: {0}", true);
            return true;
        }

        public List<SwiAction> GetSwiActionsForRegister(SwiRegisterEnum swiRegisterEnum, bool getChildren, Vehicle vehicle)
        {
            SwiRegister swiRegister = FindNodeForRegister(swiRegisterEnum, vehicle);
            if (swiRegister == null)
            {
                return null;
            }

            return CollectSwiActionsForNode(swiRegister, getChildren);
        }

        public List<SwiAction> CollectSwiActionsForNode(SwiRegister swiRegister, bool getChildren)
        {
            if (swiRegister == null)
            {
                return null;
            }

            List<SwiAction> swiActions = new List<SwiAction>();
            if (swiRegister.SwiActions != null)
            {
                swiActions.AddRange(swiRegister.SwiActions);
            }

            if (getChildren && swiRegister.Children != null)
            {
                foreach (SwiRegister swiRegisterChild in swiRegister.Children)
                {
                    List<SwiAction> swiActionsChild = CollectSwiActionsForNode(swiRegisterChild, true);
                    if (swiActionsChild != null)
                    {
                        swiActions.AddRange(swiActionsChild);
                    }
                }
            }

            return swiActions;
        }

        public SwiRegister FindNodeForRegister(SwiRegisterEnum swiRegisterEnum, Vehicle vehicle)
        {
            try
            {
                if (SwiRegisterTree == null)
                {
                    log.ErrorFormat("FindNodeForRegister No tree");
                    return null;
                }

                string registerId = SwiRegisterEnumerationNameConverter(swiRegisterEnum, vehicle);
                return FindNodeForRegisterId(SwiRegisterTree, registerId);
            }
            catch (Exception e)
            {
                log.ErrorFormat("FindNodeForRegister Exception: '{0}'", e.Message);
            }
            return null;
        }

        public SwiRegister FindNodeForRegisterId(SwiRegister swiRegister, string registerId)
        {
            if (swiRegister == null)
            {
                log.ErrorFormat("FindNodeForRegisterId No register");
                return null;
            }

            if (string.Compare(swiRegister.Identifier, registerId, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return swiRegister;
            }

            if (swiRegister.Children != null)
            {
                foreach (SwiRegister swiRegisterChild in swiRegister.Children)
                {
                    SwiRegister swiRegisterMatch = FindNodeForRegisterId(swiRegisterChild, registerId);
                    if (swiRegisterMatch != null)
                    {
                        return swiRegisterMatch;
                    }
                }
            }

            return null;
        }

        public void ReadSwiRegister(Vehicle vehicle, IFFMDynamicResolver ffmResolver = null)
        {
            List<SwiRegister> swiRegisterRoot = GetSwiRegistersByParentId(null, vehicle, ffmResolver);
            if (swiRegisterRoot != null)
            {
                SwiRegisterTree = swiRegisterRoot.FirstOrDefault();
            }

            ReadSwiRegisterTree(SwiRegisterTree, vehicle, ffmResolver);
            GetSwiActionsForTree(SwiRegisterTree, vehicle, ffmResolver);
        }

        public void ReadSwiRegisterTree(SwiRegister swiRegister, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(swiRegister.Id))
            {
                return;
            }

            List<SwiRegister> swiChildren = GetSwiRegistersByParentId(swiRegister.Id, vehicle, ffmResolver);
            if (swiChildren != null && swiChildren.Count > 0)
            {
                swiRegister.Children = swiChildren;
                foreach (SwiRegister swiChild in swiChildren)
                {
                    ReadSwiRegisterTree(swiChild, vehicle, ffmResolver);
                }
            }
        }

        public void GetSwiActionsForTree(SwiRegister swiRegister, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(swiRegister.Id))
            {
                return;
            }

            log.InfoFormat("GetSwiActionsForTree Start - Id: {0}, Name: {1}", swiRegister.Id, swiRegister.Name);
            swiRegister.SwiActions = GetSwiActionsForSwiRegister(swiRegister, vehicle, ffmResolver);
            if (swiRegister.SwiActions != null)
            {
                foreach (SwiAction swiAction in swiRegister.SwiActions)
                {
                    swiAction.SwiInfoObjs = GetServiceProgramsForSwiAction(swiAction, vehicle, ffmResolver);
                }
            }

            if (swiRegister.Children != null && swiRegister.Children.Count > 0)
            {
                foreach (SwiRegister swiChild in swiRegister.Children)
                {
                    GetSwiActionsForTree(swiChild, vehicle, ffmResolver);
                }
            }
            log.InfoFormat("GetSwiActionsForTree Finish - Id: {0}, Name: {1}", swiRegister.Id, swiRegister.Name);
        }

        public EcuVar GetEcuVariantByName(string ecuVariant)
        {
            if (string.IsNullOrEmpty(ecuVariant))
            {
                return null;
            }

            EcuVar ecuVar = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, FAULTMEMORYDELETEWAITINGTIME, NAME, " + DatabaseFunctions.SqlTitleItems + ", VALIDFROM, VALIDTO, SICHERHEITSRELEVANT, ECUGROUPID, SORT FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", ecuVariant.ToLowerInvariant());
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuVar = ReadXepEcuVar(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuVariantByName Exception: '{0}'", e.Message);
                return null;
            }

            return ecuVar;
        }

        public EcuVar GetEcuVariantById(string ecuVariantId)
        {
            if (string.IsNullOrEmpty(ecuVariantId))
            {
                return null;
            }

            EcuVar ecuVar = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, FAULTMEMORYDELETEWAITINGTIME, NAME, " + DatabaseFunctions.SqlTitleItems + ", VALIDFROM, VALIDTO, SICHERHEITSRELEVANT, ECUGROUPID, SORT FROM XEP_ECUVARIANTS WHERE (ID = {0})", ecuVariantId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuVar = ReadXepEcuVar(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuVariantById Exception: '{0}'", e.Message);
                return null;
            }

            return ecuVar;
        }

        public List<EcuVar> GetEcuVariantsByEcuCliquesId(string ecuCliquesId)
        {
            if (string.IsNullOrEmpty(ecuCliquesId))
            {
                return null;
            }

            List<EcuVar> ecuVarList = new List<EcuVar>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, ECUCLIQUEID FROM XEP_REFECUCLIQUES WHERE (ECUCLIQUEID = {0})", ecuCliquesId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString()?.Trim();
                            EcuVar ecuVar = GetEcuVariantById(id);
                            if (ecuVar != null)
                            {
                                ecuVarList.Add(ecuVar);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuVariantsByEcuCliquesId Exception: '{0}'", e.Message);
                return null;
            }

            return ecuVarList;
        }

        public EcuVar FindEcuVariantFromBntn(string bntn, int? diagAddrAsInt, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(bntn))
            {
                return null;
            }

            if (vehicle == null)
            {
                return null;
            }

            List<EcuVar> ecuVars = FindEcuVariantsFromBntn(bntn, vehicle, ffmResolver);
            if (ecuVars == null || ecuVars.Count == 0)
            {
                log.WarnFormat("FindEcuVariantFromBntn No ECU variant found for: {0}", bntn);
                return null;
            }

            EcuVar ecuVar;
            if (ecuVars.Count == 1)
            {
                ecuVar = ecuVars.FirstOrDefault();
            }
            else
            {
                log.WarnFormat("FindEcuVariantFromBntn More than one ECU variants found: {0}", bntn);
                ecuVar = ecuVars.FirstOrDefault(x => vehicle.ECU != null && vehicle.ECU.Any(i => string.Compare(x.Name, i.ECU_SGBD, StringComparison.InvariantCultureIgnoreCase) == 0));
                if (ecuVar == null)
                {
                    if (diagAddrAsInt == null)
                    {
                        return null;
                    }

                    ECU eCU = vehicle.ECU?.FirstOrDefault(v => v.ID_SG_ADR == diagAddrAsInt);
                    if (eCU != null && !string.IsNullOrEmpty(eCU.ECU_SGBD))
                    {
                        ecuVar = GetEcuVariantByName(eCU.ECU_SGBD);
                    }
                }
            }

            return ecuVar;
        }

        private List<EcuVar> FindEcuVariantsFromBntn(string bntn, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            log.InfoFormat("FindEcuVariantsFromBntn bntn: {0}", bntn);
            if (string.IsNullOrEmpty(bntn))
            {
                return null;
            }

            List<EcuPrgVar> ecuPrgVars = GetEcuProgrammingVariantByName(bntn, vehicle, ffmResolver);
            if (ecuPrgVars == null)
            {
                return null;
            }

            List<EcuVar> ecuVars = new List<EcuVar>();
            foreach (EcuPrgVar ecuPrgVar in ecuPrgVars)
            {
                EcuVar ecuVar = GetEcuVariantById(ecuPrgVar.EcuVarId);
                if (ecuVar != null)
                {
                    if (EvaluateXepRulesById(ecuVar.Id, vehicle, ffmResolver, null))
                    {
                        ecuVars.Add(ecuVar);
                    }
                }
            }
            return ecuVars;
        }

        public List<EcuPrgVar> GetEcuProgrammingVariantByName(string bnTnName, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("GetEcuProgrammingVariantByName BnTnName: {0}", bnTnName);
            if (string.IsNullOrEmpty(bnTnName))
            {
                return null;
            }

            List<EcuPrgVar> ecuPrgVarList = new List<EcuPrgVar>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, FLASHLIMIT, ECUVARIANTID FROM XEP_ECUPROGRAMMINGVARIANT WHERE UPPER(NAME) = UPPER('{0}')", bnTnName);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EcuPrgVar ecuPrgVar = ReadXepEcuPrgVar(reader);
                            bool valid = ecuPrgVar != null;
                            if (vehicle != null && ecuPrgVar != null)
                            {
                                if (!EvaluateXepRulesById(ecuPrgVar.Id, vehicle, ffmDynamicResolver))
                                {
                                    valid = false;
                                }
                            }

                            if (valid)
                            {
                                ecuPrgVarList.Add(ecuPrgVar);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuProgrammingVariantByName Exception: '{0}'", e.Message);
                return null;
            }

            return ecuPrgVarList;
        }

        public EcuPrgVar GetEcuProgrammingVariantById(string prgId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("GetEcuProgrammingVariantById Id: {0}", prgId);
            if (string.IsNullOrEmpty(prgId))
            {
                return null;
            }

            EcuPrgVar ecuPrgVar = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, FLASHLIMIT, ECUVARIANTID FROM XEP_ECUPROGRAMMINGVARIANT WHERE ID = {0}", prgId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EcuPrgVar ecuPrgVarTemp = ReadXepEcuPrgVar(reader);
                            bool valid = true;

                            if (vehicle != null)
                            {
                                if (!EvaluateXepRulesById(ecuPrgVarTemp.EcuVarId, vehicle, ffmDynamicResolver))
                                {
                                    valid = false;
                                }
                            }

                            if (valid)
                            {
                                ecuPrgVar = ecuPrgVarTemp;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuProgrammingVariantById Exception: '{0}'", e.Message);
                return null;
            }

            return ecuPrgVar;
        }

        public EcuGroup FindEcuGroup(EcuVar ecuVar, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (ecuVar == null || string.IsNullOrEmpty(ecuVar.EcuGroupId))
            {
                return null;
            }

            EcuGroup ecuGroup = null;
            string groupId = ecuVar.EcuGroupId;
            log.InfoFormat("FindEcuGroup Id: {0}", groupId);
            ecuGroup = GetEcuGroupById(groupId);
            if (ecuGroup == null)
            {
                log.InfoFormat("FindEcuGroup No EcuGroup found ECU variant: {0}/{1}", ecuVar.Name, groupId);
            }
            return ecuGroup;
        }

        public EcuGroup GetEcuGroupById(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return null;
            }

            EcuGroup ecuGroup = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, OBDIDENTIFICATION, FAULTMEMORYDELETEIDENTIFICATIO, FAULTMEMORYDELETEWAITINGTIME, NAME, VIRTUELL, SICHERHEITSRELEVANT, VALIDTO, VALIDFROM, DIAGNOSTIC_ADDRESS FROM XEP_ECUGROUPS WHERE (ID = {0})", groupId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuGroup = ReadXepEcuGroup(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuGroupById Exception: '{0}'", e.Message);
                return null;
            }

            return ecuGroup;
        }

        public EcuGroup GetEcuGroupByName(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return null;
            }

            List<EcuGroup> ecuGroups = new List<EcuGroup>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, OBDIDENTIFICATION, FAULTMEMORYDELETEIDENTIFICATIO, FAULTMEMORYDELETEWAITINGTIME, NAME, VIRTUELL, SICHERHEITSRELEVANT, VALIDTO, VALIDFROM, DIAGNOSTIC_ADDRESS FROM XEP_ECUGROUPS WHERE (NAME = '{0}' COLLATE UTF8CI)", groupName);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuGroups.Add(ReadXepEcuGroup(reader));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuGroupByName Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetEcuGroupByName: Found {0} groups for group name: {1}", ecuGroups.Count, groupName);
            return ecuGroups.FirstOrDefault();
        }

        public Equipment GetEquipmentById(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId))
            {
                return null;
            }

            Equipment equipement = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, " + DatabaseFunctions.SqlTitleItems + " FROM XEP_EQUIPMENT WHERE (ID = {0})", equipmentId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipement = ReadXepEquipment(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEquipmentById Exception: '{0}'", e.Message);
                return null;
            }

            return equipement;
        }

        public EcuClique GetEcuClique(string ecuCliqueId)
        {
            if (string.IsNullOrEmpty(ecuCliqueId))
            {
                return null;
            }

            EcuClique ecuClique = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, CLIQUENKURZBEZEICHNUNG, " + DatabaseFunctions.SqlTitleItems + ", ECUREPID FROM XEP_ECUCLIQUES WHERE (ID = {0})", ecuCliqueId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuClique = ReadXepEcuClique(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuCliqueById Exception: '{0}'", e.Message);
                return null;
            }

            return ecuClique;
        }

        public EcuRefClique GetRefEcuCliqueById(string ecuRefId)
        {
            if (string.IsNullOrEmpty(ecuRefId))
            {
                return null;
            }

            EcuRefClique ecuRefClique = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, ECUCLIQUEID FROM XEP_REFECUCLIQUES WHERE (ID = {0})", ecuRefId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuRefClique = ReadXepEcuRefClique(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetRefEcuCliqueById Exception: '{0}'", e.Message);
                return null;
            }

            return ecuRefClique;
        }

        public EcuClique FindEcuClique(EcuVar ecuVar)
        {
            if (ecuVar == null)
            {
                return null;
            }

            EcuRefClique ecuRefClique = GetRefEcuCliqueById(ecuVar.Id);
            if (ecuRefClique == null)
            {
                return null;
            }

            return GetEcuClique(ecuRefClique.EcuCliqueId);
        }

        public CharacteristicRoots GetCharacteristicRootsById(string characteristicRootsId)
        {
            if (string.IsNullOrEmpty(characteristicRootsId))
            {
                return null;
            }

            CharacteristicRoots characteristicRoots = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NODECLASS, " + DatabaseFunctions.SqlTitleItems + ", MOTORCYCLESEQUENCE, VEHICLESEQUENCE FROM XEP_CHARACTERISTICROOTS WHERE (ID = {0})", characteristicRootsId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            characteristicRoots = ReadXepCharacteristicRoots(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetCharacteristicRootsById Exception: '{0}'", e.Message);
                return null;
            }

            return characteristicRoots;
        }

        public List<Characteristics> GetCharacteristicsByTypeKeyId(string typeKeyId)
        {
            log.InfoFormat("GetCharacteristicsByTypeKeyId TypeKey: {0}", typeKeyId);
            if (string.IsNullOrEmpty(typeKeyId))
            {
                return null;
            }

            List<Characteristics> characteristicsList = new List<Characteristics>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT C.ID, C.NODECLASS, C.TITLEID, " + SqlTitleItemsC +
                    @", C.STATICCLASSVARIABLES, C.STATICCLASSVARIABLESMOTORRAD, C.PARENTID, C.ISTA_VISIBLE, C.NAME, C.LEGACY_NAME, V.DRIVEID, CR.NODECLASS" +
                    @" AS PARENTNODECLASS FROM xep_vehicles V JOIN xep_characteristics C on C.ID = V.CHARACTERISTICID JOIN xep_characteristicroots CR on CR.ID = C.PARENTID" +
                    @" WHERE TYPEKEYID = {0} AND CR.NODECLASS IS NOT NULL", typeKeyId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Characteristics characteristics = ReadXepCharacteristics(reader);
                            characteristics.DriveId = reader["DRIVEID"].ToString()?.Trim();
                            characteristics.RootNodeClass = reader["PARENTNODECLASS"].ToString()?.Trim();
                            characteristicsList.Add(characteristics);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetCharacteristicsByTypeKeyId Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetCharacteristicsByTypeKeyId Count: {0}", characteristicsList.Count);
            return characteristicsList;
        }

        public VinRanges GetVinRangesByVin(string vin)
        {
            log.InfoFormat("GetVinRangesByVin Vin: {0}", vin ?? string.Empty);
            if (string.IsNullOrEmpty(vin))
            {
                log.ErrorFormat("GetVinRangesByVin Empty Vin");
                return null;
            }

            List<VinRanges> vinRangesList = new List<VinRanges>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT VINBANDFROM, VINBANDTO, TYPSCHLUESSEL, PRODUCTIONDATEYEAR, PRODUCTIONDATEMONTH, RELEASESTATE, CHANGEDATE, GEARBOX_TYPE, VIN17_4_7" +
                    @" FROM VINRANGES WHERE ('{0}' BETWEEN VINBANDFROM AND VINBANDTO)", vin.ToUpper(CultureInfo.InvariantCulture));
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VinRanges vinRanges = ReadXepVinRanges(reader);
                            // clear invalid types
                            vinRanges.GearboxType = string.Empty;
                            vinRanges.Vin17_4_7 = string.Empty;
                            vinRangesList.Add(vinRanges);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetVinRangesByVin Exception: '{0}'", e.Message);
                return null;
            }

            if (vinRangesList.Count > 1)
            {
                log.InfoFormat("GetVinRangesByVin List count: {0}", vinRangesList.Count);
            }

            if (vinRangesList.Count == 1)
            {
                VinRanges vinRanges = vinRangesList.First();
                log.InfoFormat("GetVinRangesByVin TypeKey: {0}", vinRanges.TypeKey);
                return vinRanges;
            }

            log.ErrorFormat("GetVinRangesByVin Not found: {0}", vin);
            return null;
        }

        public VinRanges GetVinRangesByVin17(string vin17_4_7, string vin7, bool returnFirstEntryWithoutCheck, bool vehicleHasOnlyVin7, bool isPrerelease = false)
        {
            VinRanges vinRanges = DoGetVinRangesByVin17(vin17_4_7, vin7, returnFirstEntryWithoutCheck, vehicleHasOnlyVin7, false);
            if (vinRanges != null)
            {
                return vinRanges;
            }

            if (isPrerelease)
            {
                vinRanges = DoGetVinRangesByVin17(vin17_4_7, vin7, returnFirstEntryWithoutCheck, vehicleHasOnlyVin7, true);
            }

            return vinRanges;
        }

        public VinRanges DoGetVinRangesByVin17(string vin17_4_7, string vin7, bool returnFirstEntryWithoutCheck, bool vehicleHasOnlyVin7, bool isPrerelease)
        {
            log.InfoFormat("GetVinRangesByVin17 Vin17_4_7: {0}, Vin7: {1}, FirstEntry: {2}, OnlyVin7: {3}, PreRelase: {4}",
                vin17_4_7 ?? string.Empty, vin7 ?? string.Empty, returnFirstEntryWithoutCheck, vehicleHasOnlyVin7, isPrerelease);
            if (string.IsNullOrEmpty(vin17_4_7) || string.IsNullOrEmpty(vin7))
            {
                log.ErrorFormat("GetVinRangesByVin17 Empty Vin");
                return null;
            }

            List<VinRanges> vinRangesList = new List<VinRanges>();
            VinRangeQuerySettings vinRangeQuerySettings = new VinRangeQuerySettings(isPrerelease);
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT VINBANDFROM, VINBANDTO, TYPSCHLUESSEL, {0}, {1}, RELEASESTATE, CHANGEDATE, GEARBOX_TYPE, VIN17_4_7" +
                    @" FROM {2} WHERE ('{3}' BETWEEN VINBANDFROM AND VINBANDTO) AND (VIN17_4_7 = '{4}')",
                    vinRangeQuerySettings.ProductionYearColumnName, vinRangeQuerySettings.ProductionMonthColumnName, vinRangeQuerySettings.TableName,
                    vin7.ToUpper(CultureInfo.InvariantCulture), vin17_4_7.ToUpper(CultureInfo.InvariantCulture));
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VinRanges vinRanges = ReadXepVinRanges(reader);
                            vinRangesList.Add(vinRanges);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetVinRangesByVin17 Exception: '{0}'", e.Message);
                return null;
            }

            if (vinRangesList.Count > 1)
            {
                log.InfoFormat("GetVinRangesByVin17 Found more than one entry: {0} in {1}", vinRangesList.Count, vinRangeQuerySettings.TableName);
            }

            IComparer<string> comparer = new EbcdicVIN7Comparer();
            List<VinRanges> vinRangesList2 = new List<VinRanges>();
            foreach (VinRanges vinRanges in vinRangesList)
            {
                if (comparer.Compare(vinRanges.VinBandFrom, vin7) <= 0 && comparer.Compare(vinRanges.VinBandTo, vin7) >= 0)
                {
                    vinRangesList2.Add(vinRanges);
                }
            }

            if (vinRangesList2.Count == 1)
            {
                VinRanges vinRanges = vinRangesList2.First();
                log.InfoFormat("GetVinRangesByVin17 TypeKey: {0}", vinRanges.TypeKey);
                return vinRanges;
            }
            if (vinRangesList2.Count > 1)
            {
                log.ErrorFormat("GetVinRangesByVin17 Too many items after filter: {0}", vinRangesList.Count);
                return null;
            }

            if (vehicleHasOnlyVin7)
            {
                return GetVinRangesByVin(vin7);
            }

            if (returnFirstEntryWithoutCheck)
            {
                return GetVinRangesByVin17_4_7(vin17_4_7, isPrerelease);
            }

            log.ErrorFormat("GetVinRangesByVin17 Not found: {0}", vin17_4_7);
            return null;
        }

        public VinRanges GetVinRangesByVin17_4_7(string vin17_4_7, bool isPrerelease)
        {
            log.InfoFormat("GetVinRangesByVin17_4_7 Vin17_4_7: {0}, PreRelease: {1}", vin17_4_7 ?? string.Empty, isPrerelease);
            if (string.IsNullOrEmpty(vin17_4_7))
            {
                log.ErrorFormat("GetVinRangesByVin17_4_7 Empty Vin");
                return null;
            }

            List<VinRanges> vinRangesList = new List<VinRanges>();
            VinRangeQuerySettings vinRangeQuerySettings = new VinRangeQuerySettings(isPrerelease);
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT VINBANDFROM, VINBANDTO, TYPSCHLUESSEL, {0}, {1}, RELEASESTATE, CHANGEDATE, GEARBOX_TYPE, VIN17_4_7" +
                    @" FROM {2} WHERE (VIN17_4_7 = '{3}')",
                    vinRangeQuerySettings.ProductionYearColumnName, vinRangeQuerySettings.ProductionMonthColumnName, vinRangeQuerySettings.TableName,
                    vin17_4_7.ToUpper(CultureInfo.InvariantCulture));
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VinRanges vinRanges = ReadXepVinRanges(reader);
                            vinRangesList.Add(vinRanges);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetVinRangesByVin17_4_7 Exception: '{0}'", e.Message);
                return null;
            }

            if (vinRangesList.Count > 1)
            {
                log.InfoFormat("GetVinRangesByVin17_4_7 List count: {0} in {1}", vinRangesList.Count, vinRangeQuerySettings.TableName);
            }

            if (vinRangesList.Count >= 1)
            {
                VinRanges vinRanges = vinRangesList.First();
                log.InfoFormat("GetVinRangesByVin17_4_7 TypeKey: {0}", vinRanges.TypeKey);
                return vinRanges;
            }

            log.ErrorFormat("GetVinRangesByVin17_4_7 Not found: {0} in {1}", vin17_4_7, vinRangeQuerySettings.TableName);
            return null;
        }

        public List<string> GetAllTypeKeys()
        {
            log.InfoFormat("GetAllTypeKeys");
            List<string> typeKeys = new List<string>();
            try
            {
                string sql = @"SELECT DISTINCT TYPSCHLUESSEL FROM VINRANGES";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string typeKey = reader["TYPSCHLUESSEL"].ToString()?.Trim();
                            typeKeys.AddIfNotContains(typeKey);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetAllTypeKeys Exception: '{0}'", e.Message);
                return null;
            }

            return typeKeys;
        }

        public List<ProductionDate> GetAllProductionDatesForTypeKeys(List<string> typeKeys)
        {
            if (typeKeys == null || typeKeys.Count == 0)
            {
                return null;
            }

            log.InfoFormat("GetAllProductionDatesForTypeKey: {0}", typeKeys.ToStringItems());
            List<ProductionDate> productionDatesSort = null;
            try
            {
                List<ProductionDate> productionDates = new List<ProductionDate>();
                StringBuilder sbSql = new StringBuilder();

                foreach (string typeKey in typeKeys)
                {
                    if (sbSql.Length > 0)
                    {
                        sbSql.Append(", ");
                    }

                    sbSql.Append("'");
                    sbSql.Append(typeKey);
                    sbSql.Append("'");
                }

                string sql = @"SELECT DISTINCT PRODUCTIONDATEYEAR, PRODUCTIONDATEMONTH FROM VINRANGES WHERE TYPSCHLUESSEL IN(" + sbSql + ")";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string prodYear = reader["PRODUCTIONDATEYEAR"].ToString()?.Trim();
                            string prodMonth = reader["PRODUCTIONDATEMONTH"].ToString()?.Trim();
                            if (!string.IsNullOrEmpty(prodYear) && !string.IsNullOrEmpty(prodMonth))
                            {
                                ProductionDate productionDate = new ProductionDate(prodYear, prodMonth);
                                if (productionDate.GetValue() > 0)
                                {
                                    productionDates.Add(new ProductionDate(prodYear, prodMonth));
                                }
                            }
                        }
                    }
                }

                // sort items
                productionDatesSort = productionDates.OrderBy(x => x.GetValue()).ToList();
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetAllProductionDatesForTypeKey Exception: '{0}'", e.Message);
                return null;
            }

            return productionDatesSort;
        }

        public List<Characteristics> GetVehicleCharacteristics(Vehicle vehicle)
        {
            List<Characteristics> characteristicsList = GetVehicleCharacteristicsFromDatabase(vehicle, false);
            if (characteristicsList != null && characteristicsList.Count > 0)
            {
                GetAlpinaCharacteristics(vehicle, characteristicsList);
            }

            return characteristicsList;
        }

        // ToDo: Check on update
        public static BatteryEnum ResolveBatteryType(Vehicle vecInfo)
        {
            if (new List<string>
                {
                    "F80", "F82", "F83", "F90", "F91", "F92", "F93", "G80", "G81", "G82",
                    "G83"
                }.Contains(vecInfo.Ereihe) || (vecInfo.Ereihe.StartsWith("N", StringComparison.OrdinalIgnoreCase) && vecInfo.IsBev()))
            {
                return BatteryEnum.LFP;
            }
            if (vecInfo.IsBev() || vecInfo.IsPhev() || vecInfo.IsHybr() || vecInfo.IsErex() || vecInfo.Ereihe.Equals("I01") || vecInfo.hasSA("1CE"))
            {
                return BatteryEnum.PbNew;
            }
            return BatteryEnum.Pb;
        }

        public static bool IsVehicleAnAlpina(Vehicle vehicle)
        {
            return vehicle.hasSA("920");
        }

        // ToDo: Check on update
        // from VehicleIdent
        public static string GetProdArt(Vehicle vecInfo)
        {
            string result = "P";
            if (vecInfo != null && ("M".Equals(vecInfo.Prodart) || vecInfo.BrandName == BrandName.BMWMOTORRAD))
            {
                result = "M";
            }
            return result;
        }

        public void GetAlpinaCharacteristics(Vehicle vehicle, List<Characteristics> characteristicsList)
        {
            if (!IsVehicleAnAlpina(vehicle))
            {
                return;
            }

            log.InfoFormat("HandleAlpinaVehicles List size: {0}", characteristicsList.Count);
            List<Characteristics> vehicleCharacteristicsFromDatabase = GetVehicleCharacteristicsFromDatabase(vehicle, true);
            if (vehicleCharacteristicsFromDatabase != null)
            {
                using (List<Characteristics>.Enumerator enumerator = (from c in vehicleCharacteristicsFromDatabase
                    where engineRootNodeClasses.Contains(c.RootNodeClass)
                    select c).ToList().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Characteristics characteristicCurrent = enumerator.Current;
                        if (characteristicCurrent != null)
                        {
                            Characteristics characteristicMatch = characteristicsList.FirstOrDefault(c => c.RootNodeClass == characteristicCurrent.RootNodeClass);
                            if (characteristicMatch != null)
                            {
                                if (characteristicMatch.Name != characteristicCurrent.Name)
                                {
                                    log.InfoFormat("HandleAlpinaVehicles Overwrite: {0} by {1}", characteristicMatch.Name, characteristicCurrent.Name);
                                }
                                characteristicsList.Remove(characteristicMatch);
                            }
                            characteristicsList.Add(characteristicCurrent);
                        }
                    }
                }
            }
        }

        public List<Characteristics> GetVehicleCharacteristicsFromDatabase(Vehicle vehicle, bool isAlpina)
        {
            log.InfoFormat("GetVehicleCharacteristicsFromDatabase VinRangeType: {0}, Alpina: {1}", vehicle.VINRangeType, isAlpina);
            List<Characteristics> characteristicsList = null;
            if (!string.IsNullOrEmpty(vehicle.VINRangeType))
            {
                characteristicsList = GetVehicleIdentByTypeKey(vehicle.VINRangeType, isAlpina);
            }

            if (characteristicsList == null || characteristicsList.Count == 0)
            {
                characteristicsList = GetVehicleIdentByTypeKey(vehicle.GMType, isAlpina);
            }

            if (characteristicsList == null)
            {
                log.ErrorFormat("GetVehicleCharacteristicsFromDatabase VinRangeType: {0} Not found", vehicle.VINRangeType);
            }
            else
            {
                log.InfoFormat("GetVehicleCharacteristicsFromDatabase Count: {0}", characteristicsList.Count);
                foreach (Characteristics characteristics in characteristicsList)
                {
                    log.InfoFormat("Characteristics: {0}", characteristics.ToString(ClientContext.GetLanguage(vehicle)));
                }
            }

            return characteristicsList;
        }

        public List<Characteristics> GetVehicleIdentByTypeKey(string typeKey, bool isAlpina)
        {
            string typeKeyId = GetTypeKeyId(typeKey, isAlpina);
            return GetCharacteristicsByTypeKeyId(typeKeyId);
        }

        public SaLaPa GetSaLaPaById(string salapaId)
        {
            log.InfoFormat("GetSaLaPaById Id: {0}", salapaId);
            if (string.IsNullOrEmpty(salapaId))
            {
                return null;
            }

            SaLaPa saLaPa = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", NAME, PRODUCT_TYPE FROM XEP_SALAPAS WHERE (ID = {0})", salapaId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saLaPa = ReadXepSaLaPa(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSaLaPaById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetSaLaPaById Name: {0}", saLaPa?.Name);
            return saLaPa;
        }

        public List<Tuple<string, string>> GetTransmissionSaByTypeKey(string typekey)
        {
            log.InfoFormat("GetTransmissionSaByTypeKey Typekey: {0}", typekey);
            if (string.IsNullOrEmpty(typekey))
            {
                return null;
            }

            List<Tuple<string, string>> saList = new List<Tuple<string, string>>();
            try
            {
                string typeKeyId = GetTypeKeyId(typekey, false);
                if (string.IsNullOrEmpty(typeKeyId))
                {
                    return null;
                }

                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT SA.Name AS SANAME, LINK.VERB AS VERB FROM STD_TYPEKEY_SA_LINK LINK JOIN XEP_SALAPAS SA ON SA.ID = SALAPA_OID" +
                    @" WHERE VEHICLETYPE_ID = {0} AND UPPER(SA_GROUP) = 'TRANSMISSION'", typeKeyId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string saName = reader["SANAME"].ToString()?.Trim();
                            string verb = reader["VERB"].ToString()?.Trim();
                            Tuple<string, string> item = new Tuple<string, string>(saName, verb);
                            saList.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTransmissionSaByTypeKey Exception: '{0}'", e.Message);
                return null;
            }

            if (saList.Count > 1)
            {
                log.ErrorFormat("GetTransmissionSaByTypeKey Multipe entries: {0}", saList.Count);
            }
            else
            {
                log.InfoFormat("GetTransmissionSaByTypeKey Count: {0}", saList.Count);
            }
            return saList;
        }

        public SaLaPa GetSaLaPaByProductTypeAndSalesKey(string productType, string salesKey)
        {
            log.InfoFormat("GetSaLaPaByProductTypeAndSalesKey Type: {0}, Key: {1}", productType, salesKey);
            if (string.IsNullOrEmpty(productType) || string.IsNullOrEmpty(salesKey))
            {
                return null;
            }

            SaLaPa saLaPa = null;
            try
            {
                string tmpSalesKey = salesKey.TrimStart('0');
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", NAME, PRODUCT_TYPE FROM XEP_SALAPAS WHERE (NAME = '{0}' AND PRODUCT_TYPE = '{1}')", tmpSalesKey, productType);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saLaPa = ReadXepSaLaPa(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSaLaPaById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetSaLaPaById Name: {0}", saLaPa?.Name);
            return saLaPa;
        }

        public EcuReps GetEcuRepsById(string ecuId)
        {
            log.InfoFormat("GetEcuRepsById Sortcut: {0}", ecuId);
            if (string.IsNullOrEmpty(ecuId))
            {
                return null;
            }

            EcuReps ecuReps = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, STEUERGERAETEKUERZEL FROM XEP_ECUREPS WHERE (ID = {0})", ecuId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuReps = ReadXepEcuReps(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuRepsById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetEcuRepsById Sortcut: {0}", ecuReps?.EcuShortcut);
            return ecuReps;
        }

        public EcuReps FindEcuRep(EcuClique ecuClique)
        {
            if (ecuClique == null || string.IsNullOrEmpty(ecuClique.EcuRepId))
            {
                return null;
            }

            return GetEcuRepsById(ecuClique.EcuRepId);
        }

        public bool GetSwiActionsForEcuVariant(EcuInfo ecuInfo)
        {
            if (ecuInfo.EcuVar == null || string.IsNullOrEmpty(ecuInfo.EcuVar.Id))
            {
                return false;
            }

            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    @", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUVARIANTS_SWIACTION WHERE ECUVARIANT_ID = {0})",
                    ecuInfo.EcuVar.Id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarId);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiActionsForEcuVariant Exception: '{0}'", e.Message);
                return false;
            }

            return true;
        }

        public bool GetSwiActionsForEcuGroup(EcuInfo ecuInfo)
        {
            if (ecuInfo.EcuVar == null || string.IsNullOrEmpty(ecuInfo.EcuVar.EcuGroupId))
            {
                return false;
            }

            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    @", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUGROUPS_SWIACTION WHERE ECUGROUP_ID = {0})",
                    ecuInfo.EcuVar.EcuGroupId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarGroupId);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiActionsForEcuGroup Exception: '{0}'", e.Message);
                return false;
            }

            return true;
        }

        public List<SwiAction> GetSwiActionsForEcuProgrammingVariant(string prgId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            if (string.IsNullOrEmpty(prgId))
            {
                return null;
            }

            log.InfoFormat("GetSwiActionsForEcuProgrammingVariant Id: {0}", prgId);
            List<SwiAction> swiActions = new List<SwiAction>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    @", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUPRGVARI_SWIACTION WHERE ECUPROGRAMMINGVARIANT_ID = {0})",
                    prgId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarPrgEcuId);
                            if (EvaluateXepRulesById(swiAction.Id, vehicle, ffmDynamicResolver))
                            {
                                swiActions.Add(swiAction);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiActionsForEcuProgrammingVariant Exception: '{0}'", e.Message);
                return null;
            }

            return swiActions;
        }

        public List<SwiAction> GetSwiActionsForSwiRegister(SwiRegister swiRegister, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(swiRegister.Id))
            {
                return null;
            }

            log.InfoFormat("GetSwiActionsForSwiRegister Id: {0}, Name: {1}", swiRegister.Id, swiRegister.Name);
            List<SwiAction> swiActions = new List<SwiAction>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_SWIREGISTER_SWIACTION WHERE SWI_REGISTER_ID = {0})",
                    swiRegister.Id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.SwiRegister);
                            if (vehicle != null)
                            {
                                if (EvaluateXepRulesById(swiAction.Id, vehicle, ffmResolver))
                                {
                                    swiActions.Add(swiAction);
                                }
                            }
                            else
                            {
                                swiActions.Add(swiAction);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiActionsForSwiRegister Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetSwiActionsForSwiRegister Actions Count: {0}", swiActions.Count);
            foreach (SwiAction swiAction in swiActions)
            {
                log.InfoFormat("Action Id: {0}, Name: '{1}'", swiAction.Id, swiAction.Name);
            }
            return swiActions;
        }

        public List<SwiAction> GetSwiActionsLinkedToSwiActionId(string id, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            log.InfoFormat("GetSwiActionsLinkedToSwiActionId Id: {0}", id);
            List<SwiAction> swiActions = new List<SwiAction>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_TARGET_ID FROM XEP_REF_SWIACTION_SWIACTION WHERE SWI_ACTION_ID = {0})",
                    id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.SwiRegister);
                            if (EvaluateXepRulesById(swiAction.Id, vehicle, ffmResolver))
                            {
                                swiActions.Add(swiAction);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiActionsLinkedToSwiActionId Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetSwiActionsLinkedToSwiActionId Actions Count: {0}", swiActions.Count);
            foreach (SwiAction swiAction in swiActions)
            {
                log.InfoFormat("Action Id: {0}, Name: '{1}'", swiAction.Id, swiAction.Name);
            }
            return swiActions;
        }

        // From VehicleConversionManager
        public List<SwiAction> ReadLinkedSwiActions(Vehicle vehicle, List<SwiAction> selectedRegister, IFFMDynamicResolver ffmResolver)
        {
            if (selectedRegister == null)
            {
                return null;
            }

            log.InfoFormat("ReadLinkedSwiActions Register count: {0}", selectedRegister.Count);

            List<SwiAction> swiActionsLinked = new List<SwiAction>();
            foreach (SwiAction swiAction in selectedRegister)
            {
                List<SwiAction> swiActions = GetSwiActionsLinkedToSwiActionId(swiAction.Id, vehicle, ffmResolver);
                if (swiActions != null)
                {
                    swiActionsLinked.AddRange(swiActions);
                }
            }

            log.InfoFormat("ReadLinkedSwiActions Actions Count: {0}", swiActionsLinked.Count);
            foreach (SwiAction swiAction in swiActionsLinked)
            {
                log.InfoFormat("Action Id: {0}, Name: '{1}'", swiAction.Id, swiAction.Name);
            }
            return swiActionsLinked;
        }

        public List<SwiRegister> GetSwiRegistersByParentId(string parentId, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            List<SwiRegister> swiRegisterList = new List<SwiRegister>();
            try
            {
                string selection = parentId != null ? string.Format(CultureInfo.InvariantCulture, @"= {0}", parentId) : @"IS NULL";
                string sql = @"SELECT ID, NODECLASS, PARENTID, NAME, REMARK, SORT, TITLEID, " + DatabaseFunctions.SqlTitleItems +
                             @", VERSIONNUMBER, IDENTIFIER FROM XEP_SWIREGISTER WHERE PARENTID " + selection;
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiRegister swiRegister = ReadXepSwiRegister(reader);
                            if (vehicle != null)
                            {
                                if (EvaluateXepRulesById(swiRegister.Id, vehicle, ffmResolver))
                                {
                                    swiRegisterList.Add(swiRegister);
                                }
                            }
                            else
                            {
                                swiRegisterList.Add(swiRegister);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiRegistersByParentId Exception: '{0}'", e.Message);
                return null;
            }

            return swiRegisterList;
        }

        public List<SwiInfoObj> GetServiceProgramsForSwiAction(SwiAction swiAction, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, bool contextHddUpdate = false)
        {
            if (string.IsNullOrEmpty(swiAction.Id))
            {
                return null;
            }

            log.InfoFormat("GetServiceProgramsForSwiAction Id: {0}, Name: {1}", swiAction.Id, swiAction.Name);
            List<SwiInfoObj> swiInfoObjList = new List<SwiInfoObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT INFOOBJECTID, LINK_TYPE_ID, PRIORITY FROM XEP_REFINFOOBJECTS WHERE ID IN (SELECT ID FROM XEP_SWIACTION WHERE ID = {0})",
                    swiAction.Id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string infoObjId = reader["INFOOBJECTID"].ToString()?.Trim();
                            string linkTypeId = reader["LINK_TYPE_ID"].ToString()?.Trim();
                            SwiInfoObj swiInfoObj = GetInfoObjectById(infoObjId, linkTypeId, contextHddUpdate);
                            if (swiInfoObj != null)
                            {
                                if (vehicle != null)
                                {
                                    if (EvaluateXepRulesById(infoObjId, vehicle, ffmDynamicResolver, swiInfoObj.ControlId))
                                    {
                                        swiInfoObjList.Add(swiInfoObj);
                                    }
                                }
                                else
                                {
                                    swiInfoObjList.Add(swiInfoObj);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetServiceProgramsForSwiAction Exception: '{0}'", e.Message);
                return null;
            }

            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT DIAGNOSISOBJECTCONTROLID, PRIORITY FROM XEP_REF_SWIACTION_DIAGOBJECTS WHERE SWI_ACTION_ID IN (SELECT ID FROM XEP_SWIACTION WHERE ID = {0})",
                    swiAction.Id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string controlId = reader["DIAGNOSISOBJECTCONTROLID"].ToString()?.Trim();
                            List<SwiDiagObj> swiDiagObjs = GetDiagObjectsByControlId(controlId, vehicle, ffmDynamicResolver, getHidden: true);
                            if (swiDiagObjs != null)
                            {
                                foreach (SwiDiagObj swiDiagObj in swiDiagObjs)
                                {
                                    if (!string.IsNullOrEmpty(swiDiagObj.ControlId))
                                    {
                                        List<SwiInfoObj> swiInfoObjs = GetInfoObjectsByDiagObjectControlId(swiDiagObj.ControlId, SwiInfoObj.SwiActionDatabaseLinkType.SwiActionDiagnosticLink);
                                        if (swiInfoObjs != null)
                                        {
                                            swiInfoObjList.AddRange(swiInfoObjs);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetServiceProgramsForSwiAction DIAGNOSISOBJECTCONTROLID Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetServiceProgramsForSwiAction InfoObj Count: {0}", swiInfoObjList.Count);
            foreach (SwiInfoObj swiInfoObj in swiInfoObjList)
            {
                log.InfoFormat("InfoObj Id: {0}, Identifier: '{1}'", swiInfoObj.Id, swiInfoObj.Identifier);
            }

            return swiInfoObjList;
        }

        public SwiInfoObj GetInfoObjectById(string infoObjectId, string linkTypeId, bool contextHddUpdate = false)
        {
            if (string.IsNullOrEmpty(infoObjectId))
            {
                return null;
            }

            SwiInfoObj swiInfoObj = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NODECLASS, ASSEMBLY, VERSIONNUMBER, PROGRAMTYPE, SICHERHEITSRELEVANT, TITLEID, " +
                    DatabaseFunctions.SqlTitleItems + ", GENERELL, TELESERVICEKENNUNG, FAHRZEUGKOMMUNIKATION, MESSTECHNIK, VERSTECKT, NAME, INFORMATIONSTYP, " +
                    @"IDENTIFIKATOR, INFORMATIONSFORMAT, SINUMMER, ZIELISTUFE, CONTROLID, INFOTYPE, INFOFORMAT, DOCNUMBER, PRIORITY, IDENTIFIER, FLOWXML FROM XEP_INFOOBJECTS WHERE XEP_INFOOBJECTS.ID = {0}",
                    infoObjectId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiInfoObj.SwiActionDatabaseLinkType? dbLinkType = SwiInfoObj.GetLinkType(linkTypeId);
                            SwiActionLinkType? mappedLinkType = MapLinkTypeDatabaseToApplication(dbLinkType, contextHddUpdate);
                            swiInfoObj = ReadXepSwiInfoObj(reader, dbLinkType, mappedLinkType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetInfoObjectById Exception: '{0}'", e.Message);
                return null;
            }

            return swiInfoObj;
        }

        public List<SwiInfoObj> GetInfoObjectsByDiagObjectControlId(string infoObjectId, SwiInfoObj.SwiActionDatabaseLinkType linkType)
        {
            if (string.IsNullOrEmpty(infoObjectId))
            {
                return null;
            }

            List<SwiInfoObj> swiInfoObjs = new List<SwiInfoObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NODECLASS, ASSEMBLY, VERSIONNUMBER, PROGRAMTYPE, SICHERHEITSRELEVANT, TITLEID, " +
                    DatabaseFunctions.SqlTitleItems + ", GENERELL, TELESERVICEKENNUNG, FAHRZEUGKOMMUNIKATION, MESSTECHNIK, VERSTECKT, NAME, INFORMATIONSTYP, " +
                    @"IDENTIFIKATOR, INFORMATIONSFORMAT, SINUMMER, ZIELISTUFE, CONTROLID, INFOTYPE, INFOFORMAT, DOCNUMBER, PRIORITY, IDENTIFIER, FLOWXML FROM XEP_INFOOBJECTS WHERE XEP_INFOOBJECTS.ID = {0}",
                    infoObjectId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiInfoObj swiInfoObj = ReadXepSwiInfoObj(reader, linkType);
                            if (swiInfoObj != null)
                            {
                                swiInfoObjs.Add(swiInfoObj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetInfoObjectsByDiagObjectControlId Exception: '{0}'", e.Message);
                return null;
            }

            return swiInfoObjs;
        }

        public List<SwiInfoObj> GetInfoObjectsByDiagObjectControlId(string diagnosisObjectControlId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, bool getHidden, List<string> typeFilter = null)
        {
            if (string.IsNullOrEmpty(diagnosisObjectControlId))
            {
                return null;
            }

            List<SwiInfoObj> swiInfoObjs = new List<SwiInfoObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, INFOOBJECTID FROM XEP_REFINFOOBJECTS WHERE ID = {0}", diagnosisObjectControlId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string infoObjId = reader["INFOOBJECTID"].ToString()?.Trim();
                            if (vehicle == null || IsInfoObjectValid(infoObjId, vehicle, ffmDynamicResolver))
                            {
                                List<SwiInfoObj> infoObjs = SelectXepInfoObjects(infoObjId, vehicle, ffmDynamicResolver, typeFilter);
                                if (infoObjs != null)
                                {
                                    swiInfoObjs.AddRange(infoObjs);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetInfoObjectsByDiagObjectControlId Exception: '{0}'", e.Message);
                return null;
            }

            return swiInfoObjs;
        }

        public List<SwiInfoObj> SelectXepInfoObjects(string infoObjectId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, List<string> typeFilter = null)
        {
            if (string.IsNullOrEmpty(infoObjectId))
            {
                return null;
            }

            List<SwiInfoObj> swiInfoObjs = new List<SwiInfoObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NODECLASS, ASSEMBLY, VERSIONNUMBER, PROGRAMTYPE, SICHERHEITSRELEVANT, TITLEID, " +
                    DatabaseFunctions.SqlTitleItems + ", GENERELL, TELESERVICEKENNUNG, FAHRZEUGKOMMUNIKATION, MESSTECHNIK, VERSTECKT, NAME, INFORMATIONSTYP, " +
                    @"IDENTIFIKATOR, INFORMATIONSFORMAT, SINUMMER, ZIELISTUFE, CONTROLID, INFOTYPE, INFOFORMAT, DOCNUMBER, PRIORITY, IDENTIFIER, FLOWXML FROM XEP_INFOOBJECTS WHERE XEP_INFOOBJECTS.ID = {0} AND XEP_INFOOBJECTS.GENERELL = 0",
                    infoObjectId);
                //sql = EnrichQueryForServicePrograms(sql);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiInfoObj swiInfoObj = ReadXepSwiInfoObj(reader);
                            if (swiInfoObj != null)
                            {
                                if (typeFilter != null && typeFilter.Count > 0)
                                {
                                    string match = typeFilter.FirstOrDefault(x => string.Compare(x, swiInfoObj.InfoType, StringComparison.OrdinalIgnoreCase) == 0);
                                    if (match == null)
                                    {
                                        continue;
                                    }
                                }

                                if (!IsInfoObjectValid(swiInfoObj.Id, vehicle, ffmDynamicResolver))
                                {
                                    continue;
                                }

                                swiInfoObjs.Add(swiInfoObj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("SelectXepInfoObjects Exception: '{0}'", e.Message);
                return null;
            }

            return swiInfoObjs;
        }

        public bool IsInfoObjectValid(string infoObjectId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("IsInfoObjectValid Id: {0}, Vehicle: {1}", infoObjectId, vehicle != null);

            string infoObjectObjectId = GetInfoObjectObjectId(infoObjectId);
            if (!EvaluateXepRulesById(infoObjectId, vehicle, ffmDynamicResolver, infoObjectObjectId))
            {
                log.ErrorFormat("IsInfoObjectValid EvaluateXepRulesById Valid: '{0}'", false);
                return false;
            }

            List<SwiDiagObj> diagObjectsList = GetDiagObjectsForInfoObject(infoObjectId, null, null, getHidden: true);
            if (diagObjectsList == null)
            {
                log.ErrorFormat("IsInfoObjectValid GetDiagObjectsForInfoObject Valid: '{0}'", false);
                return false;
            }

            if (diagObjectsList.Count == 0)
            {
                log.InfoFormat("IsInfoObjectValid Valid: '{0}'", true);
                return true;
            }

            foreach (SwiDiagObj swiDiagObj in diagObjectsList)
            {
                if (IsDiagObjectValid(swiDiagObj.Id, vehicle, ffmDynamicResolver))
                {
                    log.InfoFormat("IsInfoObjectValid IsDiagObjectValid Valid: '{0}'", true);
                    return true;
                }
            }

            log.ErrorFormat("IsInfoObjectValid Valid: '{0}'", false);
            return false;
        }

        public string GetInfoObjectObjectId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            string controlId = string.Empty;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT CONTROLID FROM XEP_INFOOBJECTS WHERE ID = {0}", id);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            controlId = reader["CONTROLID"].ToString()?.Trim();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetInfoObjectObjectId Exception: '{0}'", e.Message);
                return null;
            }

            return controlId;
        }

        public string GetInfoObjectIdByIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            string controlId = string.Empty;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID FROM XEP_INFOOBJECTS WHERE IDENTIFIER = '{0}'", identifier);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            controlId = reader["ID"].ToString()?.Trim();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetInfoObjectIdByIdentifier Exception: '{0}'", e.Message);
                return null;
            }

            return controlId;
        }

        public List<SwiDiagObj> GetDiagObjectsForInfoObject(string infoObjectId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, bool getHidden)
        {
            if (string.IsNullOrEmpty(infoObjectId))
            {
                return null;
            }

            List<SwiDiagObj> diagObjectsList = new List<SwiDiagObj>();
            try
            {
                List<string> idList = new List<string>();
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID FROM XEP_REFINFOOBJECTS WHERE INFOOBJECTID = {0} AND (LINK_TYPE_ID = 'DiagobjDocumentLink' OR LINK_TYPE_ID = 'DiagobjServiceprogramLink')", infoObjectId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string item = reader["ID"].ToString()?.Trim();
                            if (!string.IsNullOrEmpty(item))
                            {
                                idList.AddIfNotContains(item);
                            }
                        }
                    }
                }

                foreach (string item in idList)
                {
                    List<SwiDiagObj> diagObjectsByControlId = GetDiagObjectsByControlId(item, vehicle, ffmDynamicResolver, getHidden);
                    if (diagObjectsByControlId != null)
                    {
                        diagObjectsList.AddRangeIfNotContains(diagObjectsByControlId);
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDiagObjectsForInfoObject Exception: '{0}'", e.Message);
                return null;
            }

            return diagObjectsList;
        }

        public SwiInfoObj GetInfoObjectByControlId(string controlId, SwiInfoObj.SwiActionDatabaseLinkType? linkType = null)
        {
            if (string.IsNullOrEmpty(controlId))
            {
                return null;
            }

            SwiInfoObj swiInfoObj = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NODECLASS, ASSEMBLY, VERSIONNUMBER, PROGRAMTYPE, SICHERHEITSRELEVANT, TITLEID, " +
                    DatabaseFunctions.SqlTitleItems + ", GENERELL, TELESERVICEKENNUNG, FAHRZEUGKOMMUNIKATION, MESSTECHNIK, VERSTECKT, NAME, INFORMATIONSTYP, " +
                    @"IDENTIFIKATOR, INFORMATIONSFORMAT, SINUMMER, ZIELISTUFE, CONTROLID, INFOTYPE, INFOFORMAT, DOCNUMBER, PRIORITY, IDENTIFIER, FLOWXML FROM XEP_INFOOBJECTS WHERE XEP_INFOOBJECTS.CONTROLID = {0}",
                    controlId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            swiInfoObj = ReadXepSwiInfoObj(reader, linkType);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetInfoObjectByControlId Exception: '{0}'", e.Message);
                return null;
            }

            return swiInfoObj;
        }

        public List<SwiInfoObj> CollectInfoObjectsForDiagObject(SwiDiagObj diagObject, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, List<string> typeFilter = null, bool validateInfoObject = false)
        {
            List<SwiInfoObj> swiInfoObjList = new List<SwiInfoObj>();
            if (diagObject.ControlId != null)
            {
                List<SwiInfoObj> infoObjs = GetInfoObjectsByDiagObjectControlId(diagObject.ControlId, validateInfoObject ? vehicle : null, null, getHidden: true, typeFilter);
                diagObject.InfoObjects = infoObjs;
                if (infoObjs != null)
                {
                    swiInfoObjList.AddRange(infoObjs);
                }

                List<SwiDiagObj> diagObjsChild = GetChildDiagObjects(diagObject, vehicle, ffmDynamicResolver, true);
                diagObject.Children = diagObjsChild;
                foreach (SwiDiagObj diagObjChild in diagObjsChild)
                {
                    List<SwiInfoObj> infoObjsChild = CollectInfoObjectsForDiagObject(diagObjChild, vehicle, ffmDynamicResolver, typeFilter, validateInfoObject);
                    if (infoObjsChild != null)
                    {
                        swiInfoObjList.AddRange(infoObjsChild);
                    }
                }
            }

            return swiInfoObjList;
        }

        public List<SwiDiagObj> GetInfoObjectsTreeForNodeclassName(string nodeclassName, Vehicle vehicle = null, List<string> typeFilter = null, bool validateInfoObject = false)
        {
            log.InfoFormat("GetInfoObjectsTreeForNodeclassName NodeClass: {0}", nodeclassName);

            DateTime startTime = DateTime.Now;
            List<SwiDiagObj> diagObjsRoot = GetDiagObjectsByNodeclassName(nodeclassName);
            if (diagObjsRoot != null)
            {
                foreach (SwiDiagObj swiDiagObj in diagObjsRoot)
                {
                    List<SwiDiagObj> diagObjsChild = GetChildDiagObjects(swiDiagObj);
                    if (diagObjsChild != null)
                    {
                        foreach (SwiDiagObj swiDiagObjChild in diagObjsChild)
                        {
                            List<SwiInfoObj> swiInfoObjs = CollectInfoObjectsForDiagObject(swiDiagObjChild, vehicle, null, typeFilter, validateInfoObject);
                            if (swiInfoObjs == null)
                            {
                                log.InfoFormat("GetInfoObjectsTreeForNodeclassName get info objects failed: {0}", nodeclassName);
                            }
                        }
                    }
                }
            }

            DateTime endTime = DateTime.Now;
            log.InfoFormat("GetInfoObjectsTreeForNodeclassName Update time: {0}s", (endTime - startTime).Seconds);
            return diagObjsRoot;
        }

        public List<SwiDiagObj> GetDiagObjectsByNodeclassName(string nodeclassName)
        {
            if (string.IsNullOrEmpty(nodeclassName))
            {
                return null;
            }

            log.InfoFormat("GetDiagObjectsByNodeclassName NodeClass: {0}", nodeclassName);
            List<SwiDiagObj> swiDiagObjs = new List<SwiDiagObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT " + DiagObjectItems + 
                    @" FROM XEP_DIAGNOSISOBJECTS WHERE (NODECLASS IN (SELECT ID FROM XEP_NODECLASSES WHERE (NAME = '{0}')))",
                    nodeclassName);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiDiagObj swiDiagObj = ReadXepSwiDiagObj(reader);
                            if (swiDiagObj != null)
                            {
                                swiDiagObjs.Add(swiDiagObj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDiagObjectsByNodeclassName Exception: '{0}'", e.Message);
                return null;
            }

            return swiDiagObjs;
        }

        public List<SwiDiagObj> GetChildDiagObjects(SwiDiagObj parentDiagnosisObject)
        {
            if (parentDiagnosisObject == null)
            {
                return null;
            }

            log.InfoFormat("GetChildDiagObjects Id: {0}, ControlId: {1}", parentDiagnosisObject.Id, parentDiagnosisObject.ControlId);
            string controlId = parentDiagnosisObject.ControlId;
            if (string.IsNullOrEmpty(controlId) || controlId.ConvertToInt(-1) == 0)
            {
                controlId = parentDiagnosisObject.Id;
            }

            List<SwiDiagObj> swiDiagObjs = new List<SwiDiagObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT " + DiagObjectItems +
                    @" FROM XEP_DIAGNOSISOBJECTS WHERE XEP_DIAGNOSISOBJECTS.CONTROLID IN (SELECT DIAGNOSISOBJECTCONTROLID FROM XEP_REFDIAGNOSISTREE WHERE ID = {0})",
                    controlId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiDiagObj swiDiagObj = ReadXepSwiDiagObj(reader);
                            if (swiDiagObj != null)
                            {
                                swiDiagObjs.Add(swiDiagObj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetChildDiagObjects Exception: '{0}'", e.Message);
                return null;
            }

            parentDiagnosisObject.Children = swiDiagObjs;
            return swiDiagObjs;
        }

        public List<SwiDiagObj> GetChildDiagObjects(SwiDiagObj diagnosisObject, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, bool getHidden)
        {
            if (diagnosisObject == null)
            {
                return null;
            }

            log.InfoFormat("GetChildDiagObjects ControlId: {0}", diagnosisObject.ControlId);
            string controlId = diagnosisObject.ControlId;
            List<SwiDiagObj> swiDiagObjs = new List<SwiDiagObj>();
            try
            {
                string hiddenRule = string.Empty;
                if (!getHidden)
                {
                    hiddenRule = " AND VERSTECKT = 0";
                }

                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT " + DiagObjectItems +
                    @" FROM XEP_DIAGNOSISOBJECTS WHERE XEP_DIAGNOSISOBJECTS.CONTROLID IN (SELECT DIAGNOSISOBJECTCONTROLID FROM XEP_REFDIAGNOSISTREE WHERE ID = {0}{1})",
                    controlId, hiddenRule);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiDiagObj swiDiagObj = ReadXepSwiDiagObj(reader);
                            if (swiDiagObj != null)
                            {
                                if (vehicle != null)
                                {
                                    if (IsDiagObjectValid(swiDiagObj.Id, vehicle, ffmDynamicResolver))
                                    {
                                        swiDiagObjs.Add(swiDiagObj);
                                    }
                                }
                                else
                                {
                                    swiDiagObjs.Add(swiDiagObj);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetChildDiagObjects Exception: '{0}'", e.Message);
                return null;
            }

            return swiDiagObjs;
        }

        public SwiDiagObj GetDiagObjectById(string diagObjectId)
        {
            if (string.IsNullOrEmpty(diagObjectId))
            {
                return null;
            }

            log.InfoFormat("GetDiagObjectById Id: {0}", diagObjectId);
            SwiDiagObj swiDiagObj = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT " + DiagObjectItems +
                    @" FROM XEP_DIAGNOSISOBJECTS WHERE (ID = {0})",
                    diagObjectId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            swiDiagObj = ReadXepSwiDiagObj(reader);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDiagObjectById Exception: '{0}'", e.Message);
                return null;
            }

            return swiDiagObj;
        }

        public List<SwiDiagObj> GetDiagObjectsByControlId(string controlId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, bool getHidden)
        {
            if (string.IsNullOrEmpty(controlId))
            {
                return null;
            }

            log.InfoFormat("GetDiagObjectsByControlId Id: {0}, Hidden: {1}", controlId, getHidden);
            List<SwiDiagObj> swiDiagObjs = new List<SwiDiagObj>();
            try
            {
                string hiddenRule = string.Empty;
                if (!getHidden)
                {
                    hiddenRule = " AND VERSTECKT = 0";
                }

                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT " + DiagObjectItems +
                    @" FROM XEP_DIAGNOSISOBJECTS WHERE (CONTROLID = {0}{1})",
                    controlId, hiddenRule);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiDiagObj swiDiagObj = ReadXepSwiDiagObj(reader);
                            if (vehicle != null)
                            {
                                if (IsDiagObjectValid(swiDiagObj.Id, vehicle, ffmDynamicResolver))
                                {
                                    swiDiagObjs.Add(swiDiagObj);
                                }
                            }
                            else
                            {
                                swiDiagObjs.Add(swiDiagObj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDiagObjectsByControlId Exception: '{0}'", e.Message);
                return null;
            }

            return swiDiagObjs;
        }

        public bool IsDiagObjectValid(string diagObjectId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("IsDiagObjectValid Id: {0}", diagObjectId);
            if (vehicle == null)
            {
                log.InfoFormat("IsDiagObjectValid No vehicle, Valid: {0}", true);
                return true;
            }

            string diagObjectObjectId = GetDiagObjectObjectId(diagObjectId);
            if (!EvaluateXepRulesById(diagObjectId, vehicle, ffmDynamicResolver, diagObjectObjectId))
            {
                log.InfoFormat("IsDiagObjectValid Rule failed, Valid: {0}", false);
                return false;
            }

            string diagObjectControlIdForDiagObjectId = GetDiagObjectControlIdForDiagObjectId(diagObjectId);
            if (diagObjectControlIdForDiagObjectId != null && diagObjectControlIdForDiagObjectId.ConvertToInt(-1) == 0)
            {
                log.InfoFormat("IsDiagObjectValid Control id zero, Valid: {0}", true);
                return true;
            }

            if (UseIsAtLeastOnePathToRootValid)
            {
                if (this.IsAtLeastOnePathToRootValid(diagObjectControlIdForDiagObjectId, vehicle, ffmDynamicResolver))
                {
                    log.InfoFormat("IsDiagObjectValid One parent root valid, Valid: {0}", true);
                    return true;
                }
            }
            else
            {
                if (AreAllParentDiagObjectsValid(diagObjectControlIdForDiagObjectId, vehicle, ffmDynamicResolver))
                {
                    log.InfoFormat("IsDiagObjectValid All parents valid, Valid: {0}", true);
                    return true;
                }
            }

            log.InfoFormat("IsDiagObjectValid Valid: {0}", false);
            return false;
        }

        public bool IsAtLeastOnePathToRootValid(string currentDiagObjectControlId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("IsAtLeastOnePathToRootValid Id: {0}", currentDiagObjectControlId);
            List<string> idListParent = GetParentDiagObjectControlIdsForControlId(currentDiagObjectControlId);
            if (idListParent == null)
            {
                log.InfoFormat("IsAtLeastOnePathToRootValid No parent diag objects, Valid: {0}", false);
                return false;
            }

            foreach (string parentId in idListParent)
            {
                if (IsRootDiagnosisObject(parentId))
                {
                    return true;
                }

                SwiDiagObj swiDiagObj = GetDiagObjectsByControlId(parentId, null, null, getHidden: true).FirstOrDefault();
                if (swiDiagObj == null)
                {
                    log.InfoFormat("AreAllParentDiagObjectsValid No diag control id");
                }
                else
                {
                    if (!string.IsNullOrEmpty(swiDiagObj.ControlId) && EvaluateXepRulesById(swiDiagObj.Id, vehicle, ffmDynamicResolver, parentId))
                    {
                        if (IsAtLeastOnePathToRootValid(swiDiagObj.ControlId, vehicle, ffmDynamicResolver))
                        {
                            log.InfoFormat("IsAtLeastOnePathToRootValid -> IsAtLeastOnePathToRootValid, Valid: {0}", true);
                            return true;
                        }
                    }
                }
            }

            log.InfoFormat("IsAtLeastOnePathToRootValid Valid: {0}", false);
            return false;
        }

        public bool AreAllParentDiagObjectsValid(string currentDiagObjectControlId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("AreAllParentDiagObjectsValid Id: {0}", currentDiagObjectControlId);
            if (currentDiagObjectControlId != null && currentDiagObjectControlId.ConvertToInt(-1) == 0)
            {
                log.InfoFormat("AreAllParentDiagObjectsValid Id zero, Valid: {0}", true);
                return true;
            }

            List<string> idListParent = GetParentDiagObjectControlIdsForControlId(currentDiagObjectControlId);
            if (idListParent == null || idListParent.Count == 0)
            {
                log.InfoFormat("AreAllParentDiagObjectsValid No parent diag objects, Valid: {0}", true);
                return true;
            }

            HashSet<SwiDiagObj> swiDiagObjHash = new HashSet<SwiDiagObj>();
            foreach (string parentId in idListParent)
            {
                if (parentId != null && parentId.ConvertToInt(-1) == 0)
                {
                    log.InfoFormat("AreAllParentDiagObjectsValid Parent id zero, Valid: {0}", true);
                    return true;
                }

                SwiDiagObj swiDiagObj = GetDiagObjectsByControlId(parentId, null, null, getHidden: true).FirstOrDefault();
                if (swiDiagObj == null)
                {
                    log.InfoFormat("AreAllParentDiagObjectsValid No diag control id, Valid: {0}", true);
                    return true;
                }

                if (IsRootDiagnosisObject(swiDiagObj))
                {
                    log.InfoFormat("AreAllParentDiagObjectsValid Root diag object, Valid: {0}", true);
                    return true;
                }

                if (!string.IsNullOrEmpty(swiDiagObj.ControlId) && EvaluateXepRulesById(swiDiagObj.Id, vehicle, ffmDynamicResolver))
                {
                    swiDiagObjHash.AddIfNotContains(swiDiagObj);
                }
            }

            if (swiDiagObjHash.Count > 0)
            {
                foreach (SwiDiagObj swiDiagObj in swiDiagObjHash)
                {
                    if (!string.IsNullOrEmpty(swiDiagObj.ControlId) && AreAllParentDiagObjectsValid(swiDiagObj.ControlId, vehicle, ffmDynamicResolver))
                    {
                        log.InfoFormat("AreAllParentDiagObjectsValid Sub parent objects valid, Valid: {0}", true);
                        return true;
                    }
                }
            }

            log.InfoFormat("AreAllParentDiagObjectsValid Valid: {0}", false);
            return false;
        }

        private SwiActionLinkType? MapLinkTypeDatabaseToApplication(SwiInfoObj.SwiActionDatabaseLinkType? swiActionDatabaseLinkType, bool contextHddUpdate = false)
        {
            if (swiActionDatabaseLinkType == null)
            {
                return null;
            }

            switch (swiActionDatabaseLinkType)
            {
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionVehiclePreparingLink:
                    return SwiActionLinkType.MVF;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionEcuPreparingLink:
                    return SwiActionLinkType.MVS;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionVehiclePostprocessingLink:
                    return SwiActionLinkType.MNF;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionEcuPostprocessingLink:
                    return SwiActionLinkType.MNS;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionPlanLink:
                    return SwiActionLinkType.MPB;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionPreparingHintsLink:
                    return SwiActionLinkType.MHV;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionPostprocessingHintsLink:
                    return SwiActionLinkType.MHN;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionCheckLink:
                    return SwiActionLinkType.PRF;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink:
                    if (contextHddUpdate)
                    {
                        return SwiActionLinkType.HDD;
                    }
                    return SwiActionLinkType.AUS;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiActionDiagnosticLink:
                    return SwiActionLinkType.SwiActionDiagnosticLink;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiactionEscalationPreparingGeneralLink:
                    return SwiActionLinkType.ESK_VA;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiactionEscalationPreparingVehicleLink:
                    return SwiActionLinkType.ESK_VF;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiactionEscalationPreparingEcuLink:
                    return SwiActionLinkType.ESK_VS;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiactionEscalationActionplanCalculationLink:
                    return SwiActionLinkType.ESK_MPB;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiactionEscalationPreconditionCheckLink:
                    return SwiActionLinkType.ESK_PRF;
                case SwiInfoObj.SwiActionDatabaseLinkType.SwiactionSpecialActionplanLink:
                    return SwiActionLinkType.SMP;
                default:
                    return null;
            }
        }

        private bool UpdateDiagObjectRootNodes()
        {
            if (_diagObjRootNodes != null && _diagObjRootNodeIdSet != null)
            {
                return true;
            }

            log.InfoFormat("UpdateDiagObjectRootNodes");
            List<SwiDiagObj> diagObjRootNodes = new List<SwiDiagObj>();
            HashSet<string> diagObjRootNodeIdSet = new HashSet<string>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT " + DiagObjectItems +
                    @" FROM XEP_DIAGNOSISOBJECTS WHERE (CONTROLID == 0)");
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiDiagObj swiDiagObj = ReadXepSwiDiagObj(reader);
                            if (swiDiagObj != null)
                            {
                                diagObjRootNodes.Add(swiDiagObj);
                                if (!string.IsNullOrEmpty(swiDiagObj.Id))
                                {
                                    diagObjRootNodeIdSet.Add(swiDiagObj.Id);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("UpdateDiagObjectRootNodes Exception: '{0}'", e.Message);
                return false;
            }

            log.InfoFormat("UpdateDiagObjectRootNodes Count: {0}", diagObjRootNodes.Count);
            _diagObjRootNodes = diagObjRootNodes;
            _diagObjRootNodeIdSet = diagObjRootNodeIdSet;

            return true;
        }

        private List<SwiDiagObj> GetAllDiagObjectRootNodes()
        {
            UpdateDiagObjectRootNodes();
            return _diagObjRootNodes;
        }

        private HashSet<string> GetDiagObjectRootNodeIds()
        {
            UpdateDiagObjectRootNodes();
            return _diagObjRootNodeIdSet;
        }

        private bool IsRootDiagnosisObject(SwiDiagObj diagObject)
        {
            if (diagObject == null || string.IsNullOrEmpty(diagObject.Id))
            {
                log.Error("IsRootDiagnosisObject, No diag object");
                return false;
            }

            return IsRootDiagnosisObject(diagObject.Id);
        }

        private bool IsRootDiagnosisObject(string objId)
        {
            if (string.IsNullOrEmpty(objId))
            {
                log.Error("IsRootDiagnosisObject, Object ID invalid");
                return false;
            }

            HashSet<string> diagObjRootNodeIdSet = GetDiagObjectRootNodeIds();
            if (diagObjRootNodeIdSet != null)
            {
                bool result = diagObjRootNodeIdSet.Contains(objId);
                log.InfoFormat("IsRootDiagnosisObject, Is root object: {0}", result);
                return result;
            }

            List<SwiDiagObj> diagObjRootNodes = GetAllDiagObjectRootNodes();
            if (diagObjRootNodes == null)
            {
                log.Error("IsRootDiagnosisObject, No root nodes");
                return false;
            }

            foreach (SwiDiagObj allDiagObjectRootNode in diagObjRootNodes)
            {
                if (allDiagObjectRootNode.Id == objId)
                {
                    log.InfoFormat("IsRootDiagnosisObject, Root object: {0}", objId);
                    return true;
                }
            }

            log.InfoFormat("IsRootDiagnosisObject, No root object: {0}", objId);
            return false;
        }

        private List<string> GetParentDiagObjectControlIdsForControlId(string controlId)
        {
            log.InfoFormat("GetParentDiagObjectControlIdsForControlId Id: {0}", controlId);
            if (string.IsNullOrEmpty(controlId))
            {
                return null;
            }

            List<string> idList = new List<string>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID FROM XEP_REFDIAGNOSISTREE WHERE (DIAGNOSISOBJECTCONTROLID = {0})", controlId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString()?.Trim();
                            idList.Add(id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetParentDiagObjectControlIdsForControlId Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetParentDiagObjectControlIdsForControlId IdList: {0}", idList.Count);
            return idList;
        }

        public string GetDiagObjectObjectId(string diagObjectId)
        {
            string controlId = GetDiagObjectControlIdForDiagObjectId(diagObjectId);
            if (string.IsNullOrEmpty(controlId))
            {
                controlId = diagObjectId;
            }

            return controlId;
        }

        public string GetDiagObjectControlIdForDiagObjectId(string diagObjectId)
        {
            log.InfoFormat("GetDiagObjectControlIdForDiagObjectId Id: {0}", diagObjectId);
            if (string.IsNullOrEmpty(diagObjectId))
            {
                return null;
            }

            string controlId = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT CONTROLID FROM XEP_DIAGNOSISOBJECTS WHERE (ID = {0})", diagObjectId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            controlId = reader["CONTROLID"].ToString()?.Trim();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDiagObjectControlIdForDiagObjectId Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetDiagObjectControlIdForDiagObjectId ControlId: {0}", controlId);
            return controlId;
        }

        public string GetDiagnosisCode(string diagnosisCodeId)
        {
            log.InfoFormat("GetDiagnosisCode Id: {0}", diagnosisCodeId);
            if (string.IsNullOrEmpty(diagnosisCodeId))
            {
                return null;
            }

            string diagnosisCode = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME FROM XEP_DIAGCODE WHERE (ID = {0})", diagnosisCodeId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            diagnosisCode = reader["NAME"].ToString()?.Trim();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDiagnosisCode Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetDiagnosisCode DiagCode: {0}", diagnosisCode);
            return diagnosisCode;
        }

        public Dictionary<string, XepRule> LoadXepRules()
        {
            log.InfoFormat("LoadXepRules");
            Dictionary<string, XepRule> xepRuleDict = new Dictionary<string, XepRule>();
            try
            {
                string sql = @"SELECT ID, RULE FROM XEP_RULES";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString()?.Trim();
                            byte[] rule = (byte[])reader["RULE"];
                            XepRule xepRule = new XepRule(id, rule);
                            xepRuleDict.Add(id, xepRule);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("LoadXepRules Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("LoadXepRules Count: {0}", xepRuleDict.Count);
            return xepRuleDict;
        }

        public XepRule GetRuleById(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                return null;
            }

            if (_xepRuleDict == null)
            {
                _xepRuleDict = LoadXepRules();
            }

            if (_xepRuleDict == null)
            {
                log.InfoFormat("GetRuleById No rules present");
                return null;
            }

            if (_xepRuleDict.TryGetValue(ruleId, out XepRule xepRule))
            {
                return xepRule;
            }

            return null;
        }

        public string GetEcuCharacteristicsXml(string storedXmlFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(storedXmlFileName))
                {
                    log.ErrorFormat("GetEcuCharacteristicsXml No file name");
                    return null;
                }

                if (EcuCharacteristicsStorage?.EcuXmlDict == null)
                {
                    log.ErrorFormat("GetEcuCharacteristicsXml No storage");
                    return null;
                }

                string key = Path.GetFileNameWithoutExtension(storedXmlFileName);
                if (!EcuCharacteristicsStorage.EcuXmlDict.TryGetValue(key.ToUpperInvariant(), out string xml))
                {
                    log.ErrorFormat("GetEcuCharacteristicsXml Key not found {0}", key);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(xml))
                {
                    log.ErrorFormat("GetEcuCharacteristicsXml Empty");
                    return null;
                }

                log.InfoFormat("GetEcuCharacteristicsXml Valid");
                return xml;
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetEcuCharacteristicsXml Exception: {0}", e.Message);
                return null;
            }
        }

        public List<BordnetsData> LoadBordnetsData(Vehicle vecInfo = null)
        {
            log.InfoFormat("LoadBordnetsData: VecInfo={0}", vecInfo != null);
            List<BordnetsData> boardnetsList = new List<BordnetsData>();
            try
            {
                string sql = "SELECT I.ID AS INFOOBJECTID, I.IDENTIFIER AS INFOOBJECTIDENTIFIER, C.CONTENT_DEDE AS CONTENT_DEDE FROM XEP_INFOOBJECTS I " +
                             "INNER JOIN XEP_REFCONTENTS R ON R.ID = I.CONTROLID " +
                             "INNER JOIN XEP_IOCONTENTS C ON C.CONTROLID = R.CONTENTCONTROLID WHERE I.IDENTIFIER LIKE 'BNT-XML-%'";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string infoObjId = reader["INFOOBJECTID"].ToString()?.Trim();
                            string infoObjIdent = reader["INFOOBJECTIDENTIFIER"].ToString()?.Trim();
                            string docId = reader["CONTENT_DEDE"].ToString()?.Trim();
                            BordnetsData bordnetsData = new BordnetsData(infoObjId, infoObjIdent, docId);
                            boardnetsList.Add(bordnetsData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("LoadBordnetsData Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("LoadBordnetsData Count: {0}", boardnetsList.Count);
            if (boardnetsList.Count == 0)
            {
                log.ErrorFormat("LoadBordnetsData No data");
                return null;
            }

            List<BordnetsData> boardnetsList2 = new List<BordnetsData>();
            foreach (BordnetsData bordnetsData in boardnetsList)
            {
                if (vecInfo == null || EvaluateXepRulesById(bordnetsData.InfoObjId, vecInfo, null))
                {
                    bordnetsData.DocData = GetXmlValuePrimitivesById(bordnetsData.DocId, "DEDE");
                    if (!string.IsNullOrWhiteSpace(bordnetsData.DocData))
                    {
                        log.InfoFormat("LoadBordnetsData Added: '{0}'", bordnetsData.InfoObjIdent);
                        boardnetsList2.Add(bordnetsData);
                    }
                }
            }

            log.InfoFormat("LoadBordnetsData Count filter: {0}", boardnetsList2.Count);
            if (boardnetsList2.Count == 0)
            {
                log.ErrorFormat("LoadBordnetsData No filter data");
                return null;
            }

            return boardnetsList2;
        }

        public List<BordnetsData> GetAllBordnetRules()
        {
            List<BordnetsData> boardnetsList = LoadBordnetsData();
            if (boardnetsList == null)
            {
                log.ErrorFormat("GetAllBordnetRules No data");
                return null;
            }

            foreach (BordnetsData bordnetsData in boardnetsList)
            {
                XepRule xepRule = GetRuleById(bordnetsData.InfoObjId);
                if (xepRule == null)
                {
                    log.ErrorFormat("GetAllBordnetFromDatabase No rule for: {0}", bordnetsData.ToString());
                }
                else
                {
                    bordnetsData.XepRule = xepRule;
                }
            }

            log.InfoFormat("GetAllBordnetRules Count: {0}", boardnetsList.Count);
            return boardnetsList;
        }


        public string LookupVehicleCharDeDeById(string characteristicId)
        {
            log.InfoFormat("LookupVehicleCharDeDeById Id: {0}", characteristicId);
            if (string.IsNullOrEmpty(characteristicId))
            {
                return null;
            }

            string titleDe = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, TITLE_DEDE FROM XEP_CHARACTERISTICS WHERE (ID = {0})", characteristicId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            titleDe = reader["TITLE_DEDE"].ToString()?.Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("LookupVehicleCharDeDeById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("LookupVehicleCharDeDeById Title: {0}", titleDe);
            return titleDe;
        }

        public decimal LookupVehicleCharIdByName(string name, decimal? nodeclassValue)
        {
            if (!nodeclassValue.HasValue)
            {
                return 0;
            }

            string charId = LookupVehicleCharIdByName(name, nodeclassValue.Value.ToString(CultureInfo.InvariantCulture));
            if (string.IsNullOrEmpty(charId))
            {
                return 0;
            }

            if (!decimal.TryParse(charId, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal value))
            {
                return 0;
            }

            return value;
        }

        private string LookupVehicleCharIdByName(string name, string nodeclass)
        {
            log.InfoFormat("LookupVehicleCharIdByName Id: {0} Class: {1}", name, nodeclass);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            string charId = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NODECLASS, NAME FROM XEP_CHARACTERISTICS WHERE (NAME = '{0}')", name);
                if (!string.IsNullOrEmpty(nodeclass))
                {
                    sql += string.Format(CultureInfo.InvariantCulture, @" AND (NODECLASS = {0})", nodeclass);
                }
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            charId = reader["ID"].ToString()?.Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("LookupVehicleCharIdByName Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("LookupVehicleCharIdByName CharId: {0}", charId);
            return charId;
        }

        public string GetTypeKeyId(string typeKey, bool isAlpina)
        {
            log.InfoFormat("GetTypeKeyId Key: {0}, Alpina: {1}", typeKey, isAlpina);
            if (string.IsNullOrEmpty(typeKey))
            {
                return null;
            }

            string typeId = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID FROM XEP_CHARACTERISTICS WHERE (NAME = '{0}') AND (NODECLASS = {1})", typeKey, _typeKeyClassId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            typeId = reader["ID"].ToString()?.Trim();
                            if (isAlpina)
                            {
                                typeId = GetAlpinaTypeKeyId(typeId);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTypeKeyId Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetTypeKeyId TypeId: {0}", typeId);
            return typeId;
        }

        public string GetAlpinaTypeKeyId(string typeId)
        {
            string alpinaId = GetTypeKeyMapping(typeId);
            if (!string.IsNullOrEmpty(alpinaId))
            {
                return alpinaId;
            }

            return alpinaId;
        }

        public string GetTypeKeyMapping(string typeId)
        {
            log.InfoFormat("GetTypeKeyMapping Id: {0}", typeId);
            if (string.IsNullOrEmpty(typeId))
            {
                return null;
            }

            string alpinaId = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, ALPINA_ID FROM XEP_TYPEKEY_MAPPING WHERE (ID = {0})", typeId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            alpinaId = reader["ALPINA_ID"].ToString()?.Trim();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTypeKeyMapping Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetTypeKeyMapping ILevel: '{0}'", alpinaId);
            return alpinaId;
        }

        public string GetIStufeById(string iStufenId)
        {
            log.InfoFormat("GetIStufeById Id: {0}", iStufenId);
            if (string.IsNullOrEmpty(iStufenId))
            {
                return null;
            }

            string iLevel = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME FROM XEP_ISTUFEN WHERE (ID = {0})", iStufenId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            iLevel = reader["NAME"].ToString()?.Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetIStufeById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetIStufeById ILevel: '{0}'", iLevel);
            return iLevel;
        }

        public string GetCountryById(string countryId)
        {
            log.InfoFormat("GetCountryById Id: {0}", countryId);
            if (string.IsNullOrEmpty(countryId))
            {
                return null;
            }

            string country = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, LAENDERKUERZEL FROM XEP_COUNTRIES WHERE (ID = {0})", countryId);
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            country = reader["LAENDERKUERZEL"].ToString()?.Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetCountryById Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetCountryById Country: {0}", country);
            return country;
        }

        public DbInfo GetDbInfo()
        {
            log.InfoFormat("GetDbVersion");

            DbInfo dbInfo = null;
            try
            {
                string sql = @"SELECT VERSION, CREATIONDATE FROM RG_VERSION";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string version = reader["VERSION"].ToString()?.Trim();
                            DateTime dateTime = reader.GetDateTime(1);
                            dbInfo = new DbInfo(version, dateTime);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetDbVersion Exception: '{0}'", e.Message);
                return null;
            }

            return dbInfo;
        }

        public static string GetSwiVersion()
        {
            log.InfoFormat("GetSwiVersion");
            string swiVersion = string.Empty;
            try
            {
                RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey istaKey = baseKey.OpenSubKey(@"SOFTWARE\BMWGroup\ISPI\ISTA");
                if (istaKey != null)
                {
                    string swiData = istaKey.GetValue("SWIData") as string;
                    if (!string.IsNullOrEmpty(swiData))
                    {
                        swiVersion = swiData;
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiVersion Exception: '{0}'", e.Message);
                return null;
            }

            log.InfoFormat("GetSwiVersion Ver: {0}", swiVersion);
            return swiVersion;
        }

        public static string NormalizePath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                return Path.GetFullPath(new Uri(path).LocalPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool PathStartWith(string fullPath, string subPath)
        {
            try
            {
                string fullPathNorm = NormalizePath(fullPath);
                string subPathNorm = NormalizePath(subPath);
                if (string.IsNullOrEmpty(fullPathNorm) || string.IsNullOrEmpty(subPathNorm))
                {
                    return false;
                }

                if (fullPathNorm.IndexOf(subPathNorm, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }


        public bool EvaluateXepRulesById(string id, Vehicle vehicle, IFFMDynamicResolver ffmResolver, string objectId = null)
        {
            // objectId is only required for patch rules
            log.InfoFormat("EvaluateXepRulesById Id: {0}, ObjectId: {1}", id, objectId ?? "-");
            if (vehicle == null)
            {
                log.WarnFormat("EvaluateXepRulesById No vehicle");
                return true;
            }

            XepRule xepRule = GetRuleById(id);
            bool result = true;
            if (xepRule != null)
            {
                result = xepRule.EvaluateRule(vehicle, ffmResolver);
            }
            else
            {
                log.InfoFormat("EvaluateXepRulesById No rule found for Id: {0}", id);
            }

            log.InfoFormat("EvaluateXepRulesById Id: {0}, Result: {1}", id, result);
            return result;
        }

        private static Equipment ReadXepEquipment(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            return new Equipment(id, name, GetTranslation(reader));
        }

        private static EcuClique ReadXepEcuClique(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string cliqueName = reader["CLIQUENKURZBEZEICHNUNG"].ToString()?.Trim();
            string ecuRepId = reader["ECUREPID"].ToString()?.Trim();
            return new EcuClique(id, cliqueName, ecuRepId, GetTranslation(reader));
        }

        private static EcuRefClique ReadXepEcuRefClique(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string ecuCliqueId = reader["ECUCLIQUEID"].ToString()?.Trim();
            return new EcuRefClique(id, ecuCliqueId);
        }

        private static CharacteristicRoots ReadXepCharacteristicRoots(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string nodeClass = reader["NODECLASS"].ToString()?.Trim();
            string motorCycSeq = reader["MOTORCYCLESEQUENCE"].ToString()?.Trim();
            string vehicleSeq = reader["VEHICLESEQUENCE"].ToString()?.Trim();
            return new CharacteristicRoots(id, nodeClass, motorCycSeq, vehicleSeq, GetTranslation(reader));
        }

        private static Characteristics ReadXepCharacteristics(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string nodeClass = reader["NODECLASS"].ToString()?.Trim();
            string titleId = reader["TITLEID"].ToString()?.Trim();
            string istaVisible = reader["ISTA_VISIBLE"].ToString()?.Trim();
            string staticClassVar = reader["STATICCLASSVARIABLES"].ToString()?.Trim();
            string staticClassVarMCycle = reader["STATICCLASSVARIABLESMOTORRAD"].ToString()?.Trim();
            string parentId = reader["PARENTID"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string legacyName = reader["LEGACY_NAME"].ToString()?.Trim();
            return new Characteristics(id, nodeClass, titleId, istaVisible, staticClassVar, staticClassVarMCycle, parentId, name, legacyName, GetTranslation(reader));
        }

        private static VinRanges ReadXepVinRanges(SqliteDataReader reader)
        {
            string changeDate = reader["CHANGEDATE"].ToString()?.Trim();
            string productionMonth = reader["PRODUCTIONDATEMONTH"].ToString()?.Trim();
            string productionYear = reader["PRODUCTIONDATEYEAR"].ToString()?.Trim();
            string releaseState = reader["RELEASESTATE"].ToString()?.Trim();
            string typeKey = reader["TYPSCHLUESSEL"].ToString()?.Trim();
            string vinBandFrom = reader["VINBANDFROM"].ToString()?.Trim();
            string vinBandTo = reader["VINBANDTO"].ToString()?.Trim();
            string gearboxType = reader["GEARBOX_TYPE"].ToString()?.Trim();
            string vin17_4_7 = reader["VIN17_4_7"].ToString()?.Trim();
            return new VinRanges(changeDate, productionMonth, productionYear, releaseState, typeKey, vinBandFrom, vinBandTo, gearboxType, vin17_4_7);
        }

        private static SaLaPa ReadXepSaLaPa(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string productType = reader["PRODUCT_TYPE"].ToString()?.Trim();
            return new SaLaPa(id, name, productType, GetTranslation(reader));
        }

        private static EcuReps ReadXepEcuReps(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string ecuShortcut = reader["STEUERGERAETEKUERZEL"].ToString()?.Trim();
            return new EcuReps(id, ecuShortcut);
        }

        private static EcuVar ReadXepEcuVar(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string faultMemDelWaitTime = reader["FAULTMEMORYDELETEWAITINGTIME"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string validFrom = reader["VALIDFROM"].ToString()?.Trim();
            string validTo = reader["VALIDTO"].ToString()?.Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString()?.Trim();
            string ecuGroupId = reader["ECUGROUPID"].ToString()?.Trim();
            string sort = reader["SORT"].ToString()?.Trim();
            return new EcuVar(id, faultMemDelWaitTime, name, validFrom, validTo, safetyRelevant, ecuGroupId, sort, GetTranslation(reader));
        }

        private static EcuPrgVar ReadXepEcuPrgVar(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string flashLimit = reader["FLASHLIMIT"].ToString()?.Trim();
            string ecuVarId = reader["ECUVARIANTID"].ToString()?.Trim();
            return new EcuPrgVar(id, name, flashLimit, ecuVarId);
        }

        private static EcuGroup ReadXepEcuGroup(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string obdIdent = reader["OBDIDENTIFICATION"].ToString()?.Trim();
            string faultMemDelIdent = reader["FAULTMEMORYDELETEIDENTIFICATIO"].ToString()?.Trim();
            string faultMemDelWaitTime = reader["FAULTMEMORYDELETEWAITINGTIME"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string virt = reader["VIRTUELL"].ToString()?.Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString()?.Trim();
            string validFrom = reader["VALIDFROM"].ToString()?.Trim();
            string validTo = reader["VALIDTO"].ToString()?.Trim();
            string diagAddr = reader["DIAGNOSTIC_ADDRESS"].ToString()?.Trim();
            return new EcuGroup(id, obdIdent, faultMemDelIdent, faultMemDelWaitTime, name, virt, safetyRelevant, validFrom, validTo, diagAddr);
        }

        private static SwiAction ReadXepSwiAction(SqliteDataReader reader, SwiActionSource swiActionSource)
        {
            string id = reader["ID"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string actionCategory = reader["ACTIONCATEGORY"].ToString()?.Trim();
            string selectable = reader["SELECTABLE"].ToString()?.Trim();
            string showInPlan = reader["SHOW_IN_PLAN"].ToString()?.Trim();
            string executable = reader["EXECUTABLE"].ToString()?.Trim();
            string nodeClass = reader["NODECLASS"].ToString()?.Trim();
            return new SwiAction(swiActionSource, id, name, actionCategory, selectable, showInPlan, executable, nodeClass, GetTranslation(reader));
        }

        private static SwiRegister ReadXepSwiRegister(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string nodeClass = reader["NODECLASS"].ToString()?.Trim();
            string parentId = reader["PARENTID"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string remark = reader["REMARK"].ToString()?.Trim();
            string sort = reader["SORT"].ToString()?.Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString()?.Trim();
            string identifer = reader["IDENTIFIER"].ToString()?.Trim();
            return new SwiRegister(id, nodeClass, name, parentId, remark, sort, versionNum, identifer, GetTranslation(reader));
        }

        private static SwiInfoObj ReadXepSwiInfoObj(SqliteDataReader reader, SwiInfoObj.SwiActionDatabaseLinkType? linkType = null, SwiActionLinkType? mappedLinkType = null)
        {
            string id = reader["ID"].ToString()?.Trim();
            string nodeClass = reader["NODECLASS"].ToString()?.Trim();
            string assembly = reader["ASSEMBLY"].ToString()?.Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString()?.Trim();
            string programType = reader["PROGRAMTYPE"].ToString()?.Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString()?.Trim();
            string titleId = reader["TITLEID"].ToString()?.Trim();
            string general = reader["GENERELL"].ToString()?.Trim();
            string telSrvId = reader["TELESERVICEKENNUNG"].ToString()?.Trim();
            string vehicleComm = reader["FAHRZEUGKOMMUNIKATION"].ToString()?.Trim();
            string measurement = reader["MESSTECHNIK"].ToString()?.Trim();
            string hidden = reader["VERSTECKT"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string informationType = reader["INFORMATIONSTYP"].ToString()?.Trim();
            string identification = reader["IDENTIFIKATOR"].ToString()?.Trim();
            string informationFormat = reader["INFORMATIONSFORMAT"].ToString()?.Trim();
            string siNumber = reader["SINUMMER"].ToString()?.Trim();
            string targetILevel = reader["ZIELISTUFE"].ToString()?.Trim();
            string controlId = reader["CONTROLID"].ToString()?.Trim();
            string infoType = reader["INFOTYPE"].ToString()?.Trim();
            string infoFormat = reader["INFOFORMAT"].ToString()?.Trim();
            string docNum = reader["DOCNUMBER"].ToString()?.Trim();
            string priority = reader["PRIORITY"].ToString()?.Trim();
            string identifier = reader["IDENTIFIER"].ToString()?.Trim();
            string flowXml = reader["FLOWXML"].ToString()?.Trim();
            return new SwiInfoObj(linkType, mappedLinkType, id, nodeClass, assembly, versionNum, programType, safetyRelevant, titleId, general,
                telSrvId, vehicleComm, measurement, hidden, name, informationType, identification, informationFormat, siNumber, targetILevel, controlId,
                infoType, infoFormat, docNum, priority, identifier, flowXml, GetTranslation(reader));
        }

        private static SwiDiagObj ReadXepSwiDiagObj(SqliteDataReader reader)
        {
            string id = reader["ID"].ToString()?.Trim();
            string nodeClass = reader["NODECLASS"].ToString()?.Trim();
            string titleId = reader["TITLEID"].ToString()?.Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString()?.Trim();
            string name = reader["NAME"].ToString()?.Trim();
            string failWeight = reader["FAILUREWEIGHT"].ToString()?.Trim();
            string hidden = reader["VERSTECKT"].ToString()?.Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString()?.Trim();
            string controlId = reader["CONTROLID"].ToString()?.Trim();
            string sortOrder = reader["SORT_ORDER"].ToString()?.Trim();
            return new SwiDiagObj(id, nodeClass, titleId, versionNum, name, failWeight, hidden, safetyRelevant, controlId, sortOrder, GetTranslation(reader));
        }

        private static EcuTranslation GetTranslation(SqliteDataReader reader, string prefix = "TITLE", string language = null)
        {
            return new EcuTranslation(
                language == null || language.ToLowerInvariant() == "de" ? reader[prefix + "_DEDE"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "en" ? reader[prefix + "_ENGB"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "en" ? reader[prefix + "_ENUS"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "fr" ? reader[prefix + "_FR"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "th" ? reader[prefix + "_TH"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "sv" ? reader[prefix + "_SV"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "it" ? reader[prefix + "_IT"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "es" ? reader[prefix + "_ES"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "id" ? reader[prefix + "_ID"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ko" ? reader[prefix + "_KO"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "el" ? reader[prefix + "_EL"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "tr" ? reader[prefix + "_TR"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "zh" ? reader[prefix + "_ZHCN"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ru" ? reader[prefix + "_RU"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "nl" ? reader[prefix + "_NL"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "pt" ? reader[prefix + "_PT"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ja" ? reader[prefix + "_JA"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "cs" ? reader[prefix + "_CSCZ"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "pl" ? reader[prefix + "_PLPL"].ToString() : string.Empty
                );
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (_mDbConnection != null)
                    {
                        _mDbConnection.Close();
                        _mDbConnection.Dispose();
                        _mDbConnection = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}
