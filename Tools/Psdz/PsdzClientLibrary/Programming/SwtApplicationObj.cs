using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.API
{
	[DataContract]
	internal class SwtApplicationObj : ISwtApplication, INotifyPropertyChanged, ISwtApplicationReport
	{
		internal SwtApplicationObj(ISwtApplicationId id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			this.Id = id;
		}

		public int DiagAddrAsInt
		{
			get
			{
				return this.diagAddrAsInt;
			}
			set
			{
				this.diagAddrAsInt = value;
				this.OnPropertyChanged("DiagAddrAsInt");
			}
		}

		public byte[] Fsc
		{
			get
			{
				return this.fsc;
			}
			internal set
			{
				this.fsc = value;
				this.OnPropertyChanged("Fsc");
			}
		}

		public byte[] FscCertificate
		{
			get
			{
				return this.fscCertificate;
			}
			internal set
			{
				this.fscCertificate = value;
				this.OnPropertyChanged("FscCertificate");
			}
		}

		public FscCertificateState FscCertificateState
		{
			get
			{
				return this.fscCertificateState;
			}
			internal set
			{
				this.fscCertificateState = value;
				this.OnPropertyChanged("FscCertificateState");
			}
		}

		public FscState FscState
		{
			get
			{
				return this.fscState;
			}
			internal set
			{
				this.fscState = value;
				this.OnPropertyChanged("FscState");
			}
		}

		public ISwtApplicationId Id
		{
			get
			{
				return this.id;
			}
			private set
			{
				this.id = value;
				this.OnPropertyChanged("Id");
			}
		}

		public string Title
		{
			get
			{
				return this.title;
			}
			set
			{
				this.title = value;
				this.OnPropertyChanged("Title");
			}
		}

		public IDictionary<string, string> TitleDictionary { get; set; }

		public bool IsBackupPossible
		{
			get
			{
				return this.isBackupPossible;
			}
			internal set
			{
				this.isBackupPossible = value;
				this.OnPropertyChanged("IsBackupPossible");
			}
		}

		public int Position
		{
			get
			{
				return this.position;
			}
			internal set
			{
				this.position = value;
				this.OnPropertyChanged("Position");
			}
		}

		public SwtActionType? SwtActionType
		{
			get
			{
				return this.swtActionType;
			}
			internal set
			{
				this.swtActionType = value;
				this.OnPropertyChanged("SwtActionType");
			}
		}

		public SwtType SwtType
		{
			get
			{
				return this.swtType;
			}
			internal set
			{
				this.swtType = value;
				this.OnPropertyChanged("SwtType");
			}
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
		private int diagAddrAsInt;

		[DataMember]
		private byte[] fsc;

		[DataMember]
		private byte[] fscCertificate;

		[DataMember]
		private FscCertificateState fscCertificateState;

		[DataMember]
		private FscState fscState;

		[DataMember]
		private ISwtApplicationId id;

		[DataMember]
		private string title;

		[DataMember]
		private bool isBackupPossible;

		[DataMember]
		private int position;

		[DataMember]
		private SwtActionType? swtActionType;

		[DataMember]
		private SwtType swtType;
	}
}
