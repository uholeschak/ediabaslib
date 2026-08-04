using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [DataContract]
    public class ProgrammingNote : INotifyPropertyChanged
    {
        [DataMember]
        private string msg = string.Empty;

        [DataMember]
        private bool visible;

        [DataMember]
        private string hidden;

        public string Message
        {
            get
            {
                return msg;
            }
            set
            {
                msg = value;
                OnPropertyChanged("Message");
            }
        }

        public bool IsVisible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public string Hidden
        {
            get
            {
                return hidden;
            }
            set
            {
                hidden = value;
                OnPropertyChanged("Hidden");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
