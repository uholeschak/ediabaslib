using BMW.Authoring;
using BMW.Authoring.API;
using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IQDMModeAPI : IHideObjectMembers
    {
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IAuthoringModule IstaModule { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool ActivateQDMMode(ITextLocator popupTitle = null, ITextLocator popupMessage = null, DialogSize dialogSize = DialogSize.S);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool FillTrefferListe(string identifikator, int? priority = null);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool IsQDMModeActivated();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool DeactivateQDMMode();
    }
}
