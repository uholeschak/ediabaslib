using System;

namespace PsdzClient.Core
{
    public abstract class VehicleCharacteristicAbstract
    {
        protected bool ComputeCharacteristic(string vehicleCode, params object[] param)
        {
            if (Enum.TryParse<VehicleCharacteristic>(vehicleCode, out var result))
            {
                if (result <= VehicleCharacteristic.Tueren)
                {
                    switch (result)
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
                else if (result <= VehicleCharacteristic.Hybridkennzeichen)
                {
                    switch (result)
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
                else if (result <= VehicleCharacteristic.ESeriesLifeCycle)
                {
                    if (result <= VehicleCharacteristic.Drehmoment)
                    {
                        switch (result)
                        {
                            case VehicleCharacteristic.Motorarbeitsverfahren:
                                return ComputeMotorarbeitsverfahren(param);
                            case VehicleCharacteristic.Drehmoment:
                                return ComputeDrehmoment(param);
                        }
                    }
                    else
                    {
                        VehicleCharacteristic num = result - 99999999701L;
                        if ((ulong)num <= 7uL)
                        {
                            switch ((int)num)
                            {
                                case 0:
                                    return ComputeEngineLabel2(param);
                                case 1:
                                    return ComputeEngine2(param);
                                case 2:
                                    return ComputeHeatMOTPlatzhalter1(param);
                                case 3:
                                    return ComputeHeatMOTPlatzhalter2(param);
                                case 4:
                                    return ComputeHeatMOTFortlaufendeNum(param);
                                case 5:
                                    return ComputeHeatMOTLeistungsklasse(param);
                                case 6:
                                    return ComputeHeatMOTLebenszyklus(param);
                                case 7:
                                    return ComputeHeatMOTKraftstoffart(param);
                            }
                        }

                        VehicleCharacteristic num2 = result - 99999999846L;
                        if ((ulong)num2 <= 12uL)
                        {
                            switch ((int)num2)
                            {
                                case 5:
                                    return ComputeCountryOfAssembly(param);
                                case 4:
                                    return ComputeBaseVersion(param);
                                case 8:
                                    return ComputeElektrischeReichweite(param);
                                case 2:
                                    return ComputeAEBezeichnung(param);
                                case 12:
                                    return ComputeESeriesLifeCycle(param);
                                case 10:
                                    return ComputeLifeCycle(param);
                                case 0:
                                    return ComputeSportausfuehrung(param);
                            }
                        }
                    }
                }
                else if (result <= VehicleCharacteristic.MOTEinbaulage)
                {
                    VehicleCharacteristic num3 = result - 99999999866L;
                    if ((ulong)num3 <= 14uL)
                    {
                        switch ((int)num3)
                        {
                            case 14:
                                return ComputeEMOTBaureihe(param);
                            case 12:
                                return ComputeEMOTArbeitsverfahren(param);
                            case 10:
                                return ComputeEMOTDrehmoment(param);
                            case 8:
                                return ComputeEMOTLeistungsklasse(param);
                            case 6:
                                return ComputeEMOTUeberarbeitung(param);
                            case 4:
                                return ComputeEMOTBezeichnung(param);
                            case 2:
                                return ComputeEMOTKraftstoffart(param);
                            case 0:
                                return ComputeEMOTEinbaulage(param);
                            case 1:
                            case 3:
                            case 5:
                            case 7:
                            case 9:
                            case 11:
                            case 13:
                                goto IL_0566;
                        }
                    }

                    VehicleCharacteristic num4 = result - 99999999905L;
                    if ((ulong)num4 <= 5uL)
                    {
                        switch ((int)num4)
                        {
                            case 4:
                                return ComputeMOTKraftstoffart(param);
                            case 0:
                                //[-] if (param.Length > 1 && param[0] is IIdentVehicle identVehicle && param[1] is IXepCharacteristics xepCharacteristics)
                                //[+] if (param.Length > 1 && param[0] is IIdentVehicle identVehicle && param[1] is PsdzDatabase.Characteristics xepCharacteristics)
                                if (param.Length > 1 && param[0] is IIdentVehicle identVehicle && param[1] is PsdzDatabase.Characteristics xepCharacteristics)
                                {
                                    identVehicle.TempTypeKeyLeadFromDb = xepCharacteristics.Name;
                                }

                                ComputeTypeKeyLead(param);
                                return ComputeBasicType(param);
                            case 5:
                                return ComputeMOTEinbaulage(param);
                            case 2:
                                return ComputeAELeistungsklasse(param);
                            case 3:
                                return ComputeAEUeberarbeitung(param);
                            case 1:
                                return ComputeAEKurzbezeichnung(param);
                        }
                    }
                }
                else
                {
                    switch (result)
                    {
                        case VehicleCharacteristic.Baureihenverbund:
                            return ComputeBaureihenverbund(param);
                        case VehicleCharacteristic.MOTBezeichnung:
                            return ComputeMOTBezeichnung(param);
                    }
                }

                goto IL_0566;
            }

            return false;
            IL_0566:
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