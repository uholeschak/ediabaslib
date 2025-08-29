using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Tal
{
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
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [DataMember]
        public PsdzFscDeploy FscDeploy { get; set; }

        [DataMember]
        public PsdzBlFlash BlFlash { get; set; }

        [DataMember]
        public PsdzIbaDeploy IbaDeploy { get; set; }

        [DataMember]
        public PsdzSwDeploy SwDeploy { get; set; }

        [DataMember]
        public PsdzIdRestore IdRestore { get; set; }

        [DataMember]
        public PsdzSFADeploy SFADeploy { get; set; }

        [DataMember]
        public PsdzIdBackup IdBackup { get; set; }

        [DataMember]
        public PsdzFscBackup FscBackup { get; set; }

        [DataMember]
        public PsdzHddUpdate HddUpdate { get; set; }

        [DataMember]
        public PsdzSmacTransferStart SmacTransferStart { get; set; }

        [DataMember]
        public PsdzSmacTransferStatus SmacTransferStatus { get; set; }

        [DataMember]
        public PsdzEcuMirrorDeploy EcuMirrorDeploy { get; set; }

        [DataMember]
        public PsdzEcuActivate EcuActivate { get; set; }

        [DataMember]
        public PsdzEcuPoll EcuPoll { get; set; }

        [DataMember]
        public PsdzTaCategories TaCategories { get; set; }

        [DataMember]
        public IPsdzTaCategory TaCategory { get; set; }
    }
}
