// BMW.Rheingold.Module.ISTA.SingleChoice
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DesignerCategory("code")]
    [DataContract(Name = "SingleChoice")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class SingleChoice : AChoice, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
