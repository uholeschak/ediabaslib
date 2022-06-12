using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class Swt : ISwt
    {
        internal Swt()
        {
            this.ecus = new List<ISwtEcu>();
        }

        public IEnumerable<ISwtEcu> Ecus
        {
            get
            {
                return this.ecus;
            }
        }

        public ISwtApplication GetSwtApplication(int diagAddrAsInt, ISwtApplicationId swtApplicationId)
        {
            if (swtApplicationId == null)
            {
                return null;
            }
            return (from ecu in this.ecus
                where ecu.EcuIdentifier != null && ecu.EcuIdentifier.DiagAddrAsInt == diagAddrAsInt
                select ecu).SelectMany(delegate (ISwtEcu ecu)
            {
                IEnumerable<ISwtApplication> swtApplications = ecu.SwtApplications;
                Func<ISwtApplication, bool> predicate = ((ISwtApplication swtApplication) => swtApplicationId.Equals(swtApplication.Id));
                return swtApplications.Where(predicate);
            }).FirstOrDefault<ISwtApplication>();
        }

        internal void AddEcu(ISwtEcu swtEcu)
        {
            this.ecus.Add(swtEcu);
        }

        private readonly IList<ISwtEcu> ecus;
    }
}
