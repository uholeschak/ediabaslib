using BMW.Authoring.Helper;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class BarColorsControlViewModel : NotifyPropertyChangedBase
    {
        [DataMember]
        private BarControlViewModel m_BarControlViewModel;

        [DataMember]
        private double m_Width;

        [DataMember]
        private ObservableCollection<ColoredRectangle> m_ColoredBarParts = new ObservableCollection<ColoredRectangle>();

        [DataMember]
        private Dictionary<double, ServiceDialogColor> m_ColorPositions;

        [DataMember]
        private int m_LastColorPositionsCount;

        [DataMember]
        private DictionaryComparer<double, ServiceDialogColor> m_DictionaryComparer = new DictionaryComparer<double, ServiceDialogColor>();

        public double Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                if (m_Width != value)
                {
                    m_Width = value;
                    CreateColoredBarParts();
                    OnPropertyChanged("Width");
                }
            }
        }

        public Dictionary<double, ServiceDialogColor> ColorPositions
        {
            get
            {
                return m_ColorPositions;
            }
            set
            {
                if (!m_DictionaryComparer.Equals(m_ColorPositions, value))
                {
                    m_ColorPositions = value;
                    CreateColoredBarParts();
                    OnPropertyChanged("ColorPositions");
                }
            }
        }

        public ObservableCollection<ColoredRectangle> ColoredBarParts
        {
            get
            {
                return m_ColoredBarParts;
            }
            set
            {
                if (m_ColoredBarParts != value)
                {
                    m_ColoredBarParts = value;
                    m_LastColorPositionsCount = value?.Count ?? 0;
                    OnPropertyChanged("ColoredBarParts");
                }
            }
        }

        private BarColorsControlViewModel()
        {
        }

        public BarColorsControlViewModel(BarControlViewModel barControlViewModel)
        {
            m_BarControlViewModel = barControlViewModel;
            m_BarControlViewModel.PropertyChanged += HandleScalarValuesUpdate;
        }

        private void HandleScalarValuesUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("MinValue") || e.PropertyName.Equals("MaxValue"))
            {
                CreateColoredBarParts();
            }
        }

        private void CreateColoredBarParts()
        {
            if (double.IsNaN(Width) || double.IsNaN(m_BarControlViewModel.MaxValue) || double.IsNaN(m_BarControlViewModel.MinValue) || Width == 0.0 || m_BarControlViewModel.MaxValue <= m_BarControlViewModel.MinValue || ColorPositions == null || ColorPositions.Count == 0)
            {
                return;
            }
            double num = 0.0;
            List<ColoredRectangle> list = new List<ColoredRectangle>();
            foreach (KeyValuePair<double, ServiceDialogColor> colorPosition in ColorPositions)
            {
                double num2 = GraphUtility.MapValueToRange(Math.Max(Math.Min(m_BarControlViewModel.MaxValue, colorPosition.Key), m_BarControlViewModel.MinValue), m_BarControlViewModel.MinValue, m_BarControlViewModel.MaxValue, 0.0, Width);
                list.Add(new ColoredRectangle(colorPosition.Value, num, num2 - num));
                num = num2;
            }
            ColoredBarParts = new ObservableCollection<ColoredRectangle>(list);
        }
    }
}
