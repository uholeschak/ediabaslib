using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IBasicFeatures : INotifyPropertyChanged
    {
        string Baureihe { get; }

        string Ereihe { get; }

        string Getriebe { get; }

        string Karosserie { get; }

        string Land { get; }

        string Lenkung { get; }

        string Marke { get; }

        string Modelljahr { get; }

        string Modellmonat { get; }

        string Motor { get; }

        string Prodart { get; }

        string TypeCode { get; }

        string VerkaufsBezeichnung { get; }

        string EMotBaureihe { get; }

        string AEKurzbezeichnung { get; }

        string CountryOfAssembly { get; }

        string BaseVersion { get; }
    }
}
