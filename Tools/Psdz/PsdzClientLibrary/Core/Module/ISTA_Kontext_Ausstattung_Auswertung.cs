using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    public class ISTA_Kontext_Ausstattung_Auswertung : ISTAModule
    {
        public ISTA_Kontext_Ausstattung_Auswertung(ParameterContainer InParameter)
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

        public virtual void SA_Einzeln(string SA, ref bool SA_Vorhanden)
        {
            int num = 0;
            Logger.WriteInformation("SA_Einzelncalled");
            if (SA == null)
            {
                SA_Vorhanden = false;
            }
            else
            {
                List<string> list = null;
                ParameterContainer parameterContainer = new ParameterContainer();
                ParameterContainer parameterContainer2 = new ParameterContainer();
                ParameterContainer parameterContainer3 = new ParameterContainer();
                Factory.CreateServiceDialog(this, "SA_Einzeln", "69973561867", _globalTabModuleISTA, 2499, parameterContainer, parameterContainer3).Invoke("SA_Liste", parameterContainer, parameterContainer2, parameterContainer3);
                if (parameterContainer2.getParameter("SAs") != null)
                {
                    list = (List<string>)parameterContainer2.getParameter("SAs");
                }

                if (parameterContainer2.getParameter("SA_Anzahl") != null)
                {
                    _ = (int)parameterContainer2.getParameter("SA_Anzahl");
                }

                if (list.Contains(SA.ToUpper()))
                {
                    SA_Vorhanden = true;
                }
                else
                {
                    SA_Vorhanden = false;
                }
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void SA_Liste(List<string> SA_LISTE, ref bool SA_Vorhanden_Alle, ref int SA_Vorhanden_Anzahl, ref string SA_Vorhanden_String, ref List<string> SA_Vorhanden_Liste)
        {
            int num = 0;
            Logger.WriteInformation("SA_Listecalled");
            if (SA_LISTE == null)
            {
                SA_Vorhanden_Alle = false;
                SA_Vorhanden_Anzahl = -1;
                SA_Vorhanden_String = "NV";
                SA_Vorhanden_Liste = new List<string>();
                SA_Vorhanden_Liste.Add("NV");
            }
            else
            {
                List<string> list = null;
                ParameterContainer parameterContainer = new ParameterContainer();
                ParameterContainer parameterContainer2 = new ParameterContainer();
                ParameterContainer parameterContainer3 = new ParameterContainer();
                Factory.CreateServiceDialog(this, "SA_Liste", "69973561867", _globalTabModuleISTA, 3052, parameterContainer, parameterContainer3).Invoke("SA_Liste", parameterContainer, parameterContainer2, parameterContainer3);
                if (parameterContainer2.getParameter("SAs") != null)
                {
                    list = (List<string>)parameterContainer2.getParameter("SAs");
                }

                if (parameterContainer2.getParameter("SA_Anzahl") != null)
                {
                    _ = (int)parameterContainer2.getParameter("SA_Anzahl");
                }

                int num2 = 0;
                int num3 = 0;
                List<string> list2 = new List<string>();
                List<string> list3 = new List<string>();
                string text = "";
                list2.Clear();
                list3.Clear();
                while (num2 < SA_LISTE.Count)
                {
                    _DoLoopHandling = true;
                    if (list.Contains(SA_LISTE[num2].ToUpper()))
                    {
                        num3++;
                        list2.Add(SA_LISTE[num2].ToUpper());
                        list3.Add(SA_LISTE[num2].ToUpper());
                    }

                    num2++;
                    _DoLoopHandling = false;
                }

                list2.Sort();
                if (num3 == 0)
                {
                    SA_Vorhanden_Alle = false;
                    SA_Vorhanden_Anzahl = num3;
                    SA_Vorhanden_String = "NV";
                    SA_Vorhanden_Liste = new List<string>();
                    SA_Vorhanden_Liste.Add("NV");
                }
                else
                {
                    if (num3 < SA_LISTE.Count)
                    {
                        SA_Vorhanden_Alle = false;
                    }
                    else
                    {
                        SA_Vorhanden_Alle = true;
                    }

                    SA_Vorhanden_Anzahl = num3;
                    text += "<spe:TEXTITEM  xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:LIST>";
                    _DoLoopHandling = true;
                    for (int i = 0; i < list2.Count; i++)
                    {
                        text = text + "<spe:LISTENTRY>" + list3[i] + "</spe:LISTENTRY>";
                    }

                    _DoLoopHandling = false;
                    text += "</spe:LIST></spe:TEXTITEM>";
                    SA_Vorhanden_String = text;
                    SA_Vorhanden_Liste = list2;
                }
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}