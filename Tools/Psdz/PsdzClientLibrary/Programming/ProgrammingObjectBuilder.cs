using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Obd;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Programming;
using PsdzClient.Programming.BMW.Rheingold.Programming.API;
using PsdzClient.Utility;
using PsdzClientLibrary.Core;

namespace BMW.Rheingold.Programming.API
{
	public class ProgrammingObjectBuilder : IProgrammingObjectBuilder
	{
		public ProgrammingObjectBuilder(Vehicle vehicle, IFFMDynamicResolver ffmResolver)
		{
			this.vehicle = vehicle;
			this.ffmResolver = ffmResolver;
		}

		public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa Build(IPsdzStandardFa faInput)
		{
			if (faInput == null)
			{
				return null;
			}
			return new VehicleOrder
			{
				FaVersion = faInput.FaVersion,
				Entwicklungsbaureihe = faInput.Entwicklungsbaureihe,
				Lackcode = faInput.Lackcode,
				Polstercode = faInput.Polstercode,
				Type = faInput.Type,
				Zeitkriterium = faInput.Zeitkriterium,
				EWords = ((faInput.EWords != null) ? new List<string>(faInput.EWords) : null),
				HOWords = ((faInput.HOWords != null) ? new List<string>(faInput.HOWords) : null),
				Salapas = ((faInput.Salapas != null) ? new List<string>(faInput.Salapas) : null)
			};
		}

        public IVehicleProfile Build(IPsdzFp fp)
        {
            VehicleProfile obj = (VehicleProfile)Build((IPsdzStandardFp)fp);
            obj.Baureihenverbund = fp.Baureihenverbund;
            obj.Entwicklungsbaureihe = fp.Entwicklungsbaureihe;
            return obj;
        }

        public IVehicleProfile Build(IPsdzStandardFp standardFp)
        {
            if (standardFp == null)
            {
                return null;
            }
            if (!standardFp.IsValid)
            {
                Log.Warning("ProgrammingObjectBuilder.Build()", "Vehicle profile 'standardFp' is not valid!");
                return null;
            }
            IDictionary<int, IEnumerable<IVehicleProfileCriterion>> dictionary = new Dictionary<int, IEnumerable<IVehicleProfileCriterion>>();
            foreach (int key in standardFp.Category2Criteria.Keys)
            {
                IEnumerable<IVehicleProfileCriterion> value = standardFp.Category2Criteria[key].Select(Build);
                dictionary.Add(key, value);
            }
            return new VehicleProfile(standardFp.AsString, standardFp.CategoryId2CategoryName, dictionary);
        }

        public IVehicleProfileCriterion Build(IPsdzStandardFpCriterion criterion)
        {
            if (criterion == null)
            {
                return null;
            }
            return new VehicleProfileCriterion(criterion.Value, criterion.Name, criterion.NameEn);
        }

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa Build(BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa faInput)
        {
            if (faInput == null)
            {
                return null;
            }
            BMW.Rheingold.Programming.API.VehicleOrder vehicleOrder = new BMW.Rheingold.Programming.API.VehicleOrder();
            if (!string.IsNullOrWhiteSpace(faInput.VERSION) && Regex.IsMatch(faInput.VERSION.Trim(), "^\\d+$"))
            {
                vehicleOrder.FaVersion = int.Parse(faInput.VERSION);
            }
            vehicleOrder.Entwicklungsbaureihe = FormatConverter.ConvertToBn2020ConformModelSeries(faInput.BR);
            vehicleOrder.Lackcode = faInput.LACK;
            vehicleOrder.Polstercode = faInput.POLSTER;
            vehicleOrder.Type = faInput.TYPE;
            vehicleOrder.Zeitkriterium = faInput.C_DATE;
            vehicleOrder.EWords = ((faInput.E_WORT != null) ? new List<string>(faInput.E_WORT) : null);
            vehicleOrder.HOWords = ((faInput.HO_WORT != null) ? new List<string>(faInput.HO_WORT) : null);
            vehicleOrder.Salapas = ((faInput.SA != null) ? new List<string>(faInput.SA) : null);
            return vehicleOrder;
        }

