using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum BusType
    {
        ROOT,
        ETHERNET,
        MOST,
        KCAN,
        KCAN2,
        KCAN3,
        BCAN,
        CASCAN,
        BCAN2,
        BCAN3,
        FLEXRAY,
        FACAN,
        FASCAN,
        SCAN,
        MRCAN,
        NONE,
        SIBUS,
        KBUS,
        FCAN,
        ACAN,
        HCAN,
        LOCAN,
        ZGW,
        DWA,
        BYTEFLIGHT,
        INTERNAL,
        VIRTUAL,
        VIRTUALBUSCHECK,
        VIRTUALROOT,
        IBUS,
        LECAN,
        LE2CAN,
        IKCAN,
        AECANFD,
        FASCANFD,
        USSCANFD,
        APCANFD,
        INFRACAN,
        PSRRXXCANFD,
        SENSORCANFD,
        UNKNOWN
    }

    public enum GenerationType
    {
        Classic,
        Next,
        Unknown
    }


    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum typeDiagProtocoll
    {
        UDS,
        KWP,
        DS2,
        UNKNOWN
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcu : INotifyPropertyChanged, IIdentEcu
    {
        GenerationType Generation { get; }

        IEnumerable<IAif> AIF { get; }

        bool AIF_SUCCESSFULLY { get; }

        BusType BUS { get; }

        uint BUSID { get; }

        bool COMMUNICATION_SUCCESSFULLY { get; }

        string DATEN_REFERENZ { get; }

        bool DATEN_REFERENZ_SUCCESSFULLY { get; }

        typeDiagProtocoll DiagProtocoll { get; }

        new string ECUTreeColor { get; set; }

        int ECUTreeColumn { get; }

        int ECUTreeRow { get; }

        new string ECU_ADR { get; }

        bool ECU_ASSEMBLY_CONFIRMED { get; }

        string ECU_GROBNAME { get; set; }

        new string ECU_GRUPPE { get; set; }

        bool ECU_HAS_CONFIG_OVERRIDE { get; }

        string ECU_NAME { get; }

        new string ECU_SGBD { get; }

        //IEnumerable<IDtc> FEHLER { get; }

        int FLASH_STATE { get; }

        bool FS_SUCCESSFULLY { get; }

        int F_ANZ { get; }

        string HARDWARE_REFERENZ { get; }

        bool HWREF_SUCCESSFULLY { get; }

        int? HW_REF_STATUS { get; }

        bool IDENT_SUCCESSFULLY { get; }

        string ID_BMW_NR { get; }

        short? ID_BUS_INDEX { get; }

        short? ID_COD_INDEX { get; }

        string ID_DATUM { get; }

        int? ID_DATUM_JAHR { get; }

        short? ID_DATUM_KW { get; }

        int? ID_DATUM_MONAT { get; }

        int? ID_DATUM_TAG { get; }

        int? ID_DIAG_INDEX { get; }

        short? ID_EWS_SS { get; }

        string ID_HW_NR { get; }

        short? ID_LIEF_NR { get; }

        string ID_LIEF_TEXT { get; }

        long? ID_LIN_SLAVE_ADR { get; set; }

        long? ID_SGBD_INDEX { get; }

        new long ID_SG_ADR { get; set; }

        short? ID_SW_NR { get; }

        string ID_SW_NR_FSV { get; }

        string ID_SW_NR_MCV { get; }

        string ID_SW_NR_OSV { get; }

        string ID_SW_NR_RES { get; }

        int? ID_VAR_INDEX { get; }

        //IEnumerable<IDtc> INFO { get; }

        bool IS_SUCCESSFULLY { get; }

        int I_ANZ { get; }

        //IEnumerable<IJob> JOBS { get; }

        bool PHYSHW_SUCCESSFULLY { get; }

        string PHYSIKALISCHE_HW_NR { get; }

        bool SERIAL_SUCCESSFULLY { get; }

        new string SERIENNUMMER { get; }

        ISvk SVK { get; }

        bool SVK_SUCCESSFULLY { get; }

        IEnumerable<ISwtStatus> SWTStatus { get; }

        int StillProgrammable { get; }

        IEnumerable<BusType> SubBUS { get; }

        IEnumerable<IEcuTransaction> TAL { get; }

        new string TITLE_ECUTREE { get; set; }

        IEcuStatusInfo StatusInfo { get; set; }

        string EcuUid { get; set; }

        //ILcSwitchList LCSwitchList { get; set; }

        bool IsSmartActuator { get; }

        IEcuPdxInfo EcuPdxInfo { get; }

        string EcuFullName { get; set; }

        //IDtc GetDTCById(decimal id);

        string GetNewestZusbauNoFromAif();

        bool IsRoot();

        //bool IsSet(long fOrt);

        bool IsVirtual();

        bool IsVirtualOrVirtualBusCheck();

        bool IsVirtualRootOrVirtualBusCheck();

        //IDtc getDTCbyF_ORT(int f_ORT);

        string LogEcu();
    }
}
