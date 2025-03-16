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
            crp_subCA_4ISTA_TISonly,
            [EnumMember]
            crp_PERS_ISTADeveloper_4_DEV_Programming
        }
    }
}