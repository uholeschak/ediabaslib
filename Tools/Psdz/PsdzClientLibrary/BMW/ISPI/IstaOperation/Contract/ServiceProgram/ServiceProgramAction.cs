using System.Runtime.Serialization;

namespace BMW.ISPI.IstaOperation.Contract.ServiceProgram
{
    [DataContract]
    public abstract class ServiceProgramAction
    {
        public bool IsPerformed { get; set; }
    }
}
