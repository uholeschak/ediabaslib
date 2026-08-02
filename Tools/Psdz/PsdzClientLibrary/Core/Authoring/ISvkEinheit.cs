using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISvkEinheit : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string SGBMID { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Version VERSION { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string IDENTIFIER { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        SvkProzessklasse Prozessklasse_Kurztext { get; }
    }
}
