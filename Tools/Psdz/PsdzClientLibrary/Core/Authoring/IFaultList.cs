using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IFaultList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IsAvailable { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IFault this[int index] { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IFault this[string CodeAsHexString] { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<IFault> GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Dtc_IsSet(long Code, string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Dtc_IsSet(long Code, string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Dtc_IsRelevant(long Code, string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Dtc_IsRelevant(long Code, string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool DtcVirtuell_IsRelevant(string CodeAsString);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool DtcSammel_IsRelevant(string CodeAsString);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtc Dtc_GetByCode(long Code, string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtc Dtc_GetByCode(long Code, string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtcVirtuell DtcVirtuell_GetByCode(string CodeAsString);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtcSammel DtcSammel_GetByCode(string CodeAsString);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterLikeGUI();

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByHfk(int HFK);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByVorhandenNr(int nr, params int[] nr_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByVorhandenNr(int[] nr);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByVarianten(string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByVarianten(string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwKm(double Km, double Plus, double Minus, bool ohneKMberücksichtigen);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwKm(double Km, double Plus, double Minus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwKm(double Km, double PlusMinus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwKm(double Km);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwZeit(double Zeit, double Plus, double Minus, bool ohneZeitberücksichtigen);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwZeit(double Zeit, double Plus, double Minus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwZeit(double Zeit, double PlusMinus);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByUwZeit(double Zeit);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByFehlerklasse(FehlerklasseWert Fehlerklasse, params FehlerklasseWert[] Fehlerklasse_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByFehlerklasse(FehlerklasseWert[] Fehlerklasse);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FilterByFehlergruppe(FehlergruppeWert Fehlergruppe);

        IFaultList UpdateDtc(long Code, string Variante, params string[] Variante_);

        IFaultList UpdateDtc(long Code, string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList UpdateVarianten(string Variante, params string[] Variante_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList UpdateVarianten(string[] Variante);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList UpdateAlleVarianten();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IFaultList SetFaultList(List<IFault> FaultList);

        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<IFault> FilterByIsarIds(HashSet<decimal> isarIds);
    }
}
