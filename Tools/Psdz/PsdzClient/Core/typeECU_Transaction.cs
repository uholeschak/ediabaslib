using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class typeECU_Transaction : INotifyPropertyChanged, IEcuTransaction
	{
		public typeECU_Transaction()
		{
			this.transactionFinishStatusField = false;
			this.transactionStatusField = StateType.unknown;
		}

		public string transactionId
		{
			get
			{
				return this.transactionIdField;
			}
			set
			{
				if (this.transactionIdField != null)
				{
					if (!this.transactionIdField.Equals(value))
					{
						this.transactionIdField = value;
						this.OnPropertyChanged("transactionId");
						return;
					}
				}
				else
				{
					this.transactionIdField = value;
					this.OnPropertyChanged("transactionId");
				}
			}
		}

		public string transactionName
		{
			get
			{
				return this.transactionNameField;
			}
			set
			{
				if (this.transactionNameField != null)
				{
					if (!this.transactionNameField.Equals(value))
					{
						this.transactionNameField = value;
						this.OnPropertyChanged("transactionName");
						return;
					}
				}
				else
				{
					this.transactionNameField = value;
					this.OnPropertyChanged("transactionName");
				}
			}
		}

		public string transactionResult
		{
			get
			{
				return this.transactionResultField;
			}
			set
			{
				if (this.transactionResultField != null)
				{
					if (!this.transactionResultField.Equals(value))
					{
						this.transactionResultField = value;
						this.OnPropertyChanged("transactionResult");
						return;
					}
				}
				else
				{
					this.transactionResultField = value;
					this.OnPropertyChanged("transactionResult");
				}
			}
		}

		public bool transactionFinishStatus
		{
			get
			{
				return this.transactionFinishStatusField;
			}
			set
			{
				if (!this.transactionFinishStatusField.Equals(value))
				{
					this.transactionFinishStatusField = value;
					this.OnPropertyChanged("transactionFinishStatus");
				}
			}
		}

		public StateType transactionStatus
		{
			get
			{
				return this.transactionStatusField;
			}
			set
			{
				if (!this.transactionStatusField.Equals(value))
				{
					this.transactionStatusField = value;
					this.OnPropertyChanged("transactionStatus");
				}
			}
		}

		public DateTime? transactionStart
		{
			get
			{
				return this.transactionStartField;
			}
			set
			{
				if (this.transactionStartField != null)
				{
					if (!this.transactionStartField.Equals(value))
					{
						this.transactionStartField = value;
						this.OnPropertyChanged("transactionStart");
						return;
					}
				}
				else
				{
					this.transactionStartField = value;
					this.OnPropertyChanged("transactionStart");
				}
			}
		}

		public DateTime? transactionEnd
		{
			get
			{
				return this.transactionEndField;
			}
			set
			{
				if (this.transactionEndField != null)
				{
					if (!this.transactionEndField.Equals(value))
					{
						this.transactionEndField = value;
						this.OnPropertyChanged("transactionEnd");
						return;
					}
				}
				else
				{
					this.transactionEndField = value;
					this.OnPropertyChanged("transactionEnd");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string transactionIdField;

		private string transactionNameField;

		private string transactionResultField;

		private bool transactionFinishStatusField;

		private StateType transactionStatusField;

		private DateTime? transactionStartField;

		private DateTime? transactionEndField;
	}
}
