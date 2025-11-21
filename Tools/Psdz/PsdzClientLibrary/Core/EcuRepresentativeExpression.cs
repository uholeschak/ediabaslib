using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class EcuRepresentativeExpression : SingleAssignmentExpression
    {
        public EcuRepresentativeExpression()
        {
        }

        public EcuRepresentativeExpression(long ecuRepresentativeId)
        {
            value = ecuRepresentativeId;
        }

        [PreserveSource(Hint = "Modified")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                Log.Warning("EcuRepresentativeExpression.Evaluate()", "vec was null", Array.Empty<object>());
                return false;
            }

            PsdzDatabase.EcuReps ecuRepsById = ClientContext.GetDatabase(vec)?.GetEcuRepsById(this.value.ToString(CultureInfo.InvariantCulture));
            if (ecuRepsById == null)
            {
                return false;
            }

            if (vec.VCI != null && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
            {
                return true;
            }

            if (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && vec.ECU != null && vec.ECU.Count == 0)
            {
                return true;
            }

            bool flag = vec.getECUbyTITLE_ECUTREE(ecuRepsById.EcuShortcut) != null;
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.EcuRepresentatives.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.VALID;
            }

            if (ecus.UnknownEcuRepresentatives.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.MISSING_VARIANT;
            }

            return EEvaluationResult.INVALID;
        }

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            List<long> list = new List<long>();
            if (ecus.UnknownEcuRepresentatives.ToList().BinarySearch(value) >= 0)
            {
                list.Add(value);
            }

            return list;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(18);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "EcuRepresentative=" + value.ToString(CultureInfo.InvariantCulture);
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.EcuReps ecuRepsById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuRepsById(this.value.ToString(CultureInfo.InvariantCulture));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"EcuRepresentative\", ");
            stringBuilder.Append("\"");
            if (ecuRepsById != null)
            {
                stringBuilder.Append(ecuRepsById.EcuShortcut);
            }

            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}