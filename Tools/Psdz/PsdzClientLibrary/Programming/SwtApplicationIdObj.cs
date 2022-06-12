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
            internal SwtApplicationIdObj(int appNo, int upgradeIdx)
            {
                this.AppNo = appNo;
                this.UpgradeIdx = upgradeIdx;
            }

            public int AppNo
            {
                get
                {
                    return this.appNo;
                }
                private set
                {
                    this.appNo = value;
                    this.OnPropertyChanged("AppNo");
                }
            }

            public int UpgradeIdx
            {
                get
                {
                    return this.upgradeIdx;
                }
                private set
                {
                    this.upgradeIdx = value;
                    this.OnPropertyChanged("UpgradeIdx");
                }
            }

            public override bool Equals(object obj)
            {
                ISwtApplicationId swtApplicationId = obj as ISwtApplicationId;
                return swtApplicationId != null && this.AppNo == swtApplicationId.AppNo && this.UpgradeIdx == swtApplicationId.UpgradeIdx;
            }

            public override int GetHashCode()
            {
                return this.AppNo.GetHashCode() + this.UpgradeIdx.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "SWT-Application-ID[appNo: {0}; upgradeIdx: {1}]", this.AppNo, this.UpgradeIdx);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if (propertyChanged == null)
                {
                    return;
                }
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            [DataMember]
            private int appNo;

            [DataMember]
            private int upgradeIdx;
        }
    }
}
