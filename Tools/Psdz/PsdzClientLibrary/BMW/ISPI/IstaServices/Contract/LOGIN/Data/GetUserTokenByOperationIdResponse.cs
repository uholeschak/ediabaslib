using System;
using System.Runtime.Serialization;

namespace BMW.ISPI.IstaServices.Contract.LOGIN.Data
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