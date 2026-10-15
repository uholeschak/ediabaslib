using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class BlockControlViewModel : NotifyPropertyChangedBase
    {
        [DataMember]
        private ObservableCollection<BarControlViewModel> m_Bars = new ObservableCollection<BarControlViewModel>();

        [DataMember]
        private string m_BlockHeader = string.Empty;

        [DataMember]
        private int m_BlockIndex;

        public string BlockHeader
        {
            get
            {
                return m_BlockHeader;
            }
            set
            {
                if (value != m_BlockHeader)
                {
                    m_BlockHeader = value;
                    OnPropertyChanged("BlockHeader");
                }
            }
        }

        public ObservableCollection<BarControlViewModel> Bars
        {
            get
            {
                return m_Bars;
            }
            set
            {
                if (!object.Equals(m_Bars, value))
                {
                    m_Bars = value;
                    OnPropertyChanged("Bars");
                }
            }
        }

        public int BlockIndex
        {
            get
            {
                return m_BlockIndex;
            }
            set
            {
                if (!object.Equals(m_BlockIndex, value))
                {
                    m_BlockIndex = value;
                    OnPropertyChanged("BlockIndex");
                }
            }
        }

        public BlockControlViewModel(int blockIndex)
        {
            BlockIndex = blockIndex;
        }
    }
}
