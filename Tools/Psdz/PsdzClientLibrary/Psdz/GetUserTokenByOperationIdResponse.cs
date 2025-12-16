using System;
using System.Runtime.Serialization;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class GetUserTokenByOperationIdResponse
    {
        [DataMember]
        public string UserToken { get; set; }

        [DataMember]
        public DateTime? ValidDate { get; set; }
    }
}