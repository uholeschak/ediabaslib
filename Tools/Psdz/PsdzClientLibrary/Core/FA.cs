using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Utility;

namespace PsdzClient.Core
{
	public class FA : INotifyPropertyChanged, IFa, IFARuleEvaluation, IReactorFa
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

        public short? SA_ANZ
		{
			get
			{
				return this.sA_ANZField;
			}
			set
			{
				if (this.sA_ANZField != null)
				{
					if (!this.sA_ANZField.Equals(value))
					{
						this.sA_ANZField = value;
						this.OnPropertyChanged("SA_ANZ");
						return;
					}
				}
				else
				{
					this.sA_ANZField = value;
					this.OnPropertyChanged("SA_ANZ");
				}
			}
		}

		public ObservableCollection<string> SA
		{
			get
			{
				return this.saField;
			}
			set
			{
				if (this.saField != null)
				{
					if (!this.saField.Equals(value))
					{
						this.saField = value;
						this.OnPropertyChanged("SA");
						return;
					}
				}
				else
				{
					this.saField = value;
					this.OnPropertyChanged("SA");
				}
			}
		}

		public ObservableCollection<SALAPALocalizedEntry> SALAPA
		{
			get
			{
				return this.sALAPAField;
			}
			set
			{
				if (this.sALAPAField != null)
				{
					if (!this.sALAPAField.Equals(value))
					{
						this.sALAPAField = value;
						this.OnPropertyChanged("SALAPA");
						return;
					}
				}
				else
				{
					this.sALAPAField = value;
					this.OnPropertyChanged("SALAPA");
				}
			}
		}

		public short? E_WORT_ANZ
		{
			get
			{
				return this.e_WORT_ANZField;
			}
			set
			{
				if (this.e_WORT_ANZField != null)
				{
					if (!this.e_WORT_ANZField.Equals(value))
					{
						this.e_WORT_ANZField = value;
						this.OnPropertyChanged("E_WORT_ANZ");
						return;
					}
				}
				else
				{
					this.e_WORT_ANZField = value;
					this.OnPropertyChanged("E_WORT_ANZ");
				}
			}
		}

		public ObservableCollection<string> E_WORT
		{
			get
			{
				return this.e_WORTField;
			}
			set
			{
				if (this.e_WORTField != null)
				{
					if (!this.e_WORTField.Equals(value))
					{
						this.e_WORTField = value;
						this.OnPropertyChanged("E_WORT");
						return;
					}
				}
				else
				{
					this.e_WORTField = value;
					this.OnPropertyChanged("E_WORT");
				}
			}
		}

		public short? HO_WORT_ANZ
		{
			get
			{
				return this.hO_WORT_ANZField;
			}
			set
			{
				if (this.hO_WORT_ANZField != null)
				{
					if (!this.hO_WORT_ANZField.Equals(value))
					{
						this.hO_WORT_ANZField = value;
						this.OnPropertyChanged("HO_WORT_ANZ");
						return;
					}
				}
				else
				{
					this.hO_WORT_ANZField = value;
					this.OnPropertyChanged("HO_WORT_ANZ");
				}
			}
		}

		public ObservableCollection<string> HO_WORT
		{
			get
			{
				return this.hO_WORTField;
			}
			set
			{
				if (this.hO_WORTField != null)
				{
					if (!this.hO_WORTField.Equals(value))
					{
						this.hO_WORTField = value;
						this.OnPropertyChanged("HO_WORT");
						return;
					}
				}
				else
				{
					this.hO_WORTField = value;
					this.OnPropertyChanged("HO_WORT");
				}
			}
		}

		public short? ZUSBAU_ANZ
		{
			get
			{
				return this.zUSBAU_ANZField;
			}
			set
			{
				if (this.zUSBAU_ANZField != null)
				{
					if (!this.zUSBAU_ANZField.Equals(value))
					{
						this.zUSBAU_ANZField = value;
						this.OnPropertyChanged("ZUSBAU_ANZ");
						return;
					}
				}
				else
				{
					this.zUSBAU_ANZField = value;
					this.OnPropertyChanged("ZUSBAU_ANZ");
				}
			}
		}

		public ObservableCollection<string> ZUSBAU_WORT
		{
			get
			{
				return this.zUSBAU_WORTField;
			}
			set
			{
				if (this.zUSBAU_WORTField != null)
				{
					if (!this.zUSBAU_WORTField.Equals(value))
					{
						this.zUSBAU_WORTField = value;
						this.OnPropertyChanged("ZUSBAU_WORT");
						return;
					}
				}
				else
				{
					this.zUSBAU_WORTField = value;
					this.OnPropertyChanged("ZUSBAU_WORT");
				}
			}
		}

		public string POLSTER
		{
			get
			{
				return this.pOLSTERField;
			}
			set
			{
				if (this.pOLSTERField != null)
				{
					if (!this.pOLSTERField.Equals(value))
					{
						this.pOLSTERField = value;
						this.OnPropertyChanged("POLSTER");
						return;
					}
				}
				else
				{
					this.pOLSTERField = value;
					this.OnPropertyChanged("POLSTER");
				}
			}
		}

		public string POLSTER_TEXT
		{
			get
			{
				return this.pOLSTER_TEXTField;
			}
			set
			{
				if (this.pOLSTER_TEXTField != null)
				{
					if (!this.pOLSTER_TEXTField.Equals(value))
					{
						this.pOLSTER_TEXTField = value;
						this.OnPropertyChanged("POLSTER_TEXT");
						return;
					}
				}
				else
				{
					this.pOLSTER_TEXTField = value;
					this.OnPropertyChanged("POLSTER_TEXT");
				}
			}
		}

