using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using PsdzClient.Core;
using System;
using System.ComponentModel;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XepInfoObject : IXepInfoObject, IXepInfoObjectRuleEvaluation, IMultilanguageTitle
    {
        private decimal? abgasrelevantField;

        private decimal? assemblyField;

        private string awNummerField;

        private DateTime? changeDateField;

        private decimal? controlIdField;

        private DateTime? createDateField;

        private decimal? debugInfoField;

        private string docNumberField;

        private string documentTypeField;

        private string dringlichkeitField;

        private DateTime? expiryDateField;

        private decimal? fahrzeugKommunikationField;

        private decimal? generellField;

        private string grobzeichenField;

        private decimal? hinweisIdField;

        private string hinweis_csczField;

        private string hinweis_dedeField;

        private string hinweis_elField;

        private string hinweis_engbField;

        private string hinweis_enusField;

        private string hinweis_esField;

        private string hinweis_frField;

        private string hinweis_idField;

        private string hinweis_itField;

        private string hinweis_jaField;

        private string hinweis_koField;

        private string hinweis_nlField;

        private string hinweis_plplField;

        private string hinweis_ptField;

        private string hinweis_ruField;

        private string hinweis_svField;

        private string hinweis_thField;

        private string hinweis_trField;

        private string hinweis_zhcnField;

        private string hinweis_zhtwField;

        private decimal idField;

        private string identifierField;

        private string identifikatorField;

        private string infoFormatField;

        private string infoTypeField;

        private string informationsTypField;

        private string informationsformatField;

        private bool isSuspiciousField;

        private DateTime? launchDateField;

        private decimal? messtechnikField;

        private DateTime? modificationTimeField;

        private string nameField;

        private decimal? nodeclassField;

        private decimal? priorityField;

        private string programTypeField;

        private string siNummerField;

        private decimal? sicherheitsRelevantField;

        private string swzNummerField;

        private decimal? teleserviceKennungField;

        private decimal? titleIdField;

        private string title_csczField;

        private string title_dedeField;

        private string title_elField;

        private string title_engbField;

        private string title_enusField;

        private string title_esField;

        private string title_frField;

        private string title_idField;

        private string title_itField;

        private string title_jaField;

        private string title_koField;

        private string title_nlField;

        private string title_plplField;

        private string title_ptField;

        private string title_ruField;

        private string title_svField;

        private string title_thField;

        private string title_trField;

        private string title_zhcnField;

        private string title_zhtwField;

        private string usedDeviceAdaptersField;

        private DateTime? validFromField;

        private DateTime? validToField;

        private decimal? versionNumberField;

        private decimal? verstecktField;

        private string zielIStufeField;

        private decimal? safetyRelatedInfoField;

        private bool fastNavigation;

        public bool IsOriginatedFromParentNode { get; set; }

        public bool IsNews { get; set; }

        public bool IsRgNews { get; set; }

        public string ExplicitTitle { get; set; }

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

        public decimal? ControlId
        {
            get
            {
                return controlIdField;
            }
            set
            {
                if (controlIdField.HasValue)
                {
                    if (!controlIdField.Equals(value))
                    {
                        controlIdField = value;
                        OnPropertyChanged("ControlId");
                    }
                }
                else
                {
                    controlIdField = value;
                    OnPropertyChanged("ControlId");
                }
            }
        }

        public string DocumentType
        {
            get
            {
                return documentTypeField;
            }
            set
            {
                if (documentTypeField != null)
                {
                    if (!documentTypeField.Equals(value))
                    {
                        documentTypeField = value;
                        OnPropertyChanged("DocumentType");
                    }
                }
                else
                {
                    documentTypeField = value;
                    OnPropertyChanged("DocumentType");
                }
            }
        }

        public string Identifikator
        {
            get
            {
                return identifikatorField;
            }
            set
            {
                if (identifikatorField != null)
                {
                    if (!identifikatorField.Equals(value))
                    {
                        identifikatorField = value;
                        OnPropertyChanged("Identifikator");
                    }
                }
                else
                {
                    identifikatorField = value;
                    OnPropertyChanged("Identifikator");
                }
            }
        }

        public string InfoType
        {
            get
            {
                return infoTypeField;
            }
            set
            {
                if (infoTypeField != null)
                {
                    if (!infoTypeField.Equals(value))
                    {
                        infoTypeField = value;
                        OnPropertyChanged("InfoType");
                    }
                }
                else
                {
                    infoTypeField = value;
                    OnPropertyChanged("InfoType");
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

        public decimal? Assembly
        {
            get
            {
                return assemblyField;
            }
            set
            {
                if (assemblyField.HasValue)
                {
                    if (!assemblyField.Equals(value))
                    {
                        assemblyField = value;
                        OnPropertyChanged("Assembly");
                    }
                }
                else
                {
                    assemblyField = value;
                    OnPropertyChanged("Assembly");
                }
            }
        }

        public decimal? DebugInfo
        {
            get
            {
                return debugInfoField;
            }
            set
            {
                if (debugInfoField.HasValue)
                {
                    if (!debugInfoField.Equals(value))
                    {
                        debugInfoField = value;
                        OnPropertyChanged("DebugInfo");
                    }
                }
                else
                {
                    debugInfoField = value;
                    OnPropertyChanged("DebugInfo");
                }
            }
        }

        public string UsedDeviceAdapters
        {
            get
            {
                return usedDeviceAdaptersField;
            }
            set
            {
                if (usedDeviceAdaptersField != null)
                {
                    if (!usedDeviceAdaptersField.Equals(value))
                    {
                        usedDeviceAdaptersField = value;
                        OnPropertyChanged("UsedDeviceAdapters");
                    }
                }
                else
                {
                    usedDeviceAdaptersField = value;
                    OnPropertyChanged("UsedDeviceAdapters");
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
                if (versionNumberField.HasValue)
                {
                    if (!versionNumberField.Equals(value))
                    {
                        versionNumberField = value;
                        OnPropertyChanged("VersionNumber");
                    }
                }
                else
                {
                    versionNumberField = value;
                    OnPropertyChanged("VersionNumber");
                }
            }
        }

        public string ProgramType
        {
            get
            {
                return programTypeField;
            }
            set
            {
                if (programTypeField != null)
                {
                    if (!programTypeField.Equals(value))
                    {
                        programTypeField = value;
                        OnPropertyChanged("ProgramType");
                    }
                }
                else
                {
                    programTypeField = value;
                    OnPropertyChanged("ProgramType");
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

        public decimal? SicherheitsRelevant
        {
            get
            {
                return sicherheitsRelevantField;
            }
            set
            {
                if (sicherheitsRelevantField.HasValue)
                {
                    if (!sicherheitsRelevantField.Equals(value))
                    {
                        sicherheitsRelevantField = value;
                        OnPropertyChanged("SicherheitsRelevant");
                    }
                }
                else
                {
                    sicherheitsRelevantField = value;
                    OnPropertyChanged("SicherheitsRelevant");
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

        public string Title
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ExplicitTitle))
                {
                    return ExplicitTitle;
                }
                string text;
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        text = ((IMultilanguageTitle)this).Title_dede;
                        break;
                    case "en-GB":
                        text = ((IMultilanguageTitle)this).Title_engb;
                        break;
                    case "en-US":
                        text = ((IMultilanguageTitle)this).Title_enus;
                        break;
                    case "fr-FR":
                        text = ((IMultilanguageTitle)this).Title_fr;
                        break;
                    case "es-ES":
                        text = ((IMultilanguageTitle)this).Title_es;
                        break;
                    case "th-TH":
                        text = ((IMultilanguageTitle)this).Title_th;
                        break;
                    case "tr-TR":
                        text = ((IMultilanguageTitle)this).Title_tr;
                        break;
                    case "el-GR":
                        text = ((IMultilanguageTitle)this).Title_el;
                        break;
                    case "ja-JP":
                        text = ((IMultilanguageTitle)this).Title_ja;
                        break;
                    case "ru-RU":
                        text = ((IMultilanguageTitle)this).Title_ru;
                        break;
                    case "it-IT":
                        text = ((IMultilanguageTitle)this).Title_it;
                        break;
                    case "nl-NL":
                        text = ((IMultilanguageTitle)this).Title_nl;
                        break;
                    case "pl-PL":
                        text = ((IMultilanguageTitle)this).Title_plpl;
                        break;
                    case "cs-CZ":
                        text = ((IMultilanguageTitle)this).Title_cscz;
                        break;
                    case "pt-PT":
                        text = ((IMultilanguageTitle)this).Title_pt;
                        break;
                    case "sv-SE":
                        text = ((IMultilanguageTitle)this).Title_sv;
                        break;
                    case "zh-CN":
                        text = ((IMultilanguageTitle)this).Title_zhcn;
                        break;
                    case "zh-TW":
                        text = ((IMultilanguageTitle)this).Title_zhtw;
                        break;
                    case "ko-KR":
                        text = ((IMultilanguageTitle)this).Title_ko;
                        break;
                    default:
                        Log.Warning("XepInfoObject.get_Title", "CurrentUICulture \"{0}\" not available - return title for en-GB.", ConfigSettings.CurrentUICulture);
                        text = ((IMultilanguageTitle)this).Title_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    return ((IMultilanguageTitle)this).Title_engb;
                }
                return text;
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

        public decimal? Generell
        {
            get
            {
                return generellField;
            }
            set
            {
                if (generellField.HasValue)
                {
                    if (!generellField.Equals(value))
                    {
                        generellField = value;
                        OnPropertyChanged("Generell");
                    }
                }
                else
                {
                    generellField = value;
                    OnPropertyChanged("Generell");
                }
            }
        }

        public decimal? TeleserviceKennung
        {
            get
            {
                return teleserviceKennungField;
            }
            set
            {
                if (teleserviceKennungField.HasValue)
                {
                    if (!teleserviceKennungField.Equals(value))
                    {
                        teleserviceKennungField = value;
                        OnPropertyChanged("TeleserviceKennung");
                    }
                }
                else
                {
                    teleserviceKennungField = value;
                    OnPropertyChanged("TeleserviceKennung");
                }
            }
        }

        public decimal? FahrzeugKommunikation
        {
            get
            {
                return fahrzeugKommunikationField;
            }
            set
            {
                if (fahrzeugKommunikationField.HasValue)
                {
                    if (!fahrzeugKommunikationField.Equals(value))
                    {
                        fahrzeugKommunikationField = value;
                        OnPropertyChanged("FahrzeugKommunikation");
                    }
                }
                else
                {
                    fahrzeugKommunikationField = value;
                    OnPropertyChanged("FahrzeugKommunikation");
                }
            }
        }

        public decimal? Messtechnik
        {
            get
            {
                return messtechnikField;
            }
            set
            {
                if (messtechnikField.HasValue)
                {
                    if (!messtechnikField.Equals(value))
                    {
                        messtechnikField = value;
                        OnPropertyChanged("Messtechnik");
                    }
                }
                else
                {
                    messtechnikField = value;
                    OnPropertyChanged("Messtechnik");
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
                if (verstecktField.HasValue)
                {
                    if (!verstecktField.Equals(value))
                    {
                        verstecktField = value;
                        OnPropertyChanged("Versteckt");
                    }
                }
                else
                {
                    verstecktField = value;
                    OnPropertyChanged("Versteckt");
                }
            }
        }

        public decimal? HinweisId
        {
            get
            {
                return hinweisIdField;
            }
            set
            {
                if (hinweisIdField.HasValue)
                {
                    if (!hinweisIdField.Equals(value))
                    {
                        hinweisIdField = value;
                        OnPropertyChanged("HinweisId");
                    }
                }
                else
                {
                    hinweisIdField = value;
                    OnPropertyChanged("HinweisId");
                }
            }
        }

        public string Hinweis_dede
        {
            get
            {
                return hinweis_dedeField;
            }
            set
            {
                if (hinweis_dedeField != null)
                {
                    if (!hinweis_dedeField.Equals(value))
                    {
                        hinweis_dedeField = value;
                        OnPropertyChanged("Hinweis_dede");
                    }
                }
                else
                {
                    hinweis_dedeField = value;
                    OnPropertyChanged("Hinweis_dede");
                }
            }
        }

        public string Hinweis_engb
        {
            get
            {
                return hinweis_engbField;
            }
            set
            {
                if (hinweis_engbField != null)
                {
                    if (!hinweis_engbField.Equals(value))
                    {
                        hinweis_engbField = value;
                        OnPropertyChanged("Hinweis_engb");
                    }
                }
                else
                {
                    hinweis_engbField = value;
                    OnPropertyChanged("Hinweis_engb");
                }
            }
        }

        public string Hinweis_enus
        {
            get
            {
                return hinweis_enusField;
            }
            set
            {
                if (hinweis_enusField != null)
                {
                    if (!hinweis_enusField.Equals(value))
                    {
                        hinweis_enusField = value;
                        OnPropertyChanged("Hinweis_enus");
                    }
                }
                else
                {
                    hinweis_enusField = value;
                    OnPropertyChanged("Hinweis_enus");
                }
            }
        }

        public string Hinweis_fr
        {
            get
            {
                return hinweis_frField;
            }
            set
            {
                if (hinweis_frField != null)
                {
                    if (!hinweis_frField.Equals(value))
                    {
                        hinweis_frField = value;
                        OnPropertyChanged("Hinweis_fr");
                    }
                }
                else
                {
                    hinweis_frField = value;
                    OnPropertyChanged("Hinweis_fr");
                }
            }
        }

        public string Hinweis_th
        {
            get
            {
                return hinweis_thField;
            }
            set
            {
                if (hinweis_thField != null)
                {
                    if (!hinweis_thField.Equals(value))
                    {
                        hinweis_thField = value;
                        OnPropertyChanged("Hinweis_th");
                    }
                }
                else
                {
                    hinweis_thField = value;
                    OnPropertyChanged("Hinweis_th");
                }
            }
        }

        public string Hinweis_sv
        {
            get
            {
                return hinweis_svField;
            }
            set
            {
                if (hinweis_svField != null)
                {
                    if (!hinweis_svField.Equals(value))
                    {
                        hinweis_svField = value;
                        OnPropertyChanged("Hinweis_sv");
                    }
                }
                else
                {
                    hinweis_svField = value;
                    OnPropertyChanged("Hinweis_sv");
                }
            }
        }

        public string Hinweis_it
        {
            get
            {
                return hinweis_itField;
            }
            set
            {
                if (hinweis_itField != null)
                {
                    if (!hinweis_itField.Equals(value))
                    {
                        hinweis_itField = value;
                        OnPropertyChanged("Hinweis_it");
                    }
                }
                else
                {
                    hinweis_itField = value;
                    OnPropertyChanged("Hinweis_it");
                }
            }
        }

        public string Hinweis_es
        {
            get
            {
                return hinweis_esField;
            }
            set
            {
                if (hinweis_esField != null)
                {
                    if (!hinweis_esField.Equals(value))
                    {
                        hinweis_esField = value;
                        OnPropertyChanged("Hinweis_es");
                    }
                }
                else
                {
                    hinweis_esField = value;
                    OnPropertyChanged("Hinweis_es");
                }
            }
        }

        public string Hinweis_id
        {
            get
            {
                return hinweis_idField;
            }
            set
            {
                if (hinweis_idField != null)
                {
                    if (!hinweis_idField.Equals(value))
                    {
                        hinweis_idField = value;
                        OnPropertyChanged("Hinweis_id");
                    }
                }
                else
                {
                    hinweis_idField = value;
                    OnPropertyChanged("Hinweis_id");
                }
            }
        }

        public string Hinweis_ko
        {
            get
            {
                return hinweis_koField;
            }
            set
            {
                if (hinweis_koField != null)
                {
                    if (!hinweis_koField.Equals(value))
                    {
                        hinweis_koField = value;
                        OnPropertyChanged("Hinweis_ko");
                    }
                }
                else
                {
                    hinweis_koField = value;
                    OnPropertyChanged("Hinweis_ko");
                }
            }
        }

        public string Hinweis_el
        {
            get
            {
                return hinweis_elField;
            }
            set
            {
                if (hinweis_elField != null)
                {
                    if (!hinweis_elField.Equals(value))
                    {
                        hinweis_elField = value;
                        OnPropertyChanged("Hinweis_el");
                    }
                }
                else
                {
                    hinweis_elField = value;
                    OnPropertyChanged("Hinweis_el");
                }
            }
        }

        public string Hinweis_tr
        {
            get
            {
                return hinweis_trField;
            }
            set
            {
                if (hinweis_trField != null)
                {
                    if (!hinweis_trField.Equals(value))
                    {
                        hinweis_trField = value;
                        OnPropertyChanged("Hinweis_tr");
                    }
                }
                else
                {
                    hinweis_trField = value;
                    OnPropertyChanged("Hinweis_tr");
                }
            }
        }

        public string Hinweis_zhcn
        {
            get
            {
                return hinweis_zhcnField;
            }
            set
            {
                if (hinweis_zhcnField != null)
                {
                    if (!hinweis_zhcnField.Equals(value))
                    {
                        hinweis_zhcnField = value;
                        OnPropertyChanged("Hinweis_zhcn");
                    }
                }
                else
                {
                    hinweis_zhcnField = value;
                    OnPropertyChanged("Hinweis_zhcn");
                }
            }
        }

        public string Hinweis_ru
        {
            get
            {
                return hinweis_ruField;
            }
            set
            {
                if (hinweis_ruField != null)
                {
                    if (!hinweis_ruField.Equals(value))
                    {
                        hinweis_ruField = value;
                        OnPropertyChanged("Hinweis_ru");
                    }
                }
                else
                {
                    hinweis_ruField = value;
                    OnPropertyChanged("Hinweis_ru");
                }
            }
        }

        public string Hinweis_nl
        {
            get
            {
                return hinweis_nlField;
            }
            set
            {
                if (hinweis_nlField != null)
                {
                    if (!hinweis_nlField.Equals(value))
                    {
                        hinweis_nlField = value;
                        OnPropertyChanged("Hinweis_nl");
                    }
                }
                else
                {
                    hinweis_nlField = value;
                    OnPropertyChanged("Hinweis_nl");
                }
            }
        }

        public string Hinweis_pt
        {
            get
            {
                return hinweis_ptField;
            }
            set
            {
                if (hinweis_ptField != null)
                {
                    if (!hinweis_ptField.Equals(value))
                    {
                        hinweis_ptField = value;
                        OnPropertyChanged("Hinweis_pt");
                    }
                }
                else
                {
                    hinweis_ptField = value;
                    OnPropertyChanged("Hinweis_pt");
                }
            }
        }

        public string Hinweis_zhtw
        {
            get
            {
                return hinweis_zhtwField;
            }
            set
            {
                if (hinweis_zhtwField != null)
                {
                    if (!hinweis_zhtwField.Equals(value))
                    {
                        hinweis_zhtwField = value;
                        OnPropertyChanged("Hinweis_zhtw");
                    }
                }
                else
                {
                    hinweis_zhtwField = value;
                    OnPropertyChanged("Hinweis_zhtw");
                }
            }
        }

        public string Hinweis_ja
        {
            get
            {
                return hinweis_jaField;
            }
            set
            {
                if (hinweis_jaField != null)
                {
                    if (!hinweis_jaField.Equals(value))
                    {
                        hinweis_jaField = value;
                        OnPropertyChanged("Hinweis_ja");
                    }
                }
                else
                {
                    hinweis_jaField = value;
                    OnPropertyChanged("Hinweis_ja");
                }
            }
        }

        public string Hinweis_plpl
        {
            get
            {
                return hinweis_plplField;
            }
            set
            {
                if (hinweis_plplField != null)
                {
                    if (!hinweis_plplField.Equals(value))
                    {
                        hinweis_plplField = value;
                        OnPropertyChanged("Hinweis_plpl");
                    }
                }
                else
                {
                    hinweis_plplField = value;
                    OnPropertyChanged("Hinweis_plpl");
                }
            }
        }

        public string Hinweis_cscz
        {
            get
            {
                return hinweis_csczField;
            }
            set
            {
                if (hinweis_csczField != null)
                {
                    if (!hinweis_csczField.Equals(value))
                    {
                        hinweis_csczField = value;
                        OnPropertyChanged("Hinweis_cscz");
                    }
                }
                else
                {
                    hinweis_csczField = value;
                    OnPropertyChanged("Hinweis_cscz");
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

        public string InformationsTyp
        {
            get
            {
                return informationsTypField;
            }
            set
            {
                if (informationsTypField != null)
                {
                    if (!informationsTypField.Equals(value))
                    {
                        informationsTypField = value;
                        OnPropertyChanged("InformationsTyp");
                    }
                }
                else
                {
                    informationsTypField = value;
                    OnPropertyChanged("InformationsTyp");
                }
            }
        }

        public DateTime? CreateDate
        {
            get
            {
                return createDateField;
            }
            set
            {
                if (createDateField.HasValue)
                {
                    if (!createDateField.Equals(value))
                    {
                        createDateField = value;
                        OnPropertyChanged("CreateDate");
                    }
                }
                else
                {
                    createDateField = value;
                    OnPropertyChanged("CreateDate");
                }
            }
        }

        public DateTime? ExpiryDate
        {
            get
            {
                return expiryDateField;
            }
            set
            {
                if (expiryDateField.HasValue)
                {
                    if (!expiryDateField.Equals(value))
                    {
                        expiryDateField = value;
                        OnPropertyChanged("ExpiryDate");
                    }
                }
                else
                {
                    expiryDateField = value;
                    OnPropertyChanged("ExpiryDate");
                }
            }
        }

        public DateTime? ChangeDate
        {
            get
            {
                return changeDateField;
            }
            set
            {
                if (changeDateField.HasValue)
                {
                    if (!changeDateField.Equals(value))
                    {
                        changeDateField = value;
                        OnPropertyChanged("ChangeDate");
                    }
                }
                else
                {
                    changeDateField = value;
                    OnPropertyChanged("ChangeDate");
                }
            }
        }

        public DateTime? LaunchDate
        {
            get
            {
                return launchDateField;
            }
            set
            {
                if (launchDateField.HasValue)
                {
                    if (!launchDateField.Equals(value))
                    {
                        launchDateField = value;
                        OnPropertyChanged("LaunchDate");
                    }
                }
                else
                {
                    launchDateField = value;
                    OnPropertyChanged("LaunchDate");
                }
            }
        }

        public decimal? Abgasrelevant
        {
            get
            {
                return abgasrelevantField;
            }
            set
            {
                if (abgasrelevantField.HasValue)
                {
                    if (!abgasrelevantField.Equals(value))
                    {
                        abgasrelevantField = value;
                        OnPropertyChanged("Abgasrelevant");
                    }
                }
                else
                {
                    abgasrelevantField = value;
                    OnPropertyChanged("Abgasrelevant");
                }
            }
        }

        public string Dringlichkeit
        {
            get
            {
                return dringlichkeitField;
            }
            set
            {
                if (dringlichkeitField != null)
                {
                    if (!dringlichkeitField.Equals(value))
                    {
                        dringlichkeitField = value;
                        OnPropertyChanged("Dringlichkeit");
                    }
                }
                else
                {
                    dringlichkeitField = value;
                    OnPropertyChanged("Dringlichkeit");
                }
            }
        }

        public string Informationsformat
        {
            get
            {
                return informationsformatField;
            }
            set
            {
                if (informationsformatField != null)
                {
                    if (!informationsformatField.Equals(value))
                    {
                        informationsformatField = value;
                        OnPropertyChanged("Informationsformat");
                    }
                }
                else
                {
                    informationsformatField = value;
                    OnPropertyChanged("Informationsformat");
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
                if (grobzeichenField != null)
                {
                    if (!grobzeichenField.Equals(value))
                    {
                        grobzeichenField = value;
                        OnPropertyChanged("Grobzeichen");
                    }
                }
                else
                {
                    grobzeichenField = value;
                    OnPropertyChanged("Grobzeichen");
                }
            }
        }

        public string AwNummer
        {
            get
            {
                return awNummerField;
            }
            set
            {
                if (awNummerField != null)
                {
                    if (!awNummerField.Equals(value))
                    {
                        awNummerField = value;
                        OnPropertyChanged("AwNummer");
                    }
                }
                else
                {
                    awNummerField = value;
                    OnPropertyChanged("AwNummer");
                }
            }
        }

        public string SwzNummer
        {
            get
            {
                return swzNummerField;
            }
            set
            {
                if (swzNummerField != null)
                {
                    if (!swzNummerField.Equals(value))
                    {
                        swzNummerField = value;
                        OnPropertyChanged("SwzNummer");
                    }
                }
                else
                {
                    swzNummerField = value;
                    OnPropertyChanged("SwzNummer");
                }
            }
        }

        public string SiNummer
        {
            get
            {
                return siNummerField;
            }
            set
            {
                if (siNummerField != null)
                {
                    if (!siNummerField.Equals(value))
                    {
                        siNummerField = value;
                        OnPropertyChanged("SiNummer");
                    }
                }
                else
                {
                    siNummerField = value;
                    OnPropertyChanged("SiNummer");
                }
            }
        }

        public string ZielIStufe
        {
            get
            {
                return zielIStufeField;
            }
            set
            {
                if (zielIStufeField != null)
                {
                    if (!zielIStufeField.Equals(value))
                    {
                        zielIStufeField = value;
                        OnPropertyChanged("ZielIStufe");
                    }
                }
                else
                {
                    zielIStufeField = value;
                    OnPropertyChanged("ZielIStufe");
                }
            }
        }

        public DateTime? ModificationTime
        {
            get
            {
                return modificationTimeField;
            }
            set
            {
                if (modificationTimeField.HasValue)
                {
                    if (!modificationTimeField.Equals(value))
                    {
                        modificationTimeField = value;
                        OnPropertyChanged("ModificationTime");
                    }
                }
                else
                {
                    modificationTimeField = value;
                    OnPropertyChanged("ModificationTime");
                }
            }
        }

        public string InfoFormat
        {
            get
            {
                return infoFormatField;
            }
            set
            {
                if (infoFormatField != null)
                {
                    if (!infoFormatField.Equals(value))
                    {
                        infoFormatField = value;
                        OnPropertyChanged("InfoFormat");
                    }
                }
                else
                {
                    infoFormatField = value;
                    OnPropertyChanged("InfoFormat");
                }
            }
        }

        public string DocNumber
        {
            get
            {
                return docNumberField;
            }
            set
            {
                if (docNumberField != null)
                {
                    if (!docNumberField.Equals(value))
                    {
                        docNumberField = value;
                        OnPropertyChanged("DocNumber");
                    }
                }
                else
                {
                    docNumberField = value;
                    OnPropertyChanged("DocNumber");
                }
            }
        }

        public decimal? Priority
        {
            get
            {
                return priorityField;
            }
            set
            {
                if (priorityField.HasValue)
                {
                    if (!priorityField.Equals(value))
                    {
                        priorityField = value;
                        OnPropertyChanged("Priority");
                    }
                }
                else
                {
                    priorityField = value;
                    OnPropertyChanged("Priority");
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
                if (identifierField != null)
                {
                    if (!identifierField.Equals(value))
                    {
                        identifierField = value;
                        OnPropertyChanged("Identifier");
                    }
                }
                else
                {
                    identifierField = value;
                    OnPropertyChanged("Identifier");
                }
            }
        }

        public decimal? SafetyRelatedInfo
        {
            get
            {
                return safetyRelatedInfoField;
            }
            set
            {
                if (safetyRelatedInfoField.HasValue)
                {
                    if (!safetyRelatedInfoField.Equals(value))
                    {
                        safetyRelatedInfoField = value;
                        OnPropertyChanged("SafetyRelatedInfo");
                    }
                }
                else
                {
                    safetyRelatedInfoField = value;
                    OnPropertyChanged("SafetyRelatedInfo");
                }
            }
        }

        public bool IsSuspicious
        {
            get
            {
                return isSuspiciousField;
            }
            set
            {
                if (!isSuspiciousField.Equals(value))
                {
                    isSuspiciousField = value;
                    OnPropertyChanged("IsSuspicious");
                }
            }
        }

        public bool FastNavigation
        {
            get
            {
                return fastNavigation;
            }
            set
            {
                if (!fastNavigation.Equals(value))
                {
                    fastNavigation = value;
                    OnPropertyChanged("FastNavigation");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public XepInfoObject()
        {
            isSuspiciousField = false;
            fastNavigation = true;
        }

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
            if (!(obj is IXepInfoObject xepInfoObject))
            {
                return false;
            }
            if (Id != xepInfoObject.Id)
            {
                return false;
            }
            if (!(Nodeclass == xepInfoObject.Nodeclass))
            {
                return false;
            }
            if (!(Assembly == xepInfoObject.Assembly))
            {
                return false;
            }
            if (!(DebugInfo == xepInfoObject.DebugInfo))
            {
                return false;
            }
            if (string.CompareOrdinal(UsedDeviceAdapters, xepInfoObject.UsedDeviceAdapters) != 0)
            {
                return false;
            }
            if (!(VersionNumber == xepInfoObject.VersionNumber))
            {
                return false;
            }
            if (string.CompareOrdinal(ProgramType, xepInfoObject.ProgramType) != 0)
            {
                return false;
            }
            if (ValidFrom != xepInfoObject.ValidFrom)
            {
                return false;
            }
            if (ValidTo != xepInfoObject.ValidTo)
            {
                return false;
            }
            if (!(SicherheitsRelevant == xepInfoObject.SicherheitsRelevant))
            {
                return false;
            }
            if (!(TitleId == xepInfoObject.TitleId))
            {
                return false;
            }
            if (string.CompareOrdinal(Title_dede, xepInfoObject.Title_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_engb, xepInfoObject.Title_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_enus, xepInfoObject.Title_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_fr, xepInfoObject.Title_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_th, xepInfoObject.Title_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_sv, xepInfoObject.Title_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_it, xepInfoObject.Title_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_es, xepInfoObject.Title_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_id, xepInfoObject.Title_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ko, xepInfoObject.Title_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_el, xepInfoObject.Title_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_tr, xepInfoObject.Title_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhcn, xepInfoObject.Title_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ru, xepInfoObject.Title_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_nl, xepInfoObject.Title_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_pt, xepInfoObject.Title_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_zhtw, xepInfoObject.Title_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_ja, xepInfoObject.Title_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_cscz, xepInfoObject.Title_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Title_plpl, xepInfoObject.Title_plpl) != 0)
            {
                return false;
            }
            if (!(Generell == xepInfoObject.Generell))
            {
                return false;
            }
            if (!(TeleserviceKennung == xepInfoObject.TeleserviceKennung))
            {
                return false;
            }
            if (!(FahrzeugKommunikation == xepInfoObject.FahrzeugKommunikation))
            {
                return false;
            }
            if (!(Messtechnik == xepInfoObject.Messtechnik))
            {
                return false;
            }
            if (!(Versteckt == xepInfoObject.Versteckt))
            {
                return false;
            }
            if (!(HinweisId == xepInfoObject.HinweisId))
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_dede, xepInfoObject.Hinweis_dede) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_engb, xepInfoObject.Hinweis_engb) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_enus, xepInfoObject.Hinweis_enus) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_fr, xepInfoObject.Hinweis_fr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_th, xepInfoObject.Hinweis_th) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_sv, xepInfoObject.Hinweis_sv) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_it, xepInfoObject.Hinweis_it) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_es, xepInfoObject.Hinweis_es) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_id, xepInfoObject.Hinweis_id) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_ko, xepInfoObject.Hinweis_ko) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_el, xepInfoObject.Hinweis_el) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_tr, xepInfoObject.Hinweis_tr) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_zhcn, xepInfoObject.Hinweis_zhcn) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_ru, xepInfoObject.Hinweis_ru) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_nl, xepInfoObject.Hinweis_nl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_pt, xepInfoObject.Hinweis_pt) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_zhtw, xepInfoObject.Hinweis_zhtw) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_ja, xepInfoObject.Hinweis_ja) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_cscz, xepInfoObject.Hinweis_cscz) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Hinweis_plpl, xepInfoObject.Hinweis_plpl) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Name, xepInfoObject.Name) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(InformationsTyp, xepInfoObject.InformationsTyp) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Identifikator, xepInfoObject.Identifikator) != 0)
            {
                return false;
            }
            if (CreateDate != xepInfoObject.CreateDate)
            {
                return false;
            }
            if (ExpiryDate != xepInfoObject.ExpiryDate)
            {
                return false;
            }
            if (ChangeDate != xepInfoObject.ChangeDate)
            {
                return false;
            }
            if (LaunchDate != xepInfoObject.LaunchDate)
            {
                return false;
            }
            if (!(Abgasrelevant == xepInfoObject.Abgasrelevant))
            {
                return false;
            }
            if (string.CompareOrdinal(Dringlichkeit, xepInfoObject.Dringlichkeit) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Informationsformat, xepInfoObject.Informationsformat) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Grobzeichen, xepInfoObject.Grobzeichen) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(AwNummer, xepInfoObject.AwNummer) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(SwzNummer, xepInfoObject.SwzNummer) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(SiNummer, xepInfoObject.SiNummer) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(ZielIStufe, xepInfoObject.ZielIStufe) != 0)
            {
                return false;
            }
            if (!(ControlId == xepInfoObject.ControlId))
            {
                return false;
            }
            if (ModificationTime != xepInfoObject.ModificationTime)
            {
                return false;
            }
            if (string.CompareOrdinal(InfoType, xepInfoObject.InfoType) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(InfoFormat, xepInfoObject.InfoFormat) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(DocNumber, xepInfoObject.DocNumber) != 0)
            {
                return false;
            }
            if (!(Priority == xepInfoObject.Priority))
            {
                return false;
            }
            if (string.CompareOrdinal(Identifier, xepInfoObject.Identifier) != 0)
            {
                return false;
            }
            if (FastNavigation != xepInfoObject.FastNavigation)
            {
                return false;
            }
            return true;
        }

        public void SetFastNavigation(decimal? fastNav)
        {
            decimal? num = fastNav;
            if (((num.GetValueOrDefault() == default(decimal)) & num.HasValue) || !fastNav.HasValue)
            {
                fastNavigation = false;
                return;
            }
            fastNavigation = true;
            num = fastNav;
            decimal num2 = 1;
            if (!((num.GetValueOrDefault() == num2) & num.HasValue))
            {
                Log.Error("XepInfoObject.SetFastNavigation()", "Wrong value for column Fast_Navigation in XEP_INFOOBJECTS where id = {0}", Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public string GetLocalizedInfoObjectTitle(string language)
        {
            if (!string.IsNullOrWhiteSpace(ExplicitTitle))
            {
                return ExplicitTitle;
            }
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
                    Log.Warning("XepInfoObject.get_Title", "the given language \"{0}\" ist not available - return title for en-GB.", language);
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
