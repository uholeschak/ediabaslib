using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class EcuProgrammingVariantExpression : SingleAssignmentExpression
	{
		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			bool result;
			try
			{
				if (vec == null)
				{
					//Log.Warning("EcuProgrammingVariantExpression.Evaluate()", "vec was null", Array.Empty<object>());
					result = false;
				}
				else
				{
					IDatabaseProvider instance = DatabaseProviderFactory.Instance;
					this.programmingVariant = instance.GetEcuProgrammingVariantById(this.value, vec, ffmResolver);
					if (this.programmingVariant == null)
					{
						result = false;
					}
					else
					{
						this.ecuVariant = instance.GetEcuVariantById(this.programmingVariant.EcuVariantId);
						if (this.ecuVariant == null)
						{
							result = false;
						}
						else if ((from c in vec.ECU
								  where c.ProgrammingVariantName != null && c.VARIANTE != null && c.ProgrammingVariantName.Equals(this.programmingVariant.Name, StringComparison.OrdinalIgnoreCase) && c.VARIANTE.Equals(this.ecuVariant.Name, StringComparison.OrdinalIgnoreCase)
								  select c).Any<ECU>())
						{
							result = true;
						}
						else
						{
							result = false;
						}
					}
				}
			}
			catch (Exception exception)
			{
				//Log.WarningException("EcuProgrammingVariantExpression.Evaluate()", exception);
				result = false;
			}
			finally
			{
				//Log.Info(Log.CurrentMethod(), this.ToString(), Array.Empty<object>());
			}
			return result;
		}

		public override string ToString()
		{
			if (this.programmingVariant != null && this.ecuVariant != null)
			{
				return "EcuProgrammingVariant: ProgrammingVariantName= " + this.programmingVariant.Name + " And VARIANTE= " + this.ecuVariant.Name;
			}
			return string.Format("EcuProgrammingVariant: ID= {0}", this.value);
		}

		private XEP_ECUPROGRAMMINGVARIANT programmingVariant;

		private XEP_ECUVARIANTS ecuVariant;
	}
}
