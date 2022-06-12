using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class SwtEcuObj : ISwtEcu
    {
        public SwtEcuObj()
        {
            this.swtApplications = new List<ISwtApplication>();
        }

        public IEcuIdentifier EcuIdentifier { get; internal set; }

        public RootCertificateState RootCertificateState { get; internal set; }

        public SoftwareSigState SoftwareSigState { get; internal set; }

        public IEnumerable<ISwtApplication> SwtApplications
        {
            get
            {
                return this.swtApplications;
            }
        }

        internal void AddApplication(ISwtApplication swtApplication)
        {
            this.swtApplications.Add(swtApplication);
        }

        private readonly IList<ISwtApplication> swtApplications;
    }
}
