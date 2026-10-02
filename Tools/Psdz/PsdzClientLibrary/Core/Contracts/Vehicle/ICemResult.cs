namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    public interface ICemResult
    {
        byte STAT_CEM_MAPPING_ID { get; set; }

        string STAT_CEM_MAPPING_ID_TEXT { get; set; }

        ulong STAT_CEM_ZEITSTEMPEL { get; set; }

        uint STAT_CEM_ZEITSTEMPEL_MS { get; set; }

        byte STAT_CEM_QUALITY { get; set; }

        string STAT_CEM_QUALITY_TEXT { get; set; }

        ulong STAT_CEM_INCOMING_TIME { get; set; }

        uint STAT_CEM_INCOMING_TIME_MS { get; set; }

        byte STAT_CEM_INCOMING_TIME_QUALITY { get; set; }

        string STAT_CEM_INCOMING_TIME_QUALITY_TEXT { get; set; }

        string STAT_CEM_ACTIVE_STATE_HEX { get; set; }

        string STAT_CEM_ACTIVE_STATE_BIT_0 { get; set; }

        string STAT_CEM_ACTIVE_STATE_BIT_2 { get; set; }

        string STAT_CEM_ACTIVE_STATE_BIT_3 { get; set; }

        string STAT_CEM_ACTIVE_STATE_BIT_7 { get; set; }

        string STAT_CEM_MESSAGE_TYPE_HEX { get; set; }

        string STAT_CEM_MESSAGE_TYPE_TEXT { get; set; }

        string STAT_CEM_ADRESSE_SG_HEX { get; set; }

        string STAT_CEM_SGBD_INDEX_HEX { get; set; }

        string STAT_CEM_MELDUNG_NR_HEX { get; set; }

        string STAT_CEM_MELDUNG_TEXT { get; set; }

        byte STAT_CEM_CURRENT_WUC_WERT { get; set; }

        byte STAT_CEM_CURRENT_TRIP_WERT { get; set; }

        ulong STAT_SYSKONTEXT_TIMESTAMP_WERT { get; set; }

        uint STAT_SYSKONTEXT_UTC_YEAR_WERT { get; set; }

        byte STAT_SYSKONTEXT_UTC_MONTH_WERT { get; set; }

        byte STAT_SYSKONTEXT_UTC_DAY_WERT { get; set; }

        byte STAT_SYSKONTEXT_UTC_HOUR_WERT { get; set; }

        byte STAT_SYSKONTEXT_UTC_MINUTE_WERT { get; set; }

        byte STAT_SYSKONTEXT_UTC_SECOND_WERT { get; set; }

        double STAT_SYSKONTEXT_TIMEZONE_WERT { get; set; }

        double STAT_SYSKONTEXT_DAYLIGHTSAVINGOFFSET_WERT { get; set; }

        string STAT_SYSKONTEXT_UTC_QUALIFIER_WERT { get; set; }

        ulong STAT_SYSKONTEXT_TIMESTAMP_S_WAKEUP_WERT { get; set; }

        double STAT_SYSKONTEXT_VOLTAGE12V_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_VOLTAGE12V_MAX_WERT { get; set; }

        double STAT_SYSKONTEXT_AMBIENT_TEMPERATURE_WERT { get; set; }

        double STAT_SYSKONTEXT_MILEAGESUPREME_WERT { get; set; }

        string STAT_SYSKONTEXT_MILEAGESUPREME_IN_SYNC_WERT { get; set; }

        double STAT_SYSKONTEXT_VEHICLESPEED_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_VEHICLESPEED_MAX_WERT { get; set; }

        string STAT_SYSKONTEXT_PWF_MIN_WERT { get; set; }

        string STAT_SYSKONTEXT_PWF_MAX_WERT { get; set; }

        string STAT_SYSKONTEXT_DRIVERS_ACCOUNT_ID_CRC_WERT { get; set; }

        double STAT_SYSKONTEXT_LONGITUDINAL_ACCEL_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_LONGITUDINAL_ACCEL_MAX_WERT { get; set; }

        string STAT_SYSKONTEXT_PREVPWFSTATETOCURPWF_WERT { get; set; }

        ulong STAT_SYSKONTEXT_TIMESTAMP_PREVPWFTOCURPWF_WERT { get; set; }

        double STAT_SYSKONTEXT_LINKVOLTAGE_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_LINKVOLTAGE_MAX_WERT { get; set; }

        string STAT_SYSKONTEXT_VEHICLEFUNCTIONS1_MIN_WERT { get; set; }

        string STAT_SYSKONTEXT_VEHICLEFUNCTIONS1_MAX_WERT { get; set; }

        string STAT_ZFS_KOMPLEX { get; set; }

        string JOB_STATUS { get; set; }
    }
}