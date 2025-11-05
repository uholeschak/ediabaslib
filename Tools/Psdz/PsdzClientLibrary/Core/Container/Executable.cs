// BMW.Rheingold.Module.ISTA.Executable
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public enum NoDeviceBehaviorForBody
    {
        SystemDefault,
        DefaultValueResult,
        SubstitutionValueInput
    }

    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Executable")]
    public class Executable : All, INotifyPropertyChanged
    {
        private ABranch resultField;
        private NoDeviceBehaviorForBody noDeviceBehaviorField;
        [DataMember]
        public ABranch Result
        {
            get
            {
                return resultField;
            }

            set
            {
                if (resultField != null)
                {
                    if (!resultField.Equals(value))
                    {
                        resultField = value;
                        OnPropertyChanged("Result");
                    }
                }
                else
                {
                    resultField = value;
                    OnPropertyChanged("Result");
                }
            }
        }

        [XmlAttribute]
        [DefaultValue(NoDeviceBehaviorForBody.SystemDefault)]
        [DataMember]
        public NoDeviceBehaviorForBody NoDeviceBehavior
        {
            get
            {
                return noDeviceBehaviorField;
            }

            set
            {
                if (!noDeviceBehaviorField.Equals(value))
                {
                    noDeviceBehaviorField = value;
                    OnPropertyChanged("NoDeviceBehavior");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public Executable()
        {
            noDeviceBehaviorField = NoDeviceBehaviorForBody.SystemDefault;
        }

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}