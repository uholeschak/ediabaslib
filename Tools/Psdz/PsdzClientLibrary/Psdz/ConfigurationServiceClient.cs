using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class ConfigurationServiceClient : PsdzClientBase<IConfigurationService>, IConfigurationService
    {
        public ConfigurationServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public string GetExpectedPsdzVersion()
        {
            return CallFunction((IConfigurationService service) => service.GetExpectedPsdzVersion());
        }

        public string GetPsdzVersion()
        {
            return CallFunction((IConfigurationService service) => service.GetPsdzVersion());
        }

        public string GetRootDirectory()
        {
            return CallFunction((IConfigurationService service) => service.GetRootDirectory());
        }

        public bool ImportPdx(string pathToPdxContainer, string projectName)
        {
            return CallFunction((IConfigurationService service) => service.ImportPdx(pathToPdxContainer, projectName));
        }

        public string RequestBaureihenverbund(string baureihe)
        {
            return CallFunction((IConfigurationService service) => service.RequestBaureihenverbund(baureihe));
        }

        public void SetRootDirectory(string rootDir)
        {
            CallMethod(delegate (IConfigurationService service)
            {
                service.SetRootDirectory(rootDir);
            });
        }

        public void UnsetRootDirectory()
        {
            CallMethod(delegate (IConfigurationService service)
            {
                service.UnsetRootDirectory();
            });
        }

        // [UH] for backward compatibility
        public bool IsReady()
        {
            return CallFunction((IConfigurationService service) => service.IsReady());
        }

        // [UH] for backward compatibility
        public RootDirectorySetupResultModel GetRootDirectorySetupResult()
        {
            return CallFunction((IConfigurationService service) => service.GetRootDirectorySetupResult());
        }
    }
}
