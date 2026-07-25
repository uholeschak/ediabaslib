using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Authoring.Vehicle.Enums;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IEcuList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<IEcu> GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Variante_IsSet(string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Variante_IsSet(string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Gruppe_IsSet(string Gruppe, params string[] Gruppe_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Gruppe_IsSet(string[] Gruppe);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Adresse_IsSet(long Adresse, params long[] Adresse_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Adresse_IsSet(long[] Adresse);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu_GetByVariante(string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu_GetByVariante(string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu_GetByGruppe(string Gruppe);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu_GetByAdresse(long Adresse);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Generation_IsSet(EcuGeneration Generation);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IList<IEcu> Ecu_GetByGeneration(EcuGeneration Generation);
    }
}
