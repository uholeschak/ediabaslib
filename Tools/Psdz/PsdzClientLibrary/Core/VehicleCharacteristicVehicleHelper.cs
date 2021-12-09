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
		public VehicleCharacteristicVehicleHelper()
		{
			//this.dbConnector = DatabaseProviderFactory.Instance;
			this.characteristicValue = string.Empty;
            this.characteristicRoots = null;
			this.vehicle = new Vehicle();
			this.internalResult = new ValidationRuleInternalResults();
		}

		public bool GetISTACharacteristics(PdszDatabase.CharacteristicRoots characteristicRoots, out string value, decimal id, Vehicle vec, long dataValueId, ValidationRuleInternalResults internalResult)
		{
			this.characteristicRoots = characteristicRoots;
			this.characteristicId = id;
			this.vehicle = vec;
			this.datavalueId = dataValueId;
			this.internalResult = internalResult;
            this.database = ClientContext.GetClientContext(vehicle).Database;
			bool result = base.ComputeCharacteristic(characteristicRoots.NodeClass, Array.Empty<object>());
			value = this.characteristicValue;
			return result;
		}

		protected override bool ComputeAbgas(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Abgas;
			return this.database?.LookupVehicleCharIdByName(this.vehicle.Abgas, new decimal?(68771232130L)) == this.datavalueId;
		}

		protected override bool ComputeAEBezeichnung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.AEBezeichnung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.AEBezeichnung, new decimal?(99999999849L)) == this.datavalueId;
		}

		protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.AEKurzbezeichnung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.AEKurzbezeichnung, new decimal?(99999999913L)) == this.datavalueId;
		}

		protected override bool ComputeAELeistungsklasse(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.AELeistungsklasse;
			return this.database.LookupVehicleCharIdByName(this.vehicle.AELeistungsklasse, new decimal?(99999999914L)) == this.datavalueId;
		}

		protected override bool ComputeAEUeberarbeitung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.AEUeberarbeitung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.AEUeberarbeitung, new decimal?(99999999915L)) == this.datavalueId;
		}

		protected override bool ComputeAntrieb(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Antrieb;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Antrieb, new decimal?(40124162)) == this.datavalueId;
		}

		protected override bool ComputeBaseVersion(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.BaseVersion;
			return this.database.LookupVehicleCharIdByName(this.vehicle.BaseVersion, new decimal?(99999999852L)) == this.datavalueId;
		}

		protected override bool ComputeBasicType(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.BasicType;
			return this.database.LookupVehicleCharIdByName(this.vehicle.BasicType, new decimal?(99999999912L)) == this.datavalueId;
		}

		protected override bool ComputeBaureihe(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Baureihe;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Baureihe, new decimal?(40126722)) == this.datavalueId;
		}

		protected override bool ComputeBaureihenverbund(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Baureihenverbund;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Baureihenverbund, new decimal?(99999999951L)) == this.datavalueId;
		}

		protected override bool ComputeBaustandsJahr(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Modelljahr;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Modelljahr, null) == this.datavalueId;
		}

		protected override bool ComputeBaustandsMonat(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Modellmonat;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Modellmonat, null) == this.datavalueId;
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
			this.characteristicValue = this.vehicle.Ereihe;
			BrandName? brandName = this.vehicle.BrandName;
			if (brandName.GetValueOrDefault() == BrandName.RODING & brandName != null)
			{
				return this.database.LookupVehicleCharIdByName("E89", new decimal?(40128130)) == this.datavalueId;
			}
			brandName = this.vehicle.BrandName;
			if (brandName.GetValueOrDefault() == BrandName.GIBBS & brandName != null)
			{
				return this.database.LookupVehicleCharIdByName("K40", new decimal?(40128130)) == this.datavalueId;
			}
			return this.database.LookupVehicleCharIdByName(this.vehicle.Ereihe, new decimal?(40128130)) == this.datavalueId;
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
			this.characteristicValue = this.vehicle.Land;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Land, new decimal?(40129538)) == this.datavalueId;
		}

		protected override bool ComputeLeistungsklasse(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Leistungsklasse;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Leistungsklasse, new decimal?(40136322)) == this.datavalueId;
		}

		protected override bool ComputeLenkung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Lenkung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Lenkung, new decimal?(40124802)) == this.datavalueId;
		}

		protected override bool ComputeMOTBezeichnung(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.MOTBezeichnung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.MOTBezeichnung, new decimal?(99999999919L)) == this.datavalueId;
		}

		protected override bool ComputeMOTEinbaulage(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.MOTEinbaulage;
			return this.database.LookupVehicleCharIdByName(this.vehicle.MOTEinbaulage, new decimal?(99999999916L)) == this.datavalueId;
		}

		protected override bool ComputeMOTKraftstoffart(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.MOTKraftstoffart;
			return this.database.LookupVehicleCharIdByName(this.vehicle.MOTKraftstoffart, new decimal?(99999999917L)) == this.datavalueId;
		}

		protected override bool ComputeMotor(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Motor;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Motor, new decimal?(40132226)) == this.datavalueId;
		}

		protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Motorarbeitsverfahren;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Motorarbeitsverfahren, new decimal?(68771231746L)) == this.datavalueId;
		}

		protected override bool ComputeProdart(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Prodart;
			return this.database.LookupVehicleCharIdByName(this.vehicle.Prodart, new decimal?(40135682)) == this.datavalueId;
		}

		protected override bool ComputeProduktlinie(params object[] parameters)
		{
			this.characteristicValue = this.vehicle.Produktlinie;
			BrandName? brandName = this.vehicle.BrandName;
			if (brandName.GetValueOrDefault() == BrandName.RODING & brandName != null)
			{
				return this.database.LookupVehicleCharIdByName("PL2", new decimal?(40039952514L)) == this.datavalueId;
			}
			brandName = this.vehicle.BrandName;
			if (brandName.GetValueOrDefault() == BrandName.GIBBS & brandName != null)
			{
				return this.database.LookupVehicleCharIdByName("K", new decimal?(40039952514L)) == this.datavalueId;
			}
			brandName = this.vehicle.BrandName;
			if (!(brandName.GetValueOrDefault() == BrandName.TOYOTA & brandName != null))
			{
				return this.database.LookupVehicleCharIdByName(this.vehicle.Produktlinie, new decimal?(40039952514L)) == this.datavalueId;
			}
			decimal d = this.database.LookupVehicleCharIdByName(this.vehicle.Produktlinie, new decimal?(40039952514L));
			if (!(d != 0m))
			{
				return this.database.LookupVehicleCharIdByName("MINI", new decimal?(40039952514L)) == this.datavalueId;
			}
			return d == this.datavalueId;
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
			BrandName? brandName = this.vehicle.BrandName;
			if (brandName.GetValueOrDefault() == BrandName.GIBBS & brandName != null)
			{
				this.characteristicValue = "K 1300 S";
				return this.database.LookupVehicleCharIdByName("K 1300 S", new decimal?(40122114)) == this.datavalueId;
			}
			this.characteristicValue = this.vehicle.VerkaufsBezeichnung;
			return this.database.LookupVehicleCharIdByName(this.vehicle.VerkaufsBezeichnung, new decimal?(40122114)) == this.datavalueId;
		}

		private bool HandleHeatMotorCharacteristic(Func<HeatMotor, string> getProperty, long datavalueId, ValidationRuleInternalResults internalResult, out string value, string rootNodeClass, decimal characteristicNodeclass)
		{
            decimal rootClassValue;
            if (!decimal.TryParse(rootNodeClass, NumberStyles.Integer, CultureInfo.InvariantCulture, out rootClassValue))
            {
                rootClassValue = 0;
            }
			using (List<HeatMotor>.Enumerator enumerator = this.vehicle.HeatMotors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					HeatMotor hm = enumerator.Current;
					ValidationRuleInternalResult validationRuleInternalResult = internalResult.FirstOrDefault((ValidationRuleInternalResult r) => r.Id == hm.DriveId && r.Type == ValidationRuleInternalResult.CharacteristicType.HeatMotor && r.CharacteristicId == rootClassValue);
					bool flag = database.LookupVehicleCharIdByName(getProperty(hm), new decimal?(characteristicNodeclass)) == datavalueId;
					if (validationRuleInternalResult == null)
                    {
						validationRuleInternalResult = new ValidationRuleInternalResult
						{
							Type = ValidationRuleInternalResult.CharacteristicType.HeatMotor,
							Id = hm.DriveId,
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
							validationRuleInternalResult.IsValid = flag;
						}
					}
					else
					{
						validationRuleInternalResult.IsValid = flag;
					}
				}
			}
			value = string.Join(",", from hm in this.vehicle.HeatMotors
									 select getProperty(hm));
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
