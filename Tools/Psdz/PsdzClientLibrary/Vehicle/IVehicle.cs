using PsdzClient;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

#pragma warning disable CS0618
namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicle : INotifyPropertyChanged, IVehicleRuleEvaluation, IVinValidatorVehicle
    {
        [Obsolete]
        bool IsEcuIdentSuccessfull { get; set; }
        new string AELeistungsklasse { get; }
        new string AEUeberarbeitung { get; }
        new string Antrieb { get; set; }

        string ApplicationVersion { get; }

        List<string> SxCodes { get; set; }

        [Obsolete("Is not used anymore in Testmodules. Will be removed in 4.48!")]
        BNMixed BNMixed { get; set; }

        BNType BNType { get; set; }
        new string Baureihe { get; set; }
        new string Baureihenverbund { get; set; }

        string BaustandsJahr { get; }

        string BaustandsMonat { get; }

        [PreserveSource(Hint = "IEnumerable<ICbsInfo>", Placeholder = true)]
        PlaceholderType CBS { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.Ssl2RequestFailed")]
        bool Ssl2RequestFailed { get; set; }

        ChassisType ChassisType { get; }

        [PreserveSource(Hint = "IEnumerable<IDtc>", Placeholder = true)]
        PlaceholderType CombinedFaults { get; }

        [Obsolete]
        VisibilityType ConnectIMIBIPState { get; }

        [Obsolete]
        string ConnectIMIBImage { get; }

        string EMotBaureihe { get; }

        [Obsolete]
        VisibilityType ConnectIMIBState { get; }

        [Obsolete]
        VisibilityType ConnectIPState { get; }

        [Obsolete]
        string ConnectImage { get; }

        [Obsolete]
        VisibilityType ConnectState { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.DOMRequestFailed")]
        bool DOMRequestFailed { get; set; }

        [PreserveSource(Hint = "IEnumerable<IDiagCode>", Placeholder = true)]
        PlaceholderType DiagCodes { get; }

        ObservableCollection<string> DiagCodesProgramming { get; }

        string DriveType { get; }

        string ECTypeApproval { get; }
        new IEnumerable<IEcu> ECU { get; }
        new IFa FA { get; }

        [Obsolete]
        bool FASTAAlreadyDone { get; }

        IEnumerable<IFfmResult> FFM { get; }

        DateTime? FirstRegistration { get; }

        string MainSeriesSgbd { get; }

        string MainSeriesSgbdAdditional { get; }

        decimal? Gwsz { get; }

        GwszUnitType? GwszUnit { get; }

        string ILevelBackup { get; set; }

        IEnumerable<decimal> InstalledAdapters { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.KL15FaultILevelAlreadyAlerted")]
        bool KL15FaultILevelAlreadyAlerted { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.KL15OverrideVoltageCheck")]
        bool KL15OverrideVoltageCheck { get; }

        string Kl15Voltage { get; }

        string Kl30Voltage { get; }

        DateTime KlVoltageLastMessageTime { get; }

        bool KlVoltageLastMessageTimeSpecified { get; }

        DateTime LastChangeDate { get; }

        DateTime LastSaveDate { get; }

        IVciDevice MIB { get; }

        string Modelltag { get; }

        bool PADVehicle { get; set; }

        bool Pannenfall { get; }

        bool ProductionDateSpecified { get; }

        string ProgmanVersion { get; }

        int PwfState { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.RepHistoryRequestFailed")]
        bool RepHistoryRequestFailed { get; }

        int SelectedDiagBUS { get; }

        IEcu SelectedECU { get; }

        string SerialBodyShell { get; }

        string SerialEngine { get; }

        List<DealerSessionProperty> DealerSessionProperties { get; }

        string SerialGearBox { get; }

        [PreserveSource(Hint = "IEnumerable<IServiceHistoryEntry>", Placeholder = true)]
        PlaceholderType ServiceHistory { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.SimulatedParts")]
        bool SimulatedParts { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.Status_FunctionName")]
        string Status_FunctionName { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.Status_FunctionProgress")]
        double Status_FunctionProgress { get; }

        StateType Status_FunctionState { get; }

        [Obsolete]
        DateTime Status_FunctionStateLastChangeTime { get; }

        [Obsolete]
        bool Status_FunctionStateLastChangeTimeSpecified { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.TecCampaignsRequestFailed")]
        bool TecCampaignsRequestFailed { get; }

        [PreserveSource(Hint = "IEnumerable<ITechnicalCampaign>", Placeholder = true)]
        PlaceholderType TechnicalCampaigns { get; }

        string Typ { get; set; }
        new IVciDevice VCI { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsSendFastaDataForbidden")]
        bool IsSendFastaDataForbidden { get; set; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsSendOBFCMDataForbidden")]
        bool IsSendOBFCMDataForbidden { get; set; }

        string VIN17 { get; set; }

        string VIN17_OEM { get; }

        string VIN7 { get; }

        string VINType { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.VehicleIdentAlreadyDone")]
        bool VehicleIdentAlreadyDone { get; }

        string RoadMap { get; }

        string WarrentyType { get; }

        string ZCS { get; }

        [PreserveSource(Hint = "IEnumerable<IZfsEntry>", Placeholder = true)]
        PlaceholderType ZFS { get; }

        IEnumerable<ICemResult> CEM { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.ZfsSuccessfull")]
        bool ZFS_SUCCESSFULLY { get; }

        [Obsolete]
        string RefSchema { get; }

        [Obsolete]
        string Version { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsPowerSafeModeActive")]
        bool IsPowerSafeModeActive { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsPowerSafeModeActiveByOldEcus")]
        bool IsPowerSafeModeActiveByOldEcus { get; set; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsPowerSafeModeActiveByOldEcus")]
        bool IsPowerSafeModeActiveByNewEcus { get; set; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsVehicleBreakdownAlreadyShown")]
        bool IsVehicleBreakdownAlreadyShown { get; set; }

        string ChassisCode { get; set; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.OrderDataRequestFailed")]
        bool OrderDataRequestFailed { get; set; }

        TransmissionDataType TransmissionDataType { get; }

        DateTime? C_DATETIME { get; }

        DateTime VehicleLifeStartDate { get; set; }

        double VehicleSystemTime { get; set; }
        new string TypeKeyBasic { get; set; }
        new string TypeKey { get; set; }
        new string TypeKeyLead { get; set; }
        new string ESeriesLifeCycle { get; set; }
        new string LifeCycle { get; set; }

        [Obsolete]
        bool IsDoIP { get; set; }

        IVehicleClassification Classification { get; set; }

        IVehicleProfileChecksum VPC { get; set; }

        IEcu getECU(long? sgAdr);
        IEcu getECU(long? sgAdr, long? subAddress);
        IEcu getECUbyECU_GRUPPE(string ECU_GRUPPE);
        IEcu getECUbyECU_SGBD(string ECU_SGBD);
        IEcu getECUbyTITLE_ECUTREE(string grobName);
        bool AddEcu(IEcu ecu);
        bool RemoveEcu(IEcu ecu);
        bool IsProgrammingSupported(bool considerLogisticBase);
        bool IsEreiheValid();
        bool hasBusType(BusType bus);
        bool HasSA(string checkSA);
        [PreserveSource(Hint = "SessionStart", Placeholder = true)]
        PlaceholderType SessionStart { get; set; }
    }
}