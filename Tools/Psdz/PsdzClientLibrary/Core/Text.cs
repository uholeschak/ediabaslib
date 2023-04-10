// BMW.Rheingold.Module.ISTA.Text
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public enum TranslationMode
    {
        All,
        RuntimeOnly,
        DesigntimeOnly,
        None
    }

    [Serializable]
    [DesignerCategory("code")]
    [DataContract(Name = "Text")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class Text : INotifyPropertyChanged
    {
        private TranslationMode translationModeField;

        private string textIdField;

        private string valueField;

        [DataMember]
        [DefaultValue(TranslationMode.None)]
        [XmlAttribute]
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

        [DataMember]
        [XmlAttribute]
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

        [DataMember]
        [XmlText]
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
