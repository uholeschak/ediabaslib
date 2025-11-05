using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public class typeSWTStatus : INotifyPropertyChanged, ISwtStatus
    {
        private string sTAT_SW_IDField;
        private string titleField;
        private string sTAT_ROOT_CERT_STATUS_CODEField;
        private string sTAT_SIGS_CERT_STATUS_CODEField;
        private string sTAT_SW_SIG_STATUS_CODEField;
        private string sTAT_FSCS_CERT_STATUS_CODEField;
        private string sTAT_FSC_STATUS_CODEField;
        private string orderingStatusField;
        public string STAT_SW_ID
        {
            get
            {
                return sTAT_SW_IDField;
            }

            set
            {
                if (sTAT_SW_IDField != null)
                {
                    if (!sTAT_SW_IDField.Equals(value))
                    {
                        sTAT_SW_IDField = value;
                        OnPropertyChanged("STAT_SW_ID");
                    }
                }
                else
                {
                    sTAT_SW_IDField = value;
                    OnPropertyChanged("STAT_SW_ID");
                }
            }
        }

        public string Title
        {
            get
            {
                return titleField;
            }

            set
            {
                if (titleField != null)
                {
                    if (!titleField.Equals(value))
                    {
                        titleField = value;
                        OnPropertyChanged("Title");
                    }
                }
                else
                {
                    titleField = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public string STAT_ROOT_CERT_STATUS_CODE
        {
            get
            {
                return sTAT_ROOT_CERT_STATUS_CODEField;
            }

            set
            {
                if (sTAT_ROOT_CERT_STATUS_CODEField != null)
                {
                    if (!sTAT_ROOT_CERT_STATUS_CODEField.Equals(value))
                    {
                        sTAT_ROOT_CERT_STATUS_CODEField = value;
                        OnPropertyChanged("STAT_ROOT_CERT_STATUS_CODE");
                    }
                }
                else
                {
                    sTAT_ROOT_CERT_STATUS_CODEField = value;
                    OnPropertyChanged("STAT_ROOT_CERT_STATUS_CODE");
                }
            }
        }

        public string STAT_SIGS_CERT_STATUS_CODE
        {
            get
            {
                return sTAT_SIGS_CERT_STATUS_CODEField;
            }

            set
            {
                if (sTAT_SIGS_CERT_STATUS_CODEField != null)
                {
                    if (!sTAT_SIGS_CERT_STATUS_CODEField.Equals(value))
                    {
                        sTAT_SIGS_CERT_STATUS_CODEField = value;
                        OnPropertyChanged("STAT_SIGS_CERT_STATUS_CODE");
                    }
                }
                else
                {
                    sTAT_SIGS_CERT_STATUS_CODEField = value;
                    OnPropertyChanged("STAT_SIGS_CERT_STATUS_CODE");
                }
            }
        }

        public string STAT_SW_SIG_STATUS_CODE
        {
            get
            {
                return sTAT_SW_SIG_STATUS_CODEField;
            }

            set
            {
                if (sTAT_SW_SIG_STATUS_CODEField != null)
                {
                    if (!sTAT_SW_SIG_STATUS_CODEField.Equals(value))
                    {
                        sTAT_SW_SIG_STATUS_CODEField = value;
                        OnPropertyChanged("STAT_SW_SIG_STATUS_CODE");
                    }
                }
                else
                {
                    sTAT_SW_SIG_STATUS_CODEField = value;
                    OnPropertyChanged("STAT_SW_SIG_STATUS_CODE");
                }
            }
        }

        public string STAT_FSCS_CERT_STATUS_CODE
        {
            get
            {
                return sTAT_FSCS_CERT_STATUS_CODEField;
            }

            set
            {
                if (sTAT_FSCS_CERT_STATUS_CODEField != null)
                {
                    if (!sTAT_FSCS_CERT_STATUS_CODEField.Equals(value))
                    {
                        sTAT_FSCS_CERT_STATUS_CODEField = value;
                        OnPropertyChanged("STAT_FSCS_CERT_STATUS_CODE");
                    }
                }
                else
                {
                    sTAT_FSCS_CERT_STATUS_CODEField = value;
                    OnPropertyChanged("STAT_FSCS_CERT_STATUS_CODE");
                }
            }
        }

        public string STAT_FSC_STATUS_CODE
        {
            get
            {
                return sTAT_FSC_STATUS_CODEField;
            }

            set
            {
                if (sTAT_FSC_STATUS_CODEField != null)
                {
                    if (!sTAT_FSC_STATUS_CODEField.Equals(value))
                    {
                        sTAT_FSC_STATUS_CODEField = value;
                        OnPropertyChanged("STAT_FSC_STATUS_CODE");
                    }
                }
                else
                {
                    sTAT_FSC_STATUS_CODEField = value;
                    OnPropertyChanged("STAT_FSC_STATUS_CODE");
                }
            }
        }

        public string OrderingStatus
        {
            get
            {
                return orderingStatusField;
            }

            set
            {
                if (orderingStatusField != null)
                {
                    if (!orderingStatusField.Equals(value))
                    {
                        orderingStatusField = value;
                        OnPropertyChanged("OrderingStatus");
                    }
                }
                else
                {
                    orderingStatusField = value;
                    OnPropertyChanged("OrderingStatus");
                }
            }
        }

        public uint applicationNoUI
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(STAT_SW_ID) && STAT_SW_ID.Length >= 8)
                    {
                        return Convert.ToUInt32(STAT_SW_ID.Substring(0, 4), 16);
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("swIdType.get_applicationNoUI()", exception);
                }

                return 0u;
            }
        }

        public uint upgradeIndexUI
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(STAT_SW_ID) && STAT_SW_ID.Length >= 8)
                    {
                        return Convert.ToUInt32(STAT_SW_ID.Substring(4, 4), 16);
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("swIdType.get_upgradeIndexUI()", exception);
                }

                return 0u;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}