        public IStandardSvk Build(IPsdzStandardSvk svkInput)
        {
            if (svkInput == null)
            {
                return null;
            }
            SystemVerbauKennung systemVerbauKennung = new SystemVerbauKennung();
            systemVerbauKennung.SvkVersion = svkInput.SvkVersion;
            systemVerbauKennung.ProgDepChecked = svkInput.ProgDepChecked;
            IEnumerable<IPsdzSgbmId> sgbmIds = svkInput.SgbmIds;
            if (sgbmIds != null)
            {
                List<ISgbmId> list = new List<ISgbmId>();
                foreach (IPsdzSgbmId item in sgbmIds)
                {
                    SgbmIdentifier sgbmIdentifier = new SgbmIdentifier();
                    sgbmIdentifier.ProcessClass = item.ProcessClass;
                    sgbmIdentifier.Id = item.IdAsLong;
                    sgbmIdentifier.MainVersion = item.MainVersion;
                    sgbmIdentifier.SubVersion = item.SubVersion;
                    sgbmIdentifier.PatchVersion = item.PatchVersion;
                    list.Add(sgbmIdentifier);
                }
                list.Sort();
                systemVerbauKennung.SgbmIds = list;
            }
            return systemVerbauKennung;
        }

        public ISvt Build(IPsdzStandardSvt svtInput)
        {
            if (svtInput == null)
            {
                return null;
            }
            SystemVerbauTabelle systemVerbauTabelle = new SystemVerbauTabelle();
            systemVerbauTabelle.Version = svtInput.Version;
            systemVerbauTabelle.HoSignature = svtInput.HoSignature;
            systemVerbauTabelle.HoSignatureDate = svtInput.HoSignatureDate;
            IEnumerable<IPsdzEcu> ecus = svtInput.Ecus;
            if (ecus != null)
            {
                foreach (IPsdzEcu item in ecus)
                {
                    IEcuObj ecu = Build(item);
                    systemVerbauTabelle.AddEcu(ecu);
                }
                return systemVerbauTabelle;
            }
            return systemVerbauTabelle;
        }

        public void FillOrderNumbers(IPsdzSollverbauung sollVerbauung, IDictionary<string, string> result)
        {
            IPsdzSvt svt = sollVerbauung.Svt;
            IPsdzOrderList psdzOrderList = sollVerbauung.PsdzOrderList;
            IEnumerable<IPsdzEcu> ecus = svt.Ecus;
            if (ecus == null)
            {
                return;
            }
            foreach (IPsdzEcu item in ecus)
            {
                IEcuObj ecuObj = Build(item);
                if (psdzOrderList == null || psdzOrderList.BntnVariantInstances == null)
                {
                    continue;
                }
                IPsdzEcuVariantInstance[] bntnVariantInstances = psdzOrderList.BntnVariantInstances;
                foreach (IPsdzEcuVariantInstance psdzEcuVariantInstance in bntnVariantInstances)
                {
                    if (psdzEcuVariantInstance.Ecu?.BaseVariant == ecuObj.EcuIdentifier?.BaseVariant && psdzEcuVariantInstance.Ecu?.PrimaryKey?.DiagAddrAsInt == ecuObj.EcuIdentifier?.DiagAddrAsInt)
                    {
                        result.Add(BuildKey(psdzEcuVariantInstance.Ecu), psdzEcuVariantInstance.OrderablePart?.SachNrTais);
                        break;
                    }
                }
            }
        }

        internal string BuildKey(IPsdzEcu ecu)
        {
            if (ecu != null && ecu.BaseVariant != null && ecu.PrimaryKey != null)
            {
                return ecu.BaseVariant + "-" + ecu.PrimaryKey.DiagAddrAsInt;
            }
            return null;
        }

