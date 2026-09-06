using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class VirtualFaultInfo
    {
        private ECU affectedECU;

        private IXepInfoObject serviceProgram;

        public ECU AffectedECU
        {
            get
            {
                return affectedECU;
            }
            set
            {
                if (affectedECU != value)
                {
                    affectedECU = value;
                }
            }
        }

        public IXepInfoObject ServiceProgram
        {
            get
            {
                return serviceProgram;
            }
            set
            {
                if (serviceProgram != value)
                {
                    serviceProgram = value;
                }
            }
        }

        public VirtualFaultInfo(ECU affectedECU, IXepInfoObject serviceProgram)
        {
            this.affectedECU = affectedECU;
            this.serviceProgram = serviceProgram;
        }
    }
}
