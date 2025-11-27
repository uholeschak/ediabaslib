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
    internal sealed class SecureCodingConfigWrapper : BackendConnectorProcessor
    {
        private static PsdzSecureCodingConfigCto instance;
        private static readonly object @lock = new object ();
        private PsdzSecureCodingConfigCto SecureCodingConfigCto { get; }

        [PreserveSource(Hint = "using programmingService")]
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

        [PreserveSource(Hint = "using programmingService")]
        public static void ChangeNcdRecalculationValueTo(PsdzNcdRecalculationEtoEnum psdzNcdRecalculation, ProgrammingService2 programmingService)
        {
            string value = string.Empty; // [IGNORE] ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
            if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(PsdzNcdRecalculationEtoEnum), value))
            {
                GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
            }
            else
            {
                GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = psdzNcdRecalculation;
            }
        }

        [PreserveSource(Hint = "using programmingService")]
        public static void ChangeBackendNcdCalculationValueTo(PsdzBackendNcdCalculationEtoEnum backendNcdCalculation, ProgrammingService2 programmingService)
        {
            string configString = string.Empty; // [IGNORE] ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
            if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
            {
                GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
            }
            else
            {
                GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = backendNcdCalculation;
            }
        }

        [PreserveSource(Hint = "programmingService added")]
        public static string GetSecureCodingPathWithVin(ProgrammingService2 programmingService, string vin)
        {
            return Path.Combine(programmingService.BackupDataPath, NcdRoot, vin);
        }

        [PreserveSource(Hint = "using programmingService")]
        private SecureCodingConfigWrapper(ProgrammingService2 programmingService)
        {
            this.SecureCodingConfigCto = new PsdzSecureCodingConfigCto();
            string configString = string.Empty; // [IGNORE] ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
            if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
            {
                SecureCodingConfigCto.BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
            }
            else
            {
                SecureCodingConfigCto.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.MUST_NOT;
            }

            SecureCodingConfigCto.BackendSignatureEtoEnum = PsdzBackendSignatureEtoEnum.MUST_NOT;
            string value = string.Empty; // [IGNORE] ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
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
            SecureCodingConfigCto.NcdRootDirectory = Path.Combine(programmingService.BackupDataPath, NcdRoot); // [UH] [IGNORE] replaced
            SecureCodingConfigCto.Retries = 3;
            SecureCodingConfigCto.Crls = null;
            SecureCodingConfigCto.SwlSecBackendUrls = null;
            SecureCodingConfigCto.ScbUrls = new List<string>
            {
                string.Empty // [UH] [IGNORE] URL removed
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

        [PreserveSource(Hint = "Added")]
        public const string NcdRoot = "ncd";
    }
}