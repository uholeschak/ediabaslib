using System;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class Typmerkmal_ausISTA_UXCmd : ServiceDialogCmdBase
    {
        public Typmerkmal_ausISTA_UXCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("Typmerkmal_ausISTA_UXCmd.CreateDialog()", $"{ServiceDialogConfig.Name} init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            try
            {
                if (CallingModule == null)
                {
                    Log.Error("Typmerkmal_ausISTA_UXCmd.Invoke()", "Failed to invoke method {0}, because calling module is null.", method);
                    return;
                }

                ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
                inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
                inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
                inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
                Typmerkmale_ausISTA_UX typmerkmale_ausISTA_UX = new Typmerkmale_ausISTA_UX(inParam);
                if ("InitializeDialog".Equals(method))
                {
                    string[] Typmerkmale = null;
                    string[] Sonderausstattungen = null;
                    typmerkmale_ausISTA_UX.InitializeDialog(ref Typmerkmale, ref Sonderausstattungen);
                    outParam.setParameter("Typmerkmale", Typmerkmale);
                    outParam.setParameter("Sonderausstattungen", Sonderausstattungen);
                    return;
                }

                if ("Fahrgestellnummer".Equals(method))
                {
                    string PFahrgestellnummer = null;
                    typmerkmale_ausISTA_UX.Fahrgestellnummer(ref PFahrgestellnummer);
                    outParam.setParameter("PFahrgestellnummer", PFahrgestellnummer);
                    return;
                }

                if ("Marke".Equals(method))
                {
                    string PMarke = null;
                    typmerkmale_ausISTA_UX.Marke(ref PMarke);
                    outParam.setParameter("PMarke", PMarke);
                    return;
                }

                if ("Baureihe".Equals(method))
                {
                    string PBaureihe = null;
                    typmerkmale_ausISTA_UX.Baureihe(ref PBaureihe);
                    outParam.setParameter("PBaureihe", PBaureihe);
                    return;
                }

                if ("EBezeichnung".Equals(method))
                {
                    string PEBezeichnung = null;
                    typmerkmale_ausISTA_UX.EBezeichnung(ref PEBezeichnung);
                    outParam.setParameter("PEBezeichnung", PEBezeichnung);
                    return;
                }

                if ("Verkaufsbezeichnung".Equals(method))
                {
                    string PVerkaufsbezeichnung = null;
                    typmerkmale_ausISTA_UX.Verkaufsbezeichnung(ref PVerkaufsbezeichnung);
                    outParam.setParameter("PVerkaufsbezeichnung", PVerkaufsbezeichnung);
                    return;
                }

                if ("Länderausführung".Equals(method))
                {
                    string PLänderausführung = null;
                    typmerkmale_ausISTA_UX.Länderausführung(ref PLänderausführung);
                    outParam.setParameter("PLänderausführung", PLänderausführung);
                    return;
                }

                if ("Baujahr".Equals(method))
                {
                    string PBaujahr = null;
                    typmerkmale_ausISTA_UX.Baujahr(ref PBaujahr);
                    outParam.setParameter("PBaujahr", PBaujahr);
                    return;
                }

                if ("Baumonat".Equals(method))
                {
                    string PBaumonat = null;
                    typmerkmale_ausISTA_UX.Baumonat(ref PBaumonat);
                    outParam.setParameter("PBaumonat", PBaumonat);
                    return;
                }

                if ("Typschlüssel".Equals(method))
                {
                    string PTypschlüssel = null;
                    typmerkmale_ausISTA_UX.Typschlüssel(ref PTypschlüssel);
                    outParam.setParameter("PTypschlüssel", PTypschlüssel);
                    return;
                }

                if ("IStufeHO".Equals(method))
                {
                    string PIStufeHO = null;
                    typmerkmale_ausISTA_UX.IStufeHO(ref PIStufeHO);
                    outParam.setParameter("PIStufeHO", PIStufeHO);
                    return;
                }

                if ("IStufeWerk".Equals(method))
                {
                    string PIStufeWerk = null;
                    typmerkmale_ausISTA_UX.IStufeWerk(ref PIStufeWerk);
                    outParam.setParameter("PIStufeWerk", PIStufeWerk);
                    return;
                }

                if ("Sicherheitsrelevant".Equals(method))
                {
                    string PSicherheitsrelevant = null;
                    typmerkmale_ausISTA_UX.Sicherheitsrelevant(ref PSicherheitsrelevant);
                    outParam.setParameter("PSicherheitsrelevant", PSicherheitsrelevant);
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
                Log.WarningException("Typmerkmal_ausISTA_UXCmd.Invoke()", exception);
            }
        }
    }
}