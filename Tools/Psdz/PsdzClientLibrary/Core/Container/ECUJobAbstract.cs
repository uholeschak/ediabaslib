// BMW.Rheingold.VehicleCommunication.ECUJobAbstract
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("System.Xml", "2.0.50727.3082")]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://tempuri.org/ECUJob.xsd", TypeName = "ECUJobAbstract")]
    [XmlRoot(Namespace = "http://tempuri.org/ECUJob.xsd", IsNullable = true, ElementName = "ECUJobAbstract")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class ECUJobAbstract : INotifyPropertyChanged
    {
        private List<ECUResult> jobResult;
        private string ecuName;
        private DateTime executionStartTime;
        private DateTime executionEndTime;
        private string jobName;
        private string jobParam;
        private string jobResultFilter;
        private int jobErrorCode;
        private string jobErrorText;
        private int jobResultSets;
        [XmlElement("jobResult", IsNullable = true)]
        public List<ECUResult> JobResult
        {
            get
            {
                return jobResult;
            }

            set
            {
                if (jobResult != value)
                {
                    jobResult = value;
                    RaisePropertyChanged("JobResult");
                }
            }
        }

        [XmlAttribute(AttributeName = "ecuName")]
        public string EcuName
        {
            get
            {
                return ecuName;
            }

            set
            {
                if (ecuName != value)
                {
                    ecuName = value;
                    RaisePropertyChanged("EcuName");
                }
            }
        }

        [XmlAttribute(AttributeName = "executionStartTime")]
        public DateTime ExecutionStartTime
        {
            get
            {
                return executionStartTime;
            }

            set
            {
                if (executionStartTime != value)
                {
                    executionStartTime = value;
                    RaisePropertyChanged("ExecutionStartTime");
                }
            }
        }

        [XmlAttribute(AttributeName = "executionEndTime")]
        public DateTime ExecutionEndTime
        {
            get
            {
                return executionEndTime;
            }

            set
            {
                if (executionEndTime != value)
                {
                    executionEndTime = value;
                    RaisePropertyChanged("ExecutionEndTime");
                }
            }
        }

        [XmlAttribute(AttributeName = "jobName")]
        public string JobName
        {
            get
            {
                return jobName;
            }

            set
            {
                if (jobName != value)
                {
                    jobName = value;
                    RaisePropertyChanged("JobName");
                }
            }
        }

        [XmlAttribute(AttributeName = "jobParam")]
        public string JobParam
        {
            get
            {
                return jobParam;
            }

            set
            {
                if (jobParam != value)
                {
                    jobParam = value;
                    RaisePropertyChanged("JobParam");
                }
            }
        }

        [XmlAttribute(AttributeName = "jobResultFilter")]
        public string JobResultFilter
        {
            get
            {
                return jobResultFilter;
            }

            set
            {
                if (jobResultFilter != value)
                {
                    jobResultFilter = value;
                    RaisePropertyChanged("JobResultFilter");
                }
            }
        }

        [XmlAttribute(AttributeName = "jobErrorCode")]
        public int JobErrorCode
        {
            get
            {
                return jobErrorCode;
            }

            set
            {
                if (jobErrorCode != value)
                {
                    jobErrorCode = value;
                    RaisePropertyChanged("JobErrorCode");
                }
            }
        }

        [XmlAttribute(AttributeName = "jobErrorText")]
        public string JobErrorText
        {
            get
            {
                return jobErrorText;
            }

            set
            {
                if (jobErrorText != value)
                {
                    jobErrorText = value;
                    RaisePropertyChanged("JobErrorText");
                }
            }
        }

        [XmlAttribute(AttributeName = "jobResultSets")]
        public int JobResultSets
        {
            get
            {
                return jobResultSets;
            }

            set
            {
                if (jobResultSets != value)
                {
                    jobResultSets = value;
                    RaisePropertyChanged("JobResultSets");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected ECUJobAbstract()
        {
        }

        protected ECUJobAbstract(List<ECUResult> jobResult, string ecuName, DateTime executionStartTime, DateTime executionEndTime, string jobName, string jobParam, string jobResultFilter, int jobErrorCode, string jobErrorText, int jobResultSets)
        {
            this.jobResult = jobResult;
            this.ecuName = ecuName;
            this.executionStartTime = executionStartTime;
            this.executionEndTime = executionEndTime;
            this.jobName = jobName;
            this.jobParam = jobParam;
            this.jobResultFilter = jobResultFilter;
            this.jobErrorCode = jobErrorCode;
            this.jobErrorText = jobErrorText;
            this.jobResultSets = jobResultSets;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}