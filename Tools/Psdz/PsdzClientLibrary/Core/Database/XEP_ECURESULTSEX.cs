using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECURESULTSEX : INotifyPropertyChanged
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
        private string functionNameResultField;
        private string adapterPathField;
        private string nameField;
        private decimal? fahrzeugTestRelevantField;
        private decimal? fastaRelevantField;
        private decimal? steuergeraeteFunktionenRelevanField;
        private string fastaDataTypeField;
        private string locationField;
        private decimal? fdmRelevantField;
        private decimal? unitFixedField;
        private decimal? ecuJobIdField;
        private string uwTypField;
        private string unitField;
        private string unitNameField;
        private string unitPathField;
        private string formatField;
        private string adapterCountPathField;
        private string countNameField;
        private string phaseField;
        private decimal? multiplikatorField;
        private decimal? offsetField;
        private decimal? rundenField;
        private string zahlenformatField;
        private List<XEP_STATEVALUES> stateValues = new List<XEP_STATEVALUES>();
        private object value;
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

        public decimal? Nodeclass
        {
            get
            {
                return nodeclassField;
            }

            set
            {
                if (nodeclassField.HasValue)
                {
                    if (!nodeclassField.Equals(value))
                    {
                        nodeclassField = value;
                        OnPropertyChanged("Nodeclass");
                    }
                }
                else
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

        public string FunctionNameResult
        {
            get
            {
                return functionNameResultField;
            }

            set
            {
                if (functionNameResultField != null)
                {
                    if (!functionNameResultField.Equals(value))
                    {
                        functionNameResultField = value;
                        OnPropertyChanged("FunctionNameResult");
                    }
                }
                else
                {
                    functionNameResultField = value;
                    OnPropertyChanged("FunctionNameResult");
                }
            }
        }

        public string AdapterPath
        {
            get
            {
                return adapterPathField;
            }

            set
            {
                if (adapterPathField != null)
                {
                    if (!adapterPathField.Equals(value))
                    {
                        adapterPathField = value;
                        OnPropertyChanged("AdapterPath");
                    }
                }
                else
                {
                    adapterPathField = value;
                    OnPropertyChanged("AdapterPath");
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
                if (nameField != null)
                {
                    if (!nameField.Equals(value))
                    {
                        nameField = value;
                        OnPropertyChanged("Name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public decimal? FahrzeugTestRelevant
        {
            get
            {
                return fahrzeugTestRelevantField;
            }

            set
            {
                if (fahrzeugTestRelevantField.HasValue)
                {
                    if (!fahrzeugTestRelevantField.Equals(value))
                    {
                        fahrzeugTestRelevantField = value;
                        OnPropertyChanged("FahrzeugTestRelevant");
                    }
                }
                else
                {
                    fahrzeugTestRelevantField = value;
                    OnPropertyChanged("FahrzeugTestRelevant");
                }
            }
        }

        public decimal? FastaRelevant
        {
            get
            {
                return fastaRelevantField;
            }

            set
            {
                if (fastaRelevantField.HasValue)
                {
                    if (!fastaRelevantField.Equals(value))
                    {
                        fastaRelevantField = value;
                        OnPropertyChanged("FastaRelevant");
                    }
                }
                else
                {
                    fastaRelevantField = value;
                    OnPropertyChanged("FastaRelevant");
                }
            }
        }

        public decimal? SteuergeraeteFunktionenRelevan
        {
            get
            {
                return steuergeraeteFunktionenRelevanField;
            }

            set
            {
                if (steuergeraeteFunktionenRelevanField.HasValue)
                {
                    if (!steuergeraeteFunktionenRelevanField.Equals(value))
                    {
                        steuergeraeteFunktionenRelevanField = value;
                        OnPropertyChanged("SteuergeraeteFunktionenRelevan");
                    }
                }
                else
                {
                    steuergeraeteFunktionenRelevanField = value;
                    OnPropertyChanged("SteuergeraeteFunktionenRelevan");
                }
            }
        }

        public string FastaDataType
        {
            get
            {
                return fastaDataTypeField;
            }

            set
            {
                if (fastaDataTypeField != null)
                {
                    if (!fastaDataTypeField.Equals(value))
                    {
                        fastaDataTypeField = value;
                        OnPropertyChanged("FastaDataType");
                    }
                }
                else
                {
                    fastaDataTypeField = value;
                    OnPropertyChanged("FastaDataType");
                }
            }
        }

        public string Location
        {
            get
            {
                return locationField;
            }

            set
            {
                if (locationField != null)
                {
                    if (!locationField.Equals(value))
                    {
                        locationField = value;
                        OnPropertyChanged("Location");
                    }
                }
                else
                {
                    locationField = value;
                    OnPropertyChanged("Location");
                }
            }
        }

        public decimal? FdmRelevant
        {
            get
            {
                return fdmRelevantField;
            }

            set
            {
                if (fdmRelevantField.HasValue)
                {
                    if (!fdmRelevantField.Equals(value))
                    {
                        fdmRelevantField = value;
                        OnPropertyChanged("FdmRelevant");
                    }
                }
                else
                {
                    fdmRelevantField = value;
                    OnPropertyChanged("FdmRelevant");
                }
            }
        }

        public decimal? UnitFixed
        {
            get
            {
                return unitFixedField;
            }

            set
            {
                if (unitFixedField.HasValue)
                {
                    if (!unitFixedField.Equals(value))
                    {
                        unitFixedField = value;
                        OnPropertyChanged("UnitFixed");
                    }
                }
                else
                {
                    unitFixedField = value;
                    OnPropertyChanged("UnitFixed");
                }
            }
        }

        public decimal? EcuJobId
        {
            get
            {
                return ecuJobIdField;
            }

            set
            {
                if (ecuJobIdField.HasValue)
                {
                    if (!ecuJobIdField.Equals(value))
                    {
                        ecuJobIdField = value;
                        OnPropertyChanged("EcuJobId");
                    }
                }
                else
                {
                    ecuJobIdField = value;
                    OnPropertyChanged("EcuJobId");
                }
            }
        }

        public string UwTyp
        {
            get
            {
                return uwTypField;
            }

            set
            {
                if (uwTypField != null)
                {
                    if (!uwTypField.Equals(value))
                    {
                        uwTypField = value;
                        OnPropertyChanged("UwTyp");
                    }
                }
                else
                {
                    uwTypField = value;
                    OnPropertyChanged("UwTyp");
                }
            }
        }

        public string Unit
        {
            get
            {
                return unitField;
            }

            set
            {
                if (unitField != null)
                {
                    if (!unitField.Equals(value))
                    {
                        unitField = value;
                        OnPropertyChanged("Unit");
                    }
                }
                else
                {
                    unitField = value;
                    OnPropertyChanged("Unit");
                }
            }
        }

        public string UnitName
        {
            get
            {
                return unitNameField;
            }

            set
            {
                if (unitNameField != null)
                {
                    if (!unitNameField.Equals(value))
                    {
                        unitNameField = value;
                        OnPropertyChanged("UnitName");
                    }
                }
                else
                {
                    unitNameField = value;
                    OnPropertyChanged("UnitName");
                }
            }
        }

        public string UnitPath
        {
            get
            {
                return unitPathField;
            }

            set
            {
                if (unitPathField != null)
                {
                    if (!unitPathField.Equals(value))
                    {
                        unitPathField = value;
                        OnPropertyChanged("UnitPath");
                    }
                }
                else
                {
                    unitPathField = value;
                    OnPropertyChanged("UnitPath");
                }
            }
        }

        public string Format
        {
            get
            {
                return formatField;
            }

            set
            {
                if (formatField != null)
                {
                    if (!formatField.Equals(value))
                    {
                        formatField = value;
                        OnPropertyChanged("Format");
                    }
                }
                else
                {
                    formatField = value;
                    OnPropertyChanged("Format");
                }
            }
        }

        public string AdapterCountPath
        {
            get
            {
                return adapterCountPathField;
            }

            set
            {
                if (adapterCountPathField != null)
                {
                    if (!adapterCountPathField.Equals(value))
                    {
                        adapterCountPathField = value;
                        OnPropertyChanged("AdapterCountPath");
                    }
                }
                else
                {
                    adapterCountPathField = value;
                    OnPropertyChanged("AdapterCountPath");
                }
            }
        }

        public string CountName
        {
            get
            {
                return countNameField;
            }

            set
            {
                if (countNameField != null)
                {
                    if (!countNameField.Equals(value))
                    {
                        countNameField = value;
                        OnPropertyChanged("CountName");
                    }
                }
                else
                {
                    countNameField = value;
                    OnPropertyChanged("CountName");
                }
            }
        }

        public string Phase
        {
            get
            {
                return phaseField;
            }

            set
            {
                if (phaseField != null)
                {
                    if (!phaseField.Equals(value))
                    {
                        phaseField = value;
                        OnPropertyChanged("Phase");
                    }
                }
                else
                {
                    phaseField = value;
                    OnPropertyChanged("Phase");
                }
            }
        }

        public decimal? Multiplikator
        {
            get
            {
                return multiplikatorField;
            }

            set
            {
                if (multiplikatorField.HasValue)
                {
                    if (!multiplikatorField.Equals(value))
                    {
                        multiplikatorField = value;
                        OnPropertyChanged("Multiplikator");
                    }
                }
                else
                {
                    multiplikatorField = value;
                    OnPropertyChanged("Multiplikator");
                }
            }
        }

        public decimal? Offset
        {
            get
            {
                return offsetField;
            }

            set
            {
                if (offsetField.HasValue)
                {
                    if (!offsetField.Equals(value))
                    {
                        offsetField = value;
                        OnPropertyChanged("Offset");
                    }
                }
                else
                {
                    offsetField = value;
                    OnPropertyChanged("Offset");
                }
            }
        }

        public decimal? Runden
        {
            get
            {
                return rundenField;
            }

            set
            {
                if (rundenField.HasValue)
                {
                    if (!rundenField.Equals(value))
                    {
                        rundenField = value;
                        OnPropertyChanged("Runden");
                    }
                }
                else
                {
                    rundenField = value;
                    OnPropertyChanged("Runden");
                }
            }
        }

        public string Zahlenformat
        {
            get
            {
                return zahlenformatField;
            }

            set
            {
                if (zahlenformatField != null)
                {
                    if (!zahlenformatField.Equals(value))
                    {
                        zahlenformatField = value;
                        OnPropertyChanged("Zahlenformat");
                    }
                }
                else
                {
                    zahlenformatField = value;
                    OnPropertyChanged("Zahlenformat");
                }
            }
        }

        public List<XEP_STATEVALUES> StateValues
        {
            get
            {
                return stateValues;
            }

            set
            {
                if (value != stateValues)
                {
                    stateValues = value;
                    OnPropertyChanged("StateValues");
                }
            }
        }

        public string Title => GetTitleByLanguage(ConfigSettings.CurrentUICulture);

        public object Value
        {
            get
            {
                return value;
            }

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_ECURESULTSEX Clone()
        {
            return (XEP_ECURESULTSEX)MemberwiseClone();
        }

        public string GetTitleByLanguage(string language)
        {
            string result;
            switch (language)
            {
                case "de-DE":
                    result = Title_dede;
                    break;
                case "en-GB":
                    result = Title_engb;
                    break;
                case "en-US":
                    result = Title_enus;
                    break;
                case "fr-FR":
                    result = Title_fr;
                    break;
                case "es-ES":
                    result = Title_es;
                    break;
                case "th-TH":
                    result = Title_th;
                    break;
                case "tr-TR":
                    result = Title_tr;
                    break;
                case "el-GR":
                    result = Title_el;
                    break;
                case "ja-JP":
                    result = Title_ja;
                    break;
                case "ru-RU":
                    result = Title_ru;
                    break;
                case "it-IT":
                    result = Title_it;
                    break;
                case "nl-NL":
                    result = Title_nl;
                    break;
                case "pl-PL":
                    result = Title_plpl;
                    break;
                case "cs-CZ":
                    result = Title_cscz;
                    break;
                case "pt-PT":
                    result = Title_pt;
                    break;
                case "sv-SE":
                    result = Title_sv;
                    break;
                case "zh-CN":
                    result = Title_zhcn;
                    break;
                case "zh-TW":
                    result = Title_zhtw;
                    break;
                case "ko-KR":
                    result = Title_ko;
                    break;
                default:
                    Log.Warning("XEP_ECUFIXEDFUNCTIONS.get_Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                    result = Title_engb;
                    break;
            }

            if (string.IsNullOrEmpty(result))
            {
                return Title_engb;
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is XEP_ECURESULTSEX xEP_ECURESULTSEX))
            {
                return false;
            }

            if (Id != xEP_ECURESULTSEX.Id)
            {
                return false;
            }

            if (!(Nodeclass == xEP_ECURESULTSEX.Nodeclass))
            {
                return false;
            }

            if (!(TitleId == xEP_ECURESULTSEX.TitleId))
            {
                return false;
            }

            if (string.CompareOrdinal(Title_dede, xEP_ECURESULTSEX.Title_dede) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_engb, xEP_ECURESULTSEX.Title_engb) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_enus, xEP_ECURESULTSEX.Title_enus) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_fr, xEP_ECURESULTSEX.Title_fr) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_th, xEP_ECURESULTSEX.Title_th) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_sv, xEP_ECURESULTSEX.Title_sv) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_it, xEP_ECURESULTSEX.Title_it) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_es, xEP_ECURESULTSEX.Title_es) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_id, xEP_ECURESULTSEX.Title_id) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_ko, xEP_ECURESULTSEX.Title_ko) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_el, xEP_ECURESULTSEX.Title_el) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_tr, xEP_ECURESULTSEX.Title_tr) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_zhcn, xEP_ECURESULTSEX.Title_zhcn) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_ru, xEP_ECURESULTSEX.Title_ru) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_nl, xEP_ECURESULTSEX.Title_nl) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_pt, xEP_ECURESULTSEX.Title_pt) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_zhtw, xEP_ECURESULTSEX.Title_zhtw) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_ja, xEP_ECURESULTSEX.Title_ja) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_cscz, xEP_ECURESULTSEX.Title_cscz) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_plpl, xEP_ECURESULTSEX.Title_plpl) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(FunctionNameResult, xEP_ECURESULTSEX.FunctionNameResult) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(AdapterPath, xEP_ECURESULTSEX.AdapterPath) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Name, xEP_ECURESULTSEX.Name) != 0)
            {
                return false;
            }

            if (!(FahrzeugTestRelevant == xEP_ECURESULTSEX.FahrzeugTestRelevant))
            {
                return false;
            }

            if (!(FastaRelevant == xEP_ECURESULTSEX.FastaRelevant))
            {
                return false;
            }

            if (!(SteuergeraeteFunktionenRelevan == xEP_ECURESULTSEX.SteuergeraeteFunktionenRelevan))
            {
                return false;
            }

            if (string.CompareOrdinal(FastaDataType, xEP_ECURESULTSEX.FastaDataType) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Location, xEP_ECURESULTSEX.Location) != 0)
            {
                return false;
            }

            if (!(FdmRelevant == xEP_ECURESULTSEX.FdmRelevant))
            {
                return false;
            }

            if (string.CompareOrdinal(UwTyp, xEP_ECURESULTSEX.UwTyp) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Unit, xEP_ECURESULTSEX.Unit) != 0)
            {
                return false;
            }

            if (!(UnitFixed == xEP_ECURESULTSEX.UnitFixed))
            {
                return false;
            }

            if (string.CompareOrdinal(UnitName, xEP_ECURESULTSEX.UnitName) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(UnitPath, xEP_ECURESULTSEX.UnitPath) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Format, xEP_ECURESULTSEX.Format) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(AdapterCountPath, xEP_ECURESULTSEX.AdapterCountPath) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(CountName, xEP_ECURESULTSEX.CountName) != 0)
            {
                return false;
            }

            if (!(EcuJobId == xEP_ECURESULTSEX.EcuJobId))
            {
                return false;
            }

            if (!(Multiplikator == xEP_ECURESULTSEX.Multiplikator))
            {
                return false;
            }

            if (!(Offset == xEP_ECURESULTSEX.Offset))
            {
                return false;
            }

            if (!(Runden == xEP_ECURESULTSEX.Runden))
            {
                return false;
            }

            if (Zahlenformat != xEP_ECURESULTSEX.Zahlenformat)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public string GetLocalizedEcuResultTitle(string language)
        {
            string result;
            switch (MatchLanguageToCulture(language))
            {
                case "de-DE":
                    result = Title_dede;
                    break;
                case "en-GB":
                    result = Title_engb;
                    break;
                case "en-US":
                    result = Title_enus;
                    break;
                case "fr-FR":
                    result = Title_fr;
                    break;
                case "es-ES":
                    result = Title_es;
                    break;
                case "th-TH":
                    result = Title_th;
                    break;
                case "tr-TR":
                    result = Title_tr;
                    break;
                case "el-GR":
                    result = Title_el;
                    break;
                case "ja-JP":
                    result = Title_ja;
                    break;
                case "ru-RU":
                    result = Title_ru;
                    break;
                case "it-IT":
                    result = Title_it;
                    break;
                case "nl-NL":
                    result = Title_nl;
                    break;
                case "pl-PL":
                    result = Title_plpl;
                    break;
                case "cs-CZ":
                    result = Title_cscz;
                    break;
                case "pt-PT":
                    result = Title_pt;
                    break;
                case "sv-SE":
                    result = Title_sv;
                    break;
                case "zh-CN":
                    result = Title_zhcn;
                    break;
                case "zh-TW":
                    result = Title_zhtw;
                    break;
                case "ko-KR":
                    result = Title_ko;
                    break;
                default:
                    Log.Warning("XEP_SWIACTIVATIONCODE_SWT.GetLocalizedSwiActivationCodeTitle", "the given language {0} is not available - language set to enGB", language);
                    result = Title_engb;
                    break;
            }

            if (string.IsNullOrEmpty(result))
            {
                return Title_engb;
            }

            return result;
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