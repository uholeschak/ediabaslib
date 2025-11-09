using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Incorrectly decompiled")]
    public class BasicFeaturesVci : typeBasicFeatures, IBasicFeatures, INotifyPropertyChanged
    {
        string IBasicFeatures.Baureihe => base.Baureihe;

        string IBasicFeatures.Ereihe => base.Ereihe;

        string IBasicFeatures.Getriebe => base.Getriebe;

        string IBasicFeatures.Karosserie => base.Karosserie;

        string IBasicFeatures.Land => base.Land;

        string IBasicFeatures.Lenkung => base.Lenkung;

        string IBasicFeatures.Marke => base.Marke;

        string IBasicFeatures.Modelljahr => base.Modelljahr;

        string IBasicFeatures.Modellmonat => base.Modellmonat;

        string IBasicFeatures.Motor => base.Motor;

        string IBasicFeatures.Prodart => base.Prodart;

        string IBasicFeatures.TypeCode => base.TypeCode;

        string IBasicFeatures.VerkaufsBezeichnung => base.VerkaufsBezeichnung;

        string IBasicFeatures.EMotBaureihe => base.EMotBaureihe;

        string IBasicFeatures.AEKurzbezeichnung => base.AEKurzbezeichnung;

        string IBasicFeatures.CountryOfAssembly => base.CountryOfAssembly;

        string IBasicFeatures.BaseVersion => base.BaseVersion;
    }
}
