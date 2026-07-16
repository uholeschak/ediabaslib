using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PsdzClient;
using PsdzClient.Programming;

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
        [PreserveSource(Hint = "ILogic", Placeholder = true)]
        public PlaceholderType logic;
        private IResult resultSet = new Result();
        [PreserveSource(Hint = "EcuKomProxy", Placeholder = true)]
        private PlaceholderType ecuKomProxy;
        [PreserveSource(Hint = "ISuspicionLinkCounter", Placeholder = true)]
        private PlaceholderType suspicionLinkCount;
        private string _lastCallingMethod = string.Empty;
        [PreserveSource(Hint = "IAppSessionContext", Placeholder = true)]
        private PlaceholderType appSessionContext;
        private bool _doLoopHandling;
        private bool _verboseLoopLog;
        private readonly List<string> serviceCodesHandledInLoop = new List<string>();
        [PreserveSource(Hint = "List<IDiagnosticObjectLocator>", Placeholder = true)]
        public List<PlaceholderType> SuspiciuosItems = new List<PlaceholderType>();
        [PreserveSource(Hint = "List<IDiagnosticObjectLocator>", Placeholder = true)]
        public List<PlaceholderType> OkItems = new List<PlaceholderType>();
        [PreserveSource(Hint = "List<IDiagnosticObjectLocator>", Placeholder = true)]
        public List<PlaceholderType> NotOkItems = new List<PlaceholderType>();
        public abstract ILogger Logger { get; }
        public abstract IProtocolBasic FastaProtocoler { get; }

        [PreserveSource(Hint = "IEcuKomStatement", Placeholder = true)]
        public abstract PlaceholderType EcuKomStatement { get; }
        public abstract IEcuKom ecuKom { get; }
        public abstract IFFMDynamicResolver FFMResolver { get; }

        [PreserveSource(Hint = "IInputListener", Placeholder = true)]
        public abstract PlaceholderType InputListener { get; }

        [PreserveSource(Hint = "IVehicleContext", Placeholder = true)]
        public abstract PlaceholderType VehicleContext { get; }

        [PreserveSource(Hint = "IDealerData", Placeholder = true)]
        public abstract PlaceholderType DealerData { get; }

        [PreserveSource(Hint = "ISOCAccessor", Placeholder = true)]
        public abstract PlaceholderType SOCAccessor { get; }

        [PreserveSource(Hint = "ISOCAccessor", Placeholder = true)]
        public abstract PlaceholderType Contexts { get; }
        public abstract Vehicle Vehicle { get; }

        [PreserveSource(Hint = "IDatabaseProvider", Placeholder = true)]
        public virtual PlaceholderType DBProvider { get; set; }
        protected ITextContentManager textContentManager { get; set; }

        [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
        protected PlaceholderType Me { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Hint = "IFastaGrouping", Placeholder = true)]
        public PlaceholderType FastaGrouping { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Hint = "ILogic", Placeholder = true)]
        public PlaceholderType IstaOperationLogic => logic;

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
        [PreserveSource(Cleaned = true)]
        public IEcuKomApi EcuKomApi
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [PreserveSource(Hint = "ISuspicionLinkCounter", Placeholder = true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public PlaceholderType SuspicionLinkCount
        {
            get
            {
                throw new NotImplementedException();
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

        [PreserveSource(Hint = "IAppSessionContext", Placeholder = true)]
        public virtual PlaceholderType AppSessionContext
        {
            get
            {
                throw new NotImplementedException();
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
        [PreserveSource(Hint = "IModule", Placeholder = true)]
        public abstract PlaceholderType ModuleData { get; }

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
        [PreserveSource(Hint = "IVehicleContext", Placeholder = true)]
        public PlaceholderType GetVehicleContext()
        {
            return VehicleContext;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(Hint = "IDiagnosticObjectLocator", Placeholder = true)]
        public PlaceholderType __DiagnosticObject(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(Hint = "IDiagnosticObjectLocator", Placeholder = true)]
        public PlaceholderType __DiagnosticObject(decimal id)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
        public PlaceholderType SelectDiagParent(string callingMethod)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "InfoObject", Placeholder = true)]
        public abstract PlaceholderType GetInfoObjStarted();
        [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
        public abstract PlaceholderType SelectDiagParentByAskingUser(IList<PlaceholderType> diag, string callingMethod);
        [PreserveSource(Hint = "IList<XEP_DIAGNOSISOBJECTSEX>", Placeholder = true)]
        private IList<PlaceholderType> FindDiagParents()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        private string LogDiagObj()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        private string LogDiagObj(PlaceholderType diag)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IFaultCodeLocator", Placeholder = true)]
        public PlaceholderType GetFaultCode(string refCode)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IVirtualFaultCodeLocator", Placeholder = true)]
        public PlaceholderType GetVirtualFaultCode(string refCode)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
        public abstract PlaceholderType GetRootModule();
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IVehiclePartLocator", Placeholder = true)]
        public PlaceholderType __Part(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IVehicleStateLocator", Placeholder = true)]
        public PlaceholderType __State(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IVehicleAdapterLocator", Placeholder = true)]
        public PlaceholderType __Adapter(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Cleaned = true)]
        public virtual void ClearErrorInfoMemory()
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Cleaned = true)]
        public virtual void ReadErrorInfoMemory()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public virtual void RunAllServiceProgrammsWithPrefixABLQIC()
        {
        }

        public abstract void callModuleRef(string refPath, ParameterContainer inParameters, ref ParameterContainer outParameters, ref ParameterContainer inAndOutParameters);
        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(Hint = "IDocumentLocator", Placeholder = true)]
        public virtual PlaceholderType __Document(string controlId)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IList<InfoObject>", Placeholder = true)]
        private IList<PlaceholderType> ExecuteCommandIndirectDocument(string sysName, string infoType, string heading, string callingMethod)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "List<InfoObject>", Placeholder = true)]
        private List<PlaceholderType> ValuateDocument(List<PlaceholderType> infoObjects)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(Hint = "IList<IDocumentLocator>", Placeholder = true)]
        protected IList<PlaceholderType> __IndirectDocument(string title, string heading)
        {
            throw new NotImplementedException();
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
        [PreserveSource(Hint = "IList<IDocumentLocator>", Placeholder = true)]
        public IList<PlaceholderType> __IndirectDocument(string title, string heading, string informationsTyp)
        {
            throw new NotImplementedException();
        }

        public abstract void ShowMessage(string title, string message);
        public abstract void RegisterAndDeregisterInteractionModel(InteractionModel interactionModel, bool register);
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Hint = "IServiceProgramLocator", Placeholder = true)]
        public PlaceholderType __Program(string refPath)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Cleaned = true)]
        public int? GetFaultCodeSum()
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IPerceivedSymptomsLocator", Placeholder = true)]
        public PlaceholderType __PerceivedSymptom(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IEquipmentLocator", Placeholder = true)]
        public PlaceholderType __Equipment(string refText)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IFaultCodeLocator", Placeholder = true)]
        public PlaceholderType __FaultCode(string refCode)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "ICombinedFaultLocator", Placeholder = true)]
        public PlaceholderType __CombinedFault(string fC_Id)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IFaultModeLocator", Placeholder = true)]
        public PlaceholderType __FaultMode(string refCode)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IVirtualFaultCodeLocator", Placeholder = true)]
        public PlaceholderType __VirtualFaultCode(string refCode)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "ICombinedFaultLocator", Placeholder = true)]
        public PlaceholderType CombinedFaultNode(string fC_Id)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IFaultCodeLocator", Placeholder = true)]
        public PlaceholderType __FaultCode(string sgbd, string variante, string fCode)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IFaultCodeLocator", Placeholder = true)]
        public PlaceholderType FaultCodeNode(string id)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IVirtualFaultCodeLocator", Placeholder = true)]
        public PlaceholderType VirtualFaultCodeNode(string id)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [PreserveSource(Hint = "IStateListLocator", Placeholder = true)]
        public PlaceholderType __StateList(string stateListId)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "ICharacteristicsLocator", Placeholder = true)]
        public PlaceholderType __Characteristics(string controlId)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Cleaned = true)]
        public IEcuGroupLocator __EcuGroup(string groupName)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public IEcuGroupLocator __EcuGroup(decimal ecuGroupId)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Cleaned = true)]
        public IEcuVariantLocator __EcuVariant(string variantName)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public IEcuVariantLocator __EcuVariant(decimal ecuVariantId)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Hint = "IEcuProgrammingVariantLocator", Placeholder = true)]
        public PlaceholderType __EcuProgrammingVariant(string ecuProgrammingVariant)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IEcuProgrammingVariantLocator", Placeholder = true)]
        public PlaceholderType __EcuProgrammingVariant(decimal ecuProgrammingVariantId)
        {
            throw new NotImplementedException();
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

        [PreserveSource(Hint = "ICombinedFaultLocator", Placeholder = true)]
        public PlaceholderType GetCombinedFaultCode(string refCode)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [PreserveSource(Cleaned = true)]
        public ISPELocator CalledFrom()
        {
            throw new NotImplementedException();
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
        [PreserveSource(Cleaned = true)]
        protected virtual void __StartStep()
        {
            Log.Info("ISTAModule.__StartStep()", "setting up log container for: {0} Verbose logging: {1}", LastCallingMethod, _VerboseLoopLogs);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Cleaned = true)]
        protected virtual void __FinishStep()
        {
            Log.Info("ISTAModule.__FinishStep()", "finishing log container for: {0} Verbose logging: {1}", LastCallingMethod, _VerboseLoopLogs);
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
        [PreserveSource(Hint = "IDiagnosticObjectLocator", Placeholder = true)]
        public void __SetSuspiciousItem(PlaceholderType diagObj)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IDiagnosticObjectLocator", Placeholder = true)]
        public abstract void AddSuspiciousItemToServiceProgram(PlaceholderType diagObjLocator);
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Hint = "IDiagnosticObjectLocator", Placeholder = true)]
        public void __SetOkItem(PlaceholderType diagObj)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Hint = "IDiagnosticObjectLocator", Placeholder = true)]
        public void __SetNotOkItem(PlaceholderType diagObj)
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [PreserveSource(Cleaned = true)]
        public virtual void __handleOutParameter()
        {
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