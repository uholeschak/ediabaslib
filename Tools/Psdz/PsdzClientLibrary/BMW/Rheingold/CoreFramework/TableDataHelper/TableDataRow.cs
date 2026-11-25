using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.TableDataHelper
{
    [DataContract]
    public class TableDataRow : INotifyPropertyChanged
    {
        [DataMember]
        private List<object> cells;
        public List<object> Cells
        {
            get
            {
                return cells;
            }

            set
            {
                if (cells != value)
                {
                    cells = value;
                    OnPropertyChanged("Cells");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public TableDataRow(List<object> cells)
        {
            Cells = cells;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}