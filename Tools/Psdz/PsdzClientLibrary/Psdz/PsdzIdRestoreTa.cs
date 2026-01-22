using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzIdRestoreTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string BackupFile { get; set; }
    }
}
