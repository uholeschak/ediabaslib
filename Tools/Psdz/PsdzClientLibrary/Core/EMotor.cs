using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class EMotor : INotifyPropertyChanged, IEMotor
    {
		public string EMOTBaureihe
		{
			get
			{
				return this.eMOTBaureiheField;
			}
			set
			{
				if (this.eMOTBaureiheField != null)
				{
					if (!this.eMOTBaureiheField.Equals(value))
					{
						this.eMOTBaureiheField = value;
						this.OnPropertyChanged("EMOTBaureihe");
						return;
					}
				}
				else
				{
					this.eMOTBaureiheField = value;
					this.OnPropertyChanged("EMOTBaureihe");
				}
			}
		}

		public string EMOTArbeitsverfahren
		{
			get
			{
				return this.eMOTArbeitsverfahrenField;
			}
			set
			{
				if (this.eMOTArbeitsverfahrenField != null)
				{
					if (!this.eMOTArbeitsverfahrenField.Equals(value))
					{
						this.eMOTArbeitsverfahrenField = value;
						this.OnPropertyChanged("EMOTArbeitsverfahren");
						return;
					}
				}
				else
				{
					this.eMOTArbeitsverfahrenField = value;
					this.OnPropertyChanged("EMOTArbeitsverfahren");
				}
			}
		}

		public string EMOTDrehmoment
		{
			get
			{
				return this.eMOTDrehmomentField;
			}
			set
			{
				if (this.eMOTDrehmomentField != null)
				{
					if (!this.eMOTDrehmomentField.Equals(value))
					{
						this.eMOTDrehmomentField = value;
						this.OnPropertyChanged("EMOTDrehmoment");
						return;
					}
				}
				else
				{
					this.eMOTDrehmomentField = value;
					this.OnPropertyChanged("EMOTDrehmoment");
				}
			}
		}

		public string EMOTLeistungsklasse
		{
			get
			{
				return this.eMOTLeistungsklasseField;
			}
			set
			{
				if (this.eMOTLeistungsklasseField != null)
				{
					if (!this.eMOTLeistungsklasseField.Equals(value))
					{
						this.eMOTLeistungsklasseField = value;
						this.OnPropertyChanged("EMOTLeistungsklasse");
						return;
					}
				}
				else
				{
					this.eMOTLeistungsklasseField = value;
					this.OnPropertyChanged("EMOTLeistungsklasse");
				}
			}
		}

		public string EMOTUeberarbeitung
		{
			get
			{
				return this.eMOTUeberarbeitungField;
			}
			set
			{
				if (this.eMOTUeberarbeitungField != null)
				{
					if (!this.eMOTUeberarbeitungField.Equals(value))
					{
						this.eMOTUeberarbeitungField = value;
						this.OnPropertyChanged("EMOTUeberarbeitung");
						return;
					}
				}
				else
				{
					this.eMOTUeberarbeitungField = value;
					this.OnPropertyChanged("EMOTUeberarbeitung");
				}
			}
		}

		public string EMOTBezeichnung
		{
			get
			{
				return this.eMOTBezeichnungField;
			}
			set
			{
				if (this.eMOTBezeichnungField != null)
				{
					if (!this.eMOTBezeichnungField.Equals(value))
					{
						this.eMOTBezeichnungField = value;
						this.OnPropertyChanged("EMOTBezeichnung");
						return;
					}
				}
				else
				{
					this.eMOTBezeichnungField = value;
					this.OnPropertyChanged("EMOTBezeichnung");
				}
			}
		}

		public string EMOTKraftstoffart
		{
			get
			{
				return this.eMOTKraftstoffartField;
			}
			set
			{
				if (this.eMOTKraftstoffartField != null)
				{
					if (!this.eMOTKraftstoffartField.Equals(value))
					{
						this.eMOTKraftstoffartField = value;
						this.OnPropertyChanged("EMOTKraftstoffart");
						return;
					}
				}
				else
				{
					this.eMOTKraftstoffartField = value;
					this.OnPropertyChanged("EMOTKraftstoffart");
				}
			}
		}

		public string EMOTEinbaulage
		{
			get
			{
				return this.eMOTEinbaulageField;
			}
			set
			{
				if (this.eMOTEinbaulageField != null)
				{
					if (!this.eMOTEinbaulageField.Equals(value))
					{
						this.eMOTEinbaulageField = value;
						this.OnPropertyChanged("EMOTEinbaulage");
						return;
					}
				}
				else
				{
					this.eMOTEinbaulageField = value;
					this.OnPropertyChanged("EMOTEinbaulage");
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

		private string eMOTBaureiheField;

		private string eMOTArbeitsverfahrenField;

		private string eMOTDrehmomentField;

		private string eMOTLeistungsklasseField;

		private string eMOTUeberarbeitungField;

		private string eMOTBezeichnungField;

		private string eMOTKraftstoffartField;

		private string eMOTEinbaulageField;
	}
}
