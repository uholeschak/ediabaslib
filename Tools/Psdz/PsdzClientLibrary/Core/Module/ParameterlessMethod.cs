using java.lang.reflect;
using PsdzClient;
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
    [XmlInclude(typeof(EventHandler))]
    [XmlInclude(typeof(TestStep))]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [DataContract(Name = "ParameterlessMethod")]
    [PreserveSource(Hint = "Member removed", InheritanceModified = true)]
    public class ParameterlessMethod : INotifyPropertyChanged
    {
        private ObservableCollection<Statement> statementsField;
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem(Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<Statement> Statements
        {
            get
            {
                return statementsField;
            }

            set
            {
                if (statementsField != null)
                {
                    if (!statementsField.Equals(value))
                    {
                        statementsField = value;
                        OnPropertyChanged("Statements");
                    }
                }
                else
                {
                    statementsField = value;
                    OnPropertyChanged("Statements");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}