using System;
using BMW.Rheingold.Module.ISTA;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "MainTestStep")]
    public class MainTestStep : TestStep, INotifyPropertyChanged
    {
        private DeclarationListParameters parametersField;
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public DeclarationListParameters Parameters
        {
            get
            {
                return parametersField;
            }

            set
            {
                if (parametersField != null)
                {
                    if (!parametersField.Equals(value))
                    {
                        parametersField = value;
                        OnPropertyChanged("Parameters");
                    }
                }
                else
                {
                    parametersField = value;
                    OnPropertyChanged("Parameters");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}