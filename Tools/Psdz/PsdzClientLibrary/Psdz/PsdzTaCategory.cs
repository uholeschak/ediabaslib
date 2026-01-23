using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzTa))]
    [KnownType(typeof(PsdzTaExecutionState))]
    [KnownType(typeof(PsdzIdRestoreTa))]
    [KnownType(typeof(PsdzIdRestoreLightTa))]
    [KnownType(typeof(PsdzIdBackupLightTa))]
    [KnownType(typeof(PsdzFscDeployTa))]
    [KnownType(typeof(PsdzBlFlashTa))]
    [KnownType(typeof(PsdzIbaDeployTa))]
    [KnownType(typeof(PsdzSwDeployTa))]
    [KnownType(typeof(PsdzSFADeleteTA))]
    [KnownType(typeof(PsdzSFAVerifyTA))]
    [KnownType(typeof(PsdzSFAWriteTA))]
    [KnownType(typeof(PsdzHddUpdateTA))]
    [KnownType(typeof(PsdzSmacSwDeployOnMasterTA))]
    [KnownType(typeof(PsdzSmacEcuMirrorDeployOnMasterTA))]
    [KnownType(typeof(PsdzSmacTransferStartTA))]
    [KnownType(typeof(PsdzSmacTransferStatusTA))]
    [KnownType(typeof(PsdzEcuMirrorDeployTa))]
    [KnownType(typeof(PsdzEcuActivateTa))]
    [KnownType(typeof(PsdzEcuPollTa))]
    public class PsdzTaCategory : IPsdzTaCategory
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsEmpty { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzTa> Tas { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzTaExecutionState? ExecutionState { get; set; }
    }
}
