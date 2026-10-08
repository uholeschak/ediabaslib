using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;

#pragma warning disable CS0649
namespace BMW.Rheingold.Module.ISTA
{
    internal class Fahrzeugauftrag_ausISTA_UX : ISTAModule
    {
        public string strDlgInfo;
        public bool bWriteLog;
        public int m_maxAnzahlSonderausstattungen;
        public string[] Sonderausstattungen1;
        public Fahrzeugauftrag_ausISTA_UX(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }

            __handleInParameter();
            strDlgInfo = "11.01.2010 -Typmerkmale_ausISTA_UX";
            bWriteLog = true;
            m_maxAnzahlSonderausstattungen = 100;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Sonderausstattung(ref string[] Sonderausstattungen)
        {
            int num = 0;
            Logger.WriteInformation("Sonderausstattungcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation(strDlgInfo);
            }

            Sonderausstattungen = new string[m_maxAnzahlSonderausstattungen];
            _DoLoopHandling = true;
            for (int i = 0; i < Sonderausstattungen.Length; i++)
            {
                Sonderausstattungen[i] = "";
            }

            _DoLoopHandling = false;
            string text = "";
            int num2 = 0;
            int num3 = 0;
            text = "/ExternalData/ServiceProgram/PublicData/ExtendedVehicleInformation/SP/Equipments";
            if (SOCAccessor.OrderContext.System.GetProperty(text)is List<string> list)
            {
                _DoLoopHandling = true;
                for (int j = 0; j < list.Count; j++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list[j];
                    }

                    num2++;
                    num3++;
                }

                _DoLoopHandling = false;
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
