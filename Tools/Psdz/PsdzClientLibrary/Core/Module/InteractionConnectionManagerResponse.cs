using PsdzClient.Core;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework.Interaction.Responses
{
    [DataContract]
    public class InteractionConnectionManagerResponse : InteractionResponse
    {
        [DataMember]
        public VCIDevice VciDevice { get; private set; }

        [DataMember]
        public ConnectionManagerResponseAction Action { get; private set; }

        public InteractionConnectionManagerResponse(ConnectionManagerResponseAction responseAction, VCIDevice vciDevice)
        {
            Action = responseAction;
            VciDevice = vciDevice;
        }
    }
}
