using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class VehicleCharacteristicIdent : VehicleCharacteristicAbstract
	{
		public bool AssignVehicleCharacteristic(string vehicleCode, Vehicle vehicle, PdszDatabase.Characteristics characteristic)
		{
			return base.ComputeCharacteristic(vehicleCode, new object[]
			{
				vehicle,
				characteristic
			});
		}

		protected override bool ComputeAbgas(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Abgas = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeAEBezeichnung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.AEBezeichnung = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.AEKurzbezeichnung = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeAELeistungsklasse(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.AELeistungsklasse = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeAEUeberarbeitung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.AEUeberarbeitung = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeAntrieb(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Antrieb = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBaseVersion(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.BaseVersion = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBasicType(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.BasicType = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBaureihe(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Baureihe = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBaureihenverbund(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Baureihenverbund = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBaustandsJahr(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.BaustandsJahr = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBaustandsMonat(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.BaustandsMonat = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeBrandName(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			string text = this.characteristic.Name.ToUpper(CultureInfo.InvariantCulture);
			if (text != null)
			{
				if (text == "BMW PKW")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.BMWPKW);
					this.vecInfo.Marke = "BMW PKW";
					return true;
				}
				if (text == "BMW I")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.BMWi);
					this.vecInfo.Marke = "BMW i";
					return true;
				}
				if (text == "HUSQVARNA")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.HUSQVARNA);
					this.vecInfo.Marke = "HUSQVARNA";
					return true;
				}
				if (text == "TOYOTA")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.TOYOTA);
					this.vecInfo.Marke = "TOYOTA";
					return true;
				}
				if (text == "BMW M GMBH PKW")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.BMWMGmbHPKW);
					this.vecInfo.Marke = "BMW PKW";
					return true;
				}
				if (text == "ROLLS-ROYCE PKW")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.ROLLSROYCEPKW);
					this.vecInfo.Marke = "ROLLS-ROYCE PKW";
					return true;
				}
				if (text == "ROSENBAU")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.ROSENBAUER);
					this.vecInfo.Marke = "ROSENBAU";
					return true;
				}
			    if (text == "BMW USA PKW")
			    {
				    this.vecInfo.BrandName = new BrandName?(BrandName.BMWUSAPKW);
				    this.vecInfo.Marke = "BMW PKW";
				    return true;
			    }
				if (text == "BMW MOTORRAD")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.BMWMOTORRAD);
					this.vecInfo.Marke = "BMW MOTORRAD";
					return true;
				}
				if (text == "MINI PKW")
				{
					this.vecInfo.BrandName = new BrandName?(BrandName.MINIPKW);
					this.vecInfo.Marke = "MINI PKW";
					return true;
				}
			}
#if false
			Log.Warning("VehicleIdent.UpdateVehicleCharacteristics()", "found unknown brand name: {0}", new object[]
			{
				this.characteristic.Name
			});
