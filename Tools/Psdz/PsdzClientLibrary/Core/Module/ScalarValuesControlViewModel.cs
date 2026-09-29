using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class ScalarValuesControlViewModel : NotifyPropertyChangedBase
    {
        [DataMember]
        private ObservableCollection<ScalarValue> m_ScalarValues = new ObservableCollection<ScalarValue>();

        [DataMember]
        private BarControlViewModel m_BarControlViewModel;

        [DataMember]
        private string m_ScalarValuesFormatting = "G4";

        [DataMember]
        private double m_Width;

        [DataMember]
        private int m_ScalarValuesPosition;

        public int ScalarValuesPosition
        {
            get
            {
                return m_ScalarValuesPosition;
            }
            set
            {
                if (value != m_ScalarValuesPosition)
                {
                    m_ScalarValuesPosition = value;
                    OnPropertyChanged("ScalarValuesPosition");
                }
            }
        }

        public ObservableCollection<ScalarValue> ScalarValues
        {
            get
            {
                return m_ScalarValues;
            }
            set
            {
                if (value != ScalarValues)
                {
                    m_ScalarValues = value;
                    OnPropertyChanged("ScalarValues");
                }
            }
        }

        public double Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                if (value != m_Width)
                {
                    m_Width = value;
                    CalculateScalarValues();
                    OnPropertyChanged("Width");
                }
            }
        }

        public ScalarValuesControlViewModel()
        {
        }

        public ScalarValuesControlViewModel(BarControlViewModel barControlViewModel)
        {
            m_BarControlViewModel = barControlViewModel;
            ScalarValuesPosition = m_BarControlViewModel.ScalarValuesPosition;
            m_BarControlViewModel.PropertyChanged += HandleScalarValuesUpdate;
            CalculateScalarValues();
        }

        private void HandleScalarValuesUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("MinValue") || e.PropertyName.Equals("MaxValue") || e.PropertyName.Equals("MajorDivisions"))
            {
                CalculateScalarValues();
            }
            else if (e.PropertyName.Equals("ScalarValuesPosition"))
            {
                ScalarValuesPosition = m_BarControlViewModel.ScalarValuesPosition;
            }
        }

        private void CalculateScalarValues()
        {
            if (m_BarControlViewModel == null)
            {
                return;
            }
            ScalarValues.Clear();
            double maxValue = m_BarControlViewModel.MaxValue;
            double minValue = m_BarControlViewModel.MinValue;
            int majorDivisions = m_BarControlViewModel.MajorDivisions;
            if (double.IsNaN(maxValue) || double.IsNaN(maxValue) || maxValue <= minValue)
            {
                return;
            }
            majorDivisions = ((double.IsNaN(majorDivisions) || majorDivisions < 2) ? 1 : majorDivisions);
            int num = majorDivisions + 1;
            double num2 = Width / (double)majorDivisions;
            for (int i = 0; i < num; i++)
            {
                if (i == 0)
                {
                    ScalarValues.Add(new ScalarValue(m_BarControlViewModel.MinValue.ToString(m_ScalarValuesFormatting), (double)i * num2));
                }
                else if (i == num - 1)
                {
                    ScalarValues.Add(new ScalarValue(m_BarControlViewModel.MaxValue.ToString(m_ScalarValuesFormatting), (double)i * num2));
                }
                else
                {
                    ScalarValues.Add(new ScalarValue((minValue + (maxValue - minValue) / (double)majorDivisions * (double)i).ToString(m_ScalarValuesFormatting), (double)i * num2));
                }
            }
        }
    }
}
