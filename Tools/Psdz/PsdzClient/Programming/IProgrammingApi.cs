using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IProgrammingApi
    {
        IProgrammingObjectBuilder ObjectBuilder { get; }

        IEnumerable<IEcuIdentifier> CheckProgrammingCounter();

        bool DeactivateFsc(IEcuIdentifier ecuIdentifier, ISwtApplicationId swtApplicationId);

        IDictionary<string, object> ExecuteAsamJob(IEcuIdentifier ecuIdentifier, string jobId, IAsamJobInputDictionary asamJobInputDictionary);

        IVehicleProfile GenerateVehicleProfile(IFa fa);

        IStandardSvk GetCurrentSvk(IEcuIdentifier ecu);

        IList<string> GetPossibleILevel(IFa fa);

        byte[] ReadFsc(IEcuIdentifier ecuIdentifier, ISwtApplicationId swtApplicationId);

        IFa RequestFa();

        IFa RequestFaFromBackup();

        IIstufenTriple RequestILevel();

        IIstufenTriple RequestILevelFromBackup();

        ISvt RequestSvtEcu();

        ISvt RequestSvtEcu(IEnumerable<IEcuIdentifier> ecus);

        ISvt RequestSvtFromVcm();

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
