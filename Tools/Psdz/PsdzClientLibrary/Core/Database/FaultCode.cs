using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

#pragma warning disable CS0618, CS0649
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class FaultCode : XEP_FAULTCODE
    {
        private InfoObject document;

        private DTC dtc;

        private ECU ecu;

        private decimal? ecuNoAnswer;

        private bool isVirtualDTC;

        private IList<XEP_ENVCONDSLABELS> listEnvConds;

        private decimal? parentId;

        private Vehicle vehicleContext;

        private IFFMDynamicResolver ffmResolver;

        private ISPELocator[] parents;

        private IDocumentLocator documentLocator;

        public ISPELocator[] Children
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public DTC DTC
        {
            get
            {
                return dtc;
            }
            set
            {
                if (dtc != value)
                {
                    dtc = value;
                    OnPropertyChanged("DTC");
                }
            }
        }

        public string DataClassName => "FaultCode";

        public string[] DataValueNames => new string[13]
        {
        "ID", "CODE", "DATATYPE", "WEIGHTING", "SCHEINFEHLER", "AUSBLENDINDEX", "RELEVANCE", "SICHERHEITSRELEVANT", "VALIDTO", "VALIDTO",
        "VALIDFROM", "DIAGNOSEINDEX", "ECUVARIANTID"
        };

        public InfoObject Doc
        {
            get
            {
                if (document == null)
                {
                    //[-] document = InfoObjectFactory.Instance.GetFaultCodeDocument(base.ID, FortAsHexString, vehicleContext);
                }
                return document;
            }
            set
            {
                document = value;
            }
        }

        public InfoObject Document => document;

        public ECU ECU
        {
            get
            {
                return ecu;
            }
            set
            {
                if (ecu != value)
                {
                    ecu = value;
                    OnPropertyChanged("ECU");
                }
            }
        }

        public decimal? ECUNOANSWER
        {
            get
            {
                return ecuNoAnswer;
            }
            set
            {
                if (!(value == ecuNoAnswer))
                {
                    ecuNoAnswer = value;
                    OnPropertyChanged("ECUNOANSWER");
                }
            }
        }

        public Exception Exception
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public string FortAsHexString
        {
            get
            {
                if (DTC != null)
                {
                    return DTC.FortAsHexString;
                }
                return $"{F_ORT:X}";
            }
        }

        public long? F_ORT
        {
            get
            {
                try
                {
                    if (DTC != null && DTC.F_ORT.HasValue)
                    {
                        return DTC.F_ORT;
                    }
                    if (!string.IsNullOrEmpty(base.CODE))
                    {
                        return Convert.ToInt64(base.CODE);
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("FaultCode.get_F_ORT", exception);
                }
                return null;
            }
        }

        public bool HasException
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public string Id => base.ID.ToString(CultureInfo.InvariantCulture);

        public string[] IncomingLinkNames
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public bool IsVirtualDTC
        {
            get
            {
                return isVirtualDTC;
            }
            set
            {
                if (value != isVirtualDTC)
                {
                    isVirtualDTC = value;
                    OnPropertyChanged("IsVirtualDTC");
                }
            }
        }

        public IList<XEP_ENVCONDSLABELS> ListEnvConds => listEnvConds;

        public string[] OutgoingLinkNames
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public decimal? PARENTID
        {
            get
            {
                return parentId;
            }
            set
            {
                if (!(value == parentId))
                {
                    parentId = value;
                    OnPropertyChanged("PARENTID");
                }
            }
        }

        public ISPELocator[] Parents
        {
            get
            {
                if (parents != null)
                {
                    return parents;
                }
                List<ISPELocator> list = new List<ISPELocator>();
                //[-] XEP_ECUVARIANTS xEP_ECUVARIANTS = null;
                //[+] PsdzDatabase.EcuVar xEP_ECUVARIANTS = null;
                PsdzDatabase.EcuVar xEP_ECUVARIANTS = null;
                if (ECU != null && !string.IsNullOrEmpty(ECU.VARIANTE))
                {
                    //[-] xEP_ECUVARIANTS = DatabaseProviderFactory.Instance.GetEcuVariantByName(ECU.VARIANTE);
                    //[+] xEP_ECUVARIANTS = ClientContext.GetDatabase(vehicleContext)?.GetEcuVariantById(ECU.VARIANTE);
                    xEP_ECUVARIANTS = ClientContext.GetDatabase(vehicleContext)?.GetEcuVariantById(ECU.VARIANTE);
                }
                if (xEP_ECUVARIANTS == null && base.ECUVARIANTID.HasValue)
                {
                    //[-] xEP_ECUVARIANTS = DatabaseProviderFactory.Instance.GetEcuVariantById(base.ECUVARIANTID.Value);
                    //[+] xEP_ECUVARIANTS = ClientContext.GetDatabase(vehicleContext)?.GetEcuVariantById(base.ECUVARIANTID.Value.ToString(CultureInfo.InvariantCulture));
                    xEP_ECUVARIANTS = ClientContext.GetDatabase(vehicleContext)?.GetEcuVariantById(base.ECUVARIANTID.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (xEP_ECUVARIANTS != null)
                {
                    IEcuVariantLocator item = new EcuVariantLocator(xEP_ECUVARIANTS, vehicleContext, ffmResolver);
                    list.Add(item);
                }
                parents = list.ToArray();
                return parents;
            }
        }

        public decimal SignedId => base.ID;

        public ITextContent TextContent
        {
            get
            {
                if (!IsVirtualDTC)
                {
                    //[-] XEP_FAULTLABELS xepFaultLabelByFaultCodeId = DatabaseProviderFactory.Instance.GetXepFaultLabelByFaultCodeId(base.ID);
                    //[+] XEP_FAULTLABELS xepFaultLabelByFaultCodeId = null;
                    XEP_FAULTLABELS xepFaultLabelByFaultCodeId = null;
                    if (xepFaultLabelByFaultCodeId != null)
                    {
                        return new TextContent(xepFaultLabelByFaultCodeId.Title);
                    }
                }
                if (IsVirtualDTC)
                {
                    //[-] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = DatabaseProviderFactory.Instance.GetXepVirtualFaultLabelsByVirtualFaultCodeId(base.ID);
                    //[+] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                    XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                    if (xepVirtualFaultLabelsByVirtualFaultCodeId != null)
                    {
                        return new TextContent(xepVirtualFaultLabelsByVirtualFaultCodeId.Title);
                    }
                    //[-] XEP_COMBIFAULTLABELS xepCombiFaultLabelById = DatabaseProviderFactory.Instance.GetXepCombiFaultLabelById(base.ID);
                    //[+] XEP_COMBIFAULTLABELS xepCombiFaultLabelById = null;
                    XEP_COMBIFAULTLABELS xepCombiFaultLabelById = null;
                    if (xepCombiFaultLabelById != null)
                    {
                        return new TextContent(xepCombiFaultLabelById.Title);
                    }
                }
                return new TextContent("na");
            }
        }

        public Vehicle VehicleContext
        {
            get
            {
                return vehicleContext;
            }
            set
            {
                if (vehicleContext != value)
                {
                    vehicleContext = value;
                    OnPropertyChanged("VehicleContext");
                }
            }
        }

        public IFFMDynamicResolver FFMResolver
        {
            get
            {
                return ffmResolver;
            }
            set
            {
                if (ffmResolver != value)
                {
                    ffmResolver = value;
                    OnPropertyChanged("FFMResolver");
                }
            }
        }

        public bool bRelevance
        {
            get
            {
                decimal? rELEVANCE = base.RELEVANCE;
                if ((rELEVANCE.GetValueOrDefault() == default(decimal)) & rELEVANCE.HasValue)
                {
                    return false;
                }
                return true;
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static FaultCode GetCombinedFaultCode(DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (dtc == null || vehicle == null || !dtc.Id.HasValue)
            {
                return null;
            }
            return GetCombinedFaultCode(dtc.Id.Value, vehicle, ffmResolver);
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static FaultCode GetCombinedFaultCode(decimal id, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            try
            {
                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[-] if (instance != null && instance.DatabaseAccessType != DatabaseType.None)
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                //[+] if (instance == null)
                if (instance == null)
                {
                    //[-] XEP_COMBINEDFAULTS xepCombinedFaultById = DatabaseProviderFactory.Instance.GetXepCombinedFaultById(id, vehicle, ffmResolver);
                    //[+] XEP_COMBINEDFAULTS xepCombinedFaultById = null;
                    XEP_COMBINEDFAULTS xepCombinedFaultById = null;
                    if (xepCombinedFaultById != null)
                    {
                        return new FaultCode
                        {
                            IsVirtualDTC = true,
                            VehicleContext = vehicle,
                            FFMResolver = ffmResolver,
                            DTC = vehicle.GetDTC(id),
                            ID = xepCombinedFaultById.ID,
                            CODE = xepCombinedFaultById.CODE,
                            SICHERHEITSRELEVANT = xepCombinedFaultById.SICHERHEITSRELEVANT,
                            VALIDFROM = xepCombinedFaultById.VALIDFROM,
                            VALIDTO = xepCombinedFaultById.VALIDTO,
                            WEIGHTING = xepCombinedFaultById.WEIGHTING
                        };
                    }
                    Log.Warning("FaultCode.getCombinedFaultCode()", "no combined fault with fault id:{0} found", id);
                }
                else
                {
                    Log.Error("FaultCode.getCombinedFaultCode()", "no well configured database found; DatabaseProviderFactory.Instance was null");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.getCombinedFaultCode()", exception);
            }
            return null;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static FaultCode GetFaultCode(string refCode, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            try
            {
                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[-] if (instance != null && instance.DatabaseAccessType != DatabaseType.None)
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                //[+] if (instance != null)
                if (instance != null)
                {
                    decimal id = Convert.ToDecimal(refCode);
                    //[-] FaultCode faultCodeById = instance.GetFaultCodeById(id, vehicle, ffmDynamicResolver);
                    //[+] FaultCode faultCodeById = null;
                    FaultCode faultCodeById = null;
                    if (faultCodeById != null)
                    {
                        faultCodeById.VehicleContext = vehicle;
                        return faultCodeById;
                    }
                    Log.Warning("FaultCode.GetFaultCode()", "No valid faultcode found in database for ref: {0}", refCode);
                }
                else
                {
                    Log.Error("FaultCode.GetFaultCode()", "No well configured database found; DatabaseProviderFactory. Instance was null");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.GetFaultCode()", exception);
            }
            return null;
        }

        [PreserveSource(Hint = "IDatabaseProvider", SignatureModified = true)]
        public static FaultCode GetFaultCode(ECU ecu, DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmResolver, PsdzDatabase db)
        {
            return GetFaultCode(ecu, dtc, vehicle, ffmResolver, resolveEnvCondLabels: true, db);
        }

        [PreserveSource(Hint = "IDatabaseProvider", SignatureModified = true)]
        public static FaultCode GetFaultCode(ECU ecu, DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmResolver, bool resolveEnvCondLabels, PsdzDatabase db, IList<FaultCode> resolvedFaultCodes = null)
        {
            if (ecu == null)
            {
                Log.Warning("FaultCode.GetFaultCode(ECU ecu, DTC dtc, Vehicle vec)", "ecu was null");
                return null;
            }
            if (dtc == null || !dtc.F_ORT.HasValue)
            {
                Log.Warning("FaultCode.GetFaultCode(ECU ecu, DTC dtc, Vehicle vec)", "dtc or dtc.F_ORT was null");
                return null;
            }
            try
            {
                if (string.IsNullOrEmpty(ecu.VARIANTE))
                {
                    Log.Warning("FaultCode.GetFaultCode(ECU ecu, DTC dtc, Vehicle vec)", "ecu.VARIANTE was null");
                    return null;
                }
                //[-] if (db != null && db.DatabaseAccessType != DatabaseType.None)
                //[+] if (db != null)
                if (db != null)
                {
                    decimal f_ort = dtc.F_ORT.Value;
                    string ecuDTCType = dtc.EcuDTCType;
                    string variant = ecu.VARIANTE;
                    FaultCode faultCode = null;
                    if (resolvedFaultCodes != null)
                    {
                        //[-] faultCode = resolvedFaultCodes.FirstOrDefault((FaultCode fc) => fc.CODE == f_ort.ToString() && fc.ECU.VARIANTE.Equals(variant, StringComparison.InvariantCultureIgnoreCase) && fc.DATATYPE.Equals(ecuDTCType, StringComparison.InvariantCultureIgnoreCase) && db.EvaluateXepRulesById(fc.ID, vehicle, ffmResolver));
                        //[+] faultCode = resolvedFaultCodes.FirstOrDefault((FaultCode fc) => fc.CODE == f_ort.ToString() && fc.ECU.VARIANTE.Equals(variant, StringComparison.InvariantCultureIgnoreCase) && fc.DATATYPE.Equals(ecuDTCType, StringComparison.InvariantCultureIgnoreCase) && db.EvaluateXepRulesById(fc.ID.ToString(CultureInfo.InvariantCulture), vehicle, ffmResolver));
                        faultCode = resolvedFaultCodes.FirstOrDefault((FaultCode fc) => fc.CODE == f_ort.ToString() && fc.ECU.VARIANTE.Equals(variant, StringComparison.InvariantCultureIgnoreCase) && fc.DATATYPE.Equals(ecuDTCType, StringComparison.InvariantCultureIgnoreCase) && db.EvaluateXepRulesById(fc.ID.ToString(CultureInfo.InvariantCulture), vehicle, ffmResolver));
                    }
                    //[-] faultCode = faultCode ?? db.GetFaultCodeByCodeAndVariantName(f_ort, variant, ecuDTCType, vehicle, ffmResolver);
                    if (faultCode != null)
                    {
                        if (resolveEnvCondLabels && faultCode.ECUVARIANTID.HasValue)
                        {
                            //[-] faultCode.listEnvConds = db.GetEnvCondLabels(f_ort.ToString(CultureInfo.InvariantCulture), faultCode.ECUVARIANTID.Value).ToList();
                        }
                        faultCode.DTC = dtc;
                        faultCode.ECU = ecu;
                        faultCode.VehicleContext = vehicle;
                        return faultCode;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.getFaultCode(ECU ecu, DTC dtc, Vehicle vec)", exception);
            }
            return null;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static FaultCode GetFaultCode(ECU ecu, long f_Ort, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            if (ecu == null)
            {
                Log.Warning("FaultCode.getFaultCode(ECU ecu, long f_Ort)", "ecu was null");
                return null;
            }
            try
            {
                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                if (string.IsNullOrEmpty(ecu.VARIANTE))
                {
                    Log.Warning("FaultCode.getFaultCode(ECU ecu, long f_Ort)", "ecu.VARIANTE was null");
                    return null;
                }
                //[-] if (instance != null && instance.DatabaseAccessType != DatabaseType.None)
                //[+] if (instance != null)
                if (instance != null)
                {
                    //[-] FaultCode faultCodeByCodeAndVariantName = DatabaseProviderFactory.Instance.GetFaultCodeByCodeAndVariantName(f_Ort, ecu.VARIANTE, "F", vehicle, ffmDynamicResolver);
                    //[+] FaultCode faultCodeByCodeAndVariantName = null;
                    FaultCode faultCodeByCodeAndVariantName = null;
                    if (faultCodeByCodeAndVariantName != null)
                    {
                        if (faultCodeByCodeAndVariantName.ECUVARIANTID.HasValue)
                        {
                            //[-] faultCodeByCodeAndVariantName.listEnvConds = DatabaseProviderFactory.Instance.GetEnvCondLabels(f_Ort.ToString(CultureInfo.InvariantCulture), faultCodeByCodeAndVariantName.ECUVARIANTID.Value).ToList();
                        }
                        faultCodeByCodeAndVariantName.ECU = ecu;
                        faultCodeByCodeAndVariantName.VehicleContext = vehicle;
                        return faultCodeByCodeAndVariantName;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.getFaultCode(ECU ecu, long f_Ort)", exception);
            }
            return null;
        }

        public static FaultCode GetVirtualFaultCode(string refCode, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            try
            {
                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[-] if (instance != null && instance.DatabaseAccessType != DatabaseType.None)
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                //[+] if (instance != null)
                if (instance != null)
                {
                    //[-] XEP_VIRTUALFAULTCODES virtualFaultCodeById = instance.GetVirtualFaultCodeById(Convert.ToInt64(refCode), vehicle, ffmResolver);
                    //[+] XEP_VIRTUALFAULTCODES virtualFaultCodeById = null;
                    XEP_VIRTUALFAULTCODES virtualFaultCodeById = null;
                    if (virtualFaultCodeById != null)
                    {
                        return new FaultCode
                        {
                            IsVirtualDTC = true,
                            VehicleContext = vehicle,
                            FFMResolver = ffmResolver,
                            ID = virtualFaultCodeById.ID,
                            CODE = virtualFaultCodeById.CODE,
                            SICHERHEITSRELEVANT = virtualFaultCodeById.SICHERHEITSRELEVANT,
                            VALIDFROM = virtualFaultCodeById.VALIDFROM,
                            VALIDTO = virtualFaultCodeById.VALIDTO,
                            PARENTID = virtualFaultCodeById.PARENTID,
                            WEIGHTING = virtualFaultCodeById.WEIGHTING,
                            ECUNOANSWER = virtualFaultCodeById.ECUNOANSWER,
                            DTC = DTC.GetDTCByFaultCode(virtualFaultCodeById)
                        };
                    }
                }
                else
                {
                    Log.Error("FaultCode.getVirtualFaultCode()", "no well configured database found; DatabaseProviderFactory.Instance was null");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.getVirtualFaultCode()", exception);
            }
            return null;
        }

        public static FaultCode GetVirtualFaultCode(ECU ecu, DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            try
            {
                if (ecu == null || dtc == null || vehicle == null)
                {
                    return null;
                }
                if (string.IsNullOrEmpty(ecu.ECU_GRUPPE))
                {
                    return null;
                }
                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[-] if (instance != null && instance.DatabaseAccessType != DatabaseType.None)
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                //[+] if (instance != null)
                if (instance != null)
                {
                    XEP_VIRTUALFAULTCODES xEP_VIRTUALFAULTCODES = null;
                    if (dtc.Id.HasValue)
                    {
                        //[-] xEP_VIRTUALFAULTCODES = DatabaseProviderFactory.Instance.GetVirtualFaultCodeById(dtc.Id.Value, vehicle, ffmResolver);
                    }
                    if (xEP_VIRTUALFAULTCODES == null && !string.IsNullOrEmpty(ecu.ECU_GRUPPE))
                    {
                        string code = $"S {dtc.F_ORT:X4}";
                        string[] array = ecu.ECU_GRUPPE.Split('|');
                        foreach (string ecuGroup in array)
                        {
                            //[-] xEP_VIRTUALFAULTCODES = DatabaseProviderFactory.Instance.GetVirtualFaultCodeByCodeAndEcuGroup(code, ecuGroup, vehicle, ffmResolver);
                        }
                    }
                    if (xEP_VIRTUALFAULTCODES != null)
                    {
                        return new FaultCode
                        {
                            IsVirtualDTC = true,
                            VehicleContext = vehicle,
                            DTC = dtc,
                            ID = xEP_VIRTUALFAULTCODES.ID,
                            CODE = xEP_VIRTUALFAULTCODES.CODE,
                            SICHERHEITSRELEVANT = xEP_VIRTUALFAULTCODES.SICHERHEITSRELEVANT,
                            VALIDFROM = xEP_VIRTUALFAULTCODES.VALIDFROM,
                            VALIDTO = xEP_VIRTUALFAULTCODES.VALIDTO,
                            PARENTID = xEP_VIRTUALFAULTCODES.PARENTID,
                            WEIGHTING = xEP_VIRTUALFAULTCODES.WEIGHTING,
                            ECUNOANSWER = xEP_VIRTUALFAULTCODES.ECUNOANSWER
                        };
                    }
                }
                else
                {
                    Log.Error("FaultCode.getVirtualFaultCode()", "no well configured database found; DatabaseProviderFactory.Instance was null");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.getVirtualFaultCode()", exception);
            }
            return null;
        }

        [PreserveSource(Hint = "IDatabaseProvider", SignatureModified = true)]
        public static FaultCode GetFaultCodeKindDependent(Fault fault, Vehicle vehicle, IFFMDynamicResolver ffmResolver, PsdzDatabase db)
        {
            if (!fault.DTC.IsCombined && !fault.DTC.IsVirtual)
            {
                return GetFaultCode(fault.ECU, fault.DTC, vehicle, ffmResolver, db);
            }
            if (fault.DTC.IsCombined)
            {
                return GetCombinedFaultCode(fault.DTC, vehicle, ffmResolver);
            }
            return GetVirtualFaultCode(fault.ECU, fault.DTC, vehicle, ffmResolver);
        }

        public string GetDataValue(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return base.ID.ToString(CultureInfo.InvariantCulture);
                case "CODE":
                    if (base.CODE != null)
                    {
                        return base.CODE;
                    }
                    return string.Empty;
                case "F_SELEKT_CODE":
                    if (base.CODE != null && dtc != null && dtc.F_ORT.HasValue)
                    {
                        if (dtc.IsCombined || dtc.IsVirtual)
                        {
                            return base.CODE;
                        }
                        return $"{dtc.F_ORT:X}";
                    }
                    return string.Empty;
                case "DATATYPE":
                    return base.DATATYPE;
                case "WEIGHTING":
                    return base.WEIGHTING.ToString();
                case "SCHEINFEHLER":
                    return base.SCHEINFEHLER;
                case "AUSBLENDINDEX":
                    return base.AUSBLENDINDEX;
                case "RELEVANCE":
                    return base.RELEVANCE.ToString();
                case "SICHERHEITSRELEVANT":
                    return base.SICHERHEITSRELEVANT.ToString();
                case "VALIDTO":
                    return base.VALIDTO.ToString();
                case "VALIDFROM":
                    return base.VALIDFROM.ToString();
                case "DIAGNOSEINDEX":
                    return base.DIAGNOSEINDEX;
                case "ECUVARIANTID":
                    return base.ECUVARIANTID.ToString();
                default:
                    return string.Empty;
            }
        }

        public T GetDataValue<T>(string name)
        {
            object obj = null;
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    obj = base.ID;
                    break;
                case "CODE":
                    obj = base.CODE;
                    break;
                case "DATATYPE":
                    obj = base.DATATYPE;
                    break;
                case "WEIGHTING":
                    obj = base.WEIGHTING;
                    break;
                case "SCHEINFEHLER":
                    obj = base.SCHEINFEHLER;
                    break;
                case "AUSBLENDINDEX":
                    obj = base.AUSBLENDINDEX;
                    break;
                case "RELEVANCE":
                    obj = base.RELEVANCE;
                    break;
                case "SICHERHEITSRELEVANT":
                    obj = base.SICHERHEITSRELEVANT;
                    break;
                case "VALIDTO":
                    obj = base.VALIDTO;
                    break;
                case "VALIDFROM":
                    obj = base.VALIDFROM;
                    break;
                case "DIAGNOSEINDEX":
                    obj = base.DIAGNOSEINDEX;
                    break;
                case "ECUVARIANTID":
                    obj = base.ECUVARIANTID;
                    break;
            }
            Type typeFromHandle = typeof(T);
            try
            {
                if (obj != null)
                {
                    if (obj.GetType() != typeFromHandle)
                    {
                        return (T)Convert.ChangeType(obj, typeFromHandle);
                    }
                    return (T)obj;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.GetDataValue<T>()", exception);
            }
            return default(T);
        }

        public InfoObject GetDocument()
        {
            return Doc;
        }

        public IDocumentLocator GetDocument(string docType)
        {
            if (Doc.XepInfoObject.DocumentType.Equals(docType))
            {
                if (documentLocator == null)
                {
                    documentLocator = new DocumentLocator(Doc);
                }
                return documentLocator;
            }
            return null;
        }

        public void UpdateUwDisplayAndFault(Fault fault, Vehicle vehicle, bool cleanDtcUwDisplayData = true)
        {
            if (fault == null)
            {
                return;
            }
            if (cleanDtcUwDisplayData)
            {
                DTC.F_UW_Display.Clear();
            }
            if (ListEnvConds != null)
            {
                List<F_UW_Display> list = new List<F_UW_Display>();
                foreach (XEP_ENVCONDSLABELS listEnvCond in ListEnvConds)
                {
                    if (listEnvCond != null)
                    {
                        switch (listEnvCond.Uwident)
                        {
                            case "F_EREIGNIS_DTC":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_EREIGNIS_DTC, listEnvCond.Unit));
                                break;
                            case "F_HFK":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_HFK, listEnvCond.Unit));
                                break;
                            case "F_UW_ZEIT_SUPREME":
                            case "F_UW_ZEIT":
                                list.Add(GetTimeStampDisplayItem(vehicle, fault, listEnvCond));
                                break;
                            case "F_UW_KM":
                            case "F_UW_KM_SUPREME":
                                list.Add(GetMileageDisplayItem(fault, listEnvCond));
                                break;
                            case "F_HLZ":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_HLZ, listEnvCond.Unit));
                                break;
                            case "F_PCODE_STRING":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_PCODE_STRING, listEnvCond.Unit));
                                break;
                            case "F_PCODE":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_PCODE, listEnvCond.Unit));
                                break;
                            case "F_SAE_CODE_STRING":
                                list.Add(GetSAEDisplayItem(fault, listEnvCond, vehicle.BNType == BNType.BN2000));
                                break;
                            case "F_LZ":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_LZ, listEnvCond.Unit));
                                break;
                            case "F_CODE":
                                list.Add(new F_UW_Display(listEnvCond, fault.DTC.F_CODE, listEnvCond.Unit));
                                break;
                        }
                    }
                }
                list.Sort(delegate (F_UW_Display x, F_UW_Display y)
                {
                    if (x.F_UW_TEXT == null && y.F_UW_TEXT == null)
                    {
                        return 0;
                    }
                    if (x.F_UW_TEXT == null)
                    {
                        return -1;
                    }
                    return (y.F_UW_TEXT == null) ? 1 : string.Compare(x.F_UW_TEXT, y.F_UW_TEXT, StringComparison.Ordinal);
                });
                DTC.F_UW_Display.AddRange(list);
            }
            fault.UpdateUwDisplay(this);
        }

        public XEP_ENVCONDSLABELS GetEnvCondsByUwNr(long? F_UW_NR)
        {
            if (!F_UW_NR.HasValue)
            {
                return null;
            }
            foreach (XEP_ENVCONDSLABELS listEnvCond in listEnvConds)
            {
                if (string.CompareOrdinal(listEnvCond.Uwident, F_UW_NR.Value.ToString(CultureInfo.InvariantCulture)) == 0)
                {
                    return listEnvCond;
                }
            }
            return null;
        }

        public XEP_ENVCONDSLABELS GetEnvCondsByUwNrName(long? F_UW_NR, string F_UW_NAME)
        {
            if (!F_UW_NR.HasValue)
            {
                return null;
            }
            foreach (XEP_ENVCONDSLABELS listEnvCond in listEnvConds)
            {
                if (string.CompareOrdinal(listEnvCond.Uwident, F_UW_NR.Value.ToString(CultureInfo.InvariantCulture)) == 0 && (string.IsNullOrEmpty(listEnvCond.Name) || string.IsNullOrEmpty(F_UW_NAME) || string.Equals(listEnvCond.Name, F_UW_NAME, StringComparison.OrdinalIgnoreCase)))
                {
                    return listEnvCond;
                }
            }
            return null;
        }

        public ISPELocator[] GetIncomingLinks()
        {
            throw new NotSupportedException();
        }

        public ISPELocator[] GetIncomingLinks(string incomingLinkName)
        {
            throw new NotSupportedException();
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            throw new NotSupportedException();
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            throw new NotSupportedException();
        }

        public void LoadInfoObj()
        {
            //[-] IXepInfoObject xepInfoObject = DatabaseProviderFactory.Instance.LoadXepInfoObjForFaultCodeId(base.ID);
            //[+] IXepInfoObject xepInfoObject = new XepInfoObject();
            IXepInfoObject xepInfoObject = new XepInfoObject();
            XepInfoObject xepInfoObjectCasted = Document.XepInfoObjectCasted;
            xepInfoObjectCasted.Title_dede = xepInfoObject.Title_dede;
            xepInfoObjectCasted.Title_el = xepInfoObject.Title_el;
            xepInfoObjectCasted.Title_engb = xepInfoObject.Title_engb;
            xepInfoObjectCasted.Title_enus = xepInfoObject.Title_enus;
            xepInfoObjectCasted.Title_es = xepInfoObject.Title_es;
            xepInfoObjectCasted.Title_fr = xepInfoObject.Title_fr;
            xepInfoObjectCasted.Title_id = xepInfoObject.Title_id;
            xepInfoObjectCasted.Title_it = xepInfoObject.Title_it;
            xepInfoObjectCasted.Title_ja = xepInfoObject.Title_ja;
            xepInfoObjectCasted.Title_ko = xepInfoObject.Title_ko;
            xepInfoObjectCasted.Title_nl = xepInfoObject.Title_nl;
            xepInfoObjectCasted.Title_pt = xepInfoObject.Title_pt;
            xepInfoObjectCasted.Title_ru = xepInfoObject.Title_ru;
            xepInfoObjectCasted.Title_sv = xepInfoObject.Title_sv;
            xepInfoObjectCasted.Title_th = xepInfoObject.Title_th;
            xepInfoObjectCasted.Title_tr = xepInfoObject.Title_tr;
            xepInfoObjectCasted.Title_zhcn = xepInfoObject.Title_zhcn;
            xepInfoObjectCasted.Title_zhtw = xepInfoObject.Title_zhtw;
            xepInfoObjectCasted.Title_cscz = xepInfoObject.Title_cscz;
            xepInfoObjectCasted.Title_plpl = xepInfoObject.Title_plpl;
            xepInfoObjectCasted.DocNumber = xepInfoObject.DocNumber;
        }

        public bool Set()
        {
            try
            {
                if (IsVirtualDTC)
                {
                    if (ECU == null && parentId.HasValue)
                    {
                        //[-] XEP_ECUGROUPS ecuGroupById = DatabaseProviderFactory.Instance.GetEcuGroupById(parentId.Value);
                        //[+] XEP_ECUGROUPS ecuGroupById = null;
                        XEP_ECUGROUPS ecuGroupById = null;
                        if (ecuGroupById != null)
                        {
                            ECU eCUbyECU_GRUPPE = vehicleContext.getECUbyECU_GRUPPE(ecuGroupById.Name);
                            if (eCUbyECU_GRUPPE != null)
                            {
                                ECU = eCUbyECU_GRUPPE;
                            }
                        }
                    }
                    if (ECU == null && parentId.HasValue)
                    {
                        //[-] XEP_ECUVARIANTS ecuVariantById = DatabaseProviderFactory.Instance.GetEcuVariantById(parentId.Value);
                        //[+] XEP_ECUVARIANTS ecuVariantById = null;
                        XEP_ECUVARIANTS ecuVariantById = null;
                        if (ecuVariantById != null && vehicleContext.getECUbyECU_SGBD(ecuVariantById.Name) is ECU eCU)
                        {
                            ECU = eCU;
                        }
                    }
                    if (ECU == null)
                    {
                        Log.Warning("FaultCode.Set()", "ecu was null; dtc cannot be assigned");
                        return false;
                    }
                }
                Log.Info("FaultCode.Set()", "FaultCode to set {0} for ecu: {1}", dtc.F_ORT, ecu.ID_SG_ADR);
                if (vehicleContext != null && vehicleContext.ECU != null && ecu != null)
                {
                    ECU eCU2 = vehicleContext.getECU(ecu.ID_SG_ADR);
                    if (eCU2 != null && eCU2.FEHLER != null)
                    {
                        foreach (DTC item in eCU2.FEHLER)
                        {
                            if (item.F_ORT == DTC.F_ORT)
                            {
                                Log.Info("FaultCode.Set()", "FaultCode {0:X} already set", DTC.F_ORT);
                                return false;
                            }
                        }
                        if (DTC.DTCContext == null)
                        {
                            DTC.DTCContext = new ObservableCollection<typeDTCContext>();
                        }
                        typeDTCContext typeDTCContext2 = new typeDTCContext();
                        typeDTCContext2.F_UW = new ObservableCollection<F_UW>();
                        typeDTCContext2.F_UW_ANZ = 0;
                        typeDTCContext2.SetCurrentMileage(vehicleContext);
                        typeDTCContext2.SetCurrentTimestamp(vehicleContext);
                        DTC.DTCContext.Add(typeDTCContext2);
                        eCU2.FEHLER.Add(DTC);
                        vehicleContext.CalculateFaultProperties(FFMResolver);
                        return true;
                    }
                    Log.Warning("FaultCode.Set()", "related ecu not found in VehicleContext");
                }
                else
                {
                    Log.Warning("FaultCode.Set()", "VehicleContext incorrectly set");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCode.Set()", exception);
            }
            return false;
        }

        public void Update()
        {
            throw new NotSupportedException();
        }

        private F_UW_Display GetSAEDisplayItem(Fault fault, XEP_ENVCONDSLABELS label, bool isBN2000Vehicle)
        {
            string text = string.Empty;
            DTC dTC = fault?.DTC;
            if (dTC != null)
            {
                text = ((!isBN2000Vehicle) ? dTC.F_SAE_CODE_STRING : dTC.F_PCODE_STRING);
            }
            if (string.IsNullOrEmpty(text))
            {
                text = "--";
            }
            return new F_UW_Display(label, text, label.Unit);
        }

        private F_UW_Display GetTimeStampDisplayItem(Vehicle vehicle, Fault fault, XEP_ENVCONDSLABELS label)
        {
            string current_F_UW_WERT = "-1";
            string first_F_UW_WERT = "-1";
            string second_F_UW_WERT = "-1";
            string current_F_UW_EINH = null;
            string first_F_UW_EINH = null;
            string second_F_UW_EINH = null;
            if (vehicle.VehicleLifeStartDate != default(DateTime))
            {
                if (fault.DTC.F_UW_ZEIT_SUPREME > 0.0)
                {
                    current_F_UW_WERT = vehicle.VehicleLifeStartDate.AddSeconds(fault.DTC.F_UW_ZEIT_SUPREME.Value).ToGeneralDateLongTimeWithMiliseconds();
                }
                else if (fault.DTC.F_UW_ZEIT > 0)
                {
                    current_F_UW_WERT = vehicle.VehicleLifeStartDate.AddSeconds(fault.DTC.F_UW_ZEIT).ToString("G");
                }
                if (fault.DTC.First != null)
                {
                    if (fault.DTC.First.F_UW_ZEIT_SUPREME > 0.0)
                    {
                        first_F_UW_WERT = vehicle.VehicleLifeStartDate.AddSeconds(fault.DTC.First.F_UW_ZEIT_SUPREME.Value).ToGeneralDateLongTimeWithMiliseconds();
                    }
                    else if (fault.DTC.First.F_UW_ZEIT > 0)
                    {
                        first_F_UW_WERT = vehicle.VehicleLifeStartDate.AddSeconds(fault.DTC.First.F_UW_ZEIT.Value).ToString("G");
                    }
                }
                if (fault.DTC.Second != null)
                {
                    if (fault.DTC.Second.F_UW_ZEIT_SUPREME > 0.0)
                    {
                        second_F_UW_WERT = vehicle.VehicleLifeStartDate.AddSeconds(fault.DTC.Second.F_UW_ZEIT_SUPREME.Value).ToGeneralDateLongTimeWithMiliseconds();
                    }
                    else if (fault.DTC.Second.F_UW_ZEIT > 0)
                    {
                        second_F_UW_WERT = vehicle.VehicleLifeStartDate.AddSeconds(fault.DTC.Second.F_UW_ZEIT.Value).ToString("G");
                    }
                }
            }
            else
            {
                string text = (string.IsNullOrEmpty(label.Unit) ? "s" : label.Unit);
                if (fault.DTC.F_UW_ZEIT_SUPREME > 0.0)
                {
                    current_F_UW_WERT = fault.DTC.F_UW_ZEIT_SUPREME.Value.ToString();
                    current_F_UW_EINH = text;
                }
                else if (fault.DTC.F_UW_ZEIT > 0)
                {
                    current_F_UW_WERT = fault.DTC.F_UW_ZEIT.ToString();
                    current_F_UW_EINH = text;
                }
                if (fault.DTC.First != null)
                {
                    if (fault.DTC.First.F_UW_ZEIT_SUPREME > 0.0)
                    {
                        first_F_UW_WERT = fault.DTC.First.F_UW_ZEIT_SUPREME.Value.ToString();
                        first_F_UW_EINH = text;
                    }
                    else if (fault.DTC.First.F_UW_ZEIT > 0)
                    {
                        first_F_UW_WERT = fault.DTC.First.F_UW_ZEIT.Value.ToString();
                        first_F_UW_EINH = text;
                    }
                }
                if (fault.DTC.Second != null)
                {
                    if (fault.DTC.Second.F_UW_ZEIT_SUPREME > 0.0)
                    {
                        second_F_UW_WERT = fault.DTC.Second.F_UW_ZEIT_SUPREME.Value.ToString();
                        second_F_UW_EINH = text;
                    }
                    else if (fault.DTC.Second.F_UW_ZEIT > 0)
                    {
                        second_F_UW_WERT = fault.DTC.Second.F_UW_ZEIT.Value.ToString();
                        second_F_UW_EINH = text;
                    }
                }
            }
            return new F_UW_Display(label, current_F_UW_WERT, current_F_UW_EINH, first_F_UW_WERT, first_F_UW_EINH, second_F_UW_WERT, second_F_UW_EINH);
        }

        private F_UW_Display GetMileageDisplayItem(Fault fault, XEP_ENVCONDSLABELS label)
        {
            string text = (string.IsNullOrEmpty(label.Unit) ? "km" : label.Unit);
            string text2 = "-1";
            string text3 = "0.0";
            string text4 = text2;
            if (fault.DTC.F_UW_KM_SUPREME.HasValue)
            {
                text4 = fault.DTC.F_UW_KM_SUPREME.Value.ToString(text3);
            }
            else if (fault.DTC.F_UW_KM.HasValue)
            {
                text4 = fault.DTC.F_UW_KM.Value.ToString();
            }
            string current_F_UW_EINH = ((text4 != text2) ? text : null);
            string text5 = text2;
            if (fault.DTC.First != null)
            {
                if (fault.DTC.First.F_UW_KM_SUPREME.HasValue)
                {
                    text5 = fault.DTC.First.F_UW_KM_SUPREME.Value.ToString(text3);
                }
                else if (fault.DTC.First.F_UW_KM.HasValue)
                {
                    text5 = fault.DTC.First.F_UW_KM.Value.ToString();
                }
            }
            string first_F_UW_EINH = ((text5 != text2) ? text : null);
            string text6 = text2;
            if (fault.DTC.Second != null)
            {
                if (fault.DTC.Second.F_UW_KM_SUPREME.HasValue)
                {
                    text6 = fault.DTC.Second.F_UW_KM_SUPREME.Value.ToString(text3);
                }
                else if (fault.DTC.Second.F_UW_KM.HasValue)
                {
                    text6 = fault.DTC.Second.F_UW_KM.Value.ToString();
                }
            }
            string second_F_UW_EINH = ((text6 != text2) ? text : null);
            return new F_UW_Display(label, text4, current_F_UW_EINH, text5, first_F_UW_EINH, text6, second_F_UW_EINH);
        }
    }
}
