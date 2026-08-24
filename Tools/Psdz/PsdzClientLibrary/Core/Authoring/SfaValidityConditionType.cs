using Newtonsoft.Json.Converters;
using PsdzClient.Core;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BMW.Authoring.API.Implementation.Sfa.Models.Request
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SfaValidityConditionType
    {
        [EnumMember(Value = "0x04")]
        DAYS_AFTER_ACTIVATION = 4,
        [EnumMember(Value = "0x00")]
        END_OF_CONDITIONS = 0,
        [EnumMember(Value = "0x02")]
        EXPIRATION_DATE = 2,
        [EnumMember(Value = "0x06")]
        KM_AFTER_ACTIVATION = 6,
        [EnumMember(Value = "0x08")]
        LOCAL_RELATIVE_TIME = 8,
        [EnumMember(Value = "0x09")]
        NUMBER_OF_DRIVING_CYCLES = 9,
        [EnumMember(Value = "0x07")]
        NUMBER_OF_EXECUTIONS = 7,
        [EnumMember(Value = "0x0A")]
        SPEED_TRESHOLD = 10,
        [EnumMember(Value = "0x05")]
        START_AND_END_ODOMETER_READING = 5,
        [EnumMember(Value = "0x03")]
        TIME_PERIOD = 3,
        [EnumMember(Value = "0x01")]
        UNLIMITED = 1
    }
}
