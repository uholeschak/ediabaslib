using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class SystemVerbauTabelle : ISvt
    {
        private readonly IList<IEcuObj> ecus = new List<IEcuObj>();
        public IEnumerable<IEcuObj> Ecus => ecus;
        public byte[] HoSignature { get; internal set; }
        public DateTime HoSignatureDate { get; internal set; }
        public int Version { get; internal set; }

        public bool RemoveEcu(IEcuObj ecu)
        {
            return ecus.Remove(ecu);
        }

        internal void AddEcu(IEcuObj ecu)
        {
            ecus.Add(ecu);
        }
    }
}