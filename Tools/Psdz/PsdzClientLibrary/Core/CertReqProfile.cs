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
            crp_M2M_3rdParty_4_CUST_ReadWriteControl
        }
    }
}