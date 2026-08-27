using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BMW.Rheingold.CoreFramework
{
    public class VehicleContext : IVehicleContext
    {
        private readonly Vehicle vehicle;

        private readonly IFFMDynamicResolver ffmResolver;

        private Exception exception;

        public Exception Exception => exception;

        public BrandName? VehicleBrandName => vehicle.BrandName;

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Fahrzeug.Nr3stellig", false)]
        public string Motor => vehicle.Motor;

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Fahrzeug.EBezeichnung", false)]
        public string Ereihe => vehicle.Ereihe;

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.VIN17", false)]
        public string VIN17 => vehicle.VIN17;

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.VIN7", false)]
        public string VIN7 => vehicle.VIN7;

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        public string VINType => vehicle.VINType;

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        public string VINRangeType => vehicle.VINRangeType;

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Gwsz", false)]
        public decimal? Gwsz => vehicle.Gwsz;

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        public string GwszUnit
        {
            get
            {
                if (!vehicle.GwszUnit.HasValue)
                {
                    return null;
                }
                return vehicle.GwszUnit.ToString();
            }
        }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Produktionsdatum", false)]
        public DateTime? ProductionDate
        {
            get
            {
                if (!(vehicle.ProductionDate > DateTime.MinValue))
                {
                    return null;
                }
                return vehicle.ProductionDate;
            }
        }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Erstzulassung", false)]
        public DateTime? FirstRegistration => vehicle.FirstRegistration;

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        public IdentificationLevel VehicleIdentLevel => vehicle.VehicleIdentLevel;

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        public IVciDevice MIB => vehicle.MIB;

        public IVciDevice VCI => vehicle.VCI;

        public IFa FA => vehicle.FA;

        public IEnumerable<IEcu> ECU => vehicle.ECU;

        public VehicleContext(Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            this.vehicle = vehicle;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IEcuGroupLocator group)
        {
            if (group == null)
            {
                Log.Warning("VehicleContext.IsSet(IEcuGroupLocator)", "group was null");
                return false;
            }
            try
            {
                string groupName = group.GetDataValue("NAME");
                if (vehicle.ECU != null && !string.IsNullOrEmpty(groupName))
                {
                    return vehicle.ECU.FirstOrDefault((ECU item) => groupName.Equals(item.ECU_GRUPPE, StringComparison.OrdinalIgnoreCase)) != null;
                }
            }
            catch (Exception ex)
            {
                Log.WarningException("VehicleContext.IsSet(IEcuGroupLocator)", ex);
            }
            return false;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IEcuCliqueLocator ecuClique)
        {
            return false;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IEcuVariantLocator variant)
        {
            if (variant == null)
            {
                Log.Warning("VehicleContext.IsSet(IEcuVariantLocator)", "variant was null");
                return false;
            }
            try
            {
                string variantName = variant.GetDataValue("NAME");
                if (vehicle.ECU != null && !string.IsNullOrEmpty(variantName))
                {
                    return vehicle.ECU.FirstOrDefault((ECU item) => string.Equals(variantName, item.VARIANTE, StringComparison.OrdinalIgnoreCase)) != null;
                }
            }
            catch (Exception ex)
            {
                Log.WarningException("VehicleContext.IsSet(IEcuVariantLocator)", ex);
            }
            return false;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(ICharacteristicsLocator characteristicsLocator)
        {
            bool flag = false;
            try
            {
                _ = vehicle.HeatMotors;
                if (characteristicsLocator != null)
                {
                    //[-] IXepCharacteristicRoots characteristicRootsById = DatabaseProviderFactory.Instance.GetCharacteristicRootsById(characteristicsLocator.ParentId);
                    //[+] PsdzDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetClientContext(vehicle)?.Database?.GetCharacteristicRootsById(characteristicsLocator.ParentId.ToString(CultureInfo.InvariantCulture));
                    PsdzDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetClientContext(vehicle)?.Database?.GetCharacteristicRootsById(characteristicsLocator.ParentId.ToString(CultureInfo.InvariantCulture));
                    VehicleCharacteristicContext vehicleCharacteristicContext = new VehicleCharacteristicContext();
                    if (characteristicRootsById != null)
                    {
                        //[-] flag = vehicleCharacteristicContext.IsSetVehicleCharacteristic(characteristicRootsById.Nodeclass.ToString(), vehicle, characteristicsLocator, characteristicRootsById);
                        //[+] flag = vehicleCharacteristicContext.IsSetVehicleCharacteristic(characteristicRootsById.NodeClass, vehicle, characteristicsLocator, characteristicRootsById);
                        flag = vehicleCharacteristicContext.IsSetVehicleCharacteristic(characteristicRootsById.NodeClass, vehicle, characteristicsLocator, characteristicRootsById);
                        Log.Info("VehicleContext.IsSet()", "characterValueSet with Key:{0} Value:{1} Result:{2}", characteristicsLocator.DataClassName, characteristicsLocator.Name, flag);
                        return flag;
                    }
                    Log.Info("VehicleContext.IsSet()", "characterValueSet was null; Result: false");
                }
                else
                {
                    Log.Warning("VehicleIdent.IsSet()", "characteristicsLocator was null");
                }
            }
            catch (Exception ex)
            {
                Log.WarningException("VehicleIdent.IsSet()", ex);
            }
            return flag;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IDiagnosticObjectLocator diagObject)
        {
            if (diagObject != null)
            {
                //[-] foreach (TestPlanItem item in vehicle.Testplan.Item)
                //[-] {
                //[-] if (item.DiagParent != null && item.DiagParent.Id.ToString(CultureInfo.InvariantCulture) == diagObject.Id)
                //[-] {
                //[-] return true;
                //[-] }
                //[-] }
            }
            return false;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IPerceivedSymptomsLocator perceivedSymptom)
        {
            if (perceivedSymptom == null)
            {
                return false;
            }
            //[-] if (vehicle.PerceivedSymptoms != null)
            //[-] {
            //[-] foreach (XEP_PERCEIVEDSYMPTOMSEX perceivedSymptom2 in vehicle.PerceivedSymptoms)
            //[-] {
            //[-] if (perceivedSymptom2.Id == perceivedSymptom.SignedId)
            //[-] {
            //[-] return true;
            //[-] }
            //[-] }
            //[-] }
            return false;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IFaultModeLocator faultModeLocator)
        {
            if (faultModeLocator == null)
            {
                Log.Warning("VehicleContext.IsSet(IFaultModeLocator)", "faultMode was null");
                return false;
            }
            long code = Convert.ToInt32(faultModeLocator.Code);
            if (vehicle != null && vehicle.ECU != null)
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.FEHLER != null && item.FEHLER.FirstOrDefault((DTC item) => item.F_ART == code) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<ITechnicalAction> GetTechnicalActions(bool isSoftwareCampaign)
        {
            //[-] IEnumerable<ITechnicalCampaign> enumerable = vehicle?.TechnicalCampaigns;
            //[+] IEnumerable<ITechnicalCampaign> enumerable = null;
            IEnumerable<ITechnicalCampaign> enumerable = null;
            if (enumerable == null)
            {
                return Enumerable.Empty<ITechnicalAction>();
            }
            return (from tc in enumerable
                    where tc.IsSoftwareCampaign == isSoftwareCampaign
                    select ConvertToTypeTechnicalAction(tc)).ToList();
        }

        private ITechnicalAction ConvertToTypeTechnicalAction(ITechnicalCampaign technicalCampaign)
        {
            //[-] return new TechnicalAction
            //[-] {
            //[-] Description = technicalCampaign.description,
            //[-] SpecialDefectCode = technicalCampaign.specialDefectCode,
            //[-] TechnicalActionState = GetTechnicalActionState(technicalCampaign.state),
            //[-] TechnicalActionRecallType = GetTechnicalActionRecallType(technicalCampaign.RecallType),
            //[-] IsSalesStop = technicalCampaign.IsSalesStop,
            //[-] IsSoftwareCampaign = technicalCampaign.IsSoftwareCampaign,
            //[-] SoftwareVersions = technicalCampaign.SoftwareVersions.ToArray()
            //[-] };
            //[+] return null;
            return null;
        }

        internal static TechnicalActionRecallType GetTechnicalActionRecallType(typeTechnicalCampaignRecallType typeTechnicalCampaignRecallType)
        {
            switch (typeTechnicalCampaignRecallType)
            {
                case typeTechnicalCampaignRecallType.NONE:
                    return TechnicalActionRecallType.None;
                case typeTechnicalCampaignRecallType.SAFETY:
                    return TechnicalActionRecallType.Safety;
                case typeTechnicalCampaignRecallType.EMISSION:
                    return TechnicalActionRecallType.Emission;
                case typeTechnicalCampaignRecallType.NONCOMPLIANT:
                    return TechnicalActionRecallType.NonCompliant;
                default:
                    throw new NotSupportedException();
            }
        }

        internal static TechnicalActionState GetTechnicalActionState(technicalCampaignTypeState technicalCampaignState)
        {
            switch (technicalCampaignState)
            {
                case technicalCampaignTypeState.open:
                    return TechnicalActionState.Open;
                case technicalCampaignTypeState.active:
                    return TechnicalActionState.Active;
                case technicalCampaignTypeState.closed:
                    return TechnicalActionState.Closed;
                default:
                    throw new NotSupportedException();
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IFaultCodeLocator faultCodeLocator)
        {
            if (faultCodeLocator == null)
            {
                Log.Warning("VehicleContext.IsSet(IFaultCodeLocator)", "faultCode was null");
                return false;
            }
            try
            {
                if (vehicle.ECU != null)
                {
                    foreach (ECU item in vehicle.ECU)
                    {
                        if (item.FEHLER == null)
                        {
                            continue;
                        }
                        foreach (DTC item2 in item.FEHLER)
                        {
                            decimal? id = item2.Id;
                            decimal num = Convert.ToDecimal(faultCodeLocator.Id, CultureInfo.InvariantCulture);
                            if ((id.GetValueOrDefault() == num) & id.HasValue)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WarningException("VehicleContext.IsSet(IFaultCodeLocator)", ex);
                exception = ex;
            }
            return false;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public bool IsSet(IEquipmentLocator equipment)
        {
            if (equipment == null)
            {
                Log.Warning("VehicleContext.IsSet(IEquipmentLocator)", "equipment was null");
                return false;
            }
            bool? flag = vehicle.hasFFM(equipment.Name);
            if (flag.HasValue)
            {
                Log.Info("VehicleContext.IsSet(IEquipmentLocator)", "result was: {0} for equipment check: {1}/{2}", flag, equipment.Id, equipment.Name);
                return flag.Value;
            }
            try
            {
                Log.Info("VehicleContext.IsSet(IEquipmentLocator)", "equipment check evaluation needed for equipment: {0}/{1}", equipment.Id, equipment.Name);
                if (ffmResolver != null)
                {
                    //[-] ICollection<IXepInfoObject> infoObjectsByDiagObjectControlId = DatabaseProviderFactory.Instance.GetInfoObjectsByDiagObjectControlId(equipment.SignedId, vehicle, ffmResolver, getHidden: true);
                    //[+] List<PsdzDatabase.SwiInfoObj> infoObjectsByDiagObjectControlId = database?.GetInfoObjectsByDiagObjectControlId(equipment.SignedId.ToString(CultureInfo.InvariantCulture), vehicle, ffmResolver, getHidden: true);
                    List<PsdzDatabase.SwiInfoObj> infoObjectsByDiagObjectControlId = ClientContext.GetClientContext(vehicle)?.Database?.GetInfoObjectsByDiagObjectControlId(equipment.SignedId.ToString(CultureInfo.InvariantCulture), vehicle, ffmResolver, getHidden: true);
                    if (infoObjectsByDiagObjectControlId != null && infoObjectsByDiagObjectControlId.Count > 0)
                    {
                        flag = ffmResolver.Resolve(equipment.SignedId, infoObjectsByDiagObjectControlId.First());
                        vehicle.AddOrUpdateFFM(new FFMResult(equipment.SignedId, equipment.Name, "ForcedFFMResolving", flag, reeval: false));
                        if (flag.HasValue)
                        {
                            Log.Info("VehicleContext.IsSet(IEquipmentLocator)", "result was: {0} for equipment check: {1}/{2}", flag, equipment.Id, equipment.Name);
                            return flag.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WarningException("VehicleContext.IsSet(IEquipmentLocator)", ex);
            }
            Log.Warning("VehicleContext.IsSet(IEquipmentLocator)", "returning in default path with false! for equipment check: {0}/{1}", equipment.Id, equipment.Name);
            return false;
        }

        public void SetClamp15GuardianTrigger(double voltage)
        {
            Log.Info("VehicleContext.SetClamp15GuardianTrigger()", "set clamp15 to min voltage: {0}", voltage);
            //[-] SessionInfoAccessor.SessionInfo.Clamp15MinValue = voltage;
            //[+] SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
            SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
            //[+] if (sessionInfo != null) sessionInfo.Clamp15MinValue = voltage;
            if (sessionInfo != null) sessionInfo.Clamp15MinValue = voltage;
        }

        public void SetClamp30GuardianTrigger(double voltage)
        {
            Log.Info("VehicleContext.SetClamp30GuardianTrigger()", "set clamp30 to min voltage: {0}", voltage);
            //[-] SessionInfoAccessor.SessionInfo.Clamp30MinValue = voltage;
            //[+] SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
            SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
            //[+] if (sessionInfo != null) sessionInfo.Clamp30MinValue = voltage;
            if (sessionInfo != null) sessionInfo.Clamp30MinValue = voltage;
        }

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        public void SetPWFStateGuardianTrigger(int[] validPWFStates)
        {
            if (validPWFStates != null)
            {
                Log.Info("VehicleContext.SetPWFStateGuardianTrigger()", "set PWF {0}.", validPWFStates.ToStringItems());
                HashSet<int> hashSet = new HashSet<int>();
                foreach (int item in validPWFStates)
                {
                    hashSet.Add(item);
                }
                //[-] SessionInfoAccessor.SessionInfo.ValidPWFStates = hashSet;
                //[+] SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
                SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
                //[+] if (sessionInfo != null) sessionInfo.ValidPWFStates = hashSet;
                if (sessionInfo != null) sessionInfo.ValidPWFStates = hashSet;
            }
            else
            {
                Log.Warning("VehicleContext.SetPWFStateGuardianTrigger()", "validPWFStates was null!!!");
            }
        }
    }
}
