using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum TokenDetailedStatusEto
    {
        [EnumMember(Value = "OK")]
        OK,
        [EnumMember(Value = "ERROR")]
        ERROR,
        [EnumMember(Value = "MALFORMED")]
        MALFORMED,
        [EnumMember(Value = "LINKTOID_FORMAT")]
        LINKTOID_FORMAT,
        [EnumMember(Value = "LINKTOID_MISSING")]
        LINKTOID_MISSING,
        [EnumMember(Value = "LINKTOID_MISMATCH")]
        LINKTOID_MISMATCH,
        [EnumMember(Value = "LINKTOID_DENIED")]
        LINKTOID_DENIED,
        [EnumMember(Value = "LINKTOIDTYPE_DENIED")]
        LINKTOIDTYPE_DENIED,
        [EnumMember(Value = "VIN_MISSING")]
        VIN_MISSING,
        [EnumMember(Value = "VIN_DENIED")]
        VIN_DENIED,
        [EnumMember(Value = "FEATUREID_MISSING")]
        FEATUREID_MISSING,
        [EnumMember(Value = "FEATUREID_DENIED")]
        FEATUREID_DENIED,
        [EnumMember(Value = "FEATUREID_UNKNOWN")]
        FEATUREID_UNKNOWN,
        [EnumMember(Value = "TOKENPACKAGEREFERENCE_DENIED")]
        TOKENPACKAGEREFERENCE_DENIED,
        [EnumMember(Value = "TOKENPACKAGEREFERENCE_UNKNOWN")]
        TOKENPACKAGEREFERENCE_UNKNOWN,
        [EnumMember(Value = "TOKENPACKAGEREFERENCE_MISSMATCH")]
        TOKENPACKAGEREFERENCE_MISSMATCH,
        [EnumMember(Value = "TOKENPACKAGE_REBUILD_ERROR")]
        TOKENPACKAGE_REBUILD_ERROR,
        [EnumMember(Value = "VIN_MALFORMED")]
        VIN_MALFORMED,
        [EnumMember(Value = "NOT_A_DEVELOPMENT_VIN")]
        NOT_A_DEVELOPMENT_VIN,
        [EnumMember(Value = "NOT_A_CUSTOMER_VIN")]
        NOT_A_CUSTOMER_VIN,
        [EnumMember(Value = "ECU_UID_DENIED")]
        ECU_UID_DENIED,
        [EnumMember(Value = "NOT_A_DEVELOPMENT_ECU_UID")]
        NOT_A_DEVELOPMENT_ECU_UID,
        [EnumMember(Value = "NOT_A_CUSTOMER_ECU_UID")]
        NOT_A_CUSTOMER_ECU_UID,
        [EnumMember(Value = "ECU_UID_MALFORMED")]
        ECU_UID_MALFORMED,
        [EnumMember(Value = "ECU_UID_PREFIX_INVALID")]
        ECU_UID_PREFIX_INVALID,
        [EnumMember(Value = "FEATUREID_NOT_ALLOWED")]
        FEATUREID_NOT_ALLOWED,
        [EnumMember(Value = "FEATUREID_MALFORMED")]
        FEATUREID_MALFORMED,
        [EnumMember(Value = "NO_VALIDITY_CONDITION_ALLOWED")]
        NO_VALIDITY_CONDITION_ALLOWED,
        [EnumMember(Value = "VALIDITY_CONDITION_TYPE_DENIED")]
        VALIDITY_CONDITION_TYPE_DENIED,
        [EnumMember(Value = "VALIDITY_CONDITION_TYPE_UNKNOWN")]
        VALIDITY_CONDITION_TYPE_UNKNOWN,
        [EnumMember(Value = "VALIDITY_CONDITION_VALUE_NOT_ALLOWED")]
        VALIDITY_CONDITION_VALUE_NOT_ALLOWED,
        [EnumMember(Value = "VALIDITY_CONDITION_VALUE_MALFORMED")]
        VALIDITY_CONDITION_VALUE_MALFORMED,
        [EnumMember(Value = "NO_ENABLE_TYPE_ALLOWED")]
        NO_ENABLE_TYPE_ALLOWED,
        [EnumMember(Value = "ENABLE_TYPE_NOT_ALLOWED")]
        ENABLE_TYPE_NOT_ALLOWED,
        [EnumMember(Value = "ENABLE_TYPE_UNKNOWN")]
        ENABLE_TYPE_UNKNOWN,
        [EnumMember(Value = "NO_FEATURE_SPECIFIC_FIELD_ALLOWED")]
        NO_FEATURE_SPECIFIC_FIELD_ALLOWED,
        [EnumMember(Value = "FEATURE_SPECIFIC_FIELD_TYPE_NOT_ALLOWED")]
        FEATURE_SPECIFIC_FIELD_TYPE_NOT_ALLOWED,
        [EnumMember(Value = "FEATURE_SPECIFIC_FIELD_TYPE_UNKNOWN")]
        FEATURE_SPECIFIC_FIELD_TYPE_UNKNOWN,
        [EnumMember(Value = "FEATURE_SPECIFIC_FIELD_VALUE_NOT_ALLOWED")]
        FEATURE_SPECIFIC_FIELD_VALUE_NOT_ALLOWED,
        [EnumMember(Value = "FEATURE_SPECIFIC_FIELD_VALUE_MALFORMED")]
        FEATURE_SPECIFIC_FIELD_VALUE_MALFORMED,
        [EnumMember(Value = "FEATURE_SET_REFERENCE_DENIED")]
        FEATURE_SET_REFERENCE_DENIED,
        [EnumMember(Value = "FEATURE_SET_REFERENCE_UNKNOWN")]
        FEATURE_SET_REFERENCE_UNKNOWN,
        [EnumMember(Value = "FEATURE_SET_REFERENCE_MISSMATCH")]
        FEATURE_SET_REFERENCE_MISSMATCH,
        [EnumMember(Value = "FEATURE_SET_REFERENCE_REBUILD_ERROR")]
        FEATURE_SET_REFERENCE_REBUILD_ERROR,
        [EnumMember(Value = "REBUILD_ERROR")]
        REBUILD_ERROR,
        [EnumMember(Value = "DIAGNOSTIC_ADDRESS_MALFORMED")]
        DIAGNOSTIC_ADDRESS_MALFORMED,
        [EnumMember(Value = "E_VALIDITY_CONDITION_TAG_DUPLICATE")]
        E_VALIDITY_CONDITION_TAG_DUPLICATE,
        [EnumMember(Value = "E_FEATURE_SPECIFIC_FIELD_TAG_DUPLICATE")]
        E_FEATURE_SPECIFIC_FIELD_TAG_DUPLICATE,
        [EnumMember(Value = "E_NO_RIGHTS_ASSIGNED")]
        E_NO_RIGHTS_ASSIGNED,
        [EnumMember(Value = "E_ECU_UID_PREFIX_NOT_ALLOWED")]
        E_ECU_UID_PREFIX_NOT_ALLOWED,
        [EnumMember(Value = "E_KDS_CLIENT_NOT_ALLOWED")]
        E_KDS_CLIENT_NOT_ALLOWED,
        [EnumMember(Value = "E_KDS_ZSG_NOT_ALLOWED")]
        E_KDS_ZSG_NOT_ALLOWED,
        [EnumMember(Value = "E_UNAUTHORIZED_DOWNGRADE")]
        E_UNAUTHORIZED_DOWNGRADE,
        [EnumMember(Value = "E_NO_ENTRIES_IN_FSF")]
        E_NO_ENTRIES_IN_FSF,
        [EnumMember(Value = "E_INVALID_ENTRY_IN_FSF")]
        E_INVALID_ENTRY_IN_FSF,
        [EnumMember(Value = "E_DUPLICATE_TOKEN_REQUEST")]
        E_DUPLICATE_TOKEN_REQUEST,
        [EnumMember(Value = "E_UPGRADE_INDEX_EXCEEDS_THRESHOLD")]
        E_UPGRADE_INDEX_EXCEEDS_THRESHOLD,
        [EnumMember(Value = "UNDEFINED")]
        UNDEFINED
    }
}