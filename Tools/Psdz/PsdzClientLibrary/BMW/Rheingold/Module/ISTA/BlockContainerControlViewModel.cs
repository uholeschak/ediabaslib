using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class BlockContainerControlViewModel : NotifyPropertyChangedBase
    {
        [DataMember]
        private ObservableCollection<BlockControlViewModel> m_Blocks = new ObservableCollection<BlockControlViewModel>();

        public ObservableCollection<BlockControlViewModel> Blocks
        {
            get
            {
                return m_Blocks;
            }
            set
            {
                if (!object.Equals(m_Blocks, value))
                {
                    m_Blocks = value;
                    OnPropertyChanged("Blocks");
                }
            }
        }
    }
}
