using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.ISTA.CoreFramework;
using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;

#pragma warning disable CS0169, CS0649, CS0162
namespace BMW.Rheingold.CoreFramework
{
    public abstract class IstaModuleBase : IIstaModule, IDisposable
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected ParameterContainer _globalModuleInParameter = new ParameterContainer();
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected ParameterContainer _globalModuleOutParameter = new ParameterContainer();
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected ParameterContainer _globalModuleInAndOutParameter = new ParameterContainer();
        public ILogic logic;
        private IResult resultSet = new Result();
        [PreserveSource(Hint = "EcuKomProxy", Placeholder = true)]
        private PlaceholderType ecuKomProxy;
        private ISuspicionLinkCounter suspicionLinkCount;
        private string _lastCallingMethod = string.Empty;
        private IAppSessionContext appSessionContext;
        private bool _doLoopHandling;
        private bool _verboseLoopLog;
        private readonly List<string> serviceCodesHandledInLoop = new List<string>();
        public List<IDiagnosticObjectLocator> SuspiciuosItems = new List<IDiagnosticObjectLocator>();
        public List<IDiagnosticObjectLocator> OkItems = new List<IDiagnosticObjectLocator>();
        public List<IDiagnosticObjectLocator> NotOkItems = new List<IDiagnosticObjectLocator>();
        public abstract ILogger Logger { get; }
        public abstract IProtocolBasic FastaProtocoler { get; }
        public abstract IEcuKomStatement EcuKomStatement { get; }
        public abstract IEcuKom ecuKom { get; }
        public abstract IFFMDynamicResolver FFMResolver { get; }

        [PreserveSource(Hint = "IInputListener", Placeholder = true)]
        public abstract PlaceholderType InputListener { get; }
        public abstract IVehicleContext VehicleContext { get; }
        public abstract IDealerData DealerData { get; }
        public abstract ISOCAccessor SOCAccessor { get; }
        public abstract ISOCAccessor Contexts { get; }
        public abstract Vehicle Vehicle { get; }

        [PreserveSource(Hint = "IDatabaseProvider", Placeholder = true)]
        public virtual PlaceholderType DBProvider { get; set; }
        protected ITextContentManager textContentManager { get; set; }

