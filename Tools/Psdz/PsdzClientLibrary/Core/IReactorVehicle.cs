using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.ComponentModel;
using System;

namespace PsdzClient.Core
{
    public interface IReactorVehicle : INotifyPropertyChanged
    {
        string Modelljahr { get; set; }

        string Modellmonat { get; set; }

        string Modelltag { get; set; }

        string VIN17 { get; set; }

        DateTime ProductionDate { get; set; }

        string Marke { get; set; }

        IReactorFa FA { get; set; }

        string Antrieb { get; set; }

        string Baureihe { get; set; }

        string Baureihenverbund { get; set; }

        string ElektrischeReichweite { get; set; }

        string Lenkung { get; set; }

        string AEBezeichnung { get; set; }

        string AEKurzbezeichnung { get; set; }

        string AELeistungsklasse { get; set; }

        string AEUeberarbeitung { get; set; }

        string BaustandsJahr { get; set; }

        string BaustandsMonat { get; set; }

        string Baustand { get; set; }

        BrandName? BrandName { get; set; }

        string Ereihe { get; set; }

        string Hybridkennzeichen { get; set; }

        string Karosserie { get; set; }

        string Land { get; set; }

        string BasicType { get; set; }

        string BaseVersion { get; set; }

        string CountryOfAssembly { get; set; }

        string Prodart { get; set; }

        string Produktlinie { get; set; }

        string Sicherheitsrelevant { get; set; }

        string Tueren { get; set; }

        string Typ { get; set; }

        string VerkaufsBezeichnung { get; set; }

        string Motorarbeitsverfahren { get; set; }

        string Motor { get; set; }

        string MOTBezeichnung { get; set; }

        string MOTEinbaulage { get; set; }

        string Hubraum { get; set; }

        string Kraftstoffart { get; set; }

        string Leistungsklasse { get; set; }

        string Ueberarbeitung { get; set; }

        string ILevelWerk { get; set; }

        string ILevel { get; set; }

        string Drehmoment { get; set; }

        EMotor EMotor { get; set; }

        string VINRangeType { get; set; }

        string Getriebe { get; set; }

        string KraftstoffartEinbaulage { get; set; }

        string SerialBodyShell { get; set; }

        string SerialEngine { get; set; }

        string SerialGearBox { get; set; }

        DateTime? FirstRegistration { get; set; }

        string ProgmanVersion { get; set; }

        string ECTypeApproval { get; set; }

        string TypeKey { get; set; }

        string TypeKeyLead { get; set; }

        string TypeKeyBasic { get; set; }

        string ESeriesLifeCycle { get; set; }

        string LifeCycle { get; set; }

        string Sportausfuehrung { get; set; }

        string F2Date { get; set; }
    }
}
