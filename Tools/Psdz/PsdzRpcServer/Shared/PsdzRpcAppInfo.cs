namespace PsdzRpcServer.Shared
{
    public class PsdzRpcAppInfo
    {
        public PsdzRpcAppInfo()
        {
            AppId = string.Empty;
            AdapterSerial = string.Empty;
            AdapterSerialValid = false;
        }

        public PsdzRpcAppInfo(string appId, string adapterSerial, bool adapterSerialValid)
        {
            AppId = appId;
            AdapterSerial = adapterSerial;
            AdapterSerialValid = adapterSerialValid;
        }

        public string AppId { get; set; }
        public string AdapterSerial { get; set; }
        public bool AdapterSerialValid { get; set; }
    }
}
