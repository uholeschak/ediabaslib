using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    public interface IDeviceService
    {
        IList<VCIDevice> Devices { get; }

        Regex Filter { get; set; }

        string FilterAsString { get; set; }

        event EventHandler<NotifyCollectionChangedEventArgs> DevicesChanged;

        void Start(IPAddress address);

        void Start();

        void Stop();
    }
}
