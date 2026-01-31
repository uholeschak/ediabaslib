using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Programming;
using PsdzClient.Utility;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace BMW.Rheingold.Psdz
{
    internal class PsdzObjectBuilder : IPsdzObjectBuilder
    {
        private readonly IObjectBuilderService objectBuilderService;
        [PreserveSource(Hint = "Init in constructor", SuppressWarning = true)]
        private readonly BaureiheReader baureiheReader;
        private readonly SwtActionTypeEnumMapper swtActionTypeEnumMapper = new SwtActionTypeEnumMapper();
        private readonly FscCertificateStateEnumMapper fscCertificateStateEnumMapper = new FscCertificateStateEnumMapper();
        [PreserveSource(Hint = "Namespace modified", SuppressWarning = true)]
        private readonly PsdzClient.Programming.FscStateEnumMapper fscStateEnumMapper = new PsdzClient.Programming.FscStateEnumMapper();
        [PreserveSource(Hint = "Namespace modified", SuppressWarning = true)]
        private readonly PsdzClient.Programming.SwtTypeEnumMapper swtTypeEnumMapper = new PsdzClient.Programming.SwtTypeEnumMapper();
        private readonly RootCertificateStateEnumMapper rootCertificateStateEnumMapper = new RootCertificateStateEnumMapper();
        [PreserveSource(Hint = "Namespace modified", SuppressWarning = true)]
        private readonly PsdzClient.Programming.SoftwareSigStateEnumMapper softwareSigStateEnumMapper = new PsdzClient.Programming.SoftwareSigStateEnumMapper();
        private readonly TaCategoriesEnumMapper taCategoriesEnumMapper = new TaCategoriesEnumMapper();
        [PreserveSource(Hint = "iPsdz added", SignatureModified = true)]
        public PsdzObjectBuilder(IObjectBuilderService objectBuilderService, IPsdz iPsdz)
        {
            this.objectBuilderService = objectBuilderService;
            //[+] baureiheReader = new BaureiheReader(iPsdz);
            baureiheReader = new BaureiheReader(iPsdz);
        }

        public IPsdzDiagAddress BuildDiagAddress(int diagAddress)
        {
            return new PsdzDiagAddress
            {
                Offset = diagAddress
            };
        }

        public IPsdzEcu BuildEcu(IEcuObj ecuInput)
        {
            return CreateEcu(ecuInput);
        }

        private IPsdzEcu CreateEcu(IEcuObj ecuInput)
        {
            if (ecuInput == null)
            {
                return null;
            }

            PsdzEcu psdzEcu = new PsdzEcu();
            psdzEcu.PrimaryKey = BuildEcuIdentifier(ecuInput.EcuIdentifier);
            psdzEcu.BaseVariant = ecuInput.BaseVariant;
            psdzEcu.EcuVariant = ecuInput.EcuVariant;
            psdzEcu.BnTnName = ecuInput.BnTnName;
            if (ecuInput.GatewayDiagAddrAsInt.HasValue)
            {
                psdzEcu.GatewayDiagAddr = BuildDiagAddress(ecuInput.GatewayDiagAddrAsInt.Value);
            }

            psdzEcu.DiagnosticBus = BusMapper.MapToPsdzBus(ecuInput.DiagBus);
            psdzEcu.SerialNumber = ecuInput.SerialNumber;
            if (ecuInput.EcuDetailInfo != null)
            {
                psdzEcu.EcuDetailInfo = new PsdzEcuDetailInfo
                {
                    ByteValue = ecuInput.EcuDetailInfo.Value
                };
            }

            if (ecuInput.EcuStatusInfo != null)
            {
                psdzEcu.EcuStatusInfo = new PsdzEcuStatusInfo
                {
                    ByteValue = ecuInput.EcuStatusInfo.Value,
                    HasIndividualData = ecuInput.EcuStatusInfo.HasIndividualData
                };
            }

            psdzEcu.BusConnections = ((ecuInput.BusCons != null) ? ecuInput.BusCons.Select(BusMapper.MapToPsdzBus) : null);
            IPsdzStandardSvk standardSvk = BuildSvk(ecuInput.StandardSvk);
            psdzEcu.StandardSvk = standardSvk;
            psdzEcu.PsdzEcuPdxInfo = BuildPdxInfo(ecuInput.EcuPdxInfo);
            if (psdzEcu.IsSmartActuator)
            {
                if (!(ecuInput is SmartActuatorECU smartActuatorECU))
                {
                    Log.Error(Log.CurrentMethod(), $"{ecuInput} is not a SmartActuatorECU");
                    return psdzEcu;
                }

                return new PsdzSmartActuatorEcu(psdzEcu)
                {
                    SmacID = smartActuatorECU.SmacID,
                    SmacMasterDiagAddress = BuildDiagAddress(smartActuatorECU.SmacMasterDiagAddressAsInt.Value)
                };
            }

            IPsdzEcuPdxInfo psdzEcuPdxInfo = psdzEcu.PsdzEcuPdxInfo;
            if (psdzEcuPdxInfo != null && psdzEcuPdxInfo.IsSmartActuatorMaster)
            {
                if (!(ecuInput is SmartActuatorMasterECU smartActuatorMasterECU))
                {
                    Log.Error(Log.CurrentMethod(), $"{ecuInput} is not a SmartActuatorMasterECU");
                    return psdzEcu;
                }

                return new PsdzSmartActuatorMasterEcu(psdzEcu)
                {
                    SmacMasterSVK = BuildSvk(smartActuatorMasterECU.SmacMasterSVK),
                    SmartActuatorEcus = smartActuatorMasterECU.SmartActuators.Select((ISmartActuatorEcu x) => CreateEcu(x))
                };
            }

            return psdzEcu;
        }

        private IPsdzEcuPdxInfo BuildPdxInfo(IEcuPdxInfo ecuPdxInfo)
        {
            if (ecuPdxInfo == null)
            {
                return null;
            }

            return new PsdzEcuPdxInfo
            {
                CertVersion = ecuPdxInfo.CertVersion,
                IsCert2018 = ecuPdxInfo.IsCert2018,
                IsCert2021 = ecuPdxInfo.IsCert2021,
                IsCert2025 = ecuPdxInfo.IsCert2025,
                IsCertEnabled = ecuPdxInfo.IsCertEnabled,
                IsSecOcEnabled = ecuPdxInfo.IsSecOcEnabled,
                IsSfaEnabled = ecuPdxInfo.IsSfaEnabled,
                IsIPSecEnabled = ecuPdxInfo.IsIPSecEnabled,
                IsLcsServicePackSupported = ecuPdxInfo.IsLcsServicePackSupported,
                IsLcsSystemTimeSwitchSupported = ecuPdxInfo.IsLcsSystemTimeSwitchSupported,
                LcsIntegrityProtectionOCSupported = ecuPdxInfo.IsLcsIntegrityProtectionOCSupported,
                LcsIukCluster = ecuPdxInfo.IsLcsIukCluster,
                IsMACsecEnabled = ecuPdxInfo.IsMACsecEnabled,
                IsMirrorProtocolSupported = ecuPdxInfo.IsMirrorProtocolSupported,
                IsEcuAuthEnabled = ecuPdxInfo.IsEcuAuthEnabled,
                IsIPsecBitmaskSupported = ecuPdxInfo.IsIPsecBitmaskSupported,
                ProgrammingProtectionLevel = ecuPdxInfo.ProgrammingProtectionLevel,
                IsSmartActuatorMaster = ecuPdxInfo.IsSmartActuatorMaster,
                ServicePack = ecuPdxInfo.ServicePack,
                AclEnabled = ecuPdxInfo.IsAclEnabled
            };
        }

        public IPsdzEcuIdentifier BuildEcuIdentifier(IEcuIdentifier ecuIdentifier)
        {
            if (ecuIdentifier != null)
            {
                return BuildEcuIdentifier(ecuIdentifier.DiagAddrAsInt, ecuIdentifier.BaseVariant);
            }

            return null;
        }

        public IPsdzEcuIdentifier BuildEcuIdentifier(int diagAddrAsInt, string baseVariant)
        {
            return new PsdzEcuIdentifier
            {
                BaseVariant = baseVariant,
                DiagnosisAddress = BuildDiagAddress(diagAddrAsInt)
            };
        }

        public IPsdzFa BuildEmptyFa()
        {
            return new PsdzFa
            {
                EWords = Enumerable.Empty<string>(),
                HOWords = Enumerable.Empty<string>(),
                Salapas = Enumerable.Empty<string>()
            };
        }

        [PreserveSource(Hint = "Added compiler switch", SignatureModified = true)]
        public IPsdzFa BuildFa(IPsdzStandardFa fa, string vin17)
        {
            if (fa == null)
            {
                return null;
            }

            PsdzFa fa2 = new PsdzFa
            {
//[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
                Vin = vin17,
//[+] #else
#else
                Vin = (string.IsNullOrEmpty(fa.Vin) ? vin17 : fa.Vin),

//[+] #endif
#endif
                IsValid = fa.IsValid,
                FaVersion = fa.FaVersion,
                Entwicklungsbaureihe = fa.Entwicklungsbaureihe,
                Lackcode = fa.Lackcode,
                Polstercode = fa.Polstercode,
                Type = fa.Type,
                Zeitkriterium = fa.Zeitkriterium,
                EWords = fa.EWords,
                HOWords = fa.HOWords,
                Salapas = fa.Salapas,
                AsString = fa.AsString
            };
            return ValidateBuiltFaObjectViaPsdz(fa2);
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public IPsdzFa BuildFa(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa faInput, string vin17)
        {
            if (faInput == null)
            {
                return null;
            }

            PsdzFa fa = new PsdzFa
            {
                Vin = vin17,
                FaVersion = faInput.FaVersion,
                Entwicklungsbaureihe = faInput.Entwicklungsbaureihe,
                Lackcode = faInput.Lackcode,
                Polstercode = faInput.Polstercode,
                Type = faInput.Type,
                Zeitkriterium = faInput.Zeitkriterium,
                EWords = faInput.EWords,
                HOWords = faInput.HOWords,
                Salapas = faInput.Salapas
            };
            return ValidateBuiltFaObjectViaPsdz(fa);
        }

        public IPsdzFa BuildFaFromXml(string xml)
        {
            return objectBuilderService.BuildFaFromXml(xml);
        }

        public IPsdzStandardFa BuildFaActualFromVehicleContext(IVehicle vehicleContext)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            try
            {
                PsdzStandardFa psdzStandardFa = new PsdzStandardFa();
                if (vehicleContext.FA != null)
                {
                    string baureiheFormatted = baureiheReader.GetBaureiheFormatted(vehicleContext.FA.BR);
                    psdzStandardFa.Entwicklungsbaureihe = baureiheFormatted;
                    psdzStandardFa.Type = vehicleContext.FA.TYPE;
                    psdzStandardFa.Zeitkriterium = vehicleContext.FA.C_DATE;
                    psdzStandardFa.Lackcode = vehicleContext.FA.LACK;
                    psdzStandardFa.Polstercode = vehicleContext.FA.POLSTER;
//[+] #if !OLD_PSDZ_FA
#if !OLD_PSDZ_FA
                    psdzStandardFa.Vin = vehicleContext.VIN17;
//[+] #endif
#endif
                    IList<string> list = new List<string>();
                    foreach (string item in vehicleContext.FA.E_WORT)
                    {
                        list.Add(item);
                    }

                    psdzStandardFa.EWords = list;
                    IList<string> list2 = new List<string>();
                    foreach (string item2 in vehicleContext.FA.HO_WORT)
                    {
                        list2.Add(item2);
                    }

                    psdzStandardFa.HOWords = list2;
                    IList<string> list3 = new List<string>();
                    foreach (string item3 in vehicleContext.FA.SA)
                    {
                        list3.Add(item3);
                    }

                    psdzStandardFa.Salapas = list3;
                    psdzStandardFa.AsString = vehicleContext.FA.STANDARD_FA;
                    psdzStandardFa.IsValid = vehicleContext.FA.AlreadyDone;
                    psdzStandardFa.FaVersion = 3;
                }

                return psdzStandardFa;
            }
            catch (Exception exception)
            {
                Log.WarningException(currentMethod.Name, exception);
                return null;
            }
        }

        public IPsdzFp BuildFp(IVehicleProfile vehicleProfile)
        {
            if (vehicleProfile == null)
            {
                return null;
            }

            return new PsdzFp
            {
                AsString = vehicleProfile.AsString,
                Entwicklungsbaureihe = vehicleProfile.Entwicklungsbaureihe,
                Baureihenverbund = vehicleProfile.Baureihenverbund
            };
        }

        public IPsdzIstufenTriple BuildIStufenTripleActualFromVehicleContext(IVehicle vehicleContext)
        {
            string iLevelWerk = vehicleContext.ILevelWerk;
            string iLevel = vehicleContext.ILevel;
            string iLevel2 = vehicleContext.ILevel;
            return BuildIstufenTriple(iLevelWerk, iLevel, iLevel2);
        }

        public IPsdzIstufe BuildIstufe(string istufe)
        {
            return new PsdzIstufe
            {
                IsValid = true,
                Value = istufe
            };
        }

        public IPsdzIstufenTriple BuildIstufenTriple(string shipment, string last, string current)
        {
            return new PsdzIstufenTriple
            {
                Shipment = shipment,
                Last = last,
                Current = current
            };
        }

        public IPsdzStandardSvt BuildStandardSvtActualFromVehicleContext(IVehicle vehicleContext, IEnumerable<IPsdzEcuIdentifier> ecuListFromPsdz = null)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            try
            {
                PsdzStandardSvt psdzStandardSvt = null;
                if (vehicleContext != null && vehicleContext.ECU != null)
                {
                    psdzStandardSvt = new PsdzStandardSvt();
                    IList<IPsdzEcu> list = new List<IPsdzEcu>();
                    foreach (IEcu srcEcu in vehicleContext.ECU)
                    {
                        PsdzEcu psdzEcu = new PsdzEcu();
                        IPsdzEcuIdentifier psdzEcuIdentifier = ecuListFromPsdz?.FirstOrDefault((IPsdzEcuIdentifier e) => e.DiagAddrAsInt == (int)srcEcu.ID_SG_ADR);
                        if (psdzEcuIdentifier != null)
                        {
                            psdzEcu.PrimaryKey = BuildEcuIdentifier((int)srcEcu.ID_SG_ADR, psdzEcuIdentifier.BaseVariant);
                        }
                        else
                        {
                            psdzEcu.PrimaryKey = BuildEcuIdentifier((int)srcEcu.ID_SG_ADR, srcEcu.ECU_GROBNAME);
                        }

                        PsdzStandardSvk psdzStandardSvk = new PsdzStandardSvk();
                        SgbmIdParser sgbmIdParser = new SgbmIdParser();
                        IList<IPsdzSgbmId> list2 = new List<IPsdzSgbmId>();
                        foreach (string item2 in srcEcu.SVK.XWE_SGBMID)
                        {
                            if (sgbmIdParser.ParseDec(item2))
                            {
                                IPsdzSgbmId item = BuildPsdzSgbmId(sgbmIdParser.ProcessClass, sgbmIdParser.Id, sgbmIdParser.MainVersion, sgbmIdParser.SubVersion, sgbmIdParser.PatchVersion);
                                list2.Add(item);
                            }
                        }

                        psdzStandardSvk.SgbmIds = list2;
                        psdzEcu.StandardSvk = psdzStandardSvk;
                        list.Add(psdzEcu);
                    }

                    psdzStandardSvt.Ecus = list;
                }

                return psdzStandardSvt;
            }
            catch (Exception exception)
            {
                Log.WarningException(currentMethod.Name, exception);
                return null;
            }
        }

        public IPsdzSgbmId BuildPsdzSgbmId(string processClass, long id, int mainVersion, int subVersion, int patchVersion)
        {
            PsdzSgbmId psdzSgbmId = new PsdzSgbmId();
            psdzSgbmId.ProcessClass = processClass;
            psdzSgbmId.IdAsLong = id;
            psdzSgbmId.Id = id.ToString("X8", CultureInfo.InvariantCulture);
            psdzSgbmId.MainVersion = mainVersion;
            psdzSgbmId.SubVersion = subVersion;
            psdzSgbmId.PatchVersion = patchVersion;
            psdzSgbmId.HexString = string.Format(CultureInfo.InvariantCulture, "{0}-{1:X8}-{2:X2}.{3:X2}.{4:X2}", processClass, id, mainVersion, subVersion, patchVersion);
            return psdzSgbmId;
        }

        public IPsdzSvt BuildSvt(IPsdzStandardSvt svtInput, string vin17)
        {
            if (svtInput == null)
            {
                return null;
            }

            return new PsdzSvt
            {
                Vin = vin17,
                AsString = svtInput.AsString,
                IsValid = true,
                Version = svtInput.Version,
                Ecus = svtInput.Ecus,
                HoSignature = svtInput.HoSignature,
                HoSignatureDate = svtInput.HoSignatureDate
            };
        }

        public IPsdzSvt BuildSvt(ISvt svtInput, string vin17)
        {
            if (svtInput == null)
            {
                return null;
            }

            return new PsdzSvt
            {
                Vin = vin17,
                Version = svtInput.Version,
                HoSignature = svtInput.HoSignature,
                HoSignatureDate = svtInput.HoSignatureDate,
                Ecus = ((svtInput.Ecus != null) ? svtInput.Ecus.Select(CreateEcu) : Enumerable.Empty<IPsdzEcu>()),
                IsValid = true
            };
        }

        public IPsdzSwtAction BuildSwtAction(ISwt swt)
        {
            if (swt == null)
            {
                return null;
            }

            return new PsdzSwtAction
            {
                SwtEcus = ((swt.Ecus != null) ? swt.Ecus.Select(BuildSwtEcu) : Enumerable.Empty<IPsdzSwtEcu>())
            };
        }

        public IPsdzSwtApplication BuildSwtApplication(IFSCProvided fscProvided)
        {
            if (fscProvided == null || fscProvided.FscItem == null || fscProvided.FscItem.SwID == null)
            {
                return null;
            }

            IFscItemType fscItem = fscProvided.FscItem;
            int appNo = int.Parse(fscItem.SwID.ApplicationNo, NumberStyles.AllowHexSpecifier);
            int upgradeIdx = int.Parse(fscItem.SwID.UpgradeIndex, NumberStyles.AllowHexSpecifier);
            byte[] fsc = ((fscProvided.FscItem.Fsc != null) ? fscProvided.FscItem.Fsc.GetBinaryValue() : null);
            byte[] fscCertificate = ((fscProvided.Certificate != null) ? fscProvided.Certificate.GetBinaryValue() : null);
            return BuildSwtApplication(appNo, upgradeIdx, fsc, fscCertificate, null);
        }

        public IPsdzSwtApplication BuildSwtApplication(int appNo, int upgradeIdx, byte[] fsc, byte[] fscCertificate, SwtActionType? swtActionType)
        {
            IPsdzSwtApplicationId swtApplicationId = BuildSwtApplicationId(appNo, upgradeIdx);
            return new PsdzSwtApplication
            {
                SwtApplicationId = swtApplicationId,
                Fsc = fsc,
                FscCert = fscCertificate,
                SwtActionType = (swtActionType.HasValue ? new PsdzSwtActionType? (swtActionTypeEnumMapper.GetValue(swtActionType.Value)) : ((PsdzSwtActionType? )null))
            };
        }

        public IPsdzSwtApplicationId BuildSwtApplicationId(ISwtApplicationId swtApplicationId)
        {
            if (swtApplicationId == null)
            {
                return null;
            }

            return BuildSwtApplicationId(swtApplicationId.AppNo, swtApplicationId.UpgradeIdx);
        }

        public IPsdzSwtApplicationId BuildSwtApplicationId(int appNo, int upgradeIdx)
        {
            return new PsdzSwtApplicationId
            {
                ApplicationNumber = appNo,
                UpgradeIndex = upgradeIdx
            };
        }

        public IPsdzTalFilter BuildTalFilter()
        {
            return objectBuilderService.BuildEmptyTalFilter();
        }

        public IPsdzTal BuildTalFromXml(string xml)
        {
            return objectBuilderService.BuildTalFromXml(xml);
        }

        public IPsdzTal BuildEmptyTal()
        {
            return objectBuilderService.BuildEmptyTal();
        }

        public IPsdzVin BuildVin(string vin17)
        {
            return new PsdzVin
            {
                Value = vin17
            };
        }

        public IPsdzTalFilter DefineFilterForAllEcus(TaCategories[] taCategories, TalFilterOptions talFilterOptions, IPsdzTalFilter filter)
        {
            taCategories = RemoveIdDeleteAndLogOccurence(taCategories);
            PsdzTalFilterAction talFilterAction = ConvertTalFilterOptionToTalFilterAction(talFilterOptions);
            PsdzTaCategories[] psdzTaCategories = taCategories?.Select(taCategoriesEnumMapper.GetValue).ToArray();
            return objectBuilderService.DefineFilterForAllEcus(psdzTaCategories, talFilterAction, filter);
        }

        public IPsdzTalFilter DefineFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions, IPsdzTalFilter inputTalFilter, IDictionary<string, TalFilterOptions> smacFilter = null)
        {
            taCategories = RemoveIdDeleteAndLogOccurence(taCategories);
            PsdzTalFilterAction talFilterAction = ConvertTalFilterOptionToTalFilterAction(talFilterOptions);
            PsdzTaCategories[] psdzTaCategories = taCategories?.Select(taCategoriesEnumMapper.GetValue).ToArray();
            Dictionary<string, PsdzTalFilterAction> smacFilter2 = null;
            if (smacFilter != null)
            {
                smacFilter2 = smacFilter.Select((KeyValuePair<string, TalFilterOptions> x) => new KeyValuePair<string, PsdzTalFilterAction>(x.Key, ConvertTalFilterOptionToTalFilterAction(x.Value))).ToDictionary((KeyValuePair<string, PsdzTalFilterAction> x) => x.Key, (KeyValuePair<string, PsdzTalFilterAction> y) => y.Value);
            }

            return objectBuilderService.DefineFilterForSelectedEcus(psdzTaCategories, diagAddress, talFilterAction, inputTalFilter, smacFilter2);
        }

        public IPsdzTalFilter DefineFilterForSWEs(IEcuFilterOnSweLevel ecuFilter, IPsdzTalFilter talFilter)
        {
            List<IPsdzSweTalFilterOptions> list = new List<IPsdzSweTalFilterOptions>();
            foreach (ISweTalFilterOptions sweTalFilterOption in ecuFilter.SweTalFilterOptions)
            {
                if (sweTalFilterOption.Ta != null)
                {
                    Dictionary<string, PsdzTalFilterAction> dictionary = new Dictionary<string, PsdzTalFilterAction>();
                    for (int i = 0; i < sweTalFilterOption.SgbmIds.Count; i++)
                    {
                        dictionary.Add(sweTalFilterOption.SgbmIds[i], ConvertTalFilterOptionToTalFilterAction(sweTalFilterOption.SweFilter[i]));
                    }

                    list.Add(new PsdzSweTalFilterOptions { ProcessClass = sweTalFilterOption.ProcessClass, SweFilter = dictionary, Ta = sweTalFilterOption.Ta });
                }
            }

            PsdzTaCategories value = taCategoriesEnumMapper.GetValue(ecuFilter.TaCategory);
            PsdzTalFilterAction talFilterAction = ConvertTalFilterOptionToTalFilterAction(ecuFilter.TalFilterOptions);
            return objectBuilderService.DefineFilterForSwes(ecuFilter.DiagAddress, talFilterAction, value, list, talFilter);
        }

        private static PsdzTalFilterAction ConvertTalFilterOptionToTalFilterAction(TalFilterOptions talFilterOptions)
        {
            switch (talFilterOptions)
            {
                case TalFilterOptions.Allowed:
                    return PsdzTalFilterAction.AllowedToBeTreated;
                case TalFilterOptions.Must:
                    return PsdzTalFilterAction.MustBeTreated;
                case TalFilterOptions.MustNot:
                    return PsdzTalFilterAction.MustNotBeTreated;
                case TalFilterOptions.Only:
                    return PsdzTalFilterAction.OnlyToBeTreatedAndBlockCategoryInAllEcu;
                default:
                    return PsdzTalFilterAction.Empty;
            }
        }

        private TaCategories[] RemoveIdDeleteAndLogOccurence(TaCategories[] taCategories)
        {
            if (taCategories != null && taCategories.AsEnumerable().Any((TaCategories a) => a == TaCategories.IdDelete))
            {
                List<TaCategories> list = new List<TaCategories>(taCategories);
                list.Remove(TaCategories.IdDelete);
                Log.Info("PsdzObjectBuilder.RemoveIdDeleteAndLogOccurence()", "IdDelete got removed from the TaCategories");
                return list.ToArray();
            }

            return taCategories;
        }

        public IPsdzAsamJobInputDictionary BuildAsamJobInputDictionary(IAsamJobInputDictionary inputDictionary)
        {
            if (inputDictionary == null)
            {
                return null;
            }

            IPsdzAsamJobInputDictionary psdzAsamJobInputDictionary = new PsdzAsamJobInputDictionary();
            foreach (KeyValuePair<string, object> item in inputDictionary.GetCopy())
            {
                if (item.Value is string value)
                {
                    psdzAsamJobInputDictionary.Add(item.Key, value);
                    continue;
                }

                if (item.Value is byte[] value2)
                {
                    psdzAsamJobInputDictionary.Add(item.Key, value2);
                    continue;
                }

                Type type = item.Value.GetType();
                if (type == typeof(int))
                {
                    psdzAsamJobInputDictionary.Add(item.Key, (int)item.Value);
                    continue;
                }

                if (type == typeof(long))
                {
                    psdzAsamJobInputDictionary.Add(item.Key, (long)item.Value);
                    continue;
                }

                if (type == typeof(float))
                {
                    psdzAsamJobInputDictionary.Add(item.Key, (float)item.Value);
                    continue;
                }

                if (type == typeof(double))
                {
                    psdzAsamJobInputDictionary.Add(item.Key, (double)item.Value);
                    continue;
                }

                Log.Warning("PsdzObjectBuilder.BuildAsamJobInputDictionary()", "Type {0} is not supported.", type);
            }

            return psdzAsamJobInputDictionary;
        }

        private IPsdzFa ValidateBuiltFaObjectViaPsdz(PsdzFa fa)
        {
            return objectBuilderService.BuildFa(fa);
        }

        private IPsdzStandardSvk BuildSvk(IStandardSvk svkInput)
        {
            PsdzStandardSvk psdzStandardSvk = new PsdzStandardSvk();
            if (svkInput != null)
            {
                psdzStandardSvk.SvkVersion = svkInput.SvkVersion;
                psdzStandardSvk.ProgDepChecked = svkInput.ProgDepChecked;
                psdzStandardSvk.SgbmIds = ((svkInput.SgbmIds != null) ? svkInput.SgbmIds.Select(BuildPsdzSgbmId) : null);
            }

            return psdzStandardSvk;
        }

        private static IPsdzSgbmId BuildPsdzSgbmId(ISgbmId sgbmId)
        {
            return new PsdzSgbmId
            {
                Id = sgbmId.Id.ToString("X8", CultureInfo.InvariantCulture),
                IdAsLong = sgbmId.Id,
                MainVersion = sgbmId.MainVersion,
                SubVersion = sgbmId.SubVersion,
                PatchVersion = sgbmId.PatchVersion,
                ProcessClass = sgbmId.ProcessClass,
                HexString = sgbmId.HexString
            };
        }

        private IPsdzSwtApplication BuildSwtApplication(ISwtApplication swtApplication)
        {
            if (swtApplication == null)
            {
                throw new ArgumentNullException("swtApplication");
            }

            PsdzSwtApplication obj = new PsdzSwtApplication
            {
                Fsc = swtApplication.Fsc,
                FscCert = swtApplication.FscCertificate,
                FscCertState = fscCertificateStateEnumMapper.GetValue(swtApplication.FscCertificateState),
                FscState = fscStateEnumMapper.GetValue(swtApplication.FscState),
                Position = swtApplication.Position,
                SwtType = swtTypeEnumMapper.GetValue(swtApplication.SwtType),
                SwtActionType = (swtApplication.SwtActionType.HasValue ? new PsdzSwtActionType? (new SwtActionTypeEnumMapper().GetValue(swtApplication.SwtActionType.Value)) : ((PsdzSwtActionType? )null)),
                IsBackupPossible = swtApplication.IsBackupPossible
            };
            IPsdzSwtApplicationId swtApplicationId = BuildSwtApplicationId(swtApplication.Id);
            obj.SwtApplicationId = swtApplicationId;
            return obj;
        }

        private IPsdzSwtEcu BuildSwtEcu(ISwtEcu swtEcuInput)
        {
            if (swtEcuInput == null)
            {
                return null;
            }

            PsdzSwtEcu psdzSwtEcu = new PsdzSwtEcu();
            IPsdzEcuIdentifier ecuIdentifier = BuildEcuIdentifier(swtEcuInput.EcuIdentifier);
            psdzSwtEcu.EcuIdentifier = ecuIdentifier;
            psdzSwtEcu.RootCertState = rootCertificateStateEnumMapper.GetValue(swtEcuInput.RootCertificateState);
            psdzSwtEcu.SoftwareSigState = softwareSigStateEnumMapper.GetValue(swtEcuInput.SoftwareSigState);
            IList<IPsdzSwtApplication> list = new List<IPsdzSwtApplication>();
            foreach (ISwtApplication swtApplication in swtEcuInput.SwtApplications)
            {
                IPsdzSwtApplication item = BuildSwtApplication(swtApplication);
                list.Add(item);
            }

            psdzSwtEcu.SwtApplications = list;
            return psdzSwtEcu;
        }

        public PsdzFetchEcuCertCheckingResult BuildFetchEcuCertCheckingResult(IFetchEcuCertCheckingResult fetchEcuCertCheckingResult)
        {
            return CreateFetchEcuCertCheckingResult(fetchEcuCertCheckingResult);
        }

        private PsdzFetchEcuCertCheckingResult CreateFetchEcuCertCheckingResult(IFetchEcuCertCheckingResult fetchEcuCertCheckingResult)
        {
            if (fetchEcuCertCheckingResult == null)
            {
                return null;
            }

            return new PsdzFetchEcuCertCheckingResult
            {
                FailedEcus = BuildEcuCertCheckingResultFailedEcus(fetchEcuCertCheckingResult.FailedEcus),
                Results = BuildEcuCertCheckingResults(fetchEcuCertCheckingResult.Results)
            };
        }

        private IEnumerable<PsdzEcuFailureResponse> BuildEcuCertCheckingResultFailedEcus(IEnumerable<IEcuFailureResponse> failedEcus)
        {
            List<PsdzEcuFailureResponse> list = new List<PsdzEcuFailureResponse>();
            if (failedEcus != null && failedEcus.Count() > 0)
            {
                foreach (IEcuFailureResponse failedEcu in failedEcus)
                {
                    list.Add(new PsdzEcuFailureResponse { Ecu = BuildEcuIdentifier(failedEcu.Ecu), Reason = failedEcu.Reason });
                }
            }

            return list;
        }

        private IEnumerable<PsdzEcuCertCheckingResponse> BuildEcuCertCheckingResults(IEnumerable<IEcuCertCheckingResponse> results)
        {
            List<PsdzEcuCertCheckingResponse> list = new List<PsdzEcuCertCheckingResponse>();
            if (results != null && results.Count() > 0)
            {
                foreach (IEcuCertCheckingResponse result in results)
                {
                    list.Add(new PsdzEcuCertCheckingResponse { BindingDetailStatus = BuildDetailStatus(result.BindingDetailStatus), BindingsStatus = BuildEcuCertCheckingStatus(result.BindingsStatus), CertificateStatus = BuildEcuCertCheckingStatus(result.CertificateStatus), Ecu = BuildEcuIdentifier(result.Ecu), OtherBindingDetailStatus = BuildOtherBindingDetailStatus(result.OtherBindingDetailStatus), OtherBindingsStatus = BuildEcuCertCheckingStatus(result.OtherBindingsStatus), KeyPackStatus = BuildEcuCertCheckingStatus(result.KeypackStatus), OnlineBindingsStatus = BuildEcuCertCheckingStatus(result.OnlineBindingsStatus), OnlineBindingDetailStatus = BuildDetailStatus(result.OnlineBindingDetailStatus), OnlineCertificateStatus = BuildEcuCertCheckingStatus(result.OnlineCertificateStatus), KeyPackDatailedStatus = BuildKeypackDetailStatus(result.KeyPackDetailedStatus), CreationTimestamp = result.CreationTimestamp });
                }
            }

            return list;
        }

        private PsdzOtherBindingDetailsStatus[] BuildOtherBindingDetailStatus(IOtherBindingDetailsStatus[] arrOtherBindingDetailStatus)
        {
            List<PsdzOtherBindingDetailsStatus> list = new List<PsdzOtherBindingDetailsStatus>();
            if (arrOtherBindingDetailStatus != null && arrOtherBindingDetailStatus.Count() > 0)
            {
                foreach (IOtherBindingDetailsStatus otherBindingDetailsStatus in arrOtherBindingDetailStatus)
                {
                    list.Add(new PsdzOtherBindingDetailsStatus { EcuName = otherBindingDetailsStatus.EcuName, OtherBindingStatus = BuildEcuCertCheckingStatus(otherBindingDetailsStatus.OtherBindingStatus), RollenName = otherBindingDetailsStatus.RollenName });
                }
            }

            if (list != null && list.Count > 0)
            {
                return list.ToArray();
            }

            return null;
        }

        private PsdzEcuCertCheckingStatus? BuildEcuCertCheckingStatus(EcuCertCheckingStatus? bindingStatus)
        {
            switch (bindingStatus)
            {
                case EcuCertCheckingStatus.CheckStillRunning:
                    return PsdzEcuCertCheckingStatus.CheckStillRunning;
                case EcuCertCheckingStatus.Empty:
                    return PsdzEcuCertCheckingStatus.Empty;
                case EcuCertCheckingStatus.Incomplete:
                    return PsdzEcuCertCheckingStatus.Incomplete;
                case EcuCertCheckingStatus.Malformed:
                    return PsdzEcuCertCheckingStatus.Malformed;
                case EcuCertCheckingStatus.Ok:
                    return PsdzEcuCertCheckingStatus.Ok;
                case EcuCertCheckingStatus.Other:
                    return PsdzEcuCertCheckingStatus.Other;
                case EcuCertCheckingStatus.SecurityError:
                    return PsdzEcuCertCheckingStatus.SecurityError;
                case EcuCertCheckingStatus.Unchecked:
                    return PsdzEcuCertCheckingStatus.Unchecked;
                case EcuCertCheckingStatus.WrongVin17:
                    return PsdzEcuCertCheckingStatus.WrongVin17;
                case EcuCertCheckingStatus.Decryption_Error:
                    return PsdzEcuCertCheckingStatus.Decryption_Error;
                case EcuCertCheckingStatus.IssuerCertError:
                    return PsdzEcuCertCheckingStatus.IssuerCertError;
                case EcuCertCheckingStatus.Outdated:
                    return PsdzEcuCertCheckingStatus.Outdated;
                case EcuCertCheckingStatus.OwnCertNotPresent:
                    return PsdzEcuCertCheckingStatus.OwnCertNotPresent;
                case EcuCertCheckingStatus.Undefined:
                    return PsdzEcuCertCheckingStatus.Undefined;
                case EcuCertCheckingStatus.WrongEcuUid:
                    return PsdzEcuCertCheckingStatus.WrongEcuUid;
                default:
                    return PsdzEcuCertCheckingStatus.Empty;
            }
        }

        private PsdzBindingDetailsStatus[] BuildDetailStatus(IBindingDetailsStatus[] arrBindingDetailStatus)
        {
            List<PsdzBindingDetailsStatus> list = new List<PsdzBindingDetailsStatus>();
            if (arrBindingDetailStatus != null && arrBindingDetailStatus.Count() > 0)
            {
                foreach (IBindingDetailsStatus bindingDetailsStatus in arrBindingDetailStatus)
                {
                    list.Add(new PsdzBindingDetailsStatus { BindingStatus = BuildEcuCertCheckingStatus(bindingDetailsStatus.BindingStatus), CertificateStatus = BuildEcuCertCheckingStatus(bindingDetailsStatus.CertificateStatus), RollenName = bindingDetailsStatus.RollenName });
                }
            }

            if (list != null && list.Count > 0)
            {
                return list.ToArray();
            }

            return null;
        }

        private PsdzKeypackDetailStatus[] BuildKeypackDetailStatus(IKeypackDetailStatus[] keypackDetailStatuses)
        {
            if (keypackDetailStatuses == null || keypackDetailStatuses.Length == 0)
            {
                return null;
            }

            PsdzKeypackDetailStatus[] array = new PsdzKeypackDetailStatus[keypackDetailStatuses.Length];
            for (int i = 0; i < keypackDetailStatuses.Length; i++)
            {
                if (keypackDetailStatuses[i] != null)
                {
                    array[i] = new PsdzKeypackDetailStatus
                    {
                        KeyPackStatus = BuildEcuCertCheckingStatus(keypackDetailStatuses[i].KeyPackStatus),
                        KeyId = keypackDetailStatuses[i].KeyId
                    };
                }
            }

            return array;
        }

        public IList<IPsdzFeatureSpecificFieldCto> BuildFeatureSpecificFieldsCto(IList<IFeatureSpecificField> featureSpecificFields)
        {
            List<IPsdzFeatureSpecificFieldCto> list = new List<IPsdzFeatureSpecificFieldCto>();
            foreach (IFeatureSpecificField featureSpecificField in featureSpecificFields)
            {
                PsdzFeatureSpecificFieldCto psdzFeatureSpecificFieldCto = new PsdzFeatureSpecificFieldCto();
                psdzFeatureSpecificFieldCto.FieldType = featureSpecificField.FieldType;
                psdzFeatureSpecificFieldCto.FieldValue = featureSpecificField.FieldValue;
                list.Add(psdzFeatureSpecificFieldCto);
            }

            return list;
        }

        public IList<IPsdzValidityConditionCto> BuildValidityConditionsCto(IList<IValidityCondition> validityConditions)
        {
            List<IPsdzValidityConditionCto> list = new List<IPsdzValidityConditionCto>();
            foreach (IValidityCondition validityCondition in validityConditions)
            {
                PsdzValidityConditionCto psdzValidityConditionCto = new PsdzValidityConditionCto();
                psdzValidityConditionCto.ConditionType = BuildConditionTypeEnum(validityCondition.ConditionType);
                psdzValidityConditionCto.ValidityValue = validityCondition.ValidityValue;
                list.Add(psdzValidityConditionCto);
            }

            return list;
        }

        public PsdzConditionTypeEtoEnum BuildConditionTypeEnum(ConditionTypeEnum conditionType)
        {
            switch (conditionType)
            {
                case ConditionTypeEnum.DAYS_AFTER_ACTIVATION:
                    return PsdzConditionTypeEtoEnum.DAYS_AFTER_ACTIVATION;
                case ConditionTypeEnum.END_OF_CONDITIONS:
                    return PsdzConditionTypeEtoEnum.END_OF_CONDITIONS;
                case ConditionTypeEnum.EXPIRATION_DATE:
                    return PsdzConditionTypeEtoEnum.EXPIRATION_DATE;
                case ConditionTypeEnum.KM_AFTER_ACTIVATION:
                    return PsdzConditionTypeEtoEnum.KM_AFTER_ACTIVATION;
                case ConditionTypeEnum.LOCAL_RELATIVE_TIME:
                    return PsdzConditionTypeEtoEnum.LOCAL_RELATIVE_TIME;
                case ConditionTypeEnum.NUMBER_OF_DRIVING_CYCLES:
                    return PsdzConditionTypeEtoEnum.NUMBER_OF_DRIVING_CYCLES;
                case ConditionTypeEnum.NUMBER_OF_EXECUTIONS:
                    return PsdzConditionTypeEtoEnum.NUMBER_OF_EXECUTIONS;
                case ConditionTypeEnum.SPEED_TRESHOLD:
                    return PsdzConditionTypeEtoEnum.SPEED_TRESHOLD;
                case ConditionTypeEnum.START_AND_END_ODOMETER_READING:
                    return PsdzConditionTypeEtoEnum.START_AND_END_ODOMETER_READING;
                case ConditionTypeEnum.TIME_PERIOD:
                    return PsdzConditionTypeEtoEnum.TIME_PERIOD;
                case ConditionTypeEnum.UNLIMITED:
                    return PsdzConditionTypeEtoEnum.UNLIMITED;
                default:
                    throw new ArgumentException($"'{conditionType}' is not a valid value.");
            }
        }
    }
}