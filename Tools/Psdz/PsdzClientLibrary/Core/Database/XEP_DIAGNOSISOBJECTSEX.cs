using PsdzClient.Core;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_DIAGNOSISOBJECTSEX : INotifyPropertyChanged, IInfoBrowserObj, IEquatable<IInfoBrowserObj>, ICloneable
    {
        private decimal idField;

        private decimal? nodeclassField;

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

        private decimal? failureWeightField;

        private decimal? verstecktField;

        private decimal? sicherheitsRelevantField;

        private decimal? controlIdField;

        private string nameField;

        private string grobzeichenField;

        private string hg_NummerField;

        private string hgug_NummerField;

        private DateTime? validFromField;

        private DateTime? validToField;

        private decimal? sortOrderField;

        private const decimal ParentIdUnknown = 0m;

        private bool isSubGroupNode;

        private decimal parentId;

        private IInfoBrowserObj parentNode;

        private ObservableCollectionEx<IXepInfoObject> setInfoObjs;

        private decimal? sureSuspicion = default(decimal);

        private XEP_DIAGNOSISOBJECTS_TITLE titleMode;

        private decimal? priority = default(decimal);

        private decimal? prioWeightStrongCount = default(decimal);

        private decimal? prioWeightMiddleCount = default(decimal);

        private decimal? prioWeightLowCount = default(decimal);

        private decimal? prioSureSuspicionCount = default(decimal);

        private bool isFaultPresent;

        private DiagObjPriority mainPriority = new DiagObjPriority();

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

        public decimal? FailureWeight
        {
            get
            {
                return failureWeightField;
            }
            set
            {
                if (!failureWeightField.HasValue || !failureWeightField.Equals(value))
                {
                    failureWeightField = value;
                    OnPropertyChanged("FailureWeight");
                }
            }
        }

        public decimal? Versteckt
        {
            get
            {
                return verstecktField;
            }
            set
            {
                if (!verstecktField.HasValue || !verstecktField.Equals(value))
                {
                    verstecktField = value;
                    OnPropertyChanged("Versteckt");
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

        public decimal? ControlId
        {
            get
            {
                return controlIdField;
            }
            set
            {
                if (!controlIdField.HasValue || !controlIdField.Equals(value))
                {
                    controlIdField = value;
                    OnPropertyChanged("ControlId");
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

        public string Grobzeichen
        {
            get
            {
                return grobzeichenField;
            }
            set
            {
                if (grobzeichenField == null || !grobzeichenField.Equals(value))
                {
                    grobzeichenField = value;
                    OnPropertyChanged("Grobzeichen");
                }
            }
        }

        public string Hg_Nummer
        {
            get
            {
                return hg_NummerField;
            }
            set
            {
                if (hg_NummerField == null || !hg_NummerField.Equals(value))
                {
                    hg_NummerField = value;
                    OnPropertyChanged("Hg_Nummer");
                }
            }
        }

        public string Hgug_Nummer
        {
            get
            {
                return hgug_NummerField;
            }
            set
            {
                if (hgug_NummerField == null || !hgug_NummerField.Equals(value))
                {
                    hgug_NummerField = value;
                    OnPropertyChanged("Hgug_Nummer");
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

        public decimal? SortOrder
        {
            get
            {
                return sortOrderField;
            }
            set
            {
                if (!sortOrderField.HasValue || !sortOrderField.Equals(value))
                {
                    sortOrderField = value;
                    OnPropertyChanged("SortOrder");
                }
            }
        }

        public bool IsSubGroupNode
        {
            get
            {
                return isSubGroupNode;
            }
            set
            {
                isSubGroupNode = value;
                OnPropertyChanged("IsSubGroupNode");
            }
        }

        public decimal ParentId
        {
            get
            {
                return parentId;
            }
            set
            {
                parentId = value;
                OnPropertyChanged("ParentId");
            }
        }

        public IInfoBrowserObj ParentNode
        {
            get
            {
                return parentNode;
            }
            set
            {
                parentNode = value;
                OnPropertyChanged("ParentNode");
            }
        }

        public decimal? Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                OnPropertyChanged("Priority");
            }
        }

        [XmlIgnore]
        public decimal? PrioWeightStrongCount
        {
            get
            {
                return prioWeightStrongCount;
            }
            set
            {
                prioWeightStrongCount = value;
                OnPropertyChanged("PrioWeightStrongCount");
            }
        }

        [XmlIgnore]
        public decimal? PrioWeightMiddleCount
        {
            get
            {
                return prioWeightMiddleCount;
            }
            set
            {
                prioWeightMiddleCount = value;
                OnPropertyChanged("PrioWeightMiddleCount");
            }
        }

        [XmlIgnore]
        public decimal? PrioWeightLowCount
        {
            get
            {
                return prioWeightLowCount;
            }
            set
            {
                prioWeightLowCount = value;
                OnPropertyChanged("PrioWeightLowCount");
            }
        }

        [XmlIgnore]
        public decimal? PrioSureSuspicionCount
        {
            get
            {
                return prioSureSuspicionCount;
            }
            set
            {
                prioSureSuspicionCount = value;
                OnPropertyChanged("PrioSureSuspicionCount");
            }
        }

        [XmlIgnore]
        public bool IsFaultPresent
        {
            get
            {
                return isFaultPresent;
            }
            set
            {
                isFaultPresent = value;
                OnPropertyChanged("IsFaultPresent");
            }
        }

        [XmlIgnore]
        public DiagObjPriority MainPriority
        {
            get
            {
                return mainPriority;
            }
            set
            {
                mainPriority = value;
                OnPropertyChanged("MainPriority");
            }
        }

        public ObservableCollectionEx<IXepInfoObject> SetInfoObjs
        {
            get
            {
                return setInfoObjs;
            }
            set
            {
                setInfoObjs = value;
                OnPropertyChanged("SetInfoObjs");
            }
        }

        public string Identifier
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return "Sysname: n.a.";
                }
                return "Sysname: " + Name;
            }
        }

        public string SortingTag
        {
            get
            {
                string text = Title.Replace("-", " ");
                if (!string.IsNullOrEmpty(Hg_Nummer))
                {
                    return PadNumberInString(Hg_Nummer);
                }
                if (!string.IsNullOrEmpty(Hgug_Nummer))
                {
                    return PadNumberInString(Hgug_Nummer);
                }
                if (!string.IsNullOrEmpty(Grobzeichen))
                {
                    return PadNumberInString(SortOrder + " " + Grobzeichen);
                }
                if (string.IsNullOrEmpty(SortOrder.ToString()))
                {
                    return PadNumberInString(text);
                }
                return PadNumberInString(SortOrder + " " + text);
            }
        }

        public bool IsHiddenObject
        {
            get
            {
                decimal? versteckt = Versteckt;
                decimal num = 1;
                if ((versteckt.GetValueOrDefault() == num) & versteckt.HasValue)
                {
                    return true;
                }
                return false;
            }
        }

        public decimal? SureSuspicion
        {
            get
            {
                return sureSuspicion;
            }
            set
            {
                sureSuspicion = value;
                OnPropertyChanged("SureSuspicion");
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
                        Log.Warning("XEP_DIAGNOSISOBJECTSEX.get_Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = Title_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = Title_engb;
                }
                if (TitleMode == XEP_DIAGNOSISOBJECTS_TITLE.HG_TITLE)
                {
                    text = Hg_Nummer + " " + text;
                }
                else if (TitleMode == XEP_DIAGNOSISOBJECTS_TITLE.HGUG_NUMMER)
                {
                    text = Hgug_Nummer + " " + text;
                }
                else if (TitleMode == XEP_DIAGNOSISOBJECTS_TITLE.GROBZEICHEN)
                {
                    text = Grobzeichen + " " + text;
                }
                if (IsHiddenObject && ConfigSettings.IsVerificationMode && ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.ISTAGUI.ShowHiddenDiagnosticObjects", defaultValue: false))
                {
                    text = "* " + text;
                }
                return text;
            }
        }

        public XEP_DIAGNOSISOBJECTS_TITLE TitleMode
        {
            get
            {
                return titleMode;
            }
            set
            {
                titleMode = value;
                OnPropertyChanged("TitleMode");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public XEP_DIAGNOSISOBJECTSEX(XEP_DIAGNOSISOBJECTSEX anotherDiagObj)
            : this()
        {
            ControlId = anotherDiagObj.ControlId;
            FailureWeight = anotherDiagObj.FailureWeight;
            Grobzeichen = anotherDiagObj.Grobzeichen;
            Hg_Nummer = anotherDiagObj.Hg_Nummer;
            Hgug_Nummer = anotherDiagObj.Hgug_Nummer;
            Id = anotherDiagObj.Id;
            IsSubGroupNode = anotherDiagObj.IsSubGroupNode;
            Name = anotherDiagObj.Name;
            Nodeclass = anotherDiagObj.Nodeclass;
            Priority = anotherDiagObj.Priority;
            SicherheitsRelevant = anotherDiagObj.SicherheitsRelevant;
            SureSuspicion = anotherDiagObj.SureSuspicion;
            Title_dede = anotherDiagObj.Title_dede;
            Title_el = anotherDiagObj.Title_el;
            Title_engb = anotherDiagObj.Title_engb;
            Title_enus = anotherDiagObj.Title_enus;
            Title_es = anotherDiagObj.Title_es;
            Title_fr = anotherDiagObj.Title_fr;
            Title_id = anotherDiagObj.Title_id;
            Title_it = anotherDiagObj.Title_it;
            Title_ja = anotherDiagObj.Title_ja;
            Title_ko = anotherDiagObj.Title_ko;
            Title_nl = anotherDiagObj.Title_nl;
            Title_pt = anotherDiagObj.Title_pt;
            Title_ru = anotherDiagObj.Title_ru;
            Title_sv = anotherDiagObj.Title_sv;
            Title_th = anotherDiagObj.Title_th;
            Title_tr = anotherDiagObj.Title_tr;
            Title_zhcn = anotherDiagObj.Title_zhcn;
            Title_zhtw = anotherDiagObj.Title_zhtw;
            Title_cscz = anotherDiagObj.Title_cscz;
            Title_plpl = anotherDiagObj.Title_plpl;
            TitleId = anotherDiagObj.TitleId;
            ValidFrom = anotherDiagObj.ValidFrom;
            ValidTo = anotherDiagObj.ValidTo;
            VersionNumber = anotherDiagObj.VersionNumber;
            Versteckt = anotherDiagObj.Versteckt;
            TitleMode = anotherDiagObj.TitleMode;
        }

        public XEP_DIAGNOSISOBJECTSEX()
        {
        }

        public bool Equals(IInfoBrowserObj other)
        {
            if (other != null)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_DIAGNOSISOBJECTSEX xEP_DIAGNOSISOBJECTSEX))
            {
                return false;
            }
            if (Id != xEP_DIAGNOSISOBJECTSEX.Id)
            {
                return false;
            }
            if (!(Nodeclass == xEP_DIAGNOSISOBJECTSEX.Nodeclass))
            {
                return false;
            }
            if (!(TitleId == xEP_DIAGNOSISOBJECTSEX.TitleId))
            {
                return false;
            }
            if (string.CompareOrdinal(Title_dede, xEP_DIAGNOSISOBJECTSEX.Title_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_engb, xEP_DIAGNOSISOBJECTSEX.Title_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_enus, xEP_DIAGNOSISOBJECTSEX.Title_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_fr, xEP_DIAGNOSISOBJECTSEX.Title_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_th, xEP_DIAGNOSISOBJECTSEX.Title_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_sv, xEP_DIAGNOSISOBJECTSEX.Title_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_it, xEP_DIAGNOSISOBJECTSEX.Title_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_es, xEP_DIAGNOSISOBJECTSEX.Title_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_id, xEP_DIAGNOSISOBJECTSEX.Title_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ko, xEP_DIAGNOSISOBJECTSEX.Title_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_el, xEP_DIAGNOSISOBJECTSEX.Title_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_tr, xEP_DIAGNOSISOBJECTSEX.Title_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhcn, xEP_DIAGNOSISOBJECTSEX.Title_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ru, xEP_DIAGNOSISOBJECTSEX.Title_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_nl, xEP_DIAGNOSISOBJECTSEX.Title_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_pt, xEP_DIAGNOSISOBJECTSEX.Title_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhtw, xEP_DIAGNOSISOBJECTSEX.Title_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ja, xEP_DIAGNOSISOBJECTSEX.Title_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_cscz, xEP_DIAGNOSISOBJECTSEX.Title_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_plpl, xEP_DIAGNOSISOBJECTSEX.Title_plpl) != 0)
            {
                return false;
            }
            if (!(VersionNumber == xEP_DIAGNOSISOBJECTSEX.VersionNumber))
            {
                return false;
            }
            if (string.CompareOrdinal(Name, xEP_DIAGNOSISOBJECTSEX.Name) != 0)
            {
                return false;
            }
            if (!(FailureWeight == xEP_DIAGNOSISOBJECTSEX.FailureWeight))
            {
                return false;
            }
            if (!(Versteckt == xEP_DIAGNOSISOBJECTSEX.Versteckt))
            {
                return false;
            }
            if (ValidFrom != xEP_DIAGNOSISOBJECTSEX.ValidFrom)
            {
                return false;
            }
            if (ValidTo != xEP_DIAGNOSISOBJECTSEX.ValidTo)
            {
                return false;
            }
            if (!(SicherheitsRelevant == xEP_DIAGNOSISOBJECTSEX.SicherheitsRelevant))
            {
                return false;
            }
            if (string.CompareOrdinal(Grobzeichen, xEP_DIAGNOSISOBJECTSEX.Grobzeichen) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hg_Nummer, xEP_DIAGNOSISOBJECTSEX.Hg_Nummer) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hgug_Nummer, xEP_DIAGNOSISOBJECTSEX.Hgug_Nummer) != 0)
            {
                return false;
            }
            if (!(ControlId == xEP_DIAGNOSISOBJECTSEX.ControlId))
            {
                return false;
            }
            if (!(SortOrder == xEP_DIAGNOSISOBJECTSEX.SortOrder))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public string GetLocalizedDiagnosisObjectTitle(string language)
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
                case "zh-TW":
                    text = Title_zhtw;
                    break;
                case "zh-CN":
                    text = Title_zhcn;
                    break;
                case "ko-KR":
                    text = Title_ko;
                    break;
                default:
                    Log.Warning("XEP_DIAGNOSISOBJECTSEX.GetLocalizedDiagnosisObjectTitle", "the given Language {0} is not available - language set to enGB", language);
                    text = Title_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = Title_engb;
            }
            if (TitleMode == XEP_DIAGNOSISOBJECTS_TITLE.HG_TITLE)
            {
                text = Hg_Nummer + " " + text;
            }
            else if (TitleMode == XEP_DIAGNOSISOBJECTS_TITLE.HGUG_NUMMER)
            {
                text = Hgug_Nummer + " " + text;
            }
            else if (TitleMode == XEP_DIAGNOSISOBJECTS_TITLE.GROBZEICHEN)
            {
                text = Grobzeichen + " " + text;
            }
            if (IsHiddenObject && ConfigSettings.IsVerificationMode && ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.ISTAGUI.ShowHiddenDiagnosticObjects", defaultValue: false))
            {
                text = "* " + text;
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

        private string PadNumberInString(string stringToPad)
        {
            if (string.IsNullOrEmpty(stringToPad))
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in Regex.Replace(stringToPad, "(?<=[0-9])(?=[A-Za-z])|(?<=[A-Za-z])(?=[0-9])", " ").Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList())
            {
                if (!item.All(char.IsDigit))
                {
                    stringBuilder.Append(item);
                }
                else
                {
                    stringBuilder.Append(item.PadLeft(8, '0'));
                }
            }
            return stringBuilder.ToString();
        }
    }
}
