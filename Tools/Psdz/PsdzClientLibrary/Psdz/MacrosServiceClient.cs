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
    class MacrosServiceClient : PsdzClientBase<IMacrosService>, IMacrosService
    {
        public MacrosServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public IEnumerable<IPsdzSgbmId> CheckSoftwareEntries(IEnumerable<IPsdzSgbmId> sgbmIds)
        {
            return base.CallFunction<IEnumerable<IPsdzSgbmId>>((IMacrosService service) => service.CheckSoftwareEntries(sgbmIds));
        }

        public IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuList(IPsdzFa fa, IPsdzIstufe iStufe)
        {
            return base.CallFunction<IEnumerable<IPsdzEcuIdentifier>>((IMacrosService service) => service.GetInstalledEcuList(fa, iStufe));
        }
    }
}
