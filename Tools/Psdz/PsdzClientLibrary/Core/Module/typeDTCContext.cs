using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class typeDTCContext : IDtcContext, INotifyPropertyChanged
    {
        private long? f_UW_KMField;
        private double? f_UW_KM_SUPREMEField;
        private double? f_UW_ZEIT_SUPREMEField;
        private long? f_UW_ZEITField;
        private int? f_UW_ANZField;
        private ObservableCollection<F_UW> f_UWField;
        [XmlIgnore]
        IEnumerable<IDtcUmwelt> IDtcContext.F_UW => F_UW;

        [XmlIgnore]
        public double Mileage
        {
            get
            {
                if (F_UW_KM_SUPREME.HasValue)
                {
                    return F_UW_KM_SUPREME.Value;
                }

                if (F_UW_KM.HasValue)
                {
                    return F_UW_KM.Value;
                }

                return -1.0;
            }
        }

        [XmlIgnore]
        public Guid UniqueId { get; set; }

        public long? F_UW_KM
        {
            get
            {
                return f_UW_KMField;
            }

            set
            {
                if (f_UW_KMField.HasValue)
                {
                    if (!f_UW_KMField.Equals(value))
                    {
                        f_UW_KMField = value;
                        OnPropertyChanged("F_UW_KM");
                    }
                }
                else
                {
                    f_UW_KMField = value;
                    OnPropertyChanged("F_UW_KM");
                }
            }
        }

        public double? F_UW_KM_SUPREME
        {
            get
            {
                return f_UW_KM_SUPREMEField;
            }

            set
            {
                if (f_UW_KM_SUPREMEField.HasValue)
                {
                    if (!f_UW_KM_SUPREMEField.Equals(value))
                    {
                        f_UW_KM_SUPREMEField = value;
                        OnPropertyChanged("F_UW_KM_SUPREME");
                    }
                }
                else
                {
                    f_UW_KM_SUPREMEField = value;
                    OnPropertyChanged("F_UW_KM_SUPREME");
                }
            }
        }

        public long? F_UW_ZEIT
        {
            get
            {
                return f_UW_ZEITField;
            }

            set
            {
                if (f_UW_ZEITField.HasValue)
                {
                    if (!f_UW_ZEITField.Equals(value))
                    {
                        f_UW_ZEITField = value;
                        OnPropertyChanged("F_UW_ZEIT");
                    }
                }
                else
                {
                    f_UW_ZEITField = value;
                    OnPropertyChanged("F_UW_ZEIT");
                }
            }
        }

        public double? F_UW_ZEIT_SUPREME
        {
            get
            {
                return f_UW_ZEIT_SUPREMEField;
            }

            set
            {
                if (f_UW_ZEIT_SUPREMEField.HasValue)
                {
                    if (!f_UW_ZEIT_SUPREMEField.Equals(value))
                    {
                        f_UW_ZEIT_SUPREMEField = value;
                        OnPropertyChanged("F_UW_ZEIT_SUPREME");
                    }
                }
                else
                {
                    f_UW_ZEIT_SUPREMEField = value;
                    OnPropertyChanged("F_UW_ZEIT_SUPREME");
                }
            }
        }

        public int? F_UW_ANZ
        {
            get
            {
                return f_UW_ANZField;
            }

            set
            {
                if (f_UW_ANZField.HasValue)
                {
                    if (!f_UW_ANZField.Equals(value))
                    {
                        f_UW_ANZField = value;
                        OnPropertyChanged("F_UW_ANZ");
                    }
                }
                else
                {
                    f_UW_ANZField = value;
                    OnPropertyChanged("F_UW_ANZ");
                }
            }
        }

        public ObservableCollection<F_UW> F_UW
        {
            get
            {
                return f_UWField;
            }

            set
            {
                if (f_UWField != null)
                {
                    if (!f_UWField.Equals(value))
                    {
                        f_UWField = value;
                        OnPropertyChanged("F_UW");
                    }
                }
                else
                {
                    f_UWField = value;
                    OnPropertyChanged("F_UW");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void SetCurrentMileage(Vehicle vec)
        {
            if (vec.Gwsz.HasValue)
            {
                decimal value = vec.Gwsz.Value;
                F_UW_KM_SUPREME = decimal.ToDouble(value);
                F_UW_KM = decimal.ToInt64(value);
            }
            else
            {
                F_UW_KM_SUPREME = -1.0;
                F_UW_KM = -1L;
            }
        }

        public void SetCurrentTimestamp(Vehicle vec)
        {
            double totalSeconds = (DateTime.Now - vec.VehicleLifeStartDate).TotalSeconds;
            F_UW_ZEIT_SUPREME = totalSeconds;
            F_UW_ZEIT = (long)totalSeconds;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.Append("typeDTCContext: ");
                if (F_UW != null)
                {
                    stringBuilder.AppendFormat("F_UW.Count: {0}, ", F_UW.Count);
                }
                else
                {
                    stringBuilder.Append("F_UW.Count: null, ");
                }

                if (F_UW_ANZ.HasValue)
                {
                    stringBuilder.AppendFormat("F_UW_ANZ: {0}, ", F_UW_ANZ);
                }
                else
                {
                    stringBuilder.Append("F_UW_ANZ: null, ");
                }

                if (F_UW_KM.HasValue)
                {
                    stringBuilder.AppendFormat("F_UW_KM: {0}, ", F_UW_KM);
                }
                else
                {
                    stringBuilder.Append("F_UW_KM: null, ");
                }

                if (F_UW_KM_SUPREME.HasValue)
                {
                    stringBuilder.AppendFormat("F_UW_KM_SUPREME: {0}, ", F_UW_KM_SUPREME);
                }
                else
                {
                    stringBuilder.Append("F_UW_KM_SUPREME: null, ");
                }

                if (F_UW_ZEIT.HasValue)
                {
                    stringBuilder.AppendFormat("F_UW_ZEIT: {0}", F_UW_ZEIT);
                }
                else
                {
                    stringBuilder.Append("F_UW_ZEIT: null ");
                }

                if (F_UW_ZEIT_SUPREME.HasValue)
                {
                    stringBuilder.AppendFormat("F_UW_ZEIT_SUPREME: {0}", F_UW_ZEIT_SUPREME);
                }
                else
                {
                    stringBuilder.Append("F_UW_ZEIT_SUPREME: null ");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("typeDTCContext.ToString()", exception);
            }

            return stringBuilder.ToString();
        }

        public typeDTCContext()
        {
            UniqueId = Guid.NewGuid();
            f_UWField = new ObservableCollection<F_UW>();
            f_UW_ZEITField = -1L;
            f_UW_ANZField = 0;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}