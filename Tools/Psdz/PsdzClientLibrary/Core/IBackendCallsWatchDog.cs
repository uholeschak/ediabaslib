using System.Collections.Generic;
using System.Net;
using System;

namespace PsdzClientLibrary.Core
{
    public enum BackendServiceType
    {
        ExecutionBreak,
        ValidityPatches,
        SdpOnlinePatch,
        PatchServicePrograms,
        EcuValidation,
        SfaTokenDirect,
        SfaNewestPackageForVehicle,
        SfaTokenRequest,
        SecureCoding,
        BrokerMonitor,
        DOM,
        ServiceHistory,
        NOP,
        SWT,
        SWTV3,
        CeSIM,
        FBMImport,
        CVS,
        VPS,
        OTDLSC,
        DOMVin7Resolver,
        TechnicalCampaignsv030400,
        VehicleTagsService,
        SfaNewFeatureForVehicle,
        EDGEObfcm,
        EDGESpeedlink,
        EDGEBattery,
        SmartMaintenance,
        SmartMaintenanceNew,
        AIR,
        CalibrationValue,
        DOMOrderData,
        VehicleEmissionService,
        ServiceRide,
        EDGEPDI,
        AosDocumentXmlData,
        AosDocumentStreamData,
        ServiceStateLayer2,
        VehicleBasic,
        ProtocolUploadState,
        VPSProvisioning,
        Sec4Diag,
        SCCGetFileByName,
        SCCPostFile,
        SCCDeleteFileByName,
        SCCPostVehicleSession,
        SCCGetVehicleSession,
        OrderData
    }

    public interface IBackendCallsWatchDog
    {
        Dictionary<BackendServiceType, HttpStatusCode> LatestBackendResponse { get; }

        int GetTotalCallCounter(BackendServiceType serviceType);

        int GetSpecificStatusCallCounter(BackendServiceType serviceType, HttpStatusCode statusCode);

        void AddBackendCall(BackendServiceType serviceType, HttpStatusCode? status, string refVersion = "", Exception ex = null);

        void AddBackendCall(BackendServiceType serviceType, HttpStatusCode? status, IFasta2Service fasta2Service, List<OnlinePatchDownloadStatus> patchFiles, List<string> nameFiles = null, string refVersion = "");
    }
}