using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using log4net;
using PsdzClient.Core;

namespace PsdzClient
{
    public class PdszDatabase : IDisposable
    {
        public const string SqlTitleItemsC =
            "C.TITLE_DEDE, C.TITLE_ENGB, C.TITLE_ENUS, " +
            "C.TITLE_FR, C.TITLE_TH, C.TITLE_SV, " +
            "C.TITLE_IT, C.TITLE_ES, C.TITLE_ID, " +
            "C.TITLE_KO, C.TITLE_EL, C.TITLE_TR, " +
            "C.TITLE_ZHCN, C.TITLE_RU, C.TITLE_NL, " +
            "C.TITLE_PT, C.TITLE_ZHTW, C.TITLE_JA, " +
            "C.TITLE_CSCZ, C.TITLE_PLPL";

        public enum SwiActionSource
        {
            VarId,
            VarGroupId,
            VarPrgEcuId,
            SwiRegister,
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

            public string Title
            {
                get
                {
                    return GetTitle(ClientContext.Language);
                }
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
                        sb.AppendLine();
                        sb.Append(swiAction.ToString(language, prefixChild));
                    }
                }
                return sb.ToString();
            }
        }

        public class EcuVar
        {
            public EcuVar(string id, string name, string groupId, EcuTranslation ecuTranslation)
            {
                Id = id;
                Name = name;
                GroupId = groupId;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public string GroupId { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuVar: Id={0}, Name={1}, GroupId={2}, Title='{3}'",
                    Id, Name, GroupId, EcuTranslation.GetTitle(language)));
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
            public EcuGroup(string id, string name, string virt, string safetyRelevant, string diagAddr)
            {
                Id = id;
                Name = name;
                Virt = virt;
                SafetyRelevant = safetyRelevant;
                DiagAddr = diagAddr;
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public string Virt { get; set; }

            public string SafetyRelevant { get; set; }

            public string DiagAddr { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "EcuGroup: Id={0}, Name={1}, Virt={2}, Safety={3}, Addr={4}",
                    Id, Name, Virt, SafetyRelevant, DiagAddr));
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
                    "EcuVar: Id={0}, Class={1}, ParentId={2}, Name={3}, LegacyName={4}, RootClass={5}, Title='{6}'",
                    Id, NodeClass, ParentId, Name, LegacyName, RootNodeClass, EcuTranslation.GetTitle(language)));
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

            public string ToString(string language, string prefix = "")
            {
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
                        sb.AppendLine();
                        sb.Append(swiChild.ToString(language, prefixChild));
                    }
                }

                if (SwiActions != null)
                {
                    foreach (SwiAction swiAction in SwiActions)
                    {
                        sb.AppendLine();
                        sb.Append(swiAction.ToString(language, prefixChild));
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
                string infoType, string infoFormat, string docNum, string priority, string identifier, EcuTranslation ecuTranslation)
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

            public EcuTranslation EcuTranslation { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiInfoObj: LinkType={0}, Id={1}, Class={2}, PrgType={3}, InformationType={4}, Identification={5}, ILevel={6}, InfoType={7}, Identifier={8}, Title='{9}'",
                    LinkType, Id, NodeClass, ProgramType, InformationType, Identification, TargetILevel, InfoType, Identifier, EcuTranslation.GetTitle(language)));
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

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiInfoObj: Id={0}, Class={1}, TitleId={2}, Name={3}, Identification={4}, ControlId={5}, Title='{6}'",
                    Id, NodeClass, TitleId, Name, Identifier, ControlId, EcuTranslation.GetTitle(language)));
                return sb.ToString();
            }
        }

        public class XepRule
        {
            public XepRule(string id, byte[] rule)
            {
                Id = id;
                RuleExpression = RuleExpression.Deserialize(new MemoryStream(rule));
                Reset();
            }

            public string Id { get; set; }

            public RuleExpression RuleExpression { get; }

            public bool? RuleResult { get; private set; }

            public void Reset()
            {
                RuleResult = null;
            }

            public bool EvaluateRule(Vehicle vehicle, IFFMDynamicResolver ffmResolver)
            {
                log.InfoFormat("EvaluateRule Name: '{0}'", RuleExpression);
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

        private static readonly ILog log = LogManager.GetLogger(typeof(PdszDatabase));

        private bool _disposed;
        private SQLiteConnection _mDbConnection;
        private string _rootENameClassId;
        private string _typeKeyClassId;
        private Dictionary<string, XepRule> _xepRuleDict;
        public Dictionary<string, XepRule> XepRuleDict => _xepRuleDict;
        public SwiRegister SwiRegisterTree { get; private set; }

        public PdszDatabase(string istaFolder)
        {
            string databaseFile = Path.Combine(istaFolder, "SQLiteDBs", "DiagDocDb.sqlite");
            string connection = "Data Source=\"" + databaseFile + "\";";
            _mDbConnection = new SQLiteConnection(connection);

            _mDbConnection.SetPassword("6505EFBDC3E5F324");
            _mDbConnection.Open();

            _rootENameClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"RootEBezeichnung");
            _typeKeyClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"Typschluessel");
            _xepRuleDict = new Dictionary<string, XepRule>();
            SwiRegisterTree = null;
            ClientContext.Database = this;
        }

        public void ResetXepRules()
        {
            foreach (KeyValuePair<string, XepRule> keyValuePair in _xepRuleDict)
            {
                keyValuePair.Value?.Reset();
            }
        }

        public string XepRulesToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, XepRule> keyValuePair in _xepRuleDict)
            {
                if (keyValuePair.Value != null)
                {
                    sb.AppendLine(keyValuePair.Value.ToString());
                }
            }
            return sb.ToString();
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
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool GetEcuVariants(List<EcuInfo> ecuList, Vehicle vehicle = null, IFFMDynamicResolver ffmDynamicResolver = null)
        {
            foreach (EcuInfo ecuInfo in ecuList)
            {
                ecuInfo.SwiActions.Clear();
                ecuInfo.EcuVar = GetEcuVariantByName(ecuInfo.Sgbd);
                ecuInfo.EcuPrgVars = GetEcuProgrammingVariantByName(ecuInfo.PsdzEcu?.BnTnName, vehicle, ffmDynamicResolver);

                GetSwiActionsForEcuVariant(ecuInfo);
                GetSwiActionsForEcuGroup(ecuInfo);
                foreach (EcuPrgVar ecuPrgVar in ecuInfo.EcuPrgVars)
                {
                    List<SwiAction> swiActions = GetSwiActionsForEcuProgrammingVariant(ecuPrgVar.Id, vehicle, ffmDynamicResolver);
                    if (swiActions != null)
                    {
                        ecuInfo.SwiActions.AddRange(swiActions);
                    }
                }
                foreach (SwiAction swiAction in ecuInfo.SwiActions)
                {
                    swiAction.SwiInfoObjs = GetServiceProgramsForSwiAction(swiAction, vehicle, ffmDynamicResolver);
                }
            }

            return true;
        }

        public void ReadSwiRegister(Vehicle vehicle, IFFMDynamicResolver ffmResolver = null)
        {
            List<SwiRegister> swiRegisterRoot = GetSwiRegistersByParentId(null);
            if (swiRegisterRoot != null)
            {
                SwiRegisterTree = swiRegisterRoot.FirstOrDefault();
            }

            ReadSwiRegisterTree(SwiRegisterTree);
            GetSwiActionsForTree(SwiRegisterTree, vehicle, ffmResolver);
        }

        public void ReadSwiRegisterTree(SwiRegister swiRegister)
        {
            if (string.IsNullOrEmpty(swiRegister.Id))
            {
                return;
            }

            List<SwiRegister> swiChildren = GetSwiRegistersByParentId(swiRegister.Id);
            if (swiChildren != null && swiChildren.Count > 0)
            {
                swiRegister.Children = swiChildren;
                foreach (SwiRegister swiChild in swiChildren)
                {
                    ReadSwiRegisterTree(swiChild);
                }
            }
        }

        public void GetSwiActionsForTree(SwiRegister swiRegister, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (string.IsNullOrEmpty(swiRegister.Id))
            {
                return;
            }

            swiRegister.SwiActions = GetSwiActionsForSwiRegister(swiRegister);
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
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, " + DatabaseFunctions.SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", sgbdName.ToLowerInvariant());
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
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, " + DatabaseFunctions.SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (ID = {0})", varId);
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
            if (string.IsNullOrEmpty(bnTnName))
            {
                return null;
            }

            log.InfoFormat("FindEcuVariantsFromBntn BnTnName: {0}", bnTnName);
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
            if (string.IsNullOrEmpty(bnTnName))
            {
                return null;
            }

            log.InfoFormat("GetEcuProgrammingVariantByName BnTnName: {0}", bnTnName);
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
            if (string.IsNullOrEmpty(prgId))
            {
                return null;
            }

            log.InfoFormat("GetEcuProgrammingVariantById Id: {0}", prgId);
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
            if (ecuVar == null || string.IsNullOrEmpty(ecuVar.GroupId))
            {
                return null;
            }

            EcuGroup ecuGroup = null;
            string groupId = ecuVar.GroupId;
            log.InfoFormat("FindEcuGroup Id: {0}", groupId);
            if (EvaluateXepRulesById(groupId, vehicle, ffmResolver, null))
            {
                ecuGroup = GetEcuGroupById(groupId);
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
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, VIRTUELL, SICHERHEITSRELEVANT, DIAGNOSTIC_ADDRESS FROM XEP_ECUGROUPS WHERE (ID = {0})", groupId);
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
            if (string.IsNullOrEmpty(typeKeyId))
            {
                return null;
            }

            List<Characteristics> characteristicsList = new List<Characteristics>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT C.ID, C.NODECLASS, C.TITLEID, " + SqlTitleItemsC +
                    @"C.STATICCLASSVARIABLES, C.STATICCLASSVARIABLESMOTORRAD, C.PARENTID, C.ISTA_VISIBLE, C.NAME, C.LEGACY_NAME, V.DRIVEID, CR.NODECLASS" +
                    @" AS PARENTNODECLASS FROM xep_vehicles JOIN xep_characteristics C on C.ID = V.CHARACTERISTICID JOIN xep_characteristicroots CR on CR.ID = C.PARENTID" +
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

            return characteristicsList;
        }

        public List<Characteristics> GetVehicleIdentByTypeKey(string typeKey)
        {
            string typeKeyId = GetTypeKeyId(typeKey);
            return GetCharacteristicsByTypeKeyId(typeKeyId);
        }

        public SaLaPa GetSaLaPaById(string salapaId)
        {
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

            return saLaPa;
        }

        public EcuReps GetEcuRepsById(string ecuId)
        {
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
            if (ecuInfo.EcuVar == null || string.IsNullOrEmpty(ecuInfo.EcuVar.GroupId))
            {
                return false;
            }

            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    @", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUGROUPS_SWIACTION WHERE ECUGROUP_ID = {0})",
                    ecuInfo.EcuVar.GroupId);
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

        public List<SwiAction> GetSwiActionsForSwiRegister(SwiRegister swiRegister)
        {
            if (string.IsNullOrEmpty(swiRegister.Id))
            {
                return null;
            }

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
                            swiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetSwiActionsForSwiRegister Exception: '{0}'", e.Message);
                return null;
            }

            return swiActions;
        }

        public List<SwiRegister> GetSwiRegistersByParentId(string parentId)
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
                            swiRegisterList.Add(swiRegister);
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

            log.InfoFormat("GetServiceProgramsForSwiAction Id: {0}", swiAction.Id);
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
                                if (EvaluateXepRulesById(infoObjId, vehicle, ffmDynamicResolver, swiInfoObj.ControlId))
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
                            List<SwiDiagObj> swiDiagObjs = GetDiagObjectsByControlId(controlId, vehicle, ffmDynamicResolver);
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
                    @"IDENTIFIKATOR, INFORMATIONSFORMAT, SINUMMER, ZIELISTUFE, CONTROLID, INFOTYPE, INFOFORMAT, DOCNUMBER, PRIORITY, IDENTIFIER FROM XEP_INFOOBJECTS WHERE XEP_INFOOBJECTS.ID = {0}",
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
                    @"IDENTIFIKATOR, INFORMATIONSFORMAT, SINUMMER, ZIELISTUFE, CONTROLID, INFOTYPE, INFOFORMAT, DOCNUMBER, PRIORITY, IDENTIFIER FROM XEP_INFOOBJECTS WHERE XEP_INFOOBJECTS.ID = {0}",
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

        public List<SwiDiagObj> GetDiagObjectsByControlId(string controlId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            if (string.IsNullOrEmpty(controlId))
            {
                return null;
            }

            List<SwiDiagObj> swiDiagObjs = new List<SwiDiagObj>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NODECLASS, TITLEID, " + DatabaseFunctions.SqlTitleItems +
                    @", VERSIONNUMBER, NAME, FAILUREWEIGHT, VERSTECKT, SICHERHEITSRELEVANT, " +
                    @"CONTROLID, SORT_ORDER FROM XEP_DIAGNOSISOBJECTS WHERE (CONTROLID = {0})",
                    controlId);
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

            string diagObjectControlId = GetDiagObjectControlIdForDiagObjectId(diagObjectId);
            if (string.IsNullOrEmpty(diagObjectControlId))
            {
                log.InfoFormat("IsDiagObjectValid No control id, Valid: {0}", true);
                return true;
            }

            if (AreAllParentDiagObjectsValid(diagObjectControlId, vehicle, ffmDynamicResolver))
            {
                log.InfoFormat("IsDiagObjectValid All parents valid, Valid: {0}", true);
                return true;
            }

            log.InfoFormat("IsDiagObjectValid Valid: {0}", false);
            return false;
        }

        public bool AreAllParentDiagObjectsValid(string diagObjectControlId, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            log.InfoFormat("AreAllParentDiagObjectsValid Id: {0}", diagObjectControlId);
            List<string> idList = GetParentDiagObjectControlIdsForControlId(diagObjectControlId);
            if (idList == null || idList.Count == 0)
            {
                log.InfoFormat("AreAllParentDiagObjectsValid No parent diag objects, Valid: {0}", true);
                return true;
            }

            HashSet<SwiDiagObj> swiDiagObjHash = new HashSet<SwiDiagObj>();
            foreach (string parentId in idList)
            {
                if (string.IsNullOrEmpty(parentId))
                {
                    log.InfoFormat("AreAllParentDiagObjectsValid No parent id, Valid: {0}", true);
                    return true;
                }

                SwiDiagObj swiDiagObj = GetDiagObjectsByControlId(parentId, null, null).FirstOrDefault();
                if (swiDiagObj == null)
                {
                    log.InfoFormat("AreAllParentDiagObjectsValid No diag control id, Valid: {0}", true);
                    return true;
                }

                if (string.IsNullOrEmpty(swiDiagObj.ControlId) && EvaluateXepRulesById(swiDiagObj.Id, vehicle, ffmDynamicResolver))
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

            log.InfoFormat("GetParentDiagObjectControlIdsForControlId IdList: {0}", idList);
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

        public XepRule GetRuleById(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                return null;
            }

            if (_xepRuleDict.TryGetValue(ruleId, out XepRule xepRule))
            {
                return xepRule;
            }

            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, RULE FROM XEP_RULES WHERE ID IN ({0})", ruleId);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString().Trim();
                            byte[] rule = (byte[]) reader["RULE"];
                            xepRule = new XepRule(id, rule);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("GetRuleById Exception: '{0}'", e.Message);
                return null;
            }

            _xepRuleDict.Add(ruleId, xepRule);
            return xepRule;
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

        public string LookupVehicleCharIdByName(string name, string nodeclass)
        {
            log.InfoFormat("LookupVehicleCharIdByName Id: {0} Class: {1}", name, nodeclass);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            string charId = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NODECLASS, NAME FROM XEP_CHARACTERISTICS WHERE (NAME = {0})", name);
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

        public string GetTypeKeyId(string typeKey)
        {
            log.InfoFormat("GetTypeKeyId Key: {0}", typeKey);
            if (string.IsNullOrEmpty(typeKey))
            {
                return null;
            }

            string typeId = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID FROM XEP_CHARACTERISTICS WHERE (NAME = {0}) AND (NODECLASS = {1})", typeKey, _typeKeyClassId);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            typeId = reader["ID"].ToString().Trim();
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

        public bool EvaluateXepRulesById(string id, Vehicle vehicle, IFFMDynamicResolver ffmResolver, string objectId = null)
        {
            XepRule xepRule = GetRuleById(id);
            if (xepRule == null)
            {
                log.WarnFormat("EvaluateXepRulesById Id not found: {0}", id);
                return true;
            }

            return xepRule.EvaluateRule(vehicle, ffmResolver);
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
            string name = reader["NAME"].ToString().Trim();
            string groupId = reader["ECUGROUPID"].ToString().Trim();
            return new EcuVar(id, name, groupId, GetTranslation(reader));
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
            string name = reader["NAME"].ToString().Trim();
            string virt = reader["VIRTUELL"].ToString().Trim();
            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString().Trim();
            string diagAddr = reader["DIAGNOSTIC_ADDRESS"].ToString().Trim();
            return new EcuGroup(id, name, virt, safetyRelevant, diagAddr);
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

        private static SwiInfoObj ReadXepSwiInfoObj(SQLiteDataReader reader, SwiInfoObj.SwiActionDatabaseLinkType? linkType)
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
            return new SwiInfoObj(linkType, id, nodeClass, assembly, versionNum, programType, safetyRelevant, titleId, general,
                telSrvId, vehicleComm, measurement, hidden, name, informationType, identification, informationFormat, siNumber, targetILevel, controlId,
                infoType, infoFormat, docNum, priority, identifier, GetTranslation(reader));
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

                ClientContext.Database = null;
                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}
