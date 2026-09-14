using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class RG_ECUFAULTS : INotifyPropertyChanged
    {
        private decimal ecuFault_idField;
        private string faultCodeField;
        private string ecuVariant_nameField;
        private string dataTypeField;
        private string ausblendIndexField;
        private string diagnoseIndexField;
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
        private string label_dedeField;
        private string label_engbField;
        private string label_enusField;
        private string label_frField;
        private string label_thField;
        private string label_svField;
        private string label_itField;
        private string label_esField;
        private string label_idField;
        private string label_koField;
        private string label_elField;
        private string label_trField;
        private string label_zhcnField;
        private string label_ruField;
        private string label_nlField;
        private string label_ptField;
        private string label_zhtwField;
        private string label_jaField;
        private string label_csczField;
        private string label_plplField;
        private decimal ecuGroupIDField;
        public decimal EcuFault_id
        {
            get
            {
                return ecuFault_idField;
            }

            set
            {
                _ = ecuFault_idField;
                if (!ecuFault_idField.Equals(value))
                {
                    ecuFault_idField = value;
                    OnPropertyChanged("EcuFault_id");
                }
            }
        }

        public string FaultCode
        {
            get
            {
                return faultCodeField;
            }

            set
            {
                if (faultCodeField != null)
                {
                    if (!faultCodeField.Equals(value))
                    {
                        faultCodeField = value;
                        OnPropertyChanged("FaultCode");
                    }
                }
                else
                {
                    faultCodeField = value;
                    OnPropertyChanged("FaultCode");
                }
            }
        }

        public string EcuVariant_name
        {
            get
            {
                return ecuVariant_nameField;
            }

            set
            {
                if (ecuVariant_nameField != null)
                {
                    if (!ecuVariant_nameField.Equals(value))
                    {
                        ecuVariant_nameField = value;
                        OnPropertyChanged("EcuVariant_name");
                    }
                }
                else
                {
                    ecuVariant_nameField = value;
                    OnPropertyChanged("EcuVariant_name");
                }
            }
        }

        public string DataType
        {
            get
            {
                return dataTypeField;
            }

            set
            {
                if (dataTypeField != null)
                {
                    if (!dataTypeField.Equals(value))
                    {
                        dataTypeField = value;
                        OnPropertyChanged("DataType");
                    }
                }
                else
                {
                    dataTypeField = value;
                    OnPropertyChanged("DataType");
                }
            }
        }

        public string AusblendIndex
        {
            get
            {
                return ausblendIndexField;
            }

            set
            {
                if (ausblendIndexField != null)
                {
                    if (!ausblendIndexField.Equals(value))
                    {
                        ausblendIndexField = value;
                        OnPropertyChanged("AusblendIndex");
                    }
                }
                else
                {
                    ausblendIndexField = value;
                    OnPropertyChanged("AusblendIndex");
                }
            }
        }

        public string DiagnoseIndex
        {
            get
            {
                return diagnoseIndexField;
            }

            set
            {
                if (diagnoseIndexField != null)
                {
                    if (!diagnoseIndexField.Equals(value))
                    {
                        diagnoseIndexField = value;
                        OnPropertyChanged("DiagnoseIndex");
                    }
                }
                else
                {
                    diagnoseIndexField = value;
                    OnPropertyChanged("DiagnoseIndex");
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

        public string Label_dede
        {
            get
            {
                return label_dedeField;
            }

            set
            {
                if (label_dedeField != null)
                {
                    if (!label_dedeField.Equals(value))
                    {
                        label_dedeField = value;
                        OnPropertyChanged("Label_dede");
                    }
                }
                else
                {
                    label_dedeField = value;
                    OnPropertyChanged("Label_dede");
                }
            }
        }

        public string Label_engb
        {
            get
            {
                return label_engbField;
            }

            set
            {
                if (label_engbField != null)
                {
                    if (!label_engbField.Equals(value))
                    {
                        label_engbField = value;
                        OnPropertyChanged("Label_engb");
                    }
                }
                else
                {
                    label_engbField = value;
                    OnPropertyChanged("Label_engb");
                }
            }
        }

        public string Label_enus
        {
            get
            {
                return label_enusField;
            }

            set
            {
                if (label_enusField != null)
                {
                    if (!label_enusField.Equals(value))
                    {
                        label_enusField = value;
                        OnPropertyChanged("Label_enus");
                    }
                }
                else
                {
                    label_enusField = value;
                    OnPropertyChanged("Label_enus");
                }
            }
        }

        public string Label_fr
        {
            get
            {
                return label_frField;
            }

            set
            {
                if (label_frField != null)
                {
                    if (!label_frField.Equals(value))
                    {
                        label_frField = value;
                        OnPropertyChanged("Label_fr");
                    }
                }
                else
                {
                    label_frField = value;
                    OnPropertyChanged("Label_fr");
                }
            }
        }

        public string Label_th
        {
            get
            {
                return label_thField;
            }

            set
            {
                if (label_thField != null)
                {
                    if (!label_thField.Equals(value))
                    {
                        label_thField = value;
                        OnPropertyChanged("Label_th");
                    }
                }
                else
                {
                    label_thField = value;
                    OnPropertyChanged("Label_th");
                }
            }
        }

        public string Label_sv
        {
            get
            {
                return label_svField;
            }

            set
            {
                if (label_svField != null)
                {
                    if (!label_svField.Equals(value))
                    {
                        label_svField = value;
                        OnPropertyChanged("Label_sv");
                    }
                }
                else
                {
                    label_svField = value;
                    OnPropertyChanged("Label_sv");
                }
            }
        }

        public string Label_it
        {
            get
            {
                return label_itField;
            }

            set
            {
                if (label_itField != null)
                {
                    if (!label_itField.Equals(value))
                    {
                        label_itField = value;
                        OnPropertyChanged("Label_it");
                    }
                }
                else
                {
                    label_itField = value;
                    OnPropertyChanged("Label_it");
                }
            }
        }

        public string Label_es
        {
            get
            {
                return label_esField;
            }

            set
            {
                if (label_esField != null)
                {
                    if (!label_esField.Equals(value))
                    {
                        label_esField = value;
                        OnPropertyChanged("Label_es");
                    }
                }
                else
                {
                    label_esField = value;
                    OnPropertyChanged("Label_es");
                }
            }
        }

        public string Label_id
        {
            get
            {
                return label_idField;
            }

            set
            {
                if (label_idField != null)
                {
                    if (!label_idField.Equals(value))
                    {
                        label_idField = value;
                        OnPropertyChanged("Label_id");
                    }
                }
                else
                {
                    label_idField = value;
                    OnPropertyChanged("Label_id");
                }
            }
        }

        public string Label_ko
        {
            get
            {
                return label_koField;
            }

            set
            {
                if (label_koField != null)
                {
                    if (!label_koField.Equals(value))
                    {
                        label_koField = value;
                        OnPropertyChanged("Label_ko");
                    }
                }
                else
                {
                    label_koField = value;
                    OnPropertyChanged("Label_ko");
                }
            }
        }

        public string Label_el
        {
            get
            {
                return label_elField;
            }

            set
            {
                if (label_elField != null)
                {
                    if (!label_elField.Equals(value))
                    {
                        label_elField = value;
                        OnPropertyChanged("Label_el");
                    }
                }
                else
                {
                    label_elField = value;
                    OnPropertyChanged("Label_el");
                }
            }
        }

        public string Label_tr
        {
            get
            {
                return label_trField;
            }

            set
            {
                if (label_trField != null)
                {
                    if (!label_trField.Equals(value))
                    {
                        label_trField = value;
                        OnPropertyChanged("Label_tr");
                    }
                }
                else
                {
                    label_trField = value;
                    OnPropertyChanged("Label_tr");
                }
            }
        }

        public string Label_zhcn
        {
            get
            {
                return label_zhcnField;
            }

            set
            {
                if (label_zhcnField != null)
                {
                    if (!label_zhcnField.Equals(value))
                    {
                        label_zhcnField = value;
                        OnPropertyChanged("Label_zhcn");
                    }
                }
                else
                {
                    label_zhcnField = value;
                    OnPropertyChanged("Label_zhcn");
                }
            }
        }

        public string Label_ru
        {
            get
            {
                return label_ruField;
            }

            set
            {
                if (label_ruField != null)
                {
                    if (!label_ruField.Equals(value))
                    {
                        label_ruField = value;
                        OnPropertyChanged("Label_ru");
                    }
                }
                else
                {
                    label_ruField = value;
                    OnPropertyChanged("Label_ru");
                }
            }
        }

        public string Label_nl
        {
            get
            {
                return label_nlField;
            }

            set
            {
                if (label_nlField != null)
                {
                    if (!label_nlField.Equals(value))
                    {
                        label_nlField = value;
                        OnPropertyChanged("Label_nl");
                    }
                }
                else
                {
                    label_nlField = value;
                    OnPropertyChanged("Label_nl");
                }
            }
        }

        public string Label_pt
        {
            get
            {
                return label_ptField;
            }

            set
            {
                if (label_ptField != null)
                {
                    if (!label_ptField.Equals(value))
                    {
                        label_ptField = value;
                        OnPropertyChanged("Label_pt");
                    }
                }
                else
                {
                    label_ptField = value;
                    OnPropertyChanged("Label_pt");
                }
            }
        }

        public string Label_zhtw
        {
            get
            {
                return label_zhtwField;
            }

            set
            {
                if (label_zhtwField != null)
                {
                    if (!label_zhtwField.Equals(value))
                    {
                        label_zhtwField = value;
                        OnPropertyChanged("Label_zhtw");
                    }
                }
                else
                {
                    label_zhtwField = value;
                    OnPropertyChanged("Label_zhtw");
                }
            }
        }

        public string Label_ja
        {
            get
            {
                return label_jaField;
            }

            set
            {
                if (label_jaField != null)
                {
                    if (!label_jaField.Equals(value))
                    {
                        label_jaField = value;
                        OnPropertyChanged("Label_ja");
                    }
                }
                else
                {
                    label_jaField = value;
                    OnPropertyChanged("Label_ja");
                }
            }
        }

        public string Label_cscz
        {
            get
            {
                return label_csczField;
            }

            set
            {
                if (label_csczField != null)
                {
                    if (!label_csczField.Equals(value))
                    {
                        label_csczField = value;
                        OnPropertyChanged("Label_cscz");
                    }
                }
                else
                {
                    label_csczField = value;
                    OnPropertyChanged("Label_cscz");
                }
            }
        }

        public string Label_plpl
        {
            get
            {
                return label_plplField;
            }

            set
            {
                if (label_plplField != null)
                {
                    if (!label_plplField.Equals(value))
                    {
                        label_plplField = value;
                        OnPropertyChanged("Label_plpl");
                    }
                }
                else
                {
                    label_plplField = value;
                    OnPropertyChanged("Label_plpl");
                }
            }
        }

        public decimal ecuGroupID
        {
            get
            {
                return ecuGroupIDField;
            }

            set
            {
                _ = ecuGroupIDField;
                if (!ecuGroupIDField.Equals(value))
                {
                    ecuGroupIDField = value;
                    OnPropertyChanged("EcuGroupID");
                }
            }
        }

        public string Label
        {
            get
            {
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        return Label_dede;
                    case "en-GB":
                        return Label_engb;
                    case "en-US":
                        return Label_enus;
                    case "fr-FR":
                        return Label_fr;
                    case "es-ES":
                        return Label_es;
                    case "th-TH":
                        return Label_th;
                    case "tr-TR":
                        return Label_tr;
                    case "el-GR":
                        return Label_el;
                    case "ja-JP":
                        return Label_ja;
                    case "ru-RU":
                        return Label_ru;
                    case "it-IT":
                        return Label_it;
                    case "nl-NL":
                        return Label_nl;
                    case "pl-PL":
                        return Label_plpl;
                    case "cs-CZ":
                        return Label_cscz;
                    case "pt-PT":
                        return Label_pt;
                    case "sv-SE":
                        return Label_sv;
                    case "zh-CN":
                        return Label_zhcn;
                    case "zh-TW":
                        return Label_zhtw;
                    case "ko-KR":
                        return Label_ko;
                    default:
                        Log.Warning("RG_ECUFAULTS.get_Label", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        return Label_engb;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is RG_ECUFAULTS rG_ECUFAULTS))
            {
                return false;
            }

            if (EcuFault_id != rG_ECUFAULTS.EcuFault_id)
            {
                return false;
            }

            if (ecuGroupID != rG_ECUFAULTS.ecuGroupID)
            {
                return false;
            }

            if (string.CompareOrdinal(FaultCode, rG_ECUFAULTS.FaultCode) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(EcuVariant_name, rG_ECUFAULTS.EcuVariant_name) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(DataType, rG_ECUFAULTS.DataType) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(AusblendIndex, rG_ECUFAULTS.AusblendIndex) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(DiagnoseIndex, rG_ECUFAULTS.DiagnoseIndex) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_dede, rG_ECUFAULTS.Title_dede) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_engb, rG_ECUFAULTS.Title_engb) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_enus, rG_ECUFAULTS.Title_enus) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_fr, rG_ECUFAULTS.Title_fr) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_th, rG_ECUFAULTS.Title_th) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_sv, rG_ECUFAULTS.Title_sv) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_it, rG_ECUFAULTS.Title_it) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_es, rG_ECUFAULTS.Title_es) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_id, rG_ECUFAULTS.Title_id) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_ko, rG_ECUFAULTS.Title_ko) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_el, rG_ECUFAULTS.Title_el) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_tr, rG_ECUFAULTS.Title_tr) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_zhcn, rG_ECUFAULTS.Title_zhcn) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_ru, rG_ECUFAULTS.Title_ru) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_nl, rG_ECUFAULTS.Title_nl) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_pt, rG_ECUFAULTS.Title_pt) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_zhtw, rG_ECUFAULTS.Title_zhtw) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_ja, rG_ECUFAULTS.Title_ja) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_cscz, rG_ECUFAULTS.Title_cscz) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Title_plpl, rG_ECUFAULTS.Title_plpl) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_dede, rG_ECUFAULTS.Label_dede) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_engb, rG_ECUFAULTS.Label_engb) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_enus, rG_ECUFAULTS.Label_enus) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_fr, rG_ECUFAULTS.Label_fr) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_th, rG_ECUFAULTS.Label_th) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_sv, rG_ECUFAULTS.Label_sv) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_it, rG_ECUFAULTS.Label_it) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_es, rG_ECUFAULTS.Label_es) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_id, rG_ECUFAULTS.Label_id) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_ko, rG_ECUFAULTS.Label_ko) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_el, rG_ECUFAULTS.Label_el) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_tr, rG_ECUFAULTS.Label_tr) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_zhcn, rG_ECUFAULTS.Label_zhcn) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_ru, rG_ECUFAULTS.Label_ru) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_nl, rG_ECUFAULTS.Label_nl) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_pt, rG_ECUFAULTS.Label_pt) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_zhtw, rG_ECUFAULTS.Label_zhtw) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_ja, rG_ECUFAULTS.Label_ja) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_cscz, rG_ECUFAULTS.Label_cscz) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(Label_plpl, rG_ECUFAULTS.Label_plpl) != 0)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return EcuFault_id.GetHashCode();
        }
    }
}