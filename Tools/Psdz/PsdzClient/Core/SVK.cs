using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class SVK : INotifyPropertyChanged, ISvk
	{
		[XmlIgnore]
		IEnumerable<string> ISvk.XWE_SGBMID
		{
			get
			{
				return this.XWE_SGBMID;
			}
		}

		public SVK()
		{
			this.xWE_SGBMIDField = new ObservableCollection<string>();
			this.ctordoneField = true;
		}

		public ObservableCollection<string> XWE_SGBMID
		{
			get
			{
				return this.xWE_SGBMIDField;
			}
			set
			{
				if (this.xWE_SGBMIDField != null)
				{
					if (!this.xWE_SGBMIDField.Equals(value))
					{
						this.xWE_SGBMIDField = value;
						this.OnPropertyChanged("XWE_SGBMID");
						return;
					}
				}
				else
				{
					this.xWE_SGBMIDField = value;
					this.OnPropertyChanged("XWE_SGBMID");
				}
			}
		}

		public string PROG_DATUM
		{
			get
			{
				return this.pROG_DATUMField;
			}
			set
			{
				if (this.pROG_DATUMField != null)
				{
					if (!this.pROG_DATUMField.Equals(value))
					{
						this.pROG_DATUMField = value;
						this.OnPropertyChanged("PROG_DATUM");
						return;
					}
				}
				else
				{
					this.pROG_DATUMField = value;
					this.OnPropertyChanged("PROG_DATUM");
				}
			}
		}

		public long? PROG_KM
		{
			get
			{
				return this.pROG_KMField;
			}
			set
			{
				if (this.pROG_KMField != null)
				{
					if (!this.pROG_KMField.Equals(value))
					{
						this.pROG_KMField = value;
						this.OnPropertyChanged("PROG_KM");
						return;
					}
				}
				else
				{
					this.pROG_KMField = value;
					this.OnPropertyChanged("PROG_KM");
				}
			}
		}

		public int? PROG_TEST
		{
			get
			{
				return this.pROG_TESTField;
			}
			set
			{
				if (this.pROG_TESTField != null)
				{
					if (!this.pROG_TESTField.Equals(value))
					{
						this.pROG_TESTField = value;
						this.OnPropertyChanged("PROG_TEST");
						return;
					}
				}
				else
				{
					this.pROG_TESTField = value;
					this.OnPropertyChanged("PROG_TEST");
				}
			}
		}

		[DefaultValue(true)]
		public bool ctordone
		{
			get
			{
				return this.ctordoneField;
			}
			set
			{
				if (!this.ctordoneField.Equals(value))
				{
					this.ctordoneField = value;
					this.OnPropertyChanged("ctordone");
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

		[XmlIgnore]
		public ICollection<int> XWE_PROZESSKLASSE_WERT
		{
			get
			{
				return this.prozessklasseWert;
			}
			set
			{
				this.prozessklasseWert = value;
			}
		}

		private ObservableCollection<string> xWE_SGBMIDField;

		private string pROG_DATUMField;

		private long? pROG_KMField;

		private int? pROG_TESTField;

		private bool ctordoneField;

		private ICollection<int> prozessklasseWert;
	}
}
