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
    [DataContract(Name = "ValueLiteral")]
    public class ValueLiteral : INotifyPropertyChanged
    {
        private object itemField;
        [XmlElement("Binary", typeof(byte[]), DataType = "base64Binary")]
        [XmlElement("Bool", typeof(bool))]
        [XmlElement("DateTime", typeof(DateTime))]
        [XmlElement("Decimal", typeof(decimal))]
        [XmlElement("Double", typeof(double))]
        [XmlElement("Float", typeof(float))]
        [XmlElement("Int", typeof(int))]
        [XmlElement("Long", typeof(long))]
        [XmlElement("SByte", typeof(sbyte))]
        [XmlElement("Short", typeof(short))]
        [XmlElement("Text", typeof(Text))]
        [XmlElement("UByte", typeof(byte))]
        [XmlElement("UInt", typeof(uint))]
        [XmlElement("ULong", typeof(ulong))]
        [XmlElement("UShort", typeof(ushort))]
        [DataMember]
        public object Item
        {
            get
            {
                return itemField;
            }

            set
            {
                if (itemField != null)
                {
                    if (!itemField.Equals(value))
                    {
                        itemField = value;
                        OnPropertyChanged("Item");
                    }
                }
                else
                {
                    itemField = value;
                    OnPropertyChanged("Item");
                }
            }
        }

        public string ItemType
        {
            get
            {
                if (Item is byte[])
                {
                    return "Binary";
                }

                if (Item is bool)
                {
                    return "Bool";
                }

                if (Item is DateTime)
                {
                    return "DateTime";
                }

                if (Item is decimal)
                {
                    return "Decimal";
                }

                if (Item is double)
                {
                    return "Double";
                }

                if (Item is float)
                {
                    return "Float";
                }

                if (Item is int)
                {
                    return "Int";
                }

                if (Item is long)
                {
                    return "Long";
                }

                if (Item is sbyte)
                {
                    return "SByte";
                }

                if (Item is short)
                {
                    return "Short";
                }

                if (Item is Text)
                {
                    return "Text";
                }

                if (Item is byte)
                {
                    return "UByte";
                }

                if (Item is uint)
                {
                    return "UInt";
                }

                if (Item is ulong)
                {
                    return "ULong";
                }

                if (Item is ushort)
                {
                    return "UShort";
                }

                return "unknown";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public T GetValue<T>()
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Item;
            }

            return default(T);
        }
    }
}