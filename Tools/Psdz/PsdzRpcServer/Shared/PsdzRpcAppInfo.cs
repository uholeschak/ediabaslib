namespace PsdzRpcServer.Shared
{
    public class PsdzRpcAppInfo
    {
        public PsdzRpcAppInfo(string appId, string adapterSerial, bool adapterSerialValid)
        {
            AppId = appId;
            AdapterSerial = adapterSerial;
            AdapterSerialValid = adapterSerialValid;
        }

        public string AppId { get; private set; }
        public string AdapterSerial { get; private set; }
        public bool AdapterSerialValid { get; private set; }
    }
}
