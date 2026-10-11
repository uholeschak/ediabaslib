using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
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
}
