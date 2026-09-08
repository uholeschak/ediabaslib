using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_SWIREGISTER : INotifyPropertyChanged
    {
        private decimal idField;

        private decimal? nodeclassField;

        private decimal? parentIdField;

        private string nameField;

        private string remarkField;

        private decimal? sortField;

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

        private decimal? versionNumberField;

        private string identifierField;

        public decimal Id
        {
            get
            {
                return idField;
            }
            set
            {
                if (!idField.Equals(value))
                {
                    idField = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public decimal? Nodeclass
        {
            get
            {
                return nodeclassField;
            }
            set
            {
                if (!nodeclassField.HasValue || !nodeclassField.Equals(value))
                {
                    nodeclassField = value;
                    OnPropertyChanged("Nodeclass");
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
                if (!parentIdField.HasValue || !parentIdField.Equals(value))
                {
                    parentIdField = value;
                    OnPropertyChanged("ParentId");
                }
            }
        }

        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                if (nameField == null || !nameField.Equals(value))
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                if (remarkField == null || !remarkField.Equals(value))
                {
                    remarkField = value;
                    OnPropertyChanged("Remark");
                }
            }
        }

        public decimal? Sort
        {
            get
            {
                return sortField;
            }
            set
            {
                if (!sortField.HasValue || !sortField.Equals(value))
                {
                    sortField = value;
                    OnPropertyChanged("Sort");
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
                if (!titleIdField.HasValue || !titleIdField.Equals(value))
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
                if (title_dedeField == null || !title_dedeField.Equals(value))
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
                if (title_engbField == null || !title_engbField.Equals(value))
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
                if (title_enusField == null || !title_enusField.Equals(value))
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
                if (title_frField == null || !title_frField.Equals(value))
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
                if (title_thField == null || !title_thField.Equals(value))
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
                if (title_svField == null || !title_svField.Equals(value))
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
                if (title_itField == null || !title_itField.Equals(value))
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
                if (title_esField == null || !title_esField.Equals(value))
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
                if (title_idField == null || !title_idField.Equals(value))
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
                if (title_koField == null || !title_koField.Equals(value))
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
                if (title_elField == null || !title_elField.Equals(value))
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
                if (title_trField == null || !title_trField.Equals(value))
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
                if (title_zhcnField == null || !title_zhcnField.Equals(value))
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
                if (title_ruField == null || !title_ruField.Equals(value))
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
                if (title_nlField == null || !title_nlField.Equals(value))
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
                if (title_ptField == null || !title_ptField.Equals(value))
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
                if (title_zhtwField == null || !title_zhtwField.Equals(value))
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
                if (title_jaField == null || !title_jaField.Equals(value))
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
                if (title_csczField == null || !title_csczField.Equals(value))
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
                if (title_plplField == null || !title_plplField.Equals(value))
                {
                    title_plplField = value;
                    OnPropertyChanged("Title_plpl");
                }
            }
        }

        public decimal? VersionNumber
        {
            get
            {
                return versionNumberField;
            }
            set
            {
                if (!versionNumberField.HasValue || !versionNumberField.Equals(value))
                {
                    versionNumberField = value;
                    OnPropertyChanged("VersionNumber");
                }
            }
        }

        public string Identifier
        {
            get
            {
                return identifierField;
            }
            set
            {
                if (identifierField == null || !identifierField.Equals(value))
                {
                    identifierField = value;
                    OnPropertyChanged("Identifier");
                }
            }
        }

        public string Title
        {
            get
            {
                string text;
                switch (ConfigSettings.CurrentUICulture)
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
                        Log.Warning("XEP_SWIREGISTER.Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = Title_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    return Title_engb;
                }
                return text;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_SWIREGISTER Clone()
        {
            return (XEP_SWIREGISTER)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_SWIREGISTER xEP_SWIREGISTER))
            {
                return false;
            }
            if (Id != xEP_SWIREGISTER.Id)
            {
                return false;
            }
            if (!(Nodeclass == xEP_SWIREGISTER.Nodeclass))
            {
                return false;
            }
            if (!(ParentId == xEP_SWIREGISTER.ParentId))
            {
                return false;
            }
            if (string.CompareOrdinal(Name, xEP_SWIREGISTER.Name) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Remark, xEP_SWIREGISTER.Remark) != 0)
            {
                return false;
            }
            if (!(Sort == xEP_SWIREGISTER.Sort))
            {
                return false;
            }
            if (!(TitleId == xEP_SWIREGISTER.TitleId))
            {
                return false;
            }
            if (string.CompareOrdinal(Title_dede, xEP_SWIREGISTER.Title_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_engb, xEP_SWIREGISTER.Title_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_enus, xEP_SWIREGISTER.Title_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_fr, xEP_SWIREGISTER.Title_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_th, xEP_SWIREGISTER.Title_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_sv, xEP_SWIREGISTER.Title_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_it, xEP_SWIREGISTER.Title_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_es, xEP_SWIREGISTER.Title_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_id, xEP_SWIREGISTER.Title_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ko, xEP_SWIREGISTER.Title_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_el, xEP_SWIREGISTER.Title_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_tr, xEP_SWIREGISTER.Title_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhcn, xEP_SWIREGISTER.Title_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ru, xEP_SWIREGISTER.Title_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_nl, xEP_SWIREGISTER.Title_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_pt, xEP_SWIREGISTER.Title_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhtw, xEP_SWIREGISTER.Title_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ja, xEP_SWIREGISTER.Title_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_cscz, xEP_SWIREGISTER.Title_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_plpl, xEP_SWIREGISTER.Title_plpl) != 0)
            {
                return false;
            }
            if (!(VersionNumber == xEP_SWIREGISTER.VersionNumber))
            {
                return false;
            }
            if (string.CompareOrdinal(Identifier, xEP_SWIREGISTER.Identifier) != 0)
            {
                return false;
            }
            return true;
        }

        public void SetTitle(IList<LocalizedText> titleLocalized)
        {
            foreach (LocalizedText item in titleLocalized)
            {
                switch (item.Language)
                {
                    case "de-DE":
                        Title_dede = item.TextItem;
                        break;
                    case "en-GB":
                        Title_engb = item.TextItem;
                        break;
                    case "en-US":
                        Title_enus = item.TextItem;
                        break;
                    case "fr-FR":
                        Title_fr = item.TextItem;
                        break;
                    case "es-ES":
                        Title_es = item.TextItem;
                        break;
                    case "th-TH":
                        Title_th = item.TextItem;
                        break;
                    case "tr-TR":
                        Title_tr = item.TextItem;
                        break;
                    case "el-GR":
                        Title_el = item.TextItem;
                        break;
                    case "ja-JP":
                        Title_ja = item.TextItem;
                        break;
                    case "ru-RU":
                        Title_ru = item.TextItem;
                        break;
                    case "it-IT":
                        Title_it = item.TextItem;
                        break;
                    case "nl-NL":
                        Title_nl = item.TextItem;
                        break;
                    case "pl-PL":
                        Title_plpl = item.TextItem;
                        break;
                    case "cs-CZ":
                        Title_cscz = item.TextItem;
                        break;
                    case "pt-PT":
                        Title_pt = item.TextItem;
                        break;
                    case "sv-SE":
                        Title_sv = item.TextItem;
                        break;
                    case "zh-CN":
                        Title_zhcn = item.TextItem;
                        break;
                    case "zh-TW":
                        Title_zhtw = item.TextItem;
                        break;
                    case "ko-KR":
                        Title_ko = item.TextItem;
                        break;
                    default:
                        Log.Warning("XEP_SWIREGISTER.SetTitle()", "Language \"{0}\" not supported, value \"{1}\" will not be set.", item.TextItem);
                        break;
                }
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
