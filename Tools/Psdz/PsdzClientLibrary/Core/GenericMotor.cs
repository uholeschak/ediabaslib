using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class GenericMotor : INotifyPropertyChanged
	{
		public string Engine1
		{
			get
			{
				return this.engine1Field;
			}
			set
			{
				if (this.engine1Field != null)
				{
					if (!this.engine1Field.Equals(value))
					{
						this.engine1Field = value;
						this.OnPropertyChanged("Engine1");
						return;
					}
				}
				else
				{
					this.engine1Field = value;
					this.OnPropertyChanged("Engine1");
				}
			}
		}

		public string Engine2
		{
			get
			{
				return this.engine2Field;
			}
			set
			{
				if (this.engine2Field != null)
				{
					if (!this.engine2Field.Equals(value))
					{
						this.engine2Field = value;
						this.OnPropertyChanged("Engine2");
						return;
					}
				}
				else
				{
					this.engine2Field = value;
					this.OnPropertyChanged("Engine2");
				}
			}
		}

		public string EngineLabel1
		{
			get
			{
				return this.engineLabel1Field;
			}
			set
			{
				if (this.engineLabel1Field != null)
				{
					if (!this.engineLabel1Field.Equals(value))
					{
						this.engineLabel1Field = value;
						this.OnPropertyChanged("EngineLabel1");
						return;
					}
				}
				else
				{
					this.engineLabel1Field = value;
					this.OnPropertyChanged("EngineLabel1");
				}
			}
		}

		public string EngineLabel2
		{
			get
			{
				return this.engineLabel2Field;
			}
			set
			{
				if (this.engineLabel2Field != null)
				{
					if (!this.engineLabel2Field.Equals(value))
					{
						this.engineLabel2Field = value;
						this.OnPropertyChanged("EngineLabel2");
						return;
					}
				}
				else
				{
					this.engineLabel2Field = value;
					this.OnPropertyChanged("EngineLabel2");
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

		private string engine1Field;

		private string engine2Field;

		private string engineLabel1Field;

		private string engineLabel2Field;
	}
}
