using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IZfsResult : IHideObjectMembers
    {
        ulong STAT_DM_ZEITSTEMPEL { get; set; }

        uint STAT_DM_ZEITSTEMPEL_MS { get; set; }

        string STAT_DM_TS_1AS { get; set; }

        string STAT_DM_TS_1AS_MS { get; set; }

        byte STAT_DM_ADRESSE_SG { get; set; }

        long STAT_DM_SGBD_INDEX { get; set; }

        byte STAT_DM_MELDUNG_TYP { get; set; }

        byte STAT_DM_MESSAGE_TYPE { get; set; }

        string STAT_DM_MESSAGE_TYPE_TEXT { get; set; }

        byte STAT_DM_ACTIVE_STATE { get; set; }

        long STAT_DM_MELDUNG_NR { get; set; }

        string STAT_DM_MELDUNG_TEXT { get; set; }

        byte STAT_DM_MAPPING_ID { get; set; }

        ulong STAT_SYSKONTEXT_ZEITSTEMPEL_WERT { get; set; }

        int STAT_SYSKONTEXT_KUNDENZEIT_JAHR_WERT { get; set; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_MONAT_WERT { get; set; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_TAG_WERT { get; set; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_STUNDE_WERT { get; set; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_MINUTE_WERT { get; set; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_SEKUNDE_WERT { get; set; }

        ulong STAT_SYSKONTEXT_ZEIT_WECKEN_WERT { get; set; }

        ulong STAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERT { get; set; }

        ulong STAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERT { get; set; }

        ulong STAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERT { get; set; }

        byte STAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERT { get; set; }

        byte STAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERT { get; set; }

        ulong STAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERT { get; set; }

        byte STAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERT { get; set; }

        ulong STAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERT { get; set; }

        byte STAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERT { get; set; }

        double STAT_SYSKONTEXT_SPANNUNG_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_SPANNUNG_MAX_WERT { get; set; }

        double STAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERT { get; set; }

        double STAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERT { get; set; }

        long STAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERT { get; set; }

        int STAT_SYSKONTEXT_WEGSTRECKE_METER_WERT { get; set; }

        byte STAT_SYSKONTEXT_WEGSTRECKE_INSYNC_WERT { get; set; }

        double STAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERT { get; set; }

        double STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERT { get; set; }

        byte STAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERT { get; set; }

        double STAT_SYSKONTEXT_SPANNUNG2_MIN_WERT { get; set; }

        double STAT_SYSKONTEXT_SPANNUNG2_MAX_WERT { get; set; }

        byte STAT_SYSKONTEXT_BASIS_TN_WERT { get; set; }

        ulong STAT_SYSKONTEXT_FUNKT_TN_WERT { get; set; }

        byte STAT_SYSKONTEXT_PWF_BEI_FEHLER_WERT { get; set; }

        byte STAT_SYSKONTEXT_SCHLSLPRFL_AKT_WERT { get; set; }

        double STAT_SYSKONTEXT_LAENGSBESCHLEUNIGUNG_WERT { get; set; }

        byte STAT_SYSKONTEXT_PWF_VOR_PWF_BEI_FEHLER_WERT { get; set; }

        long STAT_SYSKONTEXT_ZEIT_LETZTER_PWF_WECHSEL_WERT { get; set; }

        double STAT_SYSKONTEXT_SPANNUNG_HV_SYSTEM_WERT { get; set; }

        string STAT_ZFS_KOMPLEX { get; set; }

        string JOB_STATUS { get; set; }
    }
}
