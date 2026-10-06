using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Events;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzProgressEvent : PsdzEvent
    {
    }
}
