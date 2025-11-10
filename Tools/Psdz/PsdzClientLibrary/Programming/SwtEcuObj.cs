using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class SwtEcuObj : ISwtEcu
    {
        private readonly IList<ISwtApplication> swtApplications;
        public IEcuIdentifier EcuIdentifier { get; internal set; }
        public RootCertificateState RootCertificateState { get; internal set; }
        public SoftwareSigState SoftwareSigState { get; internal set; }
        public IEnumerable<ISwtApplication> SwtApplications => swtApplications;

        public SwtEcuObj()
        {
            swtApplications = new List<ISwtApplication>();
        }

        internal void AddApplication(ISwtApplication swtApplication)
        {
            swtApplications.Add(swtApplication);
        }
    }
}