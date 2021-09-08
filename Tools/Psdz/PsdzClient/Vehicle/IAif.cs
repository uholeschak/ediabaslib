using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Vehicle
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IAif : INotifyPropertyChanged
    {
        int? AIF_ADRESSE_HIGH { get; }

        int? AIF_ADRESSE_LOW { get; }

        int? AIF_ANZAHL_PROG { get; }

        int? AIF_ANZ_DATEN { get; }

        int? AIF_ANZ_FREI { get; }

        string AIF_BEHOERDEN_NR { get; }

        string AIF_DATUM { get; }

        string AIF_FG_NR { get; }

        string AIF_FG_NR_LANG { get; }

        int? AIF_GROESSE { get; }

        string AIF_HAENDLER_NR { get; }

        long? AIF_KM { get; }

        long? AIF_LAENGE { get; }

        string AIF_PROG_NR { get; }

        string AIF_SERIEN_NR { get; }

        string AIF_SW_NR { get; }

        string AIF_ZB_NR { get; }
    }
}
