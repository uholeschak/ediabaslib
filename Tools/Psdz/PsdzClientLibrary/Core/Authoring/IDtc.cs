using BMW.Authoring;
using BMW.Authoring.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDtc : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        long Code { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string CodeAsString { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Relevanz { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        long HFK { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        long HLZ { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int ReadyNr { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int VorhandenNr { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent VorhandenText { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int WarnungNr { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<ISaeContext> SaeContext { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        SaeCodeStatus SaeCodeStatus { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string SaeCodeString { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent SaeCodeTitel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        List<IUwSaetze> UW_SAETZE { get; }

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        List<IZfsContext> ZfsContext { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool SammelfehlerSet { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IDtcSammel SammelfehlerRef { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        WarnlampenStatus WarnlampenStatus { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        FehlerklasseWert Fehlerklasse { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        FehlergruppeWert Fehlergruppe { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool VorhandenNr_IsValue(int nr, params int[] nr_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool VorhandenNr_IsValue(int[] nr);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool UW_IsAvailableByNrName(long UW_NR, string UW_NAME);

        [EditorBrowsable(EditorBrowsableState.Always)]
        T UW_getWertByNrName<T>(long UW_NR, string UW_NAME);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        T UW_getWertByName<T>(string UW_NAME_Regelement);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<T> UW_getWerteByName<T>(string UW_NAME_Regelement);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void _Write_WarnlampenStatus(WarnlampenStatus newValue);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void _Write_FirstSaeContextEntry(string newValue);
    }
}
