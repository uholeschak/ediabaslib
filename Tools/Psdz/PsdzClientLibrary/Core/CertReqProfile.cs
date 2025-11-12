using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    internal sealed class CertReqProfile
    {
        [DataContract]
        public enum EnumType
        {
            [EnumMember]
            crp_subCA_4ISTA,
            [EnumMember]
            crp_PERS_Workshop_4_CUST_Programming
        }
    }
}