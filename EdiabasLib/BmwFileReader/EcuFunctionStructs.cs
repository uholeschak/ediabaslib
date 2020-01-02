using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace BmwFileReader
{
    public static class EcuFunctionStructs
    {
        [XmlInclude(typeof(RefEcuVariant)), XmlInclude(typeof(EcuFuncStruct))]
        public class EcuVariant
        {
            public EcuVariant()
            {
                Id = string.Empty;
                TitleEn = string.Empty;
                TitleDe = string.Empty;
                TitleRu = string.Empty;
                GroupId = string.Empty;
            }

            public EcuVariant(string id, string titleEn, string titleDe, string titleRu, string groupId,
                List<string> groupFunctionIds)
            {
                Id = id;
                TitleEn = titleEn;
                TitleDe = titleDe;
                TitleRu = titleRu;
                GroupId = groupId;
                GroupFunctionIds = groupFunctionIds;
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

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public string GetTitle(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return TitleDe;

                    case "ru":
                        return TitleRu;
                }

                return TitleEn;
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string TitleEn { get; set; }
            [XmlElement, DefaultValue("")] public string TitleDe { get; set; }
            [XmlElement, DefaultValue("")] public string TitleRu { get; set; }
            [XmlElement, DefaultValue("")] public string GroupId { get; set; }
            [XmlArray] public List<string> GroupFunctionIds { get; set; }
            [XmlArray] public List<RefEcuVariant> RefEcuVariantList { get; set; }
            [XmlArray] public List<EcuFuncStruct> EcuFuncStructList { get; set; }
        }

        [XmlInclude(typeof(EcuJob))]
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
                TitleEn = string.Empty;
                TitleDe = string.Empty;
                TitleRu = string.Empty;
                PrepOpEn = string.Empty;
                PrepOpDe = string.Empty;
                PrepOpRu = string.Empty;
                ProcOpEn = string.Empty;
                ProcOpDe = string.Empty;
                ProcOpRu = string.Empty;
                PostOpEn = string.Empty;
                PostOpDe = string.Empty;
                PostOpRu = string.Empty;
            }

            public EcuFixedFuncStruct(string id, string nodeClass, string nodeClassName, string titleEn, string titleDe,
                string titleRu,
                string prepOpEn, string prepOpDe, string prepOpRu,
                string procOpEn, string procOpDe, string procOpRu,
                string postOpEn, string postOpDe, string postOpRu)
            {
                Id = id;
                NodeClass = nodeClass;
                NodeClassName = nodeClassName;
                TitleEn = titleEn;
                TitleDe = titleDe;
                TitleRu = titleRu;
                PrepOpEn = prepOpEn;
                PrepOpDe = prepOpDe;
                PrepOpRu = prepOpRu;
                ProcOpEn = procOpEn;
                ProcOpDe = procOpDe;
                ProcOpRu = procOpRu;
                PostOpEn = postOpEn;
                PostOpDe = postOpDe;
                PostOpRu = postOpRu;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FIXEDFUNC:");
                sb.Append(this.PropertyList(prefix + " "));
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

            public string GetTitle(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return TitleDe;

                    case "ru":
                        return TitleRu;
                }

                return TitleEn;
            }

            public string GetPreOp(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return PrepOpDe;

                    case "ru":
                        return PrepOpRu;
                }

                return PrepOpEn;
            }

            public string GetProcOp(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return ProcOpDe;

                    case "ru":
                        return ProcOpRu;
                }

                return ProcOpEn;
            }

            public string GetPostOp(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return PostOpDe;

                    case "ru":
                        return PostOpRu;
                }

                return PostOpEn;
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
            [XmlElement, DefaultValue("")] public string TitleEn { get; set; }
            [XmlElement, DefaultValue("")] public string TitleDe { get; set; }
            [XmlElement, DefaultValue("")] public string TitleRu { get; set; }
            [XmlElement, DefaultValue("")] public string PrepOpEn { get; set; }
            [XmlElement, DefaultValue("")] public string PrepOpDe { get; set; }
            [XmlElement, DefaultValue("")] public string PrepOpRu { get; set; }
            [XmlElement, DefaultValue("")] public string ProcOpEn { get; set; }
            [XmlElement, DefaultValue("")] public string ProcOpDe { get; set; }
            [XmlElement, DefaultValue("")] public string ProcOpRu { get; set; }
            [XmlElement, DefaultValue("")] public string PostOpEn { get; set; }
            [XmlElement, DefaultValue("")] public string PostOpDe { get; set; }
            [XmlElement, DefaultValue("")] public string PostOpRu { get; set; }
            [XmlArray] public List<EcuJob> EcuJobList { get; set; }
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
            [XmlArray] public List<EcuFixedFuncStruct> FixedFuncStructList { get; set; }
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

        [XmlInclude(typeof(EcuFixedFuncStruct))]
        public class EcuFuncStruct
        {
            public EcuFuncStruct()
            {
                Id = string.Empty;
                TitleEn = string.Empty;
                TitleDe = string.Empty;
                TitleRu = string.Empty;
            }

            public EcuFuncStruct(string id, string titleEn, string titleDe, string titleRu)
            {
                Id = id;
                TitleEn = titleEn;
                TitleDe = titleDe;
                TitleRu = titleRu;
            }

            public string ToString(string prefix)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(prefix + "FUNC:");
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

            public string GetTitle(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return TitleDe;

                    case "ru":
                        return TitleRu;
                }

                return TitleEn;
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string TitleEn { get; set; }
            [XmlElement, DefaultValue("")] public string TitleDe { get; set; }
            [XmlElement, DefaultValue("")] public string TitleRu { get; set; }
            [XmlArray] public List<EcuFixedFuncStruct> FixedFuncStructList { get; set; }
        }

        [XmlInclude(typeof(EcuJobParameter)), XmlInclude(typeof(EcuJobResult))]
        public class EcuJob
        {
            public EcuJob()
            {
                Id = string.Empty;
                FuncNameJob = string.Empty;
                Name = string.Empty;
            }

            public EcuJob(string id, string funcNameJob, string name)
            {
                Id = id;
                FuncNameJob = funcNameJob;
                Name = name;
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

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string FuncNameJob { get; set; }
            [XmlElement, DefaultValue("")] public string Name { get; set; }
            [XmlArray] public List<EcuJobParameter> EcuJobParList { get; set; }
            [XmlArray] public List<EcuJobResult> EcuJobResultList { get; set; }
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
            [XmlElement, DefaultValue("")] public string AdapterPath { get; set; }
            [XmlElement, DefaultValue("")] public string Name { get; set; }
        }

        [XmlInclude(typeof(EcuResultStateValue))]
        public class EcuJobResult
        {
            public EcuJobResult()
            {
                Id = string.Empty;
                TitleEn = string.Empty;
                TitleDe = string.Empty;
                TitleRu = string.Empty;
                AdapterPath = string.Empty;
                Name = string.Empty;
                Location = string.Empty;
                Unit = string.Empty;
                UnitFixed = string.Empty;
                Format = string.Empty;
                Mult = string.Empty;
                Offset = string.Empty;
                Round = string.Empty;
                NumberFormat = string.Empty;
            }

            public EcuJobResult(string id, string titleEn, string titleDe, string titleRu, string funcNameResult, string adapterPath,
                string name, string ecuFuncRelevant, string location, string unit, string unitFixed, string format, string mult, string offset,
                string round, string numberFormat)
            {
                Id = id;
                TitleEn = titleEn;
                TitleDe = titleDe;
                TitleRu = titleRu;
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

            public string GetTitle(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return TitleDe;

                    case "ru":
                        return TitleRu;
                }

                return TitleEn;
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string TitleEn { get; set; }
            [XmlElement, DefaultValue("")] public string TitleDe { get; set; }
            [XmlElement, DefaultValue("")] public string TitleRu { get; set; }
            [XmlElement, DefaultValue("")] public string FuncNameResult { get; set; }
            [XmlElement, DefaultValue("")] public string AdapterPath { get; set; }
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
            [XmlArray] public List<EcuResultStateValue> EcuResultStateValueList { get; set; }
        }

        public class EcuResultStateValue
        {
            public EcuResultStateValue()
            {
                Id = string.Empty;
                TitleEn = string.Empty;
                TitleDe = string.Empty;
                TitleRu = string.Empty;
                StateValue = string.Empty;
                ValidFrom = string.Empty;
                ValidTo = string.Empty;
                ParentId = string.Empty;
            }

            public EcuResultStateValue(string id, string titleEn, string titleDe, string titleRu,
                string stateValue, string validFrom, string validTo, string parentId)
            {
                Id = id;
                TitleEn = titleEn;
                TitleDe = titleDe;
                TitleRu = titleRu;
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
                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString("");
            }

            public string GetTitle(string language)
            {
                if (string.IsNullOrEmpty(language))
                {
                    return string.Empty;
                }

                string lang = language.ToLowerInvariant();
                switch (lang)
                {
                    case "de":
                        return TitleDe;

                    case "ru":
                        return TitleRu;
                }

                return TitleEn;
            }

            [XmlElement, DefaultValue("")] public string Id { get; set; }
            [XmlElement, DefaultValue("")] public string TitleEn { get; set; }
            [XmlElement, DefaultValue("")] public string TitleDe { get; set; }
            [XmlElement, DefaultValue("")] public string TitleRu { get; set; }
            [XmlElement, DefaultValue("")] public string StateValue { get; set; }
            [XmlElement, DefaultValue("")] public string ValidFrom { get; set; }
            [XmlElement, DefaultValue("")] public string ValidTo { get; set; }
            [XmlElement, DefaultValue("")] public string ParentId { get; set; }
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
    }
}
