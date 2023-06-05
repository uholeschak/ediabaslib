using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace BmwFileReader
{
    public static class EcuFunctionStructs
    {
        [XmlInclude(typeof(EcuFaultCodeLabel)), XmlInclude(typeof(EcuFaultModeLabel)), XmlInclude(typeof(EcuEnvCondLabel))]
        [XmlType("FDat")]
        public class EcuFaultData
        {
            public EcuFaultData()
            {
                EcuFaultCodeLabelList = null;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FAULTDATA:");
                sb.Append(this.PropertyList(prefix + " "));
#if false
                if (EcuFaultCodeLabelList != null)
                {
                    foreach (EcuFaultCodeLabel ecuFaultCodeLabel in EcuFaultCodeLabelList)
                    {
                        sb.Append(ecuFaultCodeLabel.ToString(prefix + " "));
                    }
                }

                if (EcuFaultModeLabelList != null)
                {
                    foreach (EcuFaultModeLabel ecuFaultModeLabel in EcuFaultModeLabelList)
                    {
                        sb.Append(ecuFaultModeLabel.ToString(prefix + " "));
                    }
                }

                if (EcuEnvCondLabelList != null)
                {
                    foreach (EcuEnvCondLabel ecuEnvCondLabel in EcuEnvCondLabelList)
                    {
                        sb.Append(ecuEnvCondLabel.ToString(prefix + " "));
                    }
                }
#endif
                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement("DbVer"), DefaultValue("")] public string DatabaseVersion { get; set; }
            [XmlElement("DbDat")] public DateTime DatabaseDate { get; set; }
            [XmlArray("EFCLL"), DefaultValue(null)] public List<EcuFaultCodeLabel> EcuFaultCodeLabelList { get; set; }
            [XmlArray("EFMLL"), DefaultValue(null)] public List<EcuFaultModeLabel> EcuFaultModeLabelList { get; set; }
            [XmlArray("EECLL"), DefaultValue(null)] public List<EcuEnvCondLabel> EcuEnvCondLabelList { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuFuncStruct)), XmlInclude(typeof(EcuFaultCode)), XmlInclude(typeof(EcuClique))]
        [XmlType("Var")]
        public class EcuVariant
        {
            public EcuVariant()
            {
                Id = string.Empty;
                GroupId = string.Empty;
                GroupName = string.Empty;
                GroupFunctionIds = null;
                Title = null;
            }

            public EcuVariant(string id, string groupId, string groupName, EcuTranslation title, List<string> groupFunctionIds)
            {
                Id = id;
                GroupId = groupId;
                GroupName = groupName;
                GroupFunctionIds = groupFunctionIds;
                Title = title;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "VARIANT:");
                sb.Append(this.PropertyList(prefix + " "));

                if (GroupFunctionIds != null)
                {
                    sb.Append(prefix + " GROUPFUNC:");
                    foreach (string GroupFunctionId in GroupFunctionIds)
                    {
                        sb.Append(" " + GroupFunctionId);
                    }
                    sb.AppendLine();
                }

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                if (RefEcuVariantList != null)
                {
                    foreach (RefEcuVariant refEcuVariant in RefEcuVariantList)
                    {
                        sb.Append(refEcuVariant.ToString(prefix + " "));
                    }
                }

                if (EcuFuncStructList != null)
                {
                    foreach (EcuFuncStruct ecuFuncStruct in EcuFuncStructList)
                    {
                        sb.Append(ecuFuncStruct.ToString(prefix + " "));
                    }
                }

                if (EcuFaultCodeList != null)
                {
                    foreach (EcuFaultCode ecuFaultCode in EcuFaultCodeList)
                    {
                        sb.Append(ecuFaultCode.ToString(prefix + " "));
                    }
                }

                if (EcuClique != null)
                {
                    EcuClique.ToString(prefix + " ");
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public Dictionary<Int64, EcuFaultCode> GetEcuFaultCodeDict(bool info)
            {
                return info ? EcuFaultCodeDictInfo : EcuFaultCodeDictFault;
            }

            public void ClearCompatIds()
            {
                if (RefEcuVariantList != null)
                {
                    foreach (RefEcuVariant refEcuVariant in RefEcuVariantList)
                    {
                        refEcuVariant.ClearCompatIds();
                    }
                }

                if (EcuFuncStructList != null)
                {
                    foreach (EcuFuncStruct ecuFuncStruct in EcuFuncStructList)
                    {
                        ecuFuncStruct.ClearCompatIds();
                    }
                }
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("GId"), DefaultValue("")] public string GroupId { get; set; }
            [XmlElement("GNam"), DefaultValue("")] public string GroupName { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlArray("GFId"), XmlArrayItem("Str"), DefaultValue(null)] public List<string> GroupFunctionIds { get; set; }
            [XmlArray("RVL"), DefaultValue(null)] public List<RefEcuVariant> RefEcuVariantList { get; set; }
            [XmlArray("FSL"), DefaultValue(null)] public List<EcuFuncStruct> EcuFuncStructList { get; set; }
            [XmlArray("FCL"), DefaultValue(null)] public List<EcuFaultCode> EcuFaultCodeList { get; set; }
            [XmlElement("ECli"), DefaultValue(null)] public EcuClique EcuClique { get; set; }
            [XmlIgnore] public Dictionary<Int64, EcuFaultCode> EcuFaultCodeDictFault { get; set; }
            [XmlIgnore] public Dictionary<Int64, EcuFaultCode> EcuFaultCodeDictInfo { get; set; }
        }

        [XmlType("Cliq")]
        public class EcuClique
        {
            public EcuClique()
            {
                Id = string.Empty;
                CliqueName = string.Empty;
                EcuRepId = string.Empty;
                EcuRepsName = string.Empty;
            }

            public EcuClique(string id, string cliqueName, string ecuRepId)
            {
                Id = id;
                CliqueName = cliqueName;
                EcuRepId = ecuRepId;
                EcuRepsName = string.Empty;
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("CNam"), DefaultValue("")] public string CliqueName { get; set; }
            [XmlElement("RId"), DefaultValue("")] public string EcuRepId { get; set; }
            [XmlElement("RNam"), DefaultValue("")] public string EcuRepsName { get; set; }

            public string ToString(string prefix = "")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "CLIQUE:");
                sb.Append(this.PropertyList(prefix + " "));
                return sb.ToString();
            }
        }

        [XmlInclude(typeof(EcuFaultCodeLabel)), XmlInclude(typeof(EcuFaultModeLabel)), XmlInclude(typeof(EcuEnvCondLabel))]
        [XmlType("FCod")]
        public class EcuFaultCode
        {
            public EcuFaultCode()
            {
                Id = string.Empty;
                Code = string.Empty;
                DataType = string.Empty;
                Relevance = string.Empty;
                EcuFaultCodeLabelId = string.Empty;
            }

            public EcuFaultCode(string id, string code, string dataType, string relevance)
            {
                Id = id;
                Code = code;
                DataType = dataType;
                Relevance = relevance;
                EcuFaultCodeLabelId = string.Empty;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FAULTCODE:");
                sb.Append(this.PropertyList(prefix + " "));

                if (EcuFaultCodeLabel != null)
                {
                    sb.Append(EcuFaultCodeLabel.ToString(prefix + " "));
                }

                if (EcuFaultModeLabelList != null)
                {
                    foreach (EcuFaultModeLabel ecuFaultModeLabel in EcuFaultModeLabelList)
                    {
                        sb.Append(ecuFaultModeLabel.ToString(prefix + " "));
                    }
                }

                if (EcuEnvCondLabelList != null)
                {
                    foreach (EcuEnvCondLabel ecuEnvCondLabel in EcuEnvCondLabelList)
                    {
                        sb.Append(ecuEnvCondLabel.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("Cod"), DefaultValue("")] public string Code { get; set; }
            [XmlElement("DTyp"), DefaultValue("")] public string DataType { get; set; }
            [XmlElement("Rel"), DefaultValue("")] public string Relevance { get; set; }
            [XmlIgnore, XmlElement, DefaultValue(null)] public EcuFaultCodeLabel EcuFaultCodeLabel { get; set; }
            [XmlElement("FCLIdL"), DefaultValue("")] public string EcuFaultCodeLabelId { get; set; }
            [XmlIgnore, XmlArray, DefaultValue(null)] public List<EcuFaultModeLabel> EcuFaultModeLabelList { get; set; }
            [XmlArray("FMLIdL"), XmlArrayItem("Str"), DefaultValue(null)] public List<string> EcuFaultModeLabelIdList { get; set; }
            [XmlIgnore, XmlArray, DefaultValue(null)] public List<EcuEnvCondLabel> EcuEnvCondLabelList { get; set; }
            [XmlArray("ECLIdL"), XmlArrayItem("Str"), DefaultValue(null)] public List<string> EcuEnvCondLabelIdList { get; set; }
        }

        [XmlType("FCLab")]
        public class EcuFaultCodeLabel
        {
            public EcuFaultCodeLabel()
            {
                Id = string.Empty;
                Code = string.Empty;
                SaeCode = string.Empty;
                Relevance = string.Empty;
                DataType = string.Empty;
            }

            public EcuFaultCodeLabel(string id, string code, string saeCode, EcuTranslation title, string relevance, string dataType)
            {
                Id = id;
                Code = code;
                SaeCode = saeCode;
                Title = title;
                Relevance = relevance;
                DataType = dataType;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FAULTCODELABEL:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("Cod"), DefaultValue("")] public string Code { get; set; }
            [XmlElement("Sae"), DefaultValue("")] public string SaeCode { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("Rel"), DefaultValue("")] public string Relevance { get; set; }
            [XmlElement("DTyp"), DefaultValue("")] public string DataType { get; set; }
        }

        [XmlType("FMLab")]
        public class EcuFaultModeLabel
        {
            public EcuFaultModeLabel()
            {
                Id = string.Empty;
                Code = string.Empty;
                Relevance = string.Empty;
                Extended = string.Empty;
            }

            public EcuFaultModeLabel(string id, string code, EcuTranslation title, string relevance, string extended)
            {
                Id = id;
                Code = code;
                Title = title;
                Relevance = relevance;
                Extended = extended;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FAULTMODELABEL:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("Cod"), DefaultValue("")] public string Code { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("Rel"), DefaultValue("")] public string Relevance { get; set; }
            [XmlElement("Ext"), DefaultValue("")] public string Extended { get; set; }
        }

        [XmlType("ECLab")]
        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuResultStateValue))]
        public class EcuEnvCondLabel
        {
            public EcuEnvCondLabel()
            {
                Id = string.Empty;
                NodeClass = string.Empty;
                Relevance = string.Empty;
                BlockCount = string.Empty;
                IdentType = string.Empty;
                IdentStr = string.Empty;
                Unit = string.Empty;
            }

            public EcuEnvCondLabel(string id, string nodeClass, EcuTranslation title, string relevance, string blockCount, string identType, string identStr, string unit)
            {
                Id = id;
                NodeClass = nodeClass;
                Title = title;
                Relevance = relevance;
                BlockCount = blockCount;
                IdentType = identType;
                IdentStr = identStr;
                Unit = unit;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FAULTMODELABEL:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                if (EcuResultStateValueList != null)
                {
                    foreach (EcuResultStateValue ecuResultStateValue in EcuResultStateValueList)
                    {
                        sb.Append(ecuResultStateValue.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("NC"), DefaultValue("")] public string NodeClass { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("Rel"), DefaultValue("")] public string Relevance { get; set; }
            [XmlElement("BCnt"), DefaultValue("")] public string BlockCount { get; set; }
            [XmlElement("ITyp"), DefaultValue("")] public string IdentType { get; set; }
            [XmlElement("IStr"), DefaultValue("")] public string IdentStr { get; set; }
            [XmlElement("Uni"), DefaultValue("")] public string Unit { get; set; }
            [XmlArray("RSVL"), DefaultValue(null)] public List<EcuResultStateValue> EcuResultStateValueList { get; set; }
            [XmlIgnore] public bool EcuResultStateValueListSpecified
            {
                get
                {
                    if (EcuResultStateValueList != null && EcuResultStateValueList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuJob))]
        [XmlType("FFStr")]
        public class EcuFixedFuncStruct
        {
            public enum NodeClassType
            {
                Unknown,
                Identification,
                ReadState,
                ControlActuator,
            }

            public EcuFixedFuncStruct()
            {
                Id = string.Empty;
                NodeClass = string.Empty;
                NodeClassName = string.Empty;
                Title = null;
                PrepOp = null;
                ProcOp = null;
                PostOp = null;
                SortOrder = string.Empty;
                Activation = string.Empty;
                ActivationDurationMs = string.Empty;
            }

            public EcuFixedFuncStruct(string id, string nodeClass, string nodeClassName,
                EcuTranslation title, EcuTranslation preOp, EcuTranslation procOp, EcuTranslation postOp,
                string sortOrder, string activation, string activationDuringMs)
            {
                Id = id;
                NodeClass = nodeClass;
                NodeClassName = nodeClassName;
                Title = title;
                PrepOp = preOp;
                ProcOp = procOp;
                PostOp = postOp;
                SortOrder = sortOrder;
                Activation = activation;
                ActivationDurationMs = activationDuringMs;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FIXEDFUNC:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                if (PrepOp != null)
                {
                    sb.AppendLine(prefix + " PRE-");
                    sb.Append(PrepOp.ToString(prefix + " "));
                }

                if (ProcOp != null)
                {
                    sb.AppendLine(prefix + " PROC-");
                    sb.Append(ProcOp.ToString(prefix + " "));
                }

                if (PostOp != null)
                {
                    sb.AppendLine(prefix + " POST-");
                    sb.Append(PostOp.ToString(prefix + " "));
                }

                if (EcuJobList != null)
                {
                    foreach (EcuJob ecuJob in EcuJobList)
                    {
                        sb.Append(ecuJob.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public NodeClassType GetNodeClassType()
            {
                if (string.Compare(NodeClassName, "ECUFixedFunctionReadingIdentification", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return NodeClassType.Identification;
                }

                if (string.Compare(NodeClassName, "ECUFixedFunctionReadingState", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return NodeClassType.ReadState;
                }

                if (string.Compare(NodeClassName, "ECUFixedFunctionControlingActuator", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return NodeClassType.ControlActuator;
                }

                return NodeClassType.Unknown;
            }

            public bool IdPresent(string idCompare, bool includeCompatId = true)
            {
                string idTrimmed = idCompare.Trim();
                if (string.Compare(Id.Trim(), idTrimmed, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                if (includeCompatId && CompatIdListList != null)
                {
                    if (CompatIdListList.Any(x => string.Compare(x.Trim(), idTrimmed, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void ClearCompatIds()
            {
                if (CompatIdListList != null)
                {
                    CompatIdListList.Clear();
                    CompatIdListList = null;
                }

                if (EcuJobList != null)
                {
                    if (EcuJobList != null)
                    {
                        foreach (EcuJob ecuJob in EcuJobList)
                        {
                            ecuJob.ClearCompatIds();
                        }
                    }
                }
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }

            [XmlArray("CIDL"), DefaultValue(null)] public List<string> CompatIdListList { get; set; }
            [XmlIgnore] public bool CompatIdListListSpecified
            {
                get
                {
                    if (CompatIdListList != null && CompatIdListList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            [XmlElement("NC"), DefaultValue("")] public string NodeClass { get; set; }
            [XmlElement("NCNam"), DefaultValue("")] public string NodeClassName { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("Pre"), DefaultValue(null)] public EcuTranslation PrepOp { get; set; }
            [XmlElement("Proc"), DefaultValue(null)] public EcuTranslation ProcOp { get; set; }
            [XmlElement("Post"), DefaultValue(null)] public EcuTranslation PostOp { get; set; }
            [XmlElement("SOrd"), DefaultValue("")] public string SortOrder { get; set; }
            [XmlElement("Act"), DefaultValue("")] public string Activation { get; set; }
            [XmlElement("ActDur"), DefaultValue("")] public string ActivationDurationMs { get; set; }
            [XmlArray("JL"), DefaultValue(null)] public List<EcuJob> EcuJobList { get; set; }
        }

        [XmlInclude(typeof(EcuFixedFuncStruct))]
        [XmlType("RVar")]
        public class RefEcuVariant
        {
            public RefEcuVariant()
            {
                Id = string.Empty;
                EcuVariantId = string.Empty;
            }

            public RefEcuVariant(string id, string ecuVariantId)
            {
                Id = id;
                EcuVariantId = ecuVariantId;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "REFVARIANT:");
                sb.Append(this.PropertyList(prefix + " "));

                if (FixedFuncStructList != null)
                {
                    foreach (EcuFixedFuncStruct ecuFixedFuncStruct in FixedFuncStructList)
                    {
                        sb.Append(ecuFixedFuncStruct.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public void ClearCompatIds()
            {
                if (FixedFuncStructList != null)
                {
                    foreach (EcuFixedFuncStruct ecuFixedFuncStruct in FixedFuncStructList)
                    {
                        ecuFixedFuncStruct.ClearCompatIds();
                    }
                }
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("VId"), DefaultValue("")] public string EcuVariantId { get; set; }
            [XmlArray("FFSL"), DefaultValue(null)] public List<EcuFixedFuncStruct> FixedFuncStructList { get; set; }
        }

        [XmlType("VFun")]
        public class EcuVarFunc
        {
            public EcuVarFunc()
            {
                Id = string.Empty;
                GroupFuncId = string.Empty;
            }

            public EcuVarFunc(string id, string groupFuncId)
            {
                Id = id;
                GroupFuncId = groupFuncId;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "VARFUNC:");
                sb.Append(this.PropertyList(prefix + " "));
                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("GFId"), DefaultValue("")] public string GroupFuncId { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuFixedFuncStruct))]
        [XmlType("FStr")]
        public class EcuFuncStruct
        {
            public enum NodeClassType
            {
                Unknown,
                ReadStateStructure,
                ControlActuatorStructure,
            }

            public EcuFuncStruct()
            {
                Id = string.Empty;
                ParentId = string.Empty;
                SortOrder = string.Empty;
                NodeClass = string.Empty;
                NodeClassName = string.Empty;
                Title = null;
                MultiSelect = string.Empty;
            }

            public EcuFuncStruct(string id, string nodeClass, string nodeClassName, EcuTranslation title, string multiSelect, string parentId, string sortOrder)
            {
                Id = id;
                ParentId = parentId;
                SortOrder= sortOrder;
                NodeClass = nodeClass;
                NodeClassName = nodeClassName;
                Title = title;
                MultiSelect = multiSelect;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FUNC:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                if (FixedFuncStructList != null)
                {
                    foreach (EcuFixedFuncStruct ecuFixedFuncStruct in FixedFuncStructList)
                    {
                        sb.Append(ecuFixedFuncStruct.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public NodeClassType GetNodeClassType()
            {
                if (string.Compare(NodeClassName, "ECUStateReadStructure", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return NodeClassType.ReadStateStructure;
                }

                if (string.Compare(NodeClassName, "ECUControllingActuatorStructure", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return NodeClassType.ControlActuatorStructure;
                }

                return NodeClassType.Unknown;
            }

            public void ClearCompatIds()
            {
                if (FixedFuncStructList != null)
                {
                    foreach (EcuFixedFuncStruct ecuFixedFuncStruct in FixedFuncStructList)
                    {
                        ecuFixedFuncStruct.ClearCompatIds();
                    }
                }
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("NC"), DefaultValue("")] public string NodeClass { get; set; }
            [XmlElement("NCNam"), DefaultValue("")] public string NodeClassName { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("MSel"), DefaultValue("")] public string MultiSelect { get; set; }
            [XmlElement("PId"), DefaultValue("")] public string ParentId { get; set; }
            [XmlElement("SOrd"), DefaultValue("")] public string SortOrder { get; set; }
            [XmlArray("FFSL"), DefaultValue(null)] public List<EcuFixedFuncStruct> FixedFuncStructList { get; set; }
        }

        [XmlInclude(typeof(EcuJobParameter)), XmlInclude(typeof(EcuJobResult))]
        [XmlType("Job")]
        public class EcuJob
        {
            public enum PhaseType
            {
                Unknown,
                Preset,
                Main,
                Reset
            }

            public EcuJob()
            {
                Id = string.Empty;
                FuncNameJob = string.Empty;
                Name = string.Empty;
                Phase = string.Empty;
                Rank = string.Empty;
            }

            public EcuJob(string id, string funcNameJob, string name, string phase, string rank)
            {
                Id = id;
                FuncNameJob = funcNameJob;
                Name = name;
                Phase = phase;
                Rank = rank;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "JOB:");
                sb.Append(this.PropertyList(prefix + " "));
                if (EcuJobParList != null)
                {
                    foreach (EcuJobParameter ecuJobParameter in EcuJobParList)
                    {
                        sb.Append(ecuJobParameter.ToString(prefix + " "));
                    }
                }

                if (EcuJobResultList != null)
                {
                    foreach (EcuJobResult ecuJobResult in EcuJobResultList)
                    {
                        sb.Append(ecuJobResult.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public PhaseType GetPhaseType()
            {
                if (string.Compare(Phase, "Preset", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return PhaseType.Preset;
                }

                if (string.Compare(Phase, "Main", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return PhaseType.Main;
                }

                if (string.Compare(Phase, "Reset", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return PhaseType.Reset;
                }

                return PhaseType.Unknown;
            }

            public bool IdPresent(string idCompare, bool includeCompatId = true)
            {
                string idTrimmed = idCompare.Trim();
                if (string.Compare(Id.Trim(), idTrimmed, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                if (includeCompatId && CompatIdListList != null)
                {
                    if (CompatIdListList.Any(x => string.Compare(x.Trim(), idTrimmed, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void ClearCompatIds()
            {
                if (CompatIdListList != null)
                {
                    CompatIdListList.Clear();
                    CompatIdListList = null;
                }

                if (EcuJobResultList != null)
                {
                    foreach (EcuJobResult ecuJobResult in EcuJobResultList)
                    {
                        ecuJobResult.ClearCompatIds();
                    }
                }
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }

            [XmlArray("CIDL"), DefaultValue(null)] public List<string> CompatIdListList { get; set; }
            [XmlIgnore] public bool CompatIdListListSpecified
            {
                get
                {
                    if (CompatIdListList != null && CompatIdListList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }

            [XmlElement("FNJob"), DefaultValue("")] public string FuncNameJob { get; set; }
            [XmlElement("Nam"), DefaultValue("")] public string Name { get; set; }
            [XmlElement("Pha"), DefaultValue("")] public string Phase { get; set; }
            [XmlElement("Ran"), DefaultValue("")] public string Rank { get; set; }

            [XmlArray("JPL"), DefaultValue(null)] public List<EcuJobParameter> EcuJobParList { get; set; }
            [XmlIgnore] public bool EcuJobParListSpecified
            {
                get
                {
                    if (EcuJobParList != null && EcuJobParList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }

            [XmlArray("JRL"), DefaultValue(null)] public List<EcuJobResult> EcuJobResultList { get; set; }
            [XmlIgnore] public bool EcuJobResultListSpecified
            {
                get
                {
                    if (EcuJobResultList != null && EcuJobResultList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        [XmlType("JPar")]
        public class EcuJobParameter
        {
            public EcuJobParameter()
            {
                Id = string.Empty;
                Value = string.Empty;
                AdapterPath = string.Empty;
                Name = string.Empty;
            }

            public EcuJobParameter(string id, string value, string adapterPath, string name)
            {
                Id = id;
                Value = value;
                AdapterPath = adapterPath;
                Name = name;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "JOB:");
                sb.Append(this.PropertyList(prefix + " "));
                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("Val"), DefaultValue("")] public string Value { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string AdapterPath { get; set; }
            [XmlElement("Nam"), DefaultValue("")] public string Name { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuResultStateValue))]
        [XmlType("JRes")]
        public class EcuJobResult
        {
            public EcuJobResult()
            {
                Id = string.Empty;
                Title = null;
                FuncNameResult = string.Empty;
                AdapterPath = string.Empty;
                Name = string.Empty;
                EcuFuncRelevant = string.Empty;
                Location = string.Empty;
                Unit = string.Empty;
                UnitFixed = string.Empty;
                Format = string.Empty;
                Mult = string.Empty;
                Offset = string.Empty;
                Round = string.Empty;
                NumberFormat = string.Empty;
            }

            public EcuJobResult(string id, EcuTranslation title, string funcNameResult, string adapterPath,
                string name, string ecuFuncRelevant, string location, string unit, string unitFixed, string format, string mult, string offset,
                string round, string numberFormat)
            {
                Id = id;
                Title = title;
                FuncNameResult = funcNameResult;
                AdapterPath = adapterPath;
                Name = name;
                EcuFuncRelevant = ecuFuncRelevant;
                Location = location;
                Unit = unit;
                UnitFixed = unitFixed;
                Format = format;
                Mult = mult;
                Offset = offset;
                Round = round;
                NumberFormat = numberFormat;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "RESULT:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }

                if (EcuResultStateValueList != null)
                {
                    foreach (EcuResultStateValue ecuResultStateValue in EcuResultStateValueList)
                    {
                        sb.Append(ecuResultStateValue.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public bool IdPresent(string idCompare, bool includeCompatId = true)
            {
                string idTrimmed = idCompare.Trim();
                if (string.Compare(Id.Trim(), idTrimmed, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                if (includeCompatId && CompatIdListList != null)
                {
                    if (CompatIdListList.Any(x => string.Compare(x.Trim(), idTrimmed, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void ClearCompatIds()
            {
                if (CompatIdListList != null)
                {
                    CompatIdListList.Clear();
                    CompatIdListList = null;
                }
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }

            [XmlArray("CIDL"), DefaultValue(null)] public List<string> CompatIdListList { get; set; }
            [XmlIgnore] public bool CompatIdListListSpecified
            {
                get
                {
                    if (CompatIdListList != null && CompatIdListList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }

            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("FNRes"), DefaultValue("")] public string FuncNameResult { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string AdapterPath { get; set; }
            [XmlElement("Nam"), DefaultValue("")] public string Name { get; set; }
            [XmlElement("FRel"), DefaultValue("")] public string EcuFuncRelevant { get; set; }
            [XmlElement("Loc"), DefaultValue("")] public string Location { get; set; }
            [XmlElement("Uni"), DefaultValue("")] public string Unit { get; set; }
            [XmlElement("FUni"), DefaultValue("")] public string UnitFixed { get; set; }
            [XmlElement("Form"), DefaultValue("")] public string Format { get; set; }
            [XmlElement("Mul"), DefaultValue("")] public string Mult { get; set; }
            [XmlElement("Off"), DefaultValue("")] public string Offset { get; set; }
            [XmlElement("Rnd"), DefaultValue("")] public string Round { get; set; }
            [XmlElement("FNum"), DefaultValue("")] public string NumberFormat { get; set; }
            [XmlArray("RSVL"), DefaultValue(null)] public List<EcuResultStateValue> EcuResultStateValueList { get; set; }
            [XmlIgnore] public bool EcuResultStateValueListSpecified
            {
                get
                {
                    if (EcuResultStateValueList != null && EcuResultStateValueList.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        [XmlInclude(typeof(EcuTranslation))]
        [XmlType("RSVal")]
        public class EcuResultStateValue
        {
            public EcuResultStateValue()
            {
                Id = string.Empty;
                Title = null;
                StateValue = string.Empty;
                ValidFrom = string.Empty;
                ValidTo = string.Empty;
                ParentId = string.Empty;
            }

            public EcuResultStateValue(string id, EcuTranslation title,
                string stateValue, string validFrom, string validTo, string parentId)
            {
                Id = id;
                Title = title;
                StateValue = stateValue;
                ValidFrom = validFrom;
                ValidTo = validTo;
                ParentId = parentId;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "STATEVALUE:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(Title.ToString(prefix + " "));
                }
                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement("Tit"), DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement("SVal"), DefaultValue("")] public string StateValue { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string ValidFrom { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string ValidTo { get; set; }
            [XmlElement("PId"), DefaultValue("")] public string ParentId { get; set; }
        }

        [XmlType("Tran")]
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

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "TRANS:");
                sb.Append(this.PropertyList(prefix + " "));

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public string GetTitle(string language)
            {
                return GetTitleTranslated(this, language);
            }

            [XmlElement("De"), DefaultValue("")] public string TextDe { get; set; }
            [XmlElement("En"), DefaultValue("")] public string TextEn { get; set; }
            [XmlElement("Fr"), DefaultValue("")] public string TextFr { get; set; }
            [XmlElement("Th"), DefaultValue("")] public string TextTh { get; set; }
            [XmlElement("Sv"), DefaultValue("")] public string TextSv { get; set; }
            [XmlElement("It"), DefaultValue("")] public string TextIt { get; set; }
            [XmlElement("Es"), DefaultValue("")] public string TextEs { get; set; }
            [XmlElement("Id"), DefaultValue("")] public string TextId { get; set; }
            [XmlElement("Ko"), DefaultValue("")] public string TextKo { get; set; }
            [XmlElement("El"), DefaultValue("")] public string TextEl { get; set; }
            [XmlElement("Tr"), DefaultValue("")] public string TextTr { get; set; }
            [XmlElement("Zh"), DefaultValue("")] public string TextZh { get; set; }
            [XmlElement("Ru"), DefaultValue("")] public string TextRu { get; set; }
            [XmlElement("Nl"), DefaultValue("")] public string TextNl { get; set; }
            [XmlElement("Pt"), DefaultValue("")] public string TextPt { get; set; }
            [XmlElement("Ja"), DefaultValue("")] public string TextJa { get; set; }
            [XmlElement("Cs"), DefaultValue("")] public string TextCs { get; set; }
            [XmlElement("Pl"), DefaultValue("")] public string TextPl { get; set; }
        }

        public static string PropertyList(this object obj, string prefix)
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo p in props)
            {
                if (p.PropertyType == typeof(string))
                {
                    string value = p.GetValue(obj, null).ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        sb.AppendLine(prefix + p.Name + ": " + value);
                    }
                }
            }
            return sb.ToString();
        }

        public static string PropertyList(this object obj)
        {
            return obj.PropertyList("");
        }

        public static int TranslationCount(this object obj)
        {
            int count = 0;
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo p in props)
            {
                if (p.PropertyType == typeof(string))
                {
                    string value = p.GetValue(obj, null).ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static string GetTitleTranslated(this object obj, string language, string prefix = "Text")
        {
            try
            {
                if (string.IsNullOrEmpty(language) || language.Length < 2)
                {
                    return string.Empty;
                }

                string titlePropertyName = prefix + language.ToUpperInvariant()[0] + language.ToLowerInvariant()[1];
                Type objType = obj.GetType();
                PropertyInfo propertyTitle = objType.GetProperty(titlePropertyName);
                if (propertyTitle == null)
                {
                    titlePropertyName = prefix + "En";
                    propertyTitle = objType.GetProperty(titlePropertyName);
                }

                if (propertyTitle != null)
                {
                    string result = propertyTitle.GetValue(obj) as string;
                    return result ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool SetTranslation(this object obj, string language, string text, string prefix = "Text")
        {
            try
            {
                if (string.IsNullOrEmpty(language) || language.Length < 2)
                {
                    return false;
                }

                string titlePropertyName = prefix + language.ToUpperInvariant()[0] + language.ToLowerInvariant()[1];
                Type objType = obj.GetType();
                PropertyInfo propertyTitle = objType.GetProperty(titlePropertyName);
                if (propertyTitle == null)
                {
                    titlePropertyName = prefix + "En";
                    propertyTitle = objType.GetProperty(titlePropertyName);
                }

                if (propertyTitle != null)
                {
                    propertyTitle.SetValue(obj, text);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string MD5Hash(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    return BitConverter.ToString(hashBytes).Replace("-", "");
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static Int64 ConvertToInt(this string text, Int64 defaultValue = 0)
        {
            Int64 result = defaultValue;
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    result = Convert.ToInt64(text.Trim(), CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }
    }
}
