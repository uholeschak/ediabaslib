using System;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class ZFSResult : INotifyPropertyChanged, IZfsResult
    {
        private ushort indexField;
        private long? sTAT_DM_ADRESSE_SGField;
        private string sTAT_ZFS_KOMPLEXField;
        private long? sTAT_DM_MELDUNG_NRField;
        private long? sTAT_DM_SGBD_INDEXField;
        private short? sTAT_DM_MELDUNG_TYPField;
        private ulong? sTAT_DM_ZEITSTEMPELField;
        private ulong? sTAT_DM_ZEITSTEMPEL_MSField;
        private string sTAT_DM_MELDUNG_TEXTField;
        private ulong? sTAT_SYSKONTEXT_ZEIT_WECKEN_WERTField;
        private ulong? sTAT_SYSKONTEXT_ZEITSTEMPEL_WERTField;
        private double? sTAT_SYSKONTEXT_SPANNUNG_MAX_WERTField;
        private double? sTAT_SYSKONTEXT_SPANNUNG_MIN_WERTField;
        private DateTime? sTAT_SYSKONTEXT_KUNDENZEITField;
        private double? sTAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERTField;
        private short? sTAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERTField;
        private short? sTAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERTField;
        private double? sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERTField;
        private double? sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERTField;
        private short? sTAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERTField;
        private short? sTAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERTField;
        private ulong? sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERTField;
        private ulong? sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERTField;
        private ulong? sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERTField;
        private ulong? sTAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERTField;
        private int? sTAT_SYSKONTEXT_WEGSTRECKE_METER_WERTField;
        private int? sTAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERTField;
        private ulong? sTAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERTField;
        private double? sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERTField;
        private double? sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERTField;
        private double? sTAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERTField;
        private short? sTAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERTField;
        private short? sTAT_DM_MESSAGE_TYPEField;
        private int[] checkControlMessageTypes = new int[3]
        {
            2,
            3,
            255
        };
        public ushort Index
        {
            get
            {
                return indexField;
            }

            set
            {
                _ = indexField;
                if (!indexField.Equals(value))
                {
                    indexField = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        public long? STAT_DM_ADRESSE_SG
        {
            get
            {
                return sTAT_DM_ADRESSE_SGField;
            }

            set
            {
                if (sTAT_DM_ADRESSE_SGField.HasValue)
                {
                    if (!sTAT_DM_ADRESSE_SGField.Equals(value))
                    {
                        sTAT_DM_ADRESSE_SGField = value;
                        OnPropertyChanged("STAT_DM_ADRESSE_SG");
                    }
                }
                else
                {
                    sTAT_DM_ADRESSE_SGField = value;
                    OnPropertyChanged("STAT_DM_ADRESSE_SG");
                }
            }
        }

        public string STAT_ZFS_KOMPLEX
        {
            get
            {
                return sTAT_ZFS_KOMPLEXField;
            }

            set
            {
                if (sTAT_ZFS_KOMPLEXField != null)
                {
                    if (!sTAT_ZFS_KOMPLEXField.Equals(value))
                    {
                        sTAT_ZFS_KOMPLEXField = value;
                        OnPropertyChanged("STAT_ZFS_KOMPLEX");
                    }
                }
                else
                {
                    sTAT_ZFS_KOMPLEXField = value;
                    OnPropertyChanged("STAT_ZFS_KOMPLEX");
                }
            }
        }

        public long? STAT_DM_MELDUNG_NR
        {
            get
            {
                return sTAT_DM_MELDUNG_NRField;
            }

            set
            {
                if (sTAT_DM_MELDUNG_NRField.HasValue)
                {
                    if (!sTAT_DM_MELDUNG_NRField.Equals(value))
                    {
                        sTAT_DM_MELDUNG_NRField = value;
                        OnPropertyChanged("STAT_DM_MELDUNG_NR");
                    }
                }
                else
                {
                    sTAT_DM_MELDUNG_NRField = value;
                    OnPropertyChanged("STAT_DM_MELDUNG_NR");
                }
            }
        }

        public long? STAT_DM_SGBD_INDEX
        {
            get
            {
                return sTAT_DM_SGBD_INDEXField;
            }

            set
            {
                if (sTAT_DM_SGBD_INDEXField.HasValue)
                {
                    if (!sTAT_DM_SGBD_INDEXField.Equals(value))
                    {
                        sTAT_DM_SGBD_INDEXField = value;
                        OnPropertyChanged("STAT_DM_SGBD_INDEX");
                    }
                }
                else
                {
                    sTAT_DM_SGBD_INDEXField = value;
                    OnPropertyChanged("STAT_DM_SGBD_INDEX");
                }
            }
        }

        public short? STAT_DM_MELDUNG_TYP
        {
            get
            {
                return sTAT_DM_MELDUNG_TYPField;
            }

            set
            {
                if (sTAT_DM_MELDUNG_TYPField.HasValue)
                {
                    if (!sTAT_DM_MELDUNG_TYPField.Equals(value))
                    {
                        sTAT_DM_MELDUNG_TYPField = value;
                        OnPropertyChanged("STAT_DM_MELDUNG_TYP");
                    }
                }
                else
                {
                    sTAT_DM_MELDUNG_TYPField = value;
                    OnPropertyChanged("STAT_DM_MELDUNG_TYP");
                }
            }
        }

        public ulong? STAT_DM_ZEITSTEMPEL
        {
            get
            {
                return sTAT_DM_ZEITSTEMPELField;
            }

            set
            {
                if (sTAT_DM_ZEITSTEMPELField.HasValue)
                {
                    if (!sTAT_DM_ZEITSTEMPELField.Equals(value))
                    {
                        sTAT_DM_ZEITSTEMPELField = value;
                        OnPropertyChanged("STAT_DM_ZEITSTEMPEL");
                    }
                }
                else
                {
                    sTAT_DM_ZEITSTEMPELField = value;
                    OnPropertyChanged("STAT_DM_ZEITSTEMPEL");
                }
            }
        }

        public ulong? STAT_DM_ZEITSTEMPEL_MS
        {
            get
            {
                return sTAT_DM_ZEITSTEMPEL_MSField;
            }

            set
            {
                if (sTAT_DM_ZEITSTEMPEL_MSField != value)
                {
                    sTAT_DM_ZEITSTEMPEL_MSField = value;
                    OnPropertyChanged("STAT_DM_ZEITSTEMPEL_MS");
                }
            }
        }

        public string STAT_DM_MELDUNG_TEXT
        {
            get
            {
                return sTAT_DM_MELDUNG_TEXTField;
            }

            set
            {
                if (sTAT_DM_MELDUNG_TEXTField != null)
                {
                    if (!sTAT_DM_MELDUNG_TEXTField.Equals(value))
                    {
                        sTAT_DM_MELDUNG_TEXTField = value;
                        OnPropertyChanged("STAT_DM_MELDUNG_TEXT");
                    }
                }
                else
                {
                    sTAT_DM_MELDUNG_TEXTField = value;
                    OnPropertyChanged("STAT_DM_MELDUNG_TEXT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEIT_WECKEN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEIT_WECKEN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEIT_WECKEN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEIT_WECKEN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEIT_WECKEN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_WECKEN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEIT_WECKEN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_WECKEN_WERT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEITSTEMPEL_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEITSTEMPEL_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEITSTEMPEL_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEITSTEMPEL_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEITSTEMPEL_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEITSTEMPEL_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEITSTEMPEL_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEITSTEMPEL_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_SPANNUNG_MAX_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_SPANNUNG_MAX_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_SPANNUNG_MAX_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_SPANNUNG_MAX_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_SPANNUNG_MAX_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_SPANNUNG_MAX_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_SPANNUNG_MAX_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_SPANNUNG_MAX_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_SPANNUNG_MIN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_SPANNUNG_MIN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_SPANNUNG_MIN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_SPANNUNG_MIN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_SPANNUNG_MIN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_SPANNUNG_MIN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_SPANNUNG_MIN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_SPANNUNG_MIN_WERT");
                }
            }
        }

        public DateTime? STAT_SYSKONTEXT_KUNDENZEIT
        {
            get
            {
                return sTAT_SYSKONTEXT_KUNDENZEITField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_KUNDENZEITField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_KUNDENZEITField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_KUNDENZEITField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_KUNDENZEIT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_KUNDENZEITField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_KUNDENZEIT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_TEMPERATUR_AUSSEN_WERT");
                }
            }
        }

        public short? STAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_KLEMMEN_BEI_FEHLER_WERT");
                }
            }
        }

        public short? STAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_KLEMMEN_VOR_FEHLER_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_GESCHWINDIGKEIT_MAX_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_GESCHWINDIGKEIT_MIN_WERT");
                }
            }
        }

        public short? STAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_OPSTATUS_BEI_FEHLER_WERT");
                }
            }
        }

        public short? STAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_OPSTATUS_VOR_FEHLER_WERT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_ERSTE_KL_R_EIN_WERT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_ERSTE_KL_15_EIN_WERT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_ERSTE_KL_50_EIN_WERT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_KLEMMENWECHSEL_WERT");
                }
            }
        }

        public int? STAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERT");
                }
            }
        }

        public int? STAT_SYSKONTEXT_WEGSTRECKE_METER_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_WEGSTRECKE_METER_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_WEGSTRECKE_METER_WERTField != value)
                {
                    sTAT_SYSKONTEXT_WEGSTRECKE_METER_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_WEGSTRECKE_METER_WERT");
                }
            }
        }

        public ulong? STAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_ZEIT_OPSTATUSWECHSEL_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MAX_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_DREHZAHL_KURBELWELLE_MIN_WERT");
                }
            }
        }

        public double? STAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_TEMPERATUR_MOTOR_ANTRIEB_WERT");
                }
            }
        }

        public short? STAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERT
        {
            get
            {
                return sTAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERTField;
            }

            set
            {
                if (sTAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERTField.HasValue)
                {
                    if (!sTAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERTField.Equals(value))
                    {
                        sTAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERTField = value;
                        OnPropertyChanged("STAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERT");
                    }
                }
                else
                {
                    sTAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERTField = value;
                    OnPropertyChanged("STAT_SYSKONTEXT_FEHLERSPEICHERSPERRE_AKTIV_WERT");
                }
            }
        }

        public short? STAT_DM_MESSAGE_TYPE
        {
            get
            {
                return sTAT_DM_MESSAGE_TYPEField;
            }

            set
            {
                if (sTAT_DM_MESSAGE_TYPEField != value)
                {
                    sTAT_DM_MESSAGE_TYPEField = value;
                    OnPropertyChanged("STAT_DM_MESSAGE_TYPE");
                    MarkCheckControlMessage();
                    OnPropertyChanged("IsCheckControlMessage");
                }
            }
        }

        public bool IsCheckControlMessage { get; set; }
        public string JOB_STATUS { get; set; }
        public byte STAT_DM_ACTIVE_STATE { get; set; }
        public byte STAT_DM_MAPPING_ID { get; set; }
        public string STAT_DM_MESSAGE_TYPE_TEXT { get; set; }
        public string STAT_DM_TS_1AS { get; set; }
        public string STAT_DM_TS_1AS_MS { get; set; }
        public byte STAT_SYSKONTEXT_BASIS_TN_WERT { get; set; }
        public ulong STAT_SYSKONTEXT_FUNKT_TN_WERT { get; set; }
        public int STAT_SYSKONTEXT_KUNDENZEIT_JAHR_WERT { get; set; }
        public byte STAT_SYSKONTEXT_KUNDENZEIT_MONAT_WERT { get; set; }
        public byte STAT_SYSKONTEXT_KUNDENZEIT_TAG_WERT { get; set; }
        public byte STAT_SYSKONTEXT_KUNDENZEIT_STUNDE_WERT { get; set; }
        public byte STAT_SYSKONTEXT_KUNDENZEIT_MINUTE_WERT { get; set; }
        public byte STAT_SYSKONTEXT_KUNDENZEIT_SEKUNDE_WERT { get; set; }
        public double STAT_SYSKONTEXT_LAENGSBESCHLEUNIGUNG_WERT { get; set; }
        public byte STAT_SYSKONTEXT_PWF_BEI_FEHLER_WERT { get; set; }
        public byte STAT_SYSKONTEXT_PWF_VOR_PWF_BEI_FEHLER_WERT { get; set; }
        public byte STAT_SYSKONTEXT_SCHLSLPRFL_AKT_WERT { get; set; }
        public double STAT_SYSKONTEXT_SPANNUNG2_MAX_WERT { get; set; }
        public double STAT_SYSKONTEXT_SPANNUNG2_MIN_WERT { get; set; }
        public double STAT_SYSKONTEXT_SPANNUNG_HV_SYSTEM_WERT { get; set; }
        public byte STAT_SYSKONTEXT_WEGSTRECKE_INSYNC_WERT { get; set; }
        public long STAT_SYSKONTEXT_ZEIT_LETZTER_PWF_WECHSEL_WERT { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MarkCheckControlMessage()
        {
            IsCheckControlMessage = sTAT_DM_MESSAGE_TYPEField.HasValue && Array.IndexOf(checkControlMessageTypes, sTAT_DM_MESSAGE_TYPEField.Value) != -1;
        }
    }
}