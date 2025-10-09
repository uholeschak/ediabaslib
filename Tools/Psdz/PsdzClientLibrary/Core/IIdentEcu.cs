using System.Collections.Generic;

namespace PsdzClient.Core
{
    // ToDo: Check on update
    public interface IIdentEcu
    {
        string ProgrammingVariantName { get; set; }

        string VARIANTE { get; set; }

        string TITLE_ECUTREE { get; set; }

        string ECU_SGBD { get; set; }

        string ECU_GRUPPE { get; set; }

        long ID_SG_ADR { get; set; }

        string SERIENNUMMER { get; set; }

        string ECU_ADR { get; set; }

        string ECUTreeColor { get; }

        string ECUTitle { get; set; }

        //IXepEcuVariants XepEcuVariant { get; set; }

        //IXepEcuCliques XepEcuClique { get; set; }

        void FillEcuTitleTree(ISet<string> ecuShortName);
    }
}
