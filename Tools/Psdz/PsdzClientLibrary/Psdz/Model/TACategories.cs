using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum TACategories
    {
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN,
        [EnumMember(Value = "BL_FLASH")]
        BL_FLASH,
        [EnumMember(Value = "CD_DEPLOY")]
        CD_DEPLOY,
        [EnumMember(Value = "HW_DEINSTALL")]
        HW_DEINSTALL,
        [EnumMember(Value = "HW_INSTALL")]
        HW_INSTALL,
        [EnumMember(Value = "ID_BACKUP")]
        ID_BACKUP,
        [EnumMember(Value = "ID_RESTORE")]
        ID_RESTORE,
        [EnumMember(Value = "SW_DEPLOY")]
        SW_DEPLOY,
        [EnumMember(Value = "FSC_DEPLOY")]
        FSC_DEPLOY,
        [EnumMember(Value = "FSC_BACKUP")]
        FSC_BACKUP,
        [EnumMember(Value = "IBA_DEPLOY")]
        IBA_DEPLOY,
        [EnumMember(Value = "HDD_UPDATE")]
        HDD_UPDATE,
        [EnumMember(Value = "FSC_DEPLOY_PREHWD")]
        FSC_DEPLOY_PREHWD,
        [EnumMember(Value = "GATEWAY_TABLE_DEPLOY")]
        GATEWAY_TABLE_DEPLOY,
        [EnumMember(Value = "SFA_DEPLOY")]
        SFA_DEPLOY,
        [EnumMember(Value = "ECU_ACTIVATE")]
        ECU_ACTIVATE,
        [EnumMember(Value = "ECU_POLL")]
        ECU_POLL,
        [EnumMember(Value = "ECU_MIRROR_DEPLOY")]
        ECU_MIRROR_DEPLOY,
        [EnumMember(Value = "SMAC_TRANSFER_START")]
        SMAC_TRANSFER_START,
        [EnumMember(Value = "SMAC_TRANSFER_STATUS")]
        SMAC_TRANSFER_STATUS
    }
}