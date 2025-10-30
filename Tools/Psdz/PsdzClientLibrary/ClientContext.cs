using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PsdzClient.Core;

namespace PsdzClient
{
    public class ClientContext : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientContext));
        private static bool _enablePsdzMultiSession;
        private static bool _enablePsdzWebService;
        private bool _disposed;

        public PsdzDatabase Database { get; set; }
        public UiBrand SelectedBrand { get; set; }
        public string OutletCountry { get; set; }
        public string Language { get; set; }
        public bool ProtectionVehicleService { get; set; }
        public bool IsProblemHandlingTraceRunning { get; set; }

        static ClientContext()
        {
            _enablePsdzMultiSession = false;
            _enablePsdzWebService = false;
            string swiVersion = PsdzDatabase.GetSwiVersion();
            if (!string.IsNullOrEmpty(swiVersion))
            {
                string[] swiParts = swiVersion.Split('.');
                if (swiParts.Length >= 2)
                {
                    if (long.TryParse(swiParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long value1) &&
                        long.TryParse(swiParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long value2))
                    {
                        long version = value1 * 10000 + value2;
                        if (version >= 40039)
                        {
                            _enablePsdzMultiSession = true;
                        }

                        if (version >= 40056)
                        {
                            _enablePsdzWebService = true;
                        }
                    }
                }
            }
        }

        public ClientContext()
        {
            Database = null;
            SelectedBrand = UiBrand.BMWBMWiMINI;
            OutletCountry = string.Empty;
            Language = "En";
            ProtectionVehicleService = true;
        }

        public static ClientContext GetClientContext(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                if (vehicle.ClientContext == null)
                {
                    log.ErrorFormat("GetClientContext ClientContext is null");
                    return null;
                }

                if (vehicle.ClientContext._disposed)
                {
                    log.ErrorFormat("GetClientContext ClientContext is disposed");
                    return null;
                }

                return vehicle.ClientContext;
            }

            log.ErrorFormat("GetClientContext Vehicle is null");
            return null;
        }

        public static PsdzDatabase GetDatabase(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetClientContext ClientContext is null");
                return null;
            }

            if (clientContext.Database == null)
            {
                log.ErrorFormat("GetDatabase Database is null");
                return null;
            }

            return clientContext.Database;
        }

        public static UiBrand GetBrand(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetBrand ClientContext is null");
                return UiBrand.BMWBMWiMINI;
            }

            return clientContext.SelectedBrand;
        }

        public static string GetCountry(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetCountry ClientContext is null");
                return string.Empty;
            }

            if (clientContext.OutletCountry == null)
            {
                log.ErrorFormat("GetCountry OutletCountry is null");
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(clientContext.OutletCountry))
            {
                return clientContext.OutletCountry.ToUpperInvariant();
            }

            return clientContext.Language.ToUpperInvariant();
        }

        public static bool GetProtectionVehicleService(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetProtectionVehicleService ClientContext is null");
                return false;
            }

            return clientContext.ProtectionVehicleService;
        }

        public static string GetLanguage(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetLanguage ClientContext is null");
                return string.Empty;
            }

            if (clientContext.Language == null)
            {
                log.ErrorFormat("GetLanguage Language is null");
                return string.Empty;
            }

            return clientContext.Language;
        }

        public static bool EnablePsdzMultiSession()
        {
            return _enablePsdzMultiSession;
        }

        public static bool EnablePsdzWebService()
        {
            return _enablePsdzWebService;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
