using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
	public class VehicleCharacteristicVehicleHelper : VehicleCharacteristicAbstract
	{
        public VehicleCharacteristicVehicleHelper(Vehicle vec)
        {
            //dbConnector = DatabaseProviderFactory.Instance;
            characteristicValue = string.Empty;
            characteristicRoots = null;
            vehicle = vec;
            internalResult = new ValidationRuleInternalResults();
        }

        public bool GetISTACharacteristics(PsdzDatabase.CharacteristicRoots characteristicRoots, out string value, decimal id, Vehicle vec, long dataValueId, ValidationRuleInternalResults internalResult)
        {
            this.characteristicRoots = characteristicRoots;
            characteristicId = id;
            this.vehicle = vec;
            datavalueId = dataValueId;
            this.internalResult = internalResult;
            database = ClientContext.GetDatabase(vehicle);
            bool result = ComputeCharacteristic(characteristicRoots.NodeClass);
            value = characteristicValue;
            return result;
        }

        protected override bool ComputeAbgas(params object[] parameters)
        {
            characteristicValue = vehicle.Abgas;
            return database.LookupVehicleCharIdByName(vehicle.Abgas, 68771232130L) == (decimal)datavalueId;
        }

        protected override bool ComputeAEBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.AEBezeichnung;
            return database.LookupVehicleCharIdByName(vehicle.AEBezeichnung, 99999999849L) == (decimal)datavalueId;
        }

        protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.AEKurzbezeichnung;
            return database.LookupVehicleCharIdByName(vehicle.AEKurzbezeichnung, 99999999913L) == (decimal)datavalueId;
        }

        protected override bool ComputeAELeistungsklasse(params object[] parameters)
        {
            characteristicValue = vehicle.AELeistungsklasse;
            return database.LookupVehicleCharIdByName(vehicle.AELeistungsklasse, 99999999914L) == (decimal)datavalueId;
        }

        protected override bool ComputeAEUeberarbeitung(params object[] parameters)
        {
            characteristicValue = vehicle.AEUeberarbeitung;
            return database.LookupVehicleCharIdByName(vehicle.AEUeberarbeitung, 99999999915L) == (decimal)datavalueId;
        }

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.KraftstoffartEinbaulage;
            return database.LookupVehicleCharIdByName(vehicle.KraftstoffartEinbaulage, 53330059) == (decimal)datavalueId;
        }

        protected override bool ComputeAntrieb(params object[] parameters)
        {
            characteristicValue = vehicle.Antrieb;
            return database.LookupVehicleCharIdByName(vehicle.Antrieb, 40124162) == (decimal)datavalueId;
        }

        protected override bool ComputeBaseVersion(params object[] parameters)
        {
            characteristicValue = vehicle.BaseVersion;
            return database.LookupVehicleCharIdByName(vehicle.BaseVersion, 99999999852L) == (decimal)datavalueId;
        }

        protected override bool ComputeBasicType(params object[] parameters)
        {
            characteristicValue = vehicle.BasicType;
            return database.LookupVehicleCharIdByName(vehicle.BasicType, 99999999912L) == (decimal)datavalueId;
        }

        protected override bool ComputeBaureihe(params object[] parameters)
        {
            characteristicValue = vehicle.Baureihe;
            return database.LookupVehicleCharIdByName(vehicle.Baureihe, 40126722) == (decimal)datavalueId;
        }

        protected override bool ComputeBaureihenverbund(params object[] parameters)
        {
            characteristicValue = vehicle.Baureihenverbund;
            return database.LookupVehicleCharIdByName(vehicle.Baureihenverbund, 99999999951L) == (decimal)datavalueId;
        }

        protected override bool ComputeBaustandsJahr(params object[] parameters)
        {
            characteristicValue = vehicle.Modelljahr;
            return database.LookupVehicleCharIdByName(vehicle.Modelljahr, null) == (decimal)datavalueId;
        }

        protected override bool ComputeBaustandsMonat(params object[] parameters)
        {
            characteristicValue = vehicle.Modellmonat;
            return database.LookupVehicleCharIdByName(vehicle.Modellmonat, null) == (decimal)datavalueId;
        }

        // ToDo: Check on update
        protected override bool ComputeBrandName(params object[] parameters)
        {
            characteristicValue = vehicle.Marke;
            if (string.Equals("HUSQVARNA", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("BMW MOTORRAD", 40139010) == (decimal)datavalueId;
            }
            if (string.Equals("CAMPAGNA", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("CAMPAGNA", 40139010) == (decimal)datavalueId;
            }
            if (string.Equals("ROSENBAUER", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("ROSENBAU", 40139010) == (decimal)datavalueId;
            }
            if (string.Equals("VAILLANT", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("VAILLANT", 40139010) == (decimal)datavalueId;
            }
            if (string.Equals("GIBBS", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("BMW MOTORRAD", 40139010) == (decimal)datavalueId;
            }
            if (string.Equals("RODING", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("BMW PKW", 40139010) == (decimal)datavalueId;
            }
            if (string.Equals("BMW I", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return database.LookupVehicleCharIdByName("BMW I", 40139010) == (decimal)datavalueId;
            }
            return database.LookupVehicleCharIdByName(vehicle.Marke, 40139010) == (decimal)datavalueId;
        }

        protected override bool ComputeCountryOfAssembly(params object[] parameters)
        {
            characteristicValue = vehicle.CountryOfAssembly;
            return database.LookupVehicleCharIdByName(vehicle.CountryOfAssembly, 99999999853L) == (decimal)datavalueId;
        }

        protected override bool ComputeDefault(params object[] parameters)
        {
            characteristicValue = "???";
            Log.Warning("Vehicle.getISTACharactersitics()", "failed to evaluate characteristic: {0} (id: {1})", characteristicRoots?.EcuTranslation?.TextDe ?? string.Empty, characteristicId);
            return false;
        }

        protected override bool ComputeDrehmoment(params object[] parameters)
        {
            characteristicValue = vehicle.Drehmoment;
            return database.LookupVehicleCharIdByName(vehicle.Drehmoment, 68771232898L) == (decimal)datavalueId;
        }

        protected override bool ComputeElektrischeReichweite(params object[] parameters)
        {
            characteristicValue = vehicle.ElektrischeReichweite;
            return database.LookupVehicleCharIdByName(vehicle.ElektrischeReichweite, 99999999855L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTArbeitsverfahren;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTArbeitsverfahren, 99999999877L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTBaureihe(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTBaureihe;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTBaureihe, 99999999879L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTBezeichnung;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTBezeichnung, 99999999869L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTDrehmoment(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTDrehmoment;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTDrehmoment, 99999999875L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTEinbaulage;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTEinbaulage, 99999999865L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTKraftstoffart;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTKraftstoffart, 99999999867L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTLeistungsklasse;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTLeistungsklasse, 99999999873L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTUeberarbeitung;
            return database.LookupVehicleCharIdByName(vehicle.EMotor.EMOTUeberarbeitung, 99999999871L) == (decimal)datavalueId;
        }

        protected override bool ComputeEngine2(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTBaureihe, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999712L));
        }

        protected override bool ComputeEngineLabel2(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTBezeichnung, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999711L));
        }

        protected override bool ComputeEreihe(params object[] parameters)
        {
            {
                characteristicValue = vehicle.Ereihe;
                return database.LookupVehicleCharIdByName(vehicle.Ereihe, 40128130) == (decimal)datavalueId;
            }
        }

        protected override bool ComputeGetriebe(params object[] parameters)
        {
            characteristicValue = vehicle.Getriebe;
            return database.LookupVehicleCharIdByName(vehicle.Getriebe, 40137602) == (decimal)datavalueId;
        }

        protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTFortlaufendeNum, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999715L));
        }

        protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTKraftstoffart, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999718L));
        }

        protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLebenszyklus, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999717L));
        }

        protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLeistungsklasse, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999716L));
        }

        protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter1, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999713L));
        }

        protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter2, datavalueId, internalResult, out characteristicValue, characteristicRoots.NodeClass, new decimal(99999999714L));
        }

        protected override bool ComputeHubraum(params object[] parameters)
        {
            characteristicValue = vehicle.Hubraum;
            return database.LookupVehicleCharIdByName(vehicle.Hubraum, 40131586) == (decimal)datavalueId;
        }

        protected override bool ComputeHybridkennzeichen(params object[] parameters)
        {
            characteristicValue = vehicle.Hybridkennzeichen;
            return database.LookupVehicleCharIdByName(vehicle.Hybridkennzeichen, 68771232514L) == (decimal)datavalueId;
        }

        protected override bool ComputeKarosserie(params object[] parameters)
        {
            characteristicValue = vehicle.Karosserie;
            return database.LookupVehicleCharIdByName(vehicle.Karosserie, 40133634) == (decimal)datavalueId;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            characteristicValue = vehicle.Kraftstoffart;
            return database.LookupVehicleCharIdByName(vehicle.Kraftstoffart, 40125442) == (decimal)datavalueId;
        }

        protected override bool ComputeLand(params object[] parameters)
        {
            characteristicValue = vehicle.Land;
            return database.LookupVehicleCharIdByName(vehicle.Land, 40129538) == (decimal)datavalueId;
        }

        protected override bool ComputeLeistungsklasse(params object[] parameters)
        {
            characteristicValue = vehicle.Leistungsklasse;
            return database.LookupVehicleCharIdByName(vehicle.Leistungsklasse, 40136322) == (decimal)datavalueId;
        }

        protected override bool ComputeLenkung(params object[] parameters)
        {
            characteristicValue = vehicle.Lenkung;
            return database.LookupVehicleCharIdByName(vehicle.Lenkung, 40124802) == (decimal)datavalueId;
        }

        protected override bool ComputeMOTBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.MOTBezeichnung;
            return database.LookupVehicleCharIdByName(vehicle.MOTBezeichnung, 99999999919L) == (decimal)datavalueId;
        }

        protected override bool ComputeMOTEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.MOTEinbaulage;
            return database.LookupVehicleCharIdByName(vehicle.MOTEinbaulage, 99999999916L) == (decimal)datavalueId;
        }

        protected override bool ComputeMOTKraftstoffart(params object[] parameters)
        {
            characteristicValue = vehicle.MOTKraftstoffart;
            return database.LookupVehicleCharIdByName(vehicle.MOTKraftstoffart, 99999999917L) == (decimal)datavalueId;
        }

        protected override bool ComputeMotor(params object[] parameters)
        {
            characteristicValue = vehicle.Motor;
            return database.LookupVehicleCharIdByName(vehicle.Motor, 40132226) == (decimal)datavalueId;
        }

        protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
        {
            characteristicValue = vehicle.Motorarbeitsverfahren;
            return database.LookupVehicleCharIdByName(vehicle.Motorarbeitsverfahren, 68771231746L) == (decimal)datavalueId;
        }

        protected override bool ComputeProdart(params object[] parameters)
        {
            characteristicValue = vehicle.Prodart;
            return database.LookupVehicleCharIdByName(vehicle.Prodart, 40135682) == (decimal)datavalueId;
        }

        protected override bool ComputeProduktlinie(params object[] parameters)
        {
            characteristicValue = vehicle.Produktlinie;
            if (vehicle.BrandName == BrandName.TOYOTA)
            {
                decimal num2 = database.LookupVehicleCharIdByName(vehicle.Produktlinie, 40039952514L);
                if (!(num2 != 0m))
                {
                    return database.LookupVehicleCharIdByName("MINI", 40039952514L) == (decimal)datavalueId;
                }
                return num2 == (decimal)datavalueId;
            }
            return database.LookupVehicleCharIdByName(vehicle.Produktlinie, 40039952514L) == (decimal)datavalueId;
        }

        protected override bool ComputeSicherheitsrelevant(params object[] parameters)
        {
            string name = (characteristicValue = vehicle.Sicherheitsrelevant);
            return database.LookupVehicleCharIdByName(name, 40136962) == (decimal)datavalueId;
        }

        protected override bool ComputeTueren(params object[] parameters)
        {
            characteristicValue = vehicle.Tueren;
            return database.LookupVehicleCharIdByName(vehicle.Tueren, 40130946) == (decimal)datavalueId;
        }

        protected override bool ComputeTyp(params object[] parameters)
        {
            characteristicValue = vehicle.GMType;
            return database.LookupVehicleCharIdByName(vehicle.GMType, 40135042) == (decimal)datavalueId;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            characteristicValue = vehicle.Ueberarbeitung;
            return database.LookupVehicleCharIdByName(vehicle.Ueberarbeitung, 40123522) == (decimal)datavalueId;
        }

        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.VerkaufsBezeichnung;
            return database.LookupVehicleCharIdByName(vehicle.VerkaufsBezeichnung, 40122114) == (decimal)datavalueId;
        }

        private bool HandleHeatMotorCharacteristic(Func<HeatMotor, string> getProperty, long datavalueId, ValidationRuleInternalResults internalResult, out string value, string rootNodeClass, decimal characteristicNodeclass)
        {
            if (!decimal.TryParse(rootNodeClass, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal rootClassValue))
            {
                rootClassValue = 0;
            }

            foreach (HeatMotor hm2 in vehicle.HeatMotors)
            {
                ValidationRuleInternalResult validationRuleInternalResult = internalResult.FirstOrDefault((ValidationRuleInternalResult r) => r.Id == hm2.DriveId && r.Type == ValidationRuleInternalResult.CharacteristicType.HeatMotor && r.CharacteristicId == rootClassValue);
                bool flag = database.LookupVehicleCharIdByName(getProperty(hm2), characteristicNodeclass) == (decimal)datavalueId;
                if (validationRuleInternalResult == null)
                {
                    validationRuleInternalResult = new ValidationRuleInternalResult
                    {
                        Type = ValidationRuleInternalResult.CharacteristicType.HeatMotor,
                        Id = hm2.DriveId,
                        CharacteristicId = rootClassValue
                    };
                    if (!(internalResult.RuleExpression is OrExpression))
                    {
                        validationRuleInternalResult.IsValid = true;
                    }
                    internalResult.Add(validationRuleInternalResult);
                }
                RuleExpression ruleExpression = internalResult.RuleExpression;
                if (!(ruleExpression is AndExpression))
                {
                    if (!(ruleExpression is OrExpression))
                    {
                        if (ruleExpression is NotExpression)
                        {
                            validationRuleInternalResult.IsValid &= !flag;
                        }
                    }
                    else
                    {
                        validationRuleInternalResult.IsValid |= flag;
                    }
                }
                else
                {
                    validationRuleInternalResult.IsValid &= flag;
                }
            }
            value = string.Join(",", vehicle.HeatMotors.Select((HeatMotor hm) => getProperty(hm)));
            bool flag2 = (from r in internalResult
                group r by r.Id).Any((IGrouping<string, ValidationRuleInternalResult> g) => g.All((ValidationRuleInternalResult c) => c.IsValid));
            if (!(internalResult.RuleExpression is NotExpression))
            {
                return flag2;
            }
            return !flag2;
        }

        //private IDatabaseProvider dbConnector;
        PsdzDatabase database;

		private string characteristicValue;

		private PsdzDatabase.CharacteristicRoots characteristicRoots;

		private decimal characteristicId;

		private Vehicle vehicle;

		private long datavalueId;

		private ValidationRuleInternalResults internalResult;
	}
}
