using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
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
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [DataContract(Name = "Flow")]
    public class Flow : INotifyPropertyChanged
    {
        private TextReferenceStructure titleField;

        private TextReferenceStructure descriptionField;

        private MainTestStep mainTestStepField;

        private ObservableCollection<TestStep> testStepsField;

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

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public MainTestStep MainTestStep
        {
            get
            {
                return mainTestStepField;
            }
            set
            {
                if (mainTestStepField != null)
                {
                    if (!mainTestStepField.Equals(value))
                    {
                        mainTestStepField = value;
                        OnPropertyChanged("MainTestStep");
                    }
                }
                else
                {
                    mainTestStepField = value;
                    OnPropertyChanged("MainTestStep");
                }
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem(Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public ObservableCollection<TestStep> TestSteps
        {
            get
            {
                return testStepsField;
            }
            set
            {
                if (testStepsField != null)
                {
                    if (!testStepsField.Equals(value))
                    {
                        testStepsField = value;
                        OnPropertyChanged("TestSteps");
                    }
                }
                else
                {
                    testStepsField = value;
                    OnPropertyChanged("TestSteps");
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
