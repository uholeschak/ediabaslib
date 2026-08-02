using PsdzClient.Core;
using System.Runtime.Serialization;

namespace BMW.Authoring.API.MetaData
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [DataContract]
    public class PicklistDataFieldItem
    {
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(IsRequired = true, Name = "isSelected")]
        public bool IsSelected { get; set; }

        [DataMember(IsRequired = true, Name = "isEnabled")]
        public bool IsEnabled { get; set; }

        [DataMember(IsRequired = true, Name = "isVisible")]
        public bool IsVisible { get; set; }

        [DataMember(IsRequired = true, Name = "value")]
        public object Value { get; set; }
    }
}
