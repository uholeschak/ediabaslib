using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum BNMixed
    {
        HETEROGENEOUS,
        HOMOGENEOUS,
        SEPARATED,
        UNKNOWN
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum BNType
    {
        BN2000,
        BN2020,
        IBUS,
        BN2000_MOTORBIKE,
        BN2020_MOTORBIKE,
        BNK01X_MOTORBIKE,
        BEV2010,
        BN2000_MORGAN,
        BN2000_WIESMANN,
        BN2000_RODING,
        BN2000_PGO,
        BN2000_GIBBS,
        BN2020_CAMPAGNA,
        UNKNOWN
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum BrandName
    {
        [XmlEnum("BMW PKW")]
        BMWPKW,
        [XmlEnum("MINI PKW")]
        MINIPKW,
        [XmlEnum("ROLLS-ROYCE PKW")]
        ROLLSROYCEPKW,
        [XmlEnum("BMW MOTORRAD")]
        BMWMOTORRAD,
        [XmlEnum("BMW M GmbH PKW")]
        BMWMGmbHPKW,
        [XmlEnum("BMW USA PKW")]
        BMWUSAPKW,
        HUSQVARNA,
        WIESMANN,
        MORGAN,
        RODING,
        PGO,
        GIBBS,
        [XmlEnum("BMW i")]
        BMWi,
        TOYOTA,
        CAMPAGNA,
        ZINORO,
        YANMAR,
        BRILLIANCE,
        VAILLANT,
        ROSENBAUER,
        KARMA,
        TORQEEDO,
        WORKHORSE
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    //[Obsolete("Legacy Property, the ChassisType is retrieved from the Database and not used in test modules, thus it can be deleted.")]
    public enum ChassisType
    {
        LIM,
        TOU,
        COU,
        SAV,
        ROA,
        SH,
        CAB,
        SAT,
        HC,
        NONE,
        SAC,
        COM,
        CLU,
        HAT,
        SHA,
        UNKNOWN
    }

    public enum VisibilityType
    {
        Hidden,
        Collapsed,
        Visible
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum GwszUnitType
    {
        km,
        miles
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum IdentificationLevel
    {
        None,
        BasicFeatures,
        ReopenedOperation,
        VINOnly,
        VINBasedFeatures,
        VINBasedOnlineUpdated,
        VINVehicleReadout,
        VINVehicleReadoutOnlineUpdated,
        VehicleTypeNotLicensed
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicle : INotifyPropertyChanged
    {
        string AEKurzbezeichnung { get; }

        string AELeistungsklasse { get; }

        string AEUeberarbeitung { get; }

        string Abgas { get; }

        string Antrieb { get; }

        string ApplicationVersion { get; }

        BNMixed BNMixed { get; }

        BNType BNType { get; }

        string BasicType { get; }

        string Baureihe { get; }

        string Baureihenverbund { get; }

        string BaustandsJahr { get; }

        string BaustandsMonat { get; }

        BrandName? BrandName { get; }

        DateTime? C_DATETIME { get; }

        //IEnumerable<ICbsInfo> CBS { get; }

        bool CVDRequestFailed { get; set; }

        bool CvsRequestFailed { get; set; }

        ChassisType ChassisType { get; }

        //IEnumerable<IDtc> CombinedFaults { get; }

        VisibilityType ConnectIMIBIPState { get; }

        string ConnectIMIBImage { get; }

        string EMotBaureihe { get; }

        VisibilityType ConnectIMIBState { get; }

        VisibilityType ConnectIPState { get; }

        string ConnectImage { get; }

        VisibilityType ConnectState { get; }

        bool DOMRequestFailed { get; set; }

        //IEnumerable<IDiagCode> DiagCodes { get; }

        ObservableCollection<string> DiagCodesProgramming { get; }

        string Drehmoment { get; }

        string DriveType { get; }

        string ECTypeApproval { get; }

        IEnumerable<IEcu> ECU { get; }

        string Ereihe { get; }

        IFa FA { get; }

        bool FASTAAlreadyDone { get; }

        IEnumerable<IFfmResult> FFM { get; }

        DateTime? FirstRegistration { get; }

        string GMType { get; }

        string Getriebe { get; }

        string CountryOfAssembly { get; }

        string BaseVersion { get; }

        string Gsgbd { get; }

        string MainSeriesSgbd { get; }

        string MainSeriesSgbdAdditional { get; }

        decimal? Gwsz { get; }

        GwszUnitType? GwszUnit { get; }

        string Hubraum { get; }

        string Hybridkennzeichen { get; }

        string ILevel { get; }

        string ILevelBackup { get; }

        string ILevelWerk { get; }

        IEnumerable<decimal> InstalledAdapters { get; }

        bool KL15FaultILevelAlreadyAlerted { get; }

        bool KL15OverrideVoltageCheck { get; }

        string Karosserie { get; }

        string Kl15Voltage { get; }

        string Kl30Voltage { get; }

        DateTime KlVoltageLastMessageTime { get; }

        bool KlVoltageLastMessageTimeSpecified { get; }

        string Kraftstoffart { get; }

        string Land { get; }

        DateTime LastChangeDate { get; }

        DateTime LastSaveDate { get; }

        string Leistung { get; }

        string Leistungsklasse { get; }

        string Lenkung { get; }

        //IVciDevice MIB { get; }

        string MOTBezeichnung { get; }

        string MOTEinbaulage { get; }

        string MOTKraftstoffart { get; }

        string Marke { get; }

        string Modelljahr { get; }

        string Modellmonat { get; }

        string Modelltag { get; }

        string Motor { get; }

        string Motorarbeitsverfahren { get; }

        bool PADVehicle { get; }

        bool Pannenfall { get; }

        string Prodart { get; }

        DateTime ProductionDate { get; }

        bool ProductionDateSpecified { get; }

        string Produktlinie { get; set; }

        string ProgmanVersion { get; }

        int PwfState { get; }

        bool RepHistoryRequestFailed { get; }

        int SelectedDiagBUS { get; }

        IEcu SelectedECU { get; }

        string SerialBodyShell { get; }

        string SerialEngine { get; }

        List<DealerSessionProperty> DealerSessionProperties { get; }

        string SerialGearBox { get; }

        //IEnumerable<IServiceHistoryEntry> ServiceHistory { get; }

        bool SimulatedParts { get; }

        bool Sp2021Enabled { get; set; }

        string Status_FunctionName { get; }

        double Status_FunctionProgress { get; }

        StateType Status_FunctionState { get; }

        DateTime Status_FunctionStateLastChangeTime { get; }

        bool Status_FunctionStateLastChangeTimeSpecified { get; }

        bool TecCampaignsRequestFailed { get; }

        //IEnumerable<ITechnicalCampaign> TechnicalCampaigns { get; }

        string Typ { get; }

        string Ueberarbeitung { get; }

        IVciDevice VCI { get; }

        string VIN17 { get; set; }

        bool IsSendFastaDataForbidden { get; set; }

        bool IsSendFastaDataForbiddenBitsQueueFull { get; set; }

        string VIN17_OEM { get; }

        string VIN7 { get; }

        string VINType { get; }

        bool VehicleIdentAlreadyDone { get; }

        IdentificationLevel VehicleIdentLevel { get; }

        string VerkaufsBezeichnung { get; }

        string RoadMap { get; }

        string WarrentyType { get; }

        string ZCS { get; }

        //IEnumerable<IZfsResult> ZFS { get; }

        bool ZFS_SUCCESSFULLY { get; }

        string refSchema { get; }

        string version { get; }

        bool IsPowerSafeModeActive { get; }

        bool IsPowerSafeModeActiveByOldEcus { get; set; }

        bool IsPowerSafeModeActiveByNewEcus { get; set; }

        bool IsVehicleBreakdownAlreadyShown { get; set; }

        string ChassisCode { get; set; }

        //ITransmissionDataType TransmissionDataType { get; }

        bool IsMotorcycle();

        IEcu getECU(long? sgAdr);

        IEcu getECU(long? sgAdr, long? subAddress);

        IEcu getECUbyECU_GRUPPE(string ECU_GRUPPE);

        bool hasSA(string checkSA);

        bool IsProgrammingSupported(bool considerLogisticBase);

        bool IsVehicleWithOnlyVin7();
    }
}
