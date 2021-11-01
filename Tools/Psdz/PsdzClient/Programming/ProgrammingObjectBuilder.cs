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

namespace BMW.Rheingold.Programming.API
{
	public class ProgrammingObjectBuilder : IProgrammingObjectBuilder
	{
		public ProgrammingObjectBuilder()
		{
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
			VehicleProfile vehicleProfile = (VehicleProfile)this.Build(fp);
			vehicleProfile.Baureihenverbund = fp.Baureihenverbund;
			vehicleProfile.Entwicklungsbaureihe = fp.Entwicklungsbaureihe;
			return vehicleProfile;
		}

		public IVehicleProfile Build(IPsdzStandardFp standardFp)
		{
			if (standardFp == null)
			{
				return null;
			}
			if (!standardFp.IsValid)
			{
				//Log.Warning("ProgrammingObjectBuilder.Build()", "Vehicle profile 'standardFp' is not valid!", Array.Empty<object>());
				return null;
			}
			IDictionary<int, IEnumerable<IVehicleProfileCriterion>> dictionary = new Dictionary<int, IEnumerable<IVehicleProfileCriterion>>();
			foreach (int key in standardFp.Category2Criteria.Keys)
			{
				IEnumerable<IVehicleProfileCriterion> value = standardFp.Category2Criteria[key].Select(new Func<IPsdzStandardFpCriterion, IVehicleProfileCriterion>(this.Build));
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
			VehicleOrder vehicleOrder = new VehicleOrder();
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
				foreach (IPsdzSgbmId psdzSgbmId in sgbmIds)
				{
					list.Add(new SgbmIdentifier
					{
						ProcessClass = psdzSgbmId.ProcessClass,
						Id = psdzSgbmId.IdAsLong,
						MainVersion = psdzSgbmId.MainVersion,
						SubVersion = psdzSgbmId.SubVersion,
						PatchVersion = psdzSgbmId.PatchVersion
					});
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
				foreach (IPsdzEcu ecuInput in ecus)
				{
					IEcuObj ecu = this.Build(ecuInput);
					systemVerbauTabelle.AddEcu(ecu);
				}
			}
			return systemVerbauTabelle;
		}

		public void FillOrderNumbers(IPsdzSollverbauung sollVerbauung, IDictionary<string, string> result)
		{
			IPsdzStandardSvt svt = sollVerbauung.Svt;
			IPsdzOrderList psdzOrderList = sollVerbauung.PsdzOrderList;
			IEnumerable<IPsdzEcu> ecus = svt.Ecus;
			if (ecus == null)
			{
				return;
			}
			foreach (IPsdzEcu ecuInput in ecus)
			{
				IEcuObj ecuObj = this.Build(ecuInput);
				if (psdzOrderList != null && psdzOrderList.BntnVariantInstances != null)
				{
					foreach (IPsdzEcuVariantInstance psdzEcuVariantInstance in psdzOrderList.BntnVariantInstances)
					{
						IPsdzEcu ecu = psdzEcuVariantInstance.Ecu;
						string a = (ecu != null) ? ecu.BaseVariant : null;
						IEcuIdentifier ecuIdentifier = ecuObj.EcuIdentifier;
						if (a == ((ecuIdentifier != null) ? ecuIdentifier.BaseVariant : null))
						{
							IPsdzEcu ecu2 = psdzEcuVariantInstance.Ecu;
							int? num;
							if (ecu2 == null)
							{
								num = null;
							}
							else
							{
								IPsdzEcuIdentifier primaryKey = ecu2.PrimaryKey;
								num = ((primaryKey != null) ? new int?(primaryKey.DiagAddrAsInt) : null);
							}
							int? num2 = num;
							IEcuIdentifier ecuIdentifier2 = ecuObj.EcuIdentifier;
							int? num3 = (ecuIdentifier2 != null) ? new int?(ecuIdentifier2.DiagAddrAsInt) : null;
							if (num2.GetValueOrDefault() == num3.GetValueOrDefault() & num2 != null == (num3 != null))
							{
								string key = this.BuildKey(psdzEcuVariantInstance.Ecu);
								IPsdzOrderPart orderablePart = psdzEcuVariantInstance.OrderablePart;
								result.Add(key, (orderablePart != null) ? orderablePart.SachNrTais : null);
								break;
							}
						}
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
					foreach (IPsdzEcu psdzEcu in ecus)
					{
						IEcuObj ecuObj = this.Build(psdzEcu);
						if (orderNumbers != null && orderNumbers.Any<KeyValuePair<string, string>>())
						{
							string key = this.BuildKey(psdzEcu);
							if (orderNumbers.ContainsKey(key))
							{
								((EcuObj)ecuObj).OrderNumber = orderNumbers[key];
							}
						}
						systemVerbauTabelle.AddEcu(ecuObj);
					}
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
			ecuObj.DiagnosticBus = this.busEnumMapper.GetValue(ecuInput.DiagnosticBus);
			ecuObj.SerialNumber = ecuInput.SerialNumber;
			ecuObj.EcuIdentifier = this.Build(ecuInput.PrimaryKey);
			ecuObj.StandardSvk = this.Build(ecuInput.StandardSvk);
			ecuObj.BusConnections = ((ecuInput.BusConnections != null) ? ecuInput.BusConnections.Select(new Func<PsdzBus, Bus>(this.busEnumMapper.GetValue)).ToList<Bus>() : null);
			ecuObj.EcuDetailInfo = ((ecuInput.EcuDetailInfo != null) ? new EcuObjDetailInfo(ecuInput.EcuDetailInfo.ByteValue) : null);
			ecuObj.EcuStatusInfo = ((ecuInput.EcuStatusInfo != null) ? new EcuObjStatusInfo(ecuInput.EcuStatusInfo.ByteValue, ecuInput.EcuStatusInfo.HasIndividualData) : null);
			ecuObj.EcuPdxInfo = this.Build(ecuInput.PsdzEcuPdxInfo);

            PdszDatabase database = ClientContext.Database;
            if (database != null)
            {
                string bnTnName = ecuInput.BnTnName;
                IEcuIdentifier ecuIdentifier = ecuObj.EcuIdentifier;
                PdszDatabase.EcuVar ecuVar = database.FindEcuVariantFromBntn(bnTnName, (ecuIdentifier != null) ? new int?(ecuIdentifier.DiagAddrAsInt) : null, this.vehicle, this.ffmResolver);
                if (ecuVar != null && !string.IsNullOrEmpty(ecuVar.Name))
                {
                    //ecuObj.XepEcuVariant = xep_ECUVARIANTS;
                    ecuObj.EcuVariant = ecuVar.Name.ToUpper(CultureInfo.InvariantCulture);
					//ecuObj.XepEcuClique = this.vdc.FindEcuClique(xep_ECUVARIANTS);
                    PdszDatabase.EcuGroup ecuGroup = database.FindEcuGroup(ecuVar, this.vehicle, this.ffmResolver);
                    if (ecuGroup != null)
                    {
                        ecuObj.EcuGroup = ecuGroup.Name.ToUpper(CultureInfo.InvariantCulture);
                    }
#if false
					XEP_ECUREPS xep_ECUREPS = database.FindEcuRep(ecuObj.XepEcuClique);
                    if (xep_ECUREPS != null)
                    {
                        ecuObj.EcuRep = xep_ECUREPS.SteuergeraeteKuerzel;
                    }
#endif
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
					IsLcsSystemTimeSwitchSupported = psdzEcuPdxInfo.IsLcsSystemTimeSwitchSupported
				};
			}
			return null;
		}

        public ECU Build(IEcuObj ecuInput)
        {
            ECU ecu = null;
            EcuObj ecuObj = ecuInput as EcuObj;
            if (ecuObj != null)
            {
                ecu = new ECU();
                ecu.ID_SG_ADR = (long)ecuObj.EcuIdentifier.DiagAddrAsInt;
                ecu.TITLE_ECUTREE = ecuObj.EcuRep;
                ecu.ECU_SGBD = ecuObj.EcuVariant;
                ecu.VARIANTE = ecuObj.EcuVariant;
                ecu.ECU_GRUPPE = ecuObj.EcuGroup;
                ecu.ECU_GROBNAME = ecuObj.BaseVariant;
                //ecu.XepEcuClique = ecuObj.XepEcuClique;
                //ecu.ECUTitle = ((ecuObj.XepEcuClique != null) ? ecuObj.XepEcuClique.Title : string.Empty);
                //ecu.XepEcuVariant = ecuObj.XepEcuVariant;
                ecu.ProgrammingVariantName = ecuObj.BnTnName;
                ecu.StatusInfo = ecuObj.EcuStatusInfo;
            }
#if false
            if (ecu.XepEcuVariant == null)
            {
                this.vdc.FillEcuNames(ecu, this.vehicle, this.ffmResolver);
            }
#endif
            return ecu;
        }

		public IEcuIdentifier Build(IPsdzEcuIdentifier ecuIdentifierInput)
		{
			if (ecuIdentifierInput == null)
			{
				return null;
			}
			return this.BuildEcuIdentifier(ecuIdentifierInput.BaseVariant, ecuIdentifierInput.DiagAddrAsInt);
		}

		public ISwt Build(IPsdzSwtAction swtAction)
		{
			if (swtAction == null)
			{
				return null;
			}
			Swt swt = new Swt();
			foreach (IPsdzSwtEcu swtEcuInput in swtAction.SwtEcus)
			{
				ISwtEcu swtEcu = this.Build(swtEcuInput);
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
			return this.BuildSwtApplicationId(swtApplicationId.ApplicationNumber, swtApplicationId.UpgradeIndex);
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
			return this.BuildFetchEcuCertCheckingResult(psdzFetchEcuCertCheckingResult);
		}

		private IFetchEcuCertCheckingResult BuildFetchEcuCertCheckingResult(PsdzFetchEcuCertCheckingResult psdzFetchEcuCertCheckingResult)
		{
			if (psdzFetchEcuCertCheckingResult == null)
			{
				return null;
			}
			return new FetchEcuCertCheckingResult
			{
				FailedEcus = this.BuildEcuCertCheckingResultFailedEcus(psdzFetchEcuCertCheckingResult.FailedEcus),
				Results = this.BuildEcuCertCheckingResults(psdzFetchEcuCertCheckingResult.Results)
			};
		}

		private IEnumerable<IEcuCertCheckingResponse> BuildEcuCertCheckingResults(IEnumerable<PsdzEcuCertCheckingResponse> results)
		{
			List<EcuCertCheckingResponse> list = new List<EcuCertCheckingResponse>();
			if (results != null && results.Count<PsdzEcuCertCheckingResponse>() > 0)
			{
				foreach (PsdzEcuCertCheckingResponse psdzEcuCertCheckingResponse in results)
				{
					list.Add(new EcuCertCheckingResponse
					{
						BindingDetailStatus = this.BuildDetailStatus(psdzEcuCertCheckingResponse.BindingDetailStatus),
						BindingsStatus = this.BuildEcuCertCheckingStatus(psdzEcuCertCheckingResponse.BindingsStatus),
						CertificateStatus = this.BuildEcuCertCheckingStatus(psdzEcuCertCheckingResponse.CertificateStatus),
						Ecu = this.BuildEcuIdentifier(psdzEcuCertCheckingResponse.Ecu.BaseVariant, psdzEcuCertCheckingResponse.Ecu.DiagAddrAsInt),
						OtherBindingDetailStatus = this.BuildOtherBindingDetailStatus(psdzEcuCertCheckingResponse.OtherBindingDetailStatus),
						OtherBindingsStatus = this.BuildEcuCertCheckingStatus(psdzEcuCertCheckingResponse.OtherBindingsStatus)
					});
				}
			}
			return list;
		}

		private IOtherBindingDetailsStatus[] BuildOtherBindingDetailStatus(PsdzOtherBindingDetailsStatus[] arrPsdzOtherBindingDetailStatus)
		{
			List<OtherBindingDetailsStatus> list = new List<OtherBindingDetailsStatus>();
			if (arrPsdzOtherBindingDetailStatus != null && arrPsdzOtherBindingDetailStatus.Count<PsdzOtherBindingDetailsStatus>() > 0)
			{
				foreach (PsdzOtherBindingDetailsStatus psdzOtherBindingDetailsStatus in arrPsdzOtherBindingDetailStatus)
				{
					list.Add(new OtherBindingDetailsStatus
					{
						EcuName = psdzOtherBindingDetailsStatus.EcuName,
						OtherBindingStatus = this.BuildEcuCertCheckingStatus(psdzOtherBindingDetailsStatus.OtherBindingStatus),
						RollenName = psdzOtherBindingDetailsStatus.RollenName
					});
				}
			}
			if (list != null && list.Count<OtherBindingDetailsStatus>() > 0)
			{
				return list.ToArray();
			}
			return null;
		}

		private EcuCertCheckingStatus? BuildEcuCertCheckingStatus(PsdzEcuCertCheckingStatus? bindingsStatus)
		{
			if (bindingsStatus != null)
			{
				switch (bindingsStatus.GetValueOrDefault())
				{
					case PsdzEcuCertCheckingStatus.CheckStillRunning:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.CheckStillRunning);
					case PsdzEcuCertCheckingStatus.Empty:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Empty);
					case PsdzEcuCertCheckingStatus.Incomplete:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Incomplete);
					case PsdzEcuCertCheckingStatus.Malformed:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Malformed);
					case PsdzEcuCertCheckingStatus.Ok:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Ok);
					case PsdzEcuCertCheckingStatus.Other:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Other);
					case PsdzEcuCertCheckingStatus.SecurityError:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.SecurityError);
					case PsdzEcuCertCheckingStatus.Unchecked:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Unchecked);
					case PsdzEcuCertCheckingStatus.WrongVin17:
						return new EcuCertCheckingStatus?(EcuCertCheckingStatus.WrongVin17);
				}
			}
			return new EcuCertCheckingStatus?(EcuCertCheckingStatus.Empty);
		}

		private IBindingDetailsStatus[] BuildDetailStatus(PsdzBindingDetailsStatus[] arrPsdzBindingDetailStatus)
		{
			List<BindingDetailsStatus> list = new List<BindingDetailsStatus>();
			if (arrPsdzBindingDetailStatus != null && arrPsdzBindingDetailStatus.Count<PsdzBindingDetailsStatus>() > 0)
			{
				foreach (PsdzBindingDetailsStatus psdzBindingDetailsStatus in arrPsdzBindingDetailStatus)
				{
					list.Add(new BindingDetailsStatus
					{
						BindingStatus = this.BuildEcuCertCheckingStatus(psdzBindingDetailsStatus.BindingStatus),
						CertificateStatus = this.BuildEcuCertCheckingStatus(psdzBindingDetailsStatus.CertificateStatus),
						RollenName = psdzBindingDetailsStatus.RollenName
					});
				}
			}
			if (list != null && list.Count<BindingDetailsStatus>() > 0)
			{
				return list.ToArray();
			}
			return null;
		}

		private IEnumerable<IEcuFailureResponse> BuildEcuCertCheckingResultFailedEcus(IEnumerable<PsdzEcuFailureResponse> psdzFailedEcus)
		{
			List<EcuFailureResponse> list = new List<EcuFailureResponse>();
			if (psdzFailedEcus != null && psdzFailedEcus.Count<PsdzEcuFailureResponse>() > 0)
			{
				foreach (PsdzEcuFailureResponse psdzEcuFailureResponse in psdzFailedEcus)
				{
					list.Add(new EcuFailureResponse
					{
						Ecu = this.BuildEcuIdentifier(psdzEcuFailureResponse.Ecu.BaseVariant, psdzEcuFailureResponse.Ecu.DiagAddrAsInt),
						Reason = psdzEcuFailureResponse.Reason
					});
				}
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
