using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;

#pragma warning disable CS0649
namespace BMW.Rheingold.Module.ISTA
{
    internal class Typmerkmale_ausISTA_UX : ISTAModule
    {
        public string strDlgInfo;
        public bool bWriteLog;
        public int m_maxAnzahlSonderausstattungen;
        public string[] Typmerkmale;
        public string[] Sonderausstattungen;
        public Typmerkmale_ausISTA_UX(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }

            __handleInParameter();
            strDlgInfo = "14.07.2009-Typmerkmale_ausISTA_UX";
            bWriteLog = true;
            m_maxAnzahlSonderausstattungen = 100;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void InitializeDialog(ref string[] Typmerkmale, ref string[] Sonderausstattungen)
        {
            int num = 0;
            Logger.WriteInformation("InitializeDialogcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation(strDlgInfo);
            }

            Typmerkmale = new string[20];
            _DoLoopHandling = true;
            for (int i = 0; i < Typmerkmale.Length; i++)
            {
                Typmerkmale[i] = "";
            }

            _DoLoopHandling = false;
            Sonderausstattungen = new string[m_maxAnzahlSonderausstattungen];
            _DoLoopHandling = true;
            for (int j = 0; j < Sonderausstattungen.Length; j++)
            {
                Sonderausstattungen[j] = "";
            }

            _DoLoopHandling = false;
            Typmerkmale[0] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/VehicleIdentificationNumber"));
            Typmerkmale[1] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Marke/Title"));
            Typmerkmale[2] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baureihe/Title"));
            Typmerkmale[3] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/EBezeichnung/Title"));
            Typmerkmale[4] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Verkaufsbezeichnung/Title"));
            Typmerkmale[5] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Laenderausfuehrung/Title"));
            Typmerkmale[6] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baujahr/Title"));
            Typmerkmale[7] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baumonat/Title"));
            Typmerkmale[8] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Motor/Title"));
            Typmerkmale[9] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Kraftstoffart/Title"));
            Typmerkmale[10] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Getriebe/Title"));
            Typmerkmale[11] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Typschluessel/Title"));
            Typmerkmale[12] = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeWerk"));
            Typmerkmale[13] = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeHO"));
            Typmerkmale[14] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Sicherheitsrelevant/Title"));
            string text = "";
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            text = "/ExternalData/VehicleIdentification/AdditionalData/Sonderausstattungen";
            if (SOCAccessor.OrderContext.System.GetProperty(text)is List<string> list)
            {
                _DoLoopHandling = true;
                for (int k = 0; k < list.Count; k++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list[k];
                    }

                    num2++;
                    num3++;
                }

                _DoLoopHandling = false;
            }

            text = "/ExternalData/VehicleIdentification/AdditionalData/E_Worte";
            if (SOCAccessor.OrderContext.System.GetProperty(text)is List<string> list2)
            {
                _DoLoopHandling = true;
                for (int l = 0; l < list2.Count; l++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list2[l];
                    }

                    num2++;
                    num4++;
                }

                _DoLoopHandling = false;
            }

            text = "/ExternalData/VehicleIdentification/AdditionalData/K_Worte";
            if (SOCAccessor.OrderContext.System.GetProperty(text)is List<string> list3)
            {
                _DoLoopHandling = true;
                for (int m = 0; m < list3.Count; m++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list3[m];
                    }

                    num2++;
                    num5++;
                }

                _DoLoopHandling = false;
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Fahrgestellnummer(ref string PFahrgestellnummer)
        {
            int num = 0;
            Logger.WriteInformation("Fahrgestellnummercalled");
            PFahrgestellnummer = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/VehicleIdentificationNumber"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Marke(ref string PMarke)
        {
            int num = 0;
            Logger.WriteInformation("Markecalled");
            PMarke = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Marke/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baureihe(ref string PBaureihe)
        {
            int num = 0;
            Logger.WriteInformation("Baureihecalled");
            PBaureihe = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baureihe/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void EBezeichnung(ref string PEBezeichnung)
        {
            int num = 0;
            Logger.WriteInformation("EBezeichnungcalled");
            PEBezeichnung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/EBezeichnung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Verkaufsbezeichnung(ref string PVerkaufsbezeichnung)
        {
            int num = 0;
            Logger.WriteInformation("Verkaufsbezeichnungcalled");
            PVerkaufsbezeichnung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Verkaufsbezeichnung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Länderausführung(ref string PLänderausführung)
        {
            int num = 0;
            Logger.WriteInformation("Länderausführungcalled");
            PLänderausführung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Laenderausfuehrung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baujahr(ref string PBaujahr)
        {
            int num = 0;
            Logger.WriteInformation("Baujahrcalled");
            PBaujahr = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baujahr/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baumonat(ref string PBaumonat)
        {
            int num = 0;
            Logger.WriteInformation("Baumonatcalled");
            PBaumonat = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baumonat/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Typschlüssel(ref string PTypschlüssel)
        {
            int num = 0;
            Logger.WriteInformation("Typschlüsselcalled");
            PTypschlüssel = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Typschluessel/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IStufeHO(ref string PIStufeHO)
        {
            int num = 0;
            Logger.WriteInformation("IStufeHOcalled");
            PIStufeHO = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeHO"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IStufeWerk(ref string PIStufeWerk)
        {
            int num = 0;
            Logger.WriteInformation("IStufeWerkcalled");
            PIStufeWerk = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeWerk"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Sicherheitsrelevant(ref string PSicherheitsrelevant)
        {
            int num = 0;
            Logger.WriteInformation("Sicherheitsrelevantcalled");
            PSicherheitsrelevant = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Sicherheitsrelevant/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
