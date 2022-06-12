using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class SystemVerbauTabelle : ISvt
    {
        public IEnumerable<IEcuObj> Ecus
        {
            get
            {
                return this.ecus;
            }
        }

        public byte[] HoSignature { get; internal set; }

        public DateTime HoSignatureDate { get; internal set; }

        public int Version { get; internal set; }

        public bool RemoveEcu(IEcuObj ecu)
        {
            return this.ecus.Remove(ecu);
        }

        internal void AddEcu(IEcuObj ecu)
        {
            this.ecus.Add(ecu);
        }

        private readonly IList<IEcuObj> ecus = new List<IEcuObj>();
    }
}
