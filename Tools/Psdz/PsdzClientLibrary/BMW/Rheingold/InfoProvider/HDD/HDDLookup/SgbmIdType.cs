using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BMW.Rheingold.InfoProvider.HDD.HDDLookup
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.3.0.20460")]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlRoot("SgbmId", Namespace = "", IsNullable = false)]
    [DataContract(Name = "SgbmIdType")]
    public class SgbmIdType : INotifyPropertyChanged
    {
        private List<EcuVariantType> ecuVariantField;
        private List<SwUpdateType> swUpdateField;
        private string idField;
        private string sWID_FscShortField;
        private string nameField;
        private string supplierField;
        private string sopField;
        private string versionField;
        private string mapOrderNumberBMWField;
        private string mapOrderNumberMINIField;
        private string mapOrderNumberRRField;
        private string successorMapOrderNumberBMWField;
        private string successorMapOrderNumberMINIField;
        private string successorMapOrderNumberRRField;
        [XmlElement("EcuVariant", Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public List<EcuVariantType> EcuVariant
        {
            get
            {
                return ecuVariantField;
            }

            set
            {
                if (ecuVariantField != null)
                {
                    if (!ecuVariantField.Equals(value))
                    {
                        ecuVariantField = value;
                        OnPropertyChanged("EcuVariant");
                    }
                }
                else
                {
                    ecuVariantField = value;
                    OnPropertyChanged("EcuVariant");
                }
            }
        }

        [XmlElement("SwUpdate", Form = XmlSchemaForm.Unqualified)]
        [DataMember]
        public List<SwUpdateType> SwUpdate
        {
            get
            {
                return swUpdateField;
            }

            set
            {
                if (swUpdateField != null)
                {
                    if (!swUpdateField.Equals(value))
                    {
                        swUpdateField = value;
                        OnPropertyChanged("SwUpdate");
                    }
                }
                else
                {
                    swUpdateField = value;
                    OnPropertyChanged("SwUpdate");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string id
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
                        OnPropertyChanged("id");
                    }
                }
                else
                {
                    idField = value;
                    OnPropertyChanged("id");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string SWID_FscShort
        {
            get
            {
                return sWID_FscShortField;
            }

            set
            {
                if (sWID_FscShortField != null)
                {
                    if (!sWID_FscShortField.Equals(value))
                    {
                        sWID_FscShortField = value;
                        OnPropertyChanged("SWID_FscShort");
                    }
                }
                else
                {
                    sWID_FscShortField = value;
                    OnPropertyChanged("SWID_FscShort");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string name
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
                        OnPropertyChanged("name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("name");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string supplier
        {
            get
            {
                return supplierField;
            }

            set
            {
                if (supplierField != null)
                {
                    if (!supplierField.Equals(value))
                    {
                        supplierField = value;
                        OnPropertyChanged("supplier");
                    }
                }
                else
                {
                    supplierField = value;
                    OnPropertyChanged("supplier");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string sop
        {
            get
            {
                return sopField;
            }

            set
            {
                if (sopField != null)
                {
                    if (!sopField.Equals(value))
                    {
                        sopField = value;
                        OnPropertyChanged("sop");
                    }
                }
                else
                {
                    sopField = value;
                    OnPropertyChanged("sop");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string version
        {
            get
            {
                return versionField;
            }

            set
            {
                if (versionField != null)
                {
                    if (!versionField.Equals(value))
                    {
                        versionField = value;
                        OnPropertyChanged("version");
                    }
                }
                else
                {
                    versionField = value;
                    OnPropertyChanged("version");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string MapOrderNumberBMW
        {
            get
            {
                return mapOrderNumberBMWField;
            }

            set
            {
                if (mapOrderNumberBMWField != null)
                {
                    if (!mapOrderNumberBMWField.Equals(value))
                    {
                        mapOrderNumberBMWField = value;
                        OnPropertyChanged("MapOrderNumberBMW");
                    }
                }
                else
                {
                    mapOrderNumberBMWField = value;
                    OnPropertyChanged("MapOrderNumberBMW");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string MapOrderNumberMINI
        {
            get
            {
                return mapOrderNumberMINIField;
            }

            set
            {
                if (mapOrderNumberMINIField != null)
                {
                    if (!mapOrderNumberMINIField.Equals(value))
                    {
                        mapOrderNumberMINIField = value;
                        OnPropertyChanged("MapOrderNumberMINI");
                    }
                }
                else
                {
                    mapOrderNumberMINIField = value;
                    OnPropertyChanged("MapOrderNumberMINI");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string MapOrderNumberRR
        {
            get
            {
                return mapOrderNumberRRField;
            }

            set
            {
                if (mapOrderNumberRRField != null)
                {
                    if (!mapOrderNumberRRField.Equals(value))
                    {
                        mapOrderNumberRRField = value;
                        OnPropertyChanged("MapOrderNumberRR");
                    }
                }
                else
                {
                    mapOrderNumberRRField = value;
                    OnPropertyChanged("MapOrderNumberRR");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string successorMapOrderNumberBMW
        {
            get
            {
                return successorMapOrderNumberBMWField;
            }

            set
            {
                if (successorMapOrderNumberBMWField != null)
                {
                    if (!successorMapOrderNumberBMWField.Equals(value))
                    {
                        successorMapOrderNumberBMWField = value;
                        OnPropertyChanged("successorMapOrderNumberBMW");
                    }
                }
                else
                {
                    successorMapOrderNumberBMWField = value;
                    OnPropertyChanged("successorMapOrderNumberBMW");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string successorMapOrderNumberMINI
        {
            get
            {
                return successorMapOrderNumberMINIField;
            }

            set
            {
                if (successorMapOrderNumberMINIField != null)
                {
                    if (!successorMapOrderNumberMINIField.Equals(value))
                    {
                        successorMapOrderNumberMINIField = value;
                        OnPropertyChanged("successorMapOrderNumberMINI");
                    }
                }
                else
                {
                    successorMapOrderNumberMINIField = value;
                    OnPropertyChanged("successorMapOrderNumberMINI");
                }
            }
        }

        [XmlAttribute]
        [DataMember]
        public string successorMapOrderNumberRR
        {
            get
            {
                return successorMapOrderNumberRRField;
            }

            set
            {
                if (successorMapOrderNumberRRField != null)
                {
                    if (!successorMapOrderNumberRRField.Equals(value))
                    {
                        successorMapOrderNumberRRField = value;
                        OnPropertyChanged("successorMapOrderNumberRR");
                    }
                }
                else
                {
                    successorMapOrderNumberRRField = value;
                    OnPropertyChanged("successorMapOrderNumberRR");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SgbmIdType()
        {
            swUpdateField = new List<SwUpdateType>();
            ecuVariantField = new List<EcuVariantType>();
        }

        public virtual void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}