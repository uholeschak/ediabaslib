using BMW.Authoring;
using BMW.Authoring.Vehicle.Enums;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IEcu : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string Gruppe { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Variante { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string PVEName { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Kurzname { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        long Adresse { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        long Lieferantennummer { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime Herstelldatum { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Seriennummer { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ISvk Svk { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ILcSwitchList LcSwitchList { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        EcuGeneration Generation { get; }
    }
}