        [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
        protected PlaceholderType Me { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IFastaGrouping FastaGrouping { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public ILogic IstaOperationLogic => logic;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IResult ResultSet
        {
            get
            {
                return resultSet;
            }

            set
            {
                resultSet = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IEcuKom EcuKom => ecuKom;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IEcuKomApi EcuKomApi
        {
            get
            {
                //[-] if (ecuKomProxy == null || ecuKomProxy.FastaService != FastaProtocoler || ecuKom != ecuKomProxy.EcuKom)
                //[-] {
                //[-] ecuKomProxy = new EcuKomProxy(ecuKom, FastaProtocoler);
                //[-] }
                //[-] return ecuKomProxy;
                //[+] return null;
                return null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ISuspicionLinkCounter SuspicionLinkCount
        {
            get
            {
                if (suspicionLinkCount == null)
                {
                //[-] suspicionLinkCount = new SuspicionLinkCounter(this, Vehicle, FFMResolver);
                }

                return suspicionLinkCount;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public string LastCallingMethod
        {
            get
            {
                if (string.IsNullOrEmpty(_lastCallingMethod))
                {
                    return "_not_set_";
                }

                return _lastCallingMethod;
            }

            protected set
            {
                _lastCallingMethod = value;
            }
        }

        public virtual IAppSessionContext AppSessionContext
        {
            get
            {
                //[-] appSessionContext = appSessionContext ?? new AppSessionContext(ConfigSettings.OperationalMode, ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OnlineMode", defaultValue: true), logic);
                return appSessionContext;
            }
        }

        [PreserveSource(Hint = "IEnergySettings", Placeholder = true)]
        public abstract PlaceholderType EnergySettings { get; }

        [PreserveSource(Hint = "INOPProvisioning", Placeholder = true)]
        public abstract PlaceholderType NOPProvisioning { get; }

        [PreserveSource(Hint = "IVPSProvisioning", Placeholder = true)]
        public abstract PlaceholderType VPSProvisioning { get; }

        [PreserveSource(Hint = "IPersistency", Placeholder = true)]
        public abstract PlaceholderType Persistency { get; }

        [PreserveSource(Hint = "INavigationMapProcessor", Placeholder = true)]
        public abstract PlaceholderType NavigationMapProcessor { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public bool _DoLoopHandling
        {
            get
            {
                return _doLoopHandling;
            }

            set
            {
                if (_doLoopHandling && !value)
                {
                //[-] FastaProtocoler?.WriteLoopEntriesToLog(_VerboseLoopLogs);
                }

                _doLoopHandling = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public bool _VerboseLoopLogs
        {
            get
            {
                return _verboseLoopLog;
            }

            set
            {
                _verboseLoopLog = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public abstract IModule ModuleData { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string ModuleNameTransformator(string moduleName)
        {
            string text = null;
            try
            {
                if (!string.IsNullOrEmpty(moduleName))
                {
                    text = moduleName.Replace('-', '_');
                    text = text.Replace('*', '_');
                    text = text.Replace('+', '_');
                    text = text.Replace('/', '_');
                    text = text.Replace(':', '_');
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("IstaModuleBase.ModuleNameTransformator()", exception);
            }

            return text;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string ConvertFromBase64(string data)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(data);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                Log.Error("ISTAModule.ConvertBase64()", "Conversion of \"{0}\" failed: {1} ", data, ex);
                return null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public bool __convertToBoolean(object value)
        {
            bool result = false;
            try
            {
                result = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToBoolean(object)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public byte __convertToByte(object value)
        {
            byte result = 0;
            try
            {
                result = Convert.ToByte(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToByte(object)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public byte __convertToByte(double value)
        {
            byte result = 0;
            try
            {
                result = Convert.ToByte(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToByte(double)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public byte __convertToByte(float value)
        {
            byte result = 0;
            try
            {
                result = Convert.ToByte(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToByte(float)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public byte __convertToByte(string value)
        {
            byte result = 0;
            if (!string.IsNullOrEmpty(value.Trim()))
            {
                CultureInfo provider = ((!value.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                try
                {
                    result = Convert.ToByte(Math.Truncate(Convert.ToDouble(value, provider)), CultureInfo.InvariantCulture);
                }
                catch (FormatException exception)
                {
                    Log.ErrorException("ISTAModule.__convertToByte(string)", exception);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public byte __convertToByte(string value, int fromBase)
        {
            byte result = 0;
            try
            {
                result = Convert.ToByte(value, fromBase);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToByte(string, int)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public short __convertToInt16(object value)
        {
            short result = 0;
            if (value is string)
            {
                string text = ((string)value).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    CultureInfo provider = ((!text.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                    try
                    {
                        result = Convert.ToInt16(value, provider);
                    }
                    catch (FormatException exception)
                    {
                        Log.ErrorException("ISTAModule.__convertToDouble(object)", exception);
                    }
                }
            }
            else
            {
                try
                {
                    result = Convert.ToInt16(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException exception2)
                {
                    Log.ErrorException("ISTAModule.__convertToInt16(object)", exception2);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public short __convertToInt16(double value)
        {
            short result = 0;
            try
            {
                result = Convert.ToInt16(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt16(double)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public short __convertToInt16(float value)
        {
            short result = 0;
            try
            {
                result = Convert.ToInt16(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt16(float)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public short __convertToInt16(string value)
        {
            short result = 0;
            if (!string.IsNullOrEmpty(value.Trim()))
            {
                CultureInfo provider = ((!value.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                try
                {
                    result = Convert.ToInt16(Math.Truncate(Convert.ToDouble(value, provider)));
                }
                catch (FormatException exception)
                {
                    Log.ErrorException("ISTAModule.__convertToInt16(string)", exception);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public short __convertToInt16(string value, int fromBase)
        {
            short result = 0;
            try
            {
                result = Convert.ToInt16(value, fromBase);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt16(string, int)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public int __convertToInt32(object value)
        {
            int result = 0;
            if (value is string)
            {
                string text = ((string)value).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    CultureInfo provider = ((!text.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                    try
                    {
                        result = Convert.ToInt32(value, provider);
                    }
                    catch (FormatException exception)
                    {
                        Log.ErrorException("ISTAModule.__convertToInt32(object)", exception);
                    }
                }
            }
            else
            {
                try
                {
                    result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException exception2)
                {
                    Log.ErrorException("ISTAModule.__convertToInt32(object)", exception2);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public int __convertToInt32(double value)
        {
            int result = 0;
            try
            {
                result = Convert.ToInt32(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt32(double)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public int __convertToInt32(float value)
        {
            int result = 0;
            try
            {
                result = Convert.ToInt32(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt32(float)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public int __convertToInt32(string value)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(value.Trim()))
            {
                CultureInfo provider = ((!value.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                try
                {
                    result = Convert.ToInt32(Math.Truncate(Convert.ToDouble(value, provider)));
                }
                catch (FormatException exception)
                {
                    Log.ErrorException("ISTAModule.__convertToInt32(string)", exception);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public int __convertToInt32(string value, int fromBase)
        {
            int result = 0;
            try
            {
                result = Convert.ToInt32(value, fromBase);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt32(string, int)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public long __convertToInt64(object value)
        {
            long result = 0L;
            if (value is string)
            {
                string text = ((string)value).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    CultureInfo provider = ((!text.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                    try
                    {
                        result = Convert.ToInt64(value, provider);
                    }
                    catch (FormatException exception)
                    {
                        Log.ErrorException("ISTAModule.__convertToInt64(object)", exception);
                    }
                }
            }
            else
            {
                try
                {
                    result = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException exception2)
                {
                    Log.ErrorException("ISTAModule.__convertToInt64(object)", exception2);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public long __convertToInt64(double value)
        {
            long result = 0L;
            try
            {
                result = Convert.ToInt64(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt64(double)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public long __convertToInt64(float value)
        {
            long result = 0L;
            try
            {
                result = Convert.ToInt64(Math.Truncate(value), CultureInfo.InvariantCulture);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt64(float)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public long __convertToInt64(string value)
        {
            long result = 0L;
            if (!string.IsNullOrEmpty(value.Trim()))
            {
                CultureInfo provider = ((!value.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                try
                {
                    result = Convert.ToInt64(Math.Truncate(Convert.ToDouble(value, provider)));
                }
                catch (FormatException exception)
                {
                    Log.ErrorException("ISTAModule.__convertToInt64(string)", exception);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public long __convertToInt64(string value, int fromBase)
        {
            long result = 0L;
            try
            {
                result = Convert.ToInt64(value, fromBase);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToInt64(string, int)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public double __convertToDouble(object value)
        {
            double result = 0.0;
            if (value is string)
            {
                string text = ((string)value).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    CultureInfo provider = ((!text.Contains(",")) ? CultureInfo.InvariantCulture : new CultureInfo(ConfigSettings.CurrentUICulture));
                    try
                    {
                        result = Convert.ToDouble(value, provider);
                    }
                    catch (FormatException exception)
                    {
                        Log.ErrorException("ISTAModule.__convertToDouble(object)", exception);
                    }
                }
            }
            else
            {
                try
                {
                    result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException exception2)
                {
                    Log.ErrorException("ISTAModule.__convertToDouble(object)", exception2);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string __convertToString(object value)
        {
            string result = string.Empty;
            try
            {
                result = Convert.ToString(value);
            }
            catch (FormatException exception)
            {
                Log.ErrorException("ISTAModule.__convertToString(object)", exception);
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string __getChar(string text, int position)
        {
            if (text.Length <= position)
            {
                return string.Empty;
            }

            return text.Substring(position, 1);
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public static Array __initArray<T>(int[] sizes, object initValue)
        {
            Array array = null;
            try
            {
                array = Array.CreateInstance(typeof(T), sizes);
                T val = default(T);
                if (initValue != null && typeof(T).IsAssignableFrom(initValue.GetType()))
                {
                    val = (T)initValue;
                }

                int num = sizes[0];
                for (int i = 1; i < sizes.Length; i++)
                {
                    num *= sizes[i];
                }

                int[] array2 = new int[sizes.Length];
                for (int j = 0; j < num; j++)
                {
                    array.SetValue(val, array2);
                    array2[sizes.Length - 1]++;
                    for (int num2 = sizes.Length - 1; num2 >= 0; num2--)
                    {
                        if (array2[num2] == sizes[num2])
                        {
                            array2[num2] = 0;
                            if (num2 > 0)
                            {
                                array2[num2 - 1]++;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.__initArray<T>(int[], object)", exception);
            }

            return array;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public static List<T> __initList<T>(int size, object initValue)
        {
            List<T> list = null;
            try
            {
                list = new List<T>();
                if (size > 0)
                {
                    T item = default(T);
                    if (initValue != null && typeof(T).IsAssignableFrom(initValue.GetType()))
                    {
                        item = (T)initValue;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        list.Add(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.__initList<T>(int, object)", exception);
            }

            return list;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public static List<T> __initList<T>(int len, T obj)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < len; i++)
            {
                list.Add(obj);
            }

            return list;
        }

        public abstract void Dispose();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected abstract void __handleInParameter();
        [AuthorAPIHidden]
        public IVehicleContext GetVehicleContext()
        {
            return VehicleContext;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(SignatureModified = true)]
        public IDiagnosticObjectLocator __DiagnosticObject(string refText)
        {
            string text = refText;
            if (string.IsNullOrEmpty(text))
            {
            //[-] XEP_DIAGNOSISOBJECTSEX xEP_DIAGNOSISOBJECTSEX = SelectDiagParent("__DiagnosticObject(string)");
            //[-] if (xEP_DIAGNOSISOBJECTSEX != null)
            //[-] {
            //[-] text = xEP_DIAGNOSISOBJECTSEX.Name;
            //[-] }
            }

            //[-] DiagnosticObjectLocator diagnosticObjectLocator = null;
            try
            {
                //[-] ICollection<XEP_DIAGNOSISOBJECTSEX> diagObjectsByName = DBProvider.GetDiagObjectsByName(text, Vehicle, FFMResolver, getHidden: true);
                //[-] if (diagObjectsByName.Count > 0)
                //[-] {
                //[-] DiagnosticObject diagnosticObject = new DiagnosticObject(diagObjectsByName.First(), Vehicle, FFMResolver);
                //[-] Log.Info("ISTAModule.__DiagnosticObject()", "found diag object with children title:{0}", diagnosticObject.GetXepDiagnosisObject().Title_dede);
                //[-] ICollection<XEP_DIAGNOSISOBJECTSEX> childDiagObjects = DBProvider.GetChildDiagObjects(diagnosticObject.GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
                //[-] diagnosticObjectLocator = new DiagnosticObjectLocator(diagnosticObject, childDiagObjects);
                //[-] }
                //[-] else
                {
                    Log.Warning("ISTAModule.__DiagnosticObject()", "unable to find diagnostic object refreneced with: {0}", refText);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__DiagnosticObject()", exception);
            }

            //[-] if (diagnosticObjectLocator == null)
            {
                Log.Warning("ISTAModule.__DiagnosticObject()", "result was null; maybe your testmodule will die");
                return null;
            }
        //[-] return diagnosticObjectLocator;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(SignatureModified = true)]
        public IDiagnosticObjectLocator __DiagnosticObject(decimal id)
        {
            try
            {
            //[-] XEP_DIAGNOSISOBJECTSEX diagObjectById = DBProvider.GetDiagObjectById(id, Vehicle, FFMResolver, getHidden: true);
            //[-] if (diagObjectById != null)
            //[-] {
            //[-] DiagnosticObject diagnosticObject = new DiagnosticObject(diagObjectById, Vehicle, FFMResolver);
            //[-] Log.Info("ISTAModule.__DiagnosticObject()", "found diag object with children title:{0}", diagnosticObject.GetXepDiagnosisObject().Title_dede);
            //[-] ICollection<XEP_DIAGNOSISOBJECTSEX> childDiagObjects = DBProvider.GetChildDiagObjects(diagnosticObject.GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
            //[-] return new DiagnosticObjectLocator(diagnosticObject, childDiagObjects);
            //[-] }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__DiagnosticObject()", exception);
            }

            return null;
        }

        [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
        public PlaceholderType SelectDiagParent(string callingMethod)
        {
            throw new NotImplementedException();
        }

        public abstract InfoObject GetInfoObjStarted();
        [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
        public abstract PlaceholderType SelectDiagParentByAskingUser(IList<PlaceholderType> diag, string callingMethod);
        [PreserveSource(Hint = "IList<XEP_DIAGNOSISOBJECTSEX>", Placeholder = true)]
        private IList<PlaceholderType> FindDiagParents()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(SignatureModified = true)]
        private string LogDiagObj()
        {
            StringBuilder stringBuilder = new StringBuilder("[");
            //[-] foreach (XEP_DIAGNOSISOBJECTSEX item in diag)
            //[-] {
            //[-] stringBuilder.Append("{").Append(LogDiagObj(item)).Append("},");
            //[-] }
            //[-] if (diag.Count > 0)
            //[-] {
            //[-] stringBuilder.Length--;
            //[-] }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        [PreserveSource(SignatureModified = true)]
        private string LogDiagObj(PlaceholderType diag)
        {
            //[-] if (diag == null)
            //[-] {
            //[-] return "null";
            //[-] }
            StringBuilder stringBuilder = new StringBuilder();
            //[-] stringBuilder.Append(diag.Name).Append("/").Append(diag.Id);
            return stringBuilder.ToString();
        }

        public IFaultCodeLocator GetFaultCode(string refCode)
        {
            //[-] FaultCode faultCode = FaultCode.GetFaultCode(refCode, Vehicle, FFMResolver);
            //[-] if (faultCode == null)
            //[-] {
            //[-] Log.Warning("ISTAModule.__FaultCode()", "no fault code found for reference: {0}", refCode);
            //[-] return null;
            //[-] }
            //[-] return new FaultCodeLocator(faultCode, Vehicle, FFMResolver);
            //[+] return null;
            return null;
        }

        [PreserveSource(Hint = "IVirtualFaultCodeLocator replaced", SignatureModified = true)]
        public void GetVirtualFaultCode(string refCode)
        {
        //[-] FaultCode virtualFaultCode = FaultCode.GetVirtualFaultCode(refCode, Vehicle, FFMResolver);
        //[-] if (virtualFaultCode == null)
        //[-] {
        //[-] Log.Warning("ISTAModule.__VirtualFaultCode()", "no virtual fault code found for reference: {0}", refCode);
        //[-] return null;
        //[-] }
        //[-] return new VirtualFaultCodeLocator(virtualFaultCode, Vehicle, GetRootModule());
        }

        [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
        public abstract PlaceholderType GetRootModule();
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IVehiclePartLocator __Part(string refText)
        {
            try
            {
            //[-] XEP_VEHICLEPART xepVehiclePartById = DBProvider.GetXepVehiclePartById(Convert.ToDecimal(refText));
            //[-] if (xepVehiclePartById != null)
            //[-] {
            //[-] return new VehiclePart(xepVehiclePartById);
            //[-] }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__Part()", exception);
            }

            Log.Warning("ISTAModule.__Part()", "unable to locate vehicle part with reference: {0}", refText);
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IVehicleStateLocator __State(string refText)
        {
            //[-] XEP_VEHICLESTATE xepVehicleStateById = DBProvider.GetXepVehicleStateById(refText);
            //[-] if (xepVehicleStateById != null)
            //[-] {
            //[-] return new VehicleStateLocator(xepVehicleStateById);
            //[-] }
            Log.Warning("ISTAModule.__State()", "unable to locate vehicle state with reference: {0}", refText);
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IVehicleAdapterLocator __Adapter(string refText)
        {
            //[-] XEP_VEHICLEADAPTER vehicleAdapterById = DBProvider.GetVehicleAdapterById(refText);
            //[-] if (vehicleAdapterById != null)
            //[-] {
            //[-] return new VehicleAdapterLocator(vehicleAdapterById);
            //[-] }
            Log.Warning("ISTAModule.__Adapter()", "unable to locate adapter with reference: {0}", refText);
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void ClearErrorInfoMemory()
        {
        //[-] IMethodCall methodCall = FastaProtocoler.AddMethodCall("ClearErrorInfoMemory");
        //[-] logic.ClearErrorInfoMemory();
        //[-] methodCall.EndTime = DateTime.Now;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void ReadErrorInfoMemory()
        {
            //[-] IMethodCall methodCall = FastaProtocoler.AddMethodCall("ReadErrorInfoMemory");
            //[-] logic.ReadErrorInfoMemory();
            RunAllServiceProgrammsWithPrefixABLQIC();
        //[-] methodCall.EndTime = DateTime.Now;
        }

        public virtual void RunAllServiceProgrammsWithPrefixABLQIC()
        {
            string text = "ABL-QIC";
            //[-] List<IXepInfoObject> infoObjectListByIdentifierPrefix = DBProvider.GetInfoObjectListByIdentifierPrefix(text, Vehicle, logic.FFMResolver, null);
            //[-] if (infoObjectListByIdentifierPrefix == null || infoObjectListByIdentifierPrefix.Count == 0)
            {
                Log.Error("VehicleIdent.ReadErrorInfoMemory()", "Service program prefix not found: " + text);
                return;
            }
        //[-] foreach (IXepInfoObject item in infoObjectListByIdentifierPrefix)
        //[-] {
        //[-] ParameterContainer inParameters = new ParameterContainer();
        //[-] ParameterContainer outParameters = new ParameterContainer();
        //[-] ParameterContainer inAndOutParameters = new ParameterContainer();
        //[-] callModuleRef(item.ControlId.ToString(), inParameters, ref outParameters, ref inAndOutParameters);
        //[-] }
        }

        public abstract void callModuleRef(string refPath, ParameterContainer inParameters, ref ParameterContainer outParameters, ref ParameterContainer inAndOutParameters);
        [EditorBrowsable(EditorBrowsableState.Always)]
        public virtual IDocumentLocator __Document(string controlId)
        {
            //[-] InfoObject document = logic.Factory.GetDocument(Vehicle, FFMResolver, controlId, isRgNews: false);
            //[-] if (document != null && DBProvider.EvaluateXepRulesById(document.Id, Vehicle, FFMResolver))
            //[-] {
            //[-] Log.Info("ISTAModule.__Document()", "found valid document for controlId: {0} {1}", controlId, document.XepInfoObject.Identifikator);
            //[-] return new DocumentLocator(document);
            //[-] }
            //[-] Log.Info("ISTAModule.__Document()", "no valid document found for controlId: {0}", controlId);
            return null;
        }

        private IList<InfoObject> ExecuteCommandIndirectDocument(string sysName, string infoType, string heading, string callingMethod)
        {
            List<InfoObject> list = new List<InfoObject>();
            if (Vehicle == null)
            {
                Log.Error("ISTAModule.ExecuteCommandIndirectDocument()", "Vehicle is null, thus returning empty document list.");
                return list;
            }

            if (string.IsNullOrEmpty(sysName))
            {
                //[-] XEP_DIAGNOSISOBJECTSEX xEP_DIAGNOSISOBJECTSEX = SelectDiagParent(callingMethod);
                //[-] if (xEP_DIAGNOSISOBJECTSEX == null)
                {
                    Log.Warning("ISTAModule.ExecuteCommandIndirectDocument()", "No Diag parent found, returning empty document list.");
                    return list;
                }

                //[-] sysName = xEP_DIAGNOSISOBJECTSEX.Name;
                //[-] decimal id = xEP_DIAGNOSISOBJECTSEX.Id;
                if (string.IsNullOrEmpty(sysName))
                {
                    foreach (string item in BuildInfoType(infoType))
                    {
                    //[-] list.AddRange(logic.Factory.GetIndirectDocument(Vehicle, id, heading, item, FFMResolver, getHidden: true));
                    }

                    if (!list.Any())
                    {
                    //[-] Log.Warning("ISTAModule.ExecuteCommandIndirectDocument()", "No document found for DiagObj: id={0}, sysName=\"{1}\", heading=\"{2}\"", id, sysName, heading);
                    }
                    else
                    {
                        list = ValuateDocument(list);
                    }
                }
            }

            if (!string.IsNullOrEmpty(sysName))
            {
                foreach (string item2 in BuildInfoType(infoType))
                {
                //[-] list.AddRange(logic.Factory.GetIndirectDocument(Vehicle, sysName, heading, item2, FFMResolver, getHidden: true));
                }

                if (!list.Any())
                {
                    Log.Warning("ISTAModule.ExecuteCommandIndirectDocument()", "No document found for DiagObj: sysName=\"{0}\", infoType=\"{1}\", heading=\"{2}\"", sysName, infoType, heading);
                }
                else
                {
                    list = ValuateDocument(list);
                }
            }

            return list;
        }

        private List<InfoObject> ValuateDocument(List<InfoObject> infoObjects)
        {
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            List<InfoObject> list3 = new List<InfoObject>();
            if (infoObjects == null)
            {
                Log.Warning(Log.CurrentMethod(), "Document is null");
                ShowMessage(FormatedData.Localize("#Note"), FormatedData.Localize("#DocumentViewer.NotFound"));
            }
            //[-] else
            //[-] {
            //[-] foreach (InfoObject infoObject in infoObjects)
            //[-] {
            //[-] if (string.Compare(infoObject.XepInfoObject.DocumentType, "NONE", StringComparison.OrdinalIgnoreCase) == 0)
            //[-] {
            //[-] Log.Warning(Log.CurrentMethod(), "Unable to find document for document identifier: {0}", infoObject.Identifier ?? "");
            //[-] infoObject.State = typeDiagObjectState.Canceled;
            //[-] list.Add(infoObject.Identifier);
            //[-] }
            //[-] else if (!infoObject.IsExternalDocument && (string.IsNullOrEmpty(infoObject.Content.TransformedDocument) || (string.IsNullOrEmpty(infoObject.Content.Doc) && infoObject.Content.BinaryDocument == null)))
            //[-] {
            //[-] Log.Warning(Log.CurrentMethod(), "Selected document has no content for document identifier: {0} UI-culture: {1}", infoObject.Identifier ?? "", ConfigSettings.CurrentUICulture);
            //[-] infoObject.State = typeDiagObjectState.Canceled;
            //[-] list2.Add(infoObject.Identifier);
            //[-] }
            //[-] else
            //[-] {
            //[-] list3.Add(infoObject);
            //[-] }
            //[-] }
            //[-] }

            if (list.Any())
            {
                ShowMessage(FormatedData.Localize("#Note"), FormatedData.Localize("#DocumentViewer.NotFound"));
            }

            if (list2.Any())
            {
                ShowMessage(FormatedData.Localize("#Note"), FormatedData.Localize("#DocumentViewer.NoContent", "ISTAGui", true, string.Join(Environment.NewLine, list2)));
            }

            return list3;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(SignatureModified = true)]
        protected IList<IDocumentLocator> __IndirectDocument(string title, string heading)
        {
            return __IndirectDocument(title, heading, null);
        }

        private IEnumerable<string> BuildInfoType(string infoType)
        {
            if (string.IsNullOrEmpty(infoType))
            {
                return new string[1]
                {
                    string.Empty
                };
            }

            return infoType.Split('|');
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(SignatureModified = true)]
        public IList<IDocumentLocator> __IndirectDocument(string title, string heading, string informationsTyp)
        {
            List<IDocumentLocator> list = new List<IDocumentLocator>();
            List<InfoObject> list2 = new List<InfoObject>();
            Log.Info("ISTAModule.__IndirectDocument()", "Search for indirect document by title \"{0}\",  heading \"{1}\", informationTyp \"{2}\".", title, heading, informationsTyp);
            //[-] InteractionModel interactionModel = new InteractionProgressModel();
            try
            {
                //[-] RegisterAndDeregisterInteractionModel(interactionModel, register: true);
                list2.AddRange(ExecuteCommandIndirectDocument(title, informationsTyp, heading, "__IndirectDocument(string,string,string)"));
                //[-] list.AddRange(list2.Select((InfoObject x) => new DocumentLocator(x)));
                return list;
            }
            finally
            {
            //[-] RegisterAndDeregisterInteractionModel(interactionModel, register: false);
            }
        }

        public abstract void ShowMessage(string title, string message);
        public abstract void RegisterAndDeregisterInteractionModel(InteractionModel interactionModel, bool register);
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IServiceProgramLocator __Program(string refPath)
        {
            //[-] DBProvider.GetInfoObjectsByDiagObjectControlId(Convert.ToDecimal(refPath, CultureInfo.InvariantCulture), Vehicle, FFMResolver, getHidden: true);
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public int? GetFaultCodeSum()
        {
            //[-]return SessionInfoAccessor.SessionInfo.FaultCodeSum;
            //[+] return ClientContext.GetClientContext(Vehicle)?.SessionInfo?.FaultCodeSum;
            return ClientContext.GetClientContext(Vehicle)?.SessionInfo?.FaultCodeSum;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IPerceivedSymptomsLocator", Placeholder = true)]
        public PlaceholderType __PerceivedSymptom(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IEquipmentLocator __Equipment(string refText)
        {
            //[-] XEP_EQUIPMENT equipmentByName = DBProvider.GetEquipmentByName(refText);
            //[-] if (equipmentByName != null)
            //[-] {
            //[-] return new EquipmentLocator(equipmentByName);
            //[-] }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IFaultCodeLocator __FaultCode(string refCode)
        {
            return GetFaultCode(refCode);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ICombinedFaultLocator __CombinedFault(string fC_Id)
        {
            return CombinedFaultNode(fC_Id);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IFaultModeLocator __FaultMode(string refCode)
        {
            decimal id = Convert.ToDecimal(refCode, CultureInfo.InvariantCulture);
            //[-] XEP_FAULTMODELABELS faultModeLabelById = DBProvider.GetFaultModeLabelById(id);
            //[-] if (faultModeLabelById != null)
            //[-] {
            //[-] return new FaultModeLocator(faultModeLabelById);
            //[-] }
            Log.Warning("ISTAModule.__FaultMode()", "no valid faultmode found in database for ref: {0}", refCode);
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IVirtualFaultCodeLocator", Placeholder = true)]
        public PlaceholderType __VirtualFaultCode(string refCode)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(SignatureModified = true)]
        public ICombinedFaultLocator CombinedFaultNode(string fC_Id)
        {
            try
            {
            //[-] return CombinedFaultCode.GetCombinedFaultCode(Convert.ToDecimal(fC_Id, CultureInfo.InvariantCulture), Vehicle, FFMResolver);
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.CombinedFaultNode(string)", exception);
            }

            return null;
        }

        [PreserveSource(SignatureModified = true)]
        public IFaultCodeLocator FaultCodeNode(string sgbd, string variante, string fCode)
        {
            try
            {
            //[-] decimal code = Convert.ToDecimal(fCode, CultureInfo.InvariantCulture);
            //[-] FaultCode faultCodeByCodeAndVariantName = DBProvider.GetFaultCodeByCodeAndVariantName(code, variante, null, Vehicle, FFMResolver);
            //[-] if (faultCodeByCodeAndVariantName != null)
            //[-] {
            //[-] faultCodeByCodeAndVariantName.VehicleContext = Vehicle;
            //[-] }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.FaultCodeNode(string,string,string)", exception);
            }

            return null;
        }

        [PreserveSource(SignatureModified = true)]
        public IFaultCodeLocator FaultCodeNode(string id)
        {
            try
            {
                //[-] decimal id2 = Convert.ToDecimal(id, CultureInfo.InvariantCulture);
                //[-] FaultCode faultCodeById = DBProvider.GetFaultCodeById(id2, Vehicle, FFMResolver);
                //[-] if (faultCodeById != null)
                //[-] {
                //[-] faultCodeById.VehicleContext = Vehicle;
                //[-] return new FaultCodeLocator(faultCodeById, Vehicle, FFMResolver);
                //[-] }
                Log.Warning("ISTAModule.FaultCodeNode()", "Can not find fault code node for id: {0}", id);
                return null;
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.FaultCodeNode()", exception);
            }

            return null;
        }

        public IVirtualFaultCodeLocator VirtualFaultCodeNode(string id)
        {
            try
            {
            //[-] FaultCode virtualFaultCode = FaultCode.GetVirtualFaultCode(id, Vehicle, FFMResolver);
            //[-] if (virtualFaultCode != null)
            //[-] {
            //[-] return new VirtualFaultCodeLocator(virtualFaultCode, logic.VecInfo, GetRootModule());
            //[-] }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.FaultCodeNode()", exception);
            }

            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public IStateListLocator __StateList(string stateListId)
        {
            //[-] return new StateList(stateListId);
            //[+] return null;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ICharacteristicsLocator __Characteristics(string controlId)
        {
            try
            {
            //[-] IXepCharacteristics characteristicById = DBProvider.GetCharacteristicById(Convert.ToInt64(controlId, CultureInfo.InvariantCulture));
            //[-] if (characteristicById != null)
            //[-] {
            //[-] return new CharacteristicsLocator(characteristicById);
            //[-] }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__Characteristics()", exception);
            }

            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(SignatureModified = true)]
        public IEcuGroupLocator __EcuGroup(string groupName)
        {
            //[-] XEP_ECUGROUPS ecuGroupByName = DBProvider.GetEcuGroupByName(groupName);
            //[-] if (ecuGroupByName != null)
            //[-] {
            //[-] return new EcuGroupLocator(ecuGroupByName, Vehicle, FFMResolver);
            //[-] }
            return null;
        }

        [PreserveSource(SignatureModified = true)]
        public IEcuGroupLocator __EcuGroup(decimal ecuGroupId)
        {
            //[-] XEP_ECUGROUPS ecuGroupById = DBProvider.GetEcuGroupById(ecuGroupId);
            //[-] if (ecuGroupById != null)
            //[-] {
            //[-] return new EcuGroupLocator(ecuGroupById, Vehicle, FFMResolver);
            //[-] }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(SignatureModified = true)]
        public IEcuVariantLocator __EcuVariant(string variantName)
        {
            //[-] XEP_ECUVARIANTS ecuVariantByName = DBProvider.GetEcuVariantByName(variantName);
            //[-] if (ecuVariantByName != null)
            //[-] {
            //[-] return new EcuVariantLocator(ecuVariantByName, Vehicle, FFMResolver);
            //[-] }
            return null;
        }

        [PreserveSource(SignatureModified = true)]
        public IEcuVariantLocator __EcuVariant(decimal ecuVariantId)
        {
            //[-] XEP_ECUVARIANTS ecuVariantById = DBProvider.GetEcuVariantById(ecuVariantId);
            //[-] if (ecuVariantById != null)
            //[-] {
            //[-] return new EcuVariantLocator(ecuVariantById, Vehicle, FFMResolver);
            //[-] }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(SignatureModified = true)]
        public IEcuProgrammingVariantLocator __EcuProgrammingVariant(string ecuProgrammingVariant)
        {
            //[-] return EcuProgrammingVariantLocator.CreateEcuProgrammingVariantLocator(ecuProgrammingVariant, Vehicle, FFMResolver);
            //[+] return null;
            return null;
        }

        [PreserveSource(SignatureModified = true)]
        public IEcuProgrammingVariantLocator __EcuProgrammingVariant(decimal ecuProgrammingVariantId)
        {
            //[-] XEP_ECUPROGRAMMINGVARIANT ecuProgrammingVariantById = DBProvider.GetEcuProgrammingVariantById(ecuProgrammingVariantId, Vehicle, FFMResolver);
            //[-] if (ecuProgrammingVariantById != null)
            //[-] {
            //[-] return new EcuProgrammingVariantLocator(ecuProgrammingVariantById, Vehicle, FFMResolver);
            //[-] }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool HasVehicleVariant(string group, string variant)
        {
            if (!new string[4]
            {
                "UNKNOWNGROUP",
                "D_EXX",
                "G_FXX",
                "G_OBD"
            }.Any((string x) => x.Equals(group, StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrEmpty(variant))
                {
                    return Vehicle.ECU.Any((ECU x) => x.VARIANTE != null && x.VARIANTE.Equals(variant, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(group))
                {
                    return Vehicle.getECUbyECU_GRUPPE(group) != null;
                }
            }

            return true;
        }

        public ICombinedFaultLocator GetCombinedFaultCode(string refCode)
        {
            try
            {
            //[-] ICombinedFaultLocator combinedFaultCode = CombinedFaultCode.GetCombinedFaultCode(Convert.ToDecimal(refCode, CultureInfo.InvariantCulture), Vehicle, FFMResolver);
            //[-] if (combinedFaultCode == null)
            //[-] {
            //[-] Log.Warning("ISTAModule.GetCombinedFaultCode()", "no combined fault code found for reference: {0}", refCode);
            //[-] }
            //[-] return combinedFaultCode;
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.GetCombinedFaultCode()", exception);
            }

            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ISPELocator CalledFrom()
        {
            try
            {
                InfoObject infoObjStarted = GetInfoObjStarted();
                if (infoObjStarted == null)
                {
                    Log.Error("ISTAModule.CalledFrom()", "No info object found in module parameter. Returning null.");
                    return null;
                }

                //[-] XEP_DIAGNOSISOBJECTSEX xEP_DIAGNOSISOBJECTSEX = SelectDiagParent("CalledFrom()");
                //[-] if (xEP_DIAGNOSISOBJECTSEX != null)
                //[-] {
                //[-] return new DiagnosticObjectLocator(new DiagnosticObject(xEP_DIAGNOSISOBJECTSEX, Vehicle, FFMResolver));
                //[-] }
                Log.Error("ISTAModule.CalledFrom()", "No diag object found. Returning SPELocator(infoObj.Id).");
                return new SPELocator(infoObjStarted.Id);
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.CalledFrom()", exception);
                return null;
            }
        }

        public abstract void SetProperty(string propertyName, object data);
        public abstract void SetPersistentProperty(string propertyName, object data);
        public abstract void SetAcrossSessionProperty(string propertyName, object data);
        public abstract void SetDealerSessionProperty(string propertyName, string propertyValue);
        public abstract object GetAcrossSessionProperty(string propertyName);
        public abstract object GetPersistentProperty(string propertyName);
        public abstract object GetProperty(string propertyName);
        public abstract T GetPersistentProperty<T>(string propertyName);
        public abstract T GetProperty<T>(string propertyName);
        public abstract string GetDealerSessionProperty(string propertyName);
        public abstract void DoBackgroundProgressbarExecution(Action action);
        public abstract void LogStatement(string headlineValue, params object[] paramList);
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ITextLocator __Text()
        {
            return __Text(string.Empty, null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected ITextLocator __StandardText(decimal value)
        {
            return __StandardText(value, null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected ITextLocator __StandardText(decimal value, __TextParameter[] paramArray)
        {
            return textContentManager.__StandardText(value, paramArray);
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public ITextLocator __Text(string value)
        {
            return __Text(value, null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public ITextLocator TextNode(string value)
        {
            return __Text(value);
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public ITextLocator __Text(string value, __TextParameter[] paramArray)
        {
            return textContentManager.__Text(value, paramArray);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected virtual void __StartStep()
        {
            Log.Info("ISTAModule.__StartStep()", "setting up log container for: {0} Verbose logging: {1}", LastCallingMethod, _VerboseLoopLogs);
            if (FastaGrouping != null)
            {
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText(LastCallingMethod, x)));
                FastaGrouping.CreateSubGroup(GroupingType.Ablaufschritt, list);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected virtual void __FinishStep()
        {
            Log.Info("ISTAModule.__FinishStep()", "finishing log container for: {0} Verbose logging: {1}", LastCallingMethod, _VerboseLoopLogs);
            if (FastaProtocoler != null)
            {
                FastaProtocoler.EndTime = DateTime.Now;
            }
            if (FastaGrouping != null)
            {
                FastaGrouping.EndTime = DateTime.Now;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IXmlElement", Placeholder = true)]
        public PlaceholderType CreateXmlDocument(string xml)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddServiceCodeToProtocol(string name, string value)
        {
            if (_DoLoopHandling)
            {
                if (serviceCodesHandledInLoop.Contains(value))
                {
                    return;
                }

                serviceCodesHandledInLoop.Add(value);
            }
        //[-] FastaProtocoler.AddServiceCode(name, value, LayoutGroup.D);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public void ClearLoopHandledCodeStatements()
        {
            serviceCodesHandledInLoop.Clear();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Cleaned = true)]
        public void __SetSuspiciousItem(IDiagnosticObjectLocator diagObj)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public abstract void AddSuspiciousItemToServiceProgram(IDiagnosticObjectLocator diagObjLocator);
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public void __SetOkItem(IDiagnosticObjectLocator diagObj)
        {
            if (diagObj != null)
            {
                Log.Info("ISTAModule.__SetOkItem()", "adding diag obj id: {0}", diagObj.Id);
                if (!OkItems.Contains(diagObj))
                {
                    OkItems.Add(diagObj);
                }
            }
            else
            {
                Log.Info("ISTAModule.__SetOkItem()", "diag obj was null");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public void __SetNotOkItem(IDiagnosticObjectLocator diagObj)
        {
            if (diagObj != null)
            {
                Log.Info("ISTAModule.__SetNotOkItem()", "adding diag obj id: {0}", diagObj.Id);
                if (!NotOkItems.Contains(diagObj))
                {
                    NotOkItems.Add(diagObj);
                }
            }
            else
            {
                Log.Info("ISTAModule.__SetNotOkItem()", "diag obj was null");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public virtual void __handleOutParameter()
        {
            try
            {
                if (SuspiciuosItems != null && SuspiciuosItems.Count > 0)
                {
                    foreach (IDiagnosticObjectLocator suspiciuosItem in SuspiciuosItems)
                    {
                        Log.Info("ISTAModule.__handleOutParameter()", "set suspicious items: {0}", suspiciuosItem.Id, suspiciuosItem.GetDataValue("NAME"));
                        decimal diagObjectId = Convert.ToDecimal(suspiciuosItem.Id, CultureInfo.InvariantCulture);
                        //[-] XEP_DIAGNOSISOBJECTSEX diagObjectById = DBProvider.GetDiagObjectById(diagObjectId, Vehicle, FFMResolver, getHidden: true);
                        if (suspiciuosItem.Parents != null && suspiciuosItem.Parents.Any())
                        {
                            decimal diagObjectId2 = Convert.ToDecimal(suspiciuosItem.Parents[0].Id, CultureInfo.InvariantCulture);
                        //[-] DBProvider.GetDiagObjectById(diagObjectId2, Vehicle, FFMResolver, getHidden: true);
                        }
                    //[-] if (diagObjectById == null)
                    //[-] {
                    //[-] continue;
                    //[-] }
                    //[-] DiagnosticObject diagnosticObject = new DiagnosticObject(diagObjectById, Vehicle, FFMResolver);
                    //[-] if (diagnosticObject == null)
                    //[-] {
                    //[-] continue;
                    //[-] }
                    //[-] foreach (IXepInfoObject attachedInfoObject in diagnosticObject.GetAttachedInfoObjects())
                    //[-] {
                    //[-] InfoObject infoObject = logic.Factory.CreateInfoObject(attachedInfoObject);
                    //[-] string name = (string.IsNullOrEmpty(attachedInfoObject.Identifikator) ? attachedInfoObject.Title : attachedInfoObject.Identifikator);
                    //[-] infoObject.XepInfoObjectCasted.Name = name;
                    //[-] infoObject.State = typeDiagObjectState.Suspected;
                    //[-] AddSuspicious(infoObject, diagObjectById);
                    //[-] }
                    }
                }

                if (ResultSet.CollectiveResult == CollectiveResultSet.NotOk)
                {
                    foreach (IDiagnosticObjectLocator notOkItem in NotOkItems)
                    {
                        Log.Info("ISTAModule.__handleOutParameter()", "set notOK items: {0}", notOkItem.Id, notOkItem.GetDataValue("NAME"));
                        decimal diagObjectId3 = Convert.ToDecimal(notOkItem.Id, CultureInfo.InvariantCulture);
                    //[-] DiagnosticObject diagnosticObject2 = new DiagnosticObject(DBProvider.GetDiagObjectById(diagObjectId3, Vehicle, FFMResolver, getHidden: true), Vehicle, FFMResolver);
                    //[-] foreach (IXepInfoObject attachedInfoObject2 in diagnosticObject2.GetAttachedInfoObjects())
                    //[-] {
                    //[-] InfoObject infoObject2 = logic.Factory.CreateInfoObject(attachedInfoObject2);
                    //[-] string name2 = (string.IsNullOrEmpty(attachedInfoObject2.Identifikator) ? attachedInfoObject2.Title : attachedInfoObject2.Identifikator);
                    //[-] infoObject2.XepInfoObjectCasted.Name = name2;
                    //[-] AddSuspicious(infoObject2, diagnosticObject2.GetXepDiagnosisObject());
                    //[-] }
                    }
                }

                if (ResultSet.CollectiveResult != CollectiveResultSet.Ok)
                {
                    return;
                }

                foreach (IDiagnosticObjectLocator okItem in OkItems)
                {
                    Log.Info("ISTAModule.__handleOutParameter()", "set OK items: {0}", okItem.Id, okItem.GetDataValue("NAME"));
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__handleOutParameter()", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Cleaned = true)]
        public virtual void AddSuspicious()
        {
        }

        public abstract int ShowQuestionPopup(ITextContent title, ITextContent question, int size = 0, params ITextContent[] buttonTexts);
        public abstract string DeactivateOtdLscCalls();
        [PreserveSource(Hint = "FcFnActivationResult", Placeholder = true)]
        public abstract PlaceholderType StoreAndActivateFcFn(int appNo, int upgradeIdx, byte[] fsc);
    }
}