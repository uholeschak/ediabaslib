using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using log4net.Core;
using PsdzClient.Core;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PsdzClient.Core
{
    public class ReactorEngine
    {
        [PreserveSource(Hint = "Namespace modified")]
        private MultisourceLogic multisourceLogic;
        private readonly FallbackMechanisms fallback;
        private readonly IReactorVehicle vehicle;
        private static ILogger log;
        public DataHolder dataHolder;
        private bool observerEnabled;
        private static object obj = new object ();
        [PreserveSource(Hint = "Namespace modified")]
        public ReactorEngine(IReactorVehicle reactorVehicle, ILogger logger, DataHolder dataHolder)
        {
            log = logger;
            this.dataHolder = dataHolder;
            multisourceLogic = new MultisourceLogic(dataHolder, log, new MultisourceProperties(), new ValueValidator());
            fallback = new FallbackMechanisms(dataHolder);
            vehicle = reactorVehicle;
        }

        public void EnableCore()
        {
            multisourceLogic.Enabled = true;
            EnableObserver();
        }

        public void DisableCore()
        {
            multisourceLogic.Enabled = false;
        }

        public void EnableObserver()
        {
            if (!observerEnabled)
            {
                AttachPropertyChanged();
                observerEnabled = true;
            }
        }

        public void DisableObserver()
        {
            DetachPropertyChanged();
            observerEnabled = false;
        }

        public void SetFA(IReactorFa value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.FA = multisourceLogic.SetProperty(value, source, "FA");
            });
        }

        public void SetModelltag(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Modelltag = multisourceLogic.SetProperty(value, source, "Modelltag");
            }, delegate
            {
                fallback.ProductionDate(delegate (DateTime d)
                {
                    AssignProductionDate(d, DataSource.Fallback);
                }, "ProductionDate", "Modelljahr", "Modellmonat", "Modelltag");
            });
        }

        public void SetModellmonat(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                AssignModellMonat(value, source);
            }, delegate
            {
                fallback.ProductionDate(delegate (DateTime d)
                {
                    AssignProductionDate(d, DataSource.Fallback);
                }, "ProductionDate", "Modelljahr", "Modellmonat", "Modelltag");
            });
        }

        public void SetModelljahr(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                AssignModellJahr(value, source);
            }, delegate
            {
                fallback.ProductionDate(delegate (DateTime d)
                {
                    AssignProductionDate(d, DataSource.Fallback);
                }, "ProductionDate", "Modelljahr", "Modellmonat", "Modelltag");
            });
        }

        public void SetVin17(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.VIN17 = multisourceLogic.SetProperty(value, source, "VIN17");
            }, delegate
            {
                fallback.HandleVin17Fallbacks(delegate (string s)
                {
                    AssignBasicType(s, DataSource.Fallback);
                }, delegate (string s)
                {
                    AssignVinRangeType(s, DataSource.Fallback);
                }, "VIN17", "BasicType", "VINRangeType");
            });
        }

        public void SetVin17WithoutFallbacks(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.VIN17 = multisourceLogic.SetProperty(value, source, "VIN17");
            });
        }

        public void SetMarke(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Marke = multisourceLogic.SetProperty(value, source, "Marke");
            });
        }

        public void SetProductionDate(DateTime value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                AssignProductionDate(value, source);
            });
        }

        public void SetAntrieb(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Antrieb = multisourceLogic.SetProperty(value, source, "Antrieb");
            });
        }

        public void SetBaureihe(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Baureihe = multisourceLogic.SetProperty(value, source, "Baureihe");
            });
        }

        public void SetBaureihenverbund(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Baureihenverbund = multisourceLogic.SetProperty(value, source, "Baureihenverbund");
            });
        }

        public void SetElektrischeReichweite(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.ElektrischeReichweite = multisourceLogic.SetProperty(value, source, "ElektrischeReichweite");
            });
        }

        public void SetLenkung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Lenkung = multisourceLogic.SetProperty(value, source, "Lenkung");
            });
        }

        public void SetAEBezeichnung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.AEBezeichnung = multisourceLogic.SetProperty(value, source, "AEBezeichnung");
            });
        }

        public void SetAEKurzbezeichnung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.AEKurzbezeichnung = multisourceLogic.SetProperty(value, source, "AEKurzbezeichnung");
            });
        }

        public void SetAELeistungsklasse(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.AELeistungsklasse = multisourceLogic.SetProperty(value, source, "AELeistungsklasse");
            });
        }

        public void SetKraftstoffartEinbaulage(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.KraftstoffartEinbaulage = multisourceLogic.SetProperty(value, source, "KraftstoffartEinbaulage");
            });
        }

        public void SetAEUeberarbeitung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.AEUeberarbeitung = multisourceLogic.SetProperty(value, source, "AEUeberarbeitung");
            });
        }

        public void SetBaustandsJahr(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.BaustandsJahr = multisourceLogic.SetProperty(value, source, "BaustandsJahr");
            });
        }

        public void SetBaustandsMonat(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.BaustandsMonat = multisourceLogic.SetProperty(value, source, "BaustandsMonat");
            });
        }

        public void SetBaustand(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Baustand = multisourceLogic.SetProperty(value, source, "Baustand");
            });
        }

        public void SetEreihe(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Ereihe = multisourceLogic.SetProperty(value, source, "Ereihe");
            });
        }

        public void SetHybridkennzeichen(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Hybridkennzeichen = multisourceLogic.SetProperty(value, source, "Hybridkennzeichen");
            });
        }

        public void SetKarosserie(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Karosserie = multisourceLogic.SetProperty(value, source, "Karosserie");
            });
        }

        public void SetLand(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Land = multisourceLogic.SetProperty(value, source, "Land");
            });
        }

        public void SetBasicType(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                AssignBasicType(value, source);
            });
        }

        public void SetBaseVersion(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.BaseVersion = multisourceLogic.SetProperty(value, source, "BaseVersion");
            });
        }

        public void SetBrandName(BrandName? value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.BrandName = multisourceLogic.SetProperty(value, source, "BrandName");
            });
        }

        public void SetCountryOfAssembly(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.CountryOfAssembly = multisourceLogic.SetProperty(value, source, "CountryOfAssembly");
            });
        }

        public void SetProdart(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Prodart = multisourceLogic.SetProperty(value, source, "Prodart");
            });
        }

        public void SetProduktlinie(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Produktlinie = multisourceLogic.SetProperty(value, source, "Produktlinie");
            });
        }

        public void SetSicherheitsrelevant(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Sicherheitsrelevant = multisourceLogic.SetProperty(value, source, "Sicherheitsrelevant");
            });
        }

        public void SetTueren(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Tueren = multisourceLogic.SetProperty(value, source, "Tueren");
            });
        }

        public void SetTyp(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Typ = multisourceLogic.SetProperty(value, source, "Typ");
            });
        }

        public void SetVerkaufsBezeichnung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.VerkaufsBezeichnung = multisourceLogic.SetProperty(value, source, "VerkaufsBezeichnung");
            });
        }

        public void SetMotor(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Motor = multisourceLogic.SetProperty(value, source, "Motor");
            });
        }

        public void SetMotorarbeitsverfahren(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Motorarbeitsverfahren = multisourceLogic.SetProperty(value, source, "Motorarbeitsverfahren");
            });
        }

        public void SetMOTBezeichnung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.MOTBezeichnung = multisourceLogic.SetProperty(value, source, "MOTBezeichnung");
            });
        }

        public void SetMOTEinbaulage(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.MOTEinbaulage = multisourceLogic.SetProperty(value, source, "MOTEinbaulage");
            });
        }

        public void SetHubraum(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Hubraum = multisourceLogic.SetProperty(value, source, "Hubraum");
            });
        }

        public void SetKraftstoffart(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Kraftstoffart = multisourceLogic.SetProperty(value, source, "Kraftstoffart");
            });
        }

        public void SetLeistungsklasse(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Leistungsklasse = multisourceLogic.SetProperty(value, source, "Leistungsklasse");
            });
        }

        public void SetUeberarbeitung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Ueberarbeitung = multisourceLogic.SetProperty(value, source, "Ueberarbeitung");
            });
        }

        public void SetDrehmoment(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Drehmoment = multisourceLogic.SetProperty(value, source, "Drehmoment");
            });
        }

        public void SetEMOTBaureihe(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTBaureihe = multisourceLogic.SetProperty(value, source, "EMOTBaureihe");
            });
        }

        public void SetEMOTBezeichnung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTBezeichnung = multisourceLogic.SetProperty(value, source, "EMOTBezeichnung");
            });
        }

        public void SetEMOTDrehmoment(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTDrehmoment = multisourceLogic.SetProperty(value, source, "EMOTDrehmoment");
            });
        }

        public void SetEMOTEinbaulage(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTEinbaulage = multisourceLogic.SetProperty(value, source, "EMOTEinbaulage");
            });
        }

        public void SetEMOTKraftstoffart(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTKraftstoffart = multisourceLogic.SetProperty(value, source, "EMOTKraftstoffart");
            });
        }

        public void SetEMOTLeistungsklasse(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTLeistungsklasse = multisourceLogic.SetProperty(value, source, "EMOTLeistungsklasse");
            });
        }

        public void SetEMOTUeberarbeitung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTUeberarbeitung = multisourceLogic.SetProperty(value, source, "EMOTUeberarbeitung");
            });
        }

        public void SetEMOTArbeitsverfahren(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.EMotor.EMOTArbeitsverfahren = multisourceLogic.SetProperty(value, source, "EMOTArbeitsverfahren");
            });
        }

        public void SetILevelWerk(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.ILevelWerk = multisourceLogic.SetProperty(value, source, "ILevelWerk");
            }, delegate
            {
                fallback.HandleILevelWerkFallbacks(delegate (string d)
                {
                    AssignModellJahr(d, DataSource.Fallback);
                }, delegate (string d)
                {
                    AssignModellMonat(d, DataSource.Fallback);
                }, "ILevelWerk", "Modellmonat", "Modelljahr");
            });
        }

        public void SetILevel(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.ILevel = multisourceLogic.SetProperty(value, source, "ILevel");
            });
        }

        public void SetVINRangeType(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                AssignVinRangeType(value, source);
            });
        }

        public void SetGetriebe(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Getriebe = multisourceLogic.SetProperty(value, source, "Getriebe");
            });
        }

        public void SetECTypeApproval(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.ECTypeApproval = multisourceLogic.SetProperty(value, source, "ECTypeApproval");
            });
        }

        public void SetSerialBodyShell(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.SerialBodyShell = multisourceLogic.SetProperty(value, source, "SerialBodyShell");
            });
        }

        public void SetSerialEngine(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.SerialEngine = multisourceLogic.SetProperty(value, source, "SerialEngine");
            });
        }

        public void SetSerialGearBox(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.SerialGearBox = multisourceLogic.SetProperty(value, source, "SerialGearBox");
            });
        }

        public void SetFirstRegistrationDate(DateTime value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.FirstRegistration = multisourceLogic.SetProperty(value, source, "FirstRegistration");
            });
        }

        public void SetProgramVersion(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.ProgmanVersion = multisourceLogic.SetProperty(value, source, "ProgmanVersion");
            });
        }

        public void SetTypeKey(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.TypeKey = multisourceLogic.SetProperty(value, source, "TypeKey");
            });
        }

        public void SetTypeKeyLead(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.TypeKeyLead = multisourceLogic.SetProperty(value, source, "TypeKeyLead");
            });
        }

        public void SetTypeKeyBasic(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.TypeKeyBasic = multisourceLogic.SetProperty(value, source, "TypeKeyBasic");
            });
        }

        public void SetESeriesLifeCycle(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.ESeriesLifeCycle = multisourceLogic.SetProperty(value, source, "ESeriesLifeCycle");
            });
        }

        public void SetLifeCycle(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.LifeCycle = multisourceLogic.SetProperty(value, source, "LifeCycle");
            });
        }

        public void SetSportausfuehrung(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                vehicle.Sportausfuehrung = multisourceLogic.SetProperty(value, source, "Sportausfuehrung");
            });
        }

        public void AddInfoToDataholderAboutHeatMotors(List<HeatMotor> heatMotors, DataSource source)
        {
            multisourceLogic.SetProperty(string.Join(",", heatMotors.Select((HeatMotor h) => h.HeatMOTBezeichnung)), source, "HeatMotors");
        }

        public void DumpReactorState()
        {
            string text = dataHolder.ToJson(log);
            log.Info("Reactor.DumpReactorState()", "Vehicle state: " + Environment.NewLine + " " + text);
        }

        public void SetF2Date(string value, DataSource source)
        {
            AssignPropertyAndExecuteFallback(delegate
            {
                AssignF2Date(value, source);
            });
        }

        private void AssignPropertyAndExecuteFallback(Action setBody, Action fallback = null)
        {
            lock (obj)
            {
                DetachPropertyChanged();
                setBody();
                if (multisourceLogic.Enabled)
                {
                    fallback?.Invoke();
                }

                if (observerEnabled)
                {
                    AttachPropertyChanged();
                }
            }
        }

        private void AssignProductionDate(DateTime value, DataSource source)
        {
            vehicle.ProductionDate = multisourceLogic.SetProperty(value, source, "ProductionDate");
        }

        private void AssignBasicType(string value, DataSource source)
        {
            vehicle.BasicType = multisourceLogic.SetProperty(value, source, "BasicType");
        }

        private void AssignVinRangeType(string value, DataSource source)
        {
            vehicle.VINRangeType = multisourceLogic.SetProperty(value, source, "VINRangeType");
        }

        private void AssignModellMonat(string value, DataSource source)
        {
            vehicle.Modellmonat = multisourceLogic.SetProperty(value, source, "Modellmonat");
        }

        private void AssignModellJahr(string value, DataSource source)
        {
            vehicle.Modelljahr = multisourceLogic.SetProperty(value, source, "Modelljahr");
        }

        private void AssignF2Date(string value, DataSource source)
        {
            if (!string.IsNullOrEmpty(value))
            {
                vehicle.F2Date = multisourceLogic.SetProperty(value, source, "F2Date");
            }
            else
            {
                vehicle.F2Date = "-";
            }
        }

        private void Vehicle_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (Environment.StackTrace.IndexOf("BMW.ISPI.IstaOperation.Impl.ObserverBase", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    log.Error("Reactor.Vehicle_PropertyChanged", "PropertyChanged is observed in UI thread?");
                }

                if (Environment.StackTrace.IndexOf("ReadVehicleFromXml", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    log.Error("Reactor.Vehicle_PropertyChanged", "PropertyChanged is observed when deserializing?");
                }
            }
            catch (Exception ex)
            {
                log.Error("Reactor.Vehicle_PropertyChanged", "Exception thrown while reaching stack trace: {0}", ex);
            }

            bool flag = true;
            switch (e.PropertyName)
            {
                case "Modelljahr":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Modelljahr = v;
                    }, vehicle.Modelljahr, e.PropertyName);
                    break;
                case "Modellmonat":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Modellmonat = v;
                    }, vehicle.Modellmonat, e.PropertyName);
                    break;
                case "Modelltag":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Modelltag = v;
                    }, vehicle.Modelltag, e.PropertyName);
                    break;
                case "ProductionDate":
                    AssignLegacy(delegate (DateTime v)
                    {
                        vehicle.ProductionDate = v;
                    }, vehicle.ProductionDate, e.PropertyName);
                    break;
                case "VIN17":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.VIN17 = v;
                    }, vehicle.VIN17, e.PropertyName);
                    break;
                case "FA":
                    AssignLegacy(delegate (IReactorFa v)
                    {
                        vehicle.FA = v;
                    }, vehicle.FA, e.PropertyName);
                    break;
                case "Antrieb":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Antrieb = v;
                    }, vehicle.Antrieb, e.PropertyName);
                    break;
                case "Baureihe":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Baureihe = v;
                    }, vehicle.Baureihe, e.PropertyName);
                    break;
                case "Baureihenverbund":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Baureihenverbund = v;
                    }, vehicle.Baureihenverbund, e.PropertyName);
                    break;
                case "ElektrischeReichweite":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.ElektrischeReichweite = v;
                    }, vehicle.ElektrischeReichweite, e.PropertyName);
                    break;
                case "Lenkung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Lenkung = v;
                    }, vehicle.Lenkung, e.PropertyName);
                    break;
                case "AEBezeichnung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.AEBezeichnung = v;
                    }, vehicle.AEBezeichnung, e.PropertyName);
                    break;
                case "AEKurzbezeichnung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.AEKurzbezeichnung = v;
                    }, vehicle.AEKurzbezeichnung, e.PropertyName);
                    break;
                case "AELeistungsklasse":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.AELeistungsklasse = v;
                    }, vehicle.AELeistungsklasse, e.PropertyName);
                    break;
                case "AEUeberarbeitung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.AEUeberarbeitung = v;
                    }, vehicle.AEUeberarbeitung, e.PropertyName);
                    break;
                case "BaustandsJahr":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.BaustandsJahr = v;
                    }, vehicle.BaustandsJahr, e.PropertyName);
                    break;
                case "BaustandsMonat":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.BaustandsMonat = v;
                    }, vehicle.BaustandsMonat, e.PropertyName);
                    break;
                case "Ereihe":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Ereihe = v;
                    }, vehicle.Ereihe, e.PropertyName);
                    break;
                case "Hybridkennzeichen":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Hybridkennzeichen = v;
                    }, vehicle.Hybridkennzeichen, e.PropertyName);
                    break;
                case "Karosserie":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Karosserie = v;
                    }, vehicle.Karosserie, e.PropertyName);
                    break;
                case "Land":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Land = v;
                    }, vehicle.Land, e.PropertyName);
                    break;
                case "BasicType":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.BasicType = v;
                    }, vehicle.BasicType, e.PropertyName);
                    break;
                case "BaseVersion":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.BaseVersion = v;
                    }, vehicle.BaseVersion, e.PropertyName);
                    break;
                case "CountryOfAssembly":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.CountryOfAssembly = v;
                    }, vehicle.CountryOfAssembly, e.PropertyName);
                    break;
                case "Prodart":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Prodart = v;
                    }, vehicle.Prodart, e.PropertyName);
                    break;
                case "Produktlinie":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Produktlinie = v;
                    }, vehicle.Produktlinie, e.PropertyName);
                    break;
                case "Tueren":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Tueren = v;
                    }, vehicle.Tueren, e.PropertyName);
                    break;
                case "Typ":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Typ = v;
                    }, vehicle.Typ, e.PropertyName);
                    break;
                case "VerkaufsBezeichnung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.VerkaufsBezeichnung = v;
                    }, vehicle.VerkaufsBezeichnung, e.PropertyName);
                    break;
                case "Motor":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Motor = v;
                    }, vehicle.Motor, e.PropertyName);
                    break;
                case "Motorarbeitsverfahren":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Motorarbeitsverfahren = v;
                    }, vehicle.Motorarbeitsverfahren, e.PropertyName);
                    break;
                case "MOTBezeichnung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.MOTBezeichnung = v;
                    }, vehicle.MOTBezeichnung, e.PropertyName);
                    break;
                case "MOTEinbaulage":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.MOTEinbaulage = v;
                    }, vehicle.MOTEinbaulage, e.PropertyName);
                    break;
                case "Hubraum":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Hubraum = v;
                    }, vehicle.Hubraum, e.PropertyName);
                    break;
                case "Kraftstoffart":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Kraftstoffart = v;
                    }, vehicle.Kraftstoffart, e.PropertyName);
                    break;
                case "Leistungsklasse":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Leistungsklasse = v;
                    }, vehicle.Leistungsklasse, e.PropertyName);
                    break;
                case "Ueberarbeitung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Ueberarbeitung = v;
                    }, vehicle.Ueberarbeitung, e.PropertyName);
                    break;
                case "Drehmoment":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Drehmoment = v;
                    }, vehicle.Drehmoment, e.PropertyName);
                    break;
                case "EMOTBaureihe":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTBaureihe = v;
                    }, vehicle.EMotor.EMOTBaureihe, e.PropertyName);
                    break;
                case "EMOTBezeichnung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTBezeichnung = v;
                    }, vehicle.EMotor.EMOTBezeichnung, e.PropertyName);
                    break;
                case "EMOTDrehmoment":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTDrehmoment = v;
                    }, vehicle.EMotor.EMOTDrehmoment, e.PropertyName);
                    break;
                case "EMOTEinbaulage":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTEinbaulage = v;
                    }, vehicle.EMotor.EMOTEinbaulage, e.PropertyName);
                    break;
                case "EMOTKraftstoffart":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTKraftstoffart = v;
                    }, vehicle.EMotor.EMOTKraftstoffart, e.PropertyName);
                    break;
                case "EMOTLeistungsklasse":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTLeistungsklasse = v;
                    }, vehicle.EMotor.EMOTLeistungsklasse, e.PropertyName);
                    break;
                case "EMOTUeberarbeitung":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTUeberarbeitung = v;
                    }, vehicle.EMotor.EMOTUeberarbeitung, e.PropertyName);
                    break;
                case "EMOTArbeitsverfahren":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.EMotor.EMOTArbeitsverfahren = v;
                    }, vehicle.EMotor.EMOTArbeitsverfahren, e.PropertyName);
                    break;
                case "ILevelWerk":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.ILevelWerk = v;
                    }, vehicle.ILevelWerk, e.PropertyName);
                    break;
                case "ILevel":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.ILevel = v;
                    }, vehicle.ILevel, e.PropertyName);
                    break;
                case "Marke":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Marke = v;
                    }, vehicle.Marke, e.PropertyName);
                    break;
                case "VINRangeType":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Marke = v;
                    }, vehicle.Marke, e.PropertyName);
                    break;
                case "Getriebe":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Getriebe = v;
                    }, vehicle.Getriebe, e.PropertyName);
                    break;
                case "BrandName":
                    AssignLegacy(delegate (BrandName? v)
                    {
                        vehicle.BrandName = v;
                    }, vehicle.BrandName, e.PropertyName);
                    break;
                case "Sicherheitsrelevant":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Sicherheitsrelevant = v;
                    }, vehicle.Sicherheitsrelevant, e.PropertyName);
                    break;
                case "KraftstoffartEinbaulage":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.KraftstoffartEinbaulage = v;
                    }, vehicle.KraftstoffartEinbaulage, e.PropertyName);
                    break;
                case "Baustand":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.Baustand = v;
                    }, vehicle.Baustand, e.PropertyName);
                    break;
                case "SerialBodyShell":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.SerialBodyShell = v;
                    }, vehicle.SerialBodyShell, e.PropertyName);
                    break;
                case "SerialEngine":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.SerialEngine = v;
                    }, vehicle.SerialEngine, e.PropertyName);
                    break;
                case "SerialGearBox":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.SerialGearBox = v;
                    }, vehicle.SerialGearBox, e.PropertyName);
                    break;
                case "FirstRegistration":
                    AssignLegacy(delegate (DateTime? v)
                    {
                        vehicle.FirstRegistration = v;
                    }, vehicle.FirstRegistration, e.PropertyName);
                    break;
                case "ProgmanVersion":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.ProgmanVersion = v;
                    }, vehicle.ProgmanVersion, e.PropertyName);
                    break;
                case "ECTypeApproval":
                    AssignLegacy(delegate (string v)
                    {
                        vehicle.ECTypeApproval = v;
                    }, vehicle.ECTypeApproval, e.PropertyName);
                    break;
                default:
                    flag = false;
                    break;
            }

            if (flag)
            {
                log.Info("Reactor.Vehicle_PropertyChanged()", Environment.StackTrace);
            }
        }

        private void AssignLegacy<T>(Action<T> doAssignment, T value, string propName)
        {
            T val = multisourceLogic.SetLegacyProperty(value, propName);
            if (multisourceLogic.Enabled)
            {
                doAssignment(val);
            }
        }

        private void DetachPropertyChanged()
        {
            vehicle.PropertyChanged -= Vehicle_PropertyChanged;
            if (vehicle.FA != null)
            {
                vehicle.FA.PropertyChanged -= Vehicle_PropertyChanged;
            }

            if (vehicle.EMotor != null)
            {
                vehicle.EMotor.PropertyChanged -= Vehicle_PropertyChanged;
            }
        }

        private void AttachPropertyChanged()
        {
            vehicle.PropertyChanged += Vehicle_PropertyChanged;
            if (vehicle.FA != null)
            {
                vehicle.FA.PropertyChanged += Vehicle_PropertyChanged;
            }

            if (vehicle.EMotor != null)
            {
                vehicle.EMotor.PropertyChanged += Vehicle_PropertyChanged;
            }
        }
    }
}