        public ISvt Build(IPsdzSollverbauung sollVerbauung, IDictionary<string, string> orderNumbers)
        {
            if (sollVerbauung != null && sollVerbauung.Svt != null)
            {
                IPsdzSvt svt = sollVerbauung.Svt;
                IPsdzOrderList psdzOrderList = sollVerbauung.PsdzOrderList;
                SystemVerbauTabelle systemVerbauTabelle = new SystemVerbauTabelle();
                systemVerbauTabelle.Version = svt.Version;
                systemVerbauTabelle.HoSignature = svt.HoSignature;
                systemVerbauTabelle.HoSignatureDate = svt.HoSignatureDate;
                IEnumerable<IPsdzEcu> ecus = svt.Ecus;
                if (ecus != null)
                {
                    foreach (IPsdzEcu item in ecus)
                    {
                        IEcuObj ecuObj = Build(item);
                        if (orderNumbers != null && orderNumbers.Any())
                        {
                            string key = BuildKey(item);
                            if (orderNumbers.ContainsKey(key))
                            {
                                ((EcuObj)ecuObj).OrderNumber = orderNumbers[key];
                            }
                        }
                        systemVerbauTabelle.AddEcu(ecuObj);
                    }
                    return systemVerbauTabelle;
                }
                return systemVerbauTabelle;
            }
            return null;
        }

        public IEcuObj Build(IPsdzEcu ecuInput)
        {
            if (ecuInput == null)
            {
                return null;
            }
            EcuObj ecuObj = new EcuObj();
            ecuObj.BaseVariant = ecuInput.BaseVariant;
            ecuObj.EcuVariant = ecuInput.EcuVariant;
            ecuObj.BnTnName = ecuInput.BnTnName;
            ecuObj.GatewayDiagAddrAsInt = ((ecuInput.GatewayDiagAddr != null) ? new int?(ecuInput.GatewayDiagAddr.Offset) : null);
            ecuObj.DiagnosticBus = busEnumMapper.GetValue(ecuInput.DiagnosticBus);
            ecuObj.SerialNumber = ecuInput.SerialNumber;
            ecuObj.EcuIdentifier = Build(ecuInput.PrimaryKey);
            ecuObj.StandardSvk = Build(ecuInput.StandardSvk);
            ecuObj.BusConnections = ((ecuInput.BusConnections != null) ? ecuInput.BusConnections.Select(busEnumMapper.GetValue).ToList() : null);
            ecuObj.EcuDetailInfo = ((ecuInput.EcuDetailInfo != null) ? new EcuObjDetailInfo(ecuInput.EcuDetailInfo.ByteValue) : null);
            ecuObj.EcuStatusInfo = ((ecuInput.EcuStatusInfo != null) ? new EcuObjStatusInfo(ecuInput.EcuStatusInfo.ByteValue, ecuInput.EcuStatusInfo.HasIndividualData) : null);
            ecuObj.EcuPdxInfo = Build(ecuInput.PsdzEcuPdxInfo);
            PsdzDatabase database = ClientContext.GetDatabase(this.vehicle);
            if (database != null)
            {
                string bnTnName = ecuInput.BnTnName;
                IEcuIdentifier ecuIdentifier = ecuObj.EcuIdentifier;
                PsdzDatabase.EcuVar ecuVar = database.FindEcuVariantFromBntn(bnTnName, (ecuIdentifier != null) ? new int?(ecuIdentifier.DiagAddrAsInt) : null, this.vehicle, this.ffmResolver);
                if (ecuVar != null && !string.IsNullOrEmpty(ecuVar.Name))
                {
                    //ecuObj.XepEcuVariant = xep_ECUVARIANTS;
                    ecuObj.EcuVariant = ecuVar.Name.ToUpper(CultureInfo.InvariantCulture);
                    PsdzDatabase.EcuClique ecuClique = database.FindEcuClique(ecuVar);
                    //ecuObj.XepEcuClique = ecuClique;
                    PsdzDatabase.EcuGroup ecuGroup = database.FindEcuGroup(ecuVar, this.vehicle, this.ffmResolver);
                    if (ecuGroup != null)
                    {
                        ecuObj.EcuGroup = ecuGroup.Name.ToUpper(CultureInfo.InvariantCulture);
                    }
                    PsdzDatabase.EcuReps ecuReps = database.FindEcuRep(ecuClique);
                    if (ecuReps != null)
                    {
                        ecuObj.EcuRep = ecuReps.EcuShortcut;
                    }
                }
            }

            return ecuObj;
        }

