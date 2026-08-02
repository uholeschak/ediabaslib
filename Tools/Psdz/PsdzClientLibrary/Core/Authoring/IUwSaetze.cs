using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IUwSaetze : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<IUw> UWs { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int UW_ANZ { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        double UW_KM { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        double UW_ZEIT { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime UW_ZEITasDateTime { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool UW_IsAvailableByNrName(long UW_NR, string UW_NAME);

        [EditorBrowsable(EditorBrowsableState.Always)]
        T UW_getWertByNrName<T>(long UW_NR, string UW_NAME);

        [EditorBrowsable(EditorBrowsableState.Always)]
        T UW_getWertByName<T>(string UW_NAME_Regelement);
    }
}
