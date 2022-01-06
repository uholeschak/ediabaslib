using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.SignalR.Hubs;

namespace PsdzClient
{
    [HubName("psdzVehicleHub")]
    public class PsdzVehicleHub : Hub<IPsdzClient>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PsdzVehicleHub));

        [HubMethodName("vehicleResponse")]
        public async Task VehicleResponse(string message)
        {
            log.InfoFormat("Vehicle response: {0}", message);
            //await Clients.Caller.vehicleRequest("Response: " + message);
        }

        [HubMethodName("sessionConnected")]
        public async Task SessionConnected(string sessionId)
        {
            log.InfoFormat("Session connected: {0} {1}", sessionId, Context.ConnectionId);
        }

        public override Task OnConnected()
        {
            log.InfoFormat("Connected ID: {0}", Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            log.InfoFormat("Disconnected ID: {0}", Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }

    public interface IPsdzClient
    {
        Task vehicleRequest(string message);
    }
}