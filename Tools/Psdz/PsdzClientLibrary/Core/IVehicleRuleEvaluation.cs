using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.Collections.Generic;
using System;

namespace PsdzClient.Core
{
    public interface IVehicleRuleEvaluation
    {
        string AEBezeichnung { get; }

        string AEKurzbezeichnung { get; }

        string AELeistungsklasse { get; }

        string AEUeberarbeitung { get; }

        string KraftstoffartEinbaulage { get; }

        string Antrieb { get; set; }

        string BaseVersion { get; }

        string BasicType { get; }

        string Baureihe { get; set; }

        string Baureihenverbund { get; set; }

        string Marke { get; }

        string CountryOfAssembly { get; }

        string Drehmoment { get; }

        string ElektrischeReichweite { get; }

        string Ereihe { get; set; }

        string Getriebe { get; set; }

        EMotor EMotor { get; }

        string Hubraum { get; set; }

        string Hybridkennzeichen { get; }

        string Modelljahr { get; }

        string Karosserie { get; set; }

        string Kraftstoffart { get; }

        string Land { get; set; }

        string Leistungsklasse { get; }

        string Lenkung { get; set; }

        string MOTBezeichnung { get; set; }

        string MOTKraftstoffart { get; }

        string Motor { get; set; }

        string MOTEinbaulage { get; }

        string Motorarbeitsverfahren { get; }

        string Produktlinie { get; set; }

        string Sicherheitsrelevant { get; }

        string Tueren { get; }

        string GMType { get; }

        string Ueberarbeitung { get; set; }

        string Modellmonat { get; }

        string VerkaufsBezeichnung { get; set; }

        DateTime ProductionDate { get; }

        BrandName? BrandName { get; }

        IVciDeviceRuleEvaluation VCI { get; }

        IdentificationLevel VehicleIdentLevel { get; set; }

        string ILevel { get; set; }

        string ILevelWerk { get; set; }

        IList<IIdentEcu> ECU { get; }

        IFARuleEvaluation FA { get; }

        IFARuleEvaluation TargetFA { get; }

        List<HeatMotor> HeatMotors { get; }

        string Prodart { get; set; }

        string TargetILevel { get; set; }

        string TypeKey { get; set; }

        string TypeKeyLead { get; set; }

        string TypeKeyBasic { get; set; }

        string ESeriesLifeCycle { get; set; }

        string LifeCycle { get; set; }

        string Sportausfuehrung { get; set; }

        bool? hasFFM(string checkFFM);

        void AddOrUpdateFFM(IFfmResultRuleEvaluation ffm);
    }
}