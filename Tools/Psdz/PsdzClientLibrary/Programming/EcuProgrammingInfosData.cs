using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
	[DataContract]
	internal class EcuProgrammingInfosData : INotifyPropertyChanged, IEcuProgrammingInfosData
	{
		internal EcuProgrammingInfosData()
		{
			this.ecuProgrammingInfosList = new ObservableCollection<IEcuProgrammingInfoData>();
			this.selectedActionData = new ObservableCollection<IProgrammingActionData>();
			this.ecusWithIndividualData = new ObservableCollection<IEcuProgrammingInfoData>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[DataMember]
		public ObservableCollection<IEcuProgrammingInfoData> ECUsWithIndividualData
		{
			get
			{
				return this.ecusWithIndividualData;
			}
			set
			{
				this.ecusWithIndividualData = value;
				this.OnPropertyChanged("ECUsWithIndividualData");
			}
		}

		[DataMember]
		public ObservableCollection<IEcuProgrammingInfoData> List
		{
			get
			{
				return this.ecuProgrammingInfosList;
			}
			set
			{
				this.ecuProgrammingInfosList = value;
				this.OnPropertyChanged("List");
			}
		}

		[DataMember]
		public ObservableCollection<IProgrammingActionData> SelectedActionData
		{
			get
			{
				return this.selectedActionData;
			}
			set
			{
				this.selectedActionData = value;
				this.OnPropertyChanged("SelectedActionData");
			}
		}

		[DataMember]
		public virtual bool SelectionEstablished
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private ObservableCollection<IEcuProgrammingInfoData> ecuProgrammingInfosList;

		private ObservableCollection<IProgrammingActionData> selectedActionData;

		private ObservableCollection<IEcuProgrammingInfoData> ecusWithIndividualData;
	}
}
