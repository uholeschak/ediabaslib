using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IZfsResult : INotifyPropertyChanged
    {
        ushort Index { get; }

        long? STAT_DM_ADRESSE_SG { get; }

        long? STAT_DM_MELDUNG_NR { get; }

        string STAT_DM_MELDUNG_TEXT { get; }

        short? STAT_DM_MELDUNG_TYP { get; }

        long? STAT_DM_SGBD_INDEX { get; }

        ulong? STAT_DM_ZEITSTEMPEL { get; }

        ulong? STAT_DM_ZEITSTEMPEL_MS { get; }

        double? STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERT { get; }

        double? STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERT { get; }

        short? STAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERT { get; }

        double? STAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERT { get; }

        double? STAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERT { get; }

        short? STAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERT { get; }

        short? STAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERT { get; }

        DateTime? STAT_SYSKONTEXT_KUNDENZEIT { get; }

        short? STAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERT { get; }

        short? STAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERT { get; }

        double? STAT_SYSKONTEXT_SPANNUNG_MAX_WERT { get; }

        double? STAT_SYSKONTEXT_SPANNUNG_MIN_WERT { get; }

        double? STAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERT { get; }

        double? STAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERT { get; }

        int? STAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERT { get; }

        int? STAT_SYSKONTEXT_WEGSTRECKE_METER_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEITSTEMPEL_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERT { get; }

        ulong? STAT_SYSKONTEXT_ZEIT_WECKEN_WERT { get; }

        string STAT_ZFS_KOMPLEX { get; }

        short? STAT_DM_MESSAGE_TYPE { get; }

        string JOB_STATUS { get; }

        byte STAT_DM_ACTIVE_STATE { get; }

        byte STAT_DM_MAPPING_ID { get; }

        string STAT_DM_MESSAGE_TYPE_TEXT { get; }

        string STAT_DM_TS_1AS { get; }

        string STAT_DM_TS_1AS_MS { get; }

        byte STAT_SYSKONTEXT_BASIS_TN_WERT { get; }

        ulong STAT_SYSKONTEXT_FUNKT_TN_WERT { get; }

        int STAT_SYSKONTEXT_KUNDENZEIT_JAHR_WERT { get; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_MONAT_WERT { get; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_TAG_WERT { get; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_STUNDE_WERT { get; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_MINUTE_WERT { get; }

        byte STAT_SYSKONTEXT_KUNDENZEIT_SEKUNDE_WERT { get; }

        double STAT_SYSKONTEXT_LAENGSBESCHLEUNIGUNG_WERT { get; }

        byte STAT_SYSKONTEXT_PWF_BEI_FEHLER_WERT { get; }

        byte STAT_SYSKONTEXT_PWF_VOR_PWF_BEI_FEHLER_WERT { get; }

        byte STAT_SYSKONTEXT_SCHLSLPRFL_AKT_WERT { get; }

        double STAT_SYSKONTEXT_SPANNUNG2_MAX_WERT { get; }

        double STAT_SYSKONTEXT_SPANNUNG2_MIN_WERT { get; }

        double STAT_SYSKONTEXT_SPANNUNG_HV_SYSTEM_WERT { get; }

        byte STAT_SYSKONTEXT_WEGSTRECKE_INSYNC_WERT { get; }

        long STAT_SYSKONTEXT_ZEIT_LETZTER_PWF_WECHSEL_WERT { get; }
    }
}