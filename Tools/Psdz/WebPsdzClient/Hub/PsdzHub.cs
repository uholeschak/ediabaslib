using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Microsoft.AspNet.SignalR.Hubs;
using WebPsdzClient.App_Data;

namespace PsdzClient
{
    [HubName("psdzVehicleHub")]
    public class PsdzVehicleHub : Hub<IPsdzClient>
    {
        public class VehicleResponse
        {
            public VehicleResponse(string id, bool error)
            {
                Id = id;
                Valid = false;
                Error = error;
                Connected = false;
                ErrorMessage = string.Empty;
                Request = string.Empty;
                ResponseList = new List<string>();
            }

            public string Id { get; set; }
            public bool Valid { get; set; }
            public bool Error { get; set; }
            public bool Connected { get; set; }
            public string ErrorMessage { get; set; }
            public string Request { get; set; }
            public List<string> ResponseList { get; set; }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(PsdzVehicleHub));
        public static Dictionary<string, string> ConnectionDict { get; } = new Dictionary<string, string>();

        public static List<string> GetConnectionIds(string sessionId)
        {
            List<string> connectionIds = new List<string>();
            lock (ConnectionDict)
            {
                foreach (KeyValuePair<string, string> pair in ConnectionDict)
                {
                    if (string.Compare(pair.Value, sessionId, StringComparison.Ordinal) == 0)
                    {
                        connectionIds.Add(pair.Key);
                    }
                }
            }
            return connectionIds;
        }

        [HubMethodName("vehicleReceive")]
        public async Task VehicleReceive(string sessionId, string id, string data)
        {
            string transport = Context.QueryString["transport"] ?? string.Empty;
            await Task.Run(() =>
            {
                log.InfoFormat("VehicleReceive: Session={0}, Id={1}, Transport={2}, Data={3}", sessionId, id, transport, data);
                SessionContainer sessionContainer = SessionContainer.GetSessionContainer(sessionId);
                if (sessionContainer == null)
                {
                    log.ErrorFormat("VehicleReceive: Session not found: {0}", sessionId);
                }
                else
                {
                    VehicleResponse vehicleResponse = ParseVehiceResponse(id, data);
                    sessionContainer.VehicleResponseReceived(vehicleResponse);
                }
            });
        }

        [HubMethodName("vehicleError")]
        public async Task VehicleError(string sessionId, string id, string message)
        {
            string transport = Context.QueryString["transport"] ?? string.Empty;
            await Task.Run(() =>
            {
                log.ErrorFormat("VehicleError: Session={0}, Id={1}, Transport={2}, Message={3}", sessionId, id, transport, message);
                SessionContainer sessionContainer = SessionContainer.GetSessionContainer(sessionId);
                if (sessionContainer == null)
                {
                    log.ErrorFormat("VehicleError: Session not found: {0}", sessionId);
                }
                else
                {
                    VehicleResponse vehicleResponse = new VehicleResponse(id, true)
                    {
                        ErrorMessage = message
                    };
                    sessionContainer.VehicleResponseReceived(vehicleResponse);
                }
            });
        }

        [HubMethodName("sessionConnected")]
        public async Task SessionConnected(string sessionId)
        {
            string transport = Context.QueryString["transport"];
            await Task.Run(() =>
            {
                log.InfoFormat("Session connected: SessionId={0}, ConnectionId={1}, Transport={2}",
                    sessionId ?? string.Empty, Context.ConnectionId ?? string.Empty, transport ?? string.Empty);
            });
        }

        public override Task OnConnected()
        {
            string sessionId = Context.QueryString["sessionId"];
            string transport = Context.QueryString["transport"];
            string connectionId = Context.ConnectionId;
            log.InfoFormat("OnConnected: SessionId={0}, ConnectionId={1}, Transport={2}",
                sessionId ?? string.Empty, Context.ConnectionId ?? string.Empty, transport ?? string.Empty);

            if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(connectionId))
            {
                lock (ConnectionDict)
                {
                    ConnectionDict[connectionId] = sessionId;
                }
            }
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            string sessionId = Context.QueryString["sessionId"];
            string transport = Context.QueryString["transport"];
            string connectionId = Context.ConnectionId;
            log.InfoFormat("OnReconnected: SessionId={0}, ConnectionId={1}, Transport={2}",
                sessionId ?? string.Empty, Context.ConnectionId ?? string.Empty, transport ?? string.Empty);

            if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(connectionId))
            {
                lock (ConnectionDict)
                {
                    ConnectionDict[connectionId] = sessionId;
                }
            }

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string connectionId = Context.ConnectionId;
            log.InfoFormat("OnDisconnected: ConnectionId={0}", Context.ConnectionId ?? string.Empty);

            if (!string.IsNullOrEmpty(connectionId))
            {
                lock (ConnectionDict)
                {
                    ConnectionDict.Remove(connectionId);
                }
            }
            return base.OnDisconnected(stopCalled);
        }

        public VehicleResponse ParseVehiceResponse(string id, string responseXml)
        {
            VehicleResponse vehicleResponse = new VehicleResponse(id, false);
            try
            {
                string responseId = string.Empty;
                if (string.IsNullOrEmpty(responseXml))
                {
                    return vehicleResponse;
                }

                XDocument xmlDoc = XDocument.Parse(responseXml);
                if (xmlDoc.Root == null)
                {
                    return vehicleResponse;
                }

                XElement requestNode = xmlDoc.Root.Element("request");
                if (requestNode != null)
                {
                    XAttribute validAttr = requestNode.Attribute("valid");
                    if (validAttr != null)
                    {
                        try
                        {
                            vehicleResponse.Valid = XmlConvert.ToBoolean(validAttr.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    XAttribute idAttr = requestNode.Attribute("id");
                    if (idAttr != null)
                    {
                        responseId = idAttr.Value;
                    }
                }

                XElement statusNode = xmlDoc.Root.Element("status");
                if (statusNode != null)
                {
                    XAttribute connectedAttr = statusNode.Attribute("connected");
                    if (connectedAttr != null)
                    {
                        try
                        {
                            vehicleResponse.Connected = XmlConvert.ToBoolean(connectedAttr.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                foreach (XElement dataNode in xmlDoc.Root.Elements("data"))
                {
                    XAttribute requestAttr = dataNode.Attribute("request");
                    if (requestAttr != null)
                    {
                        vehicleResponse.Request = requestAttr.Value;
                    }

                    XAttribute responseAttr = dataNode.Attribute("response");
                    if (responseAttr != null)
                    {
                        vehicleResponse.ResponseList.Add(responseAttr.Value);
                    }
                }

                if (string.Compare(id, responseId, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    vehicleResponse.Valid = false;
                }
            }
            catch (Exception)
            {
                vehicleResponse.Valid = false;
            }

            return vehicleResponse;
        }
    }

    public interface IPsdzClient
    {
        [HubMethodName("vehicleConnect")]
        Task VehicleConnect(string url, string id);

        [HubMethodName("vehicleDisconnect")]
        Task VehicleDisconnect(string url, string id);

        [HubMethodName("vehicleSend")]
        Task VehicleSend(string url, string id, string data);

        [HubMethodName("reportError")]
        Task ReportError(string msg);

        [HubMethodName("updatePanels")]
        Task UpdatePanels(bool status);

        [HubMethodName("reloadPage")]
        Task ReloadPage();
    }
}