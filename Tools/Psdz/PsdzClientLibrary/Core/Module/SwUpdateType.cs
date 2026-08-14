using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BMW.Rheingold.InfoProvider.HDD.HDDLookup
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.3.0.20460")]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "SwUpdateType")]
    public class SwUpdateType : INotifyPropertyChanged
    {
        private string swUpdateEntryField;
        [XmlAttribute]
        [DataMember]
        public string SwUpdateEntry
        {
            get
            {
                return swUpdateEntryField;
            }

            set
            {
                if (swUpdateEntryField != null)
                {
                    if (!swUpdateEntryField.Equals(value))
                    {
                        swUpdateEntryField = value;
                        OnPropertyChanged("SwUpdateEntry");
                    }
                }
                else
                {
                    swUpdateEntryField = value;
                    OnPropertyChanged("SwUpdateEntry");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}