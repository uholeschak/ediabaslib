using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class typeCBSInfo : INotifyPropertyChanged, ICbsInfo
    {
        private typeCBSVersion versionField;

        private typeCBSMeaurementType typeField;

        private bool? mMIAnnouncementField;

        private short? mANIP_CBSField;

        private short? aVAI_CBS_WERTField;

        private string aVAI_CBS_EINHField;

        private short? rMMI_CBS_WERTField;

        private string rMMI_CBS_EINHField;

        private short? fRC_INTM_WAY_CBS_MESSField;

        private string fRC_INTM_WAY_CBS_EINHField;

        private short? fRC_INTM_T_CBS_MESSField;

        private DateTime? zIELField;

        private short? iD_FN_CBS_MESS_WERTField;

        private string iD_FN_CBS_MESS_TEXTField;

        private string sT_UN_CBS_HEXField;

        private string sT_UN_CBS_TEXTField;

        private short? sT_UN_CBS_WERTField;

        private string sTATUS_MESSUNG_TEXTField;

        private short? sTATUS_MESSUNGField;

        private string cOU_RSTG_CBS_MESS_EINHField;

        private short? cOU_RSTG_CBS_MESS_WERTField;

        public typeCBSVersion Version
        {
            get
            {
                return versionField;
            }
            set
            {
                if (!versionField.Equals(value))
                {
                    versionField = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        public typeCBSMeaurementType Type
        {
            get
            {
                return typeField;
            }
            set
            {
                if (!typeField.Equals(value))
                {
                    typeField = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public bool? MMIAnnouncement
        {
            get
            {
                return mMIAnnouncementField;
            }
            set
            {
                if (mMIAnnouncementField.HasValue)
                {
                    if (!mMIAnnouncementField.Equals(value))
                    {
                        mMIAnnouncementField = value;
                        OnPropertyChanged("MMIAnnouncement");
                    }
                }
                else
                {
                    mMIAnnouncementField = value;
                    OnPropertyChanged("MMIAnnouncement");
                }
            }
        }

        public short? MANIP_CBS
        {
            get
            {
                return mANIP_CBSField;
            }
            set
            {
                if (mANIP_CBSField.HasValue)
                {
                    if (!mANIP_CBSField.Equals(value))
                    {
                        mANIP_CBSField = value;
                        OnPropertyChanged("MANIP_CBS");
                    }
                }
                else
                {
                    mANIP_CBSField = value;
                    OnPropertyChanged("MANIP_CBS");
                }
            }
        }

        public short? AVAI_CBS_WERT
        {
            get
            {
                return aVAI_CBS_WERTField;
            }
            set
            {
                if (aVAI_CBS_WERTField.HasValue)
                {
                    if (!aVAI_CBS_WERTField.Equals(value))
                    {
                        aVAI_CBS_WERTField = value;
                        OnPropertyChanged("AVAI_CBS_WERT");
                    }
                }
                else
                {
                    aVAI_CBS_WERTField = value;
                    OnPropertyChanged("AVAI_CBS_WERT");
                }
            }
        }

        public string AVAI_CBS_EINH
        {
            get
            {
                return aVAI_CBS_EINHField;
            }
            set
            {
                if (aVAI_CBS_EINHField != null)
                {
                    if (!aVAI_CBS_EINHField.Equals(value))
                    {
                        aVAI_CBS_EINHField = value;
                        OnPropertyChanged("AVAI_CBS_EINH");
                    }
                }
                else
                {
                    aVAI_CBS_EINHField = value;
                    OnPropertyChanged("AVAI_CBS_EINH");
                }
            }
        }

        public short? RMMI_CBS_WERT
        {
            get
            {
                return rMMI_CBS_WERTField;
            }
            set
            {
                if (rMMI_CBS_WERTField.HasValue)
                {
                    if (!rMMI_CBS_WERTField.Equals(value))
                    {
                        rMMI_CBS_WERTField = value;
                        OnPropertyChanged("RMMI_CBS_WERT");
                    }
                }
                else
                {
                    rMMI_CBS_WERTField = value;
                    OnPropertyChanged("RMMI_CBS_WERT");
                }
            }
        }

        public string RMMI_CBS_EINH
        {
            get
            {
                return rMMI_CBS_EINHField;
            }
            set
            {
                if (rMMI_CBS_EINHField != null)
                {
                    if (!rMMI_CBS_EINHField.Equals(value))
                    {
                        rMMI_CBS_EINHField = value;
                        OnPropertyChanged("RMMI_CBS_EINH");
                    }
                }
                else
                {
                    rMMI_CBS_EINHField = value;
                    OnPropertyChanged("RMMI_CBS_EINH");
                }
            }
        }

        public short? FRC_INTM_WAY_CBS_MESS
        {
            get
            {
                return fRC_INTM_WAY_CBS_MESSField;
            }
            set
            {
                if (fRC_INTM_WAY_CBS_MESSField.HasValue)
                {
                    if (!fRC_INTM_WAY_CBS_MESSField.Equals(value))
                    {
                        fRC_INTM_WAY_CBS_MESSField = value;
                        OnPropertyChanged("FRC_INTM_WAY_CBS_MESS");
                    }
                }
                else
                {
                    fRC_INTM_WAY_CBS_MESSField = value;
                    OnPropertyChanged("FRC_INTM_WAY_CBS_MESS");
                }
            }
        }

        public string FRC_INTM_WAY_CBS_EINH
        {
            get
            {
                return fRC_INTM_WAY_CBS_EINHField;
            }
            set
            {
                if (fRC_INTM_WAY_CBS_EINHField != null)
                {
                    if (!fRC_INTM_WAY_CBS_EINHField.Equals(value))
                    {
                        fRC_INTM_WAY_CBS_EINHField = value;
                        OnPropertyChanged("FRC_INTM_WAY_CBS_EINH");
                    }
                }
                else
                {
                    fRC_INTM_WAY_CBS_EINHField = value;
                    OnPropertyChanged("FRC_INTM_WAY_CBS_EINH");
                }
            }
        }

        public short? FRC_INTM_T_CBS_MESS
        {
            get
            {
                return fRC_INTM_T_CBS_MESSField;
            }
            set
            {
                if (fRC_INTM_T_CBS_MESSField.HasValue)
                {
                    if (!fRC_INTM_T_CBS_MESSField.Equals(value))
                    {
                        fRC_INTM_T_CBS_MESSField = value;
                        OnPropertyChanged("FRC_INTM_T_CBS_MESS");
                    }
                }
                else
                {
                    fRC_INTM_T_CBS_MESSField = value;
                    OnPropertyChanged("FRC_INTM_T_CBS_MESS");
                }
            }
        }

        public DateTime? ZIEL
        {
            get
            {
                return zIELField;
            }
            set
            {
                if (zIELField.HasValue)
                {
                    if (!zIELField.Equals(value))
                    {
                        zIELField = value;
                        OnPropertyChanged("ZIEL");
                    }
                }
                else
                {
                    zIELField = value;
                    OnPropertyChanged("ZIEL");
                }
            }
        }

        public short? ID_FN_CBS_MESS_WERT
        {
            get
            {
                return iD_FN_CBS_MESS_WERTField;
            }
            set
            {
                if (iD_FN_CBS_MESS_WERTField.HasValue)
                {
                    if (!iD_FN_CBS_MESS_WERTField.Equals(value))
                    {
                        iD_FN_CBS_MESS_WERTField = value;
                        OnPropertyChanged("ID_FN_CBS_MESS_WERT");
                    }
                }
                else
                {
                    iD_FN_CBS_MESS_WERTField = value;
                    OnPropertyChanged("ID_FN_CBS_MESS_WERT");
                }
            }
        }

        public string ID_FN_CBS_MESS_TEXT
        {
            get
            {
                return iD_FN_CBS_MESS_TEXTField;
            }
            set
            {
                if (iD_FN_CBS_MESS_TEXTField != null)
                {
                    if (!iD_FN_CBS_MESS_TEXTField.Equals(value))
                    {
                        iD_FN_CBS_MESS_TEXTField = value;
                        OnPropertyChanged("ID_FN_CBS_MESS_TEXT");
                    }
                }
                else
                {
                    iD_FN_CBS_MESS_TEXTField = value;
                    OnPropertyChanged("ID_FN_CBS_MESS_TEXT");
                }
            }
        }

        public string ST_UN_CBS_HEX
        {
            get
            {
                return sT_UN_CBS_HEXField;
            }
            set
            {
                if (sT_UN_CBS_HEXField != null)
                {
                    if (!sT_UN_CBS_HEXField.Equals(value))
                    {
                        sT_UN_CBS_HEXField = value;
                        OnPropertyChanged("ST_UN_CBS_HEX");
                    }
                }
                else
                {
                    sT_UN_CBS_HEXField = value;
                    OnPropertyChanged("ST_UN_CBS_HEX");
                }
            }
        }

        public string ST_UN_CBS_TEXT
        {
            get
            {
                return sT_UN_CBS_TEXTField;
            }
            set
            {
                if (sT_UN_CBS_TEXTField != null)
                {
                    if (!sT_UN_CBS_TEXTField.Equals(value))
                    {
                        sT_UN_CBS_TEXTField = value;
                        OnPropertyChanged("ST_UN_CBS_TEXT");
                    }
                }
                else
                {
                    sT_UN_CBS_TEXTField = value;
                    OnPropertyChanged("ST_UN_CBS_TEXT");
                }
            }
        }

        public short? ST_UN_CBS_WERT
        {
            get
            {
                return sT_UN_CBS_WERTField;
            }
            set
            {
                if (sT_UN_CBS_WERTField.HasValue)
                {
                    if (!sT_UN_CBS_WERTField.Equals(value))
                    {
                        sT_UN_CBS_WERTField = value;
                        OnPropertyChanged("ST_UN_CBS_WERT");
                    }
                }
                else
                {
                    sT_UN_CBS_WERTField = value;
                    OnPropertyChanged("ST_UN_CBS_WERT");
                }
            }
        }

        public string STATUS_MESSUNG_TEXT
        {
            get
            {
                return sTATUS_MESSUNG_TEXTField;
            }
            set
            {
                if (sTATUS_MESSUNG_TEXTField != null)
                {
                    if (!sTATUS_MESSUNG_TEXTField.Equals(value))
                    {
                        sTATUS_MESSUNG_TEXTField = value;
                        OnPropertyChanged("STATUS_MESSUNG_TEXT");
                    }
                }
                else
                {
                    sTATUS_MESSUNG_TEXTField = value;
                    OnPropertyChanged("STATUS_MESSUNG_TEXT");
                }
            }
        }

        public short? STATUS_MESSUNG
        {
            get
            {
                return sTATUS_MESSUNGField;
            }
            set
            {
                if (sTATUS_MESSUNGField.HasValue)
                {
                    if (!sTATUS_MESSUNGField.Equals(value))
                    {
                        sTATUS_MESSUNGField = value;
                        OnPropertyChanged("STATUS_MESSUNG");
                    }
                }
                else
                {
                    sTATUS_MESSUNGField = value;
                    OnPropertyChanged("STATUS_MESSUNG");
                }
            }
        }

        public string COU_RSTG_CBS_MESS_EINH
        {
            get
            {
                return cOU_RSTG_CBS_MESS_EINHField;
            }
            set
            {
                if (cOU_RSTG_CBS_MESS_EINHField != null)
                {
                    if (!cOU_RSTG_CBS_MESS_EINHField.Equals(value))
                    {
                        cOU_RSTG_CBS_MESS_EINHField = value;
                        OnPropertyChanged("COU_RSTG_CBS_MESS_EINH");
                    }
                }
                else
                {
                    cOU_RSTG_CBS_MESS_EINHField = value;
                    OnPropertyChanged("COU_RSTG_CBS_MESS_EINH");
                }
            }
        }

        public short? COU_RSTG_CBS_MESS_WERT
        {
            get
            {
                return cOU_RSTG_CBS_MESS_WERTField;
            }
            set
            {
                if (cOU_RSTG_CBS_MESS_WERTField.HasValue)
                {
                    if (!cOU_RSTG_CBS_MESS_WERTField.Equals(value))
                    {
                        cOU_RSTG_CBS_MESS_WERTField = value;
                        OnPropertyChanged("COU_RSTG_CBS_MESS_WERT");
                    }
                }
                else
                {
                    cOU_RSTG_CBS_MESS_WERTField = value;
                    OnPropertyChanged("COU_RSTG_CBS_MESS_WERT");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public typeCBSInfo()
        {
            versionField = typeCBSVersion.UNKNOWN;
            typeField = typeCBSMeaurementType.Unknown;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