#endif
			return true;
		}

		protected override bool ComputeCountryOfAssembly(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.CountryOfAssembly = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeDrehmoment(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Drehmoment = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeElektrischeReichweite(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.ElektrischeReichweite = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeEreihe(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
            decimal? istaVisible = null;
            if (decimal.TryParse(this.characteristic.IstaVisible, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal visible))
            {
                istaVisible = visible;
            }
			decimal d = 0m;
			if (!(istaVisible.GetValueOrDefault() == d & istaVisible != null))
			{
				this.vecInfo.Ereihe = this.characteristic.Name;
				this.vecInfo.EBezeichnungUIText = this.characteristic.Name;
			}
			else
			{
				this.vecInfo.Ereihe = this.characteristic.Name;
				this.vecInfo.EBezeichnungUIText = DefaultEmptyCharacteristicValue;
			}
			return true;
		}

		protected override bool ComputeGetriebe(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			GearboxUtility.SetGearboxTypeFromCharacteristics(this.vecInfo, this.characteristic);
			return true;
		}

		protected override bool ComputeHybridkennzeichen(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Hybridkennzeichen = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeKarosserie(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Karosserie = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeLand(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Land = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeLenkung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Lenkung = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeMOTKraftstoffart(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.MOTKraftstoffart = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Motorarbeitsverfahren = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeProdart(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Prodart = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeProduktlinie(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Produktlinie = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeSicherheitsrelevant(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Sicherheitsrelevant = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeTueren(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Tueren = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeTyp(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Typ = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
            decimal? istaVisible = null;
            if (decimal.TryParse(this.characteristic.IstaVisible, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal visible))
            {
                istaVisible = visible;
            }
            if (!((istaVisible.GetValueOrDefault() == default(decimal)) & istaVisible.HasValue))
            {
				this.vecInfo.VerkaufsBezeichnung = this.characteristic.Name;
			}
			else
			{
				this.vecInfo.VerkaufsBezeichnung = DefaultEmptyCharacteristicValue;
			}
			return true;
		}

		protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTArbeitsverfahren = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeEMOTBaureihe(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTBaureihe = this.characteristic.Name;
			if (string.IsNullOrWhiteSpace(this.vecInfo.GenericMotor.Engine2) || this.vecInfo.GenericMotor.Engine2 == DefaultEmptyCharacteristicValue)
			{
				this.vecInfo.GenericMotor.Engine2 = this.characteristic.Name;
			}
			return true;
		}

		protected override bool ComputeEMOTBezeichnung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTBezeichnung = this.characteristic.Name;
			if (string.IsNullOrWhiteSpace(this.vecInfo.GenericMotor.EngineLabel2) || this.vecInfo.GenericMotor.EngineLabel2 == DefaultEmptyCharacteristicValue)
			{
				this.vecInfo.GenericMotor.EngineLabel2 = this.characteristic.Name;
			}
			return true;
		}

		protected override bool ComputeEMOTDrehmoment(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTDrehmoment = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeEMOTEinbaulage(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTEinbaulage = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTKraftstoffart = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTLeistungsklasse = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.EMotor.EMOTUeberarbeitung = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeLeistungsklasse(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Leistungsklasse = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeMotor(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Motor = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeHubraum(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Hubraum = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeKraftstoffart(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Kraftstoffart = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeMOTBezeichnung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.MOTBezeichnung = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeMOTEinbaulage(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.MOTEinbaulage = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeUeberarbeitung(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.vecInfo.Ueberarbeitung = this.characteristic.Name;
			return true;
		}

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            GetIdentParameters(parameters);
            vecInfo.KraftstoffartEinbaulage = this.characteristic.Name;
            return true;
        }

		protected override bool ComputeEngine2(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTBaureihe = this.characteristic.Name;
			if (string.IsNullOrWhiteSpace(this.vecInfo.GenericMotor.Engine2) || this.characteristic.Name != DefaultEmptyCharacteristicValue)
			{
				this.vecInfo.GenericMotor.Engine2 = string.Join(",", from v in this.vecInfo.HeatMotors
																	 select v.HeatMOTBaureihe);
			}
			return true;
		}

		protected override bool ComputeEngineLabel2(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTBezeichnung = this.characteristic.Name;
			if (string.IsNullOrWhiteSpace(this.vecInfo.GenericMotor.EngineLabel2) || this.characteristic.Name != DefaultEmptyCharacteristicValue)
			{
				this.vecInfo.GenericMotor.EngineLabel2 = string.Join(",", from v in this.vecInfo.HeatMotors
																		  select v.HeatMOTBezeichnung);
			}
			return true;
		}

		protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTFortlaufendeNum = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTKraftstoffart = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTLebenszyklus = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTLeistungsklasse = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTPlatzhalter1 = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
			this.GetHeatMotorByDriveId(this.characteristic.DriveId).HeatMOTPlatzhalter2 = this.characteristic.Name;
			return true;
		}

		protected override bool ComputeDefault(params object[] parameters)
		{
			this.GetIdentParameters(parameters);
#if false
			Log.Warning("VehicleIdent.UpdateVehicleCharacteristics()", "found unknown key:{0} value: {1}", new object[]
			{
				this.characteristic.RootNodeClass,
				this.characteristic.Name
			});
#endif
			return false;
		}

		private void GetIdentParameters(params object[] parameters)
		{
			this.vecInfo = (Vehicle)parameters[0];
			this.characteristic = (PdszDatabase.Characteristics)parameters[1];
		}

		private HeatMotor GetHeatMotorByDriveId(string driveId)
		{
			HeatMotor heatMotor = this.vecInfo.HeatMotors.FirstOrDefault((HeatMotor m) => m.DriveId.Equals(driveId));
			if (heatMotor != null)
			{
				return heatMotor;
			}
			heatMotor = new HeatMotor
			{
				DriveId = driveId
			};
			this.vecInfo.HeatMotors.Add(heatMotor);
			return heatMotor;
		}

		public const string DefaultEmptyCharacteristicValue = "-";

		private Vehicle vecInfo;

		private PdszDatabase.Characteristics characteristic;
	}
}
