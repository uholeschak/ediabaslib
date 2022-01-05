using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace PsdzClient
{
    [HubName("PsdzVehicleHub")]
    public class PsdzVehicleHub : Hub<IPsdzClient>
    {
        [HubMethodName("vehicleMessage")]
        public async Task VehicleMessage(string message)
        {
            await Clients.All.VehicleMessage(message);
        }
    }

    public interface IPsdzClient
    {
        Task VehicleMessage(string message);
    }
}