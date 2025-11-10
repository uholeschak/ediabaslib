using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class Swt : ISwt
    {
        private readonly IList<ISwtEcu> ecus;
        public IEnumerable<ISwtEcu> Ecus => ecus;

        internal Swt()
        {
            ecus = new List<ISwtEcu>();
        }

        public ISwtApplication GetSwtApplication(int diagAddrAsInt, ISwtApplicationId swtApplicationId)
        {
            if (swtApplicationId == null)
            {
                return null;
            }

            return ecus.Where((ISwtEcu ecu) => ecu.EcuIdentifier != null && ecu.EcuIdentifier.DiagAddrAsInt == diagAddrAsInt).SelectMany((ISwtEcu ecu) => ecu.SwtApplications.Where((ISwtApplication swtApplication) => swtApplicationId.Equals(swtApplication.Id))).FirstOrDefault();
        }

        internal void AddEcu(ISwtEcu swtEcu)
        {
            ecus.Add(swtEcu);
        }
    }
}