using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BMW.Authoring.API.MetaData
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StringInputDataType
    {
        [EnumMember(Value = "Numerical")]
        Numerical,
        [EnumMember(Value = "AlphaNumerical")]
        AlphaNumerical,
        [EnumMember(Value = "Decimal")]
        Decimal,
        [EnumMember(Value = "Hex")]
        Hex
    }
}
