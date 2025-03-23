using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz.Client;
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

namespace BMW.Rheingold.Psdz
{
	class PsdzObjectBuilder : IPsdzObjectBuilder
	{
        private readonly IObjectBuilderService objectBuilderService;

        private readonly BusEnumMapper busEnumMapper = new BusEnumMapper();

        private readonly SwtActionTypeEnumMapper swtActionTypeEnumMapper = new SwtActionTypeEnumMapper();

        private readonly FscCertificateStateEnumMapper fscCertificateStateEnumMapper = new FscCertificateStateEnumMapper();

        private readonly FscStateEnumMapper fscStateEnumMapper = new FscStateEnumMapper();

        private readonly SwtTypeEnumMapper swtTypeEnumMapper = new SwtTypeEnumMapper();

        private readonly RootCertificateStateEnumMapper rootCertificateStateEnumMapper = new RootCertificateStateEnumMapper();

        private readonly SoftwareSigStateEnumMapper softwareSigStateEnumMapper = new SoftwareSigStateEnumMapper();

        private readonly TaCategoriesEnumMapper taCategoriesEnumMapper = new TaCategoriesEnumMapper();

        public PsdzObjectBuilder(IObjectBuilderService objectBuilderService)
        {
            this.objectBuilderService = objectBuilderService;
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
            if (psdzEcu.PrimaryKey != null)
            {
                psdzEcu.PrimaryKey = BuildEcuIdentifier(ecuInput.EcuIdentifier);
            }
            psdzEcu.BaseVariant = ecuInput.BaseVariant;
            psdzEcu.EcuVariant = ecuInput.EcuVariant;
            psdzEcu.BnTnName = ecuInput.BnTnName;
            if (ecuInput.GatewayDiagAddrAsInt.HasValue)
            {
                psdzEcu.GatewayDiagAddr = BuildDiagAddress(ecuInput.GatewayDiagAddrAsInt.Value);
            }
            psdzEcu.DiagnosticBus = busEnumMapper.GetValue(ecuInput.DiagnosticBus);
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
            psdzEcu.BusConnections = ((ecuInput.BusConnections != null) ? ecuInput.BusConnections.Select(busEnumMapper.GetValue) : null);
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
                SmartActuatorMasterECU smartActuatorMasterECU = ecuInput as SmartActuatorMasterECU;
                if (smartActuatorMasterECU == null)
                {
                    Log.Error(Log.CurrentMethod(), $"{ecuInput} is not a SmartActuatorMasterECU");
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
            if (ecuPdxInfo != null)
            {
                return new PsdzEcuPdxInfo
                {
                    CertVersion = ecuPdxInfo.CertVersion,
                    IsCert2018 = ecuPdxInfo.IsCert2018,
                    IsCert2021 = ecuPdxInfo.IsCert2021,
                    IsCertEnabled = ecuPdxInfo.IsCertEnabled,
                    IsSecOcEnabled = ecuPdxInfo.IsSecOcEnabled,
                    IsSfaEnabled = ecuPdxInfo.IsSfaEnabled,
                    IsIPSecEnabled = ecuPdxInfo.IsIPSecEnabled,
                    IsLcsServicePackSupported = ecuPdxInfo.IsLcsServicePackSupported,
                    IsLcsSystemTimeSwitchSupported = ecuPdxInfo.IsLcsSystemTimeSwitchSupported,
                    IsMirrorProtocolSupported = ecuPdxInfo.IsMirrorProtocolSupported,
                    IsEcuAuthEnabled = ecuPdxInfo.IsEcuAuthEnabled,
                    IsIPsecBitmaskSupported = ecuPdxInfo.IsIPsecBitmaskSupported,
                    ProgrammingProtectionLevel = ecuPdxInfo.ProgrammingProtectionLevel
                };
            }
            return null;
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

        public IPsdzFa BuildFa(IPsdzStandardFa faInput, string vin17)
        {
            if (faInput == null)
            {
                return null;
            }
            PsdzFa fa = new PsdzFa
            {
                Vin = vin17,
                IsValid = faInput.IsValid,
                FaVersion = faInput.FaVersion,
                Entwicklungsbaureihe = faInput.Entwicklungsbaureihe,
                Lackcode = faInput.Lackcode,
                Polstercode = faInput.Polstercode,
                Type = faInput.Type,
                Zeitkriterium = faInput.Zeitkriterium,
                EWords = faInput.EWords,
                HOWords = faInput.HOWords,
                Salapas = faInput.Salapas,
                AsString = faInput.AsString
            };
            return ValidateBuiltFaObjectViaPsdz(fa);
        }

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
                    string entwicklungsbaureihe = FormatConverter.ConvertToBn2020ConformModelSeries(vehicleContext.FA.BR);
                    psdzStandardFa.Entwicklungsbaureihe = entwicklungsbaureihe;
                    psdzStandardFa.Type = vehicleContext.FA.TYPE;
                    psdzStandardFa.Zeitkriterium = vehicleContext.FA.C_DATE;
                    psdzStandardFa.Lackcode = vehicleContext.FA.LACK;
                    psdzStandardFa.Polstercode = vehicleContext.FA.POLSTER;
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
                SwtActionType = (swtActionType.HasValue ? new PsdzSwtActionType?(swtActionTypeEnumMapper.GetValue(swtActionType.Value)) : ((PsdzSwtActionType?)null))
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
            IPsdzTalFilter ecuTaCategoriesAsMustNot = objectBuilderService.BuildEmptyTalFilter();
            return SetEcuTaCategoriesAsMustNot(ecuTaCategoriesAsMustNot);
        }

        private IPsdzTalFilter SetEcuTaCategoriesAsMustNot(IPsdzTalFilter talFilter)
        {
            TaCategories[] taCategories = new TaCategories[3]
            {
                TaCategories.EcuActivate,
                TaCategories.EcuPoll,
                TaCategories.EcuMirrorDeploy
            };
            talFilter = DefineFilterForAllEcus(taCategories, TalFilterOptions.MustNot, talFilter);
            return talFilter;
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

        public IPsdzTalFilter DefineFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions, IPsdzTalFilter filter, IDictionary<string, TalFilterOptions> smacFilter = null)
        {
            taCategories = RemoveIdDeleteAndLogOccurence(taCategories);
            PsdzTalFilterAction talFilterAction = ConvertTalFilterOptionToTalFilterAction(talFilterOptions);
            PsdzTaCategories[] psdzTaCategories = taCategories?.Select(taCategoriesEnumMapper.GetValue).ToArray();
            Dictionary<string, PsdzTalFilterAction> smacFilter2 = null;
            if (smacFilter != null)
            {
                smacFilter2 = smacFilter.Select((KeyValuePair<string, TalFilterOptions> x) => new KeyValuePair<string, PsdzTalFilterAction>(x.Key, ConvertTalFilterOptionToTalFilterAction(x.Value))).ToDictionary((KeyValuePair<string, PsdzTalFilterAction> x) => x.Key, (KeyValuePair<string, PsdzTalFilterAction> y) => y.Value);
            }
            return objectBuilderService.DefineFilterForSelectedEcus(psdzTaCategories, diagAddress, talFilterAction, filter, smacFilter2);
        }

        public IPsdzTalFilter DefineFilterForSWEs(IPsdzTa ta, int diagAddress, string processClass, TalFilterOptions talFilterOptions, List<string> sgbmIds, List<TalFilterOptions> sweTalFilterOptions, TaCategories taCategory, IPsdzTalFilter filter)
        {
            PsdzTalFilterAction talFilterAction = ConvertTalFilterOptionToTalFilterAction(talFilterOptions);
            if (sgbmIds.Count != sweTalFilterOptions.Count)
            {
                throw new ArgumentException("Sgbmids does not match swe tal filter options in length");
            }
            Dictionary<string, PsdzTalFilterAction> dictionary = new Dictionary<string, PsdzTalFilterAction>();
            for (int i = 0; i < sgbmIds.Count; i++)
            {
                dictionary.Add(sgbmIds[i], ConvertTalFilterOptionToTalFilterAction(sweTalFilterOptions[i]));
            }
            PsdzTaCategories value = taCategoriesEnumMapper.GetValue(taCategory);
            return objectBuilderService.DefineFilterForSwes(diagAddress, ta, processClass, talFilterAction, dictionary, value, filter);
        }

        private static PsdzTalFilterAction ConvertTalFilterOptionToTalFilterAction(TalFilterOptions talFilterOptions)
        {
            switch (talFilterOptions)
            {
                default:
                    return PsdzTalFilterAction.OnlyToBeTreatedAndBlockCategoryInAllEcu;
                case TalFilterOptions.MustNot:
                    return PsdzTalFilterAction.MustNotBeTreated;
                case TalFilterOptions.Must:
                    return PsdzTalFilterAction.MustBeTreated;
                case TalFilterOptions.Allowed:
                    return PsdzTalFilterAction.AllowedToBeTreated;
            }
        }

		private TaCategories[] RemoveIdDeleteAndLogOccurence(TaCategories[] taCategories)
		{
            if (taCategories != null && taCategories.ToList().Any((TaCategories a) => a == TaCategories.IdDelete))
            {
                List<TaCategories> list = new List<TaCategories>(taCategories);
                list.Remove(TaCategories.IdDelete);
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
			foreach (KeyValuePair<string, object> keyValuePair in inputDictionary.GetCopy())
			{
				string text = keyValuePair.Value as string;
				if (text != null)
				{
					psdzAsamJobInputDictionary.Add(keyValuePair.Key, text);
				}
				else
				{
					byte[] array = keyValuePair.Value as byte[];
					if (array != null)
					{
						psdzAsamJobInputDictionary.Add(keyValuePair.Key, array);
					}
					else
					{
						Type type = keyValuePair.Value.GetType();
						if (type == typeof(int))
						{
							psdzAsamJobInputDictionary.Add(keyValuePair.Key, (int)keyValuePair.Value);
						}
						else if (type == typeof(long))
						{
							psdzAsamJobInputDictionary.Add(keyValuePair.Key, (long)keyValuePair.Value);
						}
						else if (type == typeof(float))
						{
							psdzAsamJobInputDictionary.Add(keyValuePair.Key, (float)keyValuePair.Value);
						}
						else if (type == typeof(double))
						{
							psdzAsamJobInputDictionary.Add(keyValuePair.Key, (double)keyValuePair.Value);
						}
						else
						{
						}
					}
				}
			}
			return psdzAsamJobInputDictionary;
		}

		private IPsdzFa ValidateBuiltFaObjectViaPsdz(PsdzFa fa)
		{
			return this.objectBuilderService.BuildFa(fa);
		}

		private IPsdzStandardSvk BuildSvk(IStandardSvk svkInput)
		{
			PsdzStandardSvk psdzStandardSvk = new PsdzStandardSvk();
			if (svkInput != null)
			{
				psdzStandardSvk.SvkVersion = svkInput.SvkVersion;
				psdzStandardSvk.ProgDepChecked = svkInput.ProgDepChecked;
				psdzStandardSvk.SgbmIds = ((svkInput.SgbmIds != null) ? svkInput.SgbmIds.Select(new Func<ISgbmId, IPsdzSgbmId>(PsdzObjectBuilder.BuildPsdzSgbmId)) : null);
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
			PsdzSwtApplication psdzSwtApplication = new PsdzSwtApplication();
			psdzSwtApplication.Fsc = swtApplication.Fsc;
			psdzSwtApplication.FscCert = swtApplication.FscCertificate;
			psdzSwtApplication.FscCertState = this.fscCertificateStateEnumMapper.GetValue(swtApplication.FscCertificateState);
			psdzSwtApplication.FscState = this.fscStateEnumMapper.GetValue(swtApplication.FscState);
			psdzSwtApplication.Position = swtApplication.Position;
			psdzSwtApplication.SwtType = this.swtTypeEnumMapper.GetValue(swtApplication.SwtType);
			psdzSwtApplication.SwtActionType = ((swtApplication.SwtActionType != null) ? new PsdzSwtActionType?(new SwtActionTypeEnumMapper().GetValue(swtApplication.SwtActionType.Value)) : null);
			psdzSwtApplication.IsBackupPossible = swtApplication.IsBackupPossible;
			IPsdzSwtApplicationId swtApplicationId = this.BuildSwtApplicationId(swtApplication.Id);
			psdzSwtApplication.SwtApplicationId = swtApplicationId;
			return psdzSwtApplication;
		}

		private IPsdzSwtEcu BuildSwtEcu(ISwtEcu swtEcuInput)
		{
			if (swtEcuInput == null)
			{
				return null;
			}
			PsdzSwtEcu psdzSwtEcu = new PsdzSwtEcu();
			IPsdzEcuIdentifier ecuIdentifier = this.BuildEcuIdentifier(swtEcuInput.EcuIdentifier);
			psdzSwtEcu.EcuIdentifier = ecuIdentifier;
			psdzSwtEcu.RootCertState = this.rootCertificateStateEnumMapper.GetValue(swtEcuInput.RootCertificateState);
			psdzSwtEcu.SoftwareSigState = this.softwareSigStateEnumMapper.GetValue(swtEcuInput.SoftwareSigState);
			IList<IPsdzSwtApplication> list = new List<IPsdzSwtApplication>();
			foreach (ISwtApplication swtApplication in swtEcuInput.SwtApplications)
			{
				IPsdzSwtApplication item = this.BuildSwtApplication(swtApplication);
				list.Add(item);
			}
			psdzSwtEcu.SwtApplications = list;
			return psdzSwtEcu;
		}

		public PsdzFetchEcuCertCheckingResult BuildFetchEcuCertCheckingResult(IFetchEcuCertCheckingResult fetchEcuCertCheckingResult)
		{
			return this.CreateFetchEcuCertCheckingResult(fetchEcuCertCheckingResult);
		}

		private PsdzFetchEcuCertCheckingResult CreateFetchEcuCertCheckingResult(IFetchEcuCertCheckingResult fetchEcuCertCheckingResult)
		{
			if (fetchEcuCertCheckingResult == null)
			{
				return null;
			}
			return new PsdzFetchEcuCertCheckingResult
			{
				FailedEcus = this.BuildEcuCertCheckingResultFailedEcus(fetchEcuCertCheckingResult.FailedEcus),
				Results = this.BuildEcuCertCheckingResults(fetchEcuCertCheckingResult.Results)
			};
		}

		private IEnumerable<PsdzEcuFailureResponse> BuildEcuCertCheckingResultFailedEcus(IEnumerable<IEcuFailureResponse> failedEcus)
		{
			List<PsdzEcuFailureResponse> list = new List<PsdzEcuFailureResponse>();
			if (failedEcus != null && failedEcus.Count<IEcuFailureResponse>() > 0)
			{
				foreach (IEcuFailureResponse ecuFailureResponse in failedEcus)
				{
					list.Add(new PsdzEcuFailureResponse
					{
						Ecu = this.BuildEcuIdentifier(ecuFailureResponse.Ecu),
						Reason = ecuFailureResponse.Reason
					});
				}
			}
			return list;
		}

		private IEnumerable<PsdzEcuCertCheckingResponse> BuildEcuCertCheckingResults(IEnumerable<IEcuCertCheckingResponse> results)
		{
			List<PsdzEcuCertCheckingResponse> list = new List<PsdzEcuCertCheckingResponse>();
			if (results != null && results.Count<IEcuCertCheckingResponse>() > 0)
			{
				foreach (IEcuCertCheckingResponse ecuCertCheckingResponse in results)
				{
					list.Add(new PsdzEcuCertCheckingResponse
					{
						BindingDetailStatus = this.BuildDetailStatus(ecuCertCheckingResponse.BindingDetailStatus),
						BindingsStatus = this.BuildEcuCertCheckingStatus(ecuCertCheckingResponse.BindingsStatus),
						CertificateStatus = this.BuildEcuCertCheckingStatus(ecuCertCheckingResponse.CertificateStatus),
						Ecu = this.BuildEcuIdentifier(ecuCertCheckingResponse.Ecu),
						OtherBindingDetailStatus = this.BuildOtherBindingDetailStatus(ecuCertCheckingResponse.OtherBindingDetailStatus),
						OtherBindingsStatus = this.BuildEcuCertCheckingStatus(ecuCertCheckingResponse.OtherBindingsStatus)
					});
				}
			}
			return list;
		}

		private PsdzOtherBindingDetailsStatus[] BuildOtherBindingDetailStatus(IOtherBindingDetailsStatus[] arrOtherBindingDetailStatus)
		{
			List<PsdzOtherBindingDetailsStatus> list = new List<PsdzOtherBindingDetailsStatus>();
			if (arrOtherBindingDetailStatus != null && arrOtherBindingDetailStatus.Count<IOtherBindingDetailsStatus>() > 0)
			{
				foreach (IOtherBindingDetailsStatus otherBindingDetailsStatus in arrOtherBindingDetailStatus)
				{
					list.Add(new PsdzOtherBindingDetailsStatus
					{
						EcuName = otherBindingDetailsStatus.EcuName,
						OtherBindingStatus = this.BuildEcuCertCheckingStatus(otherBindingDetailsStatus.OtherBindingStatus),
						RollenName = otherBindingDetailsStatus.RollenName
					});
				}
			}
			if (list != null && list.Count<PsdzOtherBindingDetailsStatus>() > 0)
			{
				return list.ToArray();
			}
			return null;
		}

		private PsdzEcuCertCheckingStatus? BuildEcuCertCheckingStatus(EcuCertCheckingStatus? bindingStatus)
		{
			if (bindingStatus != null)
			{
				switch (bindingStatus.GetValueOrDefault())
				{
					case EcuCertCheckingStatus.CheckStillRunning:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.CheckStillRunning);
					case EcuCertCheckingStatus.Empty:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Empty);
					case EcuCertCheckingStatus.Incomplete:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Incomplete);
					case EcuCertCheckingStatus.Malformed:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Malformed);
					case EcuCertCheckingStatus.Ok:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Ok);
					case EcuCertCheckingStatus.Other:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Other);
					case EcuCertCheckingStatus.SecurityError:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.SecurityError);
					case EcuCertCheckingStatus.Unchecked:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Unchecked);
					case EcuCertCheckingStatus.WrongVin17:
						return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.WrongVin17);
				}
			}
			return new PsdzEcuCertCheckingStatus?(PsdzEcuCertCheckingStatus.Empty);
		}

		private PsdzBindingDetailsStatus[] BuildDetailStatus(IBindingDetailsStatus[] arrBindingDetailStatus)
		{
			List<PsdzBindingDetailsStatus> list = new List<PsdzBindingDetailsStatus>();
			if (arrBindingDetailStatus != null && arrBindingDetailStatus.Count<IBindingDetailsStatus>() > 0)
			{
				foreach (IBindingDetailsStatus bindingDetailsStatus in arrBindingDetailStatus)
				{
					list.Add(new PsdzBindingDetailsStatus
					{
						BindingStatus = this.BuildEcuCertCheckingStatus(bindingDetailsStatus.BindingStatus),
						CertificateStatus = this.BuildEcuCertCheckingStatus(bindingDetailsStatus.CertificateStatus),
						RollenName = bindingDetailsStatus.RollenName
					});
				}
			}
			if (list != null && list.Count<PsdzBindingDetailsStatus>() > 0)
			{
				return list.ToArray();
			}
			return null;
		}

		public IList<IPsdzFeatureSpecificFieldCto> BuildFeatureSpecificFieldsCto(IList<IFeatureSpecificField> featureSpecificFields)
		{
			List<IPsdzFeatureSpecificFieldCto> list = new List<IPsdzFeatureSpecificFieldCto>();
			foreach (IFeatureSpecificField featureSpecificField in featureSpecificFields)
			{
				list.Add(new PsdzFeatureSpecificFieldCto
				{
					FieldType = featureSpecificField.FieldType,
					FieldValue = featureSpecificField.FieldValue
				});
			}
			return list;
		}

		public IList<IPsdzValidityConditionCto> BuildValidityConditionsCto(IList<IValidityCondition> validityConditions)
		{
			List<IPsdzValidityConditionCto> list = new List<IPsdzValidityConditionCto>();
			foreach (IValidityCondition validityCondition in validityConditions)
			{
				list.Add(new PsdzValidityConditionCto
				{
					ConditionType = this.BuildConditionTypeEnum(validityCondition.ConditionType),
					ValidityValue = validityCondition.ValidityValue
				});
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
