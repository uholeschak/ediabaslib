using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    public enum VCIReservationType
    {
        NONE,
        IVM,
        WEB,
        UNKNOWN
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum VCIDeviceType
    {
        ENET,
        ICOM,
        IMIB,
        EDIABAS,
        SIM,
        VIRTUALCHANNEL,
        OMITEC,
        INFOSESSION,
        TELESERVICE,
        IRAM,
        PTT,
        UNKNOWN
    }

    public enum DeviceState
    {
        Init,
        Booted,
        Lost,
        Sleep,
        Free,
        Reserved,
        Selftest,
        Fail,
        Found,
        Transit,
        Updated,
        Unregistered,
        Blocked,
        FreeNvm,
        FirmwareOutdated
    }

    public enum NetworkType
    {
        Unknown = -1,
        LAN,
        WLAN
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVciDevice : INotifyPropertyChanged, IVciDeviceRuleEvaluation
    {
        string AccuCapacity { get; }

        IBasicFeatures BasicFeatures { get; }

        string Color { get; }

        bool CommunicationDisturbanceRecognized { get; }

        bool ConnectionLossRecognized { get; }

        bool IsDoIP { get; }

        int? ControlPort { get; }

        string Counter { get; }

        string Description { get; }

        string Description1 { get; }

        string DevId { get; }

        string DevType { get; }

        bool ForceReInit { get; }

        string Gateway { get; }

        string IFHParameter { get; }

        string IFHReserved { get; }

        string IPAddress { get; }

        string ImageVersionApplication { get; }

        string ImageVersionBoot { get; }

        string ImageVersionPackage { get; }

        string Imagename { get; }

        bool IsConnectable { get; }

        bool IsImibR2 { get; }

        bool IsImibNext { get; }

        string Kl15Trigger { get; }

        string Kl15Voltage { get; }

        string Kl30Trigger { get; }

        string Kl30Voltage { get; }

        string MacAddress { get; }

        string Netmask { get; }

        string NetworkType { get; }

        string NetworkTypeLabel { get; }

        string Owner { get; }

        int? Port { get; }

        string PwfState { get; }

        string ReceivingIP { get; }

        bool ReconnectFailed { get; }

        long ReserveHandle { get; }

        DateTime ScanDate { get; }

        string Serial { get; }

        string Service { get; }

        string SignalStrength { get; }

        string State { get; }

        string UUID { get; }

        bool UnderVoltageRecognized { get; }

        DateTime UnderVoltageRecognizedLastTime { get; }

        bool UnderVoltageRecognizedLastTimeSpecified { get; }

        bool UsePdmResult { get; }

        VCIReservationType VCIReservation { get; }

        string VIN { get; }

        string VciChannels { get; }

        long leastSigBits { get; }

        bool leastSigBitsSpecified1 { get; }

        long mostSigBits { get; }

        bool mostSigBitsSpecified1 { get; }

        bool IsConnected { get; }

        bool IsMarkedToDefault { get; set; }

        bool IsSimulation { get; set; }

        bool CheckChannel(string channelId);

        double? GetClamp15();

        double? GetClamp30();

        string ToAttrList();

        string ToAttrList(bool addLineFeed);

        string getVCIDescription(VCIDeviceType devType);
    }
}
