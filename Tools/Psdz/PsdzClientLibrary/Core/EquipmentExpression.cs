using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PsdzClient.Core
{
    public class EquipmentExpression : SingleAssignmentExpression
    {
        private static object evaluationLockObject = new object ();
        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            ILogger logger = ruleEvaluationServices.Logger;
            if (vec == null)
            {
                logger.Error("EquipmentExpression.Evaluate()", "vec was null");
                return false;
            }
            if (vec.VehicleIdentLevel == IdentificationLevel.None)
            {
                return false;
            }

            //[-] if (dataProvider == null)
            //[+] PsdzDatabase database = ClientContext.GetDatabase(vec);
            PsdzDatabase database = ClientContext.GetDatabase(vec);
            //[+] if (database == null)
            if (database == null)
            {
                logger.Error("EquipmentExpression.Evaluate()", "database connection invalid");
                return false;
            }

            //[-] IXepEquipmentRuleEvaluation equipmentById = dataProvider.GetEquipmentById(value);
            //[+] PsdzDatabase.Equipment equipmentById = database.GetEquipmentById(value.ToString(CultureInfo.InvariantCulture));
            PsdzDatabase.Equipment equipmentById = database.GetEquipmentById(value.ToString(CultureInfo.InvariantCulture));
            if (equipmentById == null)
            {
                logger.Warning("EquipmentExpression.Evaluate()", "unable to lookup equipment by id: {0}", value);
                return false;
            }

            lock (evaluationLockObject)
            {
                //[-] bool? flag = vec.hasFFM(equipmentById.NAME);
                //[+] bool? flag = vec.hasFFM(equipmentById.Name);
                bool? flag = vec.hasFFM(equipmentById.Name);
                if (flag.HasValue)
                {
                    //[-] logger.Debug("EquipmentExpression.Evaluate()", "rule: {0} result: {1} [original rule: {2}]", equipmentById.NAME, flag, value);
                    //[+] logger.Debug("EquipmentExpression.Evaluate()", "rule: {0} result: {1} [original rule: {2}]", equipmentById.Name, flag, value);
                    logger.Debug("EquipmentExpression.Evaluate()", "rule: {0} result: {1} [original rule: {2}]", equipmentById.Name, flag, value);
                    return flag.Value;
                }

                //[-] RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, dataProvider, dealer);
                //[+] RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, database);
                RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, database);
                bool flag2 = ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, this.value.ToString(CultureInfo.InvariantCulture), ffmResolver);
                //[-] logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  validity: {2}", equipmentById.NAME, value, flag2);
                //[+] logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  validity: {2}", equipmentById.Name, value, flag2);
                logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  validity: {2}", equipmentById.Name, value, flag2);
                if (ffmResolver != null && flag2)
                {
                    //[-] IEnumerable<IXepInfoObjectRuleEvaluation> infoObjectsByDiagObjectControlId = dataProvider.GetInfoObjectsByDiagObjectControlId(value, vec, ffmResolver, getHidden: true);
                    //[+] List<PsdzDatabase.SwiInfoObj> infoObjectsByDiagObjectControlId = database.GetInfoObjectsByDiagObjectControlId(value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver, getHidden: true, null);
                    List<PsdzDatabase.SwiInfoObj> infoObjectsByDiagObjectControlId = database.GetInfoObjectsByDiagObjectControlId(value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver, getHidden: true, null);
                    if (infoObjectsByDiagObjectControlId == null || !infoObjectsByDiagObjectControlId.Any())
                    {
                        logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: false (due to no fitting test modules found)", equipmentById.Name, value);
                        return false;
                    }

                    bool? flag3 = ffmResolver.Resolve(value, infoObjectsByDiagObjectControlId.First());
                    //[-] vec.AddOrUpdateFFM(new FFMResultRuleEvaluation(equipmentById.ID, equipmentById.NAME, "FFMResolver", flag3, reeval: false));
                    //[+] vec.AddOrUpdateFFM(new FFMResultRuleEvaluation(Convert.ToDecimal(equipmentById.Id), equipmentById.Name, "FFMResolver", flag3, reeval: false));
                    vec.AddOrUpdateFFM(new FFMResultRuleEvaluation(Convert.ToDecimal(equipmentById.Id), equipmentById.Name, "FFMResolver", flag3, reeval: false));
                    if (flag3.HasValue)
                    {
                        //[-] logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: {2}", equipmentById.NAME, value, flag3);
                        //[+] logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: {2}", equipmentById.Name, value, flag3);
                        logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: {2}", equipmentById.Name, value, flag3);
                        return flag3.Value;
                    }

                    //[-] logger.Warning("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: True (due to result is unknown)", equipmentById.NAME, value);
                    //[+] logger.Warning("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: True (due to result is unknown)", equipmentById.Name, value);
                    logger.Warning("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: True (due to result is unknown)", equipmentById.Name, value);
                    return true;
                }

                if (flag2)
                {
                    logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1}) result: true (due to rule evaluation returned true even so ffmResolver result is unknown because ffmResolver was null)", equipmentById.Name, value);
                    return true;
                }

                logger.Info("EquipmentExpression.Evaluate()", "EquipmentId: {0} (original rule: {1})  result: false (due to ffmResolver was null and rule evalauation was false)", equipmentById.Name, value);
                return false;
            }
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.Equipments.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.VALID;
            }

            if (ecus.UnknownEquipments.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.MISSING_VARIANT;
            }

            return EEvaluationResult.INVALID;
        }

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            List<long> list = new List<long>();
            if (ecus.UnknownEquipments.ToList().BinarySearch(value) >= 0)
            {
                list.Add(value);
            }

            return list;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(13);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "Equipment=" + value.ToString(CultureInfo.InvariantCulture);
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.Equipment equipmentById = ClientContext.GetDatabase(this.vecInfo)?.GetEquipmentById(this.value.ToString(CultureInfo.InvariantCulture));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.RuleValidFunc);
            stringBuilder.Append("(");
            if (!formulaConfig.IsRuleValidNumFunc)
            {
                stringBuilder.Append("\"");
            }

            if (equipmentById != null)
            {
                string ruleId = this.value.ToString(CultureInfo.InvariantCulture);
                stringBuilder.Append(ruleId);
                if (formulaConfig.SubRuleIds != null && !formulaConfig.SubRuleIds.Contains(ruleId))
                {
                    formulaConfig.SubRuleIds.Add(ruleId);
                }
            }

            if (!formulaConfig.IsRuleValidNumFunc)
            {
                stringBuilder.Append("\"");
            }

            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}