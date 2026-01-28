using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public sealed class VehicleCharacteristicVCIDeviceHelper : VehicleCharacteristicAbstract
    {
        private BasicFeaturesVci basicFeatures;
        [PreserveSource(Hint = "Database modified", SuppressWarning = true)]
        private PsdzDatabase.Characteristics characteristic;
        [PreserveSource(Hint = "Characteristics modified", SignatureModified = true)]
        public bool AssignBasicFeaturesVciCharacteristic(string vehicleCode, BasicFeaturesVci vehicle, PsdzDatabase.Characteristics characteristic)
        {
            return ComputeCharacteristic(vehicleCode, vehicle, characteristic);
        }

        protected override bool ComputeMotor(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Motor = characteristic.Title;
            //[+] basicFeatures.Motor = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Motor = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeAEBezeichnung(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.AEKurzbezeichnung = characteristic.Title;
            //[+] basicFeatures.AEKurzbezeichnung = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.AEKurzbezeichnung = characteristic.EcuTranslation.GetTitle(_clientContext);
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
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.BaseVersion = characteristic.Title;
            //[+] basicFeatures.BaseVersion = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.BaseVersion = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeBasicType(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeBaureihe(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Baureihe = characteristic.Title;
            //[+] basicFeatures.Baureihe = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Baureihe = characteristic.EcuTranslation.GetTitle(_clientContext);
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
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Marke = characteristic.Title;
            //[+] basicFeatures.Marke = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Marke = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeCountryOfAssembly(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.CountryOfAssembly = characteristic.Title;
            //[+] basicFeatures.CountryOfAssembly = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.CountryOfAssembly = characteristic.EcuTranslation.GetTitle(_clientContext);
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
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Ereihe = characteristic.Title;
            //[+] basicFeatures.Ereihe = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Ereihe = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeGetriebe(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Getriebe = characteristic.Title;
            //[+] basicFeatures.Getriebe = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Getriebe = characteristic.EcuTranslation.GetTitle(_clientContext);
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
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Karosserie = characteristic.Title;
            //[+] basicFeatures.Karosserie = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Karosserie = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeLand(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Land = characteristic.Title;
            //[+] basicFeatures.Land = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Land = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeLeistungsklasse(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeLenkung(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Lenkung = characteristic.Title;
            //[+] basicFeatures.Lenkung = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Lenkung = characteristic.EcuTranslation.GetTitle(_clientContext);
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
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.Prodart = characteristic.Title;
            //[+] basicFeatures.Prodart = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.Prodart = characteristic.EcuTranslation.GetTitle(_clientContext);
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
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.TypeCode = characteristic.Title;
            //[+] basicFeatures.TypeCode = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.TypeCode = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            //[-] basicFeatures.VerkaufsBezeichnung = characteristic.Title;
            //[+] basicFeatures.VerkaufsBezeichnung = characteristic.EcuTranslation.GetTitle(_clientContext);
            basicFeatures.VerkaufsBezeichnung = characteristic.EcuTranslation.GetTitle(_clientContext);
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

        protected override bool ComputeTypeKeyLead(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeTypeKeyBasic(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeESeriesLifeCycle(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeLifeCycle(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeSportausfuehrung(params object[] parameters)
        {
            return true;
        }

        protected override bool ComputeDefault(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            Log.Error("VCIDeviceHelper.get_Basicfeature", "Unknown BasicFeature for nodeclass id {0}!", characteristic.RootNodeClass);
            return false;
        }

        private void GetVCIDeviceParameters(params object[] parameters)
        {
            basicFeatures = (BasicFeaturesVci)parameters[0];
            //[-] characteristic = (XEP_CHARACTERISTICS)parameters[1];
            //[+] characteristic = (PsdzDatabase.Characteristics)parameters[1];
            characteristic = (PsdzDatabase.Characteristics)parameters[1];
        }

        [PreserveSource(Added = true)]
        private ClientContext _clientContext;
        [PreserveSource(Added = true)]
        public VehicleCharacteristicVCIDeviceHelper(ClientContext clientContext)
        {
            _clientContext = clientContext;
        }
    }
}