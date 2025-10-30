using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz;
using System.Threading.Tasks;
using System;
using PsdzClient;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    public class PsdzServiceGateway : IPsdzServiceGateway, IDisposable
    {
        public enum Type
        {
            PsdzServiceHost,
            PsdzWebService
        }

        private readonly PsdzConfig _psdzConfig;

        private readonly PsdzServiceWrapper _psdzServiceHostWrapper;

        private readonly PsdzWebServiceWrapper _psdzWebServiceWrapper;

        private readonly Action _psdzServiceHostStarter;

        private bool disposedValue;

        public static Type PsdzServiceType { get; set; }

        // [UH] added
        public string PsdzServiceLogDir
        {
            get
            {
                if (PsdzServiceType != Type.PsdzServiceHost)
                {
                    return _psdzWebServiceWrapper.PsdzServiceLogDir;
                }
                return _psdzServiceHostWrapper.PsdzServiceLogDir;
            }
        }

        public IPsdz Psdz
        {
            get
            {
                if (PsdzServiceType != Type.PsdzServiceHost)
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
                if (PsdzServiceType != Type.PsdzServiceHost)
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
                if (PsdzServiceType != Type.PsdzServiceHost)
                {
                    return _psdzWebServiceWrapper.PsdzLogFilePath;
                }
                return _psdzServiceHostWrapper.PsdzLogFilePath;
            }
        }

        // [UH] istaFolder, dealerId added
        public PsdzServiceGateway(PsdzConfig psdzConfig, string istaFolder, string dealerId, Action psdzServiceHostStarter = null)
        {
            _psdzServiceHostStarter = psdzServiceHostStarter;
            _psdzConfig = psdzConfig;
            _psdzServiceHostWrapper = new PsdzServiceWrapper(_psdzConfig);
            _psdzWebServiceWrapper = new PsdzWebServiceWrapper(new PsdzWebServiceConfig(istaFolder, dealerId), istaFolder);
            PsdzServiceType = PsdzClient.ClientContext.EnablePsdzWebService() ? Type.PsdzWebService : Type.PsdzServiceHost;
        }

        public bool StartIfNotRunning(IVehicle vehicle = null)
        {
            if (PsdzStarterGuard.Instance.IsInitializationAlreadyAttempted())
            {
                Log.Debug(Log.CurrentMethod(), "There has already been an attempt to open PsdzService in the past. Returning...");
                return true;
            }
            Log.Info(Log.CurrentMethod(), "Start.");

            bool started;
            if (PsdzServiceType != Type.PsdzServiceHost)
            {
                started = PsdzStarterGuard.Instance.TryInitialize(delegate
                {
                    _psdzWebServiceWrapper.StartIfNotRunning();
                    return _psdzWebServiceWrapper.IsPsdzInitialized;
                });
            }
            else
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

        public void CloseConnectionsToPsdz(bool force = false)
        {
            try
            {
                if (PsdzServiceType != Type.PsdzServiceHost)
                {
                    _psdzWebServiceWrapper.Shutdown();
                }
                if (ConfigSettings.GetActivateSdpOnlinePatch() || force)
                {
                    _psdzServiceHostWrapper.Shutdown();
                }
                else
                {
                    _psdzServiceHostWrapper.CloseConnectionsToPsdzHost();
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private bool WaitForPsdzServiceHostInitialization()
        {
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
            _psdzServiceHostWrapper.SetLogLevel(psdzLoglevel, prodiasLoglevel);
            _psdzWebServiceWrapper.SetLogLevel(psdzLoglevel, prodiasLoglevel);
        }

        public void Shutdown()
        {
            _psdzServiceHostWrapper.Shutdown();
            _psdzWebServiceWrapper.Shutdown();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _psdzServiceHostWrapper.Dispose();
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