		public string LACK
		{
			get
			{
				return this.lACKField;
			}
			set
			{
				if (this.lACKField != null)
				{
					if (!this.lACKField.Equals(value))
					{
						this.lACKField = value;
						this.OnPropertyChanged("LACK");
						return;
					}
				}
				else
				{
					this.lACKField = value;
					this.OnPropertyChanged("LACK");
				}
			}
		}

		public string LACK_TEXT
		{
			get
			{
				return this.lACK_TEXTField;
			}
			set
			{
				if (this.lACK_TEXTField != null)
				{
					if (!this.lACK_TEXTField.Equals(value))
					{
						this.lACK_TEXTField = value;
						this.OnPropertyChanged("LACK_TEXT");
						return;
					}
				}
				else
				{
					this.lACK_TEXTField = value;
					this.OnPropertyChanged("LACK_TEXT");
				}
			}
		}

		public string C_DATE
		{
			get
			{
				return this.c_DATEField;
			}
			set
			{
				if (this.c_DATEField != null)
				{
					if (!this.c_DATEField.Equals(value))
					{
						this.c_DATEField = value;
						this.OnPropertyChanged("C_DATE");
						return;
					}
				}
				else
				{
					this.c_DATEField = value;
					this.OnPropertyChanged("C_DATE");
				}
			}
		}

		public DateTime? C_DATETIME
		{
			get
			{
				return this.c_DATETIMEField;
			}
			set
			{
				if (this.c_DATETIMEField != null)
				{
					if (!this.c_DATETIMEField.Equals(value))
					{
						this.c_DATETIMEField = value;
						this.OnPropertyChanged("C_DATETIME");
						return;
					}
				}
				else
				{
					this.c_DATETIMEField = value;
					this.OnPropertyChanged("C_DATETIME");
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
				return this.vERSIONField;
			}
			set
			{
				if (this.vERSIONField != null)
				{
					if (!this.vERSIONField.Equals(value))
					{
						this.vERSIONField = value;
						this.OnPropertyChanged("VERSION");
						return;
					}
				}
				else
				{
					this.vERSIONField = value;
					this.OnPropertyChanged("VERSION");
				}
			}
		}

		public string BR
		{
			get
			{
				return this.brField;
			}
			set
			{
				if (this.brField != null)
				{
					if (!this.brField.Equals(value))
					{
						this.brField = value;
						this.OnPropertyChanged("BR");
						return;
					}
				}
				else
				{
					this.brField = value;
					this.OnPropertyChanged("BR");
				}
			}
		}

		public string STANDARD_FA
		{
			get
			{
				return this.sTANDARD_FAField;
			}
			set
			{
				if (this.sTANDARD_FAField != null)
				{
					if (!this.sTANDARD_FAField.Equals(value))
					{
						this.sTANDARD_FAField = value;
						this.OnPropertyChanged("STANDARD_FA");
						return;
					}
				}
				else
				{
					this.sTANDARD_FAField = value;
					this.OnPropertyChanged("STANDARD_FA");
				}
			}
		}

		public string TYPE
		{
			get
			{
				return this.tYPEField;
			}
			set
			{
				if (this.tYPEField != null)
				{
					if (!this.tYPEField.Equals(value))
					{
						this.tYPEField = value;
						this.OnPropertyChanged("TYPE");
						return;
					}
				}
				else
				{
					this.tYPEField = value;
					this.OnPropertyChanged("TYPE");
				}
			}
		}

		[DefaultValue(false)]
		public bool AlreadyDone
		{
			get
			{
				return this.alreadyDoneField;
			}
			set
			{
				if (!this.alreadyDoneField.Equals(value))
				{
					this.alreadyDoneField = value;
					this.OnPropertyChanged("AlreadyDone");
				}
			}
		}

		public ObservableCollection<LocalizedSAItem> SaLocalizedItems
		{
			get
			{
				return this.saLocalizedItemsField;
			}
			set
			{
				if (this.saLocalizedItemsField != null)
				{
					if (!this.saLocalizedItemsField.Equals(value))
					{
						this.saLocalizedItemsField = value;
						this.OnPropertyChanged("SaLocalizedItems");
						return;
					}
				}
				else
				{
					this.saLocalizedItemsField = value;
					this.OnPropertyChanged("SaLocalizedItems");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [XmlIgnore]
        IEnumerable<string> IFa.DealerInstalledSA => null;

        [XmlIgnore]
        IEnumerable<string> IFa.E_WORT => E_WORT;

        [XmlIgnore]
        IEnumerable<string> IFa.HO_WORT => HO_WORT;

        [XmlIgnore]
        IEnumerable<string> IFa.SA => SA;

        [XmlIgnore]
        IEnumerable<string> IFa.ZUSBAU_WORT => ZUSBAU_WORT;

        [XmlIgnore]
        ICollection<LocalizedSAItem> IFa.SaLocalizedItems => SaLocalizedItems;

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}#{1}*{2}%{3}&{4}{5}{6}{7}", FormatConverter.ConvertToBn2020ConformModelSeries(BR), C_DATE, TYPE, LACK, POLSTER, ConcatStrElems(SA, "$"), ConcatStrElems(E_WORT, "-"), ConcatStrElems(HO_WORT, "+"));
        }

        // ToDo: Check on update
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

        // ToDo: Check on update
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
            if (elems != null && elems.Any())
            {
                string text = new List<string>(elems).Aggregate((string intermediate, string elem) => intermediate + sep + elem);
                if (!string.IsNullOrEmpty(text))
                {
                    text = sep + text;
                }
                return text;
            }
            return string.Empty;
        }
	}
}
