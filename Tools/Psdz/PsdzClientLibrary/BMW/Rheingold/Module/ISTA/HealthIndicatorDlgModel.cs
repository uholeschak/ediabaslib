using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class HealthIndicatorDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private BlockContainerControlViewModel m_BlockContainerData;

        [DataMember]
        private string m_PriorText = string.Empty;

        [DataMember]
        private string m_PastText = string.Empty;

        [DataMember]
        private bool m_IsFullScreen;

        public BlockContainerControlViewModel BlockContainerData
        {
            get
            {
                return m_BlockContainerData;
            }
            set
            {
                if (!object.Equals(m_BlockContainerData, value))
                {
                    m_BlockContainerData = value;
                    OnPropertyChanged("BlockContainerData");
                }
            }
        }

        public string PriorText
        {
            get
            {
                return m_PriorText;
            }
            set
            {
                if (value != m_PriorText)
                {
                    m_PriorText = value;
                    OnPropertyChanged("PriorText");
                }
            }
        }

        public string PastText
        {
            get
            {
                return m_PastText;
            }
            set
            {
                if (value != m_PastText)
                {
                    m_PastText = value;
                    OnPropertyChanged("PastText");
                }
            }
        }

        public bool IsFullScreen
        {
            get
            {
                return m_IsFullScreen;
            }
            set
            {
                if (value != m_IsFullScreen)
                {
                    m_IsFullScreen = value;
                    OnPropertyChanged("IsFullScreen");
                }
            }
        }
    }
}
