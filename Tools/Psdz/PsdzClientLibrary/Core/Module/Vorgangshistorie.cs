using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using Org.BouncyCastle.Utilities.Collections;
using PsdzClient.Core.Container;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    internal class Vorgangshistorie : ISTAServiceDialog
    {
        public Vorgangshistorie(ParameterContainer InParameter)
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

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if ("Serviceprogramm".Equals(method))
            {
                string identifikator = inParam.getParameter("Identifikator", null) as string;
                bool In_Pruefplan = false;
                bool In_Trefferliste = false;
                int Anzahl_abgebrochen = 0;
                int Anzahl_durchgefuehrt = 0;
                List<string> Diagnosekode_Liste = null;
                Serviceprogramm(identifikator, ref In_Pruefplan, ref In_Trefferliste, ref Anzahl_abgebrochen, ref Anzahl_durchgefuehrt, ref Diagnosekode_Liste);
                outParam.setParameter("In_Pruefplan", In_Pruefplan);
                outParam.setParameter("In_Trefferliste", In_Trefferliste);
                outParam.setParameter("Anzahl_abgebrochen", Anzahl_abgebrochen);
                outParam.setParameter("Anzahl_durchgefuehrt", Anzahl_durchgefuehrt);
                outParam.setParameter("Diagnosekode_Liste", Diagnosekode_Liste);
                return;
            }
            throw new ServiceDialogMethodUnsupportedException(method);
        }

        [PreserveSource(Cleaned = true)]
        private void Serviceprogramm(string Identifikator, ref bool In_Pruefplan, ref bool In_Trefferliste, ref int Anzahl_abgebrochen, ref int Anzahl_durchgefuehrt, ref List<string> Diagnosekode_Liste)
        {
            int num = 0;
            Logger.WriteInformation("Serviceprogramm called");
            In_Pruefplan = false;
            List<string> list = new List<string>();
            Diagnosekode_Liste = list;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
