using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzProgressEvent : PsdzEvent
    {
    }
}
