using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class AIF : INotifyPropertyChanged, IAif
	{
		public AIF()
		{
			this.aIF_ANZAHL_PROGField = new int?(0);
		}

		public long? AIF_LAENGE
		{
			get
			{
				return this.aIF_LAENGEField;
			}
			set
			{
				if (this.aIF_LAENGEField != null)
				{
					if (!this.aIF_LAENGEField.Equals(value))
					{
						this.aIF_LAENGEField = value;
						this.OnPropertyChanged("AIF_LAENGE");
						return;
					}
				}
				else
				{
					this.aIF_LAENGEField = value;
					this.OnPropertyChanged("AIF_LAENGE");
				}
			}
		}

		public int? AIF_ADRESSE_HIGH
		{
			get
			{
				return this.aIF_ADRESSE_HIGHField;
			}
			set
			{
				if (this.aIF_ADRESSE_HIGHField != null)
				{
					if (!this.aIF_ADRESSE_HIGHField.Equals(value))
					{
						this.aIF_ADRESSE_HIGHField = value;
						this.OnPropertyChanged("AIF_ADRESSE_HIGH");
						return;
					}
				}
				else
				{
					this.aIF_ADRESSE_HIGHField = value;
					this.OnPropertyChanged("AIF_ADRESSE_HIGH");
				}
			}
		}

		public int? AIF_ADRESSE_LOW
		{
			get
			{
				return this.aIF_ADRESSE_LOWField;
			}
			set
			{
				if (this.aIF_ADRESSE_LOWField != null)
				{
					if (!this.aIF_ADRESSE_LOWField.Equals(value))
					{
						this.aIF_ADRESSE_LOWField = value;
						this.OnPropertyChanged("AIF_ADRESSE_LOW");
						return;
					}
				}
				else
				{
					this.aIF_ADRESSE_LOWField = value;
					this.OnPropertyChanged("AIF_ADRESSE_LOW");
				}
			}
		}

		public string AIF_FG_NR
		{
			get
			{
				return this.aIF_FG_NRField;
			}
			set
			{
				if (this.aIF_FG_NRField != null)
				{
					if (!this.aIF_FG_NRField.Equals(value))
					{
						this.aIF_FG_NRField = value;
						this.OnPropertyChanged("AIF_FG_NR");
						return;
					}
				}
				else
				{
					this.aIF_FG_NRField = value;
					this.OnPropertyChanged("AIF_FG_NR");
				}
			}
		}

		public string AIF_FG_NR_LANG
		{
			get
			{
				return this.aIF_FG_NR_LANGField;
			}
			set
			{
				if (this.aIF_FG_NR_LANGField != null)
				{
					if (!this.aIF_FG_NR_LANGField.Equals(value))
					{
						this.aIF_FG_NR_LANGField = value;
						this.OnPropertyChanged("AIF_FG_NR_LANG");
						return;
					}
				}
				else
				{
					this.aIF_FG_NR_LANGField = value;
					this.OnPropertyChanged("AIF_FG_NR_LANG");
				}
			}
		}

		public string AIF_DATUM
		{
			get
			{
				return this.aIF_DATUMField;
			}
			set
			{
				if (this.aIF_DATUMField != null)
				{
					if (!this.aIF_DATUMField.Equals(value))
					{
						this.aIF_DATUMField = value;
						this.OnPropertyChanged("AIF_DATUM");
						return;
					}
				}
				else
				{
					this.aIF_DATUMField = value;
					this.OnPropertyChanged("AIF_DATUM");
				}
			}
		}

		public string AIF_ZB_NR
		{
			get
			{
				return this.aIF_ZB_NRField;
			}
			set
			{
				if (this.aIF_ZB_NRField != null)
				{
					if (!this.aIF_ZB_NRField.Equals(value))
					{
						this.aIF_ZB_NRField = value;
						this.OnPropertyChanged("AIF_ZB_NR");
						return;
					}
				}
				else
				{
					this.aIF_ZB_NRField = value;
					this.OnPropertyChanged("AIF_ZB_NR");
				}
			}
		}

		public string AIF_SW_NR
		{
			get
			{
				return this.aIF_SW_NRField;
			}
			set
			{
				if (this.aIF_SW_NRField != null)
				{
					if (!this.aIF_SW_NRField.Equals(value))
					{
						this.aIF_SW_NRField = value;
						this.OnPropertyChanged("AIF_SW_NR");
						return;
					}
				}
				else
				{
					this.aIF_SW_NRField = value;
					this.OnPropertyChanged("AIF_SW_NR");
				}
			}
		}

		public string AIF_BEHOERDEN_NR
		{
			get
			{
				return this.aIF_BEHOERDEN_NRField;
			}
			set
			{
				if (this.aIF_BEHOERDEN_NRField != null)
				{
					if (!this.aIF_BEHOERDEN_NRField.Equals(value))
					{
						this.aIF_BEHOERDEN_NRField = value;
						this.OnPropertyChanged("AIF_BEHOERDEN_NR");
						return;
					}
				}
				else
				{
					this.aIF_BEHOERDEN_NRField = value;
					this.OnPropertyChanged("AIF_BEHOERDEN_NR");
				}
			}
		}

		public string AIF_HAENDLER_NR
		{
			get
			{
				return this.aIF_HAENDLER_NRField;
			}
			set
			{
				if (this.aIF_HAENDLER_NRField != null)
				{
					if (!this.aIF_HAENDLER_NRField.Equals(value))
					{
						this.aIF_HAENDLER_NRField = value;
						this.OnPropertyChanged("AIF_HAENDLER_NR");
						return;
					}
				}
				else
				{
					this.aIF_HAENDLER_NRField = value;
					this.OnPropertyChanged("AIF_HAENDLER_NR");
				}
			}
		}

		public string AIF_SERIEN_NR
		{
			get
			{
				return this.aIF_SERIEN_NRField;
			}
			set
			{
				if (this.aIF_SERIEN_NRField != null)
				{
					if (!this.aIF_SERIEN_NRField.Equals(value))
					{
						this.aIF_SERIEN_NRField = value;
						this.OnPropertyChanged("AIF_SERIEN_NR");
						return;
					}
				}
				else
				{
					this.aIF_SERIEN_NRField = value;
					this.OnPropertyChanged("AIF_SERIEN_NR");
				}
			}
		}

		public long? AIF_KM
		{
			get
			{
				return this.aIF_KMField;
			}
			set
			{
				if (this.aIF_KMField != null)
				{
					if (!this.aIF_KMField.Equals(value))
					{
						this.aIF_KMField = value;
						this.OnPropertyChanged("AIF_KM");
						return;
					}
				}
				else
				{
					this.aIF_KMField = value;
					this.OnPropertyChanged("AIF_KM");
				}
			}
		}

		public string AIF_PROG_NR
		{
			get
			{
				return this.aIF_PROG_NRField;
			}
			set
			{
				if (this.aIF_PROG_NRField != null)
				{
					if (!this.aIF_PROG_NRField.Equals(value))
					{
						this.aIF_PROG_NRField = value;
						this.OnPropertyChanged("AIF_PROG_NR");
						return;
					}
				}
				else
				{
					this.aIF_PROG_NRField = value;
					this.OnPropertyChanged("AIF_PROG_NR");
				}
			}
		}

		public int? AIF_ANZ_FREI
		{
			get
			{
				return this.aIF_ANZ_FREIField;
			}
			set
			{
				if (this.aIF_ANZ_FREIField != null)
				{
					if (!this.aIF_ANZ_FREIField.Equals(value))
					{
						this.aIF_ANZ_FREIField = value;
						this.OnPropertyChanged("AIF_ANZ_FREI");
						return;
					}
				}
				else
				{
					this.aIF_ANZ_FREIField = value;
					this.OnPropertyChanged("AIF_ANZ_FREI");
				}
			}
		}

		public int? AIF_ANZAHL_PROG
		{
			get
			{
				return this.aIF_ANZAHL_PROGField;
			}
			set
			{
				if (this.aIF_ANZAHL_PROGField != null)
				{
					if (!this.aIF_ANZAHL_PROGField.Equals(value))
					{
						this.aIF_ANZAHL_PROGField = value;
						this.OnPropertyChanged("AIF_ANZAHL_PROG");
						return;
					}
				}
				else
				{
					this.aIF_ANZAHL_PROGField = value;
					this.OnPropertyChanged("AIF_ANZAHL_PROG");
				}
			}
		}

		public int? AIF_ANZ_DATEN
		{
			get
			{
				return this.aIF_ANZ_DATENField;
			}
			set
			{
				if (this.aIF_ANZ_DATENField != null)
				{
					if (!this.aIF_ANZ_DATENField.Equals(value))
					{
						this.aIF_ANZ_DATENField = value;
						this.OnPropertyChanged("AIF_ANZ_DATEN");
						return;
					}
				}
				else
				{
					this.aIF_ANZ_DATENField = value;
					this.OnPropertyChanged("AIF_ANZ_DATEN");
				}
			}
		}

		public int? AIF_GROESSE
		{
			get
			{
				return this.aIF_GROESSEField;
			}
			set
			{
				if (this.aIF_GROESSEField != null)
				{
					if (!this.aIF_GROESSEField.Equals(value))
					{
						this.aIF_GROESSEField = value;
						this.OnPropertyChanged("AIF_GROESSE");
						return;
					}
				}
				else
				{
					this.aIF_GROESSEField = value;
					this.OnPropertyChanged("AIF_GROESSE");
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

		private long? aIF_LAENGEField;

		private int? aIF_ADRESSE_HIGHField;

		private int? aIF_ADRESSE_LOWField;

		private string aIF_FG_NRField;

		private string aIF_FG_NR_LANGField;

		private string aIF_DATUMField;

		private string aIF_ZB_NRField;

		private string aIF_SW_NRField;

		private string aIF_BEHOERDEN_NRField;

		private string aIF_HAENDLER_NRField;

		private string aIF_SERIEN_NRField;

		private long? aIF_KMField;

		private string aIF_PROG_NRField;

		private int? aIF_ANZ_FREIField;

		private int? aIF_ANZAHL_PROGField;

		private int? aIF_ANZ_DATENField;

		private int? aIF_GROESSEField;
	}
}
