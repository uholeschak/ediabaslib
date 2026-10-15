using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzIdBackupLightTa : PsdzIdLightBasisTa
    {
    }
}
