using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}