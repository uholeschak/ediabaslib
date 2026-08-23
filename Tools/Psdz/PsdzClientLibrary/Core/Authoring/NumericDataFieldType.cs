using Newtonsoft.Json.Converters;
using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BMW.Authoring.API.MetaData.Enum
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [JsonConverter(typeof(StringEnumConverter))]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum NumericDataFieldType
    {
        [EnumMember(Value = "int")]
        Int,
        [EnumMember(Value = "uint")]
        UInt,
        [EnumMember(Value = "long")]
        Long,
        [EnumMember(Value = "ulong")]
        ULong,
        [EnumMember(Value = "short")]
        Short,
        [EnumMember(Value = "ushort")]
        UShort,
        [EnumMember(Value = "float")]
        Float,
        [EnumMember(Value = "double")]
        Double,
        [EnumMember(Value = "decimal")]
        Decimal
    }
}
