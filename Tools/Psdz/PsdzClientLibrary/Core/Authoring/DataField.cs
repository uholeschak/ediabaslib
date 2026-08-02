using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Authoring.API.MetaData
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [DataContract]
    public class DataField
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "mandatory")]
        public bool Mandatory { get; set; } = true;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "isEditable")]
        public bool IsEditable { get; set; } = true;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "id")]
        public string Id { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "description")]
        public string Description { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "type")]
        public string Type { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "dataFieldType")]
        public DataFieldType DataFieldType { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "value")]
        public object Value { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "minLength")]
        public int? MinLength { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "maxLength")]
        public int? MaxLength { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "stringDataType")]
        public StringInputDataType? StringDataType { get; set; }

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "format")]
        public string Format { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "minDate")]
        public DateTime? MinDate { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "maxDate")]
        public DateTime? MaxDate { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "minValue")]
        public object MinValue { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(Name = "maxValue")]
        public object MaxValue { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DataMember(IsRequired = true, Name = "possibleValues")]
        public List<PicklistDataFieldItem> PossibleValues { get; set; }
    }
}
