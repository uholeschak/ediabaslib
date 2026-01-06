using System;

namespace PsdzClient.Core
{
    public abstract class VehicleCharacteristicAbstract
    {
        protected bool ComputeCharacteristic(string vehicleCode, params object[] param)
        {
            if (Enum.TryParse<VehicleCharacteristic>(vehicleCode, out var result))
            {
                VehicleCharacteristic vehicleCharacteristic = result;
                VehicleCharacteristic vehicleCharacteristic2 = vehicleCharacteristic;
                if (vehicleCharacteristic2 <= VehicleCharacteristic.Tueren)
                {
                    switch (vehicleCharacteristic2)
                    {
                        case VehicleCharacteristic.Motor:
                            return ComputeMotor(param);
                        case VehicleCharacteristic.Baureihe:
                            return ComputeBaureihe(param);
                        case VehicleCharacteristic.Lenkung:
                            return ComputeLenkung(param);
                        case VehicleCharacteristic.Hubraum:
                            return ComputeHubraum(param);
                        case VehicleCharacteristic.Getriebe:
                            return ComputeGetriebe(param);
                        case VehicleCharacteristic.VerkaufsBezeichnung:
                            return ComputeVerkaufsBezeichnung(param);
                        case VehicleCharacteristic.Typ:
                            return ComputeTyp(param);
                        case VehicleCharacteristic.Antrieb:
                            return ComputeAntrieb(param);
                        case VehicleCharacteristic.Leistungsklasse:
                            return ComputeLeistungsklasse(param);
                        case VehicleCharacteristic.Prodart:
                            return ComputeProdart(param);
                        case VehicleCharacteristic.Ereihe:
                            return ComputeEreihe(param);
                        case VehicleCharacteristic.Tueren:
                            return ComputeTueren(param);
                        case VehicleCharacteristic.Kraftstoffart:
                            return ComputeKraftstoffart(param);
                        case VehicleCharacteristic.BaustandsJahr:
                            return ComputeBaustandsJahr(param);
                        case VehicleCharacteristic.BaustandsMonat:
                            return ComputeBaustandsMonat(param);
                        case VehicleCharacteristic.TypeKeyBasic:
                            return ComputeTypeKeyBasic(param);
                    }
                }
                else if (vehicleCharacteristic2 <= VehicleCharacteristic.Hybridkennzeichen)
                {
                    switch (vehicleCharacteristic2)
                    {
                        case VehicleCharacteristic.Karosserie:
                            return ComputeKarosserie(param);
                        case VehicleCharacteristic.BrandName:
                            return ComputeBrandName(param);
                        case VehicleCharacteristic.Ueberarbeitung:
                            return ComputeUeberarbeitung(param);
                        case VehicleCharacteristic.Land:
                            return ComputeLand(param);
                        case VehicleCharacteristic.Hybridkennzeichen:
                            return ComputeHybridkennzeichen(param);
                        case VehicleCharacteristic.Produktlinie:
                            return ComputeProduktlinie(param);
                        case VehicleCharacteristic.Sicherheitsrelevant:
                            return ComputeSicherheitsrelevant(param);
                        case VehicleCharacteristic.KraftstoffartEinbaulage:
                            return ComputeKraftstoffartEinbaulage(param);
                    }
                }
                else if (vehicleCharacteristic2 <= VehicleCharacteristic.ESeriesLifeCycle)
                {
                    if (vehicleCharacteristic2 <= VehicleCharacteristic.Drehmoment)
                    {
                        switch (vehicleCharacteristic2)
                        {
                            case VehicleCharacteristic.Motorarbeitsverfahren:
                                return ComputeMotorarbeitsverfahren(param);
                            case VehicleCharacteristic.Drehmoment:
                                return ComputeDrehmoment(param);
                        }
                    }
                    else
                    {
                        VehicleCharacteristic num = vehicleCharacteristic2 - 99999999701L;
                        if ((ulong)num <= 7uL)
                        {
                            switch (num)
                            {
                                case (VehicleCharacteristic)0L:
                                    return ComputeEngineLabel2(param);
                                case (VehicleCharacteristic)1L:
                                    return ComputeEngine2(param);
                                case (VehicleCharacteristic)2L:
                                    return ComputeHeatMOTPlatzhalter1(param);
                                case (VehicleCharacteristic)3L:
                                    return ComputeHeatMOTPlatzhalter2(param);
                                case (VehicleCharacteristic)4L:
                                    return ComputeHeatMOTFortlaufendeNum(param);
                                case (VehicleCharacteristic)5L:
                                    return ComputeHeatMOTLeistungsklasse(param);
                                case (VehicleCharacteristic)6L:
                                    return ComputeHeatMOTLebenszyklus(param);
                                case (VehicleCharacteristic)7L:
                                    return ComputeHeatMOTKraftstoffart(param);
                            }
                        }

                        VehicleCharacteristic num2 = vehicleCharacteristic2 - 99999999846L;
                        if ((ulong)num2 <= 12uL)
                        {
                            switch (num2)
                            {
                                case (VehicleCharacteristic)5L:
                                    return ComputeCountryOfAssembly(param);
                                case (VehicleCharacteristic)4L:
                                    return ComputeBaseVersion(param);
                                case (VehicleCharacteristic)8L:
                                    return ComputeElektrischeReichweite(param);
                                case (VehicleCharacteristic)2L:
                                    return ComputeAEBezeichnung(param);
                                case (VehicleCharacteristic)12L:
                                    return ComputeESeriesLifeCycle(param);
                                case (VehicleCharacteristic)10L:
                                    return ComputeLifeCycle(param);
                                case (VehicleCharacteristic)0L:
                                    return ComputeSportausfuehrung(param);
                            }
                        }
                    }
                }
                else if (vehicleCharacteristic2 <= VehicleCharacteristic.MOTEinbaulage)
                {
                    VehicleCharacteristic num3 = vehicleCharacteristic2 - 99999999866L;
                    if ((ulong)num3 <= 14uL)
                    {
                        switch (num3)
                        {
                            case (VehicleCharacteristic)14L:
                                return ComputeEMOTBaureihe(param);
                            case (VehicleCharacteristic)12L:
                                return ComputeEMOTArbeitsverfahren(param);
                            case (VehicleCharacteristic)10L:
                                return ComputeEMOTDrehmoment(param);
                            case (VehicleCharacteristic)8L:
                                return ComputeEMOTLeistungsklasse(param);
                            case (VehicleCharacteristic)6L:
                                return ComputeEMOTUeberarbeitung(param);
                            case (VehicleCharacteristic)4L:
                                return ComputeEMOTBezeichnung(param);
                            case (VehicleCharacteristic)2L:
                                return ComputeEMOTKraftstoffart(param);
                            case (VehicleCharacteristic)0L:
                                return ComputeEMOTEinbaulage(param);
                            case (VehicleCharacteristic)1L:
                            case (VehicleCharacteristic)3L:
                            case (VehicleCharacteristic)5L:
                            case (VehicleCharacteristic)7L:
                            case (VehicleCharacteristic)9L:
                            case (VehicleCharacteristic)11L:
                            case (VehicleCharacteristic)13L:
                                goto IL_0702;
                        }
                    }

                    VehicleCharacteristic num4 = vehicleCharacteristic2 - 99999999905L;
                    if ((ulong)num4 <= 5uL)
                    {
                        switch (num4)
                        {
                            case (VehicleCharacteristic)4L:
                                return ComputeMOTKraftstoffart(param);
                            case (VehicleCharacteristic)0L:
                                //[-] if (param.Length > 1 && param[0] is IIdentVehicle identVehicle && param[1] is IXepCharacteristics xepCharacteristics)
                                //[+] if (param.Length > 1 && param[0] is IIdentVehicle identVehicle && param[1] is PsdzDatabase.Characteristics xepCharacteristics)
                                if (param.Length > 1 && param[0] is IIdentVehicle identVehicle && param[1] is PsdzDatabase.Characteristics xepCharacteristics)
                                {
                                    identVehicle.TempTypeKeyLeadFromDb = xepCharacteristics.Name;
                                }
                                ComputeTypeKeyLead(param);
                                return ComputeBasicType(param);
                            case (VehicleCharacteristic)5L:
                                return ComputeMOTEinbaulage(param);
                            case (VehicleCharacteristic)2L:
                                return ComputeAELeistungsklasse(param);
                            case (VehicleCharacteristic)3L:
                                return ComputeAEUeberarbeitung(param);
                            case (VehicleCharacteristic)1L:
                                return ComputeAEKurzbezeichnung(param);
                        }
                    }
                }
                else
                {
                    switch (vehicleCharacteristic2)
                    {
                        case VehicleCharacteristic.Baureihenverbund:
                            return ComputeBaureihenverbund(param);
                        case VehicleCharacteristic.MOTBezeichnung:
                            return ComputeMOTBezeichnung(param);
                    }
                }

                goto IL_0702;
            }

            return false;
            IL_0702:
                return ComputeDefault(param);
        }

