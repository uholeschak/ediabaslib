using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public abstract class VehicleCharacteristicAbstract
	{
        public enum VehicleCharacteristic : long
        {
            Motor = 40142338L,
            Karosserie = 40146178L,
            Baureihe = 40140418L,
            Lenkung = 40141954L,
            Hubraum = 40142722L,
            Getriebe = 40141186L,
            VerkaufsBezeichnung = 40143490L,
            Typ = 40139650L,
            Antrieb = 40143874L,
            BrandName = 40144642L,
            Leistungsklasse = 40141570L,
            Ueberarbeitung = 40145794L,
            Prodart = 40140034L,
            Ereihe = 40140802L,
            Land = 40146562L,
            Tueren = 40144258L,
            Abgas = 68771233282L,
            Motorarbeitsverfahren = 68771234050L,
            Drehmoment = 68771234434L,
            Hybridkennzeichen = 68771233666L,
            Produktlinie = 40039947266L,
            Kraftstoffart = 40143106L,
            MOTKraftstoffart = 99999999909L,
            BasicType = 99999999905L,
            Baureihenverbund = 99999999950L,
            Sicherheitsrelevant = 40145410L,
            MOTEinbaulage = 99999999910L,
            MOTBezeichnung = 99999999918L,
            AELeistungsklasse = 99999999907L,
            AEUeberarbeitung,
            AEKurzbezeichnung = 99999999906L,
            BaustandsJahr = -100L,
            BaustandsMonat = -101L,
            EMOTBaureihe = 99999999880L,
            EMOTArbeitsverfahren = 99999999878L,
            EMOTDrehmoment = 99999999876L,
            EMOTLeistungsklasse = 99999999874L,
            EMOTUeberarbeitung = 99999999872L,
            EMOTBezeichnung = 99999999870L,
            EMOTKraftstoffart = 99999999868L,
            EMOTEinbaulage = 99999999866L,
            CountryOfAssembly = 99999999851L,
            BaseVersion = 99999999850L,
            ElektrischeReichweite = 99999999854L,
            AEBezeichnung = 99999999848L,
            EngineLabel2 = 99999999701L,
            Engine2,
            HeatMOTPlatzhalter1,
            HeatMOTPlatzhalter2,
            HeatMOTFortlaufendeNum,
            HeatMOTLeistungsklasse,
            HeatMOTLebenszyklus,
            HeatMOTKraftstoffart
        }

        protected bool ComputeCharacteristic(string vehicleCode, params object[] param)
		{
			VehicleCharacteristic vehicleCharacteristic;
			if (Enum.TryParse<VehicleCharacteristic>(vehicleCode, out vehicleCharacteristic))
			{
				if (vehicleCharacteristic <= VehicleCharacteristic.Tueren)
				{
					if (vehicleCharacteristic <= VehicleCharacteristic.Getriebe)
					{
						if (vehicleCharacteristic <= VehicleCharacteristic.Typ)
						{
							if (vehicleCharacteristic == VehicleCharacteristic.BaustandsMonat)
							{
								return this.ComputeBaustandsMonat(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.BaustandsJahr)
							{
								return this.ComputeBaustandsJahr(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Typ)
							{
								return this.ComputeTyp(param);
							}
						}
						else if (vehicleCharacteristic <= VehicleCharacteristic.Baureihe)
						{
							if (vehicleCharacteristic == VehicleCharacteristic.Prodart)
							{
								return this.ComputeProdart(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Baureihe)
							{
								return this.ComputeBaureihe(param);
							}
						}
						else
						{
							if (vehicleCharacteristic == VehicleCharacteristic.Ereihe)
							{
								return this.ComputeEreihe(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Getriebe)
							{
								return this.ComputeGetriebe(param);
							}
						}
					}
					else if (vehicleCharacteristic <= VehicleCharacteristic.Hubraum)
					{
						if (vehicleCharacteristic <= VehicleCharacteristic.Lenkung)
						{
							if (vehicleCharacteristic == VehicleCharacteristic.Leistungsklasse)
							{
								return this.ComputeLeistungsklasse(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Lenkung)
							{
								return this.ComputeLenkung(param);
							}
						}
						else
						{
							if (vehicleCharacteristic == VehicleCharacteristic.Motor)
							{
								return this.ComputeMotor(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Hubraum)
							{
								return this.ComputeHubraum(param);
							}
						}
					}
					else if (vehicleCharacteristic <= VehicleCharacteristic.VerkaufsBezeichnung)
					{
						if (vehicleCharacteristic == VehicleCharacteristic.Kraftstoffart)
						{
							return this.ComputeKraftstoffart(param);
						}
						if (vehicleCharacteristic == VehicleCharacteristic.VerkaufsBezeichnung)
						{
							return this.ComputeVerkaufsBezeichnung(param);
						}
					}
					else
					{
						if (vehicleCharacteristic == VehicleCharacteristic.Antrieb)
						{
							return this.ComputeAntrieb(param);
						}
						if (vehicleCharacteristic == VehicleCharacteristic.Tueren)
						{
							return this.ComputeTueren(param);
						}
					}
				}
				else if (vehicleCharacteristic <= VehicleCharacteristic.Hybridkennzeichen)
				{
					if (vehicleCharacteristic <= VehicleCharacteristic.Karosserie)
					{
						if (vehicleCharacteristic <= VehicleCharacteristic.Sicherheitsrelevant)
						{
							if (vehicleCharacteristic == VehicleCharacteristic.BrandName)
							{
								return this.ComputeBrandName(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Sicherheitsrelevant)
							{
								return this.ComputeSicherheitsrelevant(param);
							}
						}
						else
						{
							if (vehicleCharacteristic == VehicleCharacteristic.Ueberarbeitung)
							{
								return this.ComputeUeberarbeitung(param);
							}
							if (vehicleCharacteristic == VehicleCharacteristic.Karosserie)
							{
								return this.ComputeKarosserie(param);
							}
						}
					}
					else if (vehicleCharacteristic <= VehicleCharacteristic.Produktlinie)
					{
						if (vehicleCharacteristic == VehicleCharacteristic.Land)
						{
							return this.ComputeLand(param);
						}
						if (vehicleCharacteristic == VehicleCharacteristic.Produktlinie)
						{
							return this.ComputeProduktlinie(param);
						}
					}
					else
					{
						if (vehicleCharacteristic == VehicleCharacteristic.Abgas)
						{
							return this.ComputeAbgas(param);
						}
						if (vehicleCharacteristic == VehicleCharacteristic.Hybridkennzeichen)
						{
							return this.ComputeHybridkennzeichen(param);
						}
					}
				}
				else if (vehicleCharacteristic <= VehicleCharacteristic.ElektrischeReichweite)
				{
					if (vehicleCharacteristic <= VehicleCharacteristic.Drehmoment)
					{
						if (vehicleCharacteristic == VehicleCharacteristic.Motorarbeitsverfahren)
						{
							return this.ComputeMotorarbeitsverfahren(param);
						}
						if (vehicleCharacteristic == VehicleCharacteristic.Drehmoment)
						{
							return this.ComputeDrehmoment(param);
						}
					}
					else
					{
						long num = vehicleCharacteristic - VehicleCharacteristic.EngineLabel2;
						if (num <= 7L)
						{
							switch ((uint)num)
							{
								case 0U:
									return this.ComputeEngineLabel2(param);
								case 1U:
									return this.ComputeEngine2(param);
								case 2U:
									return this.ComputeHeatMOTPlatzhalter1(param);
								case 3U:
									return this.ComputeHeatMOTPlatzhalter2(param);
								case 4U:
									return this.ComputeHeatMOTFortlaufendeNum(param);
								case 5U:
									return this.ComputeHeatMOTLeistungsklasse(param);
								case 6U:
									return this.ComputeHeatMOTLebenszyklus(param);
								case 7U:
									return this.ComputeHeatMOTKraftstoffart(param);
							}
						}
						long num2 = vehicleCharacteristic - VehicleCharacteristic.AEBezeichnung;
						if (num2 <= 6L)
						{
							switch ((uint)num2)
							{
								case 0U:
									return this.ComputeAEBezeichnung(param);
								case 2U:
									return this.ComputeBaseVersion(param);
								case 3U:
									return this.ComputeCountryOfAssembly(param);
								case 6U:
									return this.ComputeElektrischeReichweite(param);
							}
						}
					}
				}
				else if (vehicleCharacteristic <= VehicleCharacteristic.MOTEinbaulage)
				{
					long num3 = vehicleCharacteristic - VehicleCharacteristic.EMOTEinbaulage;
					if (num3 <= 14L)
					{
						switch ((uint)num3)
						{
							case 0U:
								return this.ComputeEMOTEinbaulage(param);
							case 1U:
							case 3U:
							case 5U:
							case 7U:
							case 9U:
							case 11U:
							case 13U:
								goto IL_4FC;
							case 2U:
								return this.ComputeEMOTKraftstoffart(param);
							case 4U:
								return this.ComputeEMOTBezeichnung(param);
							case 6U:
								return this.ComputeEMOTUeberarbeitung(param);
							case 8U:
								return this.ComputeEMOTLeistungsklasse(param);
							case 10U:
								return this.ComputeEMOTDrehmoment(param);
							case 12U:
								return this.ComputeEMOTArbeitsverfahren(param);
							case 14U:
								return this.ComputeEMOTBaureihe(param);
						}
					}
					long num4 = vehicleCharacteristic - VehicleCharacteristic.BasicType;
					if (num4 <= 5L)
					{
						switch ((uint)num4)
						{
							case 0U:
								return this.ComputeBasicType(param);
							case 1U:
								return this.ComputeAEKurzbezeichnung(param);
							case 2U:
								return this.ComputeAELeistungsklasse(param);
							case 3U:
								return this.ComputeAEUeberarbeitung(param);
							case 4U:
								return this.ComputeMOTKraftstoffart(param);
							case 5U:
								return this.ComputeMOTEinbaulage(param);
						}
					}
				}
				else
				{
					if (vehicleCharacteristic == VehicleCharacteristic.MOTBezeichnung)
					{
						return this.ComputeMOTBezeichnung(param);
					}
					if (vehicleCharacteristic == VehicleCharacteristic.Baureihenverbund)
					{
						return this.ComputeBaureihenverbund(param);
					}
				}
				IL_4FC:
				return this.ComputeDefault(param);
			}
			return false;
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

		protected abstract bool ComputeAbgas(params object[] parameters);

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

		protected abstract bool ComputeDefault(params object[] parameters);
	}
}
