using BMW.Rheingold.Psdz.Model.SecureCoding;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
    internal sealed class SecureCodingConfigWrapper : BackendConnectorProcessor
    {
        private static PsdzSecureCodingConfigCto instance;
        private static readonly object @lock = new object ();
        private PsdzSecureCodingConfigCto SecureCodingConfigCto { get; }

        [PreserveSource(Hint = "errorManager replaced with programmingService", SignatureModified = true)]
        public static PsdzSecureCodingConfigCto GetSecureCodingConfig(ProgrammingService2 programmingService)
        {
            if (instance == null)
            {
                lock (@lock)
                {
                    if (instance == null)
                    {
                        //[-] instance = new SecureCodingConfigWrapper(errorManager).SecureCodingConfigCto;
                        //[+] instance = new SecureCodingConfigWrapper(programmingService).SecureCodingConfigCto;
                        instance = new SecureCodingConfigWrapper(programmingService).SecureCodingConfigCto;
                    }
                }
            }

            return instance;
        }

        [PreserveSource(Hint = "errorManager replaced with programmingService", SignatureModified = true)]
        public static void ChangeNcdRecalculationValueTo(PsdzNcdRecalculationEtoEnum psdzNcdRecalculation, ProgrammingService2 programmingService)
        {
            //[-] string value = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
            //[+] string value = string.Empty;
            string value = string.Empty;
            if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(PsdzNcdRecalculationEtoEnum), value))
            {
                //[-] GetSecureCodingConfig(errorManager).NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
                //[+] GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
                GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
            }
            else
            {
                //[-] GetSecureCodingConfig(errorManager).NcdRecalculationEtoEnum = psdzNcdRecalculation;
                //[+] GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = psdzNcdRecalculation;
                GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = psdzNcdRecalculation;
            }
        }

        [PreserveSource(Hint = "errorManager replaced with programmingService", SignatureModified = true)]
        public static void ChangeBackendNcdCalculationValueTo(PsdzBackendNcdCalculationEtoEnum backendNcdCalculation, ProgrammingService2 programmingService)
        {
            //[-] string configString = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
            //[+] string configString = string.Empty;
            string configString = string.Empty;
            if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
            {
                //[-] GetSecureCodingConfig(errorManager).BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
                //[+] GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
                GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
            }
            else
            {
                //[-] GetSecureCodingConfig(errorManager).BackendNcdCalculationEtoEnum = backendNcdCalculation;
                //[+] GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = backendNcdCalculation;
                GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = backendNcdCalculation;
            }
        }

        [PreserveSource(Hint = "programmingService added", SignatureModified = true)]
        public static string GetSecureCodingPathWithVin(ProgrammingService2 programmingService, string vin)
        {
            //[-] return ScManagerHelper.GetDefaultSecureCodingPathWithVin(vin);
            //[+] return Path.Combine(programmingService.BackupDataPath, NcdRoot, vin);
            return Path.Combine(programmingService.BackupDataPath, NcdRoot, vin);
        }

        [PreserveSource(Hint = "errorManager replaced with programmingService", SignatureModified = true)]
        private SecureCodingConfigWrapper(ProgrammingService2 programmingService)
        {
            SecureCodingConfigCto = new PsdzSecureCodingConfigCto();
            //[-] string configString = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
            //[+] string configString = string.Empty;
            string configString = string.Empty;
            if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
            {
                SecureCodingConfigCto.BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
            }
            else
            {
                SecureCodingConfigCto.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.MUST_NOT;
            }

            SecureCodingConfigCto.BackendSignatureEtoEnum = PsdzBackendSignatureEtoEnum.MUST_NOT;
            //[-] string value = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
            //[+] string value = string.Empty;
            string value = string.Empty;
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
            //[-] SecureCodingConfigCto.NcdRootDirectory = ScManagerHelper.GetDefaultSecureCodingPath();
            //[+] SecureCodingConfigCto.NcdRootDirectory = Path.Combine(programmingService.BackupDataPath, NcdRoot);
            SecureCodingConfigCto.NcdRootDirectory = Path.Combine(programmingService.BackupDataPath, NcdRoot);
            SecureCodingConfigCto.Retries = 3;
            SecureCodingConfigCto.Crls = null;
            SecureCodingConfigCto.SwlSecBackendUrls = null;
            //[-] SecureCodingConfigCto.ScbUrls = new List<string> { GetBackendServiceUrl(BackendServiceType.SecureCoding, errorManager, ContextError.SecureCoding).ResultObject };
            //[+] SecureCodingConfigCto.ScbUrls = new List<string> { GetBackendServiceUrl(BackendServiceType.SecureCoding, ContextError.SecureCoding).ResultObject };
            SecureCodingConfigCto.ScbUrls = new List<string> { GetBackendServiceUrl(BackendServiceType.SecureCoding, ContextError.SecureCoding).ResultObject };
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