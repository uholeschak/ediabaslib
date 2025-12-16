using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVciDevice : INotifyPropertyChanged, IVciDeviceRuleEvaluation, ICloneable
    {
        string AccuCapacity { get; set; }

        IBasicFeatures BasicFeatures { get; }

        string Color { get; set; }

        bool CommunicationDisturbanceRecognized { get; set; }

        bool ConnectionLossRecognized { get; set; }

        bool IsDoIP { get; set; }

        int? ControlPort { get; set; }

        string Counter { get; set; }

        string Description { get; set; }

        string Description1 { get; set; }

        string DevId { get; set; }

        string DevType { get; set; }

        DeviceTypeDetails DeviceTypeDetail { get; set; }

        DeviceState DeviceState { get; set; }

        bool ForceReInit { get; set; }

        string Gateway { get; set; }

        string IFHParameter { get; set; }

        string IFHReserved { get; set; }

        string IPAddress { get; set; }

        string ImageVersionApplication { get; set; }

        string ImageVersionBoot { get; set; }

        string ImageVersionPackage { get; set; }

        string Imagename { get; set; }

        bool IsConnectable { get; }

        bool IsVehicleProgrammingPossible { get; }

        bool IsDead { get; }

        bool IsAlive { get; }

        bool IsImibR2 { get; }

        bool IsImibNext { get; }

        string Kl15Trigger { get; set; }

        string Kl15Voltage { get; set; }

        string Kl30Trigger { get; set; }

        string Kl30Voltage { get; set; }

        string MacAddress { get; set; }

        string WLANMacAddress { get; set; }

        string Netmask { get; set; }

        string NetworkType { get; set; }

        string NetworkTypeLabel { get; }

        NetworkType LocalAdapterNetworkType { get; set; }

        string Owner { get; set; }

        int? Port { get; set; }

        string PwfState { get; set; }

        string ReceivingIP { get; set; }

        bool ReconnectFailed { get; set; }

        long ReserveHandle { get; set; }

        DateTime ScanDate { get; set; }

        string Serial { get; set; }

        string Service { get; set; }

        string SignalStrength { get; set; }

        string State { get; set; }

        string UUID { get; set; }

        bool UnderVoltageRecognized { get; set; }

        DateTime UnderVoltageRecognizedLastTime { get; set; }

        bool UnderVoltageRecognizedLastTimeSpecified { get; set; }

        bool UsePdmResult { get; set; }

        VCIReservationType VCIReservation { get; set; }

        string VIN { get; set; }

        string VciChannels { get; set; }

        long leastSigBits { get; set; }

        bool leastSigBitsSpecified1 { get; set; }

        long mostSigBits { get; set; }

        bool mostSigBitsSpecified1 { get; set; }

        bool IsConnected { get; set; }

        bool IsMarkedToDefault { get; set; }

        bool IsSimulation { get; set; }

        new VCIDeviceType VCIType { get; set; }

        bool CheckChannel(string channelId);

        double? GetClamp15();

        double? GetClamp30();

        string ToAttrList();

        string ToAttrList(bool addLineFeed);

        string getVCIDescription(VCIDeviceType devType);

        void SetAlive();

        bool IsSupportedImibOrICOM(string[] acceptedImibDevices);
    }
}