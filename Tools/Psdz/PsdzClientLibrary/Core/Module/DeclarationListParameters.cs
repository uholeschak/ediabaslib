using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "DeclarationListParameters")]
    public class DeclarationListParameters : INotifyPropertyChanged
    {
        private ObservableCollection<InputParameter> inputParametersField;

        private ObservableCollection<OutputParameter> outputParametersField;

        private ObservableCollection<InputParameter> inputAndOutputParametersField;

        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("Parameter", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<InputParameter> InputParameters
        {
            get
            {
                return inputParametersField;
            }
            set
            {
                if (inputParametersField != null)
                {
                    if (!inputParametersField.Equals(value))
                    {
                        inputParametersField = value;
                        OnPropertyChanged("InputParameters");
                    }
                }
                else
                {
                    inputParametersField = value;
                    OnPropertyChanged("InputParameters");
                }
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("Parameter", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<OutputParameter> OutputParameters
        {
            get
            {
                return outputParametersField;
            }
            set
            {
                if (outputParametersField != null)
                {
                    if (!outputParametersField.Equals(value))
                    {
                        outputParametersField = value;
                        OnPropertyChanged("OutputParameters");
                    }
                }
                else
                {
                    outputParametersField = value;
                    OnPropertyChanged("OutputParameters");
                }
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("Parameter", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<InputParameter> InputAndOutputParameters
        {
            get
            {
                return inputAndOutputParametersField;
            }
            set
            {
                if (inputAndOutputParametersField != null)
                {
                    if (!inputAndOutputParametersField.Equals(value))
                    {
                        inputAndOutputParametersField = value;
                        OnPropertyChanged("InputAndOutputParameters");
                    }
                }
                else
                {
                    inputAndOutputParametersField = value;
                    OnPropertyChanged("InputAndOutputParameters");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
