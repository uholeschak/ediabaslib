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
    public class EcuGroupExpression : SingleAssignmentExpression
    {
        public EcuGroupExpression()
        {
        }

        public EcuGroupExpression(long ecuGroupId)
        {
            value = ecuGroupId;
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "0D69BFC6F27343587858C90B70887CA2")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                ruleEvaluationServices.Logger.Warning("EcuGroupExpression.Evaluate()", "vec was null");
                return false;
            }

            PsdzDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(vec)?.GetEcuGroupById(value.ToString(CultureInfo.InvariantCulture));
            if (ecuGroupById == null || string.IsNullOrEmpty(ecuGroupById.Name))
            {
                ruleEvaluationServices.Logger.Warning("EcuGroupExpression.Evaluate()", "no valid group information found for id: {0}", value);
                return false;
            }

            if (vec.VCI != null && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
            {
                ruleEvaluationServices.Logger.Info("EcuGroupExpression.Evaluate()", "Infosession and manual VIN input => no ECU variant evaluation for {0} due to VehicleIdentLevel: {1}", ecuGroupById.Name, vec.VehicleIdentLevel);
                return true;
            }

            if (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && vec.ECU != null && !vec.ECU.Any())
            {
                ruleEvaluationServices.Logger.Info("EcuGroupExpression.Evaluate()", "Infosession and manual VIN input => no ECU representative evaluation for {0} due to VehicleIdentLevel: {1}", ecuGroupById.Name, vec.VehicleIdentLevel);
                return true;
            }

            bool flag = vec.getECUbyECU_GRUPPE(ecuGroupById.Name) != null;
            ruleEvaluationServices.Logger.Debug("EcuGroupExpression.Evaluate()", "EcuGroupId: {0} (original rule: {1})  result: {2}", value, ecuGroupById.Name, flag);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.EcuGroups.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.VALID;
            }

            if (ecus.UnknownEcuGroups.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.MISSING_VARIANT;
            }

            return EEvaluationResult.INVALID;
        }

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            List<long> list = new List<long>();
            if (ecus.UnknownEcuGroups.ToList().BinarySearch(value) >= 0)
            {
                list.Add(value);
            }

            return list;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(10);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "EcuGroup=" + value.ToString(CultureInfo.InvariantCulture);
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuGroupById(this.value.ToString(CultureInfo.InvariantCulture));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"EcuGroup\", ");
            if (ecuGroupById != null)
            {
                stringBuilder.Append(ecuGroupById.DiagAddr);
            }
            else
            {
                stringBuilder.Append("-1");
            }

            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}