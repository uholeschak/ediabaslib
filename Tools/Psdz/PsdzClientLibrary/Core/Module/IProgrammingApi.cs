using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IProgrammingApi
    {
        bool IsExpectedSgbmidValidationActive { get; set; }

        bool IsExpectedSgbmIdValidationForSmacTransferStartActive { get; set; }

        IProgrammingObjectBuilder ObjectBuilder { get; }

        IBoolResultObject IsSimulation();

        IEnumerable<IEcuIdentifier> CheckProgrammingCounter();

        bool DeactivateFsc(IEcuIdentifier ecuIdentifier, ISwtApplicationId swtApplicationId);

        IDictionary<string, object> ExecuteAsamJob(IEcuIdentifier ecuIdentifier, string jobId, IAsamJobInputDictionary asamJobInputDictionary);

        IVehicleProfile GenerateVehicleProfile(IFa fa);

        IStandardSvk GetCurrentSvk(IEcuIdentifier ecu);

        IList<string> GetPossibleILevel(IFa fa);

        IEnumerable<ISmartActuatorMasterEcu> RetrieveSmartActuatorMasters();

        byte[] ReadFsc(IEcuIdentifier ecuIdentifier, ISwtApplicationId swtApplicationId);

        IFa RequestFa();

        IFa RequestFaFromBackup();

        IIstufenTriple RequestILevel();

        IIstufenTriple RequestILevelFromBackup();

        ISvt RequestSvtEcu();

        ISvt RequestSvtEcuWithSmacs();

        ISvt RequestSvtEcu(IEnumerable<IEcuIdentifier> ecus);

        ISvt RequestSvtEcuWithSmacs(IEnumerable<IEcuIdentifier> ecus);

        ISvt RequestSvtFromVcm();

        IVehicleProfileChecksum RequestVpcFromVcm();

        ISwt RequestSwtInfo();

        ISwt RequestSwtInfo(IEcuIdentifier ecuIdentifier, ISwtApplicationId swtApplicationId);

        IVehicleProfile RequestVehicleProfile();

        string RequestVinFromBackup();

        string RequestVinFromMaster();

        bool StoreFsc(byte[] fsc, IEcuIdentifier ecuIdentifier, ISwtApplicationId swtApplicationId);

        bool WriteFaToVcm(IFa fa);

        bool WriteFaToVcmBackup(IFa fa);

        bool WriteILevelToVcm(string iLevelShipment, string iLevelLast, string iLevelCurrent);

        bool WriteILevelToVcmBackup(string iLevelShipment, string iLevelLast, string iLevelCurrent);

        bool WriteSvtToVcm(ISvt svt);

        bool WriteVehicleProfileToVcm(IVehicleProfile vehicleProfile);

        bool CheckIBAC(string orderCode, string ibacCode);

        void AddExecutionOrderTop(string linkType, string pattern);

        void AddExecutionOrderBottom(string linkType, string pattern);

        bool IsPsdZBackUpModeSet();

        void SetupPsdZBackupMode();

        bool IsAOSModeActive();
    }
}
