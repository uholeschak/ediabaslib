using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Obd;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Programming;
using PsdzClient.Programming.BMW.Rheingold.Programming.API;
using PsdzClient.Utility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable CS0169
namespace BMW.Rheingold.Programming.API
{
    // ToDo: Check on update
    [PreserveSource(Hint = "changed to public", AccessModified = true)]
    public class ProgrammingObjectBuilder : IProgrammingObjectBuilder
    {
        private readonly IFFMDynamicResolver ffmResolver;

        private readonly Vehicle vehicle;

        [PreserveSource(Hint = "VehicleDataConverter", Placeholder = true)]
        private readonly PlaceholderType vdc;

        private readonly RootCertificateStateEnumMapper rootCertificateStateEnumMapper = new RootCertificateStateEnumMapper();

        private readonly SoftwareSigStateEnumMapper softwareSigStateEnumMapper = new SoftwareSigStateEnumMapper();

        private readonly FscStateEnumMapper fscStateEnumMapper = new FscStateEnumMapper();

        private readonly FscCertificateStateEnumMapper fscCertificateStateEnumMapper = new FscCertificateStateEnumMapper();

        private readonly SwtTypeEnumMapper swtTypeEnumMapper = new SwtTypeEnumMapper();

        private readonly SwtActionTypeEnumMapper swtActionTypeEnumMapper = new SwtActionTypeEnumMapper();

        public ProgrammingObjectBuilder(Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            this.vehicle = vehicle;
            this.ffmResolver = ffmResolver;
            //vdc = new VehicleDataConverter(db);
        }

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa Build(IPsdzStandardFa faInput)
        {
            if (faInput == null)
            {
                return null;
            }
            return new BMW.Rheingold.Programming.API.VehicleOrder
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

        public FA Build(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa faInput)
        {
            if (faInput == null)
            {
                return null;
            }
            FA fA = new FA();
            fA.VERSION = faInput.FaVersion.ToString(CultureInfo.InvariantCulture);
            fA.BR = faInput.Entwicklungsbaureihe;
            fA.LACK = faInput.Lackcode;
            fA.POLSTER = faInput.Polstercode;
            fA.TYPE = faInput.Type;
            fA.C_DATE = faInput.Zeitkriterium;
            if (faInput.EWords == null)
            {
                fA.E_WORT_ANZ = 0;
                fA.E_WORT = new ObservableCollection<string>();
            }
            else
            {
                fA.E_WORT_ANZ = (short)faInput.EWords.Count;
                fA.E_WORT = new ObservableCollection<string>(faInput.EWords);
            }
            if (faInput.HOWords == null)
            {
                fA.HO_WORT_ANZ = 0;
                fA.HO_WORT = new ObservableCollection<string>();
            }
            else
            {
                fA.HO_WORT_ANZ = (short)faInput.HOWords.Count;
                fA.HO_WORT = new ObservableCollection<string>(faInput.HOWords);
            }
            if (faInput.Salapas == null)
            {
                fA.SA_ANZ = 0;
                fA.SA = new ObservableCollection<string>();
            }
            else
            {
                fA.SA_ANZ = (short)faInput.Salapas.Count;
                fA.SA = new ObservableCollection<string>(faInput.Salapas);
            }
            fA.STANDARD_FA = faInput.ToString();
            return fA;
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

        // [UH] namespace modified
        public ISvt Build(Psdz.IPsdzStandardSvt svtInput)
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
            }
            return systemVerbauTabelle;
        }

        // [UH] namespace modified
        public IVehicleProfileChecksum Build(Psdz.IPsdzReadVpcFromVcmCto vpcInput)
        {
            if (vpcInput == null)
            {
                return null;
            }
            return new VehicleProfileChecksum
            {
                VpcVersion = vpcInput.VpcVersion,
                VpcCrc = vpcInput.VpcCrc
            };
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
            if (ecu == null || ecu.BaseVariant == null || ecu.PrimaryKey == null)
            {
                return null;
            }
            return ecu.BaseVariant + "-" + ecu.PrimaryKey.DiagAddrAsInt;
        }

