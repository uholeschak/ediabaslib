using PsdzClient;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal abstract class PsdzDuplexClientBase<TChannel, TCallback> : PsdzClientBase<TChannel> where TChannel : class where TCallback : class
    {
        protected PsdzDuplexClientBase(TCallback callbackInstance, Binding binding, EndpointAddress remoteAddress)
//[+]#if NET
#if NET
//[+]: base((ChannelFactory<TChannel>)new DuplexChannelFactory<TChannel>(new InstanceContext(callbackInstance), binding, remoteAddress))
            : base((ChannelFactory<TChannel>)new DuplexChannelFactory<TChannel>(new InstanceContext(callbackInstance), binding, remoteAddress))
//[+]#else
#else
            : base((ChannelFactory<TChannel>)new DuplexChannelFactory<TChannel>(callbackInstance, binding, remoteAddress))
//[+]#endif
#endif
        {
        }
    }
}
