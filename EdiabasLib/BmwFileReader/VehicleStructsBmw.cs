using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace BmwFileReader
{
    public static class VehicleStructsBmw
    {
        public const string VehicleSeriesXmlFile = "VehicleSeries.xml";
        public const string ServiceDataXmlFile = "ServiceData.xml";
        public const string ServiceDataZipFile = "ServiceData.zip";
        public const string RulesZipFile = "RulesInfo.zip";
        public const string RulesXmlFile = "RulesInfo.xml";
        public const string RulesCsFile = "RulesInfo.cs";
        public const string HashPrefix = "HASH_";

        [XmlType("VEI")]
        public class VehicleEcuInfo
        {
            public VehicleEcuInfo()
            {
            }

            public VehicleEcuInfo(int diagAddr, string name, string groupSgbd)
            {
                DiagAddr = diagAddr;
                Name = name;
                GroupSgbd = groupSgbd;
            }

            [XmlElement("DiagAddr")] public int DiagAddr { get; set; }
            [XmlElement("Name"), DefaultValue(null)] public string Name { get; set; }
            [XmlElement("GroupSgbd"), DefaultValue(null)] public string GroupSgbd { get; set; }
        }

        [XmlInclude(typeof(VehicleEcuInfo))]
        [XmlType("VSI")]
        public class VehicleSeriesInfo
        {
            public VehicleSeriesInfo()
            {
            }

            public VehicleSeriesInfo(string series, string brSgbd, string bnType, List<string> brandList = null, List<VehicleEcuInfo> ecuList = null, string date = null, string dateCompare = null)
            {
                Series = series;
                BrSgbd = brSgbd;
                BnType = bnType;
                BrandList = brandList;
                EcuList = ecuList;
                Date = date;
                DateCompare = dateCompare;
            }

            public void ResetDate()
            {
                Date = null;
                DateCompare = null;
            }

            [XmlElement("Series"), DefaultValue(null)] public string Series { get; set; }
            [XmlElement("BrSgbd"), DefaultValue(null)] public string BrSgbd { get; set; }
            [XmlElement("BnType"), DefaultValue(null)] public string BnType { get; set; }
            [XmlElement("BrandList"), DefaultValue(null)] public List<string> BrandList { get; set; }
            [XmlElement("EcuList"), DefaultValue(null)] public List<VehicleEcuInfo> EcuList { get; set; }
            [XmlElement("Date"), DefaultValue(null)] public string Date { get; set; }
            [XmlElement("DateCompare"), DefaultValue(null)] public string DateCompare { get; set; }
        }

        [XmlType("VersionInfo")]
        public class VersionInfo
        {
            public class VersionStringComparer : IComparer<string>
            {
                public int Compare(string x, string y)
                {
                    if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
                    {
                        return 0;
                    }

                    if (string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return 0;
                    }

                    var version = new { First = GetVersion(x), Second = GetVersion(y) };
                    int limit = Math.Max(version.First.Length, version.Second.Length);
                    for (int i = 0; i < limit; i++)
                    {
                        int first = version.First.ElementAtOrDefault(i);
                        int second = version.Second.ElementAtOrDefault(i);
                        if (first > second)
                        {
                            return 1;
                        }

                        if (second > first)
                        {
                            return -1;
                        }
                    }
                    return 0;
                }

                private int[] GetVersion(string version)
                {
                    return (from part in version.Split('.')
                        select Parse(part)).ToArray();
                }

                private int Parse(string version)
                {
                    if (!int.TryParse(version, out var result))
                    {
                        return 0;
                    }

                    return result;
                }
            }

            public VersionInfo() : this(null, null)
            {
            }

            public VersionInfo(string version, DateTime? dateTime)
            {
                Version = version;
                if (dateTime != null)
                {
                    Date = dateTime.Value;
                }
            }

            public bool IsIdentical(string version, DateTime? dateTime)
            {
                if (Version == null || version == null)
                {
                    return false;
                }

                VersionStringComparer versionComparer = new VersionStringComparer();
                int compareResult = versionComparer.Compare(version, Version);
                if (compareResult != 0)
                {
                    return false;
                }

                if (dateTime == null || Date != dateTime)
                {
                    return false;
                }

                return true;
            }

            public bool IsMinVersion(string version, DateTime? dateTime)
            {
                try
                {
                    if (version == null || Version == null)
                    {
                        return false;
                    }

                    VersionStringComparer versionComparer = new VersionStringComparer();
                    int compareResult = versionComparer.Compare(Version, version);
                    if (compareResult < 0)
                    {
                        return false;
                    }

                    if (dateTime == null || Date < dateTime)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            [XmlElement("Version"), DefaultValue(null)] public string Version { get; set; }
            [XmlElement("Date"), DefaultValue(null)] public DateTime Date { get; set; }
        }

        [XmlInclude(typeof(VersionInfo))]
        [XmlInclude(typeof(VehicleSeriesInfo))]
        [XmlType("VehicleSeriesInfoDataXml")]
        public class VehicleSeriesInfoData
        {
            public VehicleSeriesInfoData() : this(null, null, null)
            {
            }

            public VehicleSeriesInfoData(string timeStamp, VersionInfo versionInfo, SerializableDictionary<string, List<VehicleSeriesInfo>> vehicleSeriesDict)
            {
                TimeStamp = timeStamp;
                Version = versionInfo;
                VehicleSeriesDict = vehicleSeriesDict;
            }

            [XmlElement("TimeStamp"), DefaultValue(null)] public string TimeStamp { get; set; }
            [XmlElement("Version"), DefaultValue(null)] public VersionInfo Version { get; set; }
            [XmlElement("VehicleSeriesDict"), DefaultValue(null)] public SerializableDictionary<string, List<VehicleSeriesInfo>> VehicleSeriesDict { get; set; }
        }

        [XmlType("RI")]
        public class RuleInfo
        {
            public RuleInfo()
            {
            }

            public RuleInfo(string id, string ruleFormula)
            {
                Id = id;
                RuleFormula = ruleFormula;
            }

            [XmlElement("Id"), DefaultValue(null)] public string Id { get; set; }
            [XmlElement("RF"), DefaultValue(null)] public string RuleFormula { get; set; }
        }

        [XmlInclude(typeof(RuleInfo))]
        [XmlType("RulesInfoData")]
        public class RulesInfoData
        {
            public RulesInfoData() : this(null, null, null)
            {
            }

            public RulesInfoData(VersionInfo versionInfo, SerializableDictionary<string, RuleInfo> faultRuleDict, SerializableDictionary<string, RuleInfo> ecuFuncRuleDict)
            {
                Version = versionInfo;
                FaultRuleDict = faultRuleDict;
                EcuFuncRuleDict = ecuFuncRuleDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VersionInfo Version { get; set; }
            [XmlElement("FaultRuleDict"), DefaultValue(null)] public SerializableDictionary<string, RuleInfo> FaultRuleDict { get; set; }
            [XmlElement("EcuFuncRuleDict"), DefaultValue(null)] public SerializableDictionary<string, RuleInfo> EcuFuncRuleDict { get; set; }
        }

        [XmlInclude(typeof(ServiceDataItem))]
        [XmlInclude(typeof(ServiceTextData))]
        [XmlType("SD")]
        public class ServiceData
        {
            public ServiceData() : this(null, null, null)
            {
            }

            public ServiceData(VersionInfo versionInfo, List<ServiceDataItem> serviceDataList, SerializableDictionary<string, ServiceTextData> textDict)
            {
                Version = versionInfo;
                ServiceDataList = serviceDataList;
                TextDict = textDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VersionInfo Version { get; set; }
            [XmlElement("SD"), DefaultValue(null)] public List<ServiceDataItem> ServiceDataList { get; set; }
            [XmlElement("TD"), DefaultValue(null)] public SerializableDictionary<string, ServiceTextData> TextDict { get; set; }
        }

        [XmlInclude(typeof(ServiceInfoData))]
        [XmlInclude(typeof(ServiceTextData))]
        [XmlType("SDI")]
        public class ServiceDataItem
        {
            public ServiceDataItem() : this(null, null, null, null, null)
            {
            }

            public ServiceDataItem(string infoObjId, string infoObjTextHash, List<string> diagObjIds, List<string> diagObjTextHashes, List<ServiceInfoData> infoDataList)
            {
                InfoObjId = infoObjId;
                InfoObjTextHash = infoObjTextHash;
                DiagObjIds = diagObjIds;
                DiagObjTextHashes = diagObjTextHashes;
                InfoDataList = infoDataList;
            }

            [XmlElement("IOI"), DefaultValue(null)] public string InfoObjId { get; set; }
            [XmlElement("IOTH"), DefaultValue(null)] public string InfoObjTextHash { get; set; }
            [XmlElement("DOI"), DefaultValue(null)] public List<string> DiagObjIds { get; set; }
            [XmlElement("DOTH"), DefaultValue(null)] public List<string> DiagObjTextHashes { get; set; }
            [XmlElement("IDL"), DefaultValue(null)] public List<ServiceInfoData> InfoDataList { get; set; }
        }

        [XmlType("SID")]
        public class ServiceInfoData
        {
            public ServiceInfoData()
            {
            }

            public ServiceInfoData(string methodName, string controlId, string ediabasJobBare, string ediabasJobOverride, List<string> resultList, List<string> textHashes)
            {
                MethodName = methodName;
                ControlId = controlId;
                EdiabasJobBare = ediabasJobBare;
                EdiabasJobOverride = ediabasJobOverride;
                ResultList = resultList;
                TextHashes = textHashes;
            }

            [XmlElement("MN"), DefaultValue(null)] public string MethodName { get; set; }
            [XmlElement("CI"), DefaultValue(null)] public string ControlId { get; set; }
            [XmlElement("EJB"), DefaultValue(null)] public string EdiabasJobBare { get; set; }
            [XmlElement("EJO"), DefaultValue(null)] public string EdiabasJobOverride { get; set; }
            [XmlElement("RL"), DefaultValue(null)] public List<string> ResultList { get; set; }
            [XmlElement("TH"), DefaultValue(null)] public List<string> TextHashes { get; set; }
        }

        [XmlType("STD")]
        public class ServiceTextData
        {
            public ServiceTextData() : this(null)
            {
            }

            public ServiceTextData(EcuFunctionStructs.EcuTranslation translation)
            {
                Translation = translation;
                Hash = CalculateHash();
            }

            [XmlElement("TR"), DefaultValue(null)] public EcuFunctionStructs.EcuTranslation Translation { get; set; }
            [XmlIgnore, DefaultValue(null)] public string Hash { get; set; }

            public string CalculateHash()
            {
                if (Translation == null)
                {
                    return string.Empty;
                }

                return Translation.PropertyList().MD5Hash();
            }
        }
    }
}