        protected abstract bool ComputeMotor(params object[] parameters);
        protected abstract bool ComputeKarosserie(params object[] parameters);
        protected abstract bool ComputeBaureihe(params object[] parameters);
        protected abstract bool ComputeLenkung(params object[] parameters);
        protected abstract bool ComputeHubraum(params object[] parameters);
        protected abstract bool ComputeGetriebe(params object[] parameters);
        protected abstract bool ComputeVerkaufsBezeichnung(params object[] parameters);
        protected abstract bool ComputeTyp(params object[] parameters);
        protected abstract bool ComputeAntrieb(params object[] parameters);
        protected abstract bool ComputeBrandName(params object[] parameters);
        protected abstract bool ComputeLeistungsklasse(params object[] parameters);
        protected abstract bool ComputeUeberarbeitung(params object[] parameters);
        protected abstract bool ComputeProdart(params object[] parameters);
        protected abstract bool ComputeEreihe(params object[] parameters);
        protected abstract bool ComputeLand(params object[] parameters);
        protected abstract bool ComputeTueren(params object[] parameters);
        protected abstract bool ComputeMotorarbeitsverfahren(params object[] parameters);
        protected abstract bool ComputeDrehmoment(params object[] parameters);
        protected abstract bool ComputeHybridkennzeichen(params object[] parameters);
        protected abstract bool ComputeProduktlinie(params object[] parameters);
        protected abstract bool ComputeKraftstoffart(params object[] parameters);
        protected abstract bool ComputeMOTKraftstoffart(params object[] parameters);
        protected abstract bool ComputeBasicType(params object[] parameters);
        protected abstract bool ComputeBaureihenverbund(params object[] parameters);
        protected abstract bool ComputeSicherheitsrelevant(params object[] parameters);
        protected abstract bool ComputeMOTEinbaulage(params object[] parameters);
        protected abstract bool ComputeMOTBezeichnung(params object[] parameters);
        protected abstract bool ComputeAELeistungsklasse(params object[] parameters);
        protected abstract bool ComputeAEUeberarbeitung(params object[] parameters);
        protected abstract bool ComputeAEKurzbezeichnung(params object[] parameters);
        protected abstract bool ComputeBaustandsJahr(params object[] parameters);
        protected abstract bool ComputeBaustandsMonat(params object[] parameters);
        protected abstract bool ComputeEMOTBaureihe(params object[] parameters);
        protected abstract bool ComputeEMOTArbeitsverfahren(params object[] parameters);
        protected abstract bool ComputeEMOTDrehmoment(params object[] parameters);
        protected abstract bool ComputeEMOTLeistungsklasse(params object[] parameters);
        protected abstract bool ComputeEMOTUeberarbeitung(params object[] parameters);
        protected abstract bool ComputeEMOTBezeichnung(params object[] parameters);
        protected abstract bool ComputeEMOTKraftstoffart(params object[] parameters);
        protected abstract bool ComputeEMOTEinbaulage(params object[] parameters);
        protected abstract bool ComputeCountryOfAssembly(params object[] parameters);
        protected abstract bool ComputeBaseVersion(params object[] parameters);
        protected abstract bool ComputeElektrischeReichweite(params object[] parameters);
        protected abstract bool ComputeAEBezeichnung(params object[] parameters);
        protected abstract bool ComputeEngineLabel2(params object[] parameters);
        protected abstract bool ComputeEngine2(params object[] parameters);
        protected abstract bool ComputeHeatMOTPlatzhalter1(params object[] parameters);
        protected abstract bool ComputeHeatMOTPlatzhalter2(params object[] parameters);
        protected abstract bool ComputeHeatMOTFortlaufendeNum(params object[] parameters);
        protected abstract bool ComputeHeatMOTLeistungsklasse(params object[] parameters);
        protected abstract bool ComputeHeatMOTLebenszyklus(params object[] parameters);
        protected abstract bool ComputeHeatMOTKraftstoffart(params object[] parameters);
        protected abstract bool ComputeKraftstoffartEinbaulage(params object[] parameters);
        protected abstract bool ComputeTypeKeyLead(params object[] parameters);
        protected abstract bool ComputeTypeKeyBasic(params object[] parameters);
        protected abstract bool ComputeESeriesLifeCycle(params object[] parameters);
        protected abstract bool ComputeLifeCycle(params object[] parameters);
        protected abstract bool ComputeSportausfuehrung(params object[] parameters);
        protected abstract bool ComputeDefault(params object[] parameters);
    }
}