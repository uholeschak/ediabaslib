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
        [PreserveSource(Hint = "Added")]
        private PsdzServiceWrapper _psdzServiceHostWrapper;

        private PsdzWebServiceWrapper _psdzWebServiceWrapper;

        [PreserveSource(Hint = "Added")]
        private readonly Action _psdzServiceHostStarter;

        private bool disposedValue;

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

        [PreserveSource(Hint = "Modified")]
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

        [PreserveSource(Hint = "istaFolder, dealerId added")]
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

        [PreserveSource(Hint = "Modified")]
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

        [PreserveSource(Hint = "Modified")]
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
        private bool WaitForPsdzServiceHostInitialization()
        {
            if (_psdzServiceHostWrapper == null)
            {   // [UH] [IGNORE] added
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

        public void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel)
        {
            _psdzServiceHostWrapper?.SetLogLevel(psdzLoglevel, prodiasLoglevel);
            _psdzWebServiceWrapper?.SetLogLevel(psdzLoglevel, prodiasLoglevel);
        }

        public void Shutdown()
        {
            _psdzServiceHostWrapper?.Shutdown();
            _psdzWebServiceWrapper?.Shutdown();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_psdzServiceHostWrapper != null)
                    {
                        _psdzServiceHostWrapper.Dispose();
                        _psdzServiceHostWrapper = null;
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}