        private IEcuPdxInfo Build(IPsdzEcuPdxInfo psdzEcuPdxInfo)
        {
            if (psdzEcuPdxInfo != null)
            {
                return new EcuObjPdxInfo
                {
                    CertVersion = psdzEcuPdxInfo.CertVersion,
                    IsCert2018 = psdzEcuPdxInfo.IsCert2018,
                    IsCert2021 = psdzEcuPdxInfo.IsCert2021,
                    IsCertEnabled = psdzEcuPdxInfo.IsCertEnabled,
                    IsSecOcEnabled = psdzEcuPdxInfo.IsSecOcEnabled,
                    IsSfaEnabled = psdzEcuPdxInfo.IsSfaEnabled,
                    IsIPSecEnabled = psdzEcuPdxInfo.IsIPSecEnabled,
                    IsLcsServicePackSupported = psdzEcuPdxInfo.IsLcsServicePackSupported,
                    IsLcsSystemTimeSwitchSupported = psdzEcuPdxInfo.IsLcsSystemTimeSwitchSupported,
                    //IsMirrorProtocolSupported = psdzEcuPdxInfo.IsMirrorProtocolSupported,
                    //IsEcuAuthEnabled = psdzEcuPdxInfo.IsEcuAuthEnabled
                };
            }
            return null;
        }

        public ECU Build(IEcuObj ecuInput)
        {
            ECU eCU = null;
            if (ecuInput is EcuObj ecuObj)
            {
                eCU = new ECU();
                eCU.ID_SG_ADR = ecuObj.EcuIdentifier.DiagAddrAsInt;
                eCU.TITLE_ECUTREE = ecuObj.EcuRep;
                eCU.ECU_SGBD = ecuObj.EcuVariant;
                eCU.VARIANTE = ecuObj.EcuVariant;
                eCU.ECU_GRUPPE = ecuObj.EcuGroup;
                eCU.ECU_GROBNAME = ecuObj.BaseVariant;
                //eCU.XepEcuClique = ecuObj.XepEcuClique;
                //eCU.ECUTitle = ((ecuObj.XepEcuClique != null) ? ecuObj.XepEcuClique.Title : string.Empty);
                //eCU.XepEcuVariant = ecuObj.XepEcuVariant;
                eCU.ProgrammingVariantName = ecuObj.BnTnName;
                eCU.StatusInfo = ecuObj.EcuStatusInfo;
#if false
				if (eCU.XepEcuVariant == null)
                {
                    vdc.FillEcuNames(eCU, vehicle, ffmResolver);
                }
#endif
            }
            return eCU;
        }

        public IEcuIdentifier Build(IPsdzEcuIdentifier ecuIdentifierInput)
        {
            if (ecuIdentifierInput == null)
            {
                return null;
            }
            return BuildEcuIdentifier(ecuIdentifierInput.BaseVariant, ecuIdentifierInput.DiagAddrAsInt);
        }

        public ISwt Build(IPsdzSwtAction swtAction)
        {
            if (swtAction == null)
            {
                return null;
            }
            Swt swt = new Swt();
            foreach (IPsdzSwtEcu swtEcu2 in swtAction.SwtEcus)
            {
                ISwtEcu swtEcu = Build(swtEcu2);
                swt.AddEcu(swtEcu);
            }
            return swt;
        }

        public ISwtApplicationId Build(IPsdzSwtApplicationId swtApplicationId)
        {
            if (swtApplicationId == null)
            {
                return null;
            }
            return BuildSwtApplicationId(swtApplicationId.ApplicationNumber, swtApplicationId.UpgradeIndex);
        }

        public IAsamJobInputDictionary BuildAsamJobParamDictionary()
        {
            return new AsamJobInputDictionary();
        }

        public IEcuIdentifier BuildEcuIdentifier(string baseVariant, int diagAddrAsInt)
        {
            return new EcuId(baseVariant, diagAddrAsInt);
        }

