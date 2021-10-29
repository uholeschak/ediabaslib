using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace PsdzClient
{
    public class PdszDatabase : IDisposable
    {
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
                EcuPrgVar = null;
                PsdzEcu = null;
                SwiActions = new List<SwiAction>();
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Description { get; set; }

            public string Sgbd { get; set; }

            public string Grp { get; set; }

            public EcuVar EcuVar { get; set; }

            public EcuPrgVar EcuPrgVar { get; set; }

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
                if (EcuPrgVar != null)
                {
                    sb.AppendLine();
                    sb.Append(EcuPrgVar.ToString(language, prefixChild));
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

        public class Equipement
        {
            public Equipement(string id, string name, EcuTranslation ecuTranslation)
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
                    "EcuVar: Id={0}, Class={1}, MotorSeq={2}, VehicleSeq={3},Title='{4}'",
                    Id, NodeClass, MotorCycSeq, VehicleSeq, EcuTranslation.GetTitle(language)));
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

            public SwiRule SwiRule { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiAction: Id={0}, Name={1}, Category={2}, Select={3}, Show={4}, Execute={5}, Title='{6}'",
                    Id, Name, ActionCategory, Selectable, ShowInPlan, Executable, EcuTranslation.GetTitle(language)));

                string prefixChild = prefix + " ";
                if (SwiRule != null)
                {
                    sb.AppendLine();
                    sb.Append(SwiRule.ToString(prefixChild));
                }

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
                FailWeight = string.Empty;
                SortOrder = string.Empty;
                EcuTranslation = ecuTranslation;
            }

            public SwiInfoObj(SwiActionDatabaseLinkType? linkType, string id, string nodeClass,
                string titleId, string versionNum, string name, string failWeight, string hidden,
                string safetyRelevant, string sortOrder, EcuTranslation ecuTranslation)
            {
                LinkType = linkType;
                Id = id;
                NodeClass = nodeClass;
                Assembly = string.Empty;
                VersionNum = versionNum;
                ProgramType = string.Empty;
                SafetyRelevant = safetyRelevant;
                TitleId = titleId;
                General = string.Empty;
                TelSrvId = string.Empty;
                VehicleComm = string.Empty;
                Measurement = string.Empty;
                Hidden = hidden;
                Name = name;
                InformationType = string.Empty;
                Identification = string.Empty;
                InformationFormat = string.Empty;
                SiNumber = string.Empty;
                TargetILevel = string.Empty;
                ControlId = string.Empty;
                InfoType = string.Empty;
                InfoFormat = string.Empty;
                DocNum = string.Empty;
                Priority = string.Empty;
                Identifier = string.Empty;
                FailWeight = failWeight;
                SortOrder = sortOrder;
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

            public string FailWeight { get; set; }

            public string SortOrder { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public SwiRule SwiRule { get; set; }

            public string ToString(string language, string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiInfoObj: LinkType={0}, Id={1}, Class={2}, PrgType={3}, InformationType={4}, Identification={5}, ILevel={6}, InfoType={7}, Identifier={8}, Title='{9}'",
                    LinkType, Id, NodeClass, ProgramType, InformationType, Identification, TargetILevel, InfoType, Identifier, EcuTranslation.GetTitle(language)));
                if (SwiRule != null)
                {
                    string prefixChild = prefix + " ";
                    sb.AppendLine();
                    sb.Append(SwiRule.ToString(prefixChild));
                }
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

        public class SwiRule
        {
            public SwiRule(string id, byte[] rule)
            {
                Id = id;
                Rule = rule;
            }

            public string Id { get; set; }

            public byte[] Rule { get; set; }

            public string ToString(string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);
                sb.Append(string.Format(CultureInfo.InvariantCulture,
                    "SwiRule: Id={0}, RuleSize={1}", Id, Rule.Length));
                return sb.ToString();
            }
        }

        private bool _disposed;
        private SQLiteConnection _mDbConnection;
        private string _rootENameClassId;
        private string _typeKeyClassId;
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
            SwiRegisterTree = null;
            ReadSwiRegister();
            ClientContext.Database = this;
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

        public bool GetEcuVariants(List<EcuInfo> ecuList)
        {
            foreach (EcuInfo ecuInfo in ecuList)
            {
                ecuInfo.SwiActions.Clear();
                ecuInfo.EcuVar = GetEcuVariantByName(ecuInfo.Sgbd);
                ecuInfo.EcuPrgVar = GetEcuProgrammingVariantByName(ecuInfo.PsdzEcu?.BnTnName);

                GetSwiActionsForEcuVariant(ecuInfo);
                GetSwiActionsForEcuGroup(ecuInfo);
                GetSwiActionsForEcuProgrammingVariant(ecuInfo);
                foreach (SwiAction swiAction in ecuInfo.SwiActions)
                {
                    swiAction.SwiInfoObjs = GetServiceProgramsForSwiAction(swiAction);
                }
            }

            return true;
        }

        public void ReadSwiRegister()
        {
            List<SwiRegister> swiRegisterRoot = GetSwiRegistersByParentId(null);
            if (swiRegisterRoot != null)
            {
                SwiRegisterTree = swiRegisterRoot.FirstOrDefault();
            }

            ReadSwiRegisterTree(SwiRegisterTree);
            GetSwiActionsForTree(SwiRegisterTree);
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

        public void GetSwiActionsForTree(SwiRegister swiRegister)
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
                    swiAction.SwiInfoObjs = GetServiceProgramsForSwiAction(swiAction);
                }
            }

            if (swiRegister.Children != null && swiRegister.Children.Count > 0)
            {
                foreach (SwiRegister swiChild in swiRegister.Children)
                {
                    GetSwiActionsForTree(swiChild);
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
                return null;
            }

            return ecuVarList;
        }

        public EcuPrgVar GetEcuProgrammingVariantByName(string bnTnName)
        {
            if (string.IsNullOrEmpty(bnTnName))
            {
                return null;
            }

            EcuPrgVar ecuPrgVar = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, FLASHLIMIT, ECUVARIANTID FROM XEP_ECUPROGRAMMINGVARIANT WHERE UPPER(NAME) = UPPER('{0}')", bnTnName);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuPrgVar = ReadXepEcuPrgVar(reader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return ecuPrgVar;
        }

        public EcuPrgVar GetEcuProgrammingVariantById(string prgId)
        {
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
                            ecuPrgVar = ReadXepEcuPrgVar(reader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return ecuPrgVar;
        }

        public Equipement GetEquipmentById(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId))
            {
                return null;
            }

            Equipement equipement = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, NAME, " + DatabaseFunctions.SqlTitleItems + " FROM XEP_EQUIPMENT WHERE (ID = {0})", equipmentId);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipement = ReadXepEquipement(reader);
                        }
                    }
                }
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                return null;
            }

            return ecuClique;
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
            catch (Exception)
            {
                return null;
            }

            return characteristicRoots;
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
            catch (Exception)
            {
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
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool GetSwiActionsForEcuProgrammingVariant(EcuInfo ecuInfo)
        {
            if (ecuInfo.EcuPrgVar == null || string.IsNullOrEmpty(ecuInfo.EcuPrgVar.Id))
            {
                return false;
            }

            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture,
                    @"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                    @", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUPRGVARI_SWIACTION WHERE ECUPROGRAMMINGVARIANT_ID = {0})",
                    ecuInfo.EcuPrgVar.Id);
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarPrgEcuId);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
            catch (Exception)
            {
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
            catch (Exception)
            {
                return null;
            }

            return swiRegisterList;
        }

        public List<SwiInfoObj> GetServiceProgramsForSwiAction(SwiAction swiAction)
        {
            if (string.IsNullOrEmpty(swiAction.Id))
            {
                return null;
            }

            swiAction.SwiRule = GetRuleById(swiAction.Id);
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
                                swiInfoObj.SwiRule = GetRuleById(infoObjId);
                                swiInfoObjList.Add(swiInfoObj);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
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
                            SwiInfoObj swiInfoObj = GetDiagObjectsByControlId(controlId, SwiInfoObj.SwiActionDatabaseLinkType.SwiActionDiagnosticLink);
                            if (swiInfoObj != null)
                            {
                                swiInfoObjList.Add(swiInfoObj);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
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
                            swiInfoObj = new SwiInfoObj(SwiInfoObj.GetLinkType(linkTypeId), id, nodeClass, assembly, versionNum, programType, safetyRelevant, titleId, general,
                                telSrvId, vehicleComm, measurement, hidden, name, informationType, identification, informationFormat, siNumber, targetILevel, controlId,
                                infoType, infoFormat, docNum, priority, identifier, GetTranslation(reader));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return swiInfoObj;
        }

        public SwiInfoObj GetDiagObjectsByControlId(string controlId, SwiInfoObj.SwiActionDatabaseLinkType linkType)
        {
            if (string.IsNullOrEmpty(controlId))
            {
                return null;
            }

            SwiInfoObj swiInfoObj = null;
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
                            string id = reader["ID"].ToString().Trim();
                            string nodeClass = reader["NODECLASS"].ToString().Trim();
                            string titleId = reader["TITLEID"].ToString().Trim();
                            string versionNum = reader["VERSIONNUMBER"].ToString().Trim();
                            string name = reader["NAME"].ToString().Trim();
                            string failWeight = reader["FAILUREWEIGHT"].ToString().Trim();
                            string hidden = reader["VERSTECKT"].ToString().Trim();
                            string safetyRelevant = reader["SICHERHEITSRELEVANT"].ToString().Trim();
                            string sortOrder = reader["SORT_ORDER"].ToString().Trim();
                            swiInfoObj = new SwiInfoObj(linkType, id, nodeClass, titleId, versionNum, name, failWeight, hidden, safetyRelevant,
                                sortOrder, GetTranslation(reader));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return swiInfoObj;
        }

        public SwiRule GetRuleById(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                return null;
            }

            SwiRule swiRule = null;
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
                            swiRule = new SwiRule(id, rule);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return swiRule;
        }

        public string LookupVehicleCharDeDeById(string characteristicId)
        {
            if (string.IsNullOrEmpty(characteristicId))
            {
                return null;
            }

            string titleDe = null;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT ID, TITLE_DEDE FROM XEP_CHARACTERISTICS WHERE ID = {0}", characteristicId);
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
            catch (Exception)
            {
                return null;
            }

            return titleDe;
        }

        private static Equipement ReadXepEquipement(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            return new Equipement(id, name, GetTranslation(reader));
        }

        private static EcuClique ReadXepEcuClique(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string cliqueName = reader["CLIQUENKURZBEZEICHNUNG"].ToString().Trim();
            string ecuRepId = reader["ECUREPID"].ToString().Trim();
            return new EcuClique(id, cliqueName, ecuRepId, GetTranslation(reader));
        }

        private static CharacteristicRoots ReadXepCharacteristicRoots(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string motorCycSeq = reader["MOTORCYCLESEQUENCE"].ToString().Trim();
            string vehicleSeq = reader["VEHICLESEQUENCE"].ToString().Trim();
            return new CharacteristicRoots(id, nodeClass, motorCycSeq, vehicleSeq, GetTranslation(reader));
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
