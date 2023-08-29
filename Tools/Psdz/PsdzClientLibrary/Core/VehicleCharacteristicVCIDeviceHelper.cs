using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class VehicleCharacteristicVCIDeviceHelper : VehicleCharacteristicAbstract
	{
        public VehicleCharacteristicVCIDeviceHelper(ClientContext clientContext)
        {
            _clientContext = clientContext;
        }

		public bool AssignBasicFeaturesVciCharacteristic(string vehicleCode, BasicFeaturesVci vehicle, PsdzDatabase.Characteristics characteristic)
		{
			return base.ComputeCharacteristic(vehicleCode, new object[]
			{
				vehicle,
				characteristic
			});
		}

		protected override bool ComputeMotor(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Motor = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeAbgas(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeAEBezeichnung(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.AEKurzbezeichnung = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeAELeistungsklasse(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeAEUeberarbeitung(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeAntrieb(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeBaseVersion(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.BaseVersion = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeBasicType(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeBaureihe(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Baureihe = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeBaureihenverbund(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeBaustandsJahr(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeBaustandsMonat(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeBrandName(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Marke = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeCountryOfAssembly(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.CountryOfAssembly = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeDrehmoment(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeElektrischeReichweite(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTBaureihe(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTBezeichnung(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTDrehmoment(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTEinbaulage(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEreihe(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Ereihe = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeGetriebe(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Getriebe = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeHubraum(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHybridkennzeichen(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeKarosserie(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Karosserie = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeKraftstoffart(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeLand(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Land = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeLeistungsklasse(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeLenkung(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Lenkung = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeMOTBezeichnung(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeMOTEinbaulage(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeMOTKraftstoffart(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeProdart(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.Prodart = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeProduktlinie(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeSicherheitsrelevant(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeTueren(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeTyp(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.TypeCode = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeUeberarbeitung(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			this.basicFeatures.VerkaufsBezeichnung = this.characteristic.EcuTranslation.GetTitle(_clientContext);
			return true;
		}

		protected override bool ComputeEngine2(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeEngineLabel2(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
		{
			return true;
		}

		protected override bool ComputeDefault(params object[] parameters)
		{
			this.GetVCIDeviceParameters(parameters);
			return false;
		}

		private void GetVCIDeviceParameters(params object[] parameters)
		{
			this.basicFeatures = (BasicFeaturesVci)parameters[0];
			this.characteristic = (PsdzDatabase.Characteristics)parameters[1];
		}

		private BasicFeaturesVci basicFeatures;

		private PsdzDatabase.Characteristics characteristic;

        private ClientContext _clientContext;
    }
}
