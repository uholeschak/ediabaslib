using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    public class EcuTreeEcu : IEcuTreeEcu
    {
        public long ID_SG_ADR { get; set; }

        public long? ID_LIN_SLAVE_ADR { get; set; }

        public string VARIANT { get; set; }

        public string ECU_SGBD { get; set; }

        public string ECU_GRUPPE { get; set; }

        public string ECU_GROBNAME { get; set; }

        public bool IDENT_SUCCESSFULLY { get; set; }

        public BusType BUS { get; set; }

        public IEnumerable<BusType> SubBUS { get; }

        public IEcuTreeSvk Svk { get; }

        public int ECUTreeColumn { get; set; }

        public int ECUTreeRow { get; set; }

        public bool ECU_HAS_CONFIG_OVERRIDE { get; }

        public bool IsRoot()
        {
            BusType bUS = BUS;
            BusType busType = bUS;
            if (busType == BusType.ROOT || busType == BusType.VIRTUALROOT)
            {
                return true;
            }
            return false;
        }
    }
}