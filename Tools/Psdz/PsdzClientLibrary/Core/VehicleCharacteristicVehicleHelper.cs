using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

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

        public bool GetISTACharacteristics(PdszDatabase.CharacteristicRoots characteristicRoots, out string value, decimal id, Vehicle vec, long dataValueId, ValidationRuleInternalResults internalResult)
        {
            this.characteristicRoots = characteristicRoots;
            characteristicId = id;
            this.vehicle = vec;
            datavalueId = dataValueId;
            this.internalResult = internalResult;
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

        protected override bool ComputeBrandName(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Marke;
			if (string.Equals("HUSQVARNA", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("BMW MOTORRAD", new decimal?(40139010)) == this.datavalueId;
			}
			if (string.Equals("CAMPAGNA", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("CAMPAGNA", new decimal?(40139010)) == this.datavalueId;
			}
			if (string.Equals("ROSENBAUER", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("ROSENBAU", new decimal?(40139010)) == this.datavalueId;
			}
			if (string.Equals("VAILLANT", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("VAILLANT", new decimal?(40139010)) == this.datavalueId;
			}
			if (string.Equals("GIBBS", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("BMW MOTORRAD", new decimal?(40139010)) == this.datavalueId;
			}
			if (string.Equals("RODING", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("BMW PKW", new decimal?(40139010)) == this.datavalueId;
			}
			if (string.Equals("BMW I", this.vehicle.Marke, StringComparison.OrdinalIgnoreCase))
			{
				return this.database.LookupVehicleCharIdByName("BMW I", new decimal?(40139010)) == this.datavalueId;
			}
			return this.database.LookupVehicleCharIdByName(this.vehicle.Marke, new decimal?(40139010)) == this.datavalueId;
		}

		protected override bool ComputeCountryOfAssembly(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.CountryOfAssembly;
			return this.database.LookupVehicleCharIdByName(this.vehicle.CountryOfAssembly, new decimal?(99999999853L)) == this.datavalueId;
		}

		protected override bool ComputeDefault(params object[] parameters)
		{
			this.characteristicValue = "???";
			return false;
		}

		protected override bool ComputeDrehmoment(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Drehmoment;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Drehmoment, new decimal?(68771232898L)) == this.datavalueId;
		}

		protected override bool ComputeElektrischeReichweite(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.ElektrischeReichweite;
			return this.database.LookupVehicleCharIdByName(this.vehicle.ElektrischeReichweite, new decimal?(99999999855L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTArbeitsverfahren;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTArbeitsverfahren, new decimal?(99999999877L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTBaureihe(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTBaureihe;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTBaureihe, new decimal?(99999999879L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTBezeichnung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTBezeichnung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTBezeichnung, new decimal?(99999999869L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTDrehmoment(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTDrehmoment;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTDrehmoment, new decimal?(99999999875L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTEinbaulage(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTEinbaulage;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTEinbaulage, new decimal?(99999999865L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTKraftstoffart;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTKraftstoffart, new decimal?(99999999867L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTLeistungsklasse;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTLeistungsklasse, new decimal?(99999999873L)) == this.datavalueId;
		}

		protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.EMotor.EMOTUeberarbeitung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.EMotor.EMOTUeberarbeitung, new decimal?(99999999871L)) == this.datavalueId;
		}

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.KraftstoffartEinbaulage;
            return this.database.LookupVehicleCharIdByName(vehicle.KraftstoffartEinbaulage, 53330059) == (decimal)datavalueId;
        }

		protected override bool ComputeEngine2(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTBaureihe, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999712m);
		}

		protected override bool ComputeEngineLabel2(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTBezeichnung, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999711m);
		}

        protected override bool ComputeEreihe(params object[] parameters)
        {
            characteristicValue = vehicle.Ereihe;
            if (vehicle.BrandName == BrandName.RODING)
            {
                return database.LookupVehicleCharIdByName("E89", 40128130) == datavalueId;
            }
            if (vehicle.BrandName == BrandName.GIBBS)
            {
                return database.LookupVehicleCharIdByName("K40", 40128130) == datavalueId;
            }
            return database.LookupVehicleCharIdByName(vehicle.Ereihe, 40128130) == datavalueId;
        }

		protected override bool ComputeGetriebe(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Getriebe;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Getriebe, new decimal?(40137602)) == this.datavalueId;
		}

		protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTFortlaufendeNum, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999715m);
		}

		protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTKraftstoffart, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999718m);
		}

		protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLebenszyklus, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999717m);
		}

		protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLeistungsklasse, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999716m);
		}

		protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter1, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999713m);
		}

		protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
		{
			return this.HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter2, this.datavalueId, this.internalResult, out this.characteristicValue, this.characteristicRoots?.NodeClass, 99999999714m);
		}

		protected override bool ComputeHubraum(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Hubraum;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Hubraum, new decimal?(40131586)) == this.datavalueId;
		}

		protected override bool ComputeHybridkennzeichen(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Hybridkennzeichen;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Hybridkennzeichen, new decimal?(68771232514L)) == this.datavalueId;
		}

		protected override bool ComputeKarosserie(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Karosserie;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Karosserie, new decimal?(40133634)) == this.datavalueId;
		}

		protected override bool ComputeKraftstoffart(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Kraftstoffart;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Kraftstoffart, new decimal?(40125442)) == this.datavalueId;
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
            if (vehicle.BrandName == BrandName.RODING)
			{
                return database.LookupVehicleCharIdByName("PL2", 40039952514L) == (decimal)datavalueId;
            }
            if (vehicle.BrandName == BrandName.GIBBS)
			{
                return database.LookupVehicleCharIdByName("K", 40039952514L) == (decimal)datavalueId;
            }
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
			string sicherheitsrelevant = this.vehicle.Sicherheitsrelevant;
			this.characteristicValue = sicherheitsrelevant;
			return this.database.LookupVehicleCharIdByName(sicherheitsrelevant, new decimal?(40136962)) == this.datavalueId;
		}

		protected override bool ComputeTueren(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Tueren;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Tueren, new decimal?(40130946)) == this.datavalueId;
		}

		protected override bool ComputeTyp(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.GMType;
			return this.database.LookupVehicleCharIdByName(this.vehicle.GMType, new decimal?(40135042)) == this.datavalueId;
		}

		protected override bool ComputeUeberarbeitung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Ueberarbeitung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Ueberarbeitung, new decimal?(40123522)) == this.datavalueId;
		}

        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            if (vehicle.BrandName == BrandName.GIBBS)
            {
                characteristicValue = "K 1300 S";
                return database.LookupVehicleCharIdByName("K 1300 S", 40122114) == (decimal)datavalueId;
            }
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
        PdszDatabase database;

		private string characteristicValue;

		private PdszDatabase.CharacteristicRoots characteristicRoots;

		private decimal characteristicId;

		private Vehicle vehicle;

		private long datavalueId;

		private ValidationRuleInternalResults internalResult;
	}
}
