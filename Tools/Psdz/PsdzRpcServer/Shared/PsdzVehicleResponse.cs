using System;
using System.Collections.Generic;

namespace PsdzRpcServer.Shared
{
    public class PsdzVehicleResponse
    {
        public PsdzVehicleResponse(ulong id)
        {
            Id = id;
            Valid = false;
            Connected = false;
            Request = Array.Empty<byte>();
            ResponseList = new List<byte[]>();
        }

        public ulong Id { get; set; }
        public bool Valid { get; set; }
        public bool Connected { get; set; }
        public byte[] Request { get; set; }
        public List<byte[]> ResponseList { get; set; }
    }
}