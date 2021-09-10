using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [DataContract]
    public class LocalizedSAItem
    {
        public LocalizedSAItem()
        {
        }

        public LocalizedSAItem(string id, string title)
        {
            this.Id = id;
            this.Title = title;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        public override int GetHashCode()
        {
            string id = this.Id;
            return (id != null) ? id.GetHashCode() : -1;
        }
    }
}