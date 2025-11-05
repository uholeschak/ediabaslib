// BMW.Rheingold.VehicleCommunication.ECUResult
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("System.Xml", "2.0.50727.3082")]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://tempuri.org/ECUJob.xsd", TypeName = "ECUResult")]
    [XmlRoot(Namespace = "http://tempuri.org/ECUJob.xsd", IsNullable = true, ElementName = "ECUResult")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ECUResult : INotifyPropertyChanged, IEcuResult
    {
        private object value;
        private string name;
        private int format;
        private ushort set;
        private bool setSpecified;
        private uint length;
        private bool lengthSpecified;
        private bool fastaRelevant;
        [XmlIgnore]
        public bool FASTARelevant
        {
            get
            {
                return fastaRelevant;
            }

            set
            {
                if (value != fastaRelevant)
                {
                    fastaRelevant = value;
                    RaisePropertyChanged("FASTARelevant");
                }
            }
        }

        [XmlElement(IsNullable = true, ElementName = "value")]
        public object Value
        {
            get
            {
                return value;
            }

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }

        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        [XmlAttribute(AttributeName = "format")]
        public int Format
        {
            get
            {
                return format;
            }

            set
            {
                if (format != value)
                {
                    format = value;
                    RaisePropertyChanged("Format");
                }
            }
        }

        [XmlAttribute(AttributeName = "set")]
        public ushort Set
        {
            get
            {
                return set;
            }

            set
            {
                if (set != value)
                {
                    set = value;
                    RaisePropertyChanged("Set");
                    setSpecified = true;
                }
            }
        }

        [XmlIgnore]
        public bool SetSpecified
        {
            get
            {
                return setSpecified;
            }

            set
            {
                if (setSpecified != value)
                {
                    setSpecified = value;
                    RaisePropertyChanged("SetSpecified");
                }
            }
        }

        [XmlAttribute(AttributeName = "length")]
        public uint Length
        {
            get
            {
                return length;
            }

            set
            {
                if (length != value)
                {
                    length = value;
                    RaisePropertyChanged("Length");
                    lengthSpecified = true;
                }
            }
        }

        [XmlIgnore]
        public bool LengthSpecified
        {
            get
            {
                return lengthSpecified;
            }

            set
            {
                if (lengthSpecified != value)
                {
                    lengthSpecified = value;
                    RaisePropertyChanged("LengthSpecified");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ECUResult()
        {
        }

        public ECUResult(object value, string name, int format, ushort set, bool setSpecified, uint length, bool lengthSpecified)
        {
            this.value = value;
            this.name = name;
            this.format = format;
            this.set = set;
            this.setSpecified = setSpecified;
            this.length = length;
            this.lengthSpecified = lengthSpecified;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}