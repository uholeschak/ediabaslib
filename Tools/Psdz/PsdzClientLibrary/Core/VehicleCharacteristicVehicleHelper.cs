using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PsdzClient.Core.ValidationRuleInternalResult;

namespace PsdzClient.Core
{
	public class VehicleCharacteristicVehicleHelper : VehicleCharacteristicAbstract
	{
        [PreserveSource(Hint = "Dataprovider changed")]
        PsdzDatabase dataProvider;

        private string characteristicValue;

        [PreserveSource(Hint = "changed to string")]
        private string characteristicRootsNodeClass;

        private decimal characteristicId;

        private IVehicleRuleEvaluation vehicle;

        private long datavalueId;

        private ValidationRuleInternalResults internalResult;

        [PreserveSource(Hint = "dbConnector removed")]
        public VehicleCharacteristicVehicleHelper(IVehicleRuleEvaluation vehicle)
        {
            characteristicValue = string.Empty;
            this.vehicle = vehicle;
            internalResult = new ValidationRuleInternalResults();
        }

        [PreserveSource(Hint = "characteristicRootsNodeClass to string")]
        public bool GetISTACharacteristics(string characteristicRootsNodeClass, out string value, decimal id, IVehicleRuleEvaluation vehicle, long dataValueId, ValidationRuleInternalResults internalResult)
        {
            this.characteristicRootsNodeClass = characteristicRootsNodeClass;
            characteristicId = id;
            this.vehicle = vehicle;
            datavalueId = dataValueId;
            this.internalResult = internalResult;
            dataProvider = ClientContext.GetDatabase(vehicle as Vehicle);
            bool result = ComputeCharacteristic(characteristicRootsNodeClass);
            value = characteristicValue;
            return result;
        }

