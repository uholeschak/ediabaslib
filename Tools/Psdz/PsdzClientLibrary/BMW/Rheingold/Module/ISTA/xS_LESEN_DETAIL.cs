using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.VehicleCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication;

namespace BMW.Rheingold.Module.ISTA
{
    internal class xS_LESEN_DETAIL : ISTAServiceDialog
    {
        public xS_LESEN_DETAIL(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }

            __handleInParameter();
        }

        public virtual void Prepare()
        {
            Logger.WriteInformation("Prepare called");
        }

        public virtual void Reset()
        {
            Logger.WriteInformation("Reset called");
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if ("FS_UW_LESEN".Equals(method))
            {
                string gruppe = inParam.getParameter("Gruppe", "") as string;
                string variante = inParam.getParameter("Variante", "") as string;
                int f_ORT = Convert.ToInt32(inParam.getParameter("F_ORT", 0));
                List<int> uW_NR = inParam.getParameter("UW_NR", null) as List<int>;
                List<string> UW_WERT_erstes_Auftreten = null;
                List<string> UW_WERT_letztes_Auftreten = null;
                FS_UW_LESEN(gruppe, variante, f_ORT, uW_NR, ref UW_WERT_erstes_Auftreten, ref UW_WERT_letztes_Auftreten);
                outParam.setParameter("UW_WERT_erstes_Auftreten", UW_WERT_erstes_Auftreten);
                outParam.setParameter("UW_WERT_letztes_Auftreten", UW_WERT_letztes_Auftreten);
                return;
            }

            if ("IS_UW_LESEN".Equals(method))
            {
                string gruppe2 = inParam.getParameter("Gruppe", "") as string;
                string variante2 = inParam.getParameter("Variante", "") as string;
                int f_ORT2 = Convert.ToInt32(inParam.getParameter("F_ORT", 0));
                List<int> uW_NR2 = inParam.getParameter("UW_NR", null) as List<int>;
                List<string> UW_WERT_erstes_Auftreten2 = null;
                List<string> UW_WERT_letztes_Auftreten2 = null;
                IS_UW_LESEN(gruppe2, variante2, f_ORT2, uW_NR2, ref UW_WERT_erstes_Auftreten2, ref UW_WERT_letztes_Auftreten2);
                outParam.setParameter("UW_WERT_erstes_Auftreten", UW_WERT_erstes_Auftreten2);
                outParam.setParameter("UW_WERT_letztes_Auftreten", UW_WERT_letztes_Auftreten2);
                return;
            }

            if ("HS_UW_LESEN".Equals(method))
            {
                string gruppe3 = inParam.getParameter("Gruppe", "") as string;
                string variante3 = inParam.getParameter("Variante", "") as string;
                int f_ORT3 = Convert.ToInt32(inParam.getParameter("F_ORT", 0));
                List<int> uW_NR3 = inParam.getParameter("UW_NR", null) as List<int>;
                List<string> UW_WERT_erstes_Auftreten3 = null;
                List<string> UW_WERT_letztes_Auftreten3 = null;
                HS_UW_LESEN(gruppe3, variante3, f_ORT3, uW_NR3, ref UW_WERT_erstes_Auftreten3, ref UW_WERT_letztes_Auftreten3);
                outParam.setParameter("UW_WERT_erstes_Auftreten", UW_WERT_erstes_Auftreten3);
                outParam.setParameter("UW_WERT_letztes_Auftreten", UW_WERT_letztes_Auftreten3);
                return;
            }

            if ("UW_LESEN".Equals(method))
            {
                string gruppe4 = inParam.getParameter("Gruppe", "") as string;
                string variante4 = inParam.getParameter("Variante", "") as string;
                int fS_IS_HS_LESEN_DETAIL = Convert.ToInt32(inParam.getParameter("FS_IS_HS_LESEN_DETAIL", 1));
                int f_ORT4 = Convert.ToInt32(inParam.getParameter("F_ORT", 0));
                List<int> uW_NR4 = inParam.getParameter("UW_NR", null) as List<int>;
                List<string> UW_WERT_erstes_Auftreten4 = null;
                List<string> UW_WERT_letztes_Auftreten4 = null;
                UW_LESEN(gruppe4, variante4, fS_IS_HS_LESEN_DETAIL, f_ORT4, uW_NR4, ref UW_WERT_erstes_Auftreten4, ref UW_WERT_letztes_Auftreten4);
                outParam.setParameter("UW_WERT_erstes_Auftreten", UW_WERT_erstes_Auftreten4);
                outParam.setParameter("UW_WERT_letztes_Auftreten", UW_WERT_letztes_Auftreten4);
                return;
            }

            throw new ServiceDialogMethodUnsupportedException(method);
        }

