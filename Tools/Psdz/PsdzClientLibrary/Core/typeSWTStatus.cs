using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
	public class typeSWTStatus : INotifyPropertyChanged, ISwtStatus
	{
		public string STAT_SW_ID
		{
			get
			{
				return this.sTAT_SW_IDField;
			}
			set
			{
				if (this.sTAT_SW_IDField != null)
				{
					if (!this.sTAT_SW_IDField.Equals(value))
					{
						this.sTAT_SW_IDField = value;
						this.OnPropertyChanged("STAT_SW_ID");
						return;
					}
				}
				else
				{
					this.sTAT_SW_IDField = value;
					this.OnPropertyChanged("STAT_SW_ID");
				}
			}
		}

		public string Title
		{
			get
			{
				return this.titleField;
			}
			set
			{
				if (this.titleField != null)
				{
					if (!this.titleField.Equals(value))
					{
						this.titleField = value;
						this.OnPropertyChanged("Title");
						return;
					}
				}
				else
				{
					this.titleField = value;
					this.OnPropertyChanged("Title");
				}
			}
		}

		public string STAT_ROOT_CERT_STATUS_CODE
		{
			get
			{
				return this.sTAT_ROOT_CERT_STATUS_CODEField;
			}
			set
			{
				if (this.sTAT_ROOT_CERT_STATUS_CODEField != null)
				{
					if (!this.sTAT_ROOT_CERT_STATUS_CODEField.Equals(value))
					{
						this.sTAT_ROOT_CERT_STATUS_CODEField = value;
						this.OnPropertyChanged("STAT_ROOT_CERT_STATUS_CODE");
						return;
					}
				}
				else
				{
					this.sTAT_ROOT_CERT_STATUS_CODEField = value;
					this.OnPropertyChanged("STAT_ROOT_CERT_STATUS_CODE");
				}
			}
		}

		public string STAT_SIGS_CERT_STATUS_CODE
		{
			get
			{
				return this.sTAT_SIGS_CERT_STATUS_CODEField;
			}
			set
			{
				if (this.sTAT_SIGS_CERT_STATUS_CODEField != null)
				{
					if (!this.sTAT_SIGS_CERT_STATUS_CODEField.Equals(value))
					{
						this.sTAT_SIGS_CERT_STATUS_CODEField = value;
						this.OnPropertyChanged("STAT_SIGS_CERT_STATUS_CODE");
						return;
					}
				}
				else
				{
					this.sTAT_SIGS_CERT_STATUS_CODEField = value;
					this.OnPropertyChanged("STAT_SIGS_CERT_STATUS_CODE");
				}
			}
		}

		public string STAT_SW_SIG_STATUS_CODE
		{
			get
			{
				return this.sTAT_SW_SIG_STATUS_CODEField;
			}
			set
			{
				if (this.sTAT_SW_SIG_STATUS_CODEField != null)
				{
					if (!this.sTAT_SW_SIG_STATUS_CODEField.Equals(value))
					{
						this.sTAT_SW_SIG_STATUS_CODEField = value;
						this.OnPropertyChanged("STAT_SW_SIG_STATUS_CODE");
						return;
					}
				}
				else
				{
					this.sTAT_SW_SIG_STATUS_CODEField = value;
					this.OnPropertyChanged("STAT_SW_SIG_STATUS_CODE");
				}
			}
		}

		public string STAT_FSCS_CERT_STATUS_CODE
		{
			get
			{
				return this.sTAT_FSCS_CERT_STATUS_CODEField;
			}
			set
			{
				if (this.sTAT_FSCS_CERT_STATUS_CODEField != null)
				{
					if (!this.sTAT_FSCS_CERT_STATUS_CODEField.Equals(value))
					{
						this.sTAT_FSCS_CERT_STATUS_CODEField = value;
						this.OnPropertyChanged("STAT_FSCS_CERT_STATUS_CODE");
						return;
					}
				}
				else
				{
					this.sTAT_FSCS_CERT_STATUS_CODEField = value;
					this.OnPropertyChanged("STAT_FSCS_CERT_STATUS_CODE");
				}
			}
		}

		public string STAT_FSC_STATUS_CODE
		{
			get
			{
				return this.sTAT_FSC_STATUS_CODEField;
			}
			set
			{
				if (this.sTAT_FSC_STATUS_CODEField != null)
				{
					if (!this.sTAT_FSC_STATUS_CODEField.Equals(value))
					{
						this.sTAT_FSC_STATUS_CODEField = value;
						this.OnPropertyChanged("STAT_FSC_STATUS_CODE");
						return;
					}
				}
				else
				{
					this.sTAT_FSC_STATUS_CODEField = value;
					this.OnPropertyChanged("STAT_FSC_STATUS_CODE");
				}
			}
		}

		public string OrderingStatus
		{
			get
			{
				return this.orderingStatusField;
			}
			set
			{
				if (this.orderingStatusField != null)
				{
					if (!this.orderingStatusField.Equals(value))
					{
						this.orderingStatusField = value;
						this.OnPropertyChanged("OrderingStatus");
						return;
					}
				}
				else
				{
					this.orderingStatusField = value;
					this.OnPropertyChanged("OrderingStatus");
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

		public uint applicationNoUI
		{
			get
			{
				try
				{
					if (!string.IsNullOrEmpty(this.STAT_SW_ID) && this.STAT_SW_ID.Length >= 8)
					{
						return Convert.ToUInt32(this.STAT_SW_ID.Substring(0, 4), 16);
					}
				}
				catch (Exception exception)
				{
					Log.WarningException("swIdType.get_applicationNoUI()", exception);
				}
				return 0U;
			}
		}

		public uint upgradeIndexUI
		{
			get
			{
				try
				{
					if (!string.IsNullOrEmpty(this.STAT_SW_ID) && this.STAT_SW_ID.Length >= 8)
					{
						return Convert.ToUInt32(this.STAT_SW_ID.Substring(4, 4), 16);
					}
				}
				catch (Exception exception)
				{
					Log.WarningException("swIdType.get_upgradeIndexUI()", exception);
				}
				return 0U;
			}
		}

		private string sTAT_SW_IDField;

		private string titleField;

		private string sTAT_ROOT_CERT_STATUS_CODEField;

		private string sTAT_SIGS_CERT_STATUS_CODEField;

		private string sTAT_SW_SIG_STATUS_CODEField;

		private string sTAT_FSCS_CERT_STATUS_CODEField;

		private string sTAT_FSC_STATUS_CODEField;

		private string orderingStatusField;
	}
}
