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
    public sealed class VehicleCharacteristicIdent : VehicleCharacteristicAbstract
    {
        public const string DefaultEmptyCharacteristicValue = "-";
        private IIdentVehicle vecInfo;
        [PreserveSource(Hint = "Database modified")]
        private PsdzDatabase.Characteristics characteristic;
        private ReactorEngine reactor;
        private readonly ILogger log;
        [PreserveSource(Hint = "Database modified", OriginalHash = "7E547C731AEA645296329C5AA599CF35")]
        public VehicleCharacteristicIdent(ILogger log)
        {
            this.log = log;
        }

        [PreserveSource(Hint = "Characteristic modified", SignatureModified = true)]
        public bool AssignVehicleCharacteristic(string vehicleCode, IIdentVehicle vehicle, PsdzDatabase.Characteristics characteristic)
        {
            return ComputeCharacteristic(vehicleCode, vehicle, characteristic);
        }

        protected override bool ComputeAEBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetAEBezeichnung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetAEKurzbezeichnung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeAELeistungsklasse(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetAELeistungsklasse(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeAEUeberarbeitung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetAEUeberarbeitung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeAntrieb(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetAntrieb(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBaseVersion(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetBaseVersion(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBasicType(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetBasicType(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBaureihe(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetBaureihe(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBaureihenverbund(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetBaureihenverbund(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBaustandsJahr(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetBaustandsJahr(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBaustandsMonat(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetBaustandsMonat(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeBrandName(params object[] parameters)
        {
            GetIdentParameters(parameters);
            string text = characteristic.Name.ToUpper(CultureInfo.InvariantCulture);
            reactor.SetMarke(text, DataSource.Database);
            switch (text)
            {
                case "BMW PKW":
                    reactor.SetBrandName(BrandName.BMWPKW, DataSource.Database);
                    break;
                case "MINI PKW":
                    reactor.SetBrandName(BrandName.MINIPKW, DataSource.Database);
                    break;
                case "ROLLS-ROYCE PKW":
                    reactor.SetBrandName(BrandName.ROLLSROYCEPKW, DataSource.Database);
                    break;
                case "BMW MOTORRAD":
                    reactor.SetBrandName(BrandName.BMWMOTORRAD, DataSource.Database);
                    break;
                case "BMW M GMBH PKW":
                    reactor.SetBrandName(BrandName.BMWMGmbHPKW, DataSource.Database);
                    break;
                case "BMW USA PKW":
                    reactor.SetBrandName(BrandName.BMWUSAPKW, DataSource.Database);
                    break;
                case "BMW I":
                    reactor.SetBrandName(BrandName.BMWi, DataSource.Database);
                    break;
                case "TOYOTA":
                    reactor.SetBrandName(BrandName.TOYOTA, DataSource.Database);
                    break;
                default:
                    log.Warning("VehicleIdent.UpdateVehicleCharacteristics()", "found unknown brand name: {0}", characteristic.Name);
                    break;
            }

            return true;
        }

        protected override bool ComputeCountryOfAssembly(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetCountryOfAssembly(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeDrehmoment(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetDrehmoment(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeElektrischeReichweite(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetElektrischeReichweite(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEreihe(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEreihe(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeGetriebe(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetGetriebe(characteristic.Name, DataSource.Database);
            log.Info("GearboxUtility.SetGearboxTypeFromCharacteristics()", "Gearbox type: '" + characteristic.Name + "' found in the xep_characteristics table.");
            return true;
        }

        protected override bool ComputeHybridkennzeichen(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetHybridkennzeichen(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeKarosserie(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetKarosserie(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeLand(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetLand(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeLenkung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetLenkung(characteristic.Name, DataSource.Database);
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
            reactor.SetMotorarbeitsverfahren(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeProdart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetProdart(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeProduktlinie(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetProduktlinie(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeSicherheitsrelevant(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetSicherheitsrelevant(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeTueren(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetTueren(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeTyp(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetTyp(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            //[-] decimal? istaVisible = characteristic.IstaVisible;
            //[+] decimal? istaVisible = null;
            decimal? istaVisible = null;
            //[+] if (decimal.TryParse(characteristic.IstaVisible, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal visible))
            if (decimal.TryParse(characteristic.IstaVisible, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal visible))
            //[+] {
            {
                //[+] istaVisible = visible;
                istaVisible = visible;
            //[+] }
            }

            if (!((istaVisible.GetValueOrDefault() == default(decimal)) & istaVisible.HasValue))
            {
                reactor.SetVerkaufsBezeichnung(characteristic.Name, DataSource.Database);
            }
            else
            {
                reactor.SetVerkaufsBezeichnung("-", DataSource.Hardcoded);
            }

            return true;
        }

        protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTArbeitsverfahren(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTBaureihe(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTBaureihe(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTBezeichnung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTDrehmoment(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTDrehmoment(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTEinbaulage(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTEinbaulage(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTKraftstoffart(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTLeistungsklasse(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetEMOTUeberarbeitung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeLeistungsklasse(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetLeistungsklasse(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeMotor(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetMotor(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeHubraum(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetHubraum(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetKraftstoffart(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeMOTBezeichnung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetMOTBezeichnung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeMOTEinbaulage(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetMOTEinbaulage(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetUeberarbeitung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetKraftstoffartEinbaulage(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeEngine2(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTBaureihe = characteristic.Name;
            return true;
        }

        protected override bool ComputeEngineLabel2(params object[] parameters)
        {
            GetIdentParameters(parameters);
            GetHeatMotorByDriveId(characteristic.DriveId).HeatMOTBezeichnung = characteristic.Name;
            reactor.AddInfoToDataholderAboutHeatMotors(vecInfo.HeatMotors, DataSource.Database);
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

        protected override bool ComputeTypeKeyLead(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetTypeKeyLead(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeTypeKeyBasic(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetTypeKeyBasic(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeESeriesLifeCycle(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetESeriesLifeCycle(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeLifeCycle(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetLifeCycle(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeSportausfuehrung(params object[] parameters)
        {
            GetIdentParameters(parameters);
            reactor.SetSportausfuehrung(characteristic.Name, DataSource.Database);
            return true;
        }

        protected override bool ComputeDefault(params object[] parameters)
        {
            GetIdentParameters(parameters);
            log.Warning("VehicleIdent.UpdateVehicleCharacteristics()", "found unknown key:{0} value: {1}", characteristic.RootNodeClass, characteristic.Name);
            return false;
        }

        private void GetIdentParameters(params object[] parameters)
        {
            vecInfo = (IIdentVehicle)parameters[0];
            //[-] characteristic = (IXepCharacteristics)parameters[1];
            //[+] reactor = (vecInfo as Vehicle)?.Reactor;
            reactor = (vecInfo as Vehicle)?.Reactor;
            //[+] characteristic = (PsdzDatabase.Characteristics)parameters[1];
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
    }
}