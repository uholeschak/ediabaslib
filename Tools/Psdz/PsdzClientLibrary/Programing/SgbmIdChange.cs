using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class SgbmIdChange : INotifyPropertyChanged, ISgbmIdChange
    {
        public SgbmIdChange()
        {
        }

        public SgbmIdChange(string actual, string target)
        {
            this.Actual = actual;
            this.Target = target;
        }

        public string Actual
        {
            get
            {
                return this.actual;
            }
            set
            {
                this.actual = value;
                this.OnPropertyChanged("Actual");
            }
        }

        public string Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
                this.OnPropertyChanged("Target");
            }
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
        private string actual;

        [DataMember]
        private string target;
    }
}
