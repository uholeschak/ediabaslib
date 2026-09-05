using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;
using System;

namespace BMW.Rheingold.Module.ISTA
{
    public class ISTA_Kontext_FZG_Daten : ISTAModule
    {
        public bool TYPMERKMAL_ISTA_RUN;

        public string[] Typmerkmal;

        public string[] Sonderausstattung;

        public ISTA_Kontext_FZG_Daten(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            TYPMERKMAL_ISTA_RUN = false;
            Typmerkmal = new string[1000];
            Sonderausstattung = new string[1000];
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Produktionsdatum(ref string Produktionsdatum_String, ref int Produktionsdatum_Jahr, ref int Produktionsdatum_Monat, ref int Produktionsdatum_JJJJMM)
        {
            int num = 0;
            Logger.WriteInformation("Produktionsdatum called");
            string text = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/ProductionYear") as string;
            string text2 = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/ProductionMonth") as string;
            text2 = ((!string.IsNullOrEmpty(text2)) ? text2 : "-1");
            text = ((!string.IsNullOrEmpty(text)) ? text : "-1");
            int num2 = Convert.ToInt32(text2);
            int num3 = Convert.ToInt32(text);
            Produktionsdatum_String = ((num3 == -1 || num2 == -1) ? "NV" : ($"{num3:0000}" + "/" + $"{num2:00}"));
            Produktionsdatum_Jahr = ((num3 == -1) ? (-1) : num3);
            Produktionsdatum_Monat = ((num2 == -1) ? (-1) : num2);
            Produktionsdatum_JJJJMM = ((num3 == -1 || num2 == -1) ? (-1) : Convert.ToInt32($"{num3:0000}" + $"{num2:00}"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baustand(ref string Baustand_String, ref int Baustand_Jahr, ref int Baustand_Monat, ref int Baustand_JJJJMM)
        {
            int num = 0;
            Logger.WriteInformation("Baustandcalled");
            string text = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baujahr/Title") as string;
            string text2 = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baumonat/Title") as string;
            text2 = ((!string.IsNullOrEmpty(text2)) ? text2 : "-1");
            text = ((!string.IsNullOrEmpty(text)) ? text : "-1");
            int num2 = Convert.ToInt32(text2);
            int num3 = Convert.ToInt32(text);
            Baustand_String = ((num3 == -1 || num2 == -1) ? "NV" : ($"{num3:0000}" + "/" + $"{num2:00}"));
            Baustand_Jahr = ((num3 == -1) ? (-1) : num3);
            Baustand_Monat = ((num2 == -1) ? (-1) : num2);
            Baustand_JJJJMM = ((num3 == -1 || num2 == -1) ? (-1) : Convert.ToInt32($"{num3:0000}" + $"{num2:00}"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IStufeHO(ref string IStufeHO_String, ref int IStufeHO_Jahr, ref int IStufeHO_Monat, ref int IStufeHO_Nummer, ref int IStufeHO_JJMMIII)
        {
            int num = 0;
            Logger.WriteInformation("IStufeHOcalled");
            string text = SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeHO") as string;
            if (string.IsNullOrEmpty(text))
            {
                text = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleShortTest/AdditionalData/IStufeHO") as string;
            }
            text = ((!string.IsNullOrEmpty(text)) ? text : "NV");
            string value = ((text == "NV") ? "-1" : text.Substring(5, 2));
            string value2 = ((text == "NV") ? "-1" : text.Substring(8, 2));
            string value3 = ((text == "NV") ? "-1" : text.Substring(11, 3));
            int num2 = Convert.ToInt32(value);
            int num3 = Convert.ToInt32(value2);
            int num4 = Convert.ToInt32(value3);
            IStufeHO_String = text;
            IStufeHO_Jahr = ((num2 == -1) ? (-1) : Convert.ToInt32(num2));
            IStufeHO_Monat = ((num3 == -1) ? (-1) : Convert.ToInt32(num3));
            IStufeHO_Nummer = ((num4 == -1) ? (-1) : Convert.ToInt32(num4));
            IStufeHO_JJMMIII = ((num2 == -1 || num3 == -1 || num4 == -1) ? (-1) : Convert.ToInt32($"{num2:00}" + $"{num3:00}" + $"{num4:000}"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IStufeWerk(ref string IStufeWerk_String, ref int IStufeWerk_Jahr, ref int IStufeWerk_Monat, ref int IStufeWerk_Nummer, ref int IStufeWerk_JJMMIII)
        {
            int num = 0;
            Logger.WriteInformation("IStufeWerkcalled");
            string text = SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeWerk") as string;
            if (string.IsNullOrEmpty(text))
            {
                text = SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleShortTest/AdditionalData/IStufeWerk") as string;
            }
            text = ((!string.IsNullOrEmpty(text)) ? text : "NV");
            string value = ((text == "NV") ? "-1" : text.Substring(5, 2));
            string value2 = ((text == "NV") ? "-1" : text.Substring(8, 2));
            string value3 = ((text == "NV") ? "-1" : text.Substring(11, 3));
            int num2 = Convert.ToInt32(value);
            int num3 = Convert.ToInt32(value2);
            int num4 = Convert.ToInt32(value3);
            IStufeWerk_String = text;
            IStufeWerk_Jahr = ((num2 == -1) ? (-1) : Convert.ToInt32(num2));
            IStufeWerk_Monat = ((num3 == -1) ? (-1) : Convert.ToInt32(num3));
            IStufeWerk_Nummer = ((num4 == -1) ? (-1) : Convert.ToInt32(num4));
            IStufeWerk_JJMMIII = ((num2 == -1 || num3 == -1 || num4 == -1) ? (-1) : Convert.ToInt32($"{num2:00}" + $"{num3:00}" + $"{num4:000}"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }

}
