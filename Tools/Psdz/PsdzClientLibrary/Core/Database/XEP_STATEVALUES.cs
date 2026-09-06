using PsdzClient.Core;
using System;
using System.ComponentModel;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_STATEVALUES : INotifyPropertyChanged
    {
        private decimal idField;

        private decimal? titleIdField;

        private string title_dedeField;

        private string title_engbField;

        private string title_enusField;

        private string title_frField;

        private string title_thField;

        private string title_svField;

        private string title_itField;

        private string title_esField;

        private string title_idField;

        private string title_koField;

        private string title_elField;

        private string title_trField;

        private string title_zhcnField;

        private string title_ruField;

        private string title_nlField;

        private string title_ptField;

        private string title_zhtwField;

        private string title_jaField;

        private string title_csczField;

        private string title_plplField;

        private string statevalueField;

        private DateTime? validFromField;

        private DateTime? validToField;

        private decimal? sicherheitsrelevantField;

        private decimal? parentIdField;

        public decimal Id
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
                    OnPropertyChanged("Id");
                }
            }
        }

        public decimal? TitleId
        {
            get
            {
                return titleIdField;
            }
            set
            {
                if (titleIdField.HasValue)
                {
                    if (!titleIdField.Equals(value))
                    {
                        titleIdField = value;
                        OnPropertyChanged("TitleId");
                    }
                }
                else
                {
                    titleIdField = value;
                    OnPropertyChanged("TitleId");
                }
            }
        }

        public string Title_dede
        {
            get
            {
                return title_dedeField;
            }
            set
            {
                if (title_dedeField != null)
                {
                    if (!title_dedeField.Equals(value))
                    {
                        title_dedeField = value;
                        OnPropertyChanged("Title_dede");
                    }
                }
                else
                {
                    title_dedeField = value;
                    OnPropertyChanged("Title_dede");
                }
            }
        }

        public string Title_engb
        {
            get
            {
                return title_engbField;
            }
            set
            {
                if (title_engbField != null)
                {
                    if (!title_engbField.Equals(value))
                    {
                        title_engbField = value;
                        OnPropertyChanged("Title_engb");
                    }
                }
                else
                {
                    title_engbField = value;
                    OnPropertyChanged("Title_engb");
                }
            }
        }

        public string Title_enus
        {
            get
            {
                return title_enusField;
            }
            set
            {
                if (title_enusField != null)
                {
                    if (!title_enusField.Equals(value))
                    {
                        title_enusField = value;
                        OnPropertyChanged("Title_enus");
                    }
                }
                else
                {
                    title_enusField = value;
                    OnPropertyChanged("Title_enus");
                }
            }
        }

        public string Title_fr
        {
            get
            {
                return title_frField;
            }
            set
            {
                if (title_frField != null)
                {
                    if (!title_frField.Equals(value))
                    {
                        title_frField = value;
                        OnPropertyChanged("Title_fr");
                    }
                }
                else
                {
                    title_frField = value;
                    OnPropertyChanged("Title_fr");
                }
            }
        }

        public string Title_th
        {
            get
            {
                return title_thField;
            }
            set
            {
                if (title_thField != null)
                {
                    if (!title_thField.Equals(value))
                    {
                        title_thField = value;
                        OnPropertyChanged("Title_th");
                    }
                }
                else
                {
                    title_thField = value;
                    OnPropertyChanged("Title_th");
                }
            }
        }

        public string Title_sv
        {
            get
            {
                return title_svField;
            }
            set
            {
                if (title_svField != null)
                {
                    if (!title_svField.Equals(value))
                    {
                        title_svField = value;
                        OnPropertyChanged("Title_sv");
                    }
                }
                else
                {
                    title_svField = value;
                    OnPropertyChanged("Title_sv");
                }
            }
        }

        public string Title_it
        {
            get
            {
                return title_itField;
            }
            set
            {
                if (title_itField != null)
                {
                    if (!title_itField.Equals(value))
                    {
                        title_itField = value;
                        OnPropertyChanged("Title_it");
                    }
                }
                else
                {
                    title_itField = value;
                    OnPropertyChanged("Title_it");
                }
            }
        }

        public string Title_es
        {
            get
            {
                return title_esField;
            }
            set
            {
                if (title_esField != null)
                {
                    if (!title_esField.Equals(value))
                    {
                        title_esField = value;
                        OnPropertyChanged("Title_es");
                    }
                }
                else
                {
                    title_esField = value;
                    OnPropertyChanged("Title_es");
                }
            }
        }

        public string Title_id
        {
            get
            {
                return title_idField;
            }
            set
            {
                if (title_idField != null)
                {
                    if (!title_idField.Equals(value))
                    {
                        title_idField = value;
                        OnPropertyChanged("Title_id");
                    }
                }
                else
                {
                    title_idField = value;
                    OnPropertyChanged("Title_id");
                }
            }
        }

        public string Title_ko
        {
            get
            {
                return title_koField;
            }
            set
            {
                if (title_koField != null)
                {
                    if (!title_koField.Equals(value))
                    {
                        title_koField = value;
                        OnPropertyChanged("Title_ko");
                    }
                }
                else
                {
                    title_koField = value;
                    OnPropertyChanged("Title_ko");
                }
            }
        }

        public string Title_el
        {
            get
            {
                return title_elField;
            }
            set
            {
                if (title_elField != null)
                {
                    if (!title_elField.Equals(value))
                    {
                        title_elField = value;
                        OnPropertyChanged("Title_el");
                    }
                }
                else
                {
                    title_elField = value;
                    OnPropertyChanged("Title_el");
                }
            }
        }

        public string Title_tr
        {
            get
            {
                return title_trField;
            }
            set
            {
                if (title_trField != null)
                {
                    if (!title_trField.Equals(value))
                    {
                        title_trField = value;
                        OnPropertyChanged("Title_tr");
                    }
                }
                else
                {
                    title_trField = value;
                    OnPropertyChanged("Title_tr");
                }
            }
        }

        public string Title_zhcn
        {
            get
            {
                return title_zhcnField;
            }
            set
            {
                if (title_zhcnField != null)
                {
                    if (!title_zhcnField.Equals(value))
                    {
                        title_zhcnField = value;
                        OnPropertyChanged("Title_zhcn");
                    }
                }
                else
                {
                    title_zhcnField = value;
                    OnPropertyChanged("Title_zhcn");
                }
            }
        }

        public string Title_ru
        {
            get
            {
                return title_ruField;
            }
            set
            {
                if (title_ruField != null)
                {
                    if (!title_ruField.Equals(value))
                    {
                        title_ruField = value;
                        OnPropertyChanged("Title_ru");
                    }
                }
                else
                {
                    title_ruField = value;
                    OnPropertyChanged("Title_ru");
                }
            }
        }

        public string Title_nl
        {
            get
            {
                return title_nlField;
            }
            set
            {
                if (title_nlField != null)
                {
                    if (!title_nlField.Equals(value))
                    {
                        title_nlField = value;
                        OnPropertyChanged("Title_nl");
                    }
                }
                else
                {
                    title_nlField = value;
                    OnPropertyChanged("Title_nl");
                }
            }
        }

        public string Title_pt
        {
            get
            {
                return title_ptField;
            }
            set
            {
                if (title_ptField != null)
                {
                    if (!title_ptField.Equals(value))
                    {
                        title_ptField = value;
                        OnPropertyChanged("Title_pt");
                    }
                }
                else
                {
                    title_ptField = value;
                    OnPropertyChanged("Title_pt");
                }
            }
        }

        public string Title_zhtw
        {
            get
            {
                return title_zhtwField;
            }
            set
            {
                if (title_zhtwField != null)
                {
                    if (!title_zhtwField.Equals(value))
                    {
                        title_zhtwField = value;
                        OnPropertyChanged("Title_zhtw");
                    }
                }
                else
                {
                    title_zhtwField = value;
                    OnPropertyChanged("Title_zhtw");
                }
            }
        }

        public string Title_ja
        {
            get
            {
                return title_jaField;
            }
            set
            {
                if (title_jaField != null)
                {
                    if (!title_jaField.Equals(value))
                    {
                        title_jaField = value;
                        OnPropertyChanged("Title_ja");
                    }
                }
                else
                {
                    title_jaField = value;
                    OnPropertyChanged("Title_ja");
                }
            }
        }

        public string Title_cscz
        {
            get
            {
                return title_csczField;
            }
            set
            {
                if (title_csczField != null)
                {
                    if (!title_csczField.Equals(value))
                    {
                        title_csczField = value;
                        OnPropertyChanged("Title_cscz");
                    }
                }
                else
                {
                    title_csczField = value;
                    OnPropertyChanged("Title_cscz");
                }
            }
        }

        public string Title_plpl
        {
            get
            {
                return title_plplField;
            }
            set
            {
                if (title_plplField != null)
                {
                    if (!title_plplField.Equals(value))
                    {
                        title_plplField = value;
                        OnPropertyChanged("Title_plpl");
                    }
                }
                else
                {
                    title_plplField = value;
                    OnPropertyChanged("Title_plpl");
                }
            }
        }

        public string Statevalue
        {
            get
            {
                return statevalueField;
            }
            set
            {
                if (statevalueField != null)
                {
                    if (!statevalueField.Equals(value))
                    {
                        statevalueField = value;
                        OnPropertyChanged("Statevalue");
                    }
                }
                else
                {
                    statevalueField = value;
                    OnPropertyChanged("Statevalue");
                }
            }
        }

        public DateTime? ValidFrom
        {
            get
            {
                return validFromField;
            }
            set
            {
                if (validFromField.HasValue)
                {
                    if (!validFromField.Equals(value))
                    {
                        validFromField = value;
                        OnPropertyChanged("ValidFrom");
                    }
                }
                else
                {
                    validFromField = value;
                    OnPropertyChanged("ValidFrom");
                }
            }
        }

        public DateTime? ValidTo
        {
            get
            {
                return validToField;
            }
            set
            {
                if (validToField.HasValue)
                {
                    if (!validToField.Equals(value))
                    {
                        validToField = value;
                        OnPropertyChanged("ValidTo");
                    }
                }
                else
                {
                    validToField = value;
                    OnPropertyChanged("ValidTo");
                }
            }
        }

        public decimal? Sicherheitsrelevant
        {
            get
            {
                return sicherheitsrelevantField;
            }
            set
            {
                if (sicherheitsrelevantField.HasValue)
                {
                    if (!sicherheitsrelevantField.Equals(value))
                    {
                        sicherheitsrelevantField = value;
                        OnPropertyChanged("Sicherheitsrelevant");
                    }
                }
                else
                {
                    sicherheitsrelevantField = value;
                    OnPropertyChanged("Sicherheitsrelevant");
                }
            }
        }

        public decimal? ParentId
        {
            get
            {
                return parentIdField;
            }
            set
            {
                if (parentIdField.HasValue)
                {
                    if (!parentIdField.Equals(value))
                    {
                        parentIdField = value;
                        OnPropertyChanged("ParentId");
                    }
                }
                else
                {
                    parentIdField = value;
                    OnPropertyChanged("ParentId");
                }
            }
        }

        public string Title => GetLocalizedTitle(ConfigSettings.CurrentUICulture);

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_STATEVALUES xEP_STATEVALUES))
            {
                return false;
            }
            if (Id != xEP_STATEVALUES.Id)
            {
                return false;
            }
            if (!(TitleId == xEP_STATEVALUES.TitleId))
            {
                return false;
            }
            if (string.CompareOrdinal(Title_dede, xEP_STATEVALUES.Title_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_engb, xEP_STATEVALUES.Title_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_enus, xEP_STATEVALUES.Title_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_fr, xEP_STATEVALUES.Title_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_th, xEP_STATEVALUES.Title_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_sv, xEP_STATEVALUES.Title_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_it, xEP_STATEVALUES.Title_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_es, xEP_STATEVALUES.Title_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_id, xEP_STATEVALUES.Title_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ko, xEP_STATEVALUES.Title_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_el, xEP_STATEVALUES.Title_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_tr, xEP_STATEVALUES.Title_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhcn, xEP_STATEVALUES.Title_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ru, xEP_STATEVALUES.Title_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_nl, xEP_STATEVALUES.Title_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_pt, xEP_STATEVALUES.Title_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhtw, xEP_STATEVALUES.Title_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ja, xEP_STATEVALUES.Title_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_cscz, xEP_STATEVALUES.Title_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_plpl, xEP_STATEVALUES.Title_plpl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Statevalue, xEP_STATEVALUES.Statevalue) != 0)
            {
                return false;
            }
            if (ValidFrom != xEP_STATEVALUES.ValidFrom)
            {
                return false;
            }
            if (ValidTo != xEP_STATEVALUES.ValidTo)
            {
                return false;
            }
            if (!(Sicherheitsrelevant == xEP_STATEVALUES.Sicherheitsrelevant))
            {
                return false;
            }
            if (!(ParentId == xEP_STATEVALUES.ParentId))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public string GetLocalizedTitle(string language)
        {
            string text;
            switch (MatchLanguageToCulture(language))
            {
                case "de-DE":
                    text = Title_dede;
                    break;
                case "en-GB":
                    text = Title_engb;
                    break;
                case "en-US":
                    text = Title_enus;
                    break;
                case "fr-FR":
                    text = Title_fr;
                    break;
                case "es-ES":
                    text = Title_es;
                    break;
                case "th-TH":
                    text = Title_th;
                    break;
                case "tr-TR":
                    text = Title_tr;
                    break;
                case "el-GR":
                    text = Title_el;
                    break;
                case "ja-JP":
                    text = Title_ja;
                    break;
                case "ru-RU":
                    text = Title_ru;
                    break;
                case "it-IT":
                    text = Title_it;
                    break;
                case "nl-NL":
                    text = Title_nl;
                    break;
                case "pl-PL":
                    text = Title_plpl;
                    break;
                case "cs-CZ":
                    text = Title_cscz;
                    break;
                case "pt-PT":
                    text = Title_pt;
                    break;
                case "sv-SE":
                    text = Title_sv;
                    break;
                case "zh-CN":
                    text = Title_zhcn;
                    break;
                case "zh-TW":
                    text = Title_zhtw;
                    break;
                case "ko-KR":
                    text = Title_ko;
                    break;
                default:
                    Log.Warning("XEP_STATEVALUES.GetLocalizedSwiActivationCodeTitle", "the given language {0} is not available - language set to enGB", language);
                    text = Title_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                return Title_engb;
            }
            return text;
        }

        private string MatchLanguageToCulture(string language)
        {
            string text = language;
            if (text.Length == 2)
            {
                text = ((text == "pt") ? "pt-PT" : new CultureInfo(language).Name);
            }
            return text;
        }
    }
}
