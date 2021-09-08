using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    class LogServiceClient : PsdzClientBase<ILogService>, ILogService
    {
        public LogServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public string ClosePsdzLog()
        {
            return base.CallFunction<string>((ILogService service) => service.ClosePsdzLog());
        }

        public void SetLogLevel(PsdzLoglevel psdzLoglevel)
        {
            base.CallMethod(delegate (ILogService service)
            {
                service.SetLogLevel(psdzLoglevel);
            }, true);
        }
    }
}
