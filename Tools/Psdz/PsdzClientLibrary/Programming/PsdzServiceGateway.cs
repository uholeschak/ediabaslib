using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz;
using PsdzClient.Programming;
using System.Threading.Tasks;
using System;
using PsdzClient;
using PsdzClient.Core;

namespace PsdzClientLibrary.Programming
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

        //private readonly PsdzWebServiceWrapper _psdzWebServiceWrapper;

        private readonly Action _psdzServiceHostStarter;

        private bool disposedValue;

        public static Type PsdzServiceType { get; set; }

        public IPsdz Psdz
        {
            get
            {
#if false
                if (PsdzServiceType != 0)
                {
                    return _psdzWebServiceWrapper;
                }
#endif
                return _psdzServiceHostWrapper;
            }
        }

        public string PsdzServiceLogFilePath
        {
            get
            {
#if false
                if (PsdzServiceType != 0)
                {
                    return _psdzWebServiceWrapper.PsdzServiceLogFilePath;
                }
#endif
                return _psdzServiceHostWrapper.PsdzServiceLogFilePath;
            }
        }

        public string PsdzLogFilePath
        {
            get
            {
#if false
                if (PsdzServiceType != 0)
                {
                    return _psdzWebServiceWrapper.PsdzLogFilePath;
                }
#endif
                return _psdzServiceHostWrapper.PsdzLogFilePath;
            }
        }

        public PsdzServiceGateway(PsdzConfig psdzConfig = null, Action psdzServiceHostStarter = null)
        {
            _psdzServiceHostStarter = psdzServiceHostStarter;
            _psdzConfig = psdzConfig;
            _psdzServiceHostWrapper = new PsdzServiceWrapper(_psdzConfig);
            //_psdzWebServiceWrapper = new PsdzWebServiceWrapper(new PsdzWebServiceConfig(null, BMW.Rheingold.CoreFramework.LicenseHelper.DealerInstance.GetDistributionPartnerNumber(5)));
            PsdzServiceType = ((ConfigSettings.getConfigString("BMW.Rheingold.Programming.PsdzServiceType", "PsdzServiceHost") == "PsdzWebService") ? Type.PsdzWebService : Type.PsdzServiceHost);
        }

        public bool StartIfNotRunning(IVehicle vehicle = null)
        {
            Log.Info(Log.CurrentMethod(), "Start.");
#if false
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.PsdzWebservice.Enabled", defaultValue: false))
            {
                _psdzWebServiceWrapper.StartIfNotRunning();
                Log.Info(Log.CurrentMethod(), "End.");
                return true;
            }
#endif
            if (ClientContext.EnablePsdzMultiSession())
            {
                _psdzServiceHostWrapper.StartHostIfNotRunning(vehicle);
            }
            else if (_psdzServiceHostStarter != null)
            {
                _psdzServiceHostStarter();
            }
            else
            {
                _psdzServiceHostWrapper.StartHostIfNotRunning(vehicle);
            }

            if (!WaitForPsdzServiceHostInitialization())
            {
                return false;
            }
            Log.Info(Log.CurrentMethod(), "End.");

            return true;
        }

        public void CloseConnectionsToPsdz(bool force = false)
        {
            try
            {
#if false
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.PsdzWebservice.Enabled", defaultValue: false))
                {
                    _psdzWebServiceWrapper.Shutdown();
                }
#endif
                if (!ClientContext.EnablePsdzMultiSession() || force)
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
            //_psdzWebServiceWrapper.SetLogLevel(psdzLoglevel, prodiasLoglevel);
        }

        public void Shutdown()
        {
            _psdzServiceHostWrapper.Shutdown();
            //_psdzWebServiceWrapper.Shutdown();
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