using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Authoring.API.MetaData
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataFieldType
    {
        [EnumMember(Value = "StringDataField")]
        StringDataField,
        [EnumMember(Value = "NumericDataField")]
        NumericDataField,
        [EnumMember(Value = "BoolDataField")]
        BoolDataField,
        [EnumMember(Value = "DateTimeDataField")]
        DateTimeDataField,
        [EnumMember(Value = "DateDataField")]
        DateDataField,
        [EnumMember(Value = "PicklistDataField")]
        PicklistDataField,
        [EnumMember(Value = "MultiPicklistDataField")]
        MultiPicklistDataField
    }
}
