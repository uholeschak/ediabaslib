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
    [DataContract(Name = "Unit")]
    public class Unit : INotifyPropertyChanged
    {
        private string nameField;

        private string titleField;

        private string titleIdField;

        private string commentField;

        private string commentIdField;

        private FactorPrefix factorPrefixField;

        private string measureField;

        [XmlAttribute]
        [DataMember]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                if (nameField != null)
                {
                    if (!nameField.Equals(value))
                    {
                        nameField = value;
                        OnPropertyChanged("Name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Title
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

        [XmlAttribute]
        [DataMember]
        public string TitleId
        {
            get
            {
                return titleIdField;
            }
            set
            {
                if (titleIdField != null)
                {
                    if (!titleIdField.Equals(value))
                    {
                        titleIdField = value;
                        OnPropertyChanged("TitleId");
                    }
                }
                else
                {
                    titleIdField = value;
                    OnPropertyChanged("TitleId");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Comment
        {
            get
            {
                return commentField;
            }
            set
            {
                if (commentField != null)
                {
                    if (!commentField.Equals(value))
                    {
                        commentField = value;
                        OnPropertyChanged("Comment");
                    }
                }
                else
                {
                    commentField = value;
                    OnPropertyChanged("Comment");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string CommentId
        {
            get
            {
                return commentIdField;
            }
            set
            {
                if (commentIdField != null)
                {
                    if (!commentIdField.Equals(value))
                    {
                        commentIdField = value;
                        OnPropertyChanged("CommentId");
                    }
                }
                else
                {
                    commentIdField = value;
                    OnPropertyChanged("CommentId");
                }
            }
        }

        [XmlAttribute]
        [DefaultValue(FactorPrefix.None)]
        [DataMember]
        public FactorPrefix FactorPrefix
        {
            get
            {
                return factorPrefixField;
            }
            set
            {
                if (!factorPrefixField.Equals(value))
                {
                    factorPrefixField = value;
                    OnPropertyChanged("FactorPrefix");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Measure
        {
            get
            {
                return measureField;
            }
            set
            {
                if (measureField != null)
                {
                    if (!measureField.Equals(value))
                    {
                        measureField = value;
                        OnPropertyChanged("Measure");
                    }
                }
                else
                {
                    measureField = value;
                    OnPropertyChanged("Measure");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Unit()
        {
            factorPrefixField = FactorPrefix.None;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}