        public ISwtApplicationId BuildSwtApplicationId(int appNo, int upgradeIdx)
        {
            return new SwtApplicationIdObj(appNo, upgradeIdx);
        }

        public IFetchEcuCertCheckingResult Build(PsdzFetchEcuCertCheckingResult psdzFetchEcuCertCheckingResult)
        {
            if (psdzFetchEcuCertCheckingResult == null)
            {
                return null;
            }
            return BuildFetchEcuCertCheckingResult(psdzFetchEcuCertCheckingResult);
        }

        private IFetchEcuCertCheckingResult BuildFetchEcuCertCheckingResult(PsdzFetchEcuCertCheckingResult psdzFetchEcuCertCheckingResult)
        {
            if (psdzFetchEcuCertCheckingResult == null)
            {
                return null;
            }
            return new FetchEcuCertCheckingResult
            {
                FailedEcus = BuildEcuCertCheckingResultFailedEcus(psdzFetchEcuCertCheckingResult.FailedEcus),
                Results = BuildEcuCertCheckingResults(psdzFetchEcuCertCheckingResult.Results)
            };
        }

        private IEnumerable<IEcuCertCheckingResponse> BuildEcuCertCheckingResults(IEnumerable<PsdzEcuCertCheckingResponse> results)
        {
            List<EcuCertCheckingResponse> list = new List<EcuCertCheckingResponse>();
            if (results != null && results.Count() > 0)
            {
                foreach (PsdzEcuCertCheckingResponse result in results)
                {
                    list.Add(new EcuCertCheckingResponse
                    {
                        BindingDetailStatus = BuildDetailStatus(result.BindingDetailStatus),
                        BindingsStatus = BuildEcuCertCheckingStatus(result.BindingsStatus),
                        CertificateStatus = BuildEcuCertCheckingStatus(result.CertificateStatus),
                        Ecu = BuildEcuIdentifier(result.Ecu.BaseVariant, result.Ecu.DiagAddrAsInt),
                        OtherBindingDetailStatus = BuildOtherBindingDetailStatus(result.OtherBindingDetailStatus),
                        OtherBindingsStatus = BuildEcuCertCheckingStatus(result.OtherBindingsStatus)
                    });
                }
                return list;
            }
            return list;
        }

        private IOtherBindingDetailsStatus[] BuildOtherBindingDetailStatus(PsdzOtherBindingDetailsStatus[] arrPsdzOtherBindingDetailStatus)
        {
            List<OtherBindingDetailsStatus> list = new List<OtherBindingDetailsStatus>();
            if (arrPsdzOtherBindingDetailStatus != null && arrPsdzOtherBindingDetailStatus.Count() > 0)
            {
                foreach (PsdzOtherBindingDetailsStatus psdzOtherBindingDetailsStatus in arrPsdzOtherBindingDetailStatus)
                {
                    list.Add(new OtherBindingDetailsStatus
                    {
                        EcuName = psdzOtherBindingDetailsStatus.EcuName,
                        OtherBindingStatus = BuildEcuCertCheckingStatus(psdzOtherBindingDetailsStatus.OtherBindingStatus),
                        RollenName = psdzOtherBindingDetailsStatus.RollenName
                    });
                }
            }
            if (list != null && list.Count() > 0)
            {
                return list.ToArray();
            }
            return null;
        }

        private EcuCertCheckingStatus? BuildEcuCertCheckingStatus(PsdzEcuCertCheckingStatus? bindingsStatus)
        {
            switch (bindingsStatus)
            {
                case PsdzEcuCertCheckingStatus.CheckStillRunning:
                    return EcuCertCheckingStatus.CheckStillRunning;
                case PsdzEcuCertCheckingStatus.Empty:
                    return EcuCertCheckingStatus.Empty;
                case PsdzEcuCertCheckingStatus.Incomplete:
                    return EcuCertCheckingStatus.Incomplete;
                case PsdzEcuCertCheckingStatus.Malformed:
                    return EcuCertCheckingStatus.Malformed;
                case PsdzEcuCertCheckingStatus.Ok:
                    return EcuCertCheckingStatus.Ok;
                case PsdzEcuCertCheckingStatus.Other:
                    return EcuCertCheckingStatus.Other;
                case PsdzEcuCertCheckingStatus.SecurityError:
                    return EcuCertCheckingStatus.SecurityError;
                case PsdzEcuCertCheckingStatus.Unchecked:
                    return EcuCertCheckingStatus.Unchecked;
                case PsdzEcuCertCheckingStatus.WrongVin17:
                    return EcuCertCheckingStatus.WrongVin17;
                default:
                    return EcuCertCheckingStatus.Empty;
            }
        }

