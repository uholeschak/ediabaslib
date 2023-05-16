using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BmwFileReader;
using HarmonyLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClientLibrary;

namespace PsdzClient
{
    public class PdszDatabase : IDisposable
    {
        public const string DiagObjServiceRoot = "DiagnosticObjectServicefunctionRoot";

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

        public class EcuTranslation
        {
            public EcuTranslation()
            {
                TextDe = string.Empty;
                TextEn = string.Empty;
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

            public EcuTranslation(string textDe, string textEn, string textFr, string textTh, string textSv, string textIt,
                string textEs, string textId, string textKo, string textEl, string textTr, string textZh,
                string textRu, string textNl, string textPt, string textJa, string textCs, string textPl)
            {
                TextDe = textDe;
                TextEn = textEn;
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

            public string TextDe { get; set; }
            public string TextEn { get; set; }
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
            public SwiInfoObj(SwiActionDatabaseLinkType? linkType, string id, string nodeClass, string assembly, string versionNum, string programType, string safetyRelevant,
                string titleId, string general, string telSrvId, string vehicleComm, string measurement, string hidden, string name, string informationType,
                string identification, string informationFormat, string siNumber, string targetILevel, string controlId,
                string infoType, string infoFormat, string docNum, string priority, string identifier, string flowXml, EcuTranslation ecuTranslation)
            {
                LinkType = linkType;
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
                        completeInfoObjects.AddRange(InfoObjects);
                    }

                    if (Children != null)
                    {
                        foreach (SwiDiagObj swiDiagObj in Children)
                        {
                            completeInfoObjects.AddRange(swiDiagObj.CompleteInfoObjects);
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
                        RuleExpression = RuleExpression.Deserialize(new MemoryStream(Rule), vehicle);
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
                    RuleExpression ruleExpression = RuleExpression.Deserialize(new MemoryStream(Rule), vehicle);
                    return ruleExpression.ToString();
                }
                catch (Exception e)
                {
                    log.ErrorFormat("GetRuleString Exception: '{0}'", e.Message);
                }

                return null;
            }

            public string GetRuleFormula(Vehicle vehicle, RuleExpression.FormulaConfig formulaConfig = null)
            {
                if (vehicle == null)
                {
                    log.ErrorFormat("GetRuleString No vehicle");
                    return null;
                }

                try
                {
                    RuleExpression ruleExpression = RuleExpression.Deserialize(new MemoryStream(Rule), vehicle);
                    RuleExpression.FormulaConfig formulaConfigCurrent = formulaConfig;
                    if (formulaConfigCurrent == null)
                    {
                        formulaConfigCurrent = new RuleExpression.FormulaConfig("RuleString", "RuleNum", "IsValidRuleString", "IsValidRuleNum");
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

        [XmlInclude(typeof(TestModuleData))]
        [XmlType("TestModules")]
        public class TestModules
        {
            public TestModules() : this(null, null)
            {
            }

            public TestModules(VehicleStructsBmw.VersionInfo versionInfo, SerializableDictionary<string, TestModuleData> moduleDataDict)
            {
                Version = versionInfo;
                ModuleDataDict = moduleDataDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }
            [XmlElement("ModuleDataDict"), DefaultValue(null)] public SerializableDictionary<string, TestModuleData> ModuleDataDict { get; set; }
        }

        [XmlType("TestModuleData")]
        public class TestModuleData
        {
            public TestModuleData() : this(null, null)
            {
            }

            public TestModuleData(SerializableDictionary<string, List<string>> refDict, string moduleRef)
            {
                RefDict = refDict;
                ModuleRef = moduleRef;
            }

            [XmlElement("RefDict"), DefaultValue(null)] public SerializableDictionary<string, List<string>> RefDict { get; set; }

            [XmlElement("ModuleRef"), DefaultValue(null)] public string ModuleRef { get; set;  }
        }

        [XmlInclude(typeof(ServiceModuleData))]
        [XmlType("ServiceModules")]
        public class ServiceModules
        {
            public ServiceModules() : this(null, null)
            {
            }

            public ServiceModules(VehicleStructsBmw.VersionInfo versionInfo, SerializableDictionary<string, ServiceModuleData> moduleDataDict = null, bool completed = false)
            {
                Version = versionInfo;
                ModuleDataDict = moduleDataDict;
                Completed = completed;
            }

            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }

            [XmlElement("ModuleDataDict"), DefaultValue(null)] public SerializableDictionary<string, ServiceModuleData> ModuleDataDict { get; set; }

            [XmlElement("Completed")] public bool Completed { get; set; }
        }

        [XmlInclude(typeof(ServiceModuleDataItem))]
        [XmlType("ServiceModuleData")]
        public class ServiceModuleData
        {
            public ServiceModuleData() : this(null)
            {
            }

            public ServiceModuleData(SerializableDictionary<string, ServiceModuleDataItem> dataDict)
            {
                DataDict = dataDict;
            }

            [XmlElement("DataDict"), DefaultValue(null)] public SerializableDictionary<string, ServiceModuleDataItem> DataDict { get; set; }
        }

        [XmlType("ServiceModuleDataItem")]
        public class ServiceModuleDataItem
        {
            public ServiceModuleDataItem() : this(null, null, null, null, null, null)
            {
            }

            public ServiceModuleDataItem(string methodName, string elementNo, string controlId, string serviceDialogName, string containerXml, SerializableDictionary<string, string> runOverrides = null)
            {
                MethodName = methodName;
                ElementNo = elementNo;
                ControlId = controlId;
                ServiceDialogName = serviceDialogName;
                ContainerXml = containerXml;
                RunOverrides = runOverrides;
                EdiabasJobBare = null;
                EdiabasJobOverride = null;
            }

            [XmlElement("MethodName"), DefaultValue(null)] public string MethodName { get; set; }

            [XmlElement("ElementNo"), DefaultValue(null)] public string ElementNo { get; set; }

            [XmlElement("ControlId"), DefaultValue(null)] public string ControlId { get; set; }

            [XmlElement("ServiceDialogName"), DefaultValue(null)] public string ServiceDialogName { get; set; }

            [XmlIgnore, DefaultValue(null)] public string ContainerXml { get; set; }

            [XmlElement("RunOverrides"), DefaultValue(null)] public SerializableDictionary<string, string> RunOverrides { get; set; }

            [XmlElement("EdiabasJobBare"), DefaultValue(null)] public string EdiabasJobBare { get; set; }

            [XmlElement("EdiabasJobOverride"), DefaultValue(null)] public string EdiabasJobOverride { get; set; }
        }

        [XmlInclude(typeof(VehicleStructsBmw.VersionInfo))]
        [XmlType("EcuCharacteristicsXml")]
        public class EcuCharacteristicsData
        {
            public EcuCharacteristicsData() : this(null, null)
            {
            }

            public EcuCharacteristicsData(VehicleStructsBmw.VersionInfo versionInfo, SerializableDictionary<string, string> ecuXmlDict)
            {
                Version = versionInfo;
                EcuXmlDict = ecuXmlDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }
            [XmlElement("EcuXmlDict"), DefaultValue(null)] public SerializableDictionary<string, string> EcuXmlDict { get; set; }
        }

        public class EcuCharacteristicsInfo
        {
            public EcuCharacteristicsInfo(BaseEcuCharacteristics ecuCharacteristics, List<string> seriesList, BNType? bnType, List<string> brandList, string date, string dateCompare)
            {
                EcuCharacteristics = ecuCharacteristics;
                SeriesList = seriesList;
                BnType = bnType;
                BrandList = brandList;
                Date = date;
                DateCompare = dateCompare;
            }

            public BaseEcuCharacteristics EcuCharacteristics { get; set; }
            public List<string> SeriesList { get; set; }
            public BNType? BnType { get; set; }
            public List<string> BrandList { get; set; }
            public string Date { get; set; }
            public string DateCompare { get; set; }
        }

        private const string TestModulesXmlFile = "TestModules.xml";
        private const string TestModulesZipFile = "TestModules.zip";
        private const string ServiceModulesXmlFile = "ServiceModules.xml";
        private const string ServiceModulesZipFile = "ServiceModules.zip";
        private const string EcuCharacteristicsXmFile = "EcuCharacteristics.xml";
        private const string EcuCharacteristicsZipFile = "EcuCharacteristics.zip";
        private const string ConfigurationContainerXMLPar = "ConfigurationContainerXML";
        private static readonly ILog log = LogManager.GetLogger(typeof(PdszDatabase));

        // ToDo: Check on update
        private static List<string> engineRootNodeClasses = new List<string>
        {
            "40141570",
            "40142338",
            "40142722",
            "40143106",
            "40145794",
            "99999999866",
            "99999999868",
            "99999999870",
            "99999999872",
            "99999999874",
            "99999999876",
            "99999999878",
            "99999999880",
            "99999999909",
            "99999999910",
            "99999999918",
            "99999999701",
            "99999999702",
            "99999999703",
            "99999999704",
            "99999999705",
            "99999999706",
            "99999999707",
            "99999999708"
        };

        public delegate bool ProgressDelegate(bool startConvert, int progress =-1, int failures = -1);

        private bool _disposed;
        private string _databasePath;
        private string _databaseExtractPath;
        private string _testModulePath;
        private string _frameworkPath;
        private SQLiteConnection _mDbConnection;
        private string _rootENameClassId;
        private string _typeKeyClassId;
        private Harmony _harmony;
        private Dictionary<string, XepRule> _xepRuleDict;
        private List<SwiDiagObj> _diagObjRootNodes;
        private HashSet<string> _diagObjRootNodeIdSet;
        public Dictionary<string, XepRule> XepRuleDict => _xepRuleDict;
        public SwiRegister SwiRegisterTree { get; private set; }
        public TestModules TestModuleStorage { get; private set; }
        public ServiceModules ServiceModuleStorage { get; private set; }
        public EcuCharacteristicsData EcuCharacteristicsStorage { get; private set; }
        public bool UseIsAtLeastOnePathToRootValid { get; set; }

        private static string _moduleRefPath;
        private static SerializableDictionary<string, List<string>> _moduleRefDict;
        private static SerializableDictionary<string, ServiceModuleDataItem> _serviceDialogDict;
        private static Dictionary<string, int> _serviceDialogCallsDict;
        private static ConstructorInfo _istaServiceDialogDlgCmdBaseConstructor;
        private static ConstructorInfo _istaEdiabasAdapterDeviceResultConstructor;
        private static Type _istaServiceDialogFactoryType;
        private static Type _istaServiceDialogConfigurationType;
        private static Type _coreContractsDocumentLocatorType;

        // ReSharper disable once UnusedMember.Local
        private static bool CallModuleRefPrefix(string refPath, object inParameters, ref object outParameters, ref object inAndOutParameters)
        {
            log.InfoFormat("CallModuleRefPrefix refPath: {0}", refPath);
            _moduleRefPath = refPath;
            if (inParameters != null)
            {
                try
                {
                    PropertyInfo propertyParameter = inParameters.GetType().GetProperty("Parameter");
                    if (propertyParameter == null)
                    {
                        log.ErrorFormat("CallModuleRefPrefix Parameter not found");
                    }
                    else
                    {
                        object parameters = propertyParameter.GetValue(inParameters);
                        Dictionary<string, object> paramDictionary = parameters as Dictionary<string, object>;
                        if (paramDictionary == null)
                        {
                            log.ErrorFormat("CallModuleRefPrefix Parameter Dict not found");
                        }
                        else
                        {
                            log.InfoFormat("CallModuleRefPrefix Parameter Dict items: {0}", paramDictionary.Count);
                            _moduleRefDict = new SerializableDictionary<string, List<string>>();
                            foreach (KeyValuePair<string, object> keyValuePair in paramDictionary)
                            {
                                if (keyValuePair.Value is List<string> elements)
                                {
                                    _moduleRefDict.Add(keyValuePair.Key, elements);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CallModuleRefPrefix Exception: {0}", e.Message);
                }
            }
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CallWriteFaPrefix()
        {
            log.InfoFormat("CallWriteFaPrefix");
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CallGetDatabaseProviderSQLitePrefix(ref object __result)
        {
            log.InfoFormat("CallGetDatabaseProviderSQLitePrefix");
            __result = null;
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CreateServiceDialogPrefix(ref object __result, object callingModule, string methodName, string path, object globalTabModuleISTA, int elementNo, object inParameters, ref object inoutParameters)
        {
            log.InfoFormat("CreateServiceDialogPrefix, Method: {0}, Path: {1}, Element: {2}", methodName, path, elementNo);

            string dialogRef = null;
            if (_istaServiceDialogFactoryType != null)
            {
                try
                {
                    MethodInfo methodResolveDialogRef = _istaServiceDialogFactoryType.GetMethod("ResolveDialogRef", BindingFlags.Public | BindingFlags.Static);
                    if (methodResolveDialogRef == null)
                    {
                        log.ErrorFormat("CreateServiceDialogPrefix ResolveDialogRef not found");
                    }
                    else
                    {
                        dialogRef = methodResolveDialogRef.Invoke(null, new object[] { path, true }) as string;
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix ResolveDialogRef Exception: {0}", e.Message);
                }
            }

            log.InfoFormat("CreateServiceDialogPrefix, DialogRef: '{0}'", dialogRef ?? string.Empty);

            string controlIdString = null;
            string serviceDialogConfigName = null;
            if (_istaServiceDialogConfigurationType != null && !string.IsNullOrEmpty(dialogRef))
            {
                try
                {
                    MethodInfo methodGetRegisteredConfiguration = _istaServiceDialogConfigurationType.GetMethod("GetRegisteredConfiguration", BindingFlags.Public | BindingFlags.Static);
                    if (methodGetRegisteredConfiguration == null)
                    {
                        log.ErrorFormat("CreateServiceDialogPrefix GetRegisteredConfiguration not found");
                    }
                    else
                    {
                        dynamic serviceDialogConfiguration = methodGetRegisteredConfiguration.Invoke(null, new object[] { dialogRef });
                        if (serviceDialogConfiguration == null)
                        {
                            log.ErrorFormat("CreateServiceDialogPrefix ServiceDialogConfiguration not found");
                        }
                        else
                        {
                            decimal controlId = serviceDialogConfiguration.ControlId;
                            controlIdString = controlId.ToString(CultureInfo.InvariantCulture);
                            serviceDialogConfigName = serviceDialogConfiguration.Name;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix ResolveDialogRef Exception: {0}", e.Message);
                }
            }

            log.InfoFormat("CreateServiceDialogPrefix, ControlId: {0}, ServiceDialogName: {1}",
                controlIdString ?? string.Empty, serviceDialogConfigName ?? string.Empty);

            string configurationContainerXml = string.Empty;
            SerializableDictionary<string, string> runOverridesDict = new SerializableDictionary<string, string>();
            dynamic inParametersDyn = inParameters;
            if (inParametersDyn != null)
            {
                try
                {
                    dynamic dscConfig = inParametersDyn.getParameter("/WurzelIn/DSCConfig", null);
                    if (dscConfig != null)
                    {
                        dynamic paramOverrides = dscConfig.ParametrizationOverrides;
                        if (paramOverrides != null)
                        {
                            configurationContainerXml = paramOverrides.getParameter(ConfigurationContainerXMLPar, string.Empty) as string;
                        }

                        dynamic runOverrides = dscConfig.RunOverrides;
                        if (runOverrides != null)
                        {
                            object runParameter = runOverrides.Parameter;
                            if (runParameter is Dictionary<string, object> paramDictionary)
                            {
                                foreach (KeyValuePair<string, object> keyValuePair in paramDictionary)
                                {
                                    if (keyValuePair.Value is string valueString)
                                    {
                                        log.InfoFormat("CreateServiceDialogPrefix, Override: '{0}', '{1}'", keyValuePair.Key, valueString);
                                        runOverridesDict.Add(keyValuePair.Key, valueString);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        log.ErrorFormat("CreateServiceDialogPrefix No DSCConfig");
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix DSCConfig Exception: {0}", e.Message);
                }
            }

            string elementNoString = elementNo.ToString(CultureInfo.InvariantCulture);
            string key = methodName + ";" + path + ";" + elementNoString;

            if (!string.IsNullOrWhiteSpace(configurationContainerXml))
            {
                if (_serviceDialogDict == null)
                {
                    _serviceDialogDict = new SerializableDictionary<string, ServiceModuleDataItem>();
                }

                if (!_serviceDialogDict.ContainsKey(key))
                {
                    log.InfoFormat("CreateServiceDialogPrefix Adding Key: {0}", key);
                    ServiceModuleDataItem serviceModuleDataItem = new ServiceModuleDataItem(methodName, elementNoString, controlIdString, serviceDialogConfigName, configurationContainerXml);
                    if (runOverridesDict.Count > 0)
                    {
                        serviceModuleDataItem.RunOverrides = runOverridesDict;
                    }
                    _serviceDialogDict.Add(key, serviceModuleDataItem);
                    //log.Info(configurationContainerXml);
                }
                else
                {
                    log.InfoFormat("CreateServiceDialogPrefix Key present: {0}", key);
                }
            }
            else
            {
                log.InfoFormat("CreateServiceDialogPrefix No container XML");
            }

            if (_serviceDialogCallsDict == null)
            {
                _serviceDialogCallsDict = new Dictionary<string, int>();
            }

            if (!_serviceDialogCallsDict.ContainsKey(key))
            {
                _serviceDialogCallsDict.Add(key, 1);
            }
            else
            {
                _serviceDialogCallsDict[key]++;
            }

            int calls = _serviceDialogCallsDict[key];
            log.InfoFormat("CreateServiceDialogPrefix Calls: {0}", calls);
            if (calls > 2)
            {
                Thread.CurrentThread.Abort();
            }

            object serviceDialog = null;
            if (_istaServiceDialogDlgCmdBaseConstructor != null)
            {
                object[] args = new object[] { callingModule, methodName, path, globalTabModuleISTA, elementNo };
                try
                {
                    serviceDialog = _istaServiceDialogDlgCmdBaseConstructor.Invoke(args);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix Exception: '{0}'", e.Message);
                }
            }
            else
            {
                log.ErrorFormat("CreateServiceDialogPrefix No service dialog construtor");
            }

            __result = serviceDialog;
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool ServiceDialogCmdBaseInvokePrefix(string method, object inParam, ref object outParam, ref object inoutParam)
        {
            log.InfoFormat("ServiceDialogCmdBaseInvokePrefix, Method: {0}", method);

            dynamic outParmDyn = outParam;
            if (outParmDyn != null)
            {
                try
                {
                    outParmDyn.setParameter("Quit", true);

                    object ediabasAdapterDeviceResult = _istaEdiabasAdapterDeviceResultConstructor.Invoke(null);
                    if (ediabasAdapterDeviceResult != null)
                    {
                        outParmDyn.setParameter("/WurzelOut/DSCResult", ediabasAdapterDeviceResult);
                    }
                    else
                    {
                        log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix EdiabasAdapterDeviceResult empty");
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix SetParameter Exception: '{0}'", e.Message);
                }
            }
            else
            {
                log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix No container setParameter");
            }

            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool ConfigurationContainerDeserializePrefix(string configurationContainer)
        {
            log.InfoFormat("ConfigurationContainerDeserializePrefix");
            return true;
        }

        // ReSharper disable once UnusedMember.Local
        private static void ConfigurationContainerDeserializePostfix(ref object __result, string configurationContainer)
        {
            string resultType = __result != null ? __result.GetType().FullName : string.Empty;
            log.InfoFormat("ConfigurationContainerDeserializePostfix Result: {0}", resultType);
            dynamic resultDyn = __result;
            if (resultDyn != null)
            {
                try
                {
                    resultDyn.AddParametrizationOverride(ConfigurationContainerXMLPar, configurationContainer);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("ConfigurationContainerDeserializePostfix AddParametrizationOverride Exception: {0}", e.Message);
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static bool IndirectDocumentPrefix2(ref object __result, string title, string heading)
        {
            log.InfoFormat("IndirectDocumentPrefix2 Title: {0}, Heading: {1}", title ?? string.Empty, heading ?? string.Empty);
            object documentList = null;
            try
            {
                Type listType = typeof(List<>).MakeGenericType(new[] { _coreContractsDocumentLocatorType });
                documentList = Activator.CreateInstance(listType);
            }
            catch (Exception e)
            {
                log.ErrorFormat("IndirectDocumentPrefix2 new List<IDocumentLocator>() Exception: {0}", e.Message);
            }

            __result = documentList;
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool IndirectDocumentPrefix3(ref object __result, string title, string heading, string informationsTyp)
        {
            log.InfoFormat("IndirectDocumentPrefix3 Title: {0}, Heading: {1}, Info: {2}", title ?? string.Empty, heading ?? string.Empty, informationsTyp ?? string.Empty);

            object documentList = null;
            try
            {
                Type listType = typeof(List<>).MakeGenericType(new[] { _coreContractsDocumentLocatorType });
                documentList = Activator.CreateInstance(listType);
            }
            catch (Exception e)
            {
                log.ErrorFormat("IndirectDocumentPrefix3 new List<IDocumentLocator>() Exception: {0}", e.Message);
            }

            __result = documentList;
            return false;
        }

        public PdszDatabase(string istaFolder)
        {
            if (!Directory.Exists(istaFolder))
            {
                log.ErrorFormat("PdszDatabase: ISTA path not existing: {0}", istaFolder);
                throw new Exception(string.Format("ISTA path not existing: {0}", istaFolder));
            }

            _databasePath = Path.Combine(istaFolder, "SQLiteDBs");
            if (!Directory.Exists(_databasePath))
            {
                log.ErrorFormat("PdszDatabase: ISTA database path not existing: {0}", _databasePath);
                throw new Exception(string.Format("ISTA database path not existing: {0}", _databasePath));
            }

            _databaseExtractPath = Path.Combine(_databasePath, "Extract");
            if (!Directory.Exists(_databaseExtractPath))
            {
                try
                {
                    Directory.CreateDirectory(_databaseExtractPath);
                }
                catch (Exception e)
                {
                    log.InfoFormat("PdszDatabase CreateDirectory Exception: {0}", e.Message);
                }
            }

            _testModulePath = Path.Combine(istaFolder, "Testmodule");
            if (!Directory.Exists(_testModulePath))
            {
                log.ErrorFormat("PdszDatabase: ISTA testmodule path not existing: {0}", _testModulePath);
                throw new Exception(string.Format("ISTA testmodule path not existing: {0}", _testModulePath));
            }

            _frameworkPath = Path.Combine(istaFolder, "TesterGUI", "bin","ReleaseMod");
            if (!Directory.Exists(_frameworkPath))
            {
                _frameworkPath = Path.Combine(istaFolder, "TesterGUI", "bin", "Release");
            }

            if (!Directory.Exists(_frameworkPath))
            {
                log.ErrorFormat("PdszDatabase: ISTA framework path not existing: {0}", _frameworkPath);
                throw new Exception(string.Format("ISTA framework path not existing: {0}", _frameworkPath));
            }

            log.InfoFormat("PdszDatabase: ISTA framework path: {0}", _frameworkPath);

            string databaseFile = Path.Combine(_databasePath, "DiagDocDb.sqlite");
            string connection = "Data Source=\"" + databaseFile + "\";";
            _mDbConnection = new SQLiteConnection(connection);

            _mDbConnection.SetPassword("6505EFBDC3E5F324");
            _mDbConnection.Open();

            _rootENameClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"RootEBezeichnung");
            _typeKeyClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"Typschluessel");
            _harmony = new Harmony("de.holeschak.PsdzClient");
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

        public static string SwiRegisterEnumerationNameConverter(SwiRegisterEnum swiRegisterEnum)
        {
            string arg;
            switch (swiRegisterEnum)
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
                    arg = "SOFORTMASSNAHMEN";
                    break;
                default:
                    throw new ArgumentException("Unknown SWI Register!");
            }
            return string.Format(CultureInfo.InvariantCulture, "REG|{0}", arg);
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

        public Dictionary<string, string> GetTextCollectionById(string id)
        {
            log.InfoFormat("GetTextCollectionById Id: {0}", id);

            if (string.IsNullOrEmpty(id))
            {
                log.ErrorFormat("GetTextCollectionById No ID");
                return null;
            }

            Dictionary<string, string> textCollection = new Dictionary<string, string>();
            try
            {
                EcuTranslation xmlTranslation = null;
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, INFOOBJECT_ID, " + SqlXmlItems + ", FROM XEP_REFSPTEXTCOLL WHERE (INFOOBJECT_ID = {0})", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetTextCollectionById Exception: '{0}'", e.Message);
                return null;
            }

            return textCollection;
        }

        public string GetXmlValuePrimitivesById(string id, string languageExtension)
        {
            log.InfoFormat("GetXmlValuePrimitivesById Id: {0}, Lang: {1}", id, languageExtension);

            string data = GetXmlValuePrimitivesByIdSingle(id, languageExtension);
            if (string.IsNullOrEmpty(data))
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

            log.InfoFormat("GetXmlValuePrimitivesById Data: {0}", data);
            return data;
        }

        public string GetXmlValuePrimitivesByIdSingle(string id, string languageExtension)
        {
            log.InfoFormat("GetXmlValuePrimitivesByIdSingle Id: {0}, Lang: {1}", id, languageExtension);

            string data = null;
            try
            {
                string databaseName = @"xmlvalueprimitive_" + languageExtension + @".sqlite";
                string databaseFile = Path.Combine(_databasePath, databaseName);
                if (!File.Exists(databaseFile))
                {
                    log.InfoFormat("GetXmlValuePrimitivesByIdSingle File not found: {0}", databaseFile);
                    return null;
                }

                string connection = "Data Source=\"" + databaseFile + "\";";
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.Open();
                    string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, DATA FROM XMLVALUEPRIMITIVE WHERE (ID = '{0}')", id);
                    using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
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

            log.InfoFormat("GetXmlValuePrimitivesByIdSingle Data: {0}", data);
            return data;
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

        public bool GenerateTestModuleData(ProgressDelegate progressHandler)
        {
            try
            {
                TestModules testModules = null;
                XmlSerializer serializer = new XmlSerializer(typeof(TestModules));
                string testModulesZipFile = Path.Combine(_databaseExtractPath, TestModulesZipFile);
                if (File.Exists(testModulesZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(testModulesZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, TestModulesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        testModules = serializer.Deserialize(reader) as TestModules;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (zf != null)
                            {
                                zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                                zf.Close(); // Ensure we release resources
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateTestModuleData Deserialize Exception: '{0}'", e.Message);
                    }
                }

                bool dataValid = true;
                if (testModules != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (testModules.Version == null || !testModules.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateTestModuleData Version mismatch");
                        dataValid = false;
                    }
                }

                if (testModules == null || !dataValid)
                {
                    log.InfoFormat("GenerateTestModuleData Converting test modules");
                    if (!IsExecutable())
                    {
                        log.ErrorFormat("GenerateTestModuleData Started from DLL");
                        return false;
                    }

                    if (progressHandler != null)
                    {
                        if (progressHandler.Invoke(true, 0, 0))
                        {
                            log.ErrorFormat("GenerateTestModuleData Aborted");
                            return false;
                        }
                    }

                    testModules = ConvertAllTestModules(progressHandler);
                    if (testModules == null)
                    {
                        log.ErrorFormat("GenerateTestModuleData ConvertAllTestModules failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        serializer.Serialize(memStream, testModules);
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(testModulesZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(TestModulesXmlFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                TestModuleStorage = testModules;
                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("StoreTestModuleData Exception: '{0}'", e.Message);
                return false;
            }
        }

        public TestModules ConvertAllTestModules(ProgressDelegate progressHandler)
        {
            try
            {
                ReadSwiRegister(null, null);
                List<SwiAction> swiActions = CollectSwiActionsForNode(SwiRegisterTree, true);
                if (swiActions == null)
                {
                    log.ErrorFormat("ConvertAllTestModules CollectSwiActionsForNode failed");
                    return null;
                }

                SerializableDictionary<string, TestModuleData> moduleDataDict = new SerializableDictionary<string, TestModuleData>();
                int failCount = 0;
                int index = 0;
                foreach (SwiAction swiAction in swiActions)
                {
                    if (progressHandler != null)
                    {
                        int percent = index * 100 / swiActions.Count;
                        if (progressHandler.Invoke(false, percent, failCount))
                        {
                            log.ErrorFormat("ConvertAllTestModules Aborted at {0}%", percent);
                            return null;
                        }
                    }

                    foreach (SwiInfoObj infoInfoObj in swiAction.SwiInfoObjs)
                    {
                        if (infoInfoObj.LinkType == SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                        {
                            string moduleName = infoInfoObj.ModuleName;
                            string key = moduleName.ToUpperInvariant();
                            if (!moduleDataDict.ContainsKey(key))
                            {
                                TestModuleData moduleData = ReadTestModule(moduleName, out bool failure);
                                if (moduleData == null)
                                {
                                    log.ErrorFormat("ConvertAllTestModules ReadTestModule failed for: {0}", moduleName);
                                    if (failure)
                                    {
                                        log.ErrorFormat("ConvertAllTestModules ReadTestModule generation failure for: {0}", moduleName);
                                        failCount++;
                                    }
                                }
                                else
                                {
                                    moduleDataDict.Add(key, moduleData);
                                }
                            }
                        }
                    }

                    index++;
                }

                progressHandler?.Invoke(false, 100, failCount);

                log.InfoFormat("ConvertAllTestModules Count: {0}, Failures: {1}", moduleDataDict.Count, failCount);
                if (moduleDataDict.Count == 0)
                {
                    log.ErrorFormat("ConvertAllTestModules No test modules generated");
                    return null;
                }

                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                return new TestModules(versionInfo, moduleDataDict);
            }
            catch (Exception e)
            {
                log.ErrorFormat("ConvertAllTestModules Exception: '{0}'", e.Message);
                return null;
            }
        }

        public TestModuleData GetTestModuleData(string moduleName)
        {
            log.InfoFormat("GetTestModuleData Name: {0}", moduleName);
            if (TestModuleStorage == null)
            {
                return null;
            }

            string key = moduleName.ToUpperInvariant();
            if (!TestModuleStorage.ModuleDataDict.TryGetValue(key, out TestModuleData testModuleData))
            {
                log.InfoFormat("GetTestModuleData Module not found: {0}", moduleName);
                return null;
            }

            return testModuleData;
        }

        public TestModuleData ReadTestModule(string moduleName, out bool failure)
        {
            log.InfoFormat("ReadTestModule Name: {0}", moduleName);
            failure = false;
            try
            {
                if (string.IsNullOrEmpty(moduleName))
                {
                    return null;
                }

                string fileName = moduleName + ".dll";
                string moduleFile = Path.Combine(_testModulePath, fileName);
                if (!File.Exists(moduleFile))
                {
                    log.ErrorFormat("ReadTestModule File not found: {0}", moduleFile);
                    return null;
                }

                string coreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldCoreFramework.dll");
                if (!File.Exists(coreFrameworkFile))
                {
                    log.ErrorFormat("ReadTestModule Core framework not found: {0}", coreFrameworkFile);
                    return null;
                }
                Assembly coreFrameworkAssembly = Assembly.LoadFrom(coreFrameworkFile);

                Type databaseProviderType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderFactory");
                if (databaseProviderType == null)
                {
                    log.ErrorFormat("ReadTestModule GetDatabaseProviderSQLite not found");
                    return null;
                }

                MethodInfo methodGetDatabaseProviderSQLite = databaseProviderType.GetMethod("GetDatabaseProviderSQLite", BindingFlags.Public | BindingFlags.Static);
                if (methodGetDatabaseProviderSQLite == null)
                {
                    log.ErrorFormat("ReadTestModule GetDatabaseProviderSQLite not found");
                    return null;
                }

                string istaCoreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldISTACoreFramework.dll");
                if (!File.Exists(istaCoreFrameworkFile))
                {
                    log.ErrorFormat("ReadTestModule ISTA core framework not found: {0}", istaCoreFrameworkFile);
                    return null;
                }
                Assembly istaCoreFrameworkAssembly = Assembly.LoadFrom(istaCoreFrameworkFile);

                Type istaModuleType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ISTAModule");
                if (istaModuleType == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule not found");
                    return null;
                }

                MethodInfo methodIstaModuleModuleRef = istaModuleType.GetMethod("callModuleRef", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodIstaModuleModuleRef == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule callModuleRef not found");
                    return null;
                }

                MethodInfo methodModuleRefPrefix = typeof(PdszDatabase).GetMethod("CallModuleRefPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleRefPrefix == null)
                {
                    log.ErrorFormat("ReadTestModule CallModuleRefPrefix not found");
                    return null;
                }

                MethodInfo methodWriteFaPrefix = typeof(PdszDatabase).GetMethod("CallWriteFaPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodWriteFaPrefix == null)
                {
                    log.ErrorFormat("ReadTestModule CallWriteFaPrefix not found");
                    return null;
                }

                MethodInfo methodGetDatabasePrefix = typeof(PdszDatabase).GetMethod("CallGetDatabaseProviderSQLitePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodGetDatabasePrefix == null)
                {
                    log.ErrorFormat("ReadTestModule CallGetDatabaseProviderSQLitePrefix not found");
                    return null;
                }

                bool patchedModuleRef = false;
                bool patchedGetDatabase = false;
                foreach (MethodBase methodBase in _harmony.GetPatchedMethods())
                {
                    log.InfoFormat("ReadTestModule Patched: {0}", methodBase.Name);
                    if (methodBase == methodIstaModuleModuleRef)
                    {
                        patchedModuleRef = true;
                    }

                    if (methodBase == methodGetDatabaseProviderSQLite)
                    {
                        patchedGetDatabase = true;
                    }
                }

                if (!patchedGetDatabase)
                {
                    log.InfoFormat("ReadTestModule Patching: {0}", methodGetDatabaseProviderSQLite.Name);
                    _harmony.Patch(methodGetDatabaseProviderSQLite, new HarmonyMethod(methodGetDatabasePrefix));
                }

                Assembly moduleAssembly = Assembly.LoadFrom(moduleFile);
                Type[] exportedTypes = moduleAssembly.GetExportedTypes();
                Type moduleType = null;
                foreach (Type type in exportedTypes)
                {
                    log.InfoFormat("ReadTestModule Exported type: {0}", type.FullName);
                    if (moduleType == null)
                    {
                        if (!string.IsNullOrEmpty(type.FullName) &&
                            type.FullName.StartsWith("BMW.Rheingold.Module.", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleType = type;
                        }
                    }
                }

                if (moduleType == null)
                {
                    log.ErrorFormat("ReadTestModule No module type found");
                    return null;
                }

                log.InfoFormat("ReadTestModule Using module type: {0}", moduleType.FullName);
                object moduleParamContainerInst = CreateModuleParamContainerInst(coreFrameworkAssembly, out Type moduleParamContainerType);
                if (moduleParamContainerInst == null)
                {
                    log.ErrorFormat("ReadTestModule CreateModuleParamContainerInst failed");
                }

                if (!patchedModuleRef)
                {
                    log.InfoFormat("ReadTestModule Patching: {0}", methodIstaModuleModuleRef.Name);
                    _harmony.Patch(methodIstaModuleModuleRef, new HarmonyMethod(methodModuleRefPrefix));
                }

                MethodInfo methodeTestModuleStartType = moduleType.GetMethod("Start");
                if (methodeTestModuleStartType == null)
                {
                    log.ErrorFormat("ReadTestModule Test module Start methode not found");
                    return null;
                }

                MethodInfo methodTestModuleChangeFa = moduleType.GetMethod("Change_FA", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodTestModuleChangeFa == null)
                {
                    log.ErrorFormat("ReadTestModule Test module Change_FA methode not found");
                    return null;
                }

                MethodInfo methodTestModuleWriteFa = moduleType.GetMethod("FA_schreiben", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodTestModuleWriteFa != null)
                {
                    log.InfoFormat("ReadTestModule Patching: {0}", methodTestModuleWriteFa.Name);
                    _harmony.Patch(methodTestModuleWriteFa, new HarmonyMethod(methodWriteFaPrefix));
                }

                object testModule = Activator.CreateInstance(moduleType, moduleParamContainerInst);
                log.InfoFormat("ReadTestModule Module loaded: {0}, Type: {1}", fileName, moduleType.FullName);

                _moduleRefPath = null;
                _moduleRefDict = null;
                object moduleRunInContainerInst = Activator.CreateInstance(moduleParamContainerType);
                object moduleRunOutContainerInst = Activator.CreateInstance(moduleParamContainerType);
                object moduleRunInOutContainerInst = Activator.CreateInstance(moduleParamContainerType);
                object[] startArguments = { moduleRunInContainerInst, moduleRunOutContainerInst, moduleRunInOutContainerInst };
                methodeTestModuleStartType.Invoke(testModule, startArguments);

                string moduleRef = _moduleRefPath;
                if (!string.IsNullOrEmpty(moduleRef))
                {
                    log.ErrorFormat("ReadTestModule RefPath: {0}", moduleRef);
                }

                if (_moduleRefDict == null)
                {
                    bool ignore = moduleName.StartsWith("ABL_AUS_RETROFITLANGUAGE", StringComparison.OrdinalIgnoreCase);
                    if (ignore)
                    {
                        log.InfoFormat("ReadTestModule Ignored No data from test module: {0}", moduleName);
                    }
                    else
                    {
                        log.ErrorFormat("ReadTestModule No data from test module: {0}", moduleName);
                        failure = true;
                    }
                    return null;
                }

                SerializableDictionary<string, List<string>> moduleRefDict = _moduleRefDict;
                _moduleRefDict = null;
                log.ErrorFormat("ReadTestModule Test module items: {0}", moduleRefDict.Count);
                foreach (KeyValuePair<string, List<string>> keyValuePair in moduleRefDict)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "Key: {0}", keyValuePair.Key));
                    sb.Append(", Values: ");
                    if (keyValuePair.Value is List<string> elements)
                    {
                        foreach (string element in elements)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0} ", element));
                        }
                    }

                    log.InfoFormat("ReadTestModule Entry {0}", sb);
                }

                log.InfoFormat("ReadTestModule Finished: {0}", fileName);

                return new TestModuleData(moduleRefDict, moduleRef);
            }
            catch (Exception e)
            {
                failure = true;
                log.ErrorFormat("ReadTestModule Exception: '{0}'", e.Message);
                return null;
            }
        }

        private object CreateModuleParamContainerInst(Assembly coreFrameworkAssembly, out Type moduleParamContainerType)
        {
            moduleParamContainerType = null;
            try
            {
                string sessionControllerFile = Path.Combine(_frameworkPath, "RheingoldSessionController.dll");
                if (!File.Exists(sessionControllerFile))
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Session controller not found: {0}", sessionControllerFile);
                    return null;
                }
                Assembly sessionControllerAssembly = Assembly.LoadFrom(sessionControllerFile);

                moduleParamContainerType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.ParameterContainer");
                if (moduleParamContainerType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ParameterContainer not found");
                    return null;
                }
                object moduleParamContainerInst = Activator.CreateInstance(moduleParamContainerType);

                Type moduleParamType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.ModuleParameter");
                if (moduleParamType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ModuleParameter not found");
                    return null;
                }

                Type paramNameType = moduleParamType.GetNestedType("ParameterName", BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (paramNameType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ParameterName type not found");
                    return null;
                }
                object parameterNameLogic = Enum.Parse(paramNameType, "Logic", true);
                object parameterNameVehicle = Enum.Parse(paramNameType, "Vehicle", true);

                object moduleParamInst = Activator.CreateInstance(moduleParamType);
                Type logicType = sessionControllerAssembly.GetType("BMW.Rheingold.RheingoldSessionController.Logic");
                if (logicType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Logic not found");
                    return null;
                }
                object logicInst = Activator.CreateInstance(logicType);

                Type vehicleType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle");
                if (vehicleType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Vehicle not found");
                    return null;
                }
                object vehicleInst = Activator.CreateInstance(vehicleType);

                MethodInfo methodContainerSetParameter = moduleParamContainerType.GetMethod("setParameter");
                if (methodContainerSetParameter == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ParameterContainer setParameter not found");
                    return null;
                }

                MethodInfo methodSetParameter = moduleParamType.GetMethod("setParameter");
                if (methodSetParameter == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ModuleParameter setParameter not found");
                    return null;
                }

                methodSetParameter.Invoke(moduleParamInst, new object[] { parameterNameLogic, logicInst });
                methodSetParameter.Invoke(moduleParamInst, new object[] { parameterNameVehicle, vehicleInst });
                methodContainerSetParameter.Invoke(moduleParamContainerInst, new object[] { "__RheinGoldCoreModuleParameters__", moduleParamInst });

                return moduleParamContainerInst;
            }
            catch (Exception e)
            {
                log.ErrorFormat("CreateModuleParamContainerInst Exception: '{0}'", e.Message);
                return null;
            }
        }

        public bool GenerateServiceModuleData(ProgressDelegate progressHandler)
        {
            try
            {
                ServiceModules serviceModules = null;
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceModules));
                string serviceModulesZipFile = Path.Combine(_databaseExtractPath, ServiceModulesZipFile);
                if (File.Exists(serviceModulesZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(serviceModulesZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, ServiceModulesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        serviceModules = serializer.Deserialize(reader) as ServiceModules;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (zf != null)
                            {
                                zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                                zf.Close(); // Ensure we release resources
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateServiceModuleData Deserialize Exception: '{0}'", e.Message);
                    }
                }

                bool dataValid = true;
                bool completed = false;
                if (serviceModules != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (serviceModules.Version == null || !serviceModules.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateServiceModuleData Version mismatch");
                        dataValid = false;
                    }

                    if (dataValid)
                    {
                        completed = serviceModules.Completed;
                    }
                }

                if (serviceModules == null || !dataValid || !completed)
                {
                    log.InfoFormat("GenerateServiceModuleData Converting modules");
                    if (!IsExecutable())
                    {
                        log.ErrorFormat("GenerateServiceModuleData Started from DLL");
                        return false;
                    }

                    if (progressHandler != null)
                    {
                        if (progressHandler.Invoke(true, 0, 0))
                        {
                            log.ErrorFormat("GenerateServiceModuleData Aborted");
                            return false;
                        }
                    }

                    if (serviceModules != null && dataValid)
                    {
                        serviceModules = ConvertAllServiceModules(progressHandler, serviceModules);
                    }
                    else
                    {
                        serviceModules = ConvertAllServiceModules(progressHandler, serviceModules);
                    }

                    if (serviceModules == null)
                    {
                        log.ErrorFormat("GenerateServiceModuleData ConvertAllServiceModules failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        serializer.Serialize(memStream, serviceModules);
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(serviceModulesZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(ServiceModulesXmlFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                ServiceModuleStorage = serviceModules;
                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("GenerateServiceModuleData Exception: '{0}'", e.Message);
                return false;
            }
        }

        public ServiceModules ConvertAllServiceModules(ProgressDelegate progressHandler, ServiceModules lastServiceModules = null)
        {
            try
            {
                List<SwiDiagObj> diagObjsNodeClass = GetInfoObjectsTreeForNodeclassName(DiagObjServiceRoot, null, new List<string> { "ABL" });
                if (diagObjsNodeClass == null)
                {
                    log.ErrorFormat("ConvertAllServiceModules GetInfoObjectsTreeForNodeclassName failed");
                    return null;
                }

                List<SwiInfoObj> completeInfoObjects = new List<SwiInfoObj>();
                foreach (SwiDiagObj swiDiagObj in diagObjsNodeClass)
                {
                    completeInfoObjects.AddRange(swiDiagObj.CompleteInfoObjects);
                }

                SerializableDictionary<string, ServiceModuleData> moduleDataDict = lastServiceModules?.ModuleDataDict;
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (moduleDataDict == null)
                {
                    moduleDataDict = new SerializableDictionary<string, ServiceModuleData>();
                }

                bool completed = true;
                int failCount = 0;
                int index = 0;
                foreach (SwiInfoObj swiInfoObj in completeInfoObjects)
                {
                    if (progressHandler != null)
                    {
                        int percent = index * 100 / completeInfoObjects.Count;
                        if (progressHandler.Invoke(false, percent, failCount))
                        {
                            log.ErrorFormat("ConvertAllServiceModules Aborted at {0}%", percent);
                            return null;
                        }

                        string moduleName = swiInfoObj.ModuleName;
                        string key = moduleName.ToUpperInvariant();
                        if (!moduleDataDict.ContainsKey(key))
                        {
                            ServiceModuleData moduleData = ReadServiceModule(moduleName, out bool failure);
                            if (moduleData == null)
                            {
                                log.ErrorFormat("ConvertAllServiceModules ReadServiceModule failed for: {0}", moduleName);
                                if (failure)
                                {
                                    log.ErrorFormat("ConvertAllServiceModules ReadServiceModule generation failure for: {0}", moduleName);
                                    failCount++;
                                }
                            }

                            moduleDataDict.Add(key, moduleData);

                            GC.Collect();
                            Process currentProcess = Process.GetCurrentProcess();
                            long usedMemory = currentProcess.PrivateMemorySize64;
                            long usedMemoryMB = usedMemory / (1024 * 1024);
                            log.InfoFormat("ConvertAllServiceModules Memory: {0}MB", usedMemoryMB);
                            if (usedMemoryMB > 200)
                            {
                                log.InfoFormat("ConvertAllServiceModules Memory exhausted");
                                completed = false;
                                break;
                            }
                        }
                        else
                        {
                            log.ErrorFormat("ConvertAllServiceModules ReadServiceModule Module present: {0}", moduleName);
                        }
                    }

                    index++;
                }

                progressHandler?.Invoke(false, 100, failCount);

                log.InfoFormat("ConvertAllServiceModules Count: {0}, Failures: {1}", moduleDataDict.Count, failCount);
                if (moduleDataDict.Count == 0)
                {
                    log.ErrorFormat("ConvertAllServiceModules No test modules generated");
                    return null;
                }

                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                return new ServiceModules(versionInfo, moduleDataDict, completed);
            }
            catch (Exception e)
            {
                log.ErrorFormat("ConvertAllServiceModules Exception: '{0}'", e.Message);
                return null;
            }
        }

        public ServiceModuleData ReadServiceModule(string moduleName, out bool failure)
        {
            log.InfoFormat("ReadServiceModule Name: {0}", moduleName);
            failure = false;
            try
            {
                if (string.IsNullOrEmpty(moduleName))
                {
                    return null;
                }

                string fileName = moduleName + ".dll";
                string moduleFile = Path.Combine(_testModulePath, fileName);
                if (!File.Exists(moduleFile))
                {
                    log.ErrorFormat("ReadServiceModule File not found: {0}", moduleFile);
                    return null;
                }

                string coreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldCoreFramework.dll");
                if (!File.Exists(coreFrameworkFile))
                {
                    log.ErrorFormat("ReadServiceModule Core framework not found: {0}", coreFrameworkFile);
                    return null;
                }
                Assembly coreFrameworkAssembly = Assembly.LoadFrom(coreFrameworkFile);

                Type databaseProviderType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderFactory");
                if (databaseProviderType == null)
                {
                    log.ErrorFormat("ReadServiceModule GetDatabaseProviderSQLite not found");
                    return null;
                }

                MethodInfo methodGetDatabaseProviderSQLite = databaseProviderType.GetMethod("GetDatabaseProviderSQLite", BindingFlags.Public | BindingFlags.Static);
                if (methodGetDatabaseProviderSQLite == null)
                {
                    log.ErrorFormat("ReadServiceModule GetDatabaseProviderSQLite not found");
                    return null;
                }

                string coreContractsFile = Path.Combine(_frameworkPath, "RheingoldCoreContracts.dll");
                if (!File.Exists(coreContractsFile))
                {
                    log.ErrorFormat("ReadServiceModule Core contracts not found: {0}", coreContractsFile);
                    return null;
                }
                Assembly coreContractsAssembly = Assembly.LoadFrom(coreContractsFile);

                if (_coreContractsDocumentLocatorType == null)
                {
                    Type coreContractsDocumentLocatorType = coreContractsAssembly.GetType("BMW.Rheingold.CoreFramework.Contracts.IDocumentLocator");
                    if (coreContractsDocumentLocatorType == null)
                    {
                        log.ErrorFormat("ReadServiceModule IDocumentLocator not found");
                        return null;
                    }

                    _coreContractsDocumentLocatorType = coreContractsDocumentLocatorType;
                }

                string istaCoreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldISTACoreFramework.dll");
                if (!File.Exists(istaCoreFrameworkFile))
                {
                    log.ErrorFormat("ReadServiceModule ISTA core framework not found: {0}", istaCoreFrameworkFile);
                    return null;
                }
                Assembly istaCoreFrameworkAssembly = Assembly.LoadFrom(istaCoreFrameworkFile);

                Type istaServiceDialogFactoryType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ServiceDialogFactory");
                if (istaServiceDialogFactoryType == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogFactory not found");
                    return null;
                }

                _istaServiceDialogFactoryType = istaServiceDialogFactoryType;

                Type istaServiceDialogConfigurationType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ServiceDialogConfiguration");
                if (istaServiceDialogConfigurationType == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogConfiguration not found");
                    return null;
                }

                _istaServiceDialogConfigurationType = istaServiceDialogConfigurationType;

                MethodInfo methodCreateServiceDialog = istaServiceDialogFactoryType.GetMethod("CreateServiceDialog", BindingFlags.Public | BindingFlags.Instance);
                if (methodCreateServiceDialog == null)
                {
                    log.ErrorFormat("ReadServiceModule CreateServiceDialog not found");
                    return null;
                }

                MethodInfo methodCreateServiceDialogPrefix = typeof(PdszDatabase).GetMethod("CreateServiceDialogPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodCreateServiceDialogPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule CreateServiceDialogPrefix not found");
                    return null;
                }

                Type istaServiceDialogDlgCmdBaseType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ServiceDialogCmdBase");
                if (istaServiceDialogDlgCmdBaseType == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogCmdBase not found");
                    return null;
                }

                MethodInfo methodIstaServiceDialogDlgCmdBaseInvoke = istaServiceDialogDlgCmdBaseType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
                if (methodIstaServiceDialogDlgCmdBaseInvoke == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogDlgCmd Invoke not found");
                    return null;
                }

                MethodInfo methodServiceDialogCmdBaseInvokePrefix = typeof(PdszDatabase).GetMethod("ServiceDialogCmdBaseInvokePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodServiceDialogCmdBaseInvokePrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogCmdBaseInvokePrefix not found");
                    return null;
                }

                Type istaConfigurationContainerType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ConfigurationContainer");
                if (istaConfigurationContainerType == null)
                {
                    log.ErrorFormat("ReadServiceModule ConfigurationContainer not found");
                    return null;
                }

                MethodInfo methodConfigurationContainerDeserialize = istaConfigurationContainerType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);
                if (methodConfigurationContainerDeserialize == null)
                {
                    log.ErrorFormat("ReadServiceModule ConfigurationContainer Deserialize not found");
                    return null;
                }

                MethodInfo methodConfigurationContainerDeserializePrefix = typeof(PdszDatabase).GetMethod("ConfigurationContainerDeserializePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodConfigurationContainerDeserializePrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ConfigurationContainerDeserializePrefix not found");
                    return null;
                }

                MethodInfo methodConfigurationContainerDeserializePostfix = typeof(PdszDatabase).GetMethod("ConfigurationContainerDeserializePostfix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodConfigurationContainerDeserializePostfix == null)
                {
                    log.ErrorFormat("ReadServiceModule methodConfigurationContainerDeserializePostfix not found");
                    return null;
                }

                if (_istaServiceDialogDlgCmdBaseConstructor == null)
                {
                    ConstructorInfo[] istaServiceDialogDlgCmdBaseConstructors = istaServiceDialogDlgCmdBaseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    if (istaServiceDialogDlgCmdBaseConstructors.Length != 1)
                    {
                        log.ErrorFormat("ReadServiceModule ServiceDialogCmdBase constructor not found");
                        return null;
                    }

                    ConstructorInfo istaServiceDialogDlgCmdBaseConstructor = istaServiceDialogDlgCmdBaseConstructors[0];
                    ParameterInfo[] istaServiceDialogDlgCmdBaseConstructorParameters = istaServiceDialogDlgCmdBaseConstructor.GetParameters();
                    if (istaServiceDialogDlgCmdBaseConstructorParameters.Length != 5)
                    {
                        log.ErrorFormat("ReadServiceModule ServiceDialogCmdBase parameter count invalid: {0}", istaServiceDialogDlgCmdBaseConstructorParameters.Length);
                        return null;
                    }

                    _istaServiceDialogDlgCmdBaseConstructor = istaServiceDialogDlgCmdBaseConstructor;
                }

                if (_istaEdiabasAdapterDeviceResultConstructor == null)
                {
                    Type istaEdiabasAdapterDeviceResultType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.ISTA.CoreFramework.EDIABASAdapterDeviceResult");
                    if (istaEdiabasAdapterDeviceResultType == null)
                    {
                        log.ErrorFormat("ReadServiceModule EDIABASAdapterDeviceResult not found");
                        return null;
                    }

                    ConstructorInfo istaEdiabasAdapterDeviceResultConstructor = istaEdiabasAdapterDeviceResultType.GetConstructor(Type.EmptyTypes);
                    if (istaEdiabasAdapterDeviceResultConstructor == null)
                    {
                        log.ErrorFormat("ReadServiceModule EDIABASAdapterDeviceResult constructor not found");
                        return null;
                    }

                    _istaEdiabasAdapterDeviceResultConstructor = istaEdiabasAdapterDeviceResultConstructor;
                }

                Type istaModuleType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ISTAModule");
                if (istaModuleType == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule not found");
                    return null;
                }

                MethodInfo methodIstaModuleModuleRef = istaModuleType.GetMethod("callModuleRef", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodIstaModuleModuleRef == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule callModuleRef not found");
                    return null;
                }

                MethodInfo methodModuleRefPrefix = typeof(PdszDatabase).GetMethod("CallModuleRefPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleRefPrefix == null)
                {
                    log.ErrorFormat("ReadTestModule CallModuleRefPrefix not found");
                    return null;
                }

                MethodInfo methodIstaModuleIndirectDocument2 = istaModuleType.GetMethod("__IndirectDocument", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new Type[] { typeof(string), typeof(string)}, null);
                if (methodIstaModuleIndirectDocument2 == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule __IndirectDocument 2 not found");
                    return null;
                }

                MethodInfo methodIstaModuleIndirectDocument3 = istaModuleType.GetMethod("__IndirectDocument", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new Type[] {typeof(string), typeof(string), typeof(string)}, null);
                if (methodIstaModuleIndirectDocument3 == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule __IndirectDocument 3 not found");
                    return null;
                }

                MethodInfo methodIndirectDocumentPrefix2 = typeof(PdszDatabase).GetMethod("IndirectDocumentPrefix2", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodIndirectDocumentPrefix2 == null)
                {
                    log.ErrorFormat("ReadServiceModule IndirectDocumentPrefix2 not found");
                    return null;
                }

                MethodInfo methodIndirectDocumentPrefix3 = typeof(PdszDatabase).GetMethod("IndirectDocumentPrefix3", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodIndirectDocumentPrefix3 == null)
                {
                    log.ErrorFormat("ReadServiceModule IndirectDocumentPrefix3 not found");
                    return null;
                }

                MethodInfo methodGetDatabasePrefix = typeof(PdszDatabase).GetMethod("CallGetDatabaseProviderSQLitePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodGetDatabasePrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule CallGetDatabaseProviderSQLitePrefix not found");
                    return null;
                }

                bool patchedCreateServiceDialog = false;
                bool patchedServiceDialogCmdBaseInvoke = false;
                bool patchedConfigurationContainerDeserialize = false;
                bool patchedModuleRef = false;
                bool patchedIndirectDocumentPrefix = false;
                bool patchedGetDatabase = false;
                foreach (MethodBase methodBase in _harmony.GetPatchedMethods())
                {
                    log.InfoFormat("ReadServiceModule Patched: {0}", methodBase.Name);

                    if (methodBase == methodCreateServiceDialog)
                    {
                        patchedCreateServiceDialog = true;
                    }

                    if (methodBase == methodIstaServiceDialogDlgCmdBaseInvoke)
                    {
                        patchedServiceDialogCmdBaseInvoke = true;
                    }

                    if (methodBase == methodConfigurationContainerDeserialize)
                    {
                        patchedConfigurationContainerDeserialize = true;
                    }

                    if (methodBase == methodIstaModuleModuleRef)
                    {
                        patchedModuleRef = true;
                    }

                    if (methodBase == methodIstaModuleIndirectDocument2)
                    {
                        patchedIndirectDocumentPrefix = true;
                    }

                    if (methodBase == methodGetDatabaseProviderSQLite)
                    {
                        patchedGetDatabase = true;
                    }
                }

                if (!patchedCreateServiceDialog)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodCreateServiceDialog.Name);
                    _harmony.Patch(methodCreateServiceDialog, new HarmonyMethod(methodCreateServiceDialogPrefix));
                }

                if (!patchedServiceDialogCmdBaseInvoke)
                {
                    log.InfoFormat("ServiceDialogCmdBase Patching: {0}", methodIstaServiceDialogDlgCmdBaseInvoke.Name);
                    _harmony.Patch(methodIstaServiceDialogDlgCmdBaseInvoke, new HarmonyMethod(methodServiceDialogCmdBaseInvokePrefix));
                }

                if (!patchedConfigurationContainerDeserialize)
                {
                    log.InfoFormat("ConfigurationContainer Patching: {0}", methodConfigurationContainerDeserializePrefix.Name);
                    _harmony.Patch(methodConfigurationContainerDeserialize,
                        new HarmonyMethod(methodConfigurationContainerDeserializePrefix), new HarmonyMethod(methodConfigurationContainerDeserializePostfix));
                }

                if (!patchedModuleRef)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleModuleRef.Name);
                    _harmony.Patch(methodIstaModuleModuleRef, new HarmonyMethod(methodModuleRefPrefix));
                }

                if (!patchedIndirectDocumentPrefix)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleIndirectDocument2.Name);
                    _harmony.Patch(methodIstaModuleIndirectDocument2, new HarmonyMethod(methodIndirectDocumentPrefix2));

                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleIndirectDocument3.Name);
                    _harmony.Patch(methodIstaModuleIndirectDocument3, new HarmonyMethod(methodIndirectDocumentPrefix3));
                }

                if (!patchedGetDatabase)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodGetDatabaseProviderSQLite.Name);
                    _harmony.Patch(methodGetDatabaseProviderSQLite, new HarmonyMethod(methodGetDatabasePrefix));
                }

                Assembly moduleAssembly = Assembly.LoadFrom(moduleFile);
                Type[] exportedTypes = moduleAssembly.GetExportedTypes();
                Type moduleType = null;
                foreach (Type type in exportedTypes)
                {
                    log.InfoFormat("ReadTestModule Exported type: {0}", type.FullName);
                    if (moduleType == null)
                    {
                        if (!string.IsNullOrEmpty(type.FullName) &&
                            type.FullName.StartsWith("BMW.Rheingold.Module.", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleType = type;
                        }
                    }
                }

                if (moduleType == null)
                {
                    log.ErrorFormat("ReadServiceModule No module type found");
                    return null;
                }

                log.InfoFormat("ReadServiceModule Using module type: {0}", moduleType.FullName);

                List<MethodInfo> simpleMethods = new List<MethodInfo>();
                MethodInfo[] privateMethods = moduleType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (MethodInfo privateMethod in privateMethods)
                {
                    string methodName = privateMethod.Name;
                    if (methodName.StartsWith("__"))
                    {
                        continue;
                    }

                    if (methodName.StartsWith("get_"))
                    {
                        continue;
                    }

                    if (methodName.StartsWith("Get"))
                    {
                        continue;
                    }

                    if (methodName == "Finalize")
                    {
                        continue;
                    }

                    if (methodName == "MemberwiseClone")
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = privateMethod.GetParameters();
                    if (parameters.Any())
                    {
                        continue;
                    }

                    simpleMethods.Add(privateMethod);
                }

                if (simpleMethods.Count > 0)
                {
                    StringBuilder sbSimpleMethods = new StringBuilder();
                    foreach (MethodInfo simpleMethod in simpleMethods)
                    {
                        if (sbSimpleMethods.Length > 0)
                        {
                            sbSimpleMethods.Append(", ");
                        }

                        sbSimpleMethods.Append(simpleMethod.Name);
                    }

                    log.InfoFormat("ReadServiceModule Simple methods: {0}", sbSimpleMethods);

                    object moduleParamContainerInst = CreateModuleParamContainerInst(coreFrameworkAssembly, out _);
                    if (moduleParamContainerInst == null)
                    {
                        log.ErrorFormat("ReadServiceModule CreateModuleParamContainerInst failed");
                    }

                    object testModule = Activator.CreateInstance(moduleType, moduleParamContainerInst);
                    log.InfoFormat("ReadTestModule Module loaded: {0}, Type: {1}", fileName, moduleType.FullName);

                    _serviceDialogDict = null;
                    foreach (MethodInfo simpleMethod in simpleMethods)
                    {
                        Thread moduleThread = new Thread(() =>
                        {
                            try
                            {
                                _serviceDialogCallsDict = null;
                                _moduleRefPath = null;
                                _moduleRefDict = null;
                                simpleMethod.Invoke(testModule, null);
                                log.InfoFormat("ReadServiceModule Method executed: {0}", simpleMethod.Name);
                            }
                            catch (Exception e)
                            {
                                log.ErrorFormat("ReadServiceModule Method: {0}, Exception: '{1}'", simpleMethod.Name,
                                    EdiabasLib.EdiabasNet.GetExceptionText(e));
                            }
                        });

                        moduleThread.Start();
                        if (!moduleThread.Join(3000))
                        {
                            log.ErrorFormat("ReadServiceModule Thread timeout");
                            moduleThread.Abort();
                        }
                    }
                }

                SerializableDictionary<string, ServiceModuleDataItem> serviceDialogDict = _serviceDialogDict;
                _serviceDialogDict = null;
                _serviceDialogCallsDict = null;
                _moduleRefDict = null;
                if (serviceDialogDict == null || serviceDialogDict.Count == 0)
                {
                    log.ErrorFormat("ReadServiceModule No data for: {0}", fileName);
                    return null;
                }

                log.InfoFormat("ReadServiceModule Items: {0}", serviceDialogDict.Count);

                foreach (KeyValuePair<string, ServiceModuleDataItem> dictEntry in serviceDialogDict)
                {
                    ServiceModuleDataItem dataItem = dictEntry.Value;
                    string ediabasJobBare = DetectVehicle.ConvertContainerXml(dataItem.ContainerXml);
                    if (!string.IsNullOrEmpty(ediabasJobBare))
                    {
                        log.InfoFormat("ReadServiceModule EdiabasJob bare: '{0}'", ediabasJobBare);
                        dataItem.EdiabasJobBare = ediabasJobBare;
                    }
                    else
                    {
                        log.ErrorFormat("ReadServiceModule ConvertContainerXml failed: '{0}'", dataItem.MethodName);
                    }

                    if (dataItem.RunOverrides != null && dataItem.RunOverrides.Count > 0)
                    {
                        Dictionary<string, string> runOverrides = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, string> runOverride in dataItem.RunOverrides)
                        {
                            string value = runOverride.Value;
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                value = "[OVERRIDE]";
                            }
                            runOverrides.Add(runOverride.Key, value);
                        }

                        string ediabasJobOverride = DetectVehicle.ConvertContainerXml(dataItem.ContainerXml, runOverrides);
                        if (!string.IsNullOrEmpty(ediabasJobOverride))
                        {
                            log.InfoFormat("ReadServiceModule EdiabasJob override: '{0}'", ediabasJobOverride);
                            dataItem.EdiabasJobOverride = ediabasJobOverride;
                        }
                        else
                        {
                            log.ErrorFormat("ReadServiceModule ConvertContainerXml failed: '{0}'", dataItem.MethodName);
                        }
                    }

                    if (!string.IsNullOrEmpty(dataItem.ControlId))
                    {
                        SwiInfoObj infoObject = GetInfoObjectByControlId(dataItem.ControlId);
                        if (infoObject != null)
                        {
                            log.InfoFormat("ReadServiceModule InfoObject Id: {0}, Identifer: {1}", infoObject.Id, infoObject.Identifier);
                        }
                    }
                }

                log.InfoFormat("ReadServiceModule Finished: {0}", fileName);

                return new ServiceModuleData(serviceDialogDict);
            }
            catch (Exception e)
            {
                failure = true;
                log.ErrorFormat("ReadServiceModule Exception: '{0}'", e.Message);
                return null;
            }
        }

        public bool GenerateEcuCharacteristicsData()
        {
            try
            {
                EcuCharacteristicsData ecuCharacteristicsData = null;
                XmlSerializer serializer = new XmlSerializer(typeof(EcuCharacteristicsData));
                string ecuCharacteristicsZipFile = Path.Combine(_databaseExtractPath, EcuCharacteristicsZipFile);
                if (File.Exists(ecuCharacteristicsZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(ecuCharacteristicsZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, EcuCharacteristicsXmFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        ecuCharacteristicsData = serializer.Deserialize(reader) as EcuCharacteristicsData;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (zf != null)
                            {
                                zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                                zf.Close(); // Ensure we release resources
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Deserialize Exception: '{0}'", e.Message);
                    }
                }

                bool dataValid = true;
                if (ecuCharacteristicsData != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (ecuCharacteristicsData.Version == null || !ecuCharacteristicsData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Version mismatch");
                        dataValid = false;
                    }
                }

                if (ecuCharacteristicsData == null || !dataValid)
                {
                    log.InfoFormat("GenerateEcuCharacteristicsData Converting Xml");
                    if (!IsExecutable())
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Started from DLL");
                        return false;
                    }

                    ecuCharacteristicsData = ReadEcuCharacteristicsXml();
                    if (ecuCharacteristicsData == null)
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData ReadEcuCharacteristicsXml failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        serializer.Serialize(memStream, ecuCharacteristicsData);
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(ecuCharacteristicsZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(EcuCharacteristicsXmFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                EcuCharacteristicsStorage = ecuCharacteristicsData;
                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("GenerateEcuCharacteristicsData Exception: '{0}'", e.Message);
                return false;
            }
        }

        public EcuCharacteristicsData ReadEcuCharacteristicsXml()
        {
            try
            {
                SerializableDictionary<string, string> ecuXmlDict = new SerializableDictionary<string, string>();
                string diagnosticsFile = Path.Combine(_frameworkPath, "RheingoldDiagnostics.dll");
                if (!File.Exists(diagnosticsFile))
                {
                    log.ErrorFormat("ReadEcuCharacteristicsXml Diagnostics file not found: {0}", diagnosticsFile);
                    return null;
                }

                Assembly diagnosticsAssembly = Assembly.LoadFrom(diagnosticsFile);
                string[] resourceNames = diagnosticsAssembly.GetManifestResourceNames();
                foreach (string resourceName in resourceNames)
                {
                    log.InfoFormat("ReadEcuCharacteristicsXml Resource: {0}", resourceName);

                    string[] resourceParts = resourceName.Split('.');
                    if (resourceParts.Length < 2)
                    {
                        log.ErrorFormat("ReadEcuCharacteristicsXml Invalid resource parts: {0}", resourceParts.Length);
                        continue;
                    }

                    string fileName = resourceParts[resourceParts.Length - 2];
                    if (string.IsNullOrEmpty(fileName))
                    {
                        log.ErrorFormat("ReadEcuCharacteristicsXml Invalid file name: {0}", resourceName);
                        continue;
                    }

                    string fileExt = resourceParts[resourceParts.Length - 1];
                    if (string.Compare(fileExt, "xml", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }

                    using (Stream resourceStream = diagnosticsAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (resourceStream == null)
                        {
                            log.ErrorFormat("ReadEcuCharacteristicsXml Reading stream failed for: {0}", resourceName);
                            continue;
                        }

                        using (StreamReader reader = new StreamReader(resourceStream))
                        {
                            string xmlContent = reader.ReadToEnd();
                            ecuXmlDict.Add(fileName.ToUpperInvariant(), xmlContent);
                        }
                    }
                }

                log.InfoFormat("ReadEcuCharacteristicsXml Resources: {0}", ecuXmlDict.Count);
                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                EcuCharacteristicsData ecuCharacteristicsData = new EcuCharacteristicsData(versionInfo, ecuXmlDict);
                return ecuCharacteristicsData;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ReadEcuCharacteristicsXml Exception: '{0}'", e.Message);
                return null;
            }
        }

        public bool SaveVehicleSeriesInfo(ClientContext clientContext)
        {
            try
            {
                VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = null;
                XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.VehicleSeriesInfoData));
                string vehicleSeriesFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.VehicleSeriesXmlFile);
                try
                {
                    if (File.Exists(vehicleSeriesFile))
                    {
                        using (FileStream fileStream = new FileStream(vehicleSeriesFile, FileMode.Open))
                        {
                            vehicleSeriesInfoData = serializer.Deserialize(fileStream) as VehicleStructsBmw.VehicleSeriesInfoData;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("SaveVehicleSeriesInfo Deserialize Exception: '{0}'", e.Message);
                }

                bool dataValid = true;
                if (vehicleSeriesInfoData != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (vehicleSeriesInfoData.Version == null || !vehicleSeriesInfoData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Version mismatch");
                        dataValid = false;
                    }
                }

                if (vehicleSeriesInfoData == null || !dataValid)
                {
                    vehicleSeriesInfoData = ExtractVehicleSeriesInfo(clientContext);
                    if (vehicleSeriesInfoData == null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo ExtractVehicleSeriesInfo failed");
                        return false;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo Saving: {0}", vehicleSeriesFile);
                    using (FileStream fileStream = File.Create(vehicleSeriesFile))
                    {
                        serializer.Serialize(fileStream, vehicleSeriesInfoData);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        public VehicleStructsBmw.VehicleSeriesInfoData ExtractVehicleSeriesInfo(ClientContext clientContext)
        {
            try
            {
                Regex seriesFormulaRegex = new Regex(@"IsValidRuleString\(""(Baureihenverbund|E-Bezeichnung)"",\s*""([a-z0-9\- ]+)""\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Regex brandFormulaRegex = new Regex(@"IsValidRuleString\(""(Marke)"",\s*""([a-z0-9\- ]+)""\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Regex dateFormulaRegex = new Regex(@"(RuleNum\(""Baustand""\))\s*([<>=]+)\s*([0-9]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                RuleExpression.FormulaConfig formulaConfig = new RuleExpression.FormulaConfig("RuleString", "RuleNum", "IsValidRuleString", "IsValidRuleNum", "|");

                Vehicle vehicle = new Vehicle(clientContext);
                List<EcuCharacteristicsInfo> vehicleSeriesList = new List<EcuCharacteristicsInfo>();
                List<BordnetsData> boardnetsList = GetAllBordnetRules();
                foreach (BordnetsData bordnetsData in boardnetsList)
                {
                    BaseEcuCharacteristics baseEcuCharacteristics = null;
                    if (bordnetsData.DocData != null)
                    {
                        baseEcuCharacteristics = VehicleLogistics.CreateCharacteristicsInstance<GenericEcuCharacteristics>(vehicle, bordnetsData.DocData, bordnetsData.InfoObjIdent);
                    }

                    if (baseEcuCharacteristics != null && bordnetsData.XepRule != null)
                    {
                        string ruleFormula = bordnetsData.XepRule.GetRuleFormula(vehicle, formulaConfig);
                        if (!string.IsNullOrEmpty(ruleFormula))
                        {
                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Rule formula: {0}", ruleFormula);

                            HashSet<string> seriesHash = new HashSet<string>();
                            HashSet<string> brandHash = new HashSet<string>();
                            string date = null;
                            string dateCompare = null;

                            string[] formulaParts = ruleFormula.Split('|');
                            foreach (string formulaPart in formulaParts)
                            {
                                if (string.IsNullOrWhiteSpace(formulaPart))
                                {
                                    continue;
                                }

                                MatchCollection seriesMatches = seriesFormulaRegex.Matches(formulaPart);
                                foreach (Match match in seriesMatches)
                                {
                                    if (match.Groups.Count == 3 && match.Groups[2].Success)
                                    {
                                        seriesHash.Add(match.Groups[2].Value.Trim());
                                    }
                                }

                                MatchCollection brandMatches = brandFormulaRegex.Matches(formulaPart);
                                foreach (Match match in brandMatches)
                                {
                                    if (match.Groups.Count == 3 && match.Groups[2].Success)
                                    {
                                        brandHash.Add(match.Groups[2].Value.Trim());
                                        break;
                                    }
                                }

                                MatchCollection dateMatches = dateFormulaRegex.Matches(formulaPart);
                                foreach (Match match in dateMatches)
                                {
                                    if (match.Groups.Count == 4 && match.Groups[2].Success && match.Groups[3].Success)
                                    {
                                        date = match.Groups[3].Value.Trim();
                                        dateCompare = match.Groups[2].Value.Trim();
                                        break;
                                    }
                                }
                            }

                            // detect bn type
                            HashSet<BNType> bnTypes = new HashSet<BNType>();
                            Vehicle vehicleSeries = new Vehicle(clientContext);
                            foreach (string series in seriesHash)
                            {
                                vehicleSeries.Ereihe = series;
                                BNType bnType = DiagnosticsBusinessData.Instance.GetBNType(vehicleSeries);
                                if (bnType != BNType.UNKNOWN)
                                {
                                    bnTypes.Add(bnType);
                                }
                            }

                            BNType? bnTypeSeries = null;
                            if (bnTypes.Count == 1)
                            {
                                bnTypeSeries = bnTypes.First();
                            }

                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Sgbd: {0}, Brand: {1}, Series: {2}, BnType: {3}, Date: {4} {5}",
                                baseEcuCharacteristics.brSgbd, brandHash.ToStringItems(), seriesHash.ToStringItems(), bnTypeSeries, dateCompare ?? string.Empty, date ?? string.Empty);
                            vehicleSeriesList.Add(new EcuCharacteristicsInfo(baseEcuCharacteristics, seriesHash.ToList(), bnTypeSeries, brandHash.ToList(), date, dateCompare));
                        }
                    }
                }

                SerializableDictionary<string, List<VehicleStructsBmw.VehicleSeriesInfo>> sgbdDict = new SerializableDictionary<string, List<VehicleStructsBmw.VehicleSeriesInfo>>();
                foreach (EcuCharacteristicsInfo ecuCharacteristicsInfo in vehicleSeriesList)
                {
                    BaseEcuCharacteristics ecuCharacteristics = ecuCharacteristicsInfo.EcuCharacteristics;
                    string brSgbd = ecuCharacteristics.brSgbd.Trim().ToUpperInvariant();
                    BNType? bnType = ecuCharacteristicsInfo.BnType;

                    string bnTypeName = null;
                    List<VehicleStructsBmw.VehicleEcuInfo> ecuList = null;
                    if (bnType.HasValue)
                    {
                        bnTypeName = bnType.Value.ToString();
                        if (bnType.Value == BNType.IBUS)
                        {
                            ecuList = new List<VehicleStructsBmw.VehicleEcuInfo>();
                            foreach (IEcuLogisticsEntry ecuLogisticsEntry in ecuCharacteristics.ecuTable)
                            {
                                ecuList.Add(new VehicleStructsBmw.VehicleEcuInfo(ecuLogisticsEntry.DiagAddress, ecuLogisticsEntry.Name, ecuLogisticsEntry.GroupSgbd));
                            }
                        }
                    }

                    foreach (string series in ecuCharacteristicsInfo.SeriesList)
                    {
                        string key = series.ToUpperInvariant();
                        VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfoAdd = new VehicleStructsBmw.VehicleSeriesInfo(key, brSgbd, bnTypeName, ecuCharacteristicsInfo.BrandList, ecuList, ecuCharacteristicsInfo.Date, ecuCharacteristicsInfo.DateCompare);

                        if (sgbdDict.TryGetValue(key, out List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList))
                        {
                            bool sgbdFound = false;
                            foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo in vehicleSeriesInfoList)
                            {
                                if (string.Compare(vehicleSeriesInfo.BrSgbd, brSgbd, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    sgbdFound = true;
                                }
                            }

                            if (!sgbdFound)
                            {
                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Multiple entries for Series: {0}", series);
                                vehicleSeriesInfoList.Add(vehicleSeriesInfoAdd);
                            }
                        }
                        else
                        {
                            sgbdDict.Add(key, new List<VehicleStructsBmw.VehicleSeriesInfo> { vehicleSeriesInfoAdd });
                        }
                    }
                }

                foreach (KeyValuePair<string, List<VehicleStructsBmw.VehicleSeriesInfo>> keyValue in sgbdDict)
                {
                    List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList = keyValue.Value;
                    if (vehicleSeriesInfoList.Count == 1)
                    {
                        vehicleSeriesInfoList[0].ResetDate();
                    }
                }

                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = new VehicleStructsBmw.VehicleSeriesInfoData(timeStamp, versionInfo, sgbdDict);
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, List<VehicleStructsBmw.VehicleSeriesInfo>> keyValue in vehicleSeriesInfoData.VehicleSeriesDict.OrderBy(x => x.Key))
                {
                    List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList = keyValue.Value;
                    foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo in vehicleSeriesInfoList)
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0}, {1}, '{2}' {3} {4}]",
                            vehicleSeriesInfo.BrSgbd, vehicleSeriesInfo.Series, vehicleSeriesInfo.BrandList.ToStringItems(), vehicleSeriesInfo.DateCompare ?? string.Empty, vehicleSeriesInfo.Date ?? string.Empty));
                    }
                }

                log.InfoFormat("ExtractEcuCharacteristicsVehicles Count: {0}", vehicleSeriesInfoData.VehicleSeriesDict.Count);
                log.Info(Environment.NewLine + sb);
                return vehicleSeriesInfoData;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractEcuCharacteristicsVehicles Exception: '{0}'", e.Message);
                return null;
            }
        }

        public bool SaveFaultRulesInfo(ClientContext clientContext)
        {
            try
            {
                string rulesZipFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.RulesZipFile);
                string rulesCsFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.RulesCsFile);
                VehicleStructsBmw.RulesInfoData rulesInfoData = null;
                if (File.Exists(rulesZipFile) && File.Exists(rulesCsFile))
                {
                    rulesInfoData = VehicleInfoBmw.ReadRulesInfoFromFile(_databaseExtractPath);
                }

                DbInfo dbInfo = GetDbInfo();
                bool dataValid = true;
                if (rulesInfoData != null)
                {
                    if (rulesInfoData.Version == null || !rulesInfoData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("SaveFaultRulesInfo Version mismatch");
                        dataValid = false;
                    }
                }

                if (rulesInfoData != null && dataValid)
                {
                    return true;
                }

                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> faultRulesDict = ExtractFaultRulesInfo(clientContext);
                if (faultRulesDict == null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo ExtractFaultRulesInfo failed");
                    return false;
                }

                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ecuFuncRulesDict = ExtractEcuFuncRulesInfo(clientContext);
                if (ecuFuncRulesDict == null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo ExtractEcuFuncRulesInfo failed");
                    return false;
                }

                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                rulesInfoData = new VehicleStructsBmw.RulesInfoData(versionInfo, faultRulesDict, ecuFuncRulesDict);
                if (!SaveFaultRulesClass(rulesInfoData, rulesCsFile))
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo SaveFaultRulesFunction failed");
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo Saving: {0}", rulesZipFile);

                using (MemoryStream memStream = new MemoryStream())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.RulesInfoData));
                    serializer.Serialize(memStream, rulesInfoData);
                    memStream.Seek(0, SeekOrigin.Begin);

                    FileStream fsOut = File.Create(rulesZipFile);
                    ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                    zipStream.SetLevel(3);

                    try
                    {
                        ZipEntry newEntry = new ZipEntry(VehicleStructsBmw.RulesXmlFile)
                        {
                            DateTime = DateTime.Now,
                            Size = memStream.Length
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[4096];
                        StreamUtils.Copy(memStream, zipStream, buffer);
                        zipStream.CloseEntry();
                    }
                    finally
                    {
                        zipStream.IsStreamOwner = true;
                        zipStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        public bool SaveFaultRulesClass(VehicleStructsBmw.RulesInfoData rulesInfoData, string fileName)
        {
            try
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesFunction Saving: {0}", fileName);

                if (rulesInfoData == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesFunction faultRulesInfoData missing");
                    return false;
                }

                DbInfo dbInfo = GetDbInfo();
                if (dbInfo == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesFunction GetDbInfo failed");
                    return false;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(
$@"using BmwFileReader;

public class RulesInfo
{{
    public const string DatabaseVersion = ""{dbInfo.Version}"";

    public const string DatabaseDate = ""{dbInfo.DateTime.ToString(CultureInfo.InvariantCulture)}"";

    public RuleEvalBmw RuleEvalClass {{ get; private set; }}

    public RulesInfo(RuleEvalBmw ruleEvalBmw)
    {{
        RuleEvalClass = ruleEvalBmw;
    }}

    public bool IsFaultRuleValid(string id)
    {{
        switch (id.Trim())
        {{
");
                foreach (KeyValuePair<string, VehicleStructsBmw.RuleInfo> ruleInfo in rulesInfoData.FaultRuleDict)
                {
                    sb.Append(
$@"            case ""{ruleInfo.Value.Id.Trim()}"":
                return {ruleInfo.Value.RuleFormula};
"
                    );
                }
                sb.Append(
@"
        }

        RuleNotFound(id.Trim());
        return true;
    }

    public bool IsEcuFuncRuleValid(string id)
    {
        switch (id.Trim())
        {
");
                foreach (KeyValuePair<string, VehicleStructsBmw.RuleInfo> ruleInfo in rulesInfoData.EcuFuncRuleDict)
                {
                    sb.Append(
$@"            case ""{ruleInfo.Value.Id.Trim()}"":
                return {ruleInfo.Value.RuleFormula};
"
                    );
                }
                sb.Append(
                    @"
        }

        RuleNotFound(id.Trim());
        return true;
    }

    private void RuleNotFound(string id)
    {
        if (RuleEvalClass != null)
        {
            RuleEvalClass.RuleNotFound(id);
        }
    }

    private string RuleString(string name)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.RuleString(name);
        }
        return string.Empty;
    }

    private long RuleNum(string name)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.RuleNum(name);
        }
        return -1;
    }

    private bool IsValidRuleString(string name, string value)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.IsValidRuleString(name, value);
        }
        return false;
    }

    private bool IsValidRuleNum(string name, long value)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.IsValidRuleNum(name, value);
        }
        return false;
    }
}
");
                File.WriteAllText(fileName, sb.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        public SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ExtractFaultRulesInfo(ClientContext clientContext)
        {
            try
            {
                List<EcuFunctionStructs.EcuFaultCode> ecuFaultCodeList = new List<EcuFunctionStructs.EcuFaultCode>();
                string sql = @"SELECT ID, CODE, DATATYPE, RELEVANCE FROM XEP_FAULTCODES";
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EcuFunctionStructs.EcuFaultCode ecuFaultCode = new EcuFunctionStructs.EcuFaultCode(
                                reader["ID"].ToString().Trim(),
                                reader["CODE"].ToString(),
                                reader["DATATYPE"].ToString(),
                                reader["RELEVANCE"].ToString());
                            ecuFaultCodeList.Add(ecuFaultCode);
                        }
                    }
                }

                Vehicle vehicle = new Vehicle(clientContext);
                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ruleDict = new SerializableDictionary<string, VehicleStructsBmw.RuleInfo>();
                foreach (EcuFunctionStructs.EcuFaultCode ecuFaultCode in ecuFaultCodeList)
                {
                    if (ecuFaultCode.Relevance.ConvertToInt() > 0)
                    {
                        XepRule xepRule = GetRuleById(ecuFaultCode.Id);
                        if (xepRule != null)
                        {
                            string ruleFormula = xepRule.GetRuleFormula(vehicle);
                            if (!string.IsNullOrEmpty(ruleFormula))
                            {
                                ruleDict.Add(ecuFaultCode.Id, new VehicleStructsBmw.RuleInfo(ecuFaultCode.Id, ruleFormula));
                            }
                        }
                    }
                }

                return ruleDict;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractFaultRulesInfo Exception: '{0}'", e.Message);
                return null;
            }
        }

        public SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ExtractEcuFuncRulesInfo(ClientContext clientContext)
        {
            try
            {
                List<string> ecuFixedFuncList = new List<string>();
                string sql = @"SELECT ID FROM XEP_ECUFIXEDFUNCTIONS";
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuFixedFuncList.Add(reader["ID"].ToString().Trim());
                        }
                    }
                }

                Vehicle vehicle = new Vehicle(clientContext);
                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ruleDict = new SerializableDictionary<string, VehicleStructsBmw.RuleInfo>();
                foreach (string ecuFixedFuncId in ecuFixedFuncList)
                {
                    if (ecuFixedFuncId.ConvertToInt() > 0)
                    {
                        XepRule xepRule = GetRuleById(ecuFixedFuncId);
                        if (xepRule != null)
                        {
                            string ruleFormula = xepRule.GetRuleFormula(vehicle);
                            if (!string.IsNullOrEmpty(ruleFormula))
                            {
                                ruleDict.Add(ecuFixedFuncId, new VehicleStructsBmw.RuleInfo(ecuFixedFuncId, ruleFormula));
                            }
                        }
                    }
                }

                return ruleDict;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractEcuFuncRulesInfo Exception: '{0}'", e.Message);
                return null;
            }
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

        public List<SwiAction> GetSwiActionsForRegister(SwiRegisterEnum swiRegisterEnum, bool getChildren)
        {
            SwiRegister swiRegister = FindNodeForRegister(swiRegisterEnum);
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

        public SwiRegister FindNodeForRegister(SwiRegisterEnum swiRegisterEnum)
        {
            try
            {
                if (SwiRegisterTree == null)
                {
                    log.ErrorFormat("FindNodeForRegister No tree");
                    return null;
                }

                string registerId = SwiRegisterEnumerationNameConverter(swiRegisterEnum);
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

        public EcuVar GetEcuVariantByName(string sgbdName)
        {
            if (string.IsNullOrEmpty(sgbdName))
            {
                return null;
            }

            EcuVar ecuVar = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, FAULTMEMORYDELETEWAITINGTIME, NAME, " + DatabaseFunctions.SqlTitleItems + ", VALIDFROM, VALIDTO, SICHERHEITSRELEVANT, ECUGROUPID, SORT FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", sgbdName.ToLowerInvariant());
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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

        public EcuVar GetEcuVariantById(string varId)
        {
            if (string.IsNullOrEmpty(varId))
            {
                return null;
            }

            EcuVar ecuVar = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, FAULTMEMORYDELETEWAITINGTIME, NAME, " + DatabaseFunctions.SqlTitleItems + ", VALIDFROM, VALIDTO, SICHERHEITSRELEVANT, ECUGROUPID, SORT FROM XEP_ECUVARIANTS WHERE (ID = {0})", varId);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString().Trim();
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

        public EcuVar FindEcuVariantFromBntn(string bnTnName, int? diagAddrAsInt, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(bnTnName))
            {
                return null;
            }
            List<EcuVar> ecuVars = FindEcuVariantsFromBntn(bnTnName, vehicle, ffmResolver);
            if (ecuVars == null || ecuVars.Count == 0)
            {
                return null;
            }

            EcuVar ecuVar = ecuVars.FirstOrDefault((EcuVar x) => vehicle.ECU != null && vehicle.ECU.Any((ECU i) => string.Compare(x.Name, i.ECU_SGBD, StringComparison.InvariantCultureIgnoreCase) == 0));
            if (ecuVar != null)
            {
                return ecuVar;
            }

            if (diagAddrAsInt == null)
            {
                return null;
            }

            ObservableCollection<ECU> ecu = vehicle.ECU;
            ECU ecu2 = (ecu != null) ? ecu.FirstOrDefault(delegate (ECU v)
            {
                long id_SG_ADR = v.ID_SG_ADR;
                int? diagAddrAsInt2 = diagAddrAsInt;
                long? num = (diagAddrAsInt2 != null) ? new long?((long)diagAddrAsInt2.GetValueOrDefault()) : null;
                return id_SG_ADR == num.GetValueOrDefault() & num != null;
            }) : null;

            if (ecu2 != null && !string.IsNullOrEmpty(ecu2.ECU_SGBD))
            {
                ecuVar = GetEcuVariantByName(ecu2.ECU_SGBD);
            }

            return ecuVar;
        }

        private List<EcuVar> FindEcuVariantsFromBntn(string bnTnName, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            log.InfoFormat("FindEcuVariantsFromBntn BnTnName: {0}", bnTnName);
            if (string.IsNullOrEmpty(bnTnName))
            {
                return null;
            }

            List<EcuPrgVar> ecuPrgVars = GetEcuProgrammingVariantByName(bnTnName, vehicle, ffmResolver);
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EcuPrgVar ecuPrgVarTemp = ReadXepEcuPrgVar(reader);
                            bool valid = ecuPrgVarTemp != null;
                            if (vehicle != null && ecuPrgVarTemp != null)
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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

        public EcuClique GetEcuCliqueById(string ecuCliqueId)
        {
            if (string.IsNullOrEmpty(ecuCliqueId))
            {
                return null;
            }

            EcuClique ecuClique = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, CLIQUENKURZBEZEICHNUNG, " + DatabaseFunctions.SqlTitleItems + ", ECUREPID FROM XEP_ECUCLIQUES WHERE (ID = {0})", ecuCliqueId);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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

            return GetEcuCliqueById(ecuRefClique.EcuCliqueId);
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Characteristics characteristics = ReadXepCharacteristics(reader);
                            characteristics.DriveId = reader["DRIVEID"].ToString().Trim();
                            characteristics.RootNodeClass = reader["PARENTNODECLASS"].ToString().Trim();
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

        public VinRanges GetVinRangesByVin17(string vin17_4_7, string vin7, bool returnFirstEntryWithoutCheck)
        {
            log.InfoFormat("GetVinRangesByVin17 Vin17_4_7: {0}, Vin7: {1}", vin17_4_7, vin7);
            if (string.IsNullOrEmpty(vin17_4_7) || string.IsNullOrEmpty(vin7))
            {
                return null;
            }

            List<VinRanges> vinRangesList = new List<VinRanges>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT VINBANDFROM, VINBANDTO, TYPSCHLUESSEL, PRODUCTIONDATEYEAR, PRODUCTIONDATEMONTH, RELEASESTATE, CHANGEDATE, GEARBOX_TYPE, VIN17_4_7" +
                    @" FROM VINRANGES WHERE ('{0}' BETWEEN VINBANDFROM AND VINBANDTO) AND (VIN17_4_7 = '{1}')",
                    vin7.ToUpper(CultureInfo.InvariantCulture), vin17_4_7.ToUpper(CultureInfo.InvariantCulture));
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                log.InfoFormat("GetVinRangesByVin17 List count: {0}", vinRangesList.Count);
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
                log.ErrorFormat("GetVinRangesByVin17 List2 count: {0}", vinRangesList.Count);
                return null;
            }

            if (returnFirstEntryWithoutCheck)
            {
                return GetVinRangesByVin17_4_7(vin17_4_7);
            }

            log.ErrorFormat("GetVinRangesByVin17 Not found: {0}", vin17_4_7);
            return null;
        }

        public VinRanges GetVinRangesByVin17_4_7(string vin17_4_7)
        {
            log.InfoFormat("GetVinRangesByVin17_4_7 Vin17_4_7: {0}", vin17_4_7);
            if (string.IsNullOrEmpty(vin17_4_7))
            {
                return null;
            }

            List<VinRanges> vinRangesList = new List<VinRanges>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT VINBANDFROM, VINBANDTO, TYPSCHLUESSEL, PRODUCTIONDATEYEAR, PRODUCTIONDATEMONTH, RELEASESTATE, CHANGEDATE, GEARBOX_TYPE, VIN17_4_7" +
                    @" FROM VINRANGES WHERE (VIN17_4_7 = '{0}')", vin17_4_7.ToUpper(CultureInfo.InvariantCulture));
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                log.InfoFormat("GetVinRangesByVin17_4_7 List count: {0}", vinRangesList.Count);
            }

            if (vinRangesList.Count >= 1)
            {
                VinRanges vinRanges = vinRangesList.First();
                log.InfoFormat("GetVinRangesByVin17_4_7 TypeKey: {0}", vinRanges.TypeKey);
                return vinRanges;
            }

            log.ErrorFormat("GetVinRangesByVin17_4_7 Not found: {0}", vin17_4_7);
            return null;
        }

        public List<Characteristics> GetVehicleCharacteristics(Vehicle vehicle)
        {
            List<Characteristics> characteristicsList = GetVehicleCharacteristicsFromDatabase(vehicle, false);
            if (characteristicsList != null && characteristicsList.Count > 0 && IsVehicleAnAlpina(vehicle))
            {
                HandleAlpinaVehicles(vehicle, characteristicsList);
            }

            return characteristicsList;
        }

        // ToDo: Check on update
        public static BatteryEnum ResolveBatteryType(Vehicle vecInfo)
        {
            if (new List<string>
                {
                    "F80", "F82", "F83", "F90", "F91", "F92", "F93", "G80", "G81", "G82",
                    "G83", "G90"
                }.Contains(vecInfo.Ereihe))
            {
                return BatteryEnum.LFP;
            }
            if (!vecInfo.IsBev() && !vecInfo.IsPhev() && !vecInfo.IsHybr() && !vecInfo.IsErex() && !vecInfo.Ereihe.Equals("I01") && !vecInfo.hasSA("1CE"))
            {
                return BatteryEnum.Pb;
            }
            return BatteryEnum.PbNew;
        }

        public bool IsVehicleAnAlpina(Vehicle vehicle)
        {
            return vehicle.hasSA("920");
        }

        private void HandleAlpinaVehicles(Vehicle vehicle, List<Characteristics> characteristicsList)
        {
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", NAME, PRODUCT_TYPE FROM XEP_SALAPAS WHERE (NAME = '{0}' AND PRODUCT_TYPE = '{1}')", productType, salesKey);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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

        public List<SwiAction> ReadLinkedSwiActions(List<SwiAction> selectedRegister, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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

        public List<SwiInfoObj> GetServiceProgramsForSwiAction(SwiAction swiAction, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string infoObjId = reader["INFOOBJECTID"].ToString().Trim();
                            string linkTypeId = reader["LINK_TYPE_ID"].ToString().Trim();
                            SwiInfoObj swiInfoObj = GetInfoObjectById(infoObjId, linkTypeId);
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string controlId = reader["DIAGNOSISOBJECTCONTROLID"].ToString().Trim();
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

        public SwiInfoObj GetInfoObjectById(string infoObjectId, string linkTypeId)
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            swiInfoObj = ReadXepSwiInfoObj(reader, SwiInfoObj.GetLinkType(linkTypeId));
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string infoObjId = reader["INFOOBJECTID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            controlId = reader["CONTROLID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string item = reader["ID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            controlId = reader["CONTROLID"].ToString().Trim();
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

        public Dictionary<string, XepRule> LoadXepRules()
        {
            log.InfoFormat("LoadXepRules");
            Dictionary<string, XepRule> xepRuleDict = new Dictionary<string, XepRule>();
            try
            {
                string sql = @"SELECT ID, RULE FROM XEP_RULES";
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString().Trim();
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

        public BordnetsData GetBordnetFromDatabase(Vehicle vecInfo)
        {
            List<BordnetsData> boardnetsList = LoadBordnetsData(vecInfo);
            if (boardnetsList == null)
            {
                log.ErrorFormat("GetBordnetXmlFromDatabase No data");
                return null;
            }

            if (boardnetsList.Count != 1)
            {
                log.ErrorFormat("GetBordnetXmlFromDatabase List items: {0}", boardnetsList.Count);
                return null;
            }

            log.InfoFormat("GetBordnetXmlFromDatabase Valid");
            return boardnetsList[0];
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string infoObjId = reader["INFOOBJECTID"].ToString().Trim();
                            string infoObjIdent = reader["INFOOBJECTIDENTIFIER"].ToString().Trim();
                            string docId = reader["CONTENT_DEDE"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            titleDe = reader["TITLE_DEDE"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            charId = reader["ID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            typeId = reader["ID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            alpinaId = reader["ALPINA_ID"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            iLevel = reader["NAME"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            country = reader["LAENDERKUERZEL"].ToString().Trim();
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
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string version = reader["VERSION"].ToString().Trim();
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

        private static Equipment ReadXepEquipment(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            return new Equipment(id, name, GetTranslation(reader));
        }

        private static EcuClique ReadXepEcuClique(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string cliqueName = reader["CLIQUENKURZBEZEICHNUNG"].ToString().Trim();
            string ecuRepId = reader["ECUREPID"].ToString().Trim();
            return new EcuClique(id, cliqueName, ecuRepId, GetTranslation(reader));
        }

        private static EcuRefClique ReadXepEcuRefClique(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string ecuCliqueId = reader["ECUCLIQUEID"].ToString().Trim();
            return new EcuRefClique(id, ecuCliqueId);
        }

        private static CharacteristicRoots ReadXepCharacteristicRoots(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string motorCycSeq = reader["MOTORCYCLESEQUENCE"].ToString().Trim();
            string vehicleSeq = reader["VEHICLESEQUENCE"].ToString().Trim();
            return new CharacteristicRoots(id, nodeClass, motorCycSeq, vehicleSeq, GetTranslation(reader));
        }

        private static Characteristics ReadXepCharacteristics(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string titleId = reader["TITLEID"].ToString().Trim();
            string istaVisible = reader["ISTA_VISIBLE"].ToString().Trim();
            string staticClassVar = reader["STATICCLASSVARIABLES"].ToString().Trim();
            string staticClassVarMCycle = reader["STATICCLASSVARIABLESMOTORRAD"].ToString().Trim();
            string parentId = reader["PARENTID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string legacyName = reader["LEGACY_NAME"].ToString().Trim();
            return new Characteristics(id, nodeClass, titleId, istaVisible, staticClassVar, staticClassVarMCycle, parentId, name, legacyName, GetTranslation(reader));
        }

        private static VinRanges ReadXepVinRanges(SQLiteDataReader reader)
        {
            string changeDate = reader["CHANGEDATE"].ToString().Trim();
            string productionMonth = reader["PRODUCTIONDATEMONTH"].ToString().Trim();
            string productionYear = reader["PRODUCTIONDATEYEAR"].ToString().Trim();
            string releaseState = reader["RELEASESTATE"].ToString().Trim();
            string typeKey = reader["TYPSCHLUESSEL"].ToString().Trim();
            string vinBandFrom = reader["VINBANDFROM"].ToString().Trim();
            string vinBandTo = reader["VINBANDTO"].ToString().Trim();
            string gearboxType = reader["GEARBOX_TYPE"].ToString().Trim();
            string vin17_4_7 = reader["VIN17_4_7"].ToString().Trim();
            return new VinRanges(changeDate, productionMonth, productionYear, releaseState, typeKey, vinBandFrom, vinBandTo, gearboxType, vin17_4_7);
        }

        private static SaLaPa ReadXepSaLaPa(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string productType = reader["PRODUCT_TYPE"].ToString().Trim();
            return new SaLaPa(id, name, productType, GetTranslation(reader));
        }

        private static EcuReps ReadXepEcuReps(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string ecuShortcut = reader["STEUERGERAETEKUERZEL"].ToString().Trim();
            return new EcuReps(id, ecuShortcut);
        }

        private static EcuVar ReadXepEcuVar(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string faultMemDelWaitTime = reader["FAULTMEMORYDELETEWAITINGTIME"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string validFrom = reader["VALIDFROM"].ToString().Trim();
            string validTo = reader["VALIDTO"].ToString().Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString().Trim();
            string ecuGroupId = reader["ECUGROUPID"].ToString().Trim();
            string sort = reader["SORT"].ToString().Trim();
            return new EcuVar(id, faultMemDelWaitTime, name, validFrom, validTo, safetyRelevant, ecuGroupId, sort, GetTranslation(reader));
        }

        private static EcuPrgVar ReadXepEcuPrgVar(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string flashLimit = reader["FLASHLIMIT"].ToString().Trim();
            string ecuVarId = reader["ECUVARIANTID"].ToString().Trim();
            return new EcuPrgVar(id, name, flashLimit, ecuVarId);
        }

        private static EcuGroup ReadXepEcuGroup(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string obdIdent = reader["OBDIDENTIFICATION"].ToString().Trim();
            string faultMemDelIdent = reader["FAULTMEMORYDELETEIDENTIFICATIO"].ToString().Trim();
            string faultMemDelWaitTime = reader["FAULTMEMORYDELETEWAITINGTIME"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string virt = reader["VIRTUELL"].ToString().Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString().Trim();
            string validFrom = reader["VALIDFROM"].ToString().Trim();
            string validTo = reader["VALIDTO"].ToString().Trim();
            string diagAddr = reader["DIAGNOSTIC_ADDRESS"].ToString().Trim();
            return new EcuGroup(id, obdIdent, faultMemDelIdent, faultMemDelWaitTime, name, virt, safetyRelevant, validFrom, validTo, diagAddr);
        }

        private static SwiAction ReadXepSwiAction(SQLiteDataReader reader, SwiActionSource swiActionSource)
        {
            string id = reader["ID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string actionCategory = reader["ACTIONCATEGORY"].ToString().Trim();
            string selectable = reader["SELECTABLE"].ToString().Trim();
            string showInPlan = reader["SHOW_IN_PLAN"].ToString().Trim();
            string executable = reader["EXECUTABLE"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            return new SwiAction(swiActionSource, id, name, actionCategory, selectable, showInPlan, executable, nodeClass, GetTranslation(reader));
        }

        private static SwiRegister ReadXepSwiRegister(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string parentId = reader["PARENTID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string remark = reader["REMARK"].ToString().Trim();
            string sort = reader["SORT"].ToString().Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString().Trim();
            string identifer = reader["IDENTIFIER"].ToString().Trim();
            return new SwiRegister(id, nodeClass, name, parentId, remark, sort, versionNum, identifer, GetTranslation(reader));
        }

        private static SwiInfoObj ReadXepSwiInfoObj(SQLiteDataReader reader, SwiInfoObj.SwiActionDatabaseLinkType? linkType = null)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string assembly = reader["ASSEMBLY"].ToString().Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString().Trim();
            string programType = reader["PROGRAMTYPE"].ToString().Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString().Trim();
            string titleId = reader["TITLEID"].ToString().Trim();
            string general = reader["GENERELL"].ToString().Trim();
            string telSrvId = reader["TELESERVICEKENNUNG"].ToString().Trim();
            string vehicleComm = reader["FAHRZEUGKOMMUNIKATION"].ToString().Trim();
            string measurement = reader["MESSTECHNIK"].ToString().Trim();
            string hidden = reader["VERSTECKT"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string informationType = reader["INFORMATIONSTYP"].ToString().Trim();
            string identification = reader["IDENTIFIKATOR"].ToString().Trim();
            string informationFormat = reader["INFORMATIONSFORMAT"].ToString().Trim();
            string siNumber = reader["SINUMMER"].ToString().Trim();
            string targetILevel = reader["ZIELISTUFE"].ToString().Trim();
            string controlId = reader["CONTROLID"].ToString().Trim();
            string infoType = reader["INFOTYPE"].ToString().Trim();
            string infoFormat = reader["INFOFORMAT"].ToString().Trim();
            string docNum = reader["DOCNUMBER"].ToString().Trim();
            string priority = reader["PRIORITY"].ToString().Trim();
            string identifier = reader["IDENTIFIER"].ToString().Trim();
            string flowXml = reader["FLOWXML"].ToString().Trim();
            return new SwiInfoObj(linkType, id, nodeClass, assembly, versionNum, programType, safetyRelevant, titleId, general,
                telSrvId, vehicleComm, measurement, hidden, name, informationType, identification, informationFormat, siNumber, targetILevel, controlId,
                infoType, infoFormat, docNum, priority, identifier, flowXml, GetTranslation(reader));
        }

        private static SwiDiagObj ReadXepSwiDiagObj(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string titleId = reader["TITLEID"].ToString().Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string failWeight = reader["FAILUREWEIGHT"].ToString().Trim();
            string hidden = reader["VERSTECKT"].ToString().Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString().Trim();
            string controlId = reader["CONTROLID"].ToString().Trim();
            string sortOrder = reader["SORT_ORDER"].ToString().Trim();
            return new SwiDiagObj(id, nodeClass, titleId, versionNum, name, failWeight, hidden, safetyRelevant, controlId, sortOrder, GetTranslation(reader));
        }

        private static EcuTranslation GetTranslation(SQLiteDataReader reader, string prefix = "TITLE", string language = null)
        {
            return new EcuTranslation(
                language == null || language.ToLowerInvariant() == "de" ? reader[prefix + "_DEDE"].ToString() : string.Empty,
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
                if (_mDbConnection != null)
                {
                    _mDbConnection.Dispose();
                    _mDbConnection = null;
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}
