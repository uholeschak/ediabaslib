using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.TableDataHelper
{
    [DataContract]
    public class TableData : INotifyPropertyChanged
    {
        public string InfoTitle;

        [DataMember]
        private List<string> columnHeaders;

        [DataMember]
        private List<TableDataRow> rows;

        [DataMember]
        private string beforeText;

        [DataMember]
        private string afterText;

        public List<string> ColumnHeaders
        {
            get
            {
                return columnHeaders;
            }
            set
            {
                if (columnHeaders != value)
                {
                    columnHeaders = value;
                    OnPropertyChanged("ColumnHeaders");
                }
            }
        }

        public List<TableDataRow> Rows
        {
            get
            {
                return rows;
            }
            set
            {
                if (rows != value)
                {
                    rows = value;
                    OnPropertyChanged("Rows");
                }
            }
        }

        public string BeforeText
        {
            get
            {
                return beforeText;
            }
            set
            {
                if (beforeText != value)
                {
                    beforeText = value;
                    OnPropertyChanged("BeforeText");
                }
            }
        }

        public string AfterText
        {
            get
            {
                return afterText;
            }
            set
            {
                if (afterText != value)
                {
                    afterText = value;
                    OnPropertyChanged("AfterText");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TableData(List<string> columnHeaders, List<TableDataRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Cells.Count < columnHeaders.Count)
                {
                    rows[i].Cells.Add("");
                }
            }
            ColumnHeaders = columnHeaders;
            Rows = rows;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
