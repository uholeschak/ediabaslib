using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal abstract class PsdzDuplexClientBase<TChannel, TCallback> : PsdzClientBase<TChannel> where TChannel : class where TCallback : class
    {
        protected PsdzDuplexClientBase(TCallback callbackInstance, Binding binding, EndpointAddress remoteAddress)
#if NETFRAMEWORK
            : base((ChannelFactory<TChannel>)new DuplexChannelFactory<TChannel>(callbackInstance, binding, remoteAddress))
#else
            : base((ChannelFactory<TChannel>)new DuplexChannelFactory<TChannel>(typeof(IPsdzProgressListener), binding, remoteAddress))
#endif
        {
        }
    }
}
