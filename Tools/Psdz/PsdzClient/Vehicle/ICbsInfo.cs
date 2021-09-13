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
    public enum typeCBSMeaurementType
    {
        MotorOil,
        BrakeFront,
        BrakeOil,
        MikroFilter,
        BrakeRear,
        DieselFilter,
        Batterie,
        QMV_H_Oel,
        IgnitionPlug,
        VehicleCheck,
        Kuehlfluessigkeit,
        H2_Check,
        VehicleCheckByGov,
        EmissionCheck,
        VehicleCheckCoupledWithGov,
        Unknown
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum typeCBSVersion
    {
        CBS13,
        CBS4,
        CBS5,
        UNKNOWN
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ICbsInfo : INotifyPropertyChanged
    {
        string AVAI_CBS_EINH { get; }

        short? AVAI_CBS_WERT { get; }

        string COU_RSTG_CBS_MESS_EINH { get; }

        short? COU_RSTG_CBS_MESS_WERT { get; }

        short? FRC_INTM_T_CBS_MESS { get; }

        string FRC_INTM_WAY_CBS_EINH { get; }

        short? FRC_INTM_WAY_CBS_MESS { get; }

        string ID_FN_CBS_MESS_TEXT { get; }

        short? ID_FN_CBS_MESS_WERT { get; }

        short? MANIP_CBS { get; }

        bool? MMIAnnouncement { get; }

        string RMMI_CBS_EINH { get; }

        short? RMMI_CBS_WERT { get; }

        short? STATUS_MESSUNG { get; }

        string STATUS_MESSUNG_TEXT { get; }

        string ST_UN_CBS_HEX { get; }

        string ST_UN_CBS_TEXT { get; }

        short? ST_UN_CBS_WERT { get; }

        typeCBSMeaurementType Type { get; }

        typeCBSVersion Version { get; }

        DateTime? ZIEL { get; }
    }
}
