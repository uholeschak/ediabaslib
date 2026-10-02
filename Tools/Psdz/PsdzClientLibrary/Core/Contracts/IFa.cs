using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
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

        string FahrzeugKategorie { get; set; }

        string ControlClass { get; set; }

        IFa Clone();
        [AuthorAPIHidden]
        bool AreEqual(BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa vehicleOrder);
    }
}