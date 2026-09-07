using PsdzClient.Core;
using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUCLIQUES : INotifyPropertyChanged, IXepEcuCliques
    {
        private decimal idField;
        private string cLIQUENKURZBEZEICHNUNGField;
        private decimal? tITLEIDField;
        private string tITLE_DEDEField;
        private string tITLE_ENGBField;
        private string tITLE_ENUSField;
        private string tITLE_FRField;
        private string tITLE_THField;
        private string tITLE_SVField;
        private string tITLE_ITField;
        private string tITLE_ESField;
        private string tITLE_IDField;
        private string tITLE_KOField;
        private string tITLE_ELField;
        private string tITLE_TRField;
        private string tITLE_ZHCNField;
        private string tITLE_RUField;
        private string tITLE_NLField;
        private string tITLE_PTField;
        private string tITLE_ZHTWField;
        private string tITLE_JAField;
        private string tITLE_CSCZField;
        private string tITLE_PLPLField;
        private decimal? eCUREPIDField;
        public virtual bool IsValid => true;

        public decimal ID
        {
            get
            {
                return idField;
            }

            set
            {
                _ = idField;
                if (!idField.Equals(value))
                {
                    idField = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public string CLIQUENKURZBEZEICHNUNG
        {
            get
            {
                return cLIQUENKURZBEZEICHNUNGField;
            }

            set
            {
                if (cLIQUENKURZBEZEICHNUNGField != null)
                {
                    if (!cLIQUENKURZBEZEICHNUNGField.Equals(value))
                    {
                        cLIQUENKURZBEZEICHNUNGField = value;
                        OnPropertyChanged("CLIQUENKURZBEZEICHNUNG");
                    }
                }
                else
                {
                    cLIQUENKURZBEZEICHNUNGField = value;
                    OnPropertyChanged("CLIQUENKURZBEZEICHNUNG");
                }
            }
        }

        public decimal? TITLEID
        {
            get
            {
                return tITLEIDField;
            }

            set
            {
                if (tITLEIDField.HasValue)
                {
                    if (!tITLEIDField.Equals(value))
                    {
                        tITLEIDField = value;
                        OnPropertyChanged("TITLEID");
                    }
                }
                else
                {
                    tITLEIDField = value;
                    OnPropertyChanged("TITLEID");
                }
            }
        }

        public string TITLE_DEDE
        {
            get
            {
                return tITLE_DEDEField;
            }

            set
            {
                if (tITLE_DEDEField != null)
                {
                    if (!tITLE_DEDEField.Equals(value))
                    {
                        tITLE_DEDEField = value;
                        OnPropertyChanged("TITLE_DEDE");
                    }
                }
                else
                {
                    tITLE_DEDEField = value;
                    OnPropertyChanged("TITLE_DEDE");
                }
            }
        }

        public string TITLE_ENGB
        {
            get
            {
                return tITLE_ENGBField;
            }

            set
            {
                if (tITLE_ENGBField != null)
                {
                    if (!tITLE_ENGBField.Equals(value))
                    {
                        tITLE_ENGBField = value;
                        OnPropertyChanged("TITLE_ENGB");
                    }
                }
                else
                {
                    tITLE_ENGBField = value;
                    OnPropertyChanged("TITLE_ENGB");
                }
            }
        }

        public string TITLE_ENUS
        {
            get
            {
                return tITLE_ENUSField;
            }

            set
            {
                if (tITLE_ENUSField != null)
                {
                    if (!tITLE_ENUSField.Equals(value))
                    {
                        tITLE_ENUSField = value;
                        OnPropertyChanged("TITLE_ENUS");
                    }
                }
                else
                {
                    tITLE_ENUSField = value;
                    OnPropertyChanged("TITLE_ENUS");
                }
            }
        }

        public string TITLE_FR
        {
            get
            {
                return tITLE_FRField;
            }

            set
            {
                if (tITLE_FRField != null)
                {
                    if (!tITLE_FRField.Equals(value))
                    {
                        tITLE_FRField = value;
                        OnPropertyChanged("TITLE_FR");
                    }
                }
                else
                {
                    tITLE_FRField = value;
                    OnPropertyChanged("TITLE_FR");
                }
            }
        }

        public string TITLE_TH
        {
            get
            {
                return tITLE_THField;
            }

            set
            {
                if (tITLE_THField != null)
                {
                    if (!tITLE_THField.Equals(value))
                    {
                        tITLE_THField = value;
                        OnPropertyChanged("TITLE_TH");
                    }
                }
                else
                {
                    tITLE_THField = value;
                    OnPropertyChanged("TITLE_TH");
                }
            }
        }

        public string TITLE_SV
        {
            get
            {
                return tITLE_SVField;
            }

            set
            {
                if (tITLE_SVField != null)
                {
                    if (!tITLE_SVField.Equals(value))
                    {
                        tITLE_SVField = value;
                        OnPropertyChanged("TITLE_SV");
                    }
                }
                else
                {
                    tITLE_SVField = value;
                    OnPropertyChanged("TITLE_SV");
                }
            }
        }

        public string TITLE_IT
        {
            get
            {
                return tITLE_ITField;
            }

            set
            {
                if (tITLE_ITField != null)
                {
                    if (!tITLE_ITField.Equals(value))
                    {
                        tITLE_ITField = value;
                        OnPropertyChanged("TITLE_IT");
                    }
                }
                else
                {
                    tITLE_ITField = value;
                    OnPropertyChanged("TITLE_IT");
                }
            }
        }

        public string TITLE_ES
        {
            get
            {
                return tITLE_ESField;
            }

            set
            {
                if (tITLE_ESField != null)
                {
                    if (!tITLE_ESField.Equals(value))
                    {
                        tITLE_ESField = value;
                        OnPropertyChanged("TITLE_ES");
                    }
                }
                else
                {
                    tITLE_ESField = value;
                    OnPropertyChanged("TITLE_ES");
                }
            }
        }

        public string TITLE_ID
        {
            get
            {
                return tITLE_IDField;
            }

            set
            {
                if (tITLE_IDField != null)
                {
                    if (!tITLE_IDField.Equals(value))
                    {
                        tITLE_IDField = value;
                        OnPropertyChanged("TITLE_ID");
                    }
                }
                else
                {
                    tITLE_IDField = value;
                    OnPropertyChanged("TITLE_ID");
                }
            }
        }

        public string TITLE_KO
        {
            get
            {
                return tITLE_KOField;
            }

            set
            {
                if (tITLE_KOField != null)
                {
                    if (!tITLE_KOField.Equals(value))
                    {
                        tITLE_KOField = value;
                        OnPropertyChanged("TITLE_KO");
                    }
                }
                else
                {
                    tITLE_KOField = value;
                    OnPropertyChanged("TITLE_KO");
                }
            }
        }

        public string TITLE_EL
        {
            get
            {
                return tITLE_ELField;
            }

            set
            {
                if (tITLE_ELField != null)
                {
                    if (!tITLE_ELField.Equals(value))
                    {
                        tITLE_ELField = value;
                        OnPropertyChanged("TITLE_EL");
                    }
                }
                else
                {
                    tITLE_ELField = value;
                    OnPropertyChanged("TITLE_EL");
                }
            }
        }

        public string TITLE_TR
        {
            get
            {
                return tITLE_TRField;
            }

            set
            {
                if (tITLE_TRField != null)
                {
                    if (!tITLE_TRField.Equals(value))
                    {
                        tITLE_TRField = value;
                        OnPropertyChanged("TITLE_TR");
                    }
                }
                else
                {
                    tITLE_TRField = value;
                    OnPropertyChanged("TITLE_TR");
                }
            }
        }

        public string TITLE_ZHCN
        {
            get
            {
                return tITLE_ZHCNField;
            }

            set
            {
                if (tITLE_ZHCNField != null)
                {
                    if (!tITLE_ZHCNField.Equals(value))
                    {
                        tITLE_ZHCNField = value;
                        OnPropertyChanged("TITLE_ZHCN");
                    }
                }
                else
                {
                    tITLE_ZHCNField = value;
                    OnPropertyChanged("TITLE_ZHCN");
                }
            }
        }

        public string TITLE_RU
        {
            get
            {
                return tITLE_RUField;
            }

            set
            {
                if (tITLE_RUField != null)
                {
                    if (!tITLE_RUField.Equals(value))
                    {
                        tITLE_RUField = value;
                        OnPropertyChanged("TITLE_RU");
                    }
                }
                else
                {
                    tITLE_RUField = value;
                    OnPropertyChanged("TITLE_RU");
                }
            }
        }

        public string TITLE_NL
        {
            get
            {
                return tITLE_NLField;
            }

            set
            {
                if (tITLE_NLField != null)
                {
                    if (!tITLE_NLField.Equals(value))
                    {
                        tITLE_NLField = value;
                        OnPropertyChanged("TITLE_NL");
                    }
                }
                else
                {
                    tITLE_NLField = value;
                    OnPropertyChanged("TITLE_NL");
                }
            }
        }

        public string TITLE_PT
        {
            get
            {
                return tITLE_PTField;
            }

            set
            {
                if (tITLE_PTField != null)
                {
                    if (!tITLE_PTField.Equals(value))
                    {
                        tITLE_PTField = value;
                        OnPropertyChanged("TITLE_PT");
                    }
                }
                else
                {
                    tITLE_PTField = value;
                    OnPropertyChanged("TITLE_PT");
                }
            }
        }

        public string TITLE_ZHTW
        {
            get
            {
                return tITLE_ZHTWField;
            }

            set
            {
                if (tITLE_ZHTWField != null)
                {
                    if (!tITLE_ZHTWField.Equals(value))
                    {
                        tITLE_ZHTWField = value;
                        OnPropertyChanged("TITLE_ZHTW");
                    }
                }
                else
                {
                    tITLE_ZHTWField = value;
                    OnPropertyChanged("TITLE_ZHTW");
                }
            }
        }

        public string TITLE_JA
        {
            get
            {
                return tITLE_JAField;
            }

            set
            {
                if (tITLE_JAField != null)
                {
                    if (!tITLE_JAField.Equals(value))
                    {
                        tITLE_JAField = value;
                        OnPropertyChanged("TITLE_JA");
                    }
                }
                else
                {
                    tITLE_JAField = value;
                    OnPropertyChanged("TITLE_JA");
                }
            }
        }

        public string TITLE_CSCZ
        {
            get
            {
                return tITLE_CSCZField;
            }

            set
            {
                if (tITLE_CSCZField != null)
                {
                    if (!tITLE_CSCZField.Equals(value))
                    {
                        tITLE_CSCZField = value;
                        OnPropertyChanged("TITLE_CSCZ");
                    }
                }
                else
                {
                    tITLE_CSCZField = value;
                    OnPropertyChanged("TITLE_CSCZ");
                }
            }
        }

        public string TITLE_PLPL
        {
            get
            {
                return tITLE_PLPLField;
            }

            set
            {
                if (tITLE_PLPLField != null)
                {
                    if (!tITLE_PLPLField.Equals(value))
                    {
                        tITLE_PLPLField = value;
                        OnPropertyChanged("TITLE_PLPL");
                    }
                }
                else
                {
                    tITLE_PLPLField = value;
                    OnPropertyChanged("TITLE_PLPL");
                }
            }
        }

        public decimal? ECUREPID
        {
            get
            {
                return eCUREPIDField;
            }

            set
            {
                if (eCUREPIDField.HasValue)
                {
                    if (!eCUREPIDField.Equals(value))
                    {
                        eCUREPIDField = value;
                        OnPropertyChanged("ECUREPID");
                    }
                }
                else
                {
                    eCUREPIDField = value;
                    OnPropertyChanged("ECUREPID");
                }
            }
        }

        private string ExplicitTitle { get; set; }

        public virtual string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(ExplicitTitle))
                {
                    return ExplicitTitle;
                }

                string text;
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        text = TITLE_DEDE;
                        break;
                    case "en-GB":
                        text = TITLE_ENGB;
                        break;
                    case "en-US":
                        text = TITLE_ENUS;
                        break;
                    case "fr-FR":
                        text = TITLE_FR;
                        break;
                    case "es-ES":
                        text = TITLE_ES;
                        break;
                    case "th-TH":
                        text = TITLE_TH;
                        break;
                    case "tr-TR":
                        text = TITLE_TR;
                        break;
                    case "el-GR":
                        text = TITLE_EL;
                        break;
                    case "ja-JP":
                        text = TITLE_JA;
                        break;
                    case "ru-RU":
                        text = TITLE_RU;
                        break;
                    case "it-IT":
                        text = TITLE_IT;
                        break;
                    case "nl-NL":
                        text = TITLE_NL;
                        break;
                    case "pl-PL":
                        text = TITLE_PLPL;
                        break;
                    case "cs-CZ":
                        text = TITLE_CSCZ;
                        break;
                    case "pt-PT":
                        text = TITLE_PT;
                        break;
                    case "sv-SE":
                        text = TITLE_SV;
                        break;
                    case "zh-CN":
                        text = TITLE_ZHCN;
                        break;
                    case "zh-TW":
                        text = TITLE_ZHTW;
                        break;
                    case "ko-KR":
                        text = TITLE_KO;
                        break;
                    default:
                        Log.Warning("XEP_ECUCLIQUES.get_Title()", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = TITLE_ENGB;
                        break;
                }

                if (string.IsNullOrEmpty(text))
                {
                    return TITLE_ENGB;
                }

                return text;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public XEP_ECUCLIQUES()
        {
        }

        public XEP_ECUCLIQUES(IXepEcuCliques clique)
        {
            ExplicitTitle = clique.Title;
            ID = clique.ID;
            CLIQUENKURZBEZEICHNUNG = clique.CLIQUENKURZBEZEICHNUNG;
            TITLEID = clique.TITLEID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is XEP_ECUCLIQUES xEP_ECUCLIQUES))
            {
                return false;
            }

            if (ID != xEP_ECUCLIQUES.ID)
            {
                return false;
            }

            if (string.CompareOrdinal(CLIQUENKURZBEZEICHNUNG, xEP_ECUCLIQUES.CLIQUENKURZBEZEICHNUNG) != 0)
            {
                return false;
            }

            if (!(TITLEID == xEP_ECUCLIQUES.TITLEID))
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_DEDE, xEP_ECUCLIQUES.TITLE_DEDE) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_ENGB, xEP_ECUCLIQUES.TITLE_ENGB) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_ENUS, xEP_ECUCLIQUES.TITLE_ENUS) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_FR, xEP_ECUCLIQUES.TITLE_FR) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_TH, xEP_ECUCLIQUES.TITLE_TH) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_SV, xEP_ECUCLIQUES.TITLE_SV) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_IT, xEP_ECUCLIQUES.TITLE_IT) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_ES, xEP_ECUCLIQUES.TITLE_ES) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_ID, xEP_ECUCLIQUES.TITLE_ID) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_KO, xEP_ECUCLIQUES.TITLE_KO) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_EL, xEP_ECUCLIQUES.TITLE_EL) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_TR, xEP_ECUCLIQUES.TITLE_TR) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_ZHCN, xEP_ECUCLIQUES.TITLE_ZHCN) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_RU, xEP_ECUCLIQUES.TITLE_RU) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_NL, xEP_ECUCLIQUES.TITLE_NL) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_PT, xEP_ECUCLIQUES.TITLE_PT) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_ZHTW, xEP_ECUCLIQUES.TITLE_ZHTW) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_JA, xEP_ECUCLIQUES.TITLE_JA) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_CSCZ, xEP_ECUCLIQUES.TITLE_CSCZ) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(TITLE_PLPL, xEP_ECUCLIQUES.TITLE_PLPL) != 0)
            {
                return false;
            }

            if (!(ECUREPID == xEP_ECUCLIQUES.ECUREPID))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}