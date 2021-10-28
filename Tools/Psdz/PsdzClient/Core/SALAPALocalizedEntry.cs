using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class SALAPALocalizedEntry : INotifyPropertyChanged, ISalapaLocalizedEntry
	{
		public SALAPALocalizedEntry()
		{
			this.fAHRZEUGARTField = "0";
			this.indexField = 0U;
			this.vERTRIEBSSCHLUESSELField = "0000";
			this.iSO_SPRACHEField = "0";
			this.bENENNUNGField = "0";
		}

		public string FAHRZEUGART
		{
			get
			{
				return this.fAHRZEUGARTField;
			}
			set
			{
				if (this.fAHRZEUGARTField != null)
				{
					if (!this.fAHRZEUGARTField.Equals(value))
					{
						this.fAHRZEUGARTField = value;
						this.OnPropertyChanged("FAHRZEUGART");
						return;
					}
				}
				else
				{
					this.fAHRZEUGARTField = value;
					this.OnPropertyChanged("FAHRZEUGART");
				}
			}
		}

		public uint Index
		{
			get
			{
				return this.indexField;
			}
			set
			{
				if (!this.indexField.Equals(value))
				{
					this.indexField = value;
					this.OnPropertyChanged("Index");
				}
			}
		}

		public string VERTRIEBSSCHLUESSEL
		{
			get
			{
				return this.vERTRIEBSSCHLUESSELField;
			}
			set
			{
				if (this.vERTRIEBSSCHLUESSELField != null)
				{
					if (!this.vERTRIEBSSCHLUESSELField.Equals(value))
					{
						this.vERTRIEBSSCHLUESSELField = value;
						this.OnPropertyChanged("VERTRIEBSSCHLUESSEL");
						return;
					}
				}
				else
				{
					this.vERTRIEBSSCHLUESSELField = value;
					this.OnPropertyChanged("VERTRIEBSSCHLUESSEL");
				}
			}
		}

		public string ISO_SPRACHE
		{
			get
			{
				return this.iSO_SPRACHEField;
			}
			set
			{
				if (this.iSO_SPRACHEField != null)
				{
					if (!this.iSO_SPRACHEField.Equals(value))
					{
						this.iSO_SPRACHEField = value;
						this.OnPropertyChanged("ISO_SPRACHE");
						return;
					}
				}
				else
				{
					this.iSO_SPRACHEField = value;
					this.OnPropertyChanged("ISO_SPRACHE");
				}
			}
		}

		public string BENENNUNG
		{
			get
			{
				return this.bENENNUNGField;
			}
			set
			{
				if (this.bENENNUNGField != null)
				{
					if (!this.bENENNUNGField.Equals(value))
					{
						this.bENENNUNGField = value;
						this.OnPropertyChanged("BENENNUNG");
						return;
					}
				}
				else
				{
					this.bENENNUNGField = value;
					this.OnPropertyChanged("BENENNUNG");
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

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (!(obj is SALAPALocalizedEntry))
			{
				return false;
			}
			SALAPALocalizedEntry salapalocalizedEntry = (SALAPALocalizedEntry)obj;
			return this.Index == salapalocalizedEntry.Index && string.Compare(this.BENENNUNG, salapalocalizedEntry.BENENNUNG, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(this.FAHRZEUGART, salapalocalizedEntry.FAHRZEUGART, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(this.ISO_SPRACHE, salapalocalizedEntry.ISO_SPRACHE, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(this.VERTRIEBSSCHLUESSEL, salapalocalizedEntry.VERTRIEBSSCHLUESSEL, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public override int GetHashCode()
		{
			return this.Index.GetHashCode();
		}

		private string fAHRZEUGARTField;

		private uint indexField;

		private string vERTRIEBSSCHLUESSELField;

		private string iSO_SPRACHEField;

		private string bENENNUNGField;
	}
}
