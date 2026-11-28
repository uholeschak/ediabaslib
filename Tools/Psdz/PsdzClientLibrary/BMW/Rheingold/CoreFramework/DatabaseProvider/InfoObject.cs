using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.DatabaseProvider.SmartMaintenance;

#pragma warning disable CS0067
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class InfoObject : ITherapyPlanAction, INotifyPropertyChanged, IServiceProgramProgramming
    {
        private readonly InfoObjectTransformationStrategie transformation = new DocumentHelper();

        private InfoObjectContentTransformed content;

        private XEP_WSCONFIGFILTERS infoTypeTitle;

        private IXepInfoObject xepInfoObject;

        private EslDocumentIsta eslDocument;

        private bool isMarkedForExport;

        private bool isLoadedAsHotspot;

        private byte[] documentUtf8;

        private decimal? implicitVersion;

        private bool isESLError;

        private uint historyIndexField;

        private typeDiagObjectState stateField;

        private decimal idField;

        private InfoObjectExecutionCout executionCoutField;

        public bool IsESLError
        {
            get
            {
                return isESLError;
            }
            set
            {
                isESLError = value;
            }
        }

        public decimal? Version => implicitVersion ?? xepInfoObject?.VersionNumber;

        [XmlIgnore]
        public byte[] DocumentUtf8
        {
            get
            {
                return documentUtf8;
            }
            set
            {
                documentUtf8 = value;
            }
        }

        [XmlIgnore]
        public InfoObjectContentTransformed Content
        {
            get
            {
                if (content == null)
                {
                    content = new InfoObjectContentTransformed(this);
                }
                return content;
            }
        }

        [Obsolete("Temporary workaround.")]
        [XmlIgnore]
        public XepInfoObject XepInfoObjectCasted => XepInfoObject as XepInfoObject;

        [XmlIgnore]
        public IXepInfoObject XepInfoObject
        {
            get
            {
                if (xepInfoObject == null)
                {
                    xepInfoObject = new XepInfoObject();
                }
                return xepInfoObject;
            }
            set
            {
                if (xepInfoObject != value)
                {
                    xepInfoObject = value;
                    OnPropertyChanged("XepInfoObject");
                }
            }
        }

        [XmlIgnore]
        public EslDocumentIsta EslDocument
        {
            get
            {
                return eslDocument;
            }
            set
            {
                eslDocument = value;
            }
        }

        [XmlIgnore]
        public XEP_WSCONFIGFILTERS InfoTypeTitle
        {
            get
            {
                if (infoTypeTitle == null)
                {
                    //[-] InfoTypeTitle = DatabaseProviderFactory.Instance.GetInfoTypes(xepInfoObject?.InfoType);
                    return infoTypeTitle;
                }
                return infoTypeTitle;
            }
            set
            {
                if (infoTypeTitle != value)
                {
                    infoTypeTitle = value;
                    OnPropertyChanged("InfoTypeTitle");
                }
            }
        }

        [XmlIgnore]
        [Obsolete("Property is obsolete and might get removed in future. Please use the Note property instead.")]
        public string Hinweis => Note;

        [XmlIgnore]
        public string Note
        {
            get
            {
                IXepInfoObject xepInfoObject = XepInfoObject;
                string text;
                switch (ConfigSettings.CurrentUICulture)
                {
                    case "de-DE":
                        text = xepInfoObject.Hinweis_dede;
                        break;
                    case "en-GB":
                        text = xepInfoObject.Hinweis_engb;
                        break;
                    case "en-US":
                        text = xepInfoObject.Hinweis_enus;
                        break;
                    case "fr-FR":
                        text = xepInfoObject.Hinweis_fr;
                        break;
                    case "es-ES":
                        text = xepInfoObject.Hinweis_es;
                        break;
                    case "th-TH":
                        text = xepInfoObject.Hinweis_th;
                        break;
                    case "tr-TR":
                        text = xepInfoObject.Hinweis_tr;
                        break;
                    case "el-GR":
                        text = xepInfoObject.Hinweis_el;
                        break;
                    case "ja-JP":
                        text = xepInfoObject.Hinweis_ja;
                        break;
                    case "ru-RU":
                        text = xepInfoObject.Hinweis_ru;
                        break;
                    case "it-IT":
                        text = xepInfoObject.Hinweis_it;
                        break;
                    case "nl-NL":
                        text = xepInfoObject.Hinweis_nl;
                        break;
                    case "pl-PL":
                        text = xepInfoObject.Hinweis_plpl;
                        break;
                    case "cs-CZ":
                        text = xepInfoObject.Hinweis_cscz;
                        break;
                    case "pt-PT":
                        text = xepInfoObject.Hinweis_pt;
                        break;
                    case "sv-SE":
                        text = xepInfoObject.Hinweis_sv;
                        break;
                    case "zh-CN":
                        text = xepInfoObject.Hinweis_zhcn;
                        break;
                    case "zh-TW":
                        text = xepInfoObject.Hinweis_zhtw;
                        break;
                    case "ko-KR":
                        text = xepInfoObject.Hinweis_ko;
                        break;
                    default:
                        Log.Warning("InfoObject.get_Hinweis", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                        text = xepInfoObject.Hinweis_engb;
                        break;
                }
                if (string.IsNullOrEmpty(text))
                {
                    return xepInfoObject.Hinweis_engb;
                }
                return text;
            }
        }

        [XmlIgnore]
        public virtual string Title
        {
            get
            {
                if (!IsESLError)
                {
                    return xepInfoObject.Title;
                }
                return eslDocument.Title;
            }
        }

        [XmlIgnore]
        public XEP_DIAGNOSISOBJECTSEX ParentDiagnosisObject { get; set; }

        [XmlIgnore]
        public int Index { get; set; }

        [XmlIgnore]
        public bool IsExternalDocument { get; set; }

        [XmlIgnore]
        public bool IsMarkedForExport
        {
            get
            {
                return isMarkedForExport;
            }
            set
            {
                isMarkedForExport = value;
                OnPropertyChanged("IsMarkedForExport");
            }
        }

        [XmlIgnore]
        public string InfoType
        {
            get
            {
                if (!IsESLError)
                {
                    return XepInfoObject.InfoType;
                }
                return EslDocument.InfoType;
            }
        }

        [XmlIgnore]
        public string DocumentType
        {
            get
            {
                if (!IsESLError)
                {
                    return XepInfoObject.DocumentType;
                }
                return "XML";
            }
        }

        [XmlIgnore]
        public bool IsLinked { get; private set; }

        [XmlIgnore]
        public string Identifier
        {
            get
            {
                if (!IsESLError)
                {
                    return XepInfoObject.Identifikator;
                }
                return EslDocument.Identifier;
            }
        }

        [XmlIgnore]
        public SwiActionLinkType LinkType => SwiActionLinkType.SwiActionDiagnosticLink;

        [XmlIgnore]
        public decimal? Priority { get; set; }

        [XmlIgnore]
        public DateTime? StartExecution { get; set; }

        [XmlIgnore]
        public DateTime? EndExecution { get; set; }

        [XmlIgnore]
        public IList<ISwiActionReport> SwiActionReport { get; private set; }

        [XmlIgnore]
        public bool IsLoadedAsHotspot
        {
            get
            {
                return isLoadedAsHotspot;
            }
            set
            {
                isLoadedAsHotspot = value;
                OnPropertyChanged("IsLoadedAsHotspot");
            }
        }

        public uint HistoryIndex
        {
            get
            {
                return historyIndexField;
            }
            set
            {
                _ = historyIndexField;
                if (!historyIndexField.Equals(value))
                {
                    historyIndexField = value;
                    OnPropertyChanged("HistoryIndex");
                }
            }
        }

        public typeDiagObjectState State
        {
            get
            {
                return stateField;
            }
            set
            {
                if (!stateField.Equals(value))
                {
                    stateField = value;
                    OnPropertyChanged("State");
                }
            }
        }

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

        public InfoObjectExecutionCout ExecutionCout
        {
            get
            {
                return executionCoutField;
            }
            set
            {
                if (executionCoutField != null)
                {
                    if (!executionCoutField.Equals(value))
                    {
                        executionCoutField = value;
                        OnPropertyChanged("ExecutionCout");
                    }
                }
                else
                {
                    executionCoutField = value;
                    OnPropertyChanged("ExecutionCout");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public InfoObject(IXepInfoObject xep)
        {
            historyIndexField = 0u;
            stateField = typeDiagObjectState.NotCalled;
            xepInfoObject = xep;
            idField = xep.Id;
            ExecutionCout = new InfoObjectExecutionCout();
            IsLinked = false;
            SwiActionReport = new List<ISwiActionReport>();
        }

        public InfoObject(EslDocumentIsta eslDocument)
        {
            IsESLError = true;
            this.eslDocument = eslDocument;
            historyIndexField = 0u;
            stateField = typeDiagObjectState.NotCalled;
            ExecutionCout = new InfoObjectExecutionCout();
            IsLinked = false;
            SwiActionReport = new List<ISwiActionReport>();
            idField = eslDocument.OID;
            Content.Doc = eslDocument.Xml;
            Priority = 1;
        }

        public InfoObject(IXepInfoObject xep, decimal? priority)
            : this(xep)
        {
            Priority = priority;
        }

        public IList<LocalizedText> GetLocalizedObjectTitle(IList<string> lang)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            if (XepInfoObject == null)
            {
                return list;
            }
            list.AddRange(lang.Select((string x) => new LocalizedText(xepInfoObject.GetLocalizedInfoObjectTitle(x), x)));
            return list;
        }

        public byte[] GetDocumentUtf8()
        {
            return GetDocumentUtf8(disableLinks: false, disablePictureZoom: false);
        }

        public byte[] GetDocumentUtf8(bool disableLinks, bool disablePictureZoom)
        {
            return transformation.GetDocumentUtf8(this, disableLinks, disablePictureZoom);
        }

        public void DoDocumentTransformation()
        {
            transformation.DoDocumentTransformation(this);
        }

        public override bool Equals(object obj)
        {
            if (obj is InfoObject)
            {
                InfoObject infoObject = obj as InfoObject;
                if (infoObject.IsESLError || IsESLError)
                {
                    return false;
                }
                if (infoObject != null && XepInfoObject != null && GetHashCode() == infoObject.GetHashCode() && XepInfoObject.Equals(infoObject.XepInfoObject))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Update(IServiceProgramProgramming prg)
        {
            if (prg != null)
            {
                Priority = prg.Priority;
                StartExecution = prg.StartExecution;
                EndExecution = prg.EndExecution;
                SwiActionReport.AddRangeIfNotContains(prg.SwiActionReport);
                IsLinked = true;
            }
        }

        public void ResetSwiActionReport()
        {
            IsLinked = false;
            SwiActionReport.Clear();
        }

        public ProgrammingActionState Execute(IProgrammingSessionExt session)
        {
            throw new NotSupportedException();
        }

        public ProgrammingActionState Execute(IProgrammingSessionExt session, bool setWaringOnInvokeException)
        {
            throw new NotImplementedException();
        }

        public void ResetDocumentTransformation()
        {
            if (Content != null)
            {
                Content.IsDocumentTransformationValid = false;
            }
        }

        public void SetImplicitVersion(decimal version)
        {
            implicitVersion = version;
            OnPropertyChanged("Version");
        }

        public InfoObject()
        {
            executionCoutField = new InfoObjectExecutionCout();
            historyIndexField = 0u;
            stateField = typeDiagObjectState.NotCalled;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
