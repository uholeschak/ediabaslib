using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Sec4Diag
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