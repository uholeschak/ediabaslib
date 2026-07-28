using System;
using BMW.Rheingold.Module.ISTA;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using PsdzClient;

namespace PsdzClientLibrary.Core.Module;
[Serializable]
[XmlInclude(typeof(TextReferenceStructure))]
[GeneratedCode("Xsd2Code", "3.4.0.32990")]
[DesignerCategory("code")]
[XmlRoot("ReferenceElement", Namespace = "", IsNullable = false)]
[DataContract(Name = "ReferenceStructure")]
[PreserveSource(Hint = "XmlIncludes removed", InheritanceModified = true)]
public abstract class ReferenceStructure : INotifyPropertyChanged
{
    private ReferenceStructureType typeField;
    private string pathField;
    [XmlAttribute]
    [DefaultValue(ReferenceStructureType.CMS)]
    [DataMember]
    public ReferenceStructureType Type
    {
        get
        {
            return typeField;
        }

        set
        {
            if (!typeField.Equals(value))
            {
                typeField = value;
                OnPropertyChanged("Type");
            }
        }
    }

    [XmlAttribute]
    [DataMember]
    public string Path
    {
        get
        {
            return pathField;
        }

        set
        {
            if (pathField != null)
            {
                if (!pathField.Equals(value))
                {
                    pathField = value;
                    OnPropertyChanged("Path");
                }
            }
            else
            {
                pathField = value;
                OnPropertyChanged("Path");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public ReferenceStructure()
    {
        typeField = ReferenceStructureType.CMS;
    }

    public virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}