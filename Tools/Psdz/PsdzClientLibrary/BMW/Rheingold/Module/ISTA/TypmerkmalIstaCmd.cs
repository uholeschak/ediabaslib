using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMW.Rheingold.Module.ISTA
{
    internal class TypmerkmalIstaCmd : ServiceDialogCmdBase
    {
        public TypmerkmalIstaCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("TypmerkmalIstaCmd.CreateDialog()", "TYPMERKMAL_ISTA init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("TypmerkmalIstaCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            switch (method)
            {
                case "Antrieb":
                {
                    string PAntrieb = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Antrieb(ref PAntrieb);
                    outParam.setParameter("PAntrieb", PAntrieb);
                    break;
                }

                case "Baujahr":
                {
                    string PBaujahr = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Baujahr(ref PBaujahr);
                    outParam.setParameter("PBaujahr", PBaujahr);
                    break;
                }

                case "Baumonat":
                {
                    string PBaumonat = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Baumonat(ref PBaumonat);
                    outParam.setParameter("PBaumonat", PBaumonat);
                    break;
                }

                case "Baureihe":
                {
                    string PBaureihe = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Baureihe(ref PBaureihe);
                    outParam.setParameter("PBaureihe", PBaureihe);
                    break;
                }

                case "EBezeichnung":
                {
                    string PEBezeichnung = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).EBezeichnung(ref PEBezeichnung);
                    outParam.setParameter("PEBezeichnung", PEBezeichnung);
                    break;
                }

                case "Fahrgestellnummer":
                {
                    string PFahrgestellnummer = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Fahrgestellnummer(ref PFahrgestellnummer);
                    outParam.setParameter("PFahrgestellnummer", PFahrgestellnummer);
                    break;
                }

                case "Marke":
                {
                    string PMarke = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Marke(ref PMarke);
                    outParam.setParameter("PMarke", PMarke);
                    break;
                }

                case "Motor":
                {
                    string PMotor = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Motor(ref PMotor);
                    outParam.setParameter("PMotor", PMotor);
                    break;
                }

                case "Kraftstoff":
                {
                    string PKraftstoff = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Kraftstoff(ref PKraftstoff);
                    outParam.setParameter("PKraftstoff", PKraftstoff);
                    break;
                }

                case "Getriebe":
                {
                    string PGetriebe = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Getriebe(ref PGetriebe);
                    outParam.setParameter("PGetriebe", PGetriebe);
                    break;
                }

                case "Hubraum":
                {
                    string PHubraum = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Hubraum(ref PHubraum);
                    outParam.setParameter("PHubraum", PHubraum);
                    break;
                }

                case "InitializeDialog":
                {
                    string[] Typmerkmale = null;
                    string[] Sonderausstattungen = null;
                    if (inoutParam.getParameter("Typmerkmale") != null)
                    {
                        Typmerkmale = (string[])inoutParam.getParameter("Typmerkmale");
                    }

                    if (inoutParam.getParameter("Sonderausstattungen") != null)
                    {
                        Sonderausstattungen = (string[])inoutParam.getParameter("Sonderausstattungen");
                    }

                    new TYPMERKMAL_ISTA(inParam).InitializeDialog(ref Typmerkmale, ref Sonderausstattungen);
                    int num = 0;
                    StringBuilder sb = new StringBuilder();
                    if (Typmerkmale != null)
                    {
                        string[] array = Typmerkmale;
                        foreach (string text in array)
                        {
                            sb.AppendLine($"Typenmerkmale[{num}]: {text}");
                            Log.Info("TypmerkmalIstaCmd.DoInvoke()", "TYPMERKMAL_ISTA Typenmerkmale[{0}]: {1}", num, text);
                            num++;
                        }
                    }

                    if (Sonderausstattungen != null)
                    {
                        num = 0;
                        string[] array = Sonderausstattungen;
                        foreach (string text2 in array)
                        {
                            sb.AppendLine($"SA[{num++}]: {text2}");
                            Log.Info("TypmerkmalIstaCmd.DoInvoke()", "TYPMERKMAL_ISTA Sonderausstattung[{0}]: {1}", num, text2);
                            num++;
                        }
                    }

                    if (FastaProtocoler != null)
                    {
                        IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram("TYPMERKMAL_ISTA", method);
                        List<LocalizedText> list = new List<LocalizedText>();
                        list.AddRange(CallingModule.logic.Lang.Select((string x) => new LocalizedText("n/a", x)));
                        action.SpecialAction.CreateAndAddMessageText(list);
                        List<LocalizedText> list2 = new List<LocalizedText>();
                        list2.AddRange(CallingModule.logic.Lang.Select((string x) => new LocalizedText(sb.ToString(), x)));
                        action.SpecialAction.Display = false;
                        action.SpecialAction.AddAnswer(list2, null);
                    }
                    else
                    {
                        Log.Warning("TypmerkmalIstaCmd.DoInvoke()", "No FASTA available.");
                    }

                    inoutParam.Parameter.Add("Typmerkmale", Typmerkmale);
                    inoutParam.Parameter.Add("Sonderausstattungen", Sonderausstattungen);
                    break;
                }

                case "IStufeHO":
                {
                    string PIStufeHO = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).IStufeHO(ref PIStufeHO);
                    outParam.setParameter("PIStufeHO", PIStufeHO);
                    break;
                }

                case "IStufeWerk":
                {
                    string PIStufeWerk = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).IStufeWerk(ref PIStufeWerk);
                    outParam.setParameter("PIStufeWerk", PIStufeWerk);
                    break;
                }

                case "Karosserieform":
                {
                    string PKarosserieform = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Karosserieform(ref PKarosserieform);
                    outParam.setParameter("PKarosserieform", PKarosserieform);
                    break;
                }

                case "Laenderausfuehrung":
                {
                    string PLaenderausfuehrung = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Laenderausfuehrung(ref PLaenderausfuehrung);
                    outParam.setParameter("PLaenderausfuehrung", PLaenderausfuehrung);
                    break;
                }

                case "Lenkung":
                {
                    string PLenkung = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Lenkung(ref PLenkung);
                    outParam.setParameter("PLenkung", PLenkung);
                    break;
                }

                case "Sicherheitsrelevant":
                {
                    string PSicherheitsrelevant = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Sicherheitsrelevant(ref PSicherheitsrelevant);
                    outParam.setParameter("PSicherheitsrelevant", PSicherheitsrelevant);
                    break;
                }

                case "Tueren":
                {
                    string PTueren = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Tueren(ref PTueren);
                    outParam.setParameter("PTueren", PTueren);
                    break;
                }

                case "Produktionsdatum":
                {
                    string PProduktionsdatum = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Produktionsdatum(ref PProduktionsdatum);
                    outParam.setParameter("PProduktionsdatum", PProduktionsdatum);
                    break;
                }

                case "SteuergeraeteListe":
                {
                    string[] SG_Gruppen = null;
                    string[] SG_Varianten = null;
                    int[] SG_Adressen = null;
                    new TYPMERKMAL_ISTA(inParam).SteuergeraeteListe(ref SG_Gruppen, ref SG_Varianten, ref SG_Adressen);
                    outParam.setParameter("SG_Gruppen", SG_Gruppen);
                    outParam.setParameter("SG_Varianten", SG_Varianten);
                    outParam.setParameter("SG_Adressen", SG_Adressen);
                    break;
                }

                case "Verkaufsbezeichnung":
                {
                    string PVerkaufsbezeichnung = string.Empty;
                    new TYPMERKMAL_ISTA(inParam).Verkaufsbezeichnung(ref PVerkaufsbezeichnung);
                    outParam.setParameter("PVerkaufsbezeichnung", PVerkaufsbezeichnung);
                    break;
                }

                case "SollverbauGruppenliste":
                {
                    string[] Gruppenliste = null;
                    new TYPMERKMAL_ISTA(inParam).SollverbauGruppenliste(ref Gruppenliste);
                    outParam.setParameter("Gruppenliste", Gruppenliste);
                    break;
                }

                default:
                    Log.Error("TypmerkmalIstaCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
                    break;
            }
        }
    }
}