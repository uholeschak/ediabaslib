using System.Runtime.Serialization;

namespace BMW.ISPI.IstaOperation.Contract.ServiceProgram
{
    [DataContract]
    public class ServiceProgramTextChangedAction : ServiceProgramAction
    {
        [DataMember]
        public string NewText { get; private set; }

        public ServiceProgramTextChangedAction(string newText)
        {
            NewText = newText;
        }
    }
}
