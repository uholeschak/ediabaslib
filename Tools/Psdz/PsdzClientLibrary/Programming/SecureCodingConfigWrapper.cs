using BMW.Rheingold.Psdz.Model.SecureCoding;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
	public class SecureCodingConfigWrapper
    {
        public const string NcdRoot = "ncd";

        private static PsdzSecureCodingConfigCto instance;

        private static readonly object @lock = new object();

        private PsdzSecureCodingConfigCto SecureCodingConfigCto { get; }

        // [UH] using programmingService
        public static PsdzSecureCodingConfigCto GetSecureCodingConfig(ProgrammingService2 programmingService)
        {
            if (instance == null)
            {
                lock (@lock)
                {
                    if (instance == null)
                    {
                        instance = new SecureCodingConfigWrapper(programmingService).SecureCodingConfigCto;
                    }
                }
            }
            return instance;
        }

        // [UH] using programmingService
        public static void ChangeNcdRecalculationValueTo(PsdzNcdRecalculationEtoEnum psdzNcdRecalculation, ProgrammingService2 programmingService)
        {
            string value = string.Empty;// ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
            if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(PsdzNcdRecalculationEtoEnum), value))
            {
                GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
            }
            else
            {
                GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = psdzNcdRecalculation;
            }
        }

        // [UH] using programmingService
        public static void ChangeBackendNcdCalculationValueTo(PsdzBackendNcdCalculationEtoEnum backendNcdCalculation, ProgrammingService2 programmingService)
        {
            string configString = string.Empty;// ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
            if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
            {
                GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
            }
            else
            {
                GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = backendNcdCalculation;
            }
        }

        // [UH] programmingService added
        public static string GetSecureCodingPathWithVin(ProgrammingService2 programmingService, string vin)
        {
            return Path.Combine(programmingService.BackupDataPath, NcdRoot, vin);
        }

        private SecureCodingConfigWrapper(ProgrammingService2 programmingService)
        {
            this.SecureCodingConfigCto = new PsdzSecureCodingConfigCto();
            string configString = string.Empty; //ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
            if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
            {
                SecureCodingConfigCto.BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
            }
            else
            {
                SecureCodingConfigCto.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.MUST_NOT;
            }
            SecureCodingConfigCto.BackendSignatureEtoEnum = PsdzBackendSignatureEtoEnum.MUST_NOT;
            string value = string.Empty;// ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
            if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(PsdzNcdRecalculationEtoEnum), value))
            {
                SecureCodingConfigCto.NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
            }
            else
            {
                SecureCodingConfigCto.NcdRecalculationEtoEnum = PsdzNcdRecalculationEtoEnum.ALLOW;
            }
            SecureCodingConfigCto.ConnectionTimeout = 5000;
            SecureCodingConfigCto.ScbPollingTimeout = 120;
            SecureCodingConfigCto.NcdRootDirectory = Path.Combine(programmingService.BackupDataPath, NcdRoot);	// [uH] replaced
            SecureCodingConfigCto.Retries = 3;
            SecureCodingConfigCto.Crls = null;
            SecureCodingConfigCto.SwlSecBackendUrls = null;
            SecureCodingConfigCto.ScbUrls = new List<string>
            {
                string.Empty    // [UH] URL removed
                /*GetBackendServiceUrl(BackendServiceType.SecureCoding, errorManager, ContextError.SecureCoding).ResultObject */
            };
            SecureCodingConfigCto.PsdzAuthenticationTypeEto = PsdzAuthenticationTypeEto.SSL;
        }

        internal static void LogSettings()
        {
            if (instance != null)
            {
                Log.Info("SecureCodingConfigWrapper.LogSettings()", $"BackendNcdCalculationEtoEnum: {instance.BackendNcdCalculationEtoEnum:G}");
                Log.Info("SecureCodingConfigWrapper.LogSettings()", $"BackendSignatureEtoEnum: {instance.BackendSignatureEtoEnum:G}");
                Log.Info("SecureCodingConfigWrapper.LogSettings()", $"NcdRecalculationEtoEnum: {instance.NcdRecalculationEtoEnum:G}");
                Log.Info("SecureCodingConfigWrapper.LogSettings()", $"ConnectionTimeout: {instance.ConnectionTimeout:D}");
                Log.Info("SecureCodingConfigWrapper.LogSettings()", $"ScbPollingTimeout: {instance.ScbPollingTimeout:D}");
                Log.Info("SecureCodingConfigWrapper.LogSettings()", "NcdRootDirectory: " + instance.NcdRootDirectory);
                Log.Info("SecureCodingConfigWrapper.LogSettings()", $"Retries: {instance.Retries:D}");
                Log.Info("SecureCodingConfigWrapper.LogSettings()", "Crls: " + ConvertListToString(instance.Crls));
                Log.Info("SecureCodingConfigWrapper.LogSettings()", "SwlSecBackendUrls: " + ConvertListToString(instance.SwlSecBackendUrls));
                Log.Info("SecureCodingConfigWrapper.LogSettings()", "ScbUrls: " + ConvertListToString(instance.ScbUrls));
            }
        }

        private static string ConvertListToString(IList<string> list)
        {
            if (list == null || !list.Any())
            {
                return string.Empty;
            }
            return string.Join(", ", list.ToArray());
        }
	}
}
