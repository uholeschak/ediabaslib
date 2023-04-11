// BMW.Rheingold.Module.ISTA.ANode
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DataContract(Name = "ANode")]
    [DesignerCategory("code")]
    [XmlInclude(typeof(ABranch))]
    [XmlInclude(typeof(Sequence))]
    [XmlInclude(typeof(AChoice))]
    [XmlInclude(typeof(QuantityChoice))]
    [XmlInclude(typeof(MultipleChoice))]
    [XmlInclude(typeof(SingleChoice))]
    [XmlInclude(typeof(All))]
    [XmlInclude(typeof(Executable))]
    [XmlInclude(typeof(Reference))]
    [XmlInclude(typeof(ValueBase))]
    [XmlInclude(typeof(Value))]
    [XmlInclude(typeof(MultipleValue))]
    public abstract class ANode : INotifyPropertyChanged
    {
        private string idField;

        private string nameField;

        private string titleField;

        private string titleIdField;

        private string commentField;

        private string commentIdField;

        private bool hiddenField;

        private bool hiddenFieldSpecified;

        private TranslationMode translationModeField;

        private string parentIdField;

        private string tagField;

        private string hintField;

        [DataMember]
        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                if (idField != null)
                {
                    if (!idField.Equals(value))
                    {
                        idField = value;
                        OnPropertyChanged("Id");
                    }
                }
                else
                {
                    idField = value;
                    OnPropertyChanged("Id");
                }
            }
        }

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

        [DataMember]
        [XmlAttribute]
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

        [DataMember]
        [XmlAttribute]
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

        [DataMember]
        [XmlAttribute]
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

        [DataMember]
        [XmlAttribute]
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

        [DataMember]
        [XmlAttribute]
        public bool Hidden
        {
            get
            {
                return hiddenField;
            }
            set
            {
                if (!hiddenField.Equals(value))
                {
                    hiddenField = value;
                    OnPropertyChanged("Hidden");
                }
            }
        }

        [DataMember]
        [XmlIgnore]
        public bool HiddenSpecified
        {
            get
            {
                return hiddenFieldSpecified;
            }
            set
            {
                if (!hiddenFieldSpecified.Equals(value))
                {
                    hiddenFieldSpecified = value;
                    OnPropertyChanged("HiddenSpecified");
                }
            }
        }

        [DataMember]
        [XmlAttribute]
        [DefaultValue(TranslationMode.All)]
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
        [XmlAttribute(DataType = "IDREF")]
        public string ParentId
        {
            get
            {
                return parentIdField;
            }
            set
            {
                if (parentIdField != null)
                {
                    if (!parentIdField.Equals(value))
                    {
                        parentIdField = value;
                        OnPropertyChanged("ParentId");
                    }
                }
                else
                {
                    parentIdField = value;
                    OnPropertyChanged("ParentId");
                }
            }
        }

        [DataMember]
        [XmlAttribute]
        public string Tag
        {
            get
            {
                return tagField;
            }
            set
            {
                if (tagField != null)
                {
                    if (!tagField.Equals(value))
                    {
                        tagField = value;
                        OnPropertyChanged("Tag");
                    }
                }
                else
                {
                    tagField = value;
                    OnPropertyChanged("Tag");
                }
            }
        }

        [DataMember]
        [XmlAttribute]
        public string Hint
        {
            get
            {
                return hintField;
            }
            set
            {
                if (hintField != null)
                {
                    if (!hintField.Equals(value))
                    {
                        hintField = value;
                        OnPropertyChanged("Hint");
                    }
                }
                else
                {
                    hintField = value;
                    OnPropertyChanged("Hint");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public object Query(string nodePath)
        {
            //Log.Warning("ANode.Query()", "not implemented !!!");
            if (Name != null && string.Equals(Name, nodePath))
            {
                return this;
            }
            if (this is ABranch)
            {
                ABranch aBranch = (ABranch)this;
                if (aBranch.Children != null)
                {
                    foreach (ANode child in aBranch.Children)
                    {
                        if (child.Name == null || !string.Equals(child.Name, nodePath))
                        {
                            if (child.Query(nodePath) is ANode result)
                            {
                                return result;
                            }
                            continue;
                        }
                        return this;
                    }
                }
            }
            else if (this is Sequence)
            {
                foreach (ANode child2 in ((Sequence)this).Children)
                {
                    if (child2.Name == null || !string.Equals(child2.Name, nodePath))
                    {
                        if (child2.Query(nodePath) is ANode result2)
                        {
                            return result2;
                        }
                        continue;
                    }
                    return this;
                }
            }
            else if (this is AChoice)
            {
                foreach (ANode child3 in ((AChoice)this).Children)
                {
                    if (child3.Name == null || !string.Equals(child3.Name, nodePath))
                    {
                        if (child3.Query(nodePath) is ANode result3)
                        {
                            return result3;
                        }
                        continue;
                    }
                    return this;
                }
            }
            else if (this is QuantityChoice)
            {
                foreach (ANode child4 in ((QuantityChoice)this).Children)
                {
                    if (child4.Name == null || !string.Equals(child4.Name, nodePath))
                    {
                        if (child4.Query(nodePath) is ANode result4)
                        {
                            return result4;
                        }
                        continue;
                    }
                    return this;
                }
            }
            else if (this is MultipleChoice)
            {
                foreach (ANode child5 in ((MultipleChoice)this).Children)
                {
                    if (child5.Name == null || !string.Equals(child5.Name, nodePath))
                    {
                        if (child5.Query(nodePath) is ANode result5)
                        {
                            return result5;
                        }
                        continue;
                    }
                    return this;
                }
            }
            else if (this is SingleChoice)
            {
                foreach (ANode child6 in ((SingleChoice)this).Children)
                {
                    if (child6.Name == null || !string.Equals(child6.Name, nodePath))
                    {
                        if (child6.Query(nodePath) is ANode result6)
                        {
                            return result6;
                        }
                        continue;
                    }
                    return this;
                }
            }
            else if (this is All)
            {
                foreach (ANode child7 in ((All)this).Children)
                {
                    if (child7.Name == null || !string.Equals(child7.Name, nodePath))
                    {
                        if (child7.Query(nodePath) is ANode result7)
                        {
                            return result7;
                        }
                        continue;
                    }
                    return this;
                }
            }
            else if (this is Executable)
            {
                foreach (ANode child8 in ((Executable)this).Children)
                {
                    if (child8.Name == null || !string.Equals(child8.Name, nodePath))
                    {
                        if (child8.Query(nodePath) is ANode result8)
                        {
                            return result8;
                        }
                        continue;
                    }
                    return this;
                }
            }
            return null;
        }

        public ANode()
        {
            translationModeField = TranslationMode.All;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
