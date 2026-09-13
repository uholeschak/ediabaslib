using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_PERCEIVEDSYMPTOMSEX : INotifyPropertyChanged, IInfoBrowserObj, IEquatable<IInfoBrowserObj>
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

        private decimal? weightingField;

        private DateTime? validFromField;

        private DateTime? validToField;

        private decimal? sicherheitsRelevantField;

        private decimal? parentIdField;

        private decimal? pkwField;

        private decimal? motorradField;

        private decimal? selectableField;

        private string vfcNrField;

        private string vfcTypeField;

        private IList<XEP_PERCEIVEDSYMPTOMSEX> listChilds;

        private IList<XEP_DIAGNOSISOBJECTSEX> listDiagObjs;

        private XEP_PERCEIVEDSYMPTOMSEX parentNode;

        private ObservableCollectionEx<IXepInfoObject> setInfoObjs;

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

        public decimal? Weighting
        {
            get
            {
                return weightingField;
            }
            set
            {
                if (!weightingField.HasValue || !weightingField.Equals(value))
                {
                    weightingField = value;
                    OnPropertyChanged("Weighting");
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
                if (!validFromField.HasValue || !validFromField.Equals(value))
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
                if (!validToField.HasValue || !validToField.Equals(value))
                {
                    validToField = value;
                    OnPropertyChanged("ValidTo");
                }
            }
        }

        public decimal? SicherheitsRelevant
        {
            get
            {
                return sicherheitsRelevantField;
            }
            set
            {
                if (!sicherheitsRelevantField.HasValue || !sicherheitsRelevantField.Equals(value))
                {
                    sicherheitsRelevantField = value;
                    OnPropertyChanged("SicherheitsRelevant");
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

        public decimal? Pkw
        {
            get
            {
                return pkwField;
            }
            set
            {
                if (!pkwField.HasValue || !pkwField.Equals(value))
                {
                    pkwField = value;
                    OnPropertyChanged("Pkw");
                }
            }
        }

        public decimal? Motorrad
        {
            get
            {
                return motorradField;
            }
            set
            {
                if (!motorradField.HasValue || !motorradField.Equals(value))
                {
                    motorradField = value;
                    OnPropertyChanged("Motorrad");
                }
            }
        }

        public decimal? Selectable
        {
            get
            {
                return selectableField;
            }
            set
            {
                if (!selectableField.HasValue || !selectableField.Equals(value))
                {
                    selectableField = value;
                    OnPropertyChanged("Selectable");
                }
            }
        }

        public string VfcNr
        {
            get
            {
                return vfcNrField;
            }
            set
            {
                if (vfcNrField == null || !vfcNrField.Equals(value))
                {
                    vfcNrField = value;
                    OnPropertyChanged("VfcNr");
                }
            }
        }

        public string VfcType
        {
            get
            {
                return vfcTypeField;
            }
            set
            {
                if (vfcTypeField == null || !vfcTypeField.Equals(value))
                {
                    vfcTypeField = value;
                    OnPropertyChanged("VfcType");
                }
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public IList<IInfoBrowserObj> Children
        {
            get
            {
                if (listChilds != null)
                {
                    return listChilds.ToList().ConvertAll((Converter<XEP_PERCEIVEDSYMPTOMSEX, IInfoBrowserObj>)((XEP_PERCEIVEDSYMPTOMSEX x) => x));
                }
                return null;
            }
        }

        [XmlIgnore]
        public IList<XEP_PERCEIVEDSYMPTOMSEX> ListChildsAsXEP_PERCEIVEDSYMPTOMSEX
        {
            get
            {
                return listChilds;
            }
            set
            {
                if (listChilds == value)
                {
                    return;
                }
                listChilds = value;
                foreach (XEP_PERCEIVEDSYMPTOMSEX listChild in listChilds)
                {
                    listChild.ParentNodeAsXEP_PERCEIVEDSYMPTOMSEX = this;
                }
            }
        }

        [XmlIgnore]
        public IList<XEP_DIAGNOSISOBJECTSEX> ListDiagObjs
        {
            get
            {
                return listDiagObjs;
            }
            set
            {
                listDiagObjs = value;
            }
        }

        public string Name { get; set; }

        public IInfoBrowserObj ParentNode => parentNode;

        [XmlIgnore]
        public XEP_PERCEIVEDSYMPTOMSEX ParentNodeAsXEP_PERCEIVEDSYMPTOMSEX
        {
            get
            {
                return parentNode;
            }
            set
            {
                if (parentNode != value)
                {
                    parentNode = value;
                }
            }
        }

        [XmlIgnore]
        public ObservableCollectionEx<IXepInfoObject> SetInfoObjs
        {
            get
            {
                return setInfoObjs;
            }
            set
            {
                setInfoObjs = value;
            }
        }

        [XmlIgnore]
        public string Identifier
        {
            get
            {
                string text = ((!string.IsNullOrEmpty(VfcNr)) ? VfcNr : "n.a.");
                string text2 = ((!string.IsNullOrEmpty(VfcType)) ? VfcType : "n.a.");
                return "VFC_NR: " + text + " VFC_TYPE: " + text2;
            }
        }

        public string SortingTag
        {
            get
            {
                if (parentNode != null && parentNode.VfcType != null && (parentNode.VfcType.Equals("RootFM") || parentNode.VfcType.Equals("RootFun")))
                {
                    string vfcType = VfcType;
                    if (!(vfcType == "FA"))
                    {
                        if (vfcType == "FO_F")
                        {
                            return "A_" + Title;
                        }
                        return Title;
                    }
                    return "B_" + Title;
                }
                switch (VfcType)
                {
                    case "FL_F":
                        return "A_" + Title;
                    case "FA":
                        return "B_" + Title;
                    case "FO_K":
                    case "FO_F":
                        return "C_" + Title;
                    default:
                        return Title;
                }
            }
        }

        public bool IsHiddenObject => false;

        public bool IsRoot
        {
            get
            {
                decimal? parentId = ParentId;
                decimal num = 7866251;
                if (!((parentId.GetValueOrDefault() == num) & parentId.HasValue) && ParentId.HasValue)
                {
                    parentId = ParentId;
                    return (parentId.GetValueOrDefault() == default(decimal)) & parentId.HasValue;
                }
                return true;
            }
        }

        [XmlIgnore]
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
                        Log.Warning("XEP_PERCEIVEDSYMPTOMSEX.get_Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = Title_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = Title_engb;
                }
                return text;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_PERCEIVEDSYMPTOMSEX Clone()
        {
            return (XEP_PERCEIVEDSYMPTOMSEX)MemberwiseClone();
        }

        public bool Equals(IInfoBrowserObj other)
        {
            if (other == null)
            {
                return false;
            }
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_PERCEIVEDSYMPTOMSEX xEP_PERCEIVEDSYMPTOMSEX))
            {
                return false;
            }
            if (Id != xEP_PERCEIVEDSYMPTOMSEX.Id)
            {
                return false;
            }
            if (!(TitleId == xEP_PERCEIVEDSYMPTOMSEX.TitleId))
            {
                return false;
            }
            if (string.CompareOrdinal(Title_dede, xEP_PERCEIVEDSYMPTOMSEX.Title_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_engb, xEP_PERCEIVEDSYMPTOMSEX.Title_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_enus, xEP_PERCEIVEDSYMPTOMSEX.Title_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_fr, xEP_PERCEIVEDSYMPTOMSEX.Title_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_th, xEP_PERCEIVEDSYMPTOMSEX.Title_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_sv, xEP_PERCEIVEDSYMPTOMSEX.Title_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_it, xEP_PERCEIVEDSYMPTOMSEX.Title_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_es, xEP_PERCEIVEDSYMPTOMSEX.Title_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_id, xEP_PERCEIVEDSYMPTOMSEX.Title_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ko, xEP_PERCEIVEDSYMPTOMSEX.Title_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_el, xEP_PERCEIVEDSYMPTOMSEX.Title_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_tr, xEP_PERCEIVEDSYMPTOMSEX.Title_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhcn, xEP_PERCEIVEDSYMPTOMSEX.Title_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ru, xEP_PERCEIVEDSYMPTOMSEX.Title_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_nl, xEP_PERCEIVEDSYMPTOMSEX.Title_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_pt, xEP_PERCEIVEDSYMPTOMSEX.Title_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhtw, xEP_PERCEIVEDSYMPTOMSEX.Title_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ja, xEP_PERCEIVEDSYMPTOMSEX.Title_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_cscz, xEP_PERCEIVEDSYMPTOMSEX.Title_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_plpl, xEP_PERCEIVEDSYMPTOMSEX.Title_plpl) != 0)
            {
                return false;
            }
            if (!(Weighting == xEP_PERCEIVEDSYMPTOMSEX.Weighting))
            {
                return false;
            }
            if (ValidFrom != xEP_PERCEIVEDSYMPTOMSEX.ValidFrom)
            {
                return false;
            }
            if (ValidTo != xEP_PERCEIVEDSYMPTOMSEX.ValidTo)
            {
                return false;
            }
            if (!(SicherheitsRelevant == xEP_PERCEIVEDSYMPTOMSEX.SicherheitsRelevant))
            {
                return false;
            }
            if (!(ParentId == xEP_PERCEIVEDSYMPTOMSEX.ParentId))
            {
                return false;
            }
            if (!(Pkw == xEP_PERCEIVEDSYMPTOMSEX.Pkw))
            {
                return false;
            }
            if (!(Motorrad == xEP_PERCEIVEDSYMPTOMSEX.Motorrad))
            {
                return false;
            }
            if (!(Selectable == xEP_PERCEIVEDSYMPTOMSEX.Selectable))
            {
                return false;
            }
            if (string.CompareOrdinal(VfcNr, xEP_PERCEIVEDSYMPTOMSEX.VfcNr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(VfcType, xEP_PERCEIVEDSYMPTOMSEX.VfcType) != 0)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public string GetLocalizedTitleValue(string language)
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
                    Log.Warning("XEP_PERCEIVEDSYMPTOMSEX.GetLocalizedTitleValue", "the given language {0} is not available - language set to enGB", language);
                    text = Title_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = Title_engb;
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
