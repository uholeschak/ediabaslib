using System;
using System.Collections.Generic;
using System.Linq;
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
        public static Dictionary<string, string> ConnectionDict { get; } = new Dictionary<string, string>();

        public static List<string> GetConnectionIds(string sessionId)
        {
            List<string> connectionIds =
                ConnectionDict.Where(pair => string.Compare(pair.Value, sessionId, StringComparison.Ordinal) == 0)
                    .Select(pair => pair.Key).ToList();
            return connectionIds;
        }

        [HubMethodName("vehicleReceive")]
        public async Task VehicleReceive(string data)
        {
            await Task.Run(() =>
            {
                log.InfoFormat("Vehicle receive: {0}", data);
            });
        }

        [HubMethodName("vehicleError")]
        public async Task VehicleError(string message)
        {
            await Task.Run(() =>
            {
                log.InfoFormat("Vehicle error: {0}", message);
            });
        }

        [HubMethodName("sessionConnected")]
        public async Task SessionConnected(string sessionId)
        {
            await Task.Run(() =>
            {
                log.InfoFormat("Session connected: {0} {1}", sessionId, Context.ConnectionId);
            });
        }

        public override Task OnConnected()
        {
            string sessionId = Context.QueryString["sessionId"];
            string connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(sessionId))
            {
                ConnectionDict[connectionId] = sessionId;
            }
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            string sessionId = Context.QueryString["sessionId"];
            string connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(sessionId))
            {
                ConnectionDict[connectionId] = sessionId;
            }

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string connectionId = Context.ConnectionId;

            ConnectionDict.Remove(connectionId);
            return base.OnDisconnected(stopCalled);
        }
    }

    public interface IPsdzClient
    {
        [HubMethodName("vehicleConnect")]
        Task VehicleConnect(string url);

        [HubMethodName("vehicleDisconnect")]
        Task VehicleDisconnect(string url);

        [HubMethodName("vehicleSend")]
        Task VehicleSend(string url, string data);
    }
}