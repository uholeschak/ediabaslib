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

		// Note: this type is marked as 'beforefieldinit'.
		static ProgrammingActionData()
		{
			ProgrammingActionType[] array = new ProgrammingActionType[3];
			//RuntimeHelpers.InitializeArray(array, fieldof(< PrivateImplementationDetails >.C452C71638C6C900156C4B50EB7D38764085EC72).FieldHandle);
			ProgrammingActionData.FlashActions = array;
			ProgrammingActionType[] array2 = new ProgrammingActionType[5];
			//RuntimeHelpers.InitializeArray(array2, fieldof(< PrivateImplementationDetails > .78EEF7ACC8767B132B453121E3E6BA4C0A8906C7).FieldHandle);
			ProgrammingActionData.EscalationActions = array2;
		}

		private static readonly ProgrammingActionType[] FlashActions;

		private static readonly ProgrammingActionType[] EscalationActions;

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
