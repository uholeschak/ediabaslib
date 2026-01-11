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
        [PreserveSource(Hint = "Database modified")]
        private PsdzDatabase.Characteristics characteristic;
        [PreserveSource(Hint = "Characteristics modified", SignatureModified = true)]
        public bool AssignBasicFeaturesVciCharacteristic(string vehicleCode, BasicFeaturesVci vehicle, PsdzDatabase.Characteristics characteristic)
        {
            return ComputeCharacteristic(vehicleCode, vehicle, characteristic);
        }

        [PreserveSource(Hint = "Use EcuTranslation", SignatureModified = true)]
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

        [PreserveSource(Hint = "Use EcuTranslation", SignatureModified = true)]
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

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "39A39BEE743E60202A45736B63D41B4D")]
        protected override bool ComputeBaseVersion(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            basicFeatures.BaseVersion = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeBasicType(params object[] parameters)
        {
            return true;
        }

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "DD5A7A57028965E3E904C4255884F341")]
        protected override bool ComputeBaureihe(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
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

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "B8DEB2244087B6025E48B31AAB89D846")]
        protected override bool ComputeBrandName(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            basicFeatures.Marke = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "E6F5C513F735116F76E5B17FE83B2E88")]
        protected override bool ComputeCountryOfAssembly(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
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

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "C4C47519AD135D1C0E883089FCCF1085")]
        protected override bool ComputeEreihe(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            basicFeatures.Ereihe = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "F69B5855CEB575D813D14C12E260F334")]
        protected override bool ComputeGetriebe(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
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

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "D280F8952E087B101C92CF023BEF0A15")]
        protected override bool ComputeKarosserie(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            basicFeatures.Karosserie = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            return true;
        }

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "3F7479012C995EB80D6572AE131DE213")]
        protected override bool ComputeLand(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            basicFeatures.Land = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeLeistungsklasse(params object[] parameters)
        {
            return true;
        }

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "4B080246D6A89C8BBE8AE7AE757A5140")]
        protected override bool ComputeLenkung(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
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

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "F1B57DCADD2F8FE1418BC3067E27A08B")]
        protected override bool ComputeProdart(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
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

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "2F99642DA63A32FC3C1103DEAA938FDC")]
        protected override bool ComputeTyp(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
            basicFeatures.TypeCode = characteristic.EcuTranslation.GetTitle(_clientContext);
            return true;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            return true;
        }

        [PreserveSource(Hint = "Use EcuTranslation", OriginalHash = "C280A37FEBEA2977BAEDD3DF5C11D62B")]
        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            GetVCIDeviceParameters(parameters);
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

        [PreserveSource(Hint = "Characteristics modified", OriginalHash = "4F004AFB32A41631FF0A0D5BEFA63FA7")]
        private void GetVCIDeviceParameters(params object[] parameters)
        {
            basicFeatures = (BasicFeaturesVci)parameters[0];
            characteristic = (PsdzDatabase.Characteristics)parameters[1];
        }

        [PreserveSource(Hint = "Added")]
        private ClientContext _clientContext;
        [PreserveSource(Hint = "Added")]
        public VehicleCharacteristicVCIDeviceHelper(ClientContext clientContext)
        {
            _clientContext = clientContext;
        }
    }
}