        private IBindingDetailsStatus[] BuildDetailStatus(PsdzBindingDetailsStatus[] arrPsdzBindingDetailStatus)
        {
            List<BindingDetailsStatus> list = new List<BindingDetailsStatus>();
            if (arrPsdzBindingDetailStatus != null && arrPsdzBindingDetailStatus.Count() > 0)
            {
                foreach (PsdzBindingDetailsStatus psdzBindingDetailsStatus in arrPsdzBindingDetailStatus)
                {
                    list.Add(new BindingDetailsStatus
                    {
                        BindingStatus = BuildEcuCertCheckingStatus(psdzBindingDetailsStatus.BindingStatus),
                        CertificateStatus = BuildEcuCertCheckingStatus(psdzBindingDetailsStatus.CertificateStatus),
                        RollenName = psdzBindingDetailsStatus.RollenName
                    });
                }
            }
            if (list != null && list.Count() > 0)
            {
                return list.ToArray();
            }
            return null;
        }

        private IEnumerable<IEcuFailureResponse> BuildEcuCertCheckingResultFailedEcus(IEnumerable<PsdzEcuFailureResponse> psdzFailedEcus)
        {
            List<EcuFailureResponse> list = new List<EcuFailureResponse>();
            if (psdzFailedEcus != null && psdzFailedEcus.Count() > 0)
            {
                foreach (PsdzEcuFailureResponse psdzFailedEcu in psdzFailedEcus)
                {
                    list.Add(new EcuFailureResponse
                    {
                        Ecu = BuildEcuIdentifier(psdzFailedEcu.Ecu.BaseVariant, psdzFailedEcu.Ecu.DiagAddrAsInt),
                        Reason = psdzFailedEcu.Reason
                    });
                }
                return list;
            }
            return list;
        }

        internal IIstufenTriple Build(IPsdzIstufenTriple istufenTriple)
		{
			if (istufenTriple == null)
			{
				return null;
			}
			return new IntegrationLevelTriple(istufenTriple.Shipment, istufenTriple.Last, istufenTriple.Current);
		}

		internal SystemVerbauKennung Build(ISvk svkInput)
		{
			if (svkInput == null)
			{
				return null;
			}
			SystemVerbauKennung systemVerbauKennung = new SystemVerbauKennung();
			SgbmIdParser sgbmIdParser = new SgbmIdParser();
			IEnumerable<string> xwe_SGBMID = svkInput.XWE_SGBMID;
			if (xwe_SGBMID != null)
			{
				List<ISgbmId> list = new List<ISgbmId>();
				foreach (string sgbmId in xwe_SGBMID)
				{
					if (sgbmIdParser.ParseDec(sgbmId))
					{
						list.Add(new SgbmIdentifier
						{
							ProcessClass = sgbmIdParser.ProcessClass,
							Id = sgbmIdParser.Id,
							MainVersion = sgbmIdParser.MainVersion,
							SubVersion = sgbmIdParser.SubVersion,
							PatchVersion = sgbmIdParser.PatchVersion
						});
					}
				}
				list.Sort();
				systemVerbauKennung.SgbmIds = list;
			}
			return systemVerbauKennung;
		}

