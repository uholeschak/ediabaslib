using System.Runtime.Serialization;

namespace BMW.ISPI.IstaOperation.Contract.ServiceProgram
{
    [DataContract]
    public class ServiceProgramButtonSelectionAction : ServiceProgramAction
    {
        [DataMember]
        public int SelectedIndex { get; private set; }

        public ServiceProgramButtonSelectionAction(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
}
