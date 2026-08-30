using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class KurvenDisplayActionButton : INotifyPropertyChanged
    {
        [DataMember]
        private string content;

        [DataMember]
        private bool executeAction;

        [DataMember]
        public int ButtonNumber { get; set; }

        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                if (content != value)
                {
                    content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public bool ExecuteAction
        {
            get
            {
                return executeAction;
            }
            set
            {
                if (executeAction != value)
                {
                    executeAction = value;
                    OnPropertyChanged("ExecuteAction");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ToogleExecuteState()
        {
            ExecuteAction = !ExecuteAction;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
