// BMW.Rheingold.Module.ISTA.ValueBase
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DataContract(Name = "ValueBase")]
    [XmlInclude(typeof(MultipleValue))]
    [XmlInclude(typeof(Value))]
    [DesignerCategory("code")]
    public abstract class ValueBase : ANode, INotifyPropertyChanged
    {
        private AConstraint constraintField;

        private Unit unitField;

        private bool substitutionValueField;

        [DataMember]
        public AConstraint Constraint
        {
            get
            {
                return constraintField;
            }
            set
            {
                if (constraintField != null)
                {
                    if (!constraintField.Equals(value))
                    {
                        constraintField = value;
                        OnPropertyChanged("Constraint");
                    }
                }
                else
                {
                    constraintField = value;
                    OnPropertyChanged("Constraint");
                }
            }
        }

        [DataMember]
        public Unit Unit
        {
            get
            {
                return unitField;
            }
            set
            {
                if (unitField != null)
                {
                    if (!unitField.Equals(value))
                    {
                        unitField = value;
                        OnPropertyChanged("Unit");
                    }
                }
                else
                {
                    unitField = value;
                    OnPropertyChanged("Unit");
                }
            }
        }

        [DataMember]
        [XmlAttribute]
        [DefaultValue(false)]
        public bool SubstitutionValue
        {
            get
            {
                return substitutionValueField;
            }
            set
            {
                if (!substitutionValueField.Equals(value))
                {
                    substitutionValueField = value;
                    OnPropertyChanged("SubstitutionValue");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public ValueBase()
        {
            substitutionValueField = false;
        }

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