		internal IDictionary<IEcuIdentifier, IObdData> BuildObdDataDictionary(IDictionary<IPsdzEcuIdentifier, IPsdzObdData> obdMap)
		{
			IDictionary<IEcuIdentifier, IObdData> dictionary = new Dictionary<IEcuIdentifier, IObdData>();
			foreach (KeyValuePair<IPsdzEcuIdentifier, IPsdzObdData> keyValuePair in obdMap)
			{
				IEcuIdentifier key = this.Build(keyValuePair.Key);
				IObdData value = this.Build(keyValuePair.Value);
				dictionary.Add(new KeyValuePair<IEcuIdentifier, IObdData>(key, value));
			}
			return dictionary;
		}

		private ISwtEcu Build(IPsdzSwtEcu swtEcuInput)
		{
			if (swtEcuInput == null)
			{
				return null;
			}
			SwtEcuObj swtEcuObj = new SwtEcuObj();
			swtEcuObj.EcuIdentifier = this.Build(swtEcuInput.EcuIdentifier);
			swtEcuObj.RootCertificateState = this.rootCertificateStateEnumMapper.GetValue(swtEcuInput.RootCertState);
			swtEcuObj.SoftwareSigState = this.softwareSigStateEnumMapper.GetValue(swtEcuInput.SoftwareSigState);
			foreach (IPsdzSwtApplication swtApplicationInput in swtEcuInput.SwtApplications)
			{
				ISwtApplication swtApplication = this.Build(swtApplicationInput);
				swtEcuObj.AddApplication(swtApplication);
			}
			return swtEcuObj;
		}

		private ISwtApplication Build(IPsdzSwtApplication swtApplicationInput)
		{
			if (swtApplicationInput == null)
			{
				return null;
			}
			IPsdzSwtApplicationId swtApplicationId = swtApplicationInput.SwtApplicationId;
			if (swtApplicationId == null)
			{
				return null;
			}
			return new SwtApplicationObj(this.Build(swtApplicationId))
			{
				Fsc = swtApplicationInput.Fsc,
				FscState = this.fscStateEnumMapper.GetValue(swtApplicationInput.FscState),
				FscCertificate = swtApplicationInput.FscCert,
				FscCertificateState = this.fscCertificateStateEnumMapper.GetValue(swtApplicationInput.FscCertState),
				SwtType = this.swtTypeEnumMapper.GetValue(swtApplicationInput.SwtType),
				SwtActionType = ((swtApplicationInput.SwtActionType != null) ? new SwtActionType?(this.swtActionTypeEnumMapper.GetValue(swtApplicationInput.SwtActionType.Value)) : null),
				IsBackupPossible = swtApplicationInput.IsBackupPossible,
				Position = swtApplicationInput.Position
			};
		}

		private IObdData Build(IPsdzObdData psdzObdData)
		{
			ObdData obdData = new ObdData();
			foreach (IPsdzObdTripleValue psdzObdTripleValue in psdzObdData.ObdTripleValues)
			{
				ObdTripleValue item = new ObdTripleValue(psdzObdTripleValue.CalId, psdzObdTripleValue.ObdId, psdzObdTripleValue.SubCVN);
				obdData.ObdTripleValues.Add(item);
			}
			return obdData;
		}

        public IFFMDynamicResolver IFFMDynamicResolver
        {
            get => ffmResolver;
            set => ffmResolver = value;
        }

        public Vehicle Vehicle
		{
            get => vehicle;
            set => vehicle = value;
        }

		private IFFMDynamicResolver ffmResolver;

		private Vehicle vehicle;

		private readonly BusEnumMapper busEnumMapper = new BusEnumMapper();

		private readonly RootCertificateStateEnumMapper rootCertificateStateEnumMapper = new RootCertificateStateEnumMapper();

		private readonly SoftwareSigStateEnumMapper softwareSigStateEnumMapper = new SoftwareSigStateEnumMapper();

		private readonly FscStateEnumMapper fscStateEnumMapper = new FscStateEnumMapper();

		private readonly FscCertificateStateEnumMapper fscCertificateStateEnumMapper = new FscCertificateStateEnumMapper();

		private readonly SwtTypeEnumMapper swtTypeEnumMapper = new SwtTypeEnumMapper();

		private readonly SwtActionTypeEnumMapper swtActionTypeEnumMapper = new SwtActionTypeEnumMapper();
	}
}
