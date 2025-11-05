using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public class typeECU_Transaction : IEcuTransaction, INotifyPropertyChanged
    {
        private string transactionIdField;
        private string transactionNameField;
        private string transactionResultField;
        private bool transactionFinishStatusField;
        private StateType transactionStatusField;
        private DateTime? transactionStartField;
        private DateTime? transactionEndField;
        public string transactionId
        {
            get
            {
                return transactionIdField;
            }

            set
            {
                if (transactionIdField != null)
                {
                    if (!transactionIdField.Equals(value))
                    {
                        transactionIdField = value;
                        OnPropertyChanged("transactionId");
                    }
                }
                else
                {
                    transactionIdField = value;
                    OnPropertyChanged("transactionId");
                }
            }
        }

        public string transactionName
        {
            get
            {
                return transactionNameField;
            }

            set
            {
                if (transactionNameField != null)
                {
                    if (!transactionNameField.Equals(value))
                    {
                        transactionNameField = value;
                        OnPropertyChanged("transactionName");
                    }
                }
                else
                {
                    transactionNameField = value;
                    OnPropertyChanged("transactionName");
                }
            }
        }

        public string transactionResult
        {
            get
            {
                return transactionResultField;
            }

            set
            {
                if (transactionResultField != null)
                {
                    if (!transactionResultField.Equals(value))
                    {
                        transactionResultField = value;
                        OnPropertyChanged("transactionResult");
                    }
                }
                else
                {
                    transactionResultField = value;
                    OnPropertyChanged("transactionResult");
                }
            }
        }

        public bool transactionFinishStatus
        {
            get
            {
                return transactionFinishStatusField;
            }

            set
            {
                if (!transactionFinishStatusField.Equals(value))
                {
                    transactionFinishStatusField = value;
                    OnPropertyChanged("transactionFinishStatus");
                }
            }
        }

        public StateType transactionStatus
        {
            get
            {
                return transactionStatusField;
            }

            set
            {
                if (!transactionStatusField.Equals(value))
                {
                    transactionStatusField = value;
                    OnPropertyChanged("transactionStatus");
                }
            }
        }

        public DateTime? transactionStart
        {
            get
            {
                return transactionStartField;
            }

            set
            {
                if (transactionStartField.HasValue)
                {
                    if (!transactionStartField.Equals(value))
                    {
                        transactionStartField = value;
                        OnPropertyChanged("transactionStart");
                    }
                }
                else
                {
                    transactionStartField = value;
                    OnPropertyChanged("transactionStart");
                }
            }
        }

        public DateTime? transactionEnd
        {
            get
            {
                return transactionEndField;
            }

            set
            {
                if (transactionEndField.HasValue)
                {
                    if (!transactionEndField.Equals(value))
                    {
                        transactionEndField = value;
                        OnPropertyChanged("transactionEnd");
                    }
                }
                else
                {
                    transactionEndField = value;
                    OnPropertyChanged("transactionEnd");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public typeECU_Transaction()
        {
            transactionFinishStatusField = false;
            transactionStatusField = StateType.unknown;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}