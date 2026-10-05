using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.Interaction.Responses;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Localization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionIMIBConnectionModel : InteractionRequestModel<InteractionConnectionManagerResponse>
    {
        [DataMember]
        private ObservableCollection<IVciDevice> devices;

        [DataMember]
        private List<string> localAddresses;

        public ObservableCollection<IVciDevice> Devices
        {
            get
            {
                return devices;
            }
            set
            {
                devices = value;
            }
        }

        public List<string> LocalAddresses
        {
            get
            {
                return localAddresses;
            }
            set
            {
                localAddresses = value;
            }
        }

        public InteractionIMIBConnectionModel(List<string> localAddresses)
        {
            devices = new ObservableCollection<IVciDevice>();
            this.localAddresses = localAddresses;
            base.DialogSize = 2;
            base.Title = FormatedData.Localize("#SearchForIMIB");
            base.IsCloseButtonEnabled = false;
        }

        public override void OnResponseReceived(InteractionConnectionManagerResponse response)
        {
            Log.Info("InteractionConnectionManagerModel.OnResponseRecived()", "InteractionConnectionManagerResponse was set to the model. Parameters:  Button = '{0}'", response?.Action);
            NotifyAboutResponseReceived();
            Dispose();
        }
    }
}
