using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    namespace BMW.Rheingold.Programming.API
    {
        [DataContract]
        internal class SwtApplicationIdObj : ISwtApplicationId, INotifyPropertyChanged
        {
            [DataMember]
            private int appNo;
            [DataMember]
            private int upgradeIdx;
            public int AppNo
            {
                get
                {
                    return appNo;
                }

                private set
                {
                    appNo = value;
                    OnPropertyChanged("AppNo");
                }
            }

            public int UpgradeIdx
            {
                get
                {
                    return upgradeIdx;
                }

                private set
                {
                    upgradeIdx = value;
                    OnPropertyChanged("UpgradeIdx");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            internal SwtApplicationIdObj(int appNo, int upgradeIdx)
            {
                AppNo = appNo;
                UpgradeIdx = upgradeIdx;
            }

            public override bool Equals(object obj)
            {
                if (obj is ISwtApplicationId swtApplicationId)
                {
                    if (AppNo == swtApplicationId.AppNo)
                    {
                        return UpgradeIdx == swtApplicationId.UpgradeIdx;
                    }

                    return false;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return AppNo.GetHashCode() + UpgradeIdx.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "SWT-Application-ID[appNo: {0}; upgradeIdx: {1}]", AppNo, UpgradeIdx);
            }

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}