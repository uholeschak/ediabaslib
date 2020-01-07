using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace BmwFileReader
{
    public static class EcuFunctionStructs
    {
        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuFuncStruct)), XmlInclude(typeof(EcuFaultCode))]
        public class EcuVariant
        {
            public EcuVariant()
            {
                Id = string.Empty;
                GroupId = string.Empty;
                GroupFunctionIds = null;
                Title = null;
            }

            public EcuVariant(string id, string groupId, EcuTranslation title, List<string> groupFunctionIds)
            {
                Id = id;
                GroupId = groupId;
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
                    foreach (string GroupFunctionId in GroupFunctionIds)
                    {
                        sb.Append(prefix + " " + GroupFunctionId);
                    }
                }

                if (Title != null)
                {
                    sb.Append(prefix + " " + Title);
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

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string GroupId { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlArray, DefaultValue(null)] public List<string> GroupFunctionIds { get; set; }
            [XmlArray, DefaultValue(null)] public List<RefEcuVariant> RefEcuVariantList { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuFuncStruct> EcuFuncStructList { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuFaultCode> EcuFaultCodeList { get; set; }
        }

        public class EcuFaultCode
        {
            public EcuFaultCode()
            {
                Id = string.Empty;
                Code = string.Empty;
            }

            public EcuFaultCode(string id, string code)
            {
                Id = id;
                Code = code;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FAULTCODE:");
                sb.Append(this.PropertyList(prefix + " "));

                if (EcuFaultCodeLabelList != null)
                {
                    foreach (EcuFaultCodeLabel ecuFaultCodeLabel in EcuFaultCodeLabelList)
                    {
                        sb.Append(ecuFaultCodeLabel.ToString(prefix + " "));
                    }
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string Code { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuFaultCodeLabel> EcuFaultCodeLabelList { get; set; }
        }

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
                sb.AppendLine(prefix + "FAULTLABEL:");
                sb.Append(this.PropertyList(prefix + " "));

                if (Title != null)
                {
                    sb.Append(prefix + " " + Title);
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string Code { get; set; }
            [XmlElement, DefaultValue("")] public string SaeCode { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement, DefaultValue("")] public string Relevance { get; set; }
            [XmlElement, DefaultValue("")] public string DataType { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuJob))]
        public class EcuFixedFuncStruct
        {
            public enum NodeClassType
            {
                Unknown,
                Identification,
                ReadState,
                ControlActuator
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
                    sb.Append(prefix + " " + Title);
                }

                if (PrepOp != null)
                {
                    sb.Append(prefix + " PRE-" + PrepOp);
                }

                if (ProcOp != null)
                {
                    sb.Append(prefix + " PROC-" + ProcOp);
                }

                if (PostOp != null)
                {
                    sb.Append(prefix + " POST-" + PostOp);
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

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string NodeClass { get; set; }
            [XmlElement, DefaultValue("")] public string NodeClassName { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation PrepOp { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation ProcOp { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation PostOp { get; set; }
            [XmlElement, DefaultValue("")] public string SortOrder { get; set; }
            [XmlElement, DefaultValue("")] public string Activation { get; set; }
            [XmlElement, DefaultValue("")] public string ActivationDurationMs { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuJob> EcuJobList { get; set; }
        }

        [XmlInclude(typeof(EcuFixedFuncStruct))]
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

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string EcuVariantId { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuFixedFuncStruct> FixedFuncStructList { get; set; }
        }

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
            [XmlElement, DefaultValue("")] public string GroupFuncId { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuFixedFuncStruct))]
        public class EcuFuncStruct
        {
            public EcuFuncStruct()
            {
                Id = string.Empty;
                Title = null;
                MultiSelect = string.Empty;
            }

            public EcuFuncStruct(string id, EcuTranslation title, string multiSelect)
            {
                Id = id;
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
                    sb.Append(prefix + " " + Title);
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

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement, DefaultValue("")] public string MultiSelect { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuFixedFuncStruct> FixedFuncStructList { get; set; }
        }

        [XmlInclude(typeof(EcuJobParameter)), XmlInclude(typeof(EcuJobResult))]
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

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string FuncNameJob { get; set; }
            [XmlElement, DefaultValue("")] public string Name { get; set; }
            [XmlElement, DefaultValue("")] public string Phase { get; set; }
            [XmlElement, DefaultValue("")] public string Rank { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuJobParameter> EcuJobParList { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuJobResult> EcuJobResultList { get; set; }
        }

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
            [XmlElement, DefaultValue("")] public string Value { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string AdapterPath { get; set; }
            [XmlElement, DefaultValue("")] public string Name { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation)), XmlInclude(typeof(EcuResultStateValue))]
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
                    sb.Append(prefix + " " + Title);
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
            [XmlElement, DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement, DefaultValue("")] public string FuncNameResult { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string AdapterPath { get; set; }
            [XmlElement, DefaultValue("")] public string Name { get; set; }
            [XmlElement, DefaultValue("")] public string EcuFuncRelevant { get; set; }
            [XmlElement, DefaultValue("")] public string Location { get; set; }
            [XmlElement, DefaultValue("")] public string Unit { get; set; }
            [XmlElement, DefaultValue("")] public string UnitFixed { get; set; }
            [XmlElement, DefaultValue("")] public string Format { get; set; }
            [XmlElement, DefaultValue("")] public string Mult { get; set; }
            [XmlElement, DefaultValue("")] public string Offset { get; set; }
            [XmlElement, DefaultValue("")] public string Round { get; set; }
            [XmlElement, DefaultValue("")] public string NumberFormat { get; set; }
            [XmlArray, DefaultValue(null)] public List<EcuResultStateValue> EcuResultStateValueList { get; set; }
        }

        [XmlInclude(typeof(EcuTranslation))]
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
                    sb.Append(prefix + " " + Title);
                }
                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue(null)] public EcuTranslation Title { get; set; }
            [XmlElement, DefaultValue("")] public string StateValue { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string ValidFrom { get; set; }
            [XmlIgnore, XmlElement, DefaultValue("")] public string ValidTo { get; set; }
            [XmlElement, DefaultValue("")] public string ParentId { get; set; }
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

            [XmlElement, DefaultValue("")] public string TextDe { get; set; }
            [XmlElement, DefaultValue("")] public string TextEn { get; set; }
            [XmlElement, DefaultValue("")] public string TextFr { get; set; }
            [XmlElement, DefaultValue("")] public string TextTh { get; set; }
            [XmlElement, DefaultValue("")] public string TextSv { get; set; }
            [XmlElement, DefaultValue("")] public string TextIt { get; set; }
            [XmlElement, DefaultValue("")] public string TextEs { get; set; }
            [XmlElement, DefaultValue("")] public string TextId { get; set; }
            [XmlElement, DefaultValue("")] public string TextKo { get; set; }
            [XmlElement, DefaultValue("")] public string TextEl { get; set; }
            [XmlElement, DefaultValue("")] public string TextTr { get; set; }
            [XmlElement, DefaultValue("")] public string TextZh { get; set; }
            [XmlElement, DefaultValue("")] public string TextRu { get; set; }
            [XmlElement, DefaultValue("")] public string TextNl { get; set; }
            [XmlElement, DefaultValue("")] public string TextPt { get; set; }
            [XmlElement, DefaultValue("")] public string TextJa { get; set; }
            [XmlElement, DefaultValue("")] public string TextCs { get; set; }
            [XmlElement, DefaultValue("")] public string TextPl { get; set; }
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

        public static Int64 ConvertToInt(this string text)
        {
            Int64 result = 0;
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
