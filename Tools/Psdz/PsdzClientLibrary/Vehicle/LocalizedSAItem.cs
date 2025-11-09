using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [DataContract]
    public class LocalizedSAItem
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        public LocalizedSAItem()
        {
        }

        public LocalizedSAItem(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? (-1);
        }
    }
}