using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
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
        UNKNOWN
    }
}