        public ISvt Build(IPsdzSollverbauung sollVerbauung, IDictionary<string, string> orderNumbers)
        {
            if (sollVerbauung == null || sollVerbauung.Svt == null)
            {
                return null;
            }
            IPsdzSvt svt = sollVerbauung.Svt;
            _ = sollVerbauung.PsdzOrderList;
            SystemVerbauTabelle systemVerbauTabelle = new SystemVerbauTabelle();
            systemVerbauTabelle.Version = svt.Version;
            systemVerbauTabelle.HoSignature = svt.HoSignature;
            systemVerbauTabelle.HoSignatureDate = svt.HoSignatureDate;
            IEnumerable<IPsdzEcu> ecus = svt.Ecus;
            if (ecus != null)
            {
                foreach (IPsdzEcu item in ecus)
                {
                    ECU eCU = Build(item);
                    if (orderNumbers != null && orderNumbers.Any())
                    {
                        string key = BuildKey(item);
                        if (orderNumbers.ContainsKey(key))
                        {
                            eCU.OrderNumber = orderNumbers[key];
                        }
                    }
                    systemVerbauTabelle.AddEcu(eCU);
                }
            }
            return systemVerbauTabelle;
        }

        public ECU Build(IPsdzEcu ecuInput)
        {
            if (ecuInput == null)
            {
                return null;
            }
            ECU eCU = new ECU();
            eCU.BaseVariant = ecuInput.BaseVariant;
            eCU.EcuVariant = ecuInput.EcuVariant;
            eCU.BnTnName = ecuInput.BnTnName;
            eCU.GatewayDiagAddrAsInt = ((ecuInput.GatewayDiagAddr != null) ? new int?(ecuInput.GatewayDiagAddr.Offset) : ((int?)null));
            eCU.DiagBus = BusMapper.MapToBus(ecuInput.DiagnosticBus);
            eCU.SerialNumber = ecuInput.SerialNumber;
            eCU.EcuIdentifier = Build(ecuInput.PrimaryKey);
            eCU.StandardSvk = Build(ecuInput.StandardSvk);
            eCU.BusCons = ((ecuInput.BusConnections != null) ? ecuInput.BusConnections.Select(BusMapper.MapToBus).ToList() : null);
            eCU.EcuDetailInfo = ((ecuInput.EcuDetailInfo != null) ? new EcuObjDetailInfo(ecuInput.EcuDetailInfo.ByteValue) : null);
            eCU.EcuStatusInfo = ((ecuInput.EcuStatusInfo != null) ? new EcuObjStatusInfo(ecuInput.EcuStatusInfo.ByteValue, ecuInput.EcuStatusInfo.HasIndividualData) : null);
            eCU.EcuPdxInfo = Build(ecuInput.PsdzEcuPdxInfo);
            if (eCU.EcuIdentifier != null)
            {
                eCU.ID_SG_ADR = eCU.EcuIdentifier.DiagAddrAsInt;
            }
            eCU.IsSmartActuator = ecuInput.IsSmartActuator;
            // [UH] database replaced start
            PsdzDatabase database = ClientContext.GetDatabase(this.vehicle);
            if (database != null)
            {
                string bnTnName = ecuInput.BnTnName;
                IEcuIdentifier ecuIdentifier = eCU.EcuIdentifier;
                PsdzDatabase.EcuVar ecuVar = database.FindEcuVariantFromBntn(bnTnName, (ecuIdentifier != null) ? new int?(ecuIdentifier.DiagAddrAsInt) : null, this.vehicle, this.ffmResolver);
                if (ecuVar != null && !string.IsNullOrEmpty(ecuVar.Name))
                {
                    //eCU.XepEcuVariant = xEP_ECUVARIANTS;
                    eCU.EcuVariant = ecuVar.Name.ToUpper(CultureInfo.InvariantCulture);
                    PsdzDatabase.EcuClique ecuClique = database.FindEcuClique(ecuVar);
                    //eCU.XepEcuClique = vdc.FindEcuClique(xEP_ECUVARIANTS);
                    PsdzDatabase.EcuGroup ecuGroup = database.FindEcuGroup(ecuVar, this.vehicle, this.ffmResolver);
                    if (ecuGroup != null)
                    {
                        eCU.EcuGroup = ecuGroup.Name.ToUpper(CultureInfo.InvariantCulture);
                    }
                    PsdzDatabase.EcuReps ecuReps = database.FindEcuRep(ecuClique);
                    if (ecuReps != null)
                    {
                        eCU.EcuRep = ecuReps.EcuShortcut;
                    }
                }
                else
                {
                    Log.Warning("ProgrammingObjectBuilder.Build", "No valid ECU variant found for \"{0}\".", ecuInput.BnTnName);
                }
            }
            // [UH] database replaced end
            if (eCU.IsSmartActuator)
            {
                if (!(ecuInput is PsdzSmartActuatorEcu psdzSmartActuatorEcu))
                {
                    Log.Error(Log.CurrentMethod(), $"{ecuInput} is not a SmartActuator");
                    return eCU;
                }
                return new SmartActuatorECU(eCU)
                {
                    ID_HW_NR = "n/a",
                    SmacMasterDiagAddressAsInt = ((psdzSmartActuatorEcu.SmacMasterDiagAddress != null) ? new int?(psdzSmartActuatorEcu.SmacMasterDiagAddress.Offset) : ((int?)null)),
                    SmacID = psdzSmartActuatorEcu.SmacID
                };
            }
            if (ecuInput.PsdzEcuPdxInfo != null && ecuInput.PsdzEcuPdxInfo.IsSmartActuatorMaster)
            {
                if (!(ecuInput is PsdzSmartActuatorMasterEcu psdzSmartActuatorMasterEcu))
                {
                    Log.Error(Log.CurrentMethod(), $"'{ecuInput.PrimaryKey}' is not a SmartActuatorMaster");
                    return eCU;
                }
                return new SmartActuatorMasterECU(eCU)
                {
                    SmacMasterSVK = Build(psdzSmartActuatorMasterEcu.SmacMasterSVK),
                    SmartActuators = psdzSmartActuatorMasterEcu.SmartActuatorEcus.Select((IPsdzEcu x) => (ISmartActuatorEcu)Build(x)).ToList()
                };
            }
            return eCU;
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
                    IsMirrorProtocolSupported = psdzEcuPdxInfo.IsMirrorProtocolSupported,
                    IsEcuAuthEnabled = psdzEcuPdxInfo.IsEcuAuthEnabled,
                    IsIPsecBitmaskSupported = psdzEcuPdxInfo.IsIPsecBitmaskSupported,
                    ProgrammingProtectionLevel = psdzEcuPdxInfo.ProgrammingProtectionLevel,
                    IsSmartActuatorMaster = psdzEcuPdxInfo.IsSmartActuatorMaster,
                    IsCert2025 = psdzEcuPdxInfo.IsCert2025,
                    IsLcsIntegrityProtectionOCSupported = psdzEcuPdxInfo.LcsIntegrityProtectionOCSupported,
                    IsLcsIukCluster = psdzEcuPdxInfo.LcsIukCluster,
                    IsMACsecEnabled = psdzEcuPdxInfo.IsMACsecEnabled,
                    ServicePack = psdzEcuPdxInfo.ServicePack,
                    IsAclEnabled = psdzEcuPdxInfo.AclEnabled
                };
            }
            return null;
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

