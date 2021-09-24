using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace PsdzClient.Programming
{
	public class SecureCodingConfigWrapper
	{
		private PsdzSecureCodingConfigCto SecureCodingConfigCto { get; }

		public static PsdzSecureCodingConfigCto GetSecureCodingConfig(ProgrammingService programmingService)
		{
			if (SecureCodingConfigWrapper.instance == null)
			{
				object obj = SecureCodingConfigWrapper.@lock;
				lock (obj)
				{
					if (SecureCodingConfigWrapper.instance == null)
					{
						SecureCodingConfigWrapper.instance = new SecureCodingConfigWrapper(programmingService).SecureCodingConfigCto;
					}
				}
			}
			return SecureCodingConfigWrapper.instance;
		}

		internal static void ChangeNcdRecalculationValueTo(ProgrammingService programmingService, PsdzNcdRecalculationEtoEnum psdzNcdRecalculation)
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

		internal static void ChangeBackendNcdCalculationValueTo(ProgrammingService programmingService, PsdzBackendNcdCalculationEtoEnum backendNcdCalculation)
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
			return Path.Combine(programmingService.BackupDataPath, vin);
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
			this.SecureCodingConfigCto.NcdRootDirectory = programmingService.BackupDataPath;
			this.SecureCodingConfigCto.Retries = 3;
			this.SecureCodingConfigCto.Crls = null;
			this.SecureCodingConfigCto.SwlSecBackendUrls = null;
			this.SecureCodingConfigCto.ScbUrls = new List<string>
			{
				//BackendConnector.GetBackendServiceUrl(BackendServiceType.SecureCoding, ContextError.SecureCoding).ResultObject
			};
			this.SecureCodingConfigCto.PsdzAuthenticationTypeEto = PsdzAuthenticationTypeEto.SSL;
		}

		private static string ConvertListToString(IList<string> list)
		{
			if (list != null && list.Any<string>())
			{
				return string.Join(", ", list.ToArray<string>());
			}
			return string.Empty;
		}

		private static PsdzSecureCodingConfigCto instance;

		private static readonly object @lock = new object();
	}
}
