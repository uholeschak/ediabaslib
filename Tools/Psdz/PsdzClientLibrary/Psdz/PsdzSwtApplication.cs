using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Swt
{
    [DataContract]
    [KnownType(typeof(PsdzSwtApplicationId))]
    public class PsdzSwtApplication : IPsdzSwtApplication
    {
        [DataMember]
        public byte[] Fsc { get; set; }

        [DataMember]
        public byte[] FscCert { get; set; }

        [DataMember]
        public bool IsBackupPossible { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public PsdzFscCertificateState FscCertState { get; set; }

        [DataMember]
        public PsdzFscState FscState { get; set; }

        [DataMember]
        public PsdzSoftwareSigState? SoftwareSigState { get; set; }

        [DataMember]
        public PsdzSwtActionType? SwtActionType { get; set; }

        [DataMember]
        public PsdzSwtType SwtType { get; set; }

        [DataMember]
        public IPsdzSwtApplicationId SwtApplicationId { get; set; }
    }
}
