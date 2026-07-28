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
    [DataContract(Name = "TestStep")]
    public class TestStep : ParameterlessMethod, INotifyPropertyChanged
    {
        private TextReferenceStructure titleField;

        private TextReferenceStructure descriptionField;

        private ObservableCollection<Exit> exitsField;

        private int numberOfExitsField;

        private bool verboseLoopLogsField;

        private bool verboseLoopLogsFieldSpecified;

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public TextReferenceStructure Title
        {
            get
            {
                return titleField;
            }
            set
            {
                if (titleField != null)
                {
                    if (!titleField.Equals(value))
                    {
                        titleField = value;
                        OnPropertyChanged("Title");
                    }
                }
                else
                {
                    titleField = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public TextReferenceStructure Description
        {
            get
            {
                return descriptionField;
            }
            set
            {
                if (descriptionField != null)
                {
                    if (!descriptionField.Equals(value))
                    {
                        descriptionField = value;
                        OnPropertyChanged("Description");
                    }
                }
                else
                {
                    descriptionField = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem(Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<Exit> Exits
        {
            get
            {
                return exitsField;
            }
            set
            {
                if (exitsField != null)
                {
                    if (!exitsField.Equals(value))
                    {
                        exitsField = value;
                        OnPropertyChanged("Exits");
                    }
                }
                else
                {
                    exitsField = value;
                    OnPropertyChanged("Exits");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public int NumberOfExits
        {
            get
            {
                return numberOfExitsField;
            }
            set
            {
                if (!numberOfExitsField.Equals(value))
                {
                    numberOfExitsField = value;
                    OnPropertyChanged("NumberOfExits");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public bool VerboseLoopLogs
        {
            get
            {
                return verboseLoopLogsField;
            }
            set
            {
                if (!verboseLoopLogsField.Equals(value))
                {
                    verboseLoopLogsField = value;
                    OnPropertyChanged("VerboseLoopLogs");
                }
            }
        }

        [XmlIgnore]
        [DataMember]
        public bool VerboseLoopLogsSpecified
        {
            get
            {
                return verboseLoopLogsFieldSpecified;
            }
            set
            {
                if (!verboseLoopLogsFieldSpecified.Equals(value))
                {
                    verboseLoopLogsFieldSpecified = value;
                    OnPropertyChanged("VerboseLoopLogsSpecified");
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