        protected override bool ComputeAEBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.AEBezeichnung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.AEBezeichnung, 99999999849L) == (decimal)datavalueId;
        }

        protected override bool ComputeAEKurzbezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.AEKurzbezeichnung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.AEKurzbezeichnung, 99999999913L) == (decimal)datavalueId;
        }

        protected override bool ComputeAELeistungsklasse(params object[] parameters)
        {
            characteristicValue = vehicle.AELeistungsklasse;
            return dataProvider.LookupVehicleCharIdByName(vehicle.AELeistungsklasse, 99999999914L) == (decimal)datavalueId;
        }

        protected override bool ComputeAEUeberarbeitung(params object[] parameters)
        {
            characteristicValue = vehicle.AEUeberarbeitung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.AEUeberarbeitung, 99999999915L) == (decimal)datavalueId;
        }

        protected override bool ComputeKraftstoffartEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.KraftstoffartEinbaulage;
            return dataProvider.LookupVehicleCharIdByName(vehicle.KraftstoffartEinbaulage, 53330059) == (decimal)datavalueId;
        }

        protected override bool ComputeAntrieb(params object[] parameters)
        {
            characteristicValue = vehicle.Antrieb;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Antrieb, 40124162) == (decimal)datavalueId;
        }

        protected override bool ComputeBaseVersion(params object[] parameters)
        {
            characteristicValue = vehicle.BaseVersion;
            return dataProvider.LookupVehicleCharIdByName(vehicle.BaseVersion, 99999999852L) == (decimal)datavalueId;
        }

        protected override bool ComputeBasicType(params object[] parameters)
        {
            characteristicValue = vehicle.BasicType;
            return dataProvider.LookupVehicleCharIdByName(vehicle.BasicType, 99999999912L) == (decimal)datavalueId;
        }

        protected override bool ComputeBaureihe(params object[] parameters)
        {
            characteristicValue = vehicle.Baureihe;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Baureihe, 40126722) == (decimal)datavalueId;
        }

        protected override bool ComputeBaureihenverbund(params object[] parameters)
        {
            characteristicValue = vehicle.Baureihenverbund;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Baureihenverbund, 99999999951L) == (decimal)datavalueId;
        }

        protected override bool ComputeBaustandsJahr(params object[] parameters)
        {
            characteristicValue = vehicle.Modelljahr;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Modelljahr, null) == (decimal)datavalueId;
        }

        protected override bool ComputeBaustandsMonat(params object[] parameters)
        {
            characteristicValue = vehicle.Modellmonat;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Modellmonat, null) == (decimal)datavalueId;
        }

        protected override bool ComputeBrandName(params object[] parameters)
        {
            characteristicValue = vehicle.Marke;
            if (string.Equals("BMW I", vehicle.Marke, StringComparison.OrdinalIgnoreCase))
            {
                return dataProvider.LookupVehicleCharIdByName("BMW I", 40139010) == (decimal)datavalueId;
            }
            return dataProvider.LookupVehicleCharIdByName(vehicle.Marke, 40139010) == (decimal)datavalueId;
        }

        protected override bool ComputeCountryOfAssembly(params object[] parameters)
        {
            characteristicValue = vehicle.CountryOfAssembly;
            return dataProvider.LookupVehicleCharIdByName(vehicle.CountryOfAssembly, 99999999853L) == (decimal)datavalueId;
        }

        protected override bool ComputeDefault(params object[] parameters)
        {
            characteristicValue = "???";
            return false;
        }

        protected override bool ComputeDrehmoment(params object[] parameters)
        {
            characteristicValue = vehicle.Drehmoment;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Drehmoment, 68771232898L) == (decimal)datavalueId;
        }

        protected override bool ComputeElektrischeReichweite(params object[] parameters)
        {
            characteristicValue = vehicle.ElektrischeReichweite;
            return dataProvider.LookupVehicleCharIdByName(vehicle.ElektrischeReichweite, 99999999855L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTArbeitsverfahren(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTArbeitsverfahren;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTArbeitsverfahren, 99999999877L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTBaureihe(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTBaureihe;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTBaureihe, 99999999879L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTBezeichnung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTBezeichnung, 99999999869L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTDrehmoment(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTDrehmoment;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTDrehmoment, 99999999875L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTEinbaulage;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTEinbaulage, 99999999865L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTKraftstoffart(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTKraftstoffart;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTKraftstoffart, 99999999867L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTLeistungsklasse(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTLeistungsklasse;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTLeistungsklasse, 99999999873L) == (decimal)datavalueId;
        }

        protected override bool ComputeEMOTUeberarbeitung(params object[] parameters)
        {
            characteristicValue = vehicle.EMotor.EMOTUeberarbeitung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.EMotor.EMOTUeberarbeitung, 99999999871L) == (decimal)datavalueId;
        }

        protected override bool ComputeEngine2(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTBaureihe, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999712m);
        }

        protected override bool ComputeEngineLabel2(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTBezeichnung, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999711m);
        }

        protected override bool ComputeEreihe(params object[] parameters)
        {
            characteristicValue = vehicle.Ereihe;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Ereihe, 40128130) == (decimal)datavalueId;
        }

        protected override bool ComputeGetriebe(params object[] parameters)
        {
            characteristicValue = vehicle.Getriebe;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Getriebe, 40137602) == (decimal)datavalueId;
        }

        protected override bool ComputeHeatMOTFortlaufendeNum(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTFortlaufendeNum, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999715m);
        }

        protected override bool ComputeHeatMOTKraftstoffart(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTKraftstoffart, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999718m);
        }

        protected override bool ComputeHeatMOTLebenszyklus(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLebenszyklus, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999717m);
        }

        protected override bool ComputeHeatMOTLeistungsklasse(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTLeistungsklasse, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999716m);
        }

        protected override bool ComputeHeatMOTPlatzhalter1(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter1, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999713m);
        }

        protected override bool ComputeHeatMOTPlatzhalter2(params object[] parameters)
        {
            return HandleHeatMotorCharacteristic((HeatMotor hm) => hm.HeatMOTPlatzhalter2, datavalueId, internalResult, out characteristicValue, characteristicRootsNodeClass, 99999999714m);
        }

        protected override bool ComputeHubraum(params object[] parameters)
        {
            characteristicValue = vehicle.Hubraum;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Hubraum, 40131586) == (decimal)datavalueId;
        }

        protected override bool ComputeHybridkennzeichen(params object[] parameters)
        {
            characteristicValue = vehicle.Hybridkennzeichen;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Hybridkennzeichen, 68771232514L) == (decimal)datavalueId;
        }

        protected override bool ComputeKarosserie(params object[] parameters)
        {
            characteristicValue = vehicle.Karosserie;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Karosserie, 40133634) == (decimal)datavalueId;
        }

        protected override bool ComputeKraftstoffart(params object[] parameters)
        {
            characteristicValue = vehicle.Kraftstoffart;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Kraftstoffart, 40125442) == (decimal)datavalueId;
        }

        protected override bool ComputeLand(params object[] parameters)
        {
            characteristicValue = vehicle.Land;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Land, 40129538) == (decimal)datavalueId;
        }

        protected override bool ComputeLeistungsklasse(params object[] parameters)
        {
            characteristicValue = vehicle.Leistungsklasse;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Leistungsklasse, 40136322) == (decimal)datavalueId;
        }

        protected override bool ComputeLenkung(params object[] parameters)
        {
            characteristicValue = vehicle.Lenkung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Lenkung, 40124802) == (decimal)datavalueId;
        }

        protected override bool ComputeMOTBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.MOTBezeichnung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.MOTBezeichnung, 99999999919L) == (decimal)datavalueId;
        }

        protected override bool ComputeMOTEinbaulage(params object[] parameters)
        {
            characteristicValue = vehicle.MOTEinbaulage;
            return dataProvider.LookupVehicleCharIdByName(vehicle.MOTEinbaulage, 99999999916L) == (decimal)datavalueId;
        }

        protected override bool ComputeMOTKraftstoffart(params object[] parameters)
        {
            characteristicValue = vehicle.MOTKraftstoffart;
            return dataProvider.LookupVehicleCharIdByName(vehicle.MOTKraftstoffart, 99999999917L) == (decimal)datavalueId;
        }

        protected override bool ComputeMotor(params object[] parameters)
        {
            characteristicValue = vehicle.Motor;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Motor, 40132226) == (decimal)datavalueId;
        }

        protected override bool ComputeMotorarbeitsverfahren(params object[] parameters)
        {
            characteristicValue = vehicle.Motorarbeitsverfahren;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Motorarbeitsverfahren, 68771231746L) == (decimal)datavalueId;
        }

        protected override bool ComputeProdart(params object[] parameters)
        {
            characteristicValue = vehicle.Prodart;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Prodart, 40135682) == (decimal)datavalueId;
        }

        protected override bool ComputeProduktlinie(params object[] parameters)
        {
            characteristicValue = vehicle.Produktlinie;
            if (vehicle.BrandName == BrandName.TOYOTA)
            {
                decimal num = dataProvider.LookupVehicleCharIdByName(vehicle.Produktlinie, 40039952514L);
                return (num != 0m) ? (num == (decimal)datavalueId) : (dataProvider.LookupVehicleCharIdByName("MINI", 40039952514L) == (decimal)datavalueId);
            }
            return dataProvider.LookupVehicleCharIdByName(vehicle.Produktlinie, 40039952514L) == (decimal)datavalueId;
        }

        protected override bool ComputeSicherheitsrelevant(params object[] parameters)
        {
            string name = (characteristicValue = vehicle.Sicherheitsrelevant);
            return dataProvider.LookupVehicleCharIdByName(name, 40136962) == (decimal)datavalueId;
        }

        protected override bool ComputeTueren(params object[] parameters)
        {
            characteristicValue = vehicle.Tueren;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Tueren, 40130946) == (decimal)datavalueId;
        }

        protected override bool ComputeTyp(params object[] parameters)
        {
            characteristicValue = vehicle.GMType;
            return dataProvider.LookupVehicleCharIdByName(vehicle.GMType, 40135042) == (decimal)datavalueId;
        }

        protected override bool ComputeUeberarbeitung(params object[] parameters)
        {
            characteristicValue = vehicle.Ueberarbeitung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Ueberarbeitung, 40123522) == (decimal)datavalueId;
        }

        protected override bool ComputeVerkaufsBezeichnung(params object[] parameters)
        {
            characteristicValue = vehicle.VerkaufsBezeichnung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.VerkaufsBezeichnung, 40122114) == (decimal)datavalueId;
        }

        protected override bool ComputeTypeKeyLead(params object[] parameters)
        {
            characteristicValue = vehicle.BasicType;
            return dataProvider.LookupVehicleCharIdByName(vehicle.TypeKeyLead, 99999999912L) == (decimal)datavalueId;
        }

        protected override bool ComputeTypeKeyBasic(params object[] parameters)
        {
            characteristicValue = vehicle.TypeKeyBasic;
            return dataProvider.LookupVehicleCharIdByName(vehicle.TypeKeyBasic, 40135043) == (decimal)datavalueId;
        }

        protected override bool ComputeESeriesLifeCycle(params object[] parameters)
        {
            characteristicValue = vehicle.ESeriesLifeCycle;
            return dataProvider.LookupVehicleCharIdByName(vehicle.ESeriesLifeCycle, 99999999859L) == (decimal)datavalueId;
        }

        protected override bool ComputeLifeCycle(params object[] parameters)
        {
            characteristicValue = vehicle.LifeCycle;
            return dataProvider.LookupVehicleCharIdByName(vehicle.LifeCycle, 99999999857L) == (decimal)datavalueId;
        }

        protected override bool ComputeSportausfuehrung(params object[] parameters)
        {
            characteristicValue = vehicle.Sportausfuehrung;
            return dataProvider.LookupVehicleCharIdByName(vehicle.Sportausfuehrung, 99999999847L) == (decimal)datavalueId;
        }

        [PreserveSource(Hint = "Adapted")]
        private bool HandleHeatMotorCharacteristic(Func<HeatMotor, string> getProperty, long datavalueId, ValidationRuleInternalResults internalResult, out string value, string rootNodeClass, decimal characteristicNodeclass)
        {
            if (!decimal.TryParse(rootNodeClass, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal rootClassValue))
            {
                rootClassValue = 0;
            }

            foreach (HeatMotor hm in vehicle.HeatMotors)
            {
                ValidationRuleInternalResult validationRuleInternalResult = internalResult.FirstOrDefault((ValidationRuleInternalResult r) => r.Id == hm.DriveId && r.Type == CharacteristicType.HeatMotor && r.CharacteristicId == rootClassValue);
                decimal num = dataProvider.LookupVehicleCharIdByName(getProperty(hm), characteristicNodeclass);
                bool flag = num == (decimal)datavalueId;
                if (validationRuleInternalResult == null)
                {
                    validationRuleInternalResult = new ValidationRuleInternalResult
                    {
                        Type = CharacteristicType.HeatMotor,
                        Id = hm.DriveId,
                        CharacteristicId = rootClassValue
                    };
                    if (!(internalResult.RuleExpression is OrExpression))
                    {
                        validationRuleInternalResult.IsValid = true;
                    }
                    internalResult.Add(validationRuleInternalResult);
                }
                IRuleExpression ruleExpression = internalResult.RuleExpression;
                IRuleExpression ruleExpression2 = ruleExpression;
                if (!(ruleExpression2 is AndExpression))
                {
                    if (!(ruleExpression2 is OrExpression))
                    {
                        if (ruleExpression2 is NotExpression)
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
            value = string.Join(",", vehicle.HeatMotors.Select((HeatMotor arg) => getProperty(arg)));
            bool flag2 = (from r in internalResult
                group r by r.Id).Any((IGrouping<string, ValidationRuleInternalResult> g) => g.All((ValidationRuleInternalResult c) => c.IsValid));
            return (internalResult.RuleExpression is NotExpression) ? (!flag2) : flag2;
        }
    }
}
