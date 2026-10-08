using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    public class ISTA_Kontext_Ausstattung_Daten : ISTAModule
    {
        public int m_maxAnzahlSonderausstattungen;

        public ISTA_Kontext_Ausstattung_Daten(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            m_maxAnzahlSonderausstattungen = 100;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void SA_Liste(ref List<string> SAs, ref int SA_Anzahl)
        {
            int num = 0;
            Logger.WriteInformation("SA_Listecalled");
            int num2 = 0;
            List<string> list = new List<string>();
            list = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/ServiceProgram/PublicData/ExtendedVehicleInformation/SP/Equipments") as List<string>;
            if (list != null)
            {
                list.Sort();
                num2 = list.Count;
            }
            else
            {
                list = new List<string>();
                list = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleShortTest/AdditionalData/SaLaPas") as List<string>;
                if (list != null)
                {
                    list.Sort();
                    num2 = list.Count;
                }
                else
                {
                    list = new List<string>();
                    list.Add("NV");
                    num2 = -1;
                }
            }
            SAs = list;
            SA_Anzahl = num2;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void E_Worte(ref List<string> EWorte, ref int EWort_Anzahl)
        {
            int num = 0;
            Logger.WriteInformation("E_Wortecalled");
            int num2 = 0;
            List<string> list = new List<string>();
            list = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/AdditionalData/E_Worte") as List<string>;
            if (list != null)
            {
                list.Sort();
                num2 = list.Count;
            }
            else
            {
                list = new List<string>();
                list.Clear();
                list.Add("NV");
                num2 = -1;
            }
            EWorte = list;
            EWort_Anzahl = num2;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void K_Worte(ref List<string> KWorte, ref int KWort_Anzahl)
        {
            int num = 0;
            Logger.WriteInformation("K_Wortecalled");
            int num2 = 0;
            List<string> list = new List<string>();
            list = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/AdditionalData/K_Worte") as List<string>;
            if (list != null)
            {
                list.Sort();
                num2 = list.Count;
            }
            else
            {
                list = new List<string>();
                list.Clear();
                list.Add("NV");
                num2 = -1;
            }
            KWorte = list;
            KWort_Anzahl = num2;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
