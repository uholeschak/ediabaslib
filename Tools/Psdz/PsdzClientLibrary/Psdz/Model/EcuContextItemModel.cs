using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum EcuContextItemModel
    {
        [EnumMember(Value = "PROGRAMMING_COUNTER")]
        PROGRAMMING_COUNTER,
        [EnumMember(Value = "SERIAL_NUMBER")]
        SERIAL_NUMBER,
        [EnumMember(Value = "SGBDID")]
        SGBDID,
        [EnumMember(Value = "FLASH_TIMING_PARAMETER")]
        FLASH_TIMING_PARAMETER,
        [EnumMember(Value = "SUPPLIER_ID")]
        SUPPLIER_ID,
        [EnumMember(Value = "MANUFACTURING_DATE")]
        MANUFACTURING_DATE,
        [EnumMember(Value = "LAST_PROGRAMMING_DATE")]
        LAST_PROGRAMMING_DATE,
        [EnumMember(Value = "PERFORMED_FLASH_CYCLES")]
        PERFORMED_FLASH_CYCLES,
        [EnumMember(Value = "REMAINING_FLASH_CYCLES")]
        REMAINING_FLASH_CYCLES,
        [EnumMember(Value = "FINGER_PRINT")]
        FINGER_PRINT
    }
}