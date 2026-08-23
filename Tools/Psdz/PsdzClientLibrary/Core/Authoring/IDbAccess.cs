using PsdzClient;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Database
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDbAccess : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IDbCCMessage GetCCMessageByID(int ID);

        [EditorBrowsable(EditorBrowsableState.Always)]
        List<IDbCCMessage> GetCCMessagesListByID(int ID, params int[] ID_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        List<IDbCCMessage> GetCCMessagesListByID(params int[] ID);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDbDtc GetDtcByCodeAndVariant(long Code, string Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDbComponent GetComponentByGrobzeichen(string Grobzeichen);
    }
}
