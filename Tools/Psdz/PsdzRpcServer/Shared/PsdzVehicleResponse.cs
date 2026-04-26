using System.Collections.Generic;

namespace PsdzRpcServer.Shared
{
    public class PsdzVehicleResponse
    {
        public PsdzVehicleResponse(ulong id, bool error)
        {
            Id = id;
            Valid = false;
            Error = error;
            Connected = false;
            ConnectTimeouts = null;
            AppId = string.Empty;
            AdapterSerial = string.Empty;
            SerialValid = false;
            ErrorMessage = string.Empty;
            Request = string.Empty;
            ResponseList = new List<string>();
        }

        public ulong Id { get; set; }
        public bool Valid { get; set; }
        public bool Error { get; set; }
        public bool Connected { get; set; }
        public int? ConnectTimeouts { get; set; }
        public string AppId { get; set; }
        public string AdapterSerial { get; set; }
        public bool SerialValid { get; set; }
        public string ErrorMessage { get; set; }
        public string Request { get; set; }
        public List<string> ResponseList { get; set; }
    }
}