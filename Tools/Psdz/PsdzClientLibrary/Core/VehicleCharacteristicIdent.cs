using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class VehicleCharacteristicIdent : VehicleCharacteristicAbstract
	{
        public bool AssignVehicleCharacteristic(string vehicleCode, Vehicle vehicle, PsdzDatabase.Characteristics characteristic)
        {
            return ComputeCharacteristic(vehicleCode, vehicle, characteristic);
        }

        protected override bool ComputeAbgas(params object[] parameters)
        {
            GetIdentParameters(parameters);
            vecInfo.Abgas = characteristic.Name;
            return true;
        }

        protected override bool ComputeAEBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetAEBezeichnung(characteristic.Name, DataSource.Database);
            vecInfo.AEBezeichnung = characteristic.Name;
            return true;
        }

        protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetAEKurzbezeichnung(characteristic.Name, DataSource.Database);
            vecInfo.AEKurzbezeichnung = characteristic.Name;
            return true;
        }

        protected override bool ComputeAELeistungsklasse(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetAELeistungsklasse(characteristic.Name, DataSource.Database);
            vecInfo.AELeistungsklasse = characteristic.Name;
            return true;
        }

        protected override bool ComputeAEUeberarbeitung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetAEUeberarbeitung(characteristic.Name, DataSource.Database);
            vecInfo.AEUeberarbeitung = characteristic.Name;
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
            GetIdentParameters(parameters);
            string text = characteristic.Name.ToUpper(CultureInfo.InvariantCulture);
            //reactor.SetMarke(text, DataSource.Database);
            vecInfo.Marke = text;
            switch (text)
            {
                case "BMW PKW":
                    //reactor.SetBrandName(BrandName.BMWPKW, DataSource.Database);
                    vecInfo.BrandName = BrandName.BMWPKW;
                    break;
                case "BMW I":
                    //reactor.SetBrandName(BrandName.BMWi, DataSource.Database);
                    vecInfo.BrandName = BrandName.BMWi;
                    break;
                case "TOYOTA":
                    //reactor.SetBrandName(BrandName.TOYOTA, DataSource.Database);
                    vecInfo.BrandName = BrandName.TOYOTA;
                    break;
                case "BMW M GMBH PKW":
                    //reactor.SetBrandName(BrandName.BMWMGmbHPKW, DataSource.Database);
                    vecInfo.BrandName = BrandName.BMWMGmbHPKW;
                    break;
                case "MINI PKW":
                    //reactor.SetBrandName(BrandName.MINIPKW, DataSource.Database);
                    vecInfo.BrandName = BrandName.MINIPKW;
                    break;
                case "ROLLS-ROYCE PKW":
                    //reactor.SetBrandName(BrandName.ROLLSROYCEPKW, DataSource.Database);
                    vecInfo.BrandName = BrandName.ROLLSROYCEPKW;
                    break;
                case "BMW USA PKW":
                    //reactor.SetBrandName(BrandName.BMWUSAPKW, DataSource.Database);
                    vecInfo.BrandName = BrandName.BMWUSAPKW;
                    break;
                default:
                    //Log.Warning("VehicleIdent.UpdateVehicleCharacteristics()", "found unknown brand name: {0}", characteristic.Name);
                    break;
                case "BMW MOTORRAD":
                    //reactor.SetBrandName(BrandName.BMWMOTORRAD, DataSource.Database);
                    vecInfo.BrandName = BrandName.BMWMOTORRAD;
                    break;
            }
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
            if (!((istaVisible.GetValueOrDefault() == default(decimal)) & istaVisible.HasValue))
            {
                //reactor.SetEreihe(characteristic.Name, DataSource.Database);
                vecInfo.Ereihe = characteristic.Name;
                vecInfo.EBezeichnungUIText = characteristic.Name;
            }
            else
            {
                //reactor.SetEreihe(characteristic.Name, DataSource.Database);
                vecInfo.Ereihe = characteristic.Name;
                vecInfo.EBezeichnungUIText = DefaultEmptyCharacteristicValue;
            }
            return true;
        }

        protected override bool ComputeGetriebe(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GearboxUtility.SetGearboxTypeFromCharacteristics(vecInfo, characteristic);
            return true;
        }

        protected override bool ComputeHybridkennzeichen(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetHybridkennzeichen(characteristic.Name, DataSource.Database);
            vecInfo.Hybridkennzeichen = characteristic.Name;
            return true;
        }

        protected override bool ComputeKarosserie(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetKarosserie(characteristic.Name, DataSource.Database);
            vecInfo.Karosserie = characteristic.Name;
            return true;
        }

        protected override bool ComputeLand(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetLand(characteristic.Name, DataSource.Database);
            vecInfo.Land = characteristic.Name;
            return true;
        }

        protected override bool ComputeLenkung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetLenkung(characteristic.Name, DataSource.Database);
            vecInfo.Lenkung = characteristic.Name;
            return true;
        }

        protected override bool ComputeMOTKraftstoffart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            vecInfo.MOTKraftstoffart = characteristic.Name;
            return true;
        }

        protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetMotorarbeitsverfahren(characteristic.Name, DataSource.Database);
            vecInfo.Motorarbeitsverfahren = characteristic.Name;
            return true;
        }

        protected override bool ComputeProdart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetProdart(characteristic.Name, DataSource.Database);
            vecInfo.Prodart = characteristic.Name;
            return true;
        }

        protected override bool ComputeProduktlinie(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetProduktlinie(characteristic.Name, DataSource.Database);
            vecInfo.Produktlinie = characteristic.Name;
            return true;
        }

        protected override bool ComputeSicherheitsrelevant(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetSicherheitsrelevant(characteristic.Name, DataSource.Database);
            vecInfo.Sicherheitsrelevant = characteristic.Name;
            return true;
        }

        protected override bool ComputeTueren(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetTueren(characteristic.Name, DataSource.Database);
            vecInfo.Tueren = characteristic.Name;
            return true;
        }

        protected override bool ComputeTyp(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetTyp(characteristic.Name, DataSource.Database);
            vecInfo.Typ = characteristic.Name;
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
				vecInfo.VerkaufsBezeichnung = characteristic.Name;
			}
			else
			{
				vecInfo.VerkaufsBezeichnung = DefaultEmptyCharacteristicValue;
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
            GetIdentParameters(parameters);
            //reactor.SetEMOTBaureihe(characteristic.Name, DataSource.Database);
            vecInfo.EMotor.EMOTBaureihe = characteristic.Name;
            if (string.IsNullOrWhiteSpace(vecInfo.GenericMotor.Engine2) || vecInfo.GenericMotor.Engine2 == DefaultEmptyCharacteristicValue)
            {
                vecInfo.GenericMotor.Engine2 = characteristic.Name;
            }
            return true;
        }

        protected override bool ComputeEMOTBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetEMOTBezeichnung(characteristic.Name, DataSource.Database);
            vecInfo.EMotor.EMOTBezeichnung = this.characteristic.Name;
            if (string.IsNullOrWhiteSpace(vecInfo.GenericMotor.EngineLabel2) || vecInfo.GenericMotor.EngineLabel2 == DefaultEmptyCharacteristicValue)
            {
                vecInfo.GenericMotor.EngineLabel2 = characteristic.Name;
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
            GetIdentParameters(parameters);
            //reactor.SetLeistungsklasse(characteristic.Name, DataSource.Database);
            vecInfo.Leistungsklasse = characteristic.Name;
            return true;
        }

        protected override bool ComputeMotor(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetMotor(characteristic.Name, DataSource.Database);
            vecInfo.Motor = characteristic.Name;
            return true;
        }

        protected override bool ComputeHubraum(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetHubraum(characteristic.Name, DataSource.Database);
            vecInfo.Hubraum = characteristic.Name;
            return true;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetKraftstoffart(characteristic.Name, DataSource.Database);
            vecInfo.Kraftstoffart = characteristic.Name;
            return true;
        }

        protected override bool ComputeMOTBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetMOTBezeichnung(characteristic.Name, DataSource.Database);
            vecInfo.MOTBezeichnung = characteristic.Name;
            return true;
        }

        protected override bool ComputeMOTEinbaulage(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetMOTEinbaulage(characteristic.Name, DataSource.Database);
            vecInfo.MOTEinbaulage = characteristic.Name;
            return true;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetUeberarbeitung(characteristic.Name, DataSource.Database);
            vecInfo.Ueberarbeitung = characteristic.Name;
            return true;
        }

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //reactor.SetKraftstoffartEinbaulage(characteristic.Name, DataSource.Database);
            vecInfo.KraftstoffartEinbaulage = this.characteristic.Name;
            return true;
        }

		protected override bool ComputeEngine2(params object[] parameters)
		{
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTBaureihe = characteristic.Name;
            if (string.IsNullOrWhiteSpace(vecInfo.GenericMotor.Engine2) || characteristic.Name != DefaultEmptyCharacteristicValue)
            {
                vecInfo.GenericMotor.Engine2 = string.Join(",", vecInfo.HeatMotors.Select((HeatMotor v) => v.HeatMOTBaureihe));
            }
            return true;
        }

		protected override bool ComputeEngineLabel2(params object[] parameters)
		{
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTBezeichnung = characteristic.Name;
            //reactor.AddInfoToDataholderAboutHeatMotors(vecInfo.HeatMotors, DataSource.Database);
            if (string.IsNullOrWhiteSpace(vecInfo.GenericMotor.EngineLabel2) || characteristic.Name != DefaultEmptyCharacteristicValue)
            {
                vecInfo.GenericMotor.EngineLabel2 = string.Join(",", vecInfo.HeatMotors.Select((HeatMotor v) => v.HeatMOTBezeichnung));
            }
            return true;
		}

        protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTFortlaufendeNum = characteristic.Name;
            return true;
        }

        protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTKraftstoffart = characteristic.Name;
            return true;
        }

        protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTLebenszyklus = characteristic.Name;
            return true;
        }

        protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTLeistungsklasse = characteristic.Name;
            return true;
        }

        protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTPlatzhalter1 = characteristic.Name;
            return true;
        }

        protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTPlatzhalter2 = characteristic.Name;
            return true;
        }

        protected override bool ComputeDefault(params object[] parameters)
		{
			GetIdentParameters(parameters);
            //Log.Warning("VehicleIdent.UpdateVehicleCharacteristics()", "found unknown key:{0} value: {1}", characteristic.RootNodeClass, characteristic.Name);
            return false;
		}

        private void GetIdentParameters(params object[] parameters)
        {
            vecInfo = (Vehicle)parameters[0];
            characteristic = (PsdzDatabase.Characteristics)parameters[1];
        }

        private HeatMotor GetHeatMotorByDriveId(string driveId)
        {
            HeatMotor heatMotor = vecInfo.HeatMotors.FirstOrDefault((HeatMotor m) => m.DriveId.Equals(driveId));
            if (heatMotor != null)
            {
                return heatMotor;
            }
            heatMotor = new HeatMotor
            {
                DriveId = driveId
            };
            vecInfo.HeatMotors.Add(heatMotor);
            return heatMotor;
        }

        public const string DefaultEmptyCharacteristicValue = "-";

		private Vehicle vecInfo;

		private PsdzDatabase.Characteristics characteristic;
	}
}
