using BMW.Rheingold.Psdz.Model.SecureCoding;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
	public class SecureCodingConfigWrapper
    {
        public const string NcdRoot = "ncd";

        private static PsdzSecureCodingConfigCto instance;

        private static readonly object @lock = new object();

        private PsdzSecureCodingConfigCto SecureCodingConfigCto { get; }

        // [UH] error manager replaced
        public static PsdzSecureCodingConfigCto GetSecureCodingConfig(ProgrammingService programmingService)
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

        public static void ChangeNcdRecalculationValueTo(ProgrammingService programmingService, PsdzNcdRecalculationEtoEnum psdzNcdRecalculation)
		{
#if false
			string value = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
			if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(PsdzNcdRecalculationEtoEnum), value))
			{
				SecureCodingConfigWrapper.GetSecureCodingConfig().NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
				return;
			}
#endif
			SecureCodingConfigWrapper.GetSecureCodingConfig(programmingService).NcdRecalculationEtoEnum = psdzNcdRecalculation;
		}

        public static void ChangeBackendNcdCalculationValueTo(ProgrammingService programmingService, PsdzBackendNcdCalculationEtoEnum backendNcdCalculation)
		{
#if false
			string configString = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
			if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
			{
				SecureCodingConfigWrapper.GetSecureCodingConfig().BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
				return;
			}
#endif
			SecureCodingConfigWrapper.GetSecureCodingConfig(programmingService).BackendNcdCalculationEtoEnum = backendNcdCalculation;
		}

		public static string GetSecureCodingPathWithVin(ProgrammingService programmingService, string vin)
		{
			return Path.Combine(programmingService.BackupDataPath, NcdRoot, vin);
		}

		private SecureCodingConfigWrapper(ProgrammingService programmingService)
		{
			this.SecureCodingConfigCto = new PsdzSecureCodingConfigCto();
#if false
			if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.Security.SC.UseOnlineNCD", false))
			{
				string configString = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.BackendNcdCalculationMode");
				if (!string.IsNullOrEmpty(configString) && Enum.IsDefined(typeof(PsdzBackendNcdCalculationEtoEnum), configString))
				{
					this.SecureCodingConfigCto.BackendNcdCalculationEtoEnum = (PsdzBackendNcdCalculationEtoEnum)Enum.Parse(typeof(PsdzBackendNcdCalculationEtoEnum), configString);
				}
				else
				{
					this.SecureCodingConfigCto.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.MUST_NOT;
				}
				this.SecureCodingConfigCto.BackendSignatureEtoEnum = PsdzBackendSignatureEtoEnum.MUST_NOT;
				string value = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.NcdRecalculationEnum").ToUpper();
				if (!string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(PsdzNcdRecalculationEtoEnum), value))
				{
					this.SecureCodingConfigCto.NcdRecalculationEtoEnum = (PsdzNcdRecalculationEtoEnum)Enum.Parse(typeof(PsdzNcdRecalculationEtoEnum), value);
				}
				else
				{
					this.SecureCodingConfigCto.NcdRecalculationEtoEnum = PsdzNcdRecalculationEtoEnum.ALLOW;
				}
			}
			else
#endif
			{
				this.SecureCodingConfigCto.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.MUST_NOT;
				this.SecureCodingConfigCto.BackendSignatureEtoEnum = PsdzBackendSignatureEtoEnum.MUST_NOT;
				this.SecureCodingConfigCto.NcdRecalculationEtoEnum = PsdzNcdRecalculationEtoEnum.ALLOW;
			}
			this.SecureCodingConfigCto.ConnectionTimeout = 5000;
			this.SecureCodingConfigCto.ScbPollingTimeout = 120;
			this.SecureCodingConfigCto.NcdRootDirectory = Path.Combine(programmingService.BackupDataPath, NcdRoot);
			this.SecureCodingConfigCto.Retries = 3;
			this.SecureCodingConfigCto.Crls = null;
			this.SecureCodingConfigCto.SwlSecBackendUrls = null;
			this.SecureCodingConfigCto.ScbUrls = new List<string>
			{
				string.Empty
				//BackendConnector.GetBackendServiceUrl(BackendServiceType.SecureCoding, ContextError.SecureCoding).ResultObject
			};
			this.SecureCodingConfigCto.PsdzAuthenticationTypeEto = PsdzAuthenticationTypeEto.SSL;
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
