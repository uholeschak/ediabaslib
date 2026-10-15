using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class KurvendisplayDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private readonly ObservableCollection<CurveData> curves;

        [DataMember]
        private string abschluss;

        [DataMember]
        private CurveDisplayData data;

        [DataMember]
        private string einleitung;

        [DataMember]
        private bool initChart;

        [DataMember]
        private bool isCurveDisplayVisible;

        [DataMember]
        private bool isFrozen;

        [DataMember]
        private string ueberschrift;

        public IEnumerable<CurveData> Curves
        {
            get
            {
                return curves;
            }
            set
            {
                Data.UpDateCurvesToView(curves);
                OnPropertyChanged("Curves");
            }
        }

        public CurveDisplayData Data => data;

        public string Ueberschrift
        {
            get
            {
                return ueberschrift;
            }
            set
            {
                if (!object.Equals(ueberschrift, value))
                {
                    ueberschrift = value;
                    OnPropertyChanged("Ueberschrift");
                }
            }
        }

        public string Einleitung
        {
            get
            {
                return einleitung;
            }
            set
            {
                if (!object.Equals(einleitung, value))
                {
                    einleitung = value;
                    OnPropertyChanged("Einleitung");
                }
            }
        }

        public string Abschluss
        {
            get
            {
                return abschluss;
            }
            set
            {
                if (!object.Equals(abschluss, value))
                {
                    abschluss = value;
                    OnPropertyChanged("Abschluss");
                }
            }
        }

        public bool IsCurveDisplayVisible
        {
            get
            {
                return isCurveDisplayVisible;
            }
            set
            {
                if (!object.Equals(isCurveDisplayVisible, value))
                {
                    isCurveDisplayVisible = value;
                    OnPropertyChanged("IsCurveDisplayVisible");
                }
            }
        }

        public bool InitChart
        {
            get
            {
                return initChart;
            }
            set
            {
                if (!object.Equals(initChart, value))
                {
                    initChart = value;
                    OnPropertyChanged("InitChart");
                }
            }
        }

        public bool IsFrozen
        {
            get
            {
                return isFrozen;
            }
            set
            {
                if (!object.Equals(isFrozen, value))
                {
                    isFrozen = value;
                    OnPropertyChanged("IsFrozen");
                }
            }
        }

        public KurvendisplayDlgModel()
        {
            data = new CurveDisplayData();
            curves = new ObservableCollection<CurveData>();
        }

        public void AddCurve(CurveData result)
        {
            curves.Add(result);
        }

        public void UpdateView()
        {
            Data.UpDateCurvesToView(curves);
        }

        public void UpdateCurves()
        {
            OnPropertyChanged("Curves");
        }

        public void ClearCurves()
        {
            curves.Clear();
        }
    }
}
