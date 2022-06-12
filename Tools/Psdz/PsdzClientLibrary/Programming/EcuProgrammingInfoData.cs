using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
	[DataContract]
	public class EcuProgrammingInfoData : INotifyPropertyChanged, IEcuProgrammingInfo, IEcuProgrammingInfoData
	{
		[XmlIgnore]
		public IEcu Ecu
		{
			get
			{
				return this.ecu;
			}
			set
			{
				this.ecu = value;
				this.OnPropertyChanged("Ecu");
			}
		}

		[XmlIgnore]
		public IEnumerable<IProgrammingAction> ProgrammingActions
		{
			get
			{
				return new List<IProgrammingAction>();
			}
		}

		[DataMember]
		public string Category
		{
			get
			{
				return this.category;
			}
			set
			{
				this.category = value;
				this.OnPropertyChanged("Category");
			}
		}

		[DataMember]
		public string EcuTitle
		{
			get
			{
				return this.ecuTitle;
			}
			set
			{
				this.ecuTitle = value;
				this.OnPropertyChanged("EcuTitle");
			}
		}

		[DataMember]
		public string EcuDescription
		{
			get
			{
				return this.ecuDescription;
			}
			set
			{
				this.ecuDescription = value;
				this.OnPropertyChanged("EcuDescription");
			}
		}

		[DataMember]
		[XmlIgnore]
		public ObservableCollection<IProgrammingActionData> ProgrammingActionData
		{
			get
			{
				return this.programmingActions;
			}
			set
			{
				this.programmingActions = value;
				this.OnPropertyChanged("ProgrammingActionData");
			}
		}

		public bool IsCodingDisabled
		{
			get
			{
				return this.codingDisabled;
			}
			set
			{
				if (this.codingDisabled != value)
				{
					this.codingDisabled = value;
					this.OnPropertyChanged("IsCodingDisabled");
				}
			}
		}

		public bool IsCodingScheduled
		{
			get
			{
				return this.codingScheduled;
			}
			set
			{
				if (this.codingScheduled != value)
				{
					this.codingScheduled = value;
					this.OnPropertyChanged("IsCodingScheduled");
				}
			}
		}

		public bool IsProgrammingDisabled
		{
			get
			{
				return this.programmingDisabled;
			}
			set
			{
				if (this.programmingDisabled != value)
				{
					this.programmingDisabled = value;
					this.OnPropertyChanged("IsProgrammingDisabled");
				}
			}
		}

		public bool IsProgrammingSelectionDisabled
		{
			get
			{
				return this.programmingSelectionDisabled;
			}
			set
			{
				if (this.programmingSelectionDisabled != value)
				{
					this.programmingSelectionDisabled = value;
					this.OnPropertyChanged("IsProgrammingSelectionDisabled");
				}
			}
		}

		public bool IsCodingSelectionDisabled
		{
			get
			{
				return this.codingSelectionDisabled;
			}
			set
			{
				if (this.codingSelectionDisabled != value)
				{
					this.codingSelectionDisabled = value;
					this.OnPropertyChanged("IsCodingSelectionDisabled");
				}
			}
		}

		public bool IsExchangeDoneDisabled
		{
			get
			{
				return this.exchangeDoneDisabled;
			}
			set
			{
				if (this.exchangeDoneDisabled != value)
				{
					this.exchangeDoneDisabled = value;
					this.OnPropertyChanged("IsExchangeDoneDisabled");
				}
			}
		}

		public bool IsExchangeScheduledDisabled
		{
			get
			{
				return this.exchangeScheduledDisabled;
			}
			set
			{
				if (this.exchangeScheduledDisabled != value)
				{
					this.exchangeScheduledDisabled = value;
					this.OnPropertyChanged("IsExchangeScheduledDisabled");
				}
			}
		}

		public bool IsExchangeDone
		{
			get
			{
				return this.isExchangeDone;
			}
			set
			{
				if (this.isExchangeDone != value)
				{
					this.isExchangeDone = value;
					this.OnPropertyChanged("IsExchangeDone");
				}
			}
		}

		public bool IsExchangeScheduled
		{
			get
			{
				return this.isExchangeScheduled;
			}
			set
			{
				if (this.isExchangeScheduled != value)
				{
					this.isExchangeScheduled = value;
					this.OnPropertyChanged("IsExchangeScheduled");
				}
			}
		}

		public double ProgressValue
		{
			get
			{
				return this.progressValue;
			}
			set
			{
				if (this.progressValue != value)
				{
					this.progressValue = value;
					this.OnPropertyChanged("ProgressValue");
				}
			}
		}

		public bool IsProgrammingScheduled
		{
			get
			{
				return this.programmingScheduled;
			}
			set
			{
				if (this.programmingScheduled != value)
				{
					this.programmingScheduled = value;
					this.OnPropertyChanged("IsProgrammingScheduled");
				}
			}
		}

		[DataMember]
		public ProgrammingActionState? State { get; set; }

		[XmlIgnore]
		public IStandardSvk SvkCurrent
		{
			get
			{
				return this.svkCurrent;
			}
			set
			{
				if (this.svkCurrent != value)
				{
					this.svkCurrent = value;
					this.OnPropertyChanged("SvkCurrent");
				}
			}
		}

		[XmlIgnore]
		public IStandardSvk SvkTarget
		{
			get
			{
				return this.svkTarget;
			}
			set
			{
				if (this.svkTarget != value)
				{
					this.svkTarget = value;
					this.OnPropertyChanged("SvkTarget");
				}
			}
		}

		public string EcuIdentifier { get; set; }

		public int FlashOrder
		{
			get
			{
				return this.flashOrder;
			}
			set
			{
				if (this.flashOrder != value)
				{
					this.flashOrder = value;
					this.OnPropertyChanged("FlashOrder");
				}
			}
		}

		public IProgrammingAction GetProgrammingAction(ProgrammingActionType type)
		{
			throw new NotSupportedException();
		}

		public IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter)
		{
			throw new NotSupportedException();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		[DataMember]
		private bool codingScheduled;

		[DataMember]
		private bool programmingScheduled;

		[DataMember]
		private bool codingDisabled;

		[DataMember]
		private bool programmingDisabled;

		[DataMember]
		private double progressValue;

		[DataMember]
		private IStandardSvk svkCurrent;

		[DataMember]
		private IStandardSvk svkTarget;

		[DataMember]
		private bool isExchangeScheduled;

		[DataMember]
		private bool isExchangeDone;

		[DataMember]
		private bool exchangeDoneDisabled;

		[DataMember]
		private bool exchangeScheduledDisabled;

		[DataMember]
		private bool programmingSelectionDisabled;

		[DataMember]
		private bool codingSelectionDisabled;

		[DataMember]
		private IEcu ecu;

		[DataMember]
		private int flashOrder = int.MaxValue;

		private string category = string.Empty;

		private string ecuTitle = string.Empty;

		private string ecuDescription = string.Empty;

		private ObservableCollection<IProgrammingActionData> programmingActions = new ObservableCollection<IProgrammingActionData>();
	}
}
