using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum EcuMode
    {
        PLANT,
        FIELD,
        ENGINEERING,
        ECUSubfunctionNotSupported
    }
}
