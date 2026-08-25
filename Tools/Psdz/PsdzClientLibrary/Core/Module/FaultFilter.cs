using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BMW.Rheingold.CoreFramework
{
    public class FaultFilter : INotifyPropertyChanged
    {
        private IList<string> faultClassHidden;

        private IList<int> faultGroupNumbers;

        private long? lowerKMBound;

        private long? upperKMBound;

        private List<int> defaultGroups = new List<int> { 1, 2, 3, 4, 5 };

        public IList<string> FaultClassHidden
        {
            get
            {
                return faultClassHidden;
            }
            set
            {
                if (value != faultClassHidden)
                {
                    faultClassHidden = value;
                    OnPropertyChanged("FaultClassHidden");
                }
            }
        }

        public IList<int> FaultGroupNumbers
        {
            get
            {
                return faultGroupNumbers;
            }
            set
            {
                if (value != faultGroupNumbers)
                {
                    faultGroupNumbers = value;
                    OnPropertyChanged("FaultGroupNumbers");
                }
            }
        }

        public bool IsDefault
        {
            get
            {
                if (!upperKMBound.HasValue && !lowerKMBound.HasValue && (faultClassHidden == null || faultClassHidden.Count == 0))
                {
                    return FaultGroupNumbers.SequenceEqual(GetDefaultFaultGroups());
                }
                return false;
            }
        }

        public long? LowerKMBound
        {
            get
            {
                return lowerKMBound;
            }
            set
            {
                if (value != lowerKMBound)
                {
                    lowerKMBound = value;
                    OnPropertyChanged("LowerKMBound");
                }
            }
        }

        public long? UpperKMBound
        {
            get
            {
                return upperKMBound;
            }
            set
            {
                if (value != upperKMBound)
                {
                    upperKMBound = value;
                    OnPropertyChanged("UpperKMBound");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FaultFilter()
        {
            lowerKMBound = null;
            upperKMBound = null;
            faultClassHidden = null;
            faultGroupNumbers = GetDefaultFaultGroups();
        }

        public virtual void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler propertyChangedEventHandler = this.PropertyChanged;
            if (propertyChangedEventHandler != null)
            {
                propertyChangedEventHandler(this, new PropertyChangedEventArgs(info));
                if (!"IsDefault".Equals(info))
                {
                    propertyChangedEventHandler(this, new PropertyChangedEventArgs("IsDefault"));
                }
            }
        }

        public void SetDefault()
        {
            LowerKMBound = null;
            UpperKMBound = null;
            FaultClassHidden = null;
            faultGroupNumbers = GetDefaultFaultGroups();
            OnPropertyChanged("IsDefault");
        }

        public virtual List<int> GetDefaultFaultGroups()
        {
            return defaultGroups;
        }
    }
}
