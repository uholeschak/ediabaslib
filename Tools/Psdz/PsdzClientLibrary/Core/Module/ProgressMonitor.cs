using PsdzClient.Core;
using System.ComponentModel;
using System.Windows.Threading;

namespace BMW.Rheingold.CoreFramework
{
    public class ProgressMonitor : INotifyPropertyChanged, IProgressMonitor
    {
        private long endTime;
        private double processProgress;
        private FormatedData processDescription;
        private FormatedData taskDescription;
        private ProgressCancelBehavior cancelBehavior;
        private double progressMultiplier = 1.0;
        private double progressOffset;
        private double progressMultiplierCmdList = 1.0;
        private bool isAborted;
        private bool minimizeable;
        private bool isRunningInBackground;
        protected Dispatcher dispatcher;
        public bool IsRunningInBackground
        {
            get
            {
                return isRunningInBackground;
            }

            set
            {
                isRunningInBackground = value;
                NotifyPropertyChanged("IsRunningInBackground");
            }
        }

        public bool IsMinimizeable
        {
            get
            {
                return minimizeable;
            }

            set
            {
                if (minimizeable != value)
                {
                    minimizeable = value;
                    NotifyPropertyChanged("IsMinimizeable");
                }
            }
        }

        public bool IsMinimizeableToAppHeader { get; set; }

        public bool IsAborted
        {
            get
            {
                return isAborted;
            }

            set
            {
                if (isAborted != value)
                {
                    isAborted = value;
                    NotifyPropertyChanged("IsAborted");
                }
            }
        }

        public ProgressCancelBehavior CancelBehavior
        {
            get
            {
                return cancelBehavior;
            }

            set
            {
                if (cancelBehavior == value)
                {
                    return;
                }

                if (dispatcher != null)
                {
                    dispatcher.InvokeIfNoAccess(delegate
                    {
                        cancelBehavior = value;
                        NotifyPropertyChanged("CancelBehavior");
                    });
                }
                else
                {
                    cancelBehavior = value;
                    NotifyPropertyChanged("CancelBehavior");
                }
            }
        }

        public long EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                if (endTime != value)
                {
                    endTime = value;
                    NotifyPropertyChanged("EndTime");
                }
            }
        }

        public double ProcessProgress
        {
            get
            {
                return processProgress;
            }

            set
            {
                if (processProgress == value)
                {
                    return;
                }

                if (dispatcher != null)
                {
                    dispatcher.InvokeIfNoAccess(delegate
                    {
                        processProgress = progressOffset + value * progressMultiplier * progressMultiplierCmdList;
                        NotifyPropertyChanged("ProcessProgress");
                    });
                }
                else
                {
                    processProgress = progressOffset + value * progressMultiplier * progressMultiplierCmdList;
                    NotifyPropertyChanged("ProcessProgress");
                }
            }
        }

        public double ProgressMultiplierCmdList
        {
            get
            {
                return progressMultiplierCmdList;
            }

            set
            {
                progressOffset = processProgress;
                progressMultiplier = 1.0;
                progressMultiplierCmdList = value;
            }
        }

        public FormatedData TaskDescription
        {
            get
            {
                return taskDescription;
            }

            set
            {
                if (value == null || taskDescription == value)
                {
                    return;
                }

                if (dispatcher != null)
                {
                    dispatcher.InvokeIfNoAccess(delegate
                    {
                        taskDescription = value;
                        NotifyPropertyChanged("TaskDescription");
                    });
                }
                else
                {
                    taskDescription = value;
                    NotifyPropertyChanged("TaskDescription");
                }
            }
        }

        public FormatedData ProcessDescription
        {
            get
            {
                return processDescription;
            }

            set
            {
                if (value == null || processDescription == value)
                {
                    return;
                }

                if (dispatcher != null)
                {
                    dispatcher.InvokeIfNoAccess(delegate
                    {
                        processDescription = value;
                        NotifyPropertyChanged("ProcessDescription");
                    });
                }
                else
                {
                    processDescription = value;
                    NotifyPropertyChanged("ProcessDescription");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ProgressMonitor()
        {
            IsRunningInBackground = false;
            IsMinimizeableToAppHeader = false;
            IsMinimizeable = false;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual bool RequestConfirmation(ProgressRequestConfirmationType requestType, params object[] paramList)
        {
            return false;
        }
    }
}