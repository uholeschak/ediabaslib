using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Client
{
    internal class MacrosServiceClient : PsdzClientBase<IMacrosService>, IMacrosService
    {
        public MacrosServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IEnumerable<IPsdzSgbmId> CheckSoftwareEntries(IEnumerable<IPsdzSgbmId> sgbmIds)
        {
            return CallFunction((IMacrosService service) => service.CheckSoftwareEntries(sgbmIds));
        }

        public IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuList(IPsdzFa fa, IPsdzIstufe iStufe)
        {
            return CallFunction((IMacrosService service) => service.GetInstalledEcuList(fa, iStufe));
        }

        public IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuListWithConnection(IPsdzConnection connection, IPsdzFa fa, IPsdzIstufe iStufe)
        {
            return CallFunction((IMacrosService service) => service.GetInstalledEcuListWithConnection(connection, fa, iStufe));
        }
    }
}
