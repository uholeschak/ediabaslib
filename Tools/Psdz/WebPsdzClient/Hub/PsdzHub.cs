using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace PsdzClient
{
    [HubName("psdzVehicleHub")]
    public class PsdzVehicleHub : Hub<IPsdzClient>
    {
        [HubMethodName("vehicleResponse")]
        public async Task VehicleResponse(string message)
        {
            await Clients.All.vehicleRequest("Response: " + message);
        }
    }

    public interface IPsdzClient
    {
        Task vehicleRequest(string message);
    }
}