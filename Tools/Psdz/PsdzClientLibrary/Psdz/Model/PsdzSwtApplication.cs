using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Swt
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSwtApplicationId))]
    public class PsdzSwtApplication : IPsdzSwtApplication
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] Fsc { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] FscCert { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsBackupPossible { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Position { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzFscCertificateState FscCertState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzFscState FscState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSoftwareSigState? SoftwareSigState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSwtActionType? SwtActionType { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSwtType SwtType { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSwtApplicationId SwtApplicationId { get; set; }
    }
}
