using BMW.Rheingold.CoreFramework;
using System;

namespace BMW.Rheingold.Module.ISTA
{
    public class Messwerte_Generator1 : ISTAModule
    {
        public double Rueckgabewert;

        public string Richtung;

        public int p_Von;

        public int p_Bis;

        public Messwerte_Generator1(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            Rueckgabewert = 0.0;
            Richtung = "0";
            p_Von = 0;
            p_Bis = 0;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Zufallszahl(int Von, int Bis, int Nachkomma, ref double Wert)
        {
            int num = 0;
            Logger.WriteInformation("Zufallszahlcalled");
            int minValue = Von * Convert.ToInt32(Math.Pow(10.0, Nachkomma));
            int maxValue = Bis * Convert.ToInt32(Math.Pow(10.0, Nachkomma));
            int value = new Random().Next(minValue, maxValue);
            Rueckgabewert = Convert.ToDouble(value) / Math.Pow(10.0, Nachkomma);
            Wert = Rueckgabewert;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Zahlenbereich_hin_her(int Von, int Bis, int Nachkomma, double Schrittweite, ref double Wert)
        {
            int num = 0;
            Logger.WriteInformation("Zahlenbereich_hin_hercalled");
            p_Von = Von;
            p_Bis = Bis;
            string richtung = Richtung;
            if (!(richtung == "+"))
            {
                if (richtung == "-")
                {
                    if (Rueckgabewert > (double)p_Von)
                    {
                        Rueckgabewert = (Rueckgabewert * Math.Pow(10.0, Nachkomma) - Schrittweite * Math.Pow(10.0, Nachkomma)) / Math.Pow(10.0, Nachkomma);
                    }
                    else
                    {
                        Richtung = "+";
                    }
                }
                else
                {
                    Richtung = "+";
                }
            }
            else if (Rueckgabewert < (double)p_Bis)
            {
                Rueckgabewert = (Rueckgabewert * Math.Pow(10.0, Nachkomma) + Schrittweite * Math.Pow(10.0, Nachkomma)) / Math.Pow(10.0, Nachkomma);
            }
            else
            {
                Richtung = "-";
            }
            Wert = Rueckgabewert;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