        public IEcuFailureResponse BuildFailureEcu(IPsdzEcuFailureResponseCto failedEcu)
        {
            if (failedEcu == null)
            {
                return null;
            }
            return new EcuFailureResponse
            {
                Ecu = BuildEcuIdentifier(failedEcu.EcuIdentifierCto.BaseVariant, failedEcu.EcuIdentifierCto.DiagAddrAsInt),
                Reason = failedEcu.Cause.Description
            };
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
                        OtherBindingsStatus = BuildEcuCertCheckingStatus(result.OtherBindingsStatus),
                        KeypackStatus = BuildEcuCertCheckingStatus(result.KeyPackStatus),
                        OnlineCertificateStatus = BuildEcuCertCheckingStatus(result.OnlineCertificateStatus),
                        OnlineBindingsStatus = BuildEcuCertCheckingStatus(result.OnlineBindingsStatus),
                        OnlineBindingDetailStatus = BuildDetailStatus(result.OnlineBindingDetailStatus),
                        KeyPackDetailedStatus = BuildKeypackDetailStatus(result.KeyPackDatailedStatus),
                        CreationTimestamp = result.CreationTimestamp
                    });
                }
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
            if (list != null && list.Count > 0)
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
                case PsdzEcuCertCheckingStatus.Decryption_Error:
                    return EcuCertCheckingStatus.Decryption_Error;
                case PsdzEcuCertCheckingStatus.IssuerCertError:
                    return EcuCertCheckingStatus.IssuerCertError;
                case PsdzEcuCertCheckingStatus.Outdated:
                    return EcuCertCheckingStatus.Outdated;
                case PsdzEcuCertCheckingStatus.OwnCertNotPresent:
                    return EcuCertCheckingStatus.OwnCertNotPresent;
                case PsdzEcuCertCheckingStatus.Undefined:
                    return EcuCertCheckingStatus.Undefined;
                case PsdzEcuCertCheckingStatus.WrongEcuUid:
                    return EcuCertCheckingStatus.WrongEcuUid;
                case PsdzEcuCertCheckingStatus.KeyError:
                    return EcuCertCheckingStatus.KeyError;
                case PsdzEcuCertCheckingStatus.NotUsed:
                    return EcuCertCheckingStatus.NotUsed;
                default:
                    return EcuCertCheckingStatus.Unknown;
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
            if (list != null && list.Count > 0)
            {
                return list.ToArray();
            }
            return null;
        }

        private IKeypackDetailStatus[] BuildKeypackDetailStatus(PsdzKeypackDetailStatus[] psdzKeypackDetailStatuses)
        {
            if (psdzKeypackDetailStatuses == null || psdzKeypackDetailStatuses.Length == 0)
            {
                return null;
            }
            KeypackDetailStatus[] array = new KeypackDetailStatus[psdzKeypackDetailStatuses.Length];
            for (int i = 0; i < psdzKeypackDetailStatuses.Length; i++)
            {
                if (psdzKeypackDetailStatuses[i] != null)
                {
                    array[i] = new KeypackDetailStatus
                    {
                        KeyPackStatus = BuildEcuCertCheckingStatus(psdzKeypackDetailStatuses[i].KeyPackStatus),
                        KeyId = psdzKeypackDetailStatuses[i].KeyId
                    };
                }
            }
            return array;
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
            IEnumerable<string> xWE_SGBMID = svkInput.XWE_SGBMID;
            if (xWE_SGBMID != null)
            {
                List<ISgbmId> list = new List<ISgbmId>();
                foreach (string item in xWE_SGBMID)
                {
                    if (sgbmIdParser.ParseDec(item))
                    {
                        SgbmIdentifier sgbmIdentifier = new SgbmIdentifier();
                        sgbmIdentifier.ProcessClass = sgbmIdParser.ProcessClass;
                        sgbmIdentifier.Id = sgbmIdParser.Id;
                        sgbmIdentifier.MainVersion = sgbmIdParser.MainVersion;
                        sgbmIdentifier.SubVersion = sgbmIdParser.SubVersion;
                        sgbmIdentifier.PatchVersion = sgbmIdParser.PatchVersion;
                        list.Add(sgbmIdentifier);
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
            foreach (KeyValuePair<IPsdzEcuIdentifier, IPsdzObdData> item in obdMap)
            {
                IEcuIdentifier key = Build(item.Key);
                IObdData value = Build(item.Value);
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
            swtEcuObj.EcuIdentifier = Build(swtEcuInput.EcuIdentifier);
            swtEcuObj.RootCertificateState = rootCertificateStateEnumMapper.GetValue(swtEcuInput.RootCertState);
            swtEcuObj.SoftwareSigState = softwareSigStateEnumMapper.GetValue(swtEcuInput.SoftwareSigState);
            foreach (IPsdzSwtApplication swtApplication2 in swtEcuInput.SwtApplications)
            {
                ISwtApplication swtApplication = Build(swtApplication2);
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
            return new SwtApplicationObj(Build(swtApplicationId))
            {
                Fsc = swtApplicationInput.Fsc,
                FscState = fscStateEnumMapper.GetValue(swtApplicationInput.FscState),
                FscCertificate = swtApplicationInput.FscCert,
                FscCertificateState = fscCertificateStateEnumMapper.GetValue(swtApplicationInput.FscCertState),
                SwtType = swtTypeEnumMapper.GetValue(swtApplicationInput.SwtType),
                SwtActionType = (swtApplicationInput.SwtActionType.HasValue ? new SwtActionType?(swtActionTypeEnumMapper.GetValue(swtApplicationInput.SwtActionType.Value)) : ((SwtActionType?)null)),
                IsBackupPossible = swtApplicationInput.IsBackupPossible,
                Position = swtApplicationInput.Position
            };
        }

        private IObdData Build(IPsdzObdData psdzObdData)
        {
            ObdData obdData = new ObdData();
            foreach (IPsdzObdTripleValue obdTripleValue in psdzObdData.ObdTripleValues)
            {
                ObdTripleValue item = new ObdTripleValue(obdTripleValue.CalId, obdTripleValue.ObdId, obdTripleValue.SubCVN);
                obdData.ObdTripleValues.Add(item);
            }
            return obdData;
        }
    }
}
