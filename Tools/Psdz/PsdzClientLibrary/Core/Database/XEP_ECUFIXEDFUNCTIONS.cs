using PsdzClient.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUFIXEDFUNCTIONS : INotifyPropertyChanged
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

        private decimal? teleserviceRelevantField;

        private decimal? sicherheitsRelevantField;

        private DateTime? validFromField;

        private DateTime? validToField;

        private decimal? fastaRelevantField;

        private decimal? steuergeraeteFunktionenRelevanField;

        private decimal? fahrzeugTestRelevantField;

        private string fastaDataTypeField;

        private decimal? fdmRelevantField;

        private decimal? preparingOperatorTextIdField;

        private string preparingOperatorText_dedeField;

        private string preparingOperatorText_engbField;

        private string preparingOperatorText_enusField;

        private string preparingOperatorText_frField;

        private string preparingOperatorText_thField;

        private string preparingOperatorText_svField;

        private string preparingOperatorText_itField;

        private string preparingOperatorText_esField;

        private string preparingOperatorText_idField;

        private string preparingOperatorText_koField;

        private string preparingOperatorText_elField;

        private string preparingOperatorText_trField;

        private string preparingOperatorText_zhcnField;

        private string preparingOperatorText_ruField;

        private string preparingOperatorText_nlField;

        private string preparingOperatorText_ptField;

        private string preparingOperatorText_zhtwField;

        private string preparingOperatorText_jaField;

        private string preparingOperatorText_csczField;

        private string preparingOperatorText_plplField;

        private decimal? processingOperatorTextIdField;

        private string processingOperatorText_dedeField;

        private string processingOperatorText_engbField;

        private string processingOperatorText_enusField;

        private string processingOperatorText_frField;

        private string processingOperatorText_thField;

        private string processingOperatorText_svField;

        private string processingOperatorText_itField;

        private string processingOperatorText_esField;

        private string processingOperatorText_idField;

        private string processingOperatorText_koField;

        private string processingOperatorText_elField;

        private string processingOperatorText_trField;

        private string processingOperatorText_zhcnField;

        private string processingOperatorText_ruField;

        private string processingOperatorText_nlField;

        private string processingOperatorText_ptField;

        private string processingOperatorText_zhtwField;

        private string processingOperatorText_jaField;

        private string processingOperatorText_csczField;

        private string processingOperatorText_plplField;

        private decimal? postOperatorTextIdField;

        private string postOperatorText_dedeField;

        private string postOperatorText_engbField;

        private string postOperatorText_enusField;

        private string postOperatorText_frField;

        private string postOperatorText_thField;

        private string postOperatorText_svField;

        private string postOperatorText_itField;

        private string postOperatorText_esField;

        private string postOperatorText_idField;

        private string postOperatorText_koField;

        private string postOperatorText_elField;

        private string postOperatorText_trField;

        private string postOperatorText_zhcnField;

        private string postOperatorText_ruField;

        private string postOperatorText_nlField;

        private string postOperatorText_ptField;

        private string postOperatorText_zhtwField;

        private string postOperatorText_jaField;

        private string postOperatorText_csczField;

        private string postOperatorText_plplField;

        private decimal? parentIdField;

        private decimal? sortOrderField;

        private decimal activationField;

        private decimal activationDurationMsField;

        private ObservableCollection<XEP_ECUJOBSEX> jobs;

        private bool isSelected;

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

        public decimal? TeleserviceRelevant
        {
            get
            {
                return teleserviceRelevantField;
            }
            set
            {
                if (!teleserviceRelevantField.HasValue || !teleserviceRelevantField.Equals(value))
                {
                    teleserviceRelevantField = value;
                    OnPropertyChanged("TeleserviceRelevant");
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

        public decimal? FastaRelevant
        {
            get
            {
                return fastaRelevantField;
            }
            set
            {
                if (!fastaRelevantField.HasValue || !fastaRelevantField.Equals(value))
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
                if (!steuergeraeteFunktionenRelevanField.HasValue || !steuergeraeteFunktionenRelevanField.Equals(value))
                {
                    steuergeraeteFunktionenRelevanField = value;
                    OnPropertyChanged("SteuergeraeteFunktionenRelevan");
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
                if (!fahrzeugTestRelevantField.HasValue || !fahrzeugTestRelevantField.Equals(value))
                {
                    fahrzeugTestRelevantField = value;
                    OnPropertyChanged("FahrzeugTestRelevant");
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
                if (fastaDataTypeField == null || !fastaDataTypeField.Equals(value))
                {
                    fastaDataTypeField = value;
                    OnPropertyChanged("FastaDataType");
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
                if (!fdmRelevantField.HasValue || !fdmRelevantField.Equals(value))
                {
                    fdmRelevantField = value;
                    OnPropertyChanged("FdmRelevant");
                }
            }
        }

        public decimal? PreparingOperatorTextId
        {
            get
            {
                return preparingOperatorTextIdField;
            }
            set
            {
                if (!preparingOperatorTextIdField.HasValue || !preparingOperatorTextIdField.Equals(value))
                {
                    preparingOperatorTextIdField = value;
                    OnPropertyChanged("PreparingOperatorTextId");
                }
            }
        }

        public string PreparingOperatorText_dede
        {
            get
            {
                return preparingOperatorText_dedeField;
            }
            set
            {
                if (preparingOperatorText_dedeField == null || !preparingOperatorText_dedeField.Equals(value))
                {
                    preparingOperatorText_dedeField = value;
                    OnPropertyChanged("PreparingOperatorText_dede");
                }
            }
        }

        public string PreparingOperatorText_engb
        {
            get
            {
                return preparingOperatorText_engbField;
            }
            set
            {
                if (preparingOperatorText_engbField == null || !preparingOperatorText_engbField.Equals(value))
                {
                    preparingOperatorText_engbField = value;
                    OnPropertyChanged("PreparingOperatorText_engb");
                }
            }
        }

        public string PreparingOperatorText_enus
        {
            get
            {
                return preparingOperatorText_enusField;
            }
            set
            {
                if (preparingOperatorText_enusField == null || !preparingOperatorText_enusField.Equals(value))
                {
                    preparingOperatorText_enusField = value;
                    OnPropertyChanged("PreparingOperatorText_enus");
                }
            }
        }

        public string PreparingOperatorText_fr
        {
            get
            {
                return preparingOperatorText_frField;
            }
            set
            {
                if (preparingOperatorText_frField == null || !preparingOperatorText_frField.Equals(value))
                {
                    preparingOperatorText_frField = value;
                    OnPropertyChanged("PreparingOperatorText_fr");
                }
            }
        }

        public string PreparingOperatorText_th
        {
            get
            {
                return preparingOperatorText_thField;
            }
            set
            {
                if (preparingOperatorText_thField == null || !preparingOperatorText_thField.Equals(value))
                {
                    preparingOperatorText_thField = value;
                    OnPropertyChanged("PreparingOperatorText_th");
                }
            }
        }

        public string PreparingOperatorText_sv
        {
            get
            {
                return preparingOperatorText_svField;
            }
            set
            {
                if (preparingOperatorText_svField == null || !preparingOperatorText_svField.Equals(value))
                {
                    preparingOperatorText_svField = value;
                    OnPropertyChanged("PreparingOperatorText_sv");
                }
            }
        }

        public string PreparingOperatorText_it
        {
            get
            {
                return preparingOperatorText_itField;
            }
            set
            {
                if (preparingOperatorText_itField == null || !preparingOperatorText_itField.Equals(value))
                {
                    preparingOperatorText_itField = value;
                    OnPropertyChanged("PreparingOperatorText_it");
                }
            }
        }

        public string PreparingOperatorText_es
        {
            get
            {
                return preparingOperatorText_esField;
            }
            set
            {
                if (preparingOperatorText_esField == null || !preparingOperatorText_esField.Equals(value))
                {
                    preparingOperatorText_esField = value;
                    OnPropertyChanged("PreparingOperatorText_es");
                }
            }
        }

        public string PreparingOperatorText_id
        {
            get
            {
                return preparingOperatorText_idField;
            }
            set
            {
                if (preparingOperatorText_idField == null || !preparingOperatorText_idField.Equals(value))
                {
                    preparingOperatorText_idField = value;
                    OnPropertyChanged("PreparingOperatorText_id");
                }
            }
        }

        public string PreparingOperatorText_ko
        {
            get
            {
                return preparingOperatorText_koField;
            }
            set
            {
                if (preparingOperatorText_koField == null || !preparingOperatorText_koField.Equals(value))
                {
                    preparingOperatorText_koField = value;
                    OnPropertyChanged("PreparingOperatorText_ko");
                }
            }
        }

        public string PreparingOperatorText_el
        {
            get
            {
                return preparingOperatorText_elField;
            }
            set
            {
                if (preparingOperatorText_elField == null || !preparingOperatorText_elField.Equals(value))
                {
                    preparingOperatorText_elField = value;
                    OnPropertyChanged("PreparingOperatorText_el");
                }
            }
        }

        public string PreparingOperatorText_tr
        {
            get
            {
                return preparingOperatorText_trField;
            }
            set
            {
                if (preparingOperatorText_trField == null || !preparingOperatorText_trField.Equals(value))
                {
                    preparingOperatorText_trField = value;
                    OnPropertyChanged("PreparingOperatorText_tr");
                }
            }
        }

        public string PreparingOperatorText_zhcn
        {
            get
            {
                return preparingOperatorText_zhcnField;
            }
            set
            {
                if (preparingOperatorText_zhcnField == null || !preparingOperatorText_zhcnField.Equals(value))
                {
                    preparingOperatorText_zhcnField = value;
                    OnPropertyChanged("PreparingOperatorText_zhcn");
                }
            }
        }

        public string PreparingOperatorText_ru
        {
            get
            {
                return preparingOperatorText_ruField;
            }
            set
            {
                if (preparingOperatorText_ruField == null || !preparingOperatorText_ruField.Equals(value))
                {
                    preparingOperatorText_ruField = value;
                    OnPropertyChanged("PreparingOperatorText_ru");
                }
            }
        }

        public string PreparingOperatorText_nl
        {
            get
            {
                return preparingOperatorText_nlField;
            }
            set
            {
                if (preparingOperatorText_nlField == null || !preparingOperatorText_nlField.Equals(value))
                {
                    preparingOperatorText_nlField = value;
                    OnPropertyChanged("PreparingOperatorText_nl");
                }
            }
        }

        public string PreparingOperatorText_pt
        {
            get
            {
                return preparingOperatorText_ptField;
            }
            set
            {
                if (preparingOperatorText_ptField == null || !preparingOperatorText_ptField.Equals(value))
                {
                    preparingOperatorText_ptField = value;
                    OnPropertyChanged("PreparingOperatorText_pt");
                }
            }
        }

        public string PreparingOperatorText_zhtw
        {
            get
            {
                return preparingOperatorText_zhtwField;
            }
            set
            {
                if (preparingOperatorText_zhtwField == null || !preparingOperatorText_zhtwField.Equals(value))
                {
                    preparingOperatorText_zhtwField = value;
                    OnPropertyChanged("PreparingOperatorText_zhtw");
                }
            }
        }

        public string PreparingOperatorText_ja
        {
            get
            {
                return preparingOperatorText_jaField;
            }
            set
            {
                if (preparingOperatorText_jaField == null || !preparingOperatorText_jaField.Equals(value))
                {
                    preparingOperatorText_jaField = value;
                    OnPropertyChanged("PreparingOperatorText_ja");
                }
            }
        }

        public string PreparingOperatorText_cscz
        {
            get
            {
                return preparingOperatorText_csczField;
            }
            set
            {
                if (preparingOperatorText_csczField == null || !preparingOperatorText_csczField.Equals(value))
                {
                    preparingOperatorText_csczField = value;
                    OnPropertyChanged("PreparingOperatorText_cscz");
                }
            }
        }

        public string PreparingOperatorText_plpl
        {
            get
            {
                return preparingOperatorText_plplField;
            }
            set
            {
                if (preparingOperatorText_plplField == null || !preparingOperatorText_plplField.Equals(value))
                {
                    preparingOperatorText_plplField = value;
                    OnPropertyChanged("PreparingOperatorText_plpl");
                }
            }
        }

        public decimal? ProcessingOperatorTextId
        {
            get
            {
                return processingOperatorTextIdField;
            }
            set
            {
                if (!processingOperatorTextIdField.HasValue || !processingOperatorTextIdField.Equals(value))
                {
                    processingOperatorTextIdField = value;
                    OnPropertyChanged("ProcessingOperatorTextId");
                }
            }
        }

        public string ProcessingOperatorText_dede
        {
            get
            {
                return processingOperatorText_dedeField;
            }
            set
            {
                if (processingOperatorText_dedeField == null || !processingOperatorText_dedeField.Equals(value))
                {
                    processingOperatorText_dedeField = value;
                    OnPropertyChanged("ProcessingOperatorText_dede");
                }
            }
        }

        public string ProcessingOperatorText_engb
        {
            get
            {
                return processingOperatorText_engbField;
            }
            set
            {
                if (processingOperatorText_engbField == null || !processingOperatorText_engbField.Equals(value))
                {
                    processingOperatorText_engbField = value;
                    OnPropertyChanged("ProcessingOperatorText_engb");
                }
            }
        }

        public string ProcessingOperatorText_enus
        {
            get
            {
                return processingOperatorText_enusField;
            }
            set
            {
                if (processingOperatorText_enusField == null || !processingOperatorText_enusField.Equals(value))
                {
                    processingOperatorText_enusField = value;
                    OnPropertyChanged("ProcessingOperatorText_enus");
                }
            }
        }

        public string ProcessingOperatorText_fr
        {
            get
            {
                return processingOperatorText_frField;
            }
            set
            {
                if (processingOperatorText_frField == null || !processingOperatorText_frField.Equals(value))
                {
                    processingOperatorText_frField = value;
                    OnPropertyChanged("ProcessingOperatorText_fr");
                }
            }
        }

        public string ProcessingOperatorText_th
        {
            get
            {
                return processingOperatorText_thField;
            }
            set
            {
                if (processingOperatorText_thField == null || !processingOperatorText_thField.Equals(value))
                {
                    processingOperatorText_thField = value;
                    OnPropertyChanged("ProcessingOperatorText_th");
                }
            }
        }

        public string ProcessingOperatorText_sv
        {
            get
            {
                return processingOperatorText_svField;
            }
            set
            {
                if (processingOperatorText_svField == null || !processingOperatorText_svField.Equals(value))
                {
                    processingOperatorText_svField = value;
                    OnPropertyChanged("ProcessingOperatorText_sv");
                }
            }
        }

        public string ProcessingOperatorText_it
        {
            get
            {
                return processingOperatorText_itField;
            }
            set
            {
                if (processingOperatorText_itField == null || !processingOperatorText_itField.Equals(value))
                {
                    processingOperatorText_itField = value;
                    OnPropertyChanged("ProcessingOperatorText_it");
                }
            }
        }

        public string ProcessingOperatorText_es
        {
            get
            {
                return processingOperatorText_esField;
            }
            set
            {
                if (processingOperatorText_esField == null || !processingOperatorText_esField.Equals(value))
                {
                    processingOperatorText_esField = value;
                    OnPropertyChanged("ProcessingOperatorText_es");
                }
            }
        }

        public string ProcessingOperatorText_id
        {
            get
            {
                return processingOperatorText_idField;
            }
            set
            {
                if (processingOperatorText_idField == null || !processingOperatorText_idField.Equals(value))
                {
                    processingOperatorText_idField = value;
                    OnPropertyChanged("ProcessingOperatorText_id");
                }
            }
        }

        public string ProcessingOperatorText_ko
        {
            get
            {
                return processingOperatorText_koField;
            }
            set
            {
                if (processingOperatorText_koField == null || !processingOperatorText_koField.Equals(value))
                {
                    processingOperatorText_koField = value;
                    OnPropertyChanged("ProcessingOperatorText_ko");
                }
            }
        }

        public string ProcessingOperatorText_el
        {
            get
            {
                return processingOperatorText_elField;
            }
            set
            {
                if (processingOperatorText_elField == null || !processingOperatorText_elField.Equals(value))
                {
                    processingOperatorText_elField = value;
                    OnPropertyChanged("ProcessingOperatorText_el");
                }
            }
        }

        public string ProcessingOperatorText_tr
        {
            get
            {
                return processingOperatorText_trField;
            }
            set
            {
                if (processingOperatorText_trField == null || !processingOperatorText_trField.Equals(value))
                {
                    processingOperatorText_trField = value;
                    OnPropertyChanged("ProcessingOperatorText_tr");
                }
            }
        }

        public string ProcessingOperatorText_zhcn
        {
            get
            {
                return processingOperatorText_zhcnField;
            }
            set
            {
                if (processingOperatorText_zhcnField == null || !processingOperatorText_zhcnField.Equals(value))
                {
                    processingOperatorText_zhcnField = value;
                    OnPropertyChanged("ProcessingOperatorText_zhcn");
                }
            }
        }

        public string ProcessingOperatorText_ru
        {
            get
            {
                return processingOperatorText_ruField;
            }
            set
            {
                if (processingOperatorText_ruField == null || !processingOperatorText_ruField.Equals(value))
                {
                    processingOperatorText_ruField = value;
                    OnPropertyChanged("ProcessingOperatorText_ru");
                }
            }
        }

        public string ProcessingOperatorText_nl
        {
            get
            {
                return processingOperatorText_nlField;
            }
            set
            {
                if (processingOperatorText_nlField == null || !processingOperatorText_nlField.Equals(value))
                {
                    processingOperatorText_nlField = value;
                    OnPropertyChanged("ProcessingOperatorText_nl");
                }
            }
        }

        public string ProcessingOperatorText_pt
        {
            get
            {
                return processingOperatorText_ptField;
            }
            set
            {
                if (processingOperatorText_ptField == null || !processingOperatorText_ptField.Equals(value))
                {
                    processingOperatorText_ptField = value;
                    OnPropertyChanged("ProcessingOperatorText_pt");
                }
            }
        }

        public string ProcessingOperatorText_zhtw
        {
            get
            {
                return processingOperatorText_zhtwField;
            }
            set
            {
                if (processingOperatorText_zhtwField == null || !processingOperatorText_zhtwField.Equals(value))
                {
                    processingOperatorText_zhtwField = value;
                    OnPropertyChanged("ProcessingOperatorText_zhtw");
                }
            }
        }

        public string ProcessingOperatorText_ja
        {
            get
            {
                return processingOperatorText_jaField;
            }
            set
            {
                if (processingOperatorText_jaField == null || !processingOperatorText_jaField.Equals(value))
                {
                    processingOperatorText_jaField = value;
                    OnPropertyChanged("ProcessingOperatorText_ja");
                }
            }
        }

        public string ProcessingOperatorText_cscz
        {
            get
            {
                return processingOperatorText_csczField;
            }
            set
            {
                if (processingOperatorText_csczField == null || !processingOperatorText_csczField.Equals(value))
                {
                    processingOperatorText_csczField = value;
                    OnPropertyChanged("ProcessingOperatorText_cscz");
                }
            }
        }

        public string ProcessingOperatorText_plpl
        {
            get
            {
                return processingOperatorText_plplField;
            }
            set
            {
                if (processingOperatorText_plplField == null || !processingOperatorText_plplField.Equals(value))
                {
                    processingOperatorText_plplField = value;
                    OnPropertyChanged("ProcessingOperatorText_plpl");
                }
            }
        }

        public decimal? PostOperatorTextId
        {
            get
            {
                return postOperatorTextIdField;
            }
            set
            {
                if (!postOperatorTextIdField.HasValue || !postOperatorTextIdField.Equals(value))
                {
                    postOperatorTextIdField = value;
                    OnPropertyChanged("PostOperatorTextId");
                }
            }
        }

        public string PostOperatorText_dede
        {
            get
            {
                return postOperatorText_dedeField;
            }
            set
            {
                if (postOperatorText_dedeField == null || !postOperatorText_dedeField.Equals(value))
                {
                    postOperatorText_dedeField = value;
                    OnPropertyChanged("PostOperatorText_dede");
                }
            }
        }

        public string PostOperatorText_engb
        {
            get
            {
                return postOperatorText_engbField;
            }
            set
            {
                if (postOperatorText_engbField == null || !postOperatorText_engbField.Equals(value))
                {
                    postOperatorText_engbField = value;
                    OnPropertyChanged("PostOperatorText_engb");
                }
            }
        }

        public string PostOperatorText_enus
        {
            get
            {
                return postOperatorText_enusField;
            }
            set
            {
                if (postOperatorText_enusField == null || !postOperatorText_enusField.Equals(value))
                {
                    postOperatorText_enusField = value;
                    OnPropertyChanged("PostOperatorText_enus");
                }
            }
        }

        public string PostOperatorText_fr
        {
            get
            {
                return postOperatorText_frField;
            }
            set
            {
                if (postOperatorText_frField == null || !postOperatorText_frField.Equals(value))
                {
                    postOperatorText_frField = value;
                    OnPropertyChanged("PostOperatorText_fr");
                }
            }
        }

        public string PostOperatorText_th
        {
            get
            {
                return postOperatorText_thField;
            }
            set
            {
                if (postOperatorText_thField == null || !postOperatorText_thField.Equals(value))
                {
                    postOperatorText_thField = value;
                    OnPropertyChanged("PostOperatorText_th");
                }
            }
        }

        public string PostOperatorText_sv
        {
            get
            {
                return postOperatorText_svField;
            }
            set
            {
                if (postOperatorText_svField == null || !postOperatorText_svField.Equals(value))
                {
                    postOperatorText_svField = value;
                    OnPropertyChanged("PostOperatorText_sv");
                }
            }
        }

        public string PostOperatorText_it
        {
            get
            {
                return postOperatorText_itField;
            }
            set
            {
                if (postOperatorText_itField == null || !postOperatorText_itField.Equals(value))
                {
                    postOperatorText_itField = value;
                    OnPropertyChanged("PostOperatorText_it");
                }
            }
        }

        public string PostOperatorText_es
        {
            get
            {
                return postOperatorText_esField;
            }
            set
            {
                if (postOperatorText_esField == null || !postOperatorText_esField.Equals(value))
                {
                    postOperatorText_esField = value;
                    OnPropertyChanged("PostOperatorText_es");
                }
            }
        }

        public string PostOperatorText_id
        {
            get
            {
                return postOperatorText_idField;
            }
            set
            {
                if (postOperatorText_idField == null || !postOperatorText_idField.Equals(value))
                {
                    postOperatorText_idField = value;
                    OnPropertyChanged("PostOperatorText_id");
                }
            }
        }

        public string PostOperatorText_ko
        {
            get
            {
                return postOperatorText_koField;
            }
            set
            {
                if (postOperatorText_koField == null || !postOperatorText_koField.Equals(value))
                {
                    postOperatorText_koField = value;
                    OnPropertyChanged("PostOperatorText_ko");
                }
            }
        }

        public string PostOperatorText_el
        {
            get
            {
                return postOperatorText_elField;
            }
            set
            {
                if (postOperatorText_elField == null || !postOperatorText_elField.Equals(value))
                {
                    postOperatorText_elField = value;
                    OnPropertyChanged("PostOperatorText_el");
                }
            }
        }

        public string PostOperatorText_tr
        {
            get
            {
                return postOperatorText_trField;
            }
            set
            {
                if (postOperatorText_trField == null || !postOperatorText_trField.Equals(value))
                {
                    postOperatorText_trField = value;
                    OnPropertyChanged("PostOperatorText_tr");
                }
            }
        }

        public string PostOperatorText_zhcn
        {
            get
            {
                return postOperatorText_zhcnField;
            }
            set
            {
                if (postOperatorText_zhcnField == null || !postOperatorText_zhcnField.Equals(value))
                {
                    postOperatorText_zhcnField = value;
                    OnPropertyChanged("PostOperatorText_zhcn");
                }
            }
        }

        public string PostOperatorText_ru
        {
            get
            {
                return postOperatorText_ruField;
            }
            set
            {
                if (postOperatorText_ruField == null || !postOperatorText_ruField.Equals(value))
                {
                    postOperatorText_ruField = value;
                    OnPropertyChanged("PostOperatorText_ru");
                }
            }
        }

        public string PostOperatorText_nl
        {
            get
            {
                return postOperatorText_nlField;
            }
            set
            {
                if (postOperatorText_nlField == null || !postOperatorText_nlField.Equals(value))
                {
                    postOperatorText_nlField = value;
                    OnPropertyChanged("PostOperatorText_nl");
                }
            }
        }

        public string PostOperatorText_pt
        {
            get
            {
                return postOperatorText_ptField;
            }
            set
            {
                if (postOperatorText_ptField == null || !postOperatorText_ptField.Equals(value))
                {
                    postOperatorText_ptField = value;
                    OnPropertyChanged("PostOperatorText_pt");
                }
            }
        }

        public string PostOperatorText_zhtw
        {
            get
            {
                return postOperatorText_zhtwField;
            }
            set
            {
                if (postOperatorText_zhtwField == null || !postOperatorText_zhtwField.Equals(value))
                {
                    postOperatorText_zhtwField = value;
                    OnPropertyChanged("PostOperatorText_zhtw");
                }
            }
        }

        public string PostOperatorText_ja
        {
            get
            {
                return postOperatorText_jaField;
            }
            set
            {
                if (postOperatorText_jaField == null || !postOperatorText_jaField.Equals(value))
                {
                    postOperatorText_jaField = value;
                    OnPropertyChanged("PostOperatorText_ja");
                }
            }
        }

        public string PostOperatorText_cscz
        {
            get
            {
                return postOperatorText_csczField;
            }
            set
            {
                if (postOperatorText_csczField == null || !postOperatorText_csczField.Equals(value))
                {
                    postOperatorText_csczField = value;
                    OnPropertyChanged("PostOperatorText_cscz");
                }
            }
        }

        public string PostOperatorText_plpl
        {
            get
            {
                return postOperatorText_plplField;
            }
            set
            {
                if (postOperatorText_plplField == null || !postOperatorText_plplField.Equals(value))
                {
                    postOperatorText_plplField = value;
                    OnPropertyChanged("PostOperatorText_plpl");
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

        public decimal Activation
        {
            get
            {
                return activationField;
            }
            set
            {
                if (!activationField.Equals(value))
                {
                    activationField = value;
                    OnPropertyChanged("Activation");
                }
            }
        }

        public decimal ActivationDurationMs
        {
            get
            {
                return activationDurationMsField;
            }
            set
            {
                if (!activationDurationMsField.Equals(value))
                {
                    activationDurationMsField = value;
                    OnPropertyChanged("ActivationDurationMs");
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public decimal SortOrderNullAtEnd => SortOrder ?? decimal.MaxValue;

        public ObservableCollection<XEP_ECUJOBSEX> Jobs
        {
            get
            {
                return jobs;
            }
            set
            {
                if (value != jobs)
                {
                    jobs = value;
                    OnPropertyChanged("Jobs");
                }
            }
        }

        public string PostOperatorText
        {
            get
            {
                string text;
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        text = PostOperatorText_dede;
                        break;
                    case "en-GB":
                        text = PostOperatorText_engb;
                        break;
                    case "en-US":
                        text = PostOperatorText_enus;
                        break;
                    case "fr-FR":
                        text = PostOperatorText_fr;
                        break;
                    case "es-ES":
                        text = PostOperatorText_es;
                        break;
                    case "th-TH":
                        text = PostOperatorText_th;
                        break;
                    case "tr-TR":
                        text = PostOperatorText_tr;
                        break;
                    case "el-GR":
                        text = PostOperatorText_el;
                        break;
                    case "ja-JP":
                        text = PostOperatorText_ja;
                        break;
                    case "ru-RU":
                        text = PostOperatorText_ru;
                        break;
                    case "it-IT":
                        text = PostOperatorText_it;
                        break;
                    case "nl-NL":
                        text = PostOperatorText_nl;
                        break;
                    case "pl-PL":
                        text = PostOperatorText_plpl;
                        break;
                    case "cs-CZ":
                        text = PostOperatorText_cscz;
                        break;
                    case "pt-PT":
                        text = PostOperatorText_pt;
                        break;
                    case "sv-SE":
                        text = PostOperatorText_sv;
                        break;
                    case "zh-CN":
                        text = PostOperatorText_zhcn;
                        break;
                    case "zh-TW":
                        text = PostOperatorText_zhtw;
                        break;
                    case "ko-KR":
                        text = PostOperatorText_ko;
                        break;
                    default:
                        Log.Warning("XEP_ECUFIXEDFUNCTIONS.PostOperatorText", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = PostOperatorText_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    return PostOperatorText_engb;
                }
                return text;
            }
        }

        public string PreparingOperatorText
        {
            get
            {
                string text;
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        text = PreparingOperatorText_dede;
                        break;
                    case "en-GB":
                        text = PreparingOperatorText_engb;
                        break;
                    case "en-US":
                        text = PreparingOperatorText_enus;
                        break;
                    case "fr-FR":
                        text = PreparingOperatorText_fr;
                        break;
                    case "es-ES":
                        text = PreparingOperatorText_es;
                        break;
                    case "th-TH":
                        text = PreparingOperatorText_th;
                        break;
                    case "tr-TR":
                        text = PreparingOperatorText_tr;
                        break;
                    case "el-GR":
                        text = PreparingOperatorText_el;
                        break;
                    case "ja-JP":
                        text = PreparingOperatorText_ja;
                        break;
                    case "ru-RU":
                        text = PreparingOperatorText_ru;
                        break;
                    case "it-IT":
                        text = PreparingOperatorText_it;
                        break;
                    case "nl-NL":
                        text = PreparingOperatorText_nl;
                        break;
                    case "pl-PL":
                        text = PreparingOperatorText_plpl;
                        break;
                    case "cs-CZ":
                        text = PreparingOperatorText_cscz;
                        break;
                    case "pt-PT":
                        text = PreparingOperatorText_pt;
                        break;
                    case "sv-SE":
                        text = PreparingOperatorText_sv;
                        break;
                    case "zh-CN":
                        text = PreparingOperatorText_zhcn;
                        break;
                    case "zh-TW":
                        text = PreparingOperatorText_zhtw;
                        break;
                    case "ko-KR":
                        text = PreparingOperatorText_ko;
                        break;
                    default:
                        Log.Warning("XEP_ECUFIXEDFUNCTIONS.PreparingOperatorText", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = PreparingOperatorText_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    return PreparingOperatorText_engb;
                }
                return text;
            }
        }

        public string ProcessingOperatorText
        {
            get
            {
                string text;
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        text = ProcessingOperatorText_dede;
                        break;
                    case "en-GB":
                        text = ProcessingOperatorText_engb;
                        break;
                    case "en-US":
                        text = ProcessingOperatorText_enus;
                        break;
                    case "fr-FR":
                        text = ProcessingOperatorText_fr;
                        break;
                    case "es-ES":
                        text = ProcessingOperatorText_es;
                        break;
                    case "th-TH":
                        text = ProcessingOperatorText_th;
                        break;
                    case "tr-TR":
                        text = ProcessingOperatorText_tr;
                        break;
                    case "el-GR":
                        text = ProcessingOperatorText_el;
                        break;
                    case "ja-JP":
                        text = ProcessingOperatorText_ja;
                        break;
                    case "ru-RU":
                        text = ProcessingOperatorText_ru;
                        break;
                    case "it-IT":
                        text = ProcessingOperatorText_it;
                        break;
                    case "nl-NL":
                        text = ProcessingOperatorText_nl;
                        break;
                    case "pl-PL":
                        text = ProcessingOperatorText_plpl;
                        break;
                    case "cs-CZ":
                        text = ProcessingOperatorText_cscz;
                        break;
                    case "pt-PT":
                        text = ProcessingOperatorText_pt;
                        break;
                    case "sv-SE":
                        text = ProcessingOperatorText_sv;
                        break;
                    case "zh-CN":
                        text = ProcessingOperatorText_zhcn;
                        break;
                    case "zh-TW":
                        text = ProcessingOperatorText_zhtw;
                        break;
                    case "ko-KR":
                        text = ProcessingOperatorText_ko;
                        break;
                    default:
                        Log.Warning("XEP_ECUFIXEDFUNCTIONS.ProcessingOperatorText", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = ProcessingOperatorText_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    return ProcessingOperatorText_engb;
                }
                return text;
            }
        }

        public ObservableCollection<XEP_ECURESULTSEX> Results
        {
            get
            {
                ObservableCollection<XEP_ECURESULTSEX> observableCollection = new ObservableCollection<XEP_ECURESULTSEX>();
                if (Jobs != null)
                {
                    foreach (XEP_ECUJOBSEX job in Jobs)
                    {
                        foreach (XEP_ECURESULTSEX result in job.Results)
                        {
                            observableCollection.Add(result);
                        }
                    }
                }
                return observableCollection;
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
                        Log.Warning("XEP_ECUFIXEDFUNCTIONS.get_Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
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

        public virtual XEP_ECUFIXEDFUNCTIONS Clone()
        {
            return (XEP_ECUFIXEDFUNCTIONS)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_ECUFIXEDFUNCTIONS xEP_ECUFIXEDFUNCTIONS))
            {
                return false;
            }
            if (Id != xEP_ECUFIXEDFUNCTIONS.Id)
            {
                return false;
            }
            if (!(Nodeclass == xEP_ECUFIXEDFUNCTIONS.Nodeclass))
            {
                return false;
            }
            if (!(TitleId == xEP_ECUFIXEDFUNCTIONS.TitleId))
            {
                return false;
            }
            if (string.CompareOrdinal(Title_dede, xEP_ECUFIXEDFUNCTIONS.Title_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_engb, xEP_ECUFIXEDFUNCTIONS.Title_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_enus, xEP_ECUFIXEDFUNCTIONS.Title_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_fr, xEP_ECUFIXEDFUNCTIONS.Title_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_th, xEP_ECUFIXEDFUNCTIONS.Title_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_sv, xEP_ECUFIXEDFUNCTIONS.Title_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_it, xEP_ECUFIXEDFUNCTIONS.Title_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_es, xEP_ECUFIXEDFUNCTIONS.Title_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_id, xEP_ECUFIXEDFUNCTIONS.Title_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ko, xEP_ECUFIXEDFUNCTIONS.Title_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_el, xEP_ECUFIXEDFUNCTIONS.Title_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_tr, xEP_ECUFIXEDFUNCTIONS.Title_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhcn, xEP_ECUFIXEDFUNCTIONS.Title_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ru, xEP_ECUFIXEDFUNCTIONS.Title_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_nl, xEP_ECUFIXEDFUNCTIONS.Title_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_pt, xEP_ECUFIXEDFUNCTIONS.Title_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhtw, xEP_ECUFIXEDFUNCTIONS.Title_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ja, xEP_ECUFIXEDFUNCTIONS.Title_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_cscz, xEP_ECUFIXEDFUNCTIONS.Title_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_plpl, xEP_ECUFIXEDFUNCTIONS.Title_plpl) != 0)
            {
                return false;
            }
            if (!(TeleserviceRelevant == xEP_ECUFIXEDFUNCTIONS.TeleserviceRelevant))
            {
                return false;
            }
            if (!(SicherheitsRelevant == xEP_ECUFIXEDFUNCTIONS.SicherheitsRelevant))
            {
                return false;
            }
            if (ValidTo != xEP_ECUFIXEDFUNCTIONS.ValidTo)
            {
                return false;
            }
            if (ValidFrom != xEP_ECUFIXEDFUNCTIONS.ValidFrom)
            {
                return false;
            }
            if (!(FastaRelevant == xEP_ECUFIXEDFUNCTIONS.FastaRelevant))
            {
                return false;
            }
            if (!(SteuergeraeteFunktionenRelevan == xEP_ECUFIXEDFUNCTIONS.SteuergeraeteFunktionenRelevan))
            {
                return false;
            }
            if (!(FahrzeugTestRelevant == xEP_ECUFIXEDFUNCTIONS.FahrzeugTestRelevant))
            {
                return false;
            }
            if (string.CompareOrdinal(FastaDataType, xEP_ECUFIXEDFUNCTIONS.FastaDataType) != 0)
            {
                return false;
            }
            if (!(FdmRelevant == xEP_ECUFIXEDFUNCTIONS.FdmRelevant))
            {
                return false;
            }
            if (!(PreparingOperatorTextId == xEP_ECUFIXEDFUNCTIONS.PreparingOperatorTextId))
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_dede, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_engb, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_enus, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_fr, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_th, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_sv, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_it, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_es, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_id, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_ko, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_el, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_tr, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_zhcn, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_ru, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_nl, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_pt, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_zhtw, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_ja, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_cscz, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PreparingOperatorText_plpl, xEP_ECUFIXEDFUNCTIONS.PreparingOperatorText_plpl) != 0)
            {
                return false;
            }
            if (!(PostOperatorTextId == xEP_ECUFIXEDFUNCTIONS.PostOperatorTextId))
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_dede, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_engb, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_enus, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_fr, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_th, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_sv, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_it, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_es, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_id, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_ko, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_el, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_tr, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_zhcn, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_ru, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_nl, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_pt, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_zhtw, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_ja, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_cscz, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(PostOperatorText_plpl, xEP_ECUFIXEDFUNCTIONS.PostOperatorText_plpl) != 0)
            {
                return false;
            }
            if (!(ParentId == xEP_ECUFIXEDFUNCTIONS.ParentId))
            {
                return false;
            }
            if (!(SortOrder == xEP_ECUFIXEDFUNCTIONS.SortOrder))
            {
                return false;
            }
            if (Activation != xEP_ECUFIXEDFUNCTIONS.Activation)
            {
                return false;
            }
            if (ActivationDurationMs != xEP_ECUFIXEDFUNCTIONS.ActivationDurationMs)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public ObservableCollection<XEP_ECUJOBSEX> GetJobsByPhase(string phase)
        {
            ObservableCollection<XEP_ECUJOBSEX> observableCollection = new ObservableCollection<XEP_ECUJOBSEX>();
            if (Jobs != null)
            {
                foreach (XEP_ECUJOBSEX job in Jobs)
                {
                    if (string.Compare(job.Phase, phase, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        observableCollection.Add(job);
                    }
                }
            }
            return observableCollection;
        }

        public string GetLocalizedPostOperatorText(string language)
        {
            string text;
            switch (MatchLanguageToCulture(language))
            {
                case "de-DE":
                    text = PostOperatorText_dede;
                    break;
                case "en-GB":
                    text = PostOperatorText_engb;
                    break;
                case "en-US":
                    text = PostOperatorText_enus;
                    break;
                case "fr-FR":
                    text = PostOperatorText_fr;
                    break;
                case "es-ES":
                    text = PostOperatorText_es;
                    break;
                case "th-TH":
                    text = PostOperatorText_th;
                    break;
                case "tr-TR":
                    text = PostOperatorText_tr;
                    break;
                case "el-GR":
                    text = PostOperatorText_el;
                    break;
                case "ja-JP":
                    text = PostOperatorText_ja;
                    break;
                case "ru-RU":
                    text = PostOperatorText_ru;
                    break;
                case "it-IT":
                    text = PostOperatorText_it;
                    break;
                case "nl-NL":
                    text = PostOperatorText_nl;
                    break;
                case "pl-PL":
                    text = PostOperatorText_plpl;
                    break;
                case "cs-CZ":
                    text = PostOperatorText_cscz;
                    break;
                case "pt-PT":
                    text = PostOperatorText_pt;
                    break;
                case "sv-SE":
                    text = PostOperatorText_sv;
                    break;
                case "zh-CN":
                    text = PostOperatorText_zhcn;
                    break;
                case "zh-TW":
                    text = PostOperatorText_zhtw;
                    break;
                case "ko-KR":
                    text = PostOperatorText_ko;
                    break;
                default:
                    Log.Warning("XEP_ECUFIXEDFUNCTIONS.GetLocalizedPostOperatorText", "given Language {0} not available - language set to enGB", language);
                    text = PostOperatorText_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                return PostOperatorText_engb;
            }
            return text;
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
                    Log.Warning("XEP_ECUFIXEDFUNCTIONS.GetLocalizedTitle", "language {0} not available - language set to enGB", language);
                    text = Title_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                return Title_engb;
            }
            return text;
        }

        public string GetLocalizedPreparingOperatorText(string language)
        {
            string text;
            switch (MatchLanguageToCulture(language))
            {
                case "de-DE":
                    text = PreparingOperatorText_dede;
                    break;
                case "en-GB":
                    text = PreparingOperatorText_engb;
                    break;
                case "en-US":
                    text = PreparingOperatorText_enus;
                    break;
                case "fr-FR":
                    text = PreparingOperatorText_fr;
                    break;
                case "es-ES":
                    text = PreparingOperatorText_es;
                    break;
                case "th-TH":
                    text = PreparingOperatorText_th;
                    break;
                case "tr-TR":
                    text = PreparingOperatorText_tr;
                    break;
                case "el-GR":
                    text = PreparingOperatorText_el;
                    break;
                case "ja-JP":
                    text = PreparingOperatorText_ja;
                    break;
                case "ru-RU":
                    text = PreparingOperatorText_ru;
                    break;
                case "it-IT":
                    text = PreparingOperatorText_it;
                    break;
                case "nl-NL":
                    text = PreparingOperatorText_nl;
                    break;
                case "pl-PL":
                    text = PreparingOperatorText_plpl;
                    break;
                case "cs-CZ":
                    text = PreparingOperatorText_cscz;
                    break;
                case "pt-PT":
                    text = PreparingOperatorText_pt;
                    break;
                case "sv-SE":
                    text = PreparingOperatorText_sv;
                    break;
                case "zh-CN":
                    text = PreparingOperatorText_zhcn;
                    break;
                case "zh-TW":
                    text = PreparingOperatorText_zhtw;
                    break;
                case "ko-KR":
                    text = PreparingOperatorText_ko;
                    break;
                default:
                    Log.Warning("XEP_ECUFIXEDFUNCTIONS.GetLocalizedPreparingOperatorText", "the given Language {0} is not available - language set to enGB", language);
                    text = PreparingOperatorText_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                return PreparingOperatorText_engb;
            }
            return text;
        }

        public string GetLocalizedProcessingOperatorText(string language)
        {
            language = ((language.Length > 2) ? language.Remove(2, 3) : language);
            string text;
            switch (MatchLanguageToCulture(language))
            {
                case "de-DE":
                    text = ProcessingOperatorText_dede;
                    break;
                case "en-GB":
                    text = ProcessingOperatorText_engb;
                    break;
                case "en-US":
                    text = ProcessingOperatorText_enus;
                    break;
                case "fr-FR":
                    text = ProcessingOperatorText_fr;
                    break;
                case "es-ES":
                    text = ProcessingOperatorText_es;
                    break;
                case "th-TH":
                    text = ProcessingOperatorText_th;
                    break;
                case "tr-TR":
                    text = ProcessingOperatorText_tr;
                    break;
                case "el-GR":
                    text = ProcessingOperatorText_el;
                    break;
                case "ja-JP":
                    text = ProcessingOperatorText_ja;
                    break;
                case "ru-RU":
                    text = ProcessingOperatorText_ru;
                    break;
                case "it-IT":
                    text = ProcessingOperatorText_it;
                    break;
                case "nl-NL":
                    text = ProcessingOperatorText_nl;
                    break;
                case "pl-PL":
                    text = ProcessingOperatorText_plpl;
                    break;
                case "cs-CZ":
                    text = ProcessingOperatorText_cscz;
                    break;
                case "pt-PT":
                    text = ProcessingOperatorText_pt;
                    break;
                case "sv-SE":
                    text = ProcessingOperatorText_sv;
                    break;
                case "zh-CN":
                    text = ProcessingOperatorText_zhcn;
                    break;
                case "zh-TW":
                    text = ProcessingOperatorText_zhtw;
                    break;
                case "ko-KR":
                    text = ProcessingOperatorText_ko;
                    break;
                default:
                    Log.Warning("XEP_ECUFIXEDFUNCTIONS.GetLocalizedProcessingOperatorText", "The given Language {0} is not available - language set to enGB", language);
                    text = ProcessingOperatorText_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                return ProcessingOperatorText_engb;
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
