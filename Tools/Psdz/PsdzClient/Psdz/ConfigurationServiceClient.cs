using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
	class ConfigurationServiceClient : PsdzClientBase<IConfigurationService>, IConfigurationService
	{
		public ConfigurationServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public string GetExpectedPsdzVersion()
		{
			return base.CallFunction<string>((IConfigurationService service) => service.GetExpectedPsdzVersion());
		}

		public string GetPsdzVersion()
		{
			return base.CallFunction<string>((IConfigurationService service) => service.GetPsdzVersion());
		}

		public string GetRootDirectory()
		{
			return base.CallFunction<string>((IConfigurationService service) => service.GetRootDirectory());
		}

		public bool ImportPdx(string pathToPdxContainer, string projectName)
		{
			return base.CallFunction<bool>((IConfigurationService service) => service.ImportPdx(pathToPdxContainer, projectName));
		}

		public string RequestBaureihenverbund(string baureihe)
		{
			return base.CallFunction<string>((IConfigurationService service) => service.RequestBaureihenverbund(baureihe));
		}

		public void SetRootDirectory(string rootDir)
		{
			base.CallMethod(delegate (IConfigurationService service)
			{
				service.SetRootDirectory(rootDir);
			}, true);
		}

		public void UnsetRootDirectory()
		{
			base.CallMethod(delegate (IConfigurationService service)
			{
				service.UnsetRootDirectory();
			}, true);
		}
	}
}
