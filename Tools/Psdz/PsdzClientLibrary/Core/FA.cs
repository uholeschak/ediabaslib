using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Programming;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    public class FA : INotifyPropertyChanged, BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa, IFARuleEvaluation, IReactorFa
    {
        private short? sA_ANZField;
        private ObservableCollection<string> saField;
        private ObservableCollection<LocalizedSAItem> saLocalizedItemsField;
        private ObservableCollection<SALAPALocalizedEntry> sALAPAField;
        private short? e_WORT_ANZField;
        private ObservableCollection<string> e_WORTField;
        private short? hO_WORT_ANZField;
        private ObservableCollection<string> hO_WORTField;
        private short? zUSBAU_ANZField;
        private ObservableCollection<string> zUSBAU_WORTField;
        private string pOLSTERField;
        private string pOLSTER_TEXTField;
        private string lACKField;
        private string lACK_TEXTField;
        private string fAHRZEUG_KATEGORIEField;
        private string kONTROLL_KLASSEField;
        private string c_DATEField;
        private DateTime? c_DATETIMEField;
        private string vERSIONField;
        private string brField;
        private string sTANDARD_FAField;
        private string tYPEField;
        private bool alreadyDoneField;
        public short? SA_ANZ
        {
            get
            {
                return sA_ANZField;
            }

            set
            {
                if (sA_ANZField.HasValue)
                {
                    if (!sA_ANZField.Equals(value))
                    {
                        sA_ANZField = value;
                        OnPropertyChanged("SA_ANZ");
                    }
                }
                else
                {
                    sA_ANZField = value;
                    OnPropertyChanged("SA_ANZ");
                }
            }
        }

        public ObservableCollection<string> SA
        {
            get
            {
                return saField;
            }

            set
            {
                if (saField != value)
                {
                    saField = value;
                    OnPropertyChanged("SA");
                }
            }
        }

        public ObservableCollection<SALAPALocalizedEntry> SALAPA
        {
            get
            {
                return sALAPAField;
            }

            set
            {
                if (sALAPAField != null)
                {
                    if (!sALAPAField.Equals(value))
                    {
                        sALAPAField = value;
                        OnPropertyChanged("SALAPA");
                    }
                }
                else
                {
                    sALAPAField = value;
                    OnPropertyChanged("SALAPA");
                }
            }
        }

        public short? E_WORT_ANZ
        {
            get
            {
                return e_WORT_ANZField;
            }

            set
            {
                if (e_WORT_ANZField.HasValue)
                {
                    if (!e_WORT_ANZField.Equals(value))
                    {
                        e_WORT_ANZField = value;
                        OnPropertyChanged("E_WORT_ANZ");
                    }
                }
                else
                {
                    e_WORT_ANZField = value;
                    OnPropertyChanged("E_WORT_ANZ");
                }
            }
        }

        public ObservableCollection<string> E_WORT
        {
            get
            {
                return e_WORTField;
            }

            set
            {
                if (e_WORTField != null)
                {
                    if (!e_WORTField.Equals(value))
                    {
                        e_WORTField = value;
                        OnPropertyChanged("E_WORT");
                    }
                }
                else
                {
                    e_WORTField = value;
                    OnPropertyChanged("E_WORT");
                }
            }
        }

        public short? HO_WORT_ANZ
        {
            get
            {
                return hO_WORT_ANZField;
            }

            set
            {
                if (hO_WORT_ANZField.HasValue)
                {
                    if (!hO_WORT_ANZField.Equals(value))
                    {
                        hO_WORT_ANZField = value;
                        OnPropertyChanged("HO_WORT_ANZ");
                    }
                }
                else
                {
                    hO_WORT_ANZField = value;
                    OnPropertyChanged("HO_WORT_ANZ");
                }
            }
        }

        public ObservableCollection<string> HO_WORT
        {
            get
            {
                return hO_WORTField;
            }

            set
            {
                if (hO_WORTField != null)
                {
                    if (!hO_WORTField.Equals(value))
                    {
                        hO_WORTField = value;
                        OnPropertyChanged("HO_WORT");
                    }
                }
                else
                {
                    hO_WORTField = value;
                    OnPropertyChanged("HO_WORT");
                }
            }
        }

        public short? ZUSBAU_ANZ
        {
            get
            {
                return zUSBAU_ANZField;
            }

            set
            {
                if (zUSBAU_ANZField.HasValue)
                {
                    if (!zUSBAU_ANZField.Equals(value))
                    {
                        zUSBAU_ANZField = value;
                        OnPropertyChanged("ZUSBAU_ANZ");
                    }
                }
                else
                {
                    zUSBAU_ANZField = value;
                    OnPropertyChanged("ZUSBAU_ANZ");
                }
            }
        }

        public ObservableCollection<string> ZUSBAU_WORT
        {
            get
            {
                return zUSBAU_WORTField;
            }

            set
            {
                if (zUSBAU_WORTField != null)
                {
                    if (!zUSBAU_WORTField.Equals(value))
                    {
                        zUSBAU_WORTField = value;
                        OnPropertyChanged("ZUSBAU_WORT");
                    }
                }
                else
                {
                    zUSBAU_WORTField = value;
                    OnPropertyChanged("ZUSBAU_WORT");
                }
            }
        }

        public string POLSTER
        {
            get
            {
                return pOLSTERField;
            }

            set
            {
                if (pOLSTERField != null)
                {
                    if (!pOLSTERField.Equals(value))
                    {
                        pOLSTERField = value;
                        OnPropertyChanged("POLSTER");
                    }
                }
                else
                {
                    pOLSTERField = value;
                    OnPropertyChanged("POLSTER");
                }
            }
        }

        public string POLSTER_TEXT
        {
            get
            {
                return pOLSTER_TEXTField;
            }

            set
            {
                if (pOLSTER_TEXTField != null)
                {
                    if (!pOLSTER_TEXTField.Equals(value))
                    {
                        pOLSTER_TEXTField = value;
                        OnPropertyChanged("POLSTER_TEXT");
                    }
                }
                else
                {
                    pOLSTER_TEXTField = value;
                    OnPropertyChanged("POLSTER_TEXT");
                }
            }
        }

        public string LACK
        {
            get
            {
                return lACKField;
            }

            set
            {
                if (lACKField != null)
                {
                    if (!lACKField.Equals(value))
                    {
                        lACKField = value;
                        OnPropertyChanged("LACK");
                    }
                }
                else
                {
                    lACKField = value;
                    OnPropertyChanged("LACK");
                }
            }
        }

        public string LACK_TEXT
        {
            get
            {
                return lACK_TEXTField;
            }

            set
            {
                if (lACK_TEXTField != null)
                {
                    if (!lACK_TEXTField.Equals(value))
                    {
                        lACK_TEXTField = value;
                        OnPropertyChanged("LACK_TEXT");
                    }
                }
                else
                {
                    lACK_TEXTField = value;
                    OnPropertyChanged("LACK_TEXT");
                }
            }
        }

        public string C_DATE
        {
            get
            {
                return c_DATEField;
            }

            set
            {
                if (c_DATEField != null)
                {
                    if (!c_DATEField.Equals(value))
                    {
                        c_DATEField = value;
                        OnPropertyChanged("C_DATE");
                    }
                }
                else
                {
                    c_DATEField = value;
                    OnPropertyChanged("C_DATE");
                }
            }
        }

        public DateTime? C_DATETIME
        {
            get
            {
                return c_DATETIMEField;
            }

            set
            {
                if (c_DATETIMEField.HasValue)
                {
                    if (!c_DATETIMEField.Equals(value))
                    {
                        c_DATETIMEField = value;
                        OnPropertyChanged("C_DATETIME");
                    }
                }
                else
                {
                    c_DATETIMEField = value;
                    OnPropertyChanged("C_DATETIME");
                }
            }
        }

        public string FAHRZEUG_KATEGORIE
        {
            get
            {
                return fAHRZEUG_KATEGORIEField;
            }

            set
            {
                if (fAHRZEUG_KATEGORIEField != null)
                {
                    if (!fAHRZEUG_KATEGORIEField.Equals(value))
                    {
                        fAHRZEUG_KATEGORIEField = value;
                        OnPropertyChanged("FAHRZEUG_KATEGORIE");
                    }
                }
                else
                {
                    fAHRZEUG_KATEGORIEField = value;
                    OnPropertyChanged("FAHRZEUG_KATEGORIE");
                }
            }
        }

        public string KONTROLL_KLASSE
        {
            get
            {
                return kONTROLL_KLASSEField;
            }

            set
            {
                if (kONTROLL_KLASSEField != null)
                {
                    if (!kONTROLL_KLASSEField.Equals(value))
                    {
                        kONTROLL_KLASSEField = value;
                        OnPropertyChanged("KONTROLL_KLASSE");
                    }
                }
                else
                {
                    kONTROLL_KLASSEField = value;
                    OnPropertyChanged("KONTROLL_KLASSE");
                }
            }
        }

        public string VERSION
        {
            get
            {
                return vERSIONField;
            }

            set
            {
                if (vERSIONField != null)
                {
                    if (!vERSIONField.Equals(value))
                    {
                        vERSIONField = value;
                        OnPropertyChanged("VERSION");
                    }
                }
                else
                {
                    vERSIONField = value;
                    OnPropertyChanged("VERSION");
                }
            }
        }

        public string BR
        {
            get
            {
                return brField;
            }

            set
            {
                if (brField != null)
                {
                    if (!brField.Equals(value))
                    {
                        brField = value;
                        OnPropertyChanged("BR");
                    }
                }
                else
                {
                    brField = value;
                    OnPropertyChanged("BR");
                }
            }
        }

        public string STANDARD_FA
        {
            get
            {
                return sTANDARD_FAField;
            }

            set
            {
                if (sTANDARD_FAField != null)
                {
                    if (!sTANDARD_FAField.Equals(value))
                    {
                        sTANDARD_FAField = value;
                        OnPropertyChanged("STANDARD_FA");
                    }
                }
                else
                {
                    sTANDARD_FAField = value;
                    OnPropertyChanged("STANDARD_FA");
                }
            }
        }

        public string TYPE
        {
            get
            {
                return tYPEField;
            }

            set
            {
                if (tYPEField != null)
                {
                    if (!tYPEField.Equals(value))
                    {
                        tYPEField = value;
                        OnPropertyChanged("TYPE");
                    }
                }
                else
                {
                    tYPEField = value;
                    OnPropertyChanged("TYPE");
                }
            }
        }

        [DefaultValue(false)]
        public bool AlreadyDone
        {
            get
            {
                return alreadyDoneField;
            }

            set
            {
                if (!alreadyDoneField.Equals(value))
                {
                    alreadyDoneField = value;
                    OnPropertyChanged("AlreadyDone");
                }
            }
        }

        public ObservableCollection<LocalizedSAItem> SaLocalizedItems
        {
            get
            {
                return saLocalizedItemsField;
            }

            set
            {
                if (saLocalizedItemsField != null)
                {
                    if (!saLocalizedItemsField.Equals(value))
                    {
                        saLocalizedItemsField = value;
                        OnPropertyChanged("SaLocalizedItems");
                    }
                }
                else
                {
                    saLocalizedItemsField = value;
                    OnPropertyChanged("SaLocalizedItems");
                }
            }
        }

        [XmlIgnore]
        IEnumerable<string> BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa.DealerInstalledSA => null;

        [XmlIgnore]
        IEnumerable<string> BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa.E_WORT => E_WORT;

        [XmlIgnore]
        IEnumerable<string> BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa.HO_WORT => HO_WORT;

        [XmlIgnore]
        IEnumerable<string> BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa.SA => SA;

        [XmlIgnore]
        IEnumerable<string> BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa.ZUSBAU_WORT => ZUSBAU_WORT;

        [XmlIgnore]
        ICollection<LocalizedSAItem> BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa.SaLocalizedItems => SaLocalizedItems;

        public event PropertyChangedEventHandler PropertyChanged;
        public FA()
        {
            zUSBAU_WORTField = new ObservableCollection<string>();
            hO_WORTField = new ObservableCollection<string>();
            e_WORTField = new ObservableCollection<string>();
            sALAPAField = new ObservableCollection<SALAPALocalizedEntry>();
            saField = new ObservableCollection<string>();
            saLocalizedItemsField = new ObservableCollection<LocalizedSAItem>();
            sA_ANZField = 0;
            e_WORT_ANZField = 0;
            hO_WORT_ANZField = 0;
            zUSBAU_ANZField = 0;
            alreadyDoneField = false;
            sTANDARD_FAField = string.Empty;
            lACKField = string.Empty;
            pOLSTERField = string.Empty;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}#{1}*{2}%{3}&{4}{5}{6}{7}", GetFormattedModelSeries(BR), C_DATE, TYPE, LACK, POLSTER, ConcatStrElems(SA, "$"), ConcatStrElems(E_WORT, "-"), ConcatStrElems(HO_WORT, "+"));
        }

        [PreserveSource(Hint = "Changed to IProgrammingService2", OriginalHash = "56C08045066BBE0CB56F6D078FBF66CB")]
        private string GetFormattedModelSeries(string baureihe)
        {
            try
            {
                using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
                {
                    if (istaIcsServiceClient.IsAvailable() && istaIcsServiceClient.GetFeatureEnabledStatus("UsePsdzSeriesFormatter", istaIcsServiceClient.IsAvailable()).IsActive)
                    {
                        return ServiceLocator.Current.GetService<IProgrammingService2>()?.Psdz?.BaureiheUtilityService?.GetBaureihe(baureihe) ?? FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
                    }
                }

                return FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
            }
            catch (Exception exception)
            {
                Log.ErrorException("Baureihereader logged exception while reading baureihe from psdz.", exception);
                return FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
            }
        }

        public string ExtractEreihe()
        {
            try
            {
                if (!string.IsNullOrEmpty(BR) && BR.Length == 4)
                {
                    if (BR.EndsWith("_", StringComparison.Ordinal))
                    {
                        string text = BR.TrimEnd('_');
                        if (Regex.Match(text, "[ERKHM]\\d\\d").Success)
                        {
                            return text;
                        }
                    }

                    if (BR.StartsWith("RR", StringComparison.OrdinalIgnoreCase))
                    {
                        string text2 = BR.TrimEnd('_');
                        if (Regex.Match(text2, "^RR\\d$").Success)
                        {
                            return text2;
                        }

                        if (Regex.Match(text2, "^RR0\\d$").Success)
                        {
                            return "RR" + BR.Substring(3, 1);
                        }

                        if (Regex.Match(text2, "^RR1\\d$").Success)
                        {
                            return text2;
                        }
                    }

                    string text3 = BR.Substring(0, 1);
                    string text4 = BR.Substring(2, 2);
                    return text3 + text4;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FA.ExtractEreihe()", exception);
            }

            return null;
        }

        public string ExtractType()
        {
            if (!string.IsNullOrEmpty(STANDARD_FA))
            {
                Match match = Regex.Match(STANDARD_FA, "\\*(?<TYPE>\\w{4})");
                if (match.Success)
                {
                    return match.Groups["TYPE"].Value;
                }
            }

            return null;
        }

        public static string ConcatStrElems(IEnumerable<string> elems, string sep)
        {
            if (elems == null || !elems.Any())
            {
                return string.Empty;
            }

            string text = new List<string>(elems).Aggregate((string intermediate, string elem) => intermediate + sep + elem);
            if (!string.IsNullOrEmpty(text))
            {
                text = sep + text;
            }

            return text;
        }
    }
}