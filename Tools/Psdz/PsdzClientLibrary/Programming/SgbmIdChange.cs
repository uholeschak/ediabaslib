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
    public class SgbmIdChange : ISgbmIdChange, INotifyPropertyChanged
    {
        [DataMember]
        private string actual;
        [DataMember]
        private string target;
        public string Actual
        {
            get
            {
                return actual;
            }

            set
            {
                actual = value;
                OnPropertyChanged("Actual");
            }
        }

        public string Target
        {
            get
            {
                return target;
            }

            set
            {
                target = value;
                OnPropertyChanged("Target");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SgbmIdChange()
        {
        }

        public SgbmIdChange(string actual, string target)
        {
            Actual = actual;
            Target = target;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}