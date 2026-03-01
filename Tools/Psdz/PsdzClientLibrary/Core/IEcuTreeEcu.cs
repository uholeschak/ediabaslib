using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    public interface IEcuTreeEcu
    {
        long ID_SG_ADR { get; set; }

        long? ID_LIN_SLAVE_ADR { get; set; }

        string VARIANT { get; set; }

        string ECU_SGBD { get; set; }

        string ECU_GRUPPE { get; set; }

        string ECU_GROBNAME { get; set; }

        bool IDENT_SUCCESSFULLY { get; set; }

        BusType BUS { get; set; }

        IEnumerable<BusType> SubBUS { get; }

        IEcuTreeSvk Svk { get; }

        int ECUTreeColumn { get; set; }

        int ECUTreeRow { get; set; }

        bool ECU_HAS_CONFIG_OVERRIDE { get; }

        bool IsRoot();
    }
}