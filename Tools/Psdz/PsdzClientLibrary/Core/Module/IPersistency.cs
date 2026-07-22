using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IPersistency
    {
        object GetData(string sType, string sID);

        void StoreData(string sType, string sID, int flags, object sData);
    }
}
