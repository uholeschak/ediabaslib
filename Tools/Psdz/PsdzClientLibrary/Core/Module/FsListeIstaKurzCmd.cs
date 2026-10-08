using System;
using BMW.Rheingold.CoreFramework;
using System.Collections.Generic;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FsListeIstaKurzCmd : ServiceDialogCmdBase
    {
        public FsListeIstaKurzCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("FsListeIstaKurzCmd.CreateDialog()", $"{ServiceDialogConfig.Name} init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("FsListeIstaKurzCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            Log.Info("FsListeIstaKurzCmd.DoInvoke()", "FS_LISTE_ISTA_KURZ");
            CallingModule.GlobalTabModuleISTA.IsWaitCursor();
            try
            {
                if (CallingModule.__RheinGoldCoreModuleParameters__ != null)
                {
                    ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
                    inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
                }

                inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
                inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
                switch (method)
                {
                    case "InitializeDialog":
                    {
                        string[] array = null;
                        string[] array2 = null;
                        string[] array3 = null;
                        int[] array4 = null;
                        int Anzahl_Fehlerspeicher2 = 0;
                        array2 = new string[300];
                        array4 = new int[300];
                        array = new string[300];
                        array3 = new string[300];
                        array4 = new int[300];
                        new FS_LISTE_ISTA_KURZ(inParam).InitializeDialog(ref array2, ref Anzahl_Fehlerspeicher2, ref array4, ref array, ref array3);
                        inoutParam.setParameter("Fehlerkode_Text", array2);
                        inoutParam.setParameter("Anzahl_Fehlerspeicher", Anzahl_Fehlerspeicher2);
                        inoutParam.setParameter("Fehlerkode_dez", array4);
                        inoutParam.setParameter("Fehlerkode_hex", array);
                        inoutParam.setParameter("Fehlerkode_SGBD", array3);
                        return;
                    }

                    case "LeseAnzahlFehlerspeicher":
                    {
                        int Anzahl_Fehlerspeicher = 0;
                        new FS_LISTE_ISTA_KURZ(inParam).LeseAnzahlFehlerspeicher(ref Anzahl_Fehlerspeicher);
                        outParam.setParameter("Anzahl_Fehlerspeicher", Anzahl_Fehlerspeicher);
                        return;
                    }

                    case "FSPvorhanden":
                    {
                        string SGGruppe = inoutParam.getParameter("SGGruppe", null) as string;
                        string SGVariante = inoutParam.getParameter("SGVariante", null) as string;
                        string FType = inoutParam.getParameter("FType", null) as string;
                        int FehlerCode = (int)inoutParam.getParameter("FehlerCode", 0);
                        bool isVorhanden = (bool)inoutParam.getParameter("isVorhanden", false);
                        new FS_LISTE_ISTA_KURZ(inParam).FSPvorhanden(ref SGGruppe, ref SGVariante, ref FType, ref FehlerCode, ref isVorhanden);
                        inoutParam.setParameter("SGGruppe", SGGruppe);
                        inoutParam.setParameter("SGVariante", SGVariante);
                        inoutParam.setParameter("FType", FType);
                        inoutParam.setParameter("FehlerCode", FehlerCode);
                        outParam.setParameter("isVorhanden", isVorhanden);
                        return;
                    }

                    case "FSPvorhanden_pruefen":
                    {
                        bool fSPvorhanden = (bool)inoutParam.getParameter("FSPvorhanden", false);
                        string SGgruppe = inoutParam.getParameter("SGgruppe", null) as string;
                        string SGvariante = inoutParam.getParameter("SGvariante", null) as string;
                        string Fehlerart = inoutParam.getParameter("Fehlerart", null) as string;
                        int Fehlercode = (int)inoutParam.getParameter("Fehlercode", 0);
                        new FS_LISTE_ISTA_KURZ(inParam).FSPvorhanden_pruefen(fSPvorhanden, ref SGgruppe, ref SGvariante, ref Fehlerart, ref Fehlercode);
                        inoutParam.setParameter("SGgruppe", SGgruppe);
                        inoutParam.setParameter("SGvariante", SGvariante);
                        inoutParam.setParameter("Fehlerart", Fehlerart);
                        inoutParam.setParameter("FehlerCode", Fehlercode);
                        return;
                    }
                }

                if ("AnzahlFehlerspeicher".Equals(method))
                {
                    int anzahlFehlerspeicherGesamt = 0;
                    int anzahlFehlerkodes = 0;
                    int anzahlVirtuelleFehlerkodes = 0;
                    int anzahlSammelfehlerkodes = 0;
                    new FS_LISTE_ISTA_KURZ(inParam).AnzahlFehlerspeicher(ref anzahlFehlerspeicherGesamt, ref anzahlFehlerkodes, ref anzahlVirtuelleFehlerkodes, ref anzahlSammelfehlerkodes);
                    outParam.setParameter("anzahlFehlerspeicherGesamt", anzahlFehlerspeicherGesamt);
                    outParam.setParameter("anzahlFehlerkodes", anzahlFehlerkodes);
                    outParam.setParameter("anzahlVirtuelleFehlerkodes", anzahlVirtuelleFehlerkodes);
                    outParam.setParameter("anzahlSammelfehlerkodes", anzahlSammelfehlerkodes);
                    return;
                }

                if ("Fehlerkodes".Equals(method) || "VirtuelleFehlerkodes".Equals(method) || "Sammelfehlerkodes".Equals(method))
                {
                    int anzahl_Fehlerspeicher = 0;
                    List<string> fehlerkode_NodeID = null;
                    List<string> fehlerkode_Text = null;
                    List<int> fehlerkode_dez = null;
                    List<string> fehlerkode_hex = null;
                    List<string> fehlerkode_SGBD = null;
                    FS_LISTE_ISTA_KURZ fS_LISTE_ISTA_KURZ = new FS_LISTE_ISTA_KURZ(inParam);
                    if ("Fehlerkodes".Equals(method))
                    {
                        fS_LISTE_ISTA_KURZ.Fehlerkodes(ref anzahl_Fehlerspeicher, ref fehlerkode_NodeID, ref fehlerkode_Text, ref fehlerkode_dez, ref fehlerkode_hex, ref fehlerkode_SGBD);
                    }
                    else if ("VirtuelleFehlerkodes".Equals(method))
                    {
                        fS_LISTE_ISTA_KURZ.VirtuelleFehlerkodes(ref anzahl_Fehlerspeicher, ref fehlerkode_NodeID, ref fehlerkode_Text, ref fehlerkode_dez, ref fehlerkode_hex, ref fehlerkode_SGBD);
                    }
                    else if ("Sammelfehlerkodes".Equals(method))
                    {
                        fS_LISTE_ISTA_KURZ.Sammelfehlerkodes(ref anzahl_Fehlerspeicher, ref fehlerkode_NodeID, ref fehlerkode_Text, ref fehlerkode_dez, ref fehlerkode_hex, ref fehlerkode_SGBD);
                    }

                    outParam.setParameter("anzahl_Fehlerspeicher", anzahl_Fehlerspeicher);
                    outParam.setParameter("fehlerkode_NodeID", fehlerkode_NodeID);
                    outParam.setParameter("fehlerkode_Text", fehlerkode_Text);
                    outParam.setParameter("fehlerkode_dez", fehlerkode_dez);
                    outParam.setParameter("fehlerkode_hex", fehlerkode_hex);
                    outParam.setParameter("fehlerkode_SGBD", fehlerkode_SGBD);
                    return;
                }

                throw new ServiceDialogMethodUnsupportedException();
            }
            catch (ServiceDialogMethodUnsupportedException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Log.WarningException("FsListeIstaKurzCmd.DoInvoke()", exception);
            }
        }
    }
}
