using BMW.Authoring.Helper;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class ScalarLinesControlViewModel : NotifyPropertyChangedBase
    {
        [DataMember]
        private BarControlViewModel m_BarControlViewModel;

        [DataMember]
        private double m_Width;

        [DataMember]
        private ObservableCollection<ColoredRectangle> m_MajorScalarLines = new ObservableCollection<ColoredRectangle>();

        [DataMember]
        private ObservableCollection<ColoredRectangle> m_MinorScalarLines = new ObservableCollection<ColoredRectangle>();

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
                    CreateMajorScalarLines();
                    CreateMinorScalarLines();
                    OnPropertyChanged("Width");
                }
            }
        }

        public ObservableCollection<ColoredRectangle> MajorScalarLines
        {
            get
            {
                return m_MajorScalarLines;
            }
            set
            {
                if (value != m_MajorScalarLines)
                {
                    m_MajorScalarLines = value;
                    OnPropertyChanged("MajorScalarLines");
                }
            }
        }

        public ObservableCollection<ColoredRectangle> MinorScalarLines
        {
            get
            {
                return m_MinorScalarLines;
            }
            set
            {
                if (value != m_MinorScalarLines)
                {
                    m_MinorScalarLines = value;
                    OnPropertyChanged("MinorScalarLines");
                }
            }
        }

        private ScalarLinesControlViewModel()
        {
        }

        public ScalarLinesControlViewModel(BarControlViewModel barControlViewModel)
        {
            m_BarControlViewModel = barControlViewModel;
            m_BarControlViewModel.PropertyChanged += HandleScalarLinesUpdates;
        }

        private void HandleScalarLinesUpdates(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("MajorDivisions"))
            {
                CreateMajorScalarLines();
                CreateMinorScalarLines();
            }
            else if (e.PropertyName.Equals("MinorDivisions"))
            {
                CreateMinorScalarLines();
            }
        }

        private void CreateMajorScalarLines()
        {
            MajorScalarLines.Clear();
            if (m_BarControlViewModel != null && !double.IsNaN(m_BarControlViewModel.MajorDivisions) && m_BarControlViewModel.MajorDivisions >= 2)
            {
                double num = Width / (double)m_BarControlViewModel.MajorDivisions;
                for (int i = 1; i < m_BarControlViewModel.MajorDivisions; i++)
                {
                    MajorScalarLines.Add(new ColoredRectangle(ServiceDialogColor.Black, num * (double)i, 1.0));
                }
            }
        }

        private void CreateMinorScalarLines()
        {
            MinorScalarLines.Clear();
            if (m_BarControlViewModel == null || double.IsNaN(m_BarControlViewModel.MajorDivisions) || double.IsNaN(m_BarControlViewModel.MinorDivisions) || m_BarControlViewModel.MajorDivisions < 2 || m_BarControlViewModel.MinorDivisions < 2)
            {
                return;
            }
            int num = m_BarControlViewModel.MajorDivisions * m_BarControlViewModel.MinorDivisions;
            double num2 = Width / (double)num;
            for (int i = 0; i < num; i++)
            {
                if (i % m_BarControlViewModel.MinorDivisions != 0)
                {
                    MinorScalarLines.Add(new ColoredRectangle(ServiceDialogColor.Black, num2 * (double)i, 1.0));
                }
            }
        }
    }
}
