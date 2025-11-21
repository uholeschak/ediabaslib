using PsdzClient.Core;
using PsdzClient;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal class LogServiceClient : PsdzClientBase<ILogService>, ILogService
    {
        public LogServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public string ClosePsdzLog()
        {
            return CallFunction((ILogService service) => service.ClosePsdzLog());
        }

        public void SetLogLevel(PsdzLoglevel psdzLoglevel)
        {
            CallMethod(delegate (ILogService service)
            {
                service.SetLogLevel(psdzLoglevel);
            });
        }

        public void PrepareLoggingForCurrentThread()
        {
        }
    }
}