        public virtual void FS_UW_LESEN(string Gruppe, string Variante, int F_ORT, List<int> UW_NR, ref List<string> UW_WERT_erstes_Auftreten, ref List<string> UW_WERT_letztes_Auftreten)
        {
            int num = 0;
            Logger.WriteInformation("FS_UW_LESEN called");
            UW_LESEN(Gruppe, Variante, 1, F_ORT, UW_NR, ref UW_WERT_erstes_Auftreten, ref UW_WERT_letztes_Auftreten);
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IS_UW_LESEN(string Gruppe, string Variante, int F_ORT, List<int> UW_NR, ref List<string> UW_WERT_erstes_Auftreten, ref List<string> UW_WERT_letztes_Auftreten)
        {
            int num = 0;
            Logger.WriteInformation("IS_UW_LESEN called");
            UW_LESEN(Gruppe, Variante, 2, F_ORT, UW_NR, ref UW_WERT_erstes_Auftreten, ref UW_WERT_letztes_Auftreten);
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void HS_UW_LESEN(string Gruppe, string Variante, int F_ORT, List<int> UW_NR, ref List<string> UW_WERT_erstes_Auftreten, ref List<string> UW_WERT_letztes_Auftreten)
        {
            int num = 0;
            Logger.WriteInformation("HS_UW_LESEN called");
            UW_LESEN(Gruppe, Variante, 3, F_ORT, UW_NR, ref UW_WERT_erstes_Auftreten, ref UW_WERT_letztes_Auftreten);
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        private void UW_LESEN(string Gruppe, string Variante, int FS_IS_HS_LESEN_DETAIL, int F_ORT, List<int> UW_NR, ref List<string> UW_WERT_erstes_Auftreten, ref List<string> UW_WERT_letztes_Auftreten)
        {
            int num = 0;
            Logger.WriteInformation("UW_LESEN called");
            string job = "FS_LESEN_DETAIL";
            if (FS_IS_HS_LESEN_DETAIL == 2)
            {
                job = "IS_LESEN_DETAIL";
            }

            if (FS_IS_HS_LESEN_DETAIL == 3)
            {
                job = "HS_LESEN_DETAIL";
            }

            string param = F_ORT.ToString();
            IEcuJob ecuJob = EcuKom.ApiJob((!string.IsNullOrEmpty(Variante)) ? Variante : Gruppe, job, param);
            IDictionary<int, string> dictionary = new Dictionary<int, string>();
            IDictionary<string, string> dictionary2 = new Dictionary<string, string>();
            IDictionary<string, string> dictionary3 = new Dictionary<string, string>();
            if (ecuJob.IsOkay())
            {
                foreach (ECUResult item in ecuJob.JobResult.Where((IEcuResult x) => x.Name.StartsWith("F_UW")))
                {
                    if (item.Name.EndsWith("_NR"))
                    {
                        string value = item.Name.Replace("F_UW", string.Empty).Replace("_NR", string.Empty);
                        int key = Convert.ToInt32(item.Value);
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, value);
                        }
                    }

                    if (item.Name.EndsWith("_WERT"))
                    {
                        string key2 = item.Name.Replace("F_UW", string.Empty).Replace("_WERT", string.Empty);
                        if (!dictionary2.ContainsKey(key2))
                        {
                            dictionary2.Add(key2, item.Value.ToString());
                            dictionary3.Add(key2, item.Value.ToString());
                        }
                        else
                        {
                            dictionary3.Remove(key2);
                            dictionary3.Add(key2, item.Value.ToString());
                        }
                    }
                }
            }

            UW_WERT_erstes_Auftreten = new List<string>();
            UW_WERT_letztes_Auftreten = new List<string>();
            _DoLoopHandling = true;
            for (int num2 = 0; num2 < UW_NR.Count; num2++)
            {
                if (dictionary.ContainsKey(UW_NR[num2]) && dictionary2.ContainsKey(dictionary[UW_NR[num2]]))
                {
                    string text = null;
                    text = dictionary2[dictionary[UW_NR[num2]]];
                    UW_WERT_erstes_Auftreten.Add(text);
                    text = dictionary3[dictionary[UW_NR[num2]]];
                    UW_WERT_letztes_Auftreten.Add(text);
                }
                else
                {
                    UW_WERT_erstes_Auftreten.Add("");
                    UW_WERT_letztes_Auftreten.Add("");
                }
            }

            _DoLoopHandling = false;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}