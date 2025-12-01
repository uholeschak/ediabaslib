using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Text")]
    public class Text : INotifyPropertyChanged
    {
        private TranslationMode translationModeField;
        private string textIdField;
        private string valueField;
        [XmlAttribute]
        [DefaultValue(TranslationMode.None)]
        [DataMember]
        public TranslationMode TranslationMode
        {
            get
            {
                return translationModeField;
            }

            set
            {
                if (!translationModeField.Equals(value))
                {
                    translationModeField = value;
                    OnPropertyChanged("TranslationMode");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string TextId
        {
            get
            {
                return textIdField;
            }

            set
            {
                if (textIdField != null)
                {
                    if (!textIdField.Equals(value))
                    {
                        textIdField = value;
                        OnPropertyChanged("TextId");
                    }
                }
                else
                {
                    textIdField = value;
                    OnPropertyChanged("TextId");
                }
            }
        }

        [XmlText]
        [DataMember]
        public string Value
        {
            get
            {
                return valueField;
            }

            set
            {
                if (valueField != null)
                {
                    if (!valueField.Equals(value))
                    {
                        valueField = value;
                        OnPropertyChanged("Value");
                    }
                }
                else
                {
                    valueField = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Text()
        {
            translationModeField = TranslationMode.None;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}