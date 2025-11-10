using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Programming.API
{
    public class VehicleOrder : BMW.Rheingold.CoreFramework.Contracts.Programming.IFa
    {
        public IList<string> EWords { get; set; }
        public string Entwicklungsbaureihe { get; set; }
        public int FaVersion { get; set; }
        public IList<string> HOWords { get; set; }
        public string Lackcode { get; set; }
        public string Polstercode { get; set; }
        public IList<string> Salapas { get; set; }
        public string Type { get; set; }
        public string Zeitkriterium { get; set; }
        public string FahrzeugKategorie { get; set; }
        public string ControlClass { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}#{1}*{2}%{3}&{4}{5}{6}{7}", Entwicklungsbaureihe, Zeitkriterium, Type, Lackcode, Polstercode, FA.ConcatStrElems(Salapas, "$"), FA.ConcatStrElems(EWords, "-"), FA.ConcatStrElems(HOWords, "+"));
        }

        public bool AreEqual(BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa vehicleOrder)
        {
            if (!Equals(Entwicklungsbaureihe, vehicleOrder.BR))
            {
                return false;
            }

            if (!Equals(Lackcode, vehicleOrder.LACK))
            {
                return false;
            }

            if (!Equals(Polstercode, vehicleOrder.POLSTER))
            {
                return false;
            }

            if (!Equals(Type, vehicleOrder.TYPE))
            {
                return false;
            }

            if (!Equals(Zeitkriterium, vehicleOrder.C_DATE))
            {
                return false;
            }

            if (!Equals(Salapas, vehicleOrder.SA))
            {
                return false;
            }

            if (!Equals(EWords, vehicleOrder.E_WORT))
            {
                return false;
            }

            if (!Equals(HOWords, vehicleOrder.HO_WORT))
            {
                return false;
            }

            return true;
        }

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa Clone()
        {
            return new VehicleOrder
            {
                Entwicklungsbaureihe = Entwicklungsbaureihe,
                EWords = new List<string>(EWords),
                FaVersion = FaVersion,
                HOWords = new List<string>(HOWords),
                Lackcode = Lackcode,
                Polstercode = Polstercode,
                Salapas = new List<string>(Salapas),
                Type = Type,
                Zeitkriterium = Zeitkriterium
            };
        }

        private bool Equals(string a, string b)
        {
            return a?.Equals(b) ?? (b == null);
        }

        private bool Equals(IEnumerable<string> a, IEnumerable<string> b)
        {
            if (a == null)
            {
                return b == null;
            }

            if (b == null)
            {
                return false;
            }

            List<string> list = new List<string>(a);
            int num = 0;
            foreach (string item in b)
            {
                if (!list.Contains(item))
                {
                    return false;
                }

                num++;
            }

            return num == list.Count;
        }
    }
}