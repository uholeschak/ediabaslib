using BMW.Authoring;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IVerbraucher : IHideObjectMembers
    {
        int EFuseId { get; set; }

        IList<double> EFuse_Nennwert { get; set; }

        IList<string> Grobzeichen { get; set; }

        IList<ITextLocator> Langnamen { get; set; }

        IList<string> Kurznamen { get; set; }

        IList<string> Durchschliff { get; set; }
    }
}
