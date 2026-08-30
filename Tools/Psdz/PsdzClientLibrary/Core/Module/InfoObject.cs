using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

#pragma warning disable CS0067
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public class InfoObject : ITherapyPlanAction, INotifyPropertyChanged
    {
        private InfoObjectContentTransformed content;

        //private IXepInfoObject xepInfoObject;

        private bool isMarkedForExport;

        private bool isLoadedAsHotspot;

        private byte[] documentUtf8;

        private decimal? implicitVersion;

        private InfoObjectExecutionCout executionCoutField;

        private bool isESLError;

        private uint historyIndexField;

        private typeDiagObjectState stateField;

        private decimal idField;

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
#if false
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
#endif
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

        [XmlIgnore]
        public virtual string Title
        {
            get
            {
                return string.Empty;
            }
        }

        [XmlIgnore]
        public PsdzDatabase.SwiDiagObj ParentDiagnosisObject { get; set; }

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
                return String.Empty;
            }
        }

        [XmlIgnore]
        public bool IsLinked { get; private set; }

        [XmlIgnore]
        public string Identifier
        {
            get
            {
                return string.Empty;
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

        public IList<LocalizedText> GetLocalizedObjectTitle(IList<string> lang)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            return list;
        }

        public byte[] GetDocumentUtf8()
        {
            return GetDocumentUtf8(disableLinks: false, disablePictureZoom: false);
        }

        public byte[] GetDocumentUtf8(bool disableLinks, bool disablePictureZoom)
        {
            return null;
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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
            SwiActionReport = new List<ISwiActionReport>();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
