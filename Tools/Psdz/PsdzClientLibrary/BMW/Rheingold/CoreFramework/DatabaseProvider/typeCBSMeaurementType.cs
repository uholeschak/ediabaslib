using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
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
}
