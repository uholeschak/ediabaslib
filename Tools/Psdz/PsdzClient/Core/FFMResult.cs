using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class FFMResult : INotifyPropertyChanged, IFfmResult
	{
		public FFMResult()
		{
			this.reEvaluationNeededField = false;
		}

		public decimal ID
		{
			get
			{
				return this.idField;
			}
			set
			{
				if (!this.idField.Equals(value))
				{
					this.idField = value;
					this.OnPropertyChanged("ID");
				}
			}
		}

		public string Name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				if (this.nameField != null)
				{
					if (!this.nameField.Equals(value))
					{
						this.nameField = value;
						this.OnPropertyChanged("Name");
						return;
					}
				}
				else
				{
					this.nameField = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		public bool? Result
		{
			get
			{
				return this.resultField;
			}
			set
			{
				if (this.resultField != null)
				{
					if (!this.resultField.Equals(value))
					{
						this.resultField = value;
						this.OnPropertyChanged("Result");
						return;
					}
				}
				else
				{
					this.resultField = value;
					this.OnPropertyChanged("Result");
				}
			}
		}

		public string Evaluation
		{
			get
			{
				return this.evaluationField;
			}
			set
			{
				if (this.evaluationField != null)
				{
					if (!this.evaluationField.Equals(value))
					{
						this.evaluationField = value;
						this.OnPropertyChanged("Evaluation");
						return;
					}
				}
				else
				{
					this.evaluationField = value;
					this.OnPropertyChanged("Evaluation");
				}
			}
		}

		public bool ReEvaluationNeeded
		{
			get
			{
				return this.reEvaluationNeededField;
			}
			set
			{
				if (!this.reEvaluationNeededField.Equals(value))
				{
					this.reEvaluationNeededField = value;
					this.OnPropertyChanged("ReEvaluationNeeded");
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

		public FFMResult(string name, bool? result)
		{
			this.ID = -1m;
			this.Name = name;
			this.Result = result;
		}

		public FFMResult(decimal id, string name, bool? result)
		{
			this.ID = id;
			this.Name = name;
			this.Result = result;
		}

		public FFMResult(decimal id, string name, string eval, bool? result)
		{
			this.ID = id;
			this.Name = name;
			this.Result = result;
			this.Evaluation = eval;
		}

		public FFMResult(decimal id, string name, string eval, bool? result, bool reeval)
		{
			this.ID = id;
			this.Name = name;
			this.Result = result;
			this.Evaluation = eval;
			this.ReEvaluationNeeded = reeval;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "FFMResult ID: {0} Name: {1} Result: {2} EvalutedBy: {3} Reeval: {4}", new object[]
			{
				this.ID,
				this.Name,
				this.Result,
				this.Evaluation,
				this.ReEvaluationNeeded
			});
		}

		private decimal idField;

		private string nameField;

		private bool? resultField;

		private string evaluationField;

		private bool reEvaluationNeededField;
	}
}
