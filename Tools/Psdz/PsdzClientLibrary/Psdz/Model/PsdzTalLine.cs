using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzTaCategory))]
    [KnownType(typeof(PsdzBlFlash))]
    [KnownType(typeof(PsdzCdDeploy))]
    [KnownType(typeof(PsdzFscBackup))]
    [KnownType(typeof(PsdzFscDeploy))]
    [KnownType(typeof(PsdzGatewayTableDeploy))]
    [KnownType(typeof(PsdzHddUpdate))]
    [KnownType(typeof(PsdzHwDeinstall))]
    [KnownType(typeof(PsdzHwInstall))]
    [KnownType(typeof(PsdzIbaDeploy))]
    [KnownType(typeof(PsdzIdBackup))]
    [KnownType(typeof(PsdzIdRestore))]
    [KnownType(typeof(PsdzPreviousRun))]
    [KnownType(typeof(PsdzSwDelete))]
    [KnownType(typeof(PsdzSwDeploy))]
    [KnownType(typeof(PsdzSFADeploy))]
    [KnownType(typeof(PsdzSmacTransferStart))]
    [KnownType(typeof(PsdzSmacTransferStatus))]
    public class PsdzTalLine : PsdzTalElement, IPsdzTalLine, IPsdzTalElement
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzFscDeploy FscDeploy { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBlFlash BlFlash { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzIbaDeploy IbaDeploy { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSwDeploy SwDeploy { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzIdRestore IdRestore { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSFADeploy SFADeploy { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzIdBackup IdBackup { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzFscBackup FscBackup { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzHddUpdate HddUpdate { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSmacTransferStart SmacTransferStart { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSmacTransferStatus SmacTransferStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuMirrorDeploy EcuMirrorDeploy { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuActivate EcuActivate { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuPoll EcuPoll { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzTaCategories TaCategories { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzTaCategory TaCategory { get; set; }
    }
}
