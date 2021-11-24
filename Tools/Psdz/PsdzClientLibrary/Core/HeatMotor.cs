using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class HeatMotor : INotifyPropertyChanged
	{
		public string DriveId
		{
			get
			{
				return this.driveId;
			}
			set
			{
				if (this.driveId != null)
				{
					if (!this.driveId.Equals(value))
					{
						this.driveId = value;
						this.OnPropertyChanged("DriveId");
						return;
					}
				}
				else
				{
					this.driveId = value;
					this.OnPropertyChanged("DriveId");
				}
			}
		}

		public string HeatMOTBezeichnung
		{
			get
			{
				return this.heatMOTBezeichnungField;
			}
			set
			{
				if (this.heatMOTBezeichnungField != null)
				{
					if (!this.heatMOTBezeichnungField.Equals(value))
					{
						this.heatMOTBezeichnungField = value;
						this.OnPropertyChanged("HeatMOTBezeichnung");
						return;
					}
				}
				else
				{
					this.heatMOTBezeichnungField = value;
					this.OnPropertyChanged("HeatMOTBezeichnung");
				}
			}
		}

		public string HeatMOTBaureihe
		{
			get
			{
				return this.heatMOTBaureiheField;
			}
			set
			{
				if (this.heatMOTBaureiheField != null)
				{
					if (!this.heatMOTBaureiheField.Equals(value))
					{
						this.heatMOTBaureiheField = value;
						this.OnPropertyChanged("HeatMOTBaureihe");
						return;
					}
				}
				else
				{
					this.heatMOTBaureiheField = value;
					this.OnPropertyChanged("HeatMOTBaureihe");
				}
			}
		}

		public string HeatMOTPlatzhalter1
		{
			get
			{
				return this.heatMOTPlatzhalter1Field;
			}
			set
			{
				if (this.heatMOTPlatzhalter1Field != null)
				{
					if (!this.heatMOTPlatzhalter1Field.Equals(value))
					{
						this.heatMOTPlatzhalter1Field = value;
						this.OnPropertyChanged("HeatMOTPlatzhalter1");
						return;
					}
				}
				else
				{
					this.heatMOTPlatzhalter1Field = value;
					this.OnPropertyChanged("HeatMOTPlatzhalter1");
				}
			}
		}

		public string HeatMOTPlatzhalter2
		{
			get
			{
				return this.heatMOTPlatzhalter2Field;
			}
			set
			{
				if (this.heatMOTPlatzhalter2Field != null)
				{
					if (!this.heatMOTPlatzhalter2Field.Equals(value))
					{
						this.heatMOTPlatzhalter2Field = value;
						this.OnPropertyChanged("HeatMOTPlatzhalter2");
						return;
					}
				}
				else
				{
					this.heatMOTPlatzhalter2Field = value;
					this.OnPropertyChanged("HeatMOTPlatzhalter2");
				}
			}
		}

		public string HeatMOTFortlaufendeNum
		{
			get
			{
				return this.heatMOTFortlaufendeNumField;
			}
			set
			{
				if (this.heatMOTFortlaufendeNumField != null)
				{
					if (!this.heatMOTFortlaufendeNumField.Equals(value))
					{
						this.heatMOTFortlaufendeNumField = value;
						this.OnPropertyChanged("HeatMOTFortlaufendeNum");
						return;
					}
				}
				else
				{
					this.heatMOTFortlaufendeNumField = value;
					this.OnPropertyChanged("HeatMOTFortlaufendeNum");
				}
			}
		}

		public string HeatMOTLeistungsklasse
		{
			get
			{
				return this.heatMOTLeistungsklasseField;
			}
			set
			{
				if (this.heatMOTLeistungsklasseField != null)
				{
					if (!this.heatMOTLeistungsklasseField.Equals(value))
					{
						this.heatMOTLeistungsklasseField = value;
						this.OnPropertyChanged("HeatMOTLeistungsklasse");
						return;
					}
				}
				else
				{
					this.heatMOTLeistungsklasseField = value;
					this.OnPropertyChanged("HeatMOTLeistungsklasse");
				}
			}
		}

		public string HeatMOTLebenszyklus
		{
			get
			{
				return this.heatMOTLebenszyklusField;
			}
			set
			{
				if (this.heatMOTLebenszyklusField != null)
				{
					if (!this.heatMOTLebenszyklusField.Equals(value))
					{
						this.heatMOTLebenszyklusField = value;
						this.OnPropertyChanged("HeatMOTLebenszyklus");
						return;
					}
				}
				else
				{
					this.heatMOTLebenszyklusField = value;
					this.OnPropertyChanged("HeatMOTLebenszyklus");
				}
			}
		}

		public string HeatMOTKraftstoffart
		{
			get
			{
				return this.heatMOTKraftstoffartField;
			}
			set
			{
				if (this.heatMOTKraftstoffartField != null)
				{
					if (!this.heatMOTKraftstoffartField.Equals(value))
					{
						this.heatMOTKraftstoffartField = value;
						this.OnPropertyChanged("HeatMOTKraftstoffart");
						return;
					}
				}
				else
				{
					this.heatMOTKraftstoffartField = value;
					this.OnPropertyChanged("HeatMOTKraftstoffart");
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

		private string driveId;

		private string heatMOTBezeichnungField;

		private string heatMOTBaureiheField;

		private string heatMOTPlatzhalter1Field;

		private string heatMOTPlatzhalter2Field;

		private string heatMOTFortlaufendeNumField;

		private string heatMOTLeistungsklasseField;

		private string heatMOTLebenszyklusField;

		private string heatMOTKraftstoffartField;
	}
}
