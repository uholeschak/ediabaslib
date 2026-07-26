using BMW.Authoring;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface ICCMList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IsAvailable { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ICCM this[int index] { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ICCM this[long code] { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<ICCM> GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCM CCM_GetByCode(long code);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool CCM_IsSet(long code);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByKm(double km, double plus, double minus, bool ohneKMberücksichtigen);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByKm(double km, double plus, double minus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByKm(double km, double plusMinus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByKm(double km);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByZeit(double zeit, double plus, double minus, bool ohneZeitberücksichtigen);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByZeit(double zeit, double plus, double minus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByZeit(double zeit, double plusMinus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByZeit(double zeit);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByCode(long code, double plus, double minus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByCode(long code, double plusMinus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList FilterByCode(long code);
    }
}
