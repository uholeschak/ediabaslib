using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;
using System;

namespace BMW.Rheingold.Module.ISTA
{
    public class ISTA_Zeit : ISTAModule
    {
        public ISTA_Zeit(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Datum(ref string Datum_String, ref int Datum_Jahr, ref int Datum_Monat, ref int Datum_Tag, ref int Datum_JJJJMMTT)
        {
            int num = 0;
            Logger.WriteInformation("Datumcalled");
            int year = DateTime.UtcNow.Year;
            int month = DateTime.UtcNow.Month;
            int day = DateTime.UtcNow.Day;
            Datum_String = $"{year:0000}" + "/" + $"{month:00}" + "/" + $"{day:00}";
            Datum_Jahr = year;
            Datum_Monat = month;
            Datum_Tag = day;
            Datum_JJJJMMTT = Convert.ToInt32($"{year:0000}" + $"{month:00}" + $"{day:00}");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
