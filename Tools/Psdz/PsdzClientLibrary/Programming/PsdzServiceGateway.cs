using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz;
using System.Threading.Tasks;
using System;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    public class PsdzServiceGateway : IPsdzServiceGateway, IDisposable
    {
        private readonly PsdzWebServiceWrapper _psdzWebServiceWrapper;
        public static Type PsdzServiceType { get; set; }

        [PreserveSource(Hint = "Added service host", OriginalHash = "C60FDDC78BCC55F8280E56E617864EA9")]
        public IPsdz Psdz
        {
            get
            {
                // [UH] [IGNORE] modified
                if (_psdzWebServiceWrapper != null)
                {
                    return _psdzWebServiceWrapper;
                }

                return _psdzServiceHostWrapper;
            }
        }

        [PreserveSource(Hint = "Added service host", OriginalHash = "FAEBB11719E72CAFA7CE7407E9C005F5")]
        public string PsdzWebServiceLogFilePath
        {
            get
            {
                // [UH] [IGNORE] modified
                if (_psdzWebServiceWrapper != null)
                {
                    return _psdzWebServiceWrapper.PsdzServiceLogFilePath;
                }

                return _psdzServiceHostWrapper.PsdzServiceLogFilePath;
            }
        }

        [PreserveSource(Hint = "Added service host", OriginalHash = "9FA19EABF011B967C0DB531E9CEA9906")]
        public string PsdzLogFilePath
        {
            get
            {
                // [UH] [IGNORE] modified
                if (_psdzWebServiceWrapper != null)
                {
                    return _psdzWebServiceWrapper.PsdzLogFilePath;
                }

                return _psdzServiceHostWrapper.PsdzLogFilePath;
            }
        }

        [PreserveSource(Hint = "istaFolder, dealerId, service host added", OriginalHash = "5684E9EC27BEDFC1149E71FFCABB5D41")]
        public PsdzServiceGateway(PsdzConfig psdzConfig, string istaFolder, string dealerId, Action psdzServiceHostStarter = null)
        {
            _psdzServiceHostStarter = psdzServiceHostStarter;
            // [UH] [IGNORE] modified
            if (ClientContext.EnablePsdzWebService())
            {
                _psdzWebServiceWrapper = new PsdzWebServiceWrapper(new PsdzWebServiceConfig(istaFolder, dealerId), istaFolder);
            }
            else
            {
                _psdzServiceHostWrapper = new PsdzServiceWrapper(psdzConfig);
            }
        }

        [PreserveSource(Hint = "Return bool, service host added", OriginalHash = "3611706C96D5401E249248D5C98E1450")]
        public bool StartIfNotRunning(IVehicle vehicle = null)
        {
            if (PsdzStarterGuard.Instance.IsInitializationAlreadyAttempted())
            {
                Log.Debug(Log.CurrentMethod(), "There has already been an attempt to open PsdzService in the past. Returning...");
                return true;
            }

            Log.Info(Log.CurrentMethod(), "Start.");
            bool started = false;
            if (_psdzWebServiceWrapper != null)
            {
                started = PsdzStarterGuard.Instance.TryInitialize(delegate
                {
                    _psdzWebServiceWrapper.StartIfNotRunning();
                    return _psdzWebServiceWrapper.IsPsdzInitialized;
                });
            }

            if (_psdzServiceHostWrapper != null)
            {
                if (ConfigSettings.GetActivateSdpOnlinePatch() || _psdzServiceHostStarter == null)
                {
                    started = PsdzStarterGuard.Instance.TryInitialize(delegate
                    {
                        _psdzServiceHostWrapper.StartHostIfNotRunning(vehicle);
                        WaitForPsdzServiceHostInitialization();
                        return _psdzServiceHostWrapper.IsPsdzInitialized;
                    });
                }
                else
                {
                    started = PsdzStarterGuard.Instance.TryInitialize(delegate
                    {
                        _psdzServiceHostStarter();
                        WaitForPsdzServiceHostInitialization();
                        return _psdzServiceHostWrapper.IsPsdzInitialized;
                    });
                }
            }

            Log.Info(Log.CurrentMethod(), "Started: {0}", started);
            Log.Info(Log.CurrentMethod(), "End.");
            return started;
        }

        [PreserveSource(Hint = "Added service host", OriginalHash = "B16B1A32BB80FEB5DE2E576D47AFBE40")]
        public void CloseConnectionsToPsdz(bool force = false)
        {
            try
            {
                if (_psdzWebServiceWrapper != null)
                {
                    _psdzWebServiceWrapper.Shutdown();
                }

                if (_psdzServiceHostWrapper != null)
                {
                    if (ConfigSettings.GetActivateSdpOnlinePatch() || force)
                    {
                        _psdzServiceHostWrapper.Shutdown();
                    }
                    else
                    {
                        _psdzServiceHostWrapper.CloseConnectionsToPsdzHost();
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        [PreserveSource(Hint = "Added")]
        public void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel)
        {
            _psdzServiceHostWrapper?.SetLogLevel(psdzLoglevel, prodiasLoglevel);
            _psdzWebServiceWrapper?.SetLogLevel(psdzLoglevel, prodiasLoglevel);
        }

        [PreserveSource(Hint = "Added")]
        public void Shutdown()
        {
            _psdzServiceHostWrapper?.Shutdown();
            _psdzWebServiceWrapper?.Shutdown();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        [PreserveSource(Hint = "Added")]
        private PsdzServiceWrapper _psdzServiceHostWrapper;
        [PreserveSource(Hint = "Added")]
        private readonly Action _psdzServiceHostStarter;
        [PreserveSource(Hint = "Added")]
        public string PsdzServiceLogDir
        {
            get
            {
                if (_psdzWebServiceWrapper != null)
                {
                    return _psdzWebServiceWrapper.PsdzServiceLogDir;
                }

                return _psdzServiceHostWrapper.PsdzServiceLogDir;
            }
        }

        [PreserveSource(Hint = "Added")]
        private bool WaitForPsdzServiceHostInitialization()
        {
            if (_psdzServiceHostWrapper == null)
            { // [UH] [IGNORE] added
                Log.Error(Log.CurrentMethod(), $"_psdzServiceHostWrapper is null");
                return false;
            }

            int num = 40;
            DateTime dateTime = DateTime.Now.AddSeconds(num);
            while (!_psdzServiceHostWrapper.IsPsdzInitialized)
            {
                if (DateTime.Now > dateTime)
                {
                    Log.Error(Log.CurrentMethod(), $"PsdzServiceHost failed to start in {num} seconds. The method will stop waiting for it.");
                    return false;
                }

                Task.Delay(500).Wait();
            }

            _psdzServiceHostWrapper.DoInitSettings();
            return true;
        }
    }
}