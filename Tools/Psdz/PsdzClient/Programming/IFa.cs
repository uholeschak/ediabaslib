using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFa
    {
        IList<string> EWords { get; set; }

        string Entwicklungsbaureihe { get; set; }

        int FaVersion { get; set; }

        IList<string> HOWords { get; }

        string Lackcode { get; set; }

        string Polstercode { get; set; }

        IList<string> Salapas { get; set; }

        string Type { get; set; }

        string Zeitkriterium { get; set; }

        IFa Clone();

        //[AuthorAPIHidden]
        bool AreEqual(IFa vehicleOrder);
    }
}
