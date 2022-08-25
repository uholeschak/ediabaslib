using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
	[DataContract]
	public class ProgrammingActionData : INotifyPropertyChanged, IProgrammingActionData
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[DataMember]
		public ProgrammingActionState StateProgramming
		{
			get
			{
				return this.stateProgramming;
			}
			set
			{
				if (this.stateProgramming != value)
				{
					this.stateProgramming = value;
					this.OnPropertyChanged("StateProgramming");
				}
			}
		}

		[DataMember]
		public IEcu ParentEcu
		{
			get
			{
				return this.parentEcu;
			}
			set
			{
				if (this.parentEcu != value)
				{
					this.parentEcu = value;
					this.OnPropertyChanged("ParentEcu");
				}
			}
		}

		[DataMember]
		public ProgrammingActionType Type
		{
			get
			{
				return this.type;
			}
			set
			{
				if (this.type != value)
				{
					this.type = value;
					this.OnPropertyChanged("Type");
				}
			}
		}

		[DataMember]
		public string Channel
		{
			get
			{
				return this.channel;
			}
			set
			{
				if (this.channel != value)
				{
					this.channel = value;
					this.OnPropertyChanged("Channel");
				}
			}
		}

		[DataMember]
		public string Note
		{
			get
			{
				return this.note;
			}
			set
			{
				if (this.note != value)
				{
					this.note = value;
					this.OnPropertyChanged("Note");
				}
			}
		}

		[DataMember]
		public bool IsEditable
		{
			get
			{
				return this.isEditable;
			}
			set
			{
				if (this.isEditable != value)
				{
					this.isEditable = value;
					this.OnPropertyChanged("IsEditable");
				}
			}
		}

		[DataMember]
		public bool IsSelected
		{
			get
			{
				return this.isSelected;
			}
			set
			{
				if (this.isSelected != value)
				{
					this.isSelected = value;
					this.OnPropertyChanged("IsSelected");
				}
			}
		}

		[DataMember]
		public int Order
		{
			get
			{
				return this.order;
			}
			internal set
			{
				this.order = value;
				this.OnPropertyChanged("Order");
			}
		}

		public bool IsFlashAction
		{
			get
			{
				return ProgrammingActionData.FlashActions.Contains(this.Type);
			}
		}

		public bool IsEscalationActionType
		{
			get
			{
				return ProgrammingActionData.EscalationActions.Contains(this.Type);
			}
		}

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

        private static readonly ProgrammingActionType[] FlashActions = new ProgrammingActionType[3]
        {
            ProgrammingActionType.BootloaderProgramming,
            ProgrammingActionType.Programming,
            ProgrammingActionType.DataProgramming
        };

        private static readonly ProgrammingActionType[] EscalationActions = new ProgrammingActionType[5]
        {
            ProgrammingActionType.BootloaderProgramming,
            ProgrammingActionType.Programming,
            ProgrammingActionType.DataProgramming,
            ProgrammingActionType.Coding,
            ProgrammingActionType.IbaDeploy
        };

        private ProgrammingActionState stateProgramming;

		private IEcu parentEcu;

		private ProgrammingActionType type;

		private string channel = string.Empty;

		private string note = string.Empty;

		private bool isEditable;

		private bool isSelected;

		private int order;
	}
}
