using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BMW.Rheingold.CoreFramework
{
    public sealed class VehicleCharacteristicContext : VehicleCharacteristicAbstract
    {
        private Vehicle vehicle;

        private ICharacteristicsLocator characteristicsLocator;

        [PreserveSource(Hint = "XEP_CHARACTERISTICROOTS", Placeholder = true)]
        private PsdzDatabase.CharacteristicRoots characteristicRoot;

        [PreserveSource(Hint = "XEP_CHARACTERISTICROOTS", SignatureModified = true)]
        public bool IsSetVehicleCharacteristic(string vehicleCode, Vehicle vehicle, ICharacteristicsLocator characteristicsLocator, PsdzDatabase.CharacteristicRoots characteristic)
        {
            return ComputeCharacteristic(vehicleCode, vehicle, characteristicsLocator, characteristic);
        }

        protected override bool ComputeMotor(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Motor, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeAEBezeichnung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.AEBezeichnung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.AEKurzbezeichnung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeAELeistungsklasse(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.AELeistungsklasse, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeAEUeberarbeitung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.AEUeberarbeitung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.KraftstoffartEinbaulage, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeAntrieb(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Antrieb, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBaseVersion(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.BaseVersion, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBasicType(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.BasicType, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBaureihe(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Baureihe, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBaureihenverbund(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Baureihenverbund, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBaustandsJahr(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.BaustandsJahr, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBaustandsMonat(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.BaustandsMonat, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeBrandName(params object[] parameters)
        {
            GetContextParameters(parameters);
            switch (characteristicsLocator.Name.ToUpper(CultureInfo.InvariantCulture))
            {
                case "BMW PKW":
                case "BMW M GMBH PKW":
                case "BMW USA PKW":
                    if (vehicle.BrandName != BrandName.BMWPKW && vehicle.BrandName != BrandName.BMWMGmbHPKW)
                    {
                        return vehicle.BrandName == BrandName.BMWUSAPKW;
                    }
                    return true;
                case "BMW I":
                    return vehicle.BrandName == BrandName.BMWi;
                case "MINI PKW":
                    return vehicle.BrandName == BrandName.MINIPKW;
                case "ROLLS-ROYCE PKW":
                    return vehicle.BrandName == BrandName.ROLLSROYCEPKW;
                case "BMW MOTORRAD":
                    return vehicle.BrandName == BrandName.BMWMOTORRAD;
                case "TOYOTA":
                    return vehicle.BrandName == BrandName.TOYOTA;
                default:
                    if (parameters.Length > 2)
                    {
                        GetContextParameters(parameters);
                        //[-] Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic '{2} Nodeclass ID: {3}' have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name, characteristicRoot.Title_dede, characteristicRoot.Nodeclass);
                        //[+] Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic '{2} Nodeclass ID: {3}' have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name, characteristicRoot.EcuTranslation.TextDe, characteristicRoot.NodeClass);
                        Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic '{2} Nodeclass ID: {3}' have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name, characteristicRoot.EcuTranslation.TextDe, characteristicRoot.NodeClass);
                    }
                    else
                    {
                        Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name);
                    }
                    return false;
            }
        }

        protected override bool ComputeCountryOfAssembly(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.CountryOfAssembly, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeDrehmoment(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Drehmoment, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeElektrischeReichweite(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.ElektrischeReichweite, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTArbeitsverfahren, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTBaureihe(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTBaureihe, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTBezeichnung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTBezeichnung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTDrehmoment(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTDrehmoment, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTEinbaulage(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTEinbaulage, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTKraftstoffart, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTLeistungsklasse, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.EMotor.EMOTUeberarbeitung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEreihe(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Ereihe, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeGetriebe(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Getriebe, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeHubraum(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Hubraum, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeHybridkennzeichen(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Hybridkennzeichen, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeKarosserie(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Karosserie, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Kraftstoffart, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeLand(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Land, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeLeistungsklasse(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Leistungsklasse, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeLenkung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Lenkung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeMOTBezeichnung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.MOTBezeichnung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeMOTEinbaulage(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.MOTEinbaulage, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeMOTKraftstoffart(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.MOTKraftstoffart, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Motorarbeitsverfahren, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeProdart(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Prodart, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeProduktlinie(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Produktlinie, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeSicherheitsrelevant(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Sicherheitsrelevant, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeTueren(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Tueren, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeTyp(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Typ, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Ueberarbeitung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.VerkaufsBezeichnung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeEngine2(params object[] parameters)
        {
            GetContextParameters(parameters);
            return vehicle.HeatMotors.Any((HeatMotor v) => v.HeatMOTBaureihe.Equals(characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase));
        }

        protected override bool ComputeEngineLabel2(params object[] parameters)
        {
            GetContextParameters(parameters);
            return vehicle.HeatMotors.Any((HeatMotor v) => v.HeatMOTBezeichnung.Equals(characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase));
        }

        protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
        {
            GetContextParameters(parameters);
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter1, characteristicsLocator.Name, vehicle.HeatMotors);
        }

        protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
        {
            GetContextParameters(parameters);
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter2, characteristicsLocator.Name, vehicle.HeatMotors);
        }

        protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
        {
            GetContextParameters(parameters);
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTFortlaufendeNum, characteristicsLocator.Name, vehicle.HeatMotors);
        }

        protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
        {
            GetContextParameters(parameters);
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTKraftstoffart, characteristicsLocator.Name, vehicle.HeatMotors);
        }

        protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
        {
            GetContextParameters(parameters);
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLebenszyklus, characteristicsLocator.Name, vehicle.HeatMotors);
        }

        protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
        {
            GetContextParameters(parameters);
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLeistungsklasse, characteristicsLocator.Name, vehicle.HeatMotors);
        }

        protected override bool ComputeTypeKeyLead(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.TypeKeyLead, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeTypeKeyBasic(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.TypeKeyBasic, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeESeriesLifeCycle(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.ESeriesLifeCycle, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeLifeCycle(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.LifeCycle, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeSportausfuehrung(params object[] parameters)
        {
            GetContextParameters(parameters);
            return string.Compare(vehicle.Sportausfuehrung, characteristicsLocator.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool ComputeDefault(params object[] parameters)
        {
            GetContextParameters(parameters);
            if (parameters.Length > 2)
            {
                //[-] Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic '{2} Nodeclass ID: {3}' have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name, characteristicRoot.Title_dede, characteristicRoot.Nodeclass);
                //[+] Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic '{2} Nodeclass ID: {3}' have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name, characteristicRoot.EcuTranslation.TextDe, characteristicRoot.NodeClass);
                Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic '{2} Nodeclass ID: {3}' have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name, characteristicRoot.EcuTranslation.TextDe, characteristicRoot.NodeClass);
            }
            else
            {
                Log.Warning("VehicleIdent.IsSet()", "Found unknown key:{0} value: {1}. WARNING!!! Unknown characteristic have been used!!!", characteristicsLocator.DataClassName, characteristicsLocator.Name);
            }
            return false;
        }

        private void GetContextParameters(params object[] parameters)
        {
            vehicle = (Vehicle)parameters[0];
            characteristicsLocator = (ICharacteristicsLocator)parameters[1];
            //[-] characteristicRoot = (XEP_CHARACTERISTICROOTS)parameters[2];
            //[+] characteristicRoot = (PsdzDatabase.CharacteristicRoots)parameters[2];
            characteristicRoot = (PsdzDatabase.CharacteristicRoots)parameters[2];
        }

        private bool HandleHeatMotorCharacteristic(Func<HeatMotor, string> getProperty, string value, List<HeatMotor> heatMotors)
        {
            return heatMotors?.Any((HeatMotor hm) => getProperty(hm).Equals(value, StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }
    }
}
