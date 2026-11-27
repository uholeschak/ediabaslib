using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.SmartMaintenance
{
    [DataContract]
    public class LinkedMedia
    {
        [DataMember]
        public LinkLocation LinkLocation { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string LinkOid { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public byte[] ContentInBytes { get; set; }

        [DataMember]
        public bool ContentIsFromMediaService { get; set; }
    }
}
