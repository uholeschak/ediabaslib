using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.ServiceProgram;

namespace BMW.ISPI.IstaOperation.Contract.ServiceProgram
{
    [DataContract]
    public class ServiceProgramNavigationAction : ServiceProgramAction
    {
        [DataMember]
        public NavigationAction NavigationAction { get; private set; }

        public ServiceProgramNavigationAction(NavigationAction navigationAction)
        {
            NavigationAction = navigationAction;
        }
    }
}
