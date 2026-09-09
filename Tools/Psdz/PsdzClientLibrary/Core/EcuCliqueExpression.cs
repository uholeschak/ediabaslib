using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer;
using PsdzClient;
using PsdzClient.Core.Container;
using PsdzClient.Utility;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace PsdzClient.Core
{
    public class EcuCliqueExpression : SingleAssignmentExpression
    {
        public EcuCliqueExpression()
        {
        }

        public EcuCliqueExpression(long ecuCliqueId)
        {
            value = ecuCliqueId;
        }

        [PreserveSource(Hint = "dataprovider removed", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            bool flag = false;
            //[-] IXepEcuCliques ecuClique = dataProvider.GetEcuClique(value, Language.de_DE);
            //[+] PsdzDatabase database = ClientContext.GetDatabase(vec);
            PsdzDatabase database = ClientContext.GetDatabase(vec);
            //[+] if (database == null)
            if (database == null)
            //[+] {
            {
                //[+] return false;
                return false;
            //[+] }
            }

            //[+] IXepEcuCliques ecuClique = XepConverter.Convert(database.GetEcuClique(this.value.ToString(CultureInfo.InvariantCulture)));
            IXepEcuCliques ecuClique = XepConverter.Convert(database.GetEcuClique(this.value.ToString(CultureInfo.InvariantCulture)));
            if (vec == null)
            {
                return false;
            }

            if (ecuClique == null)
            {
                return true;
            }

            //[-] RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, dataProvider, dealer);
            //[+] RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, database);
            RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, database);
            if (!ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, ecuClique.ID, ffmResolver))
            {
                return false;
            }

            //[-] ICollection<IXepEcuVariants> ecuVariantsByEcuCliquesId = dataProvider.GetEcuVariantsByEcuCliquesId(ecuClique.ID);
            //[+] ICollection<IXepEcuVariants> ecuVariantsByEcuCliquesId = XepConverter.Convert(database.GetEcuVariantsByEcuCliquesId(ecuClique.ID.ToString(CultureInfo.InvariantCulture)));
            ICollection<IXepEcuVariants> ecuVariantsByEcuCliquesId = XepConverter.Convert(database.GetEcuVariantsByEcuCliquesId(ecuClique.ID.ToString(CultureInfo.InvariantCulture)));
            if (ecuVariantsByEcuCliquesId == null || ecuVariantsByEcuCliquesId.Count == 0)
            {
                ruleEvaluationServices.Logger.Info("EcuCliqueExpression.Evaluate()", "Unable to find ECU variants for ECU clique id: {0}", ecuClique.ID);
                return false;
            }

            if (vec.VCI != null && vec.VCI.VCIType == VCIDeviceType.INFOSESSION && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && (vec.ECU == null || (vec.ECU != null && !vec.ECU.Any()))) || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
            {
                foreach (IXepEcuVariants item in ecuVariantsByEcuCliquesId)
                {
                    flag = ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, item.Id, ffmResolver);
                    ruleEvaluationServices.Logger.Info("EcuCliqueExpression.Evaluate()", "Infosession with manual VIN input or basic features => ECU variant/clique evaluation based on other rules for {0} due to VehicleIdentLevel: {1} result: {2}", ecuClique.CLIQUENKURZBEZEICHNUNG, vec.VehicleIdentLevel, flag);
                    if (!flag || !item.EcuGroupId.HasValue)
                    {
                        continue;
                    }

                    decimal? ecuGroupId = item.EcuGroupId;
                    if ((ecuGroupId.GetValueOrDefault() > 0m) & ecuGroupId.HasValue)
                    {
                        flag = ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, item.EcuGroupId.Value, ffmResolver);
                        ruleEvaluationServices.Logger.Info("EcuCliqueExpression.Evaluate()", "ECU variant: {0} was valid, group.Id:{1} evaluation result was: {2}", item.Name, item.EcuGroupId, flag);
                        if (flag)
                        {
                            break;
                        }
                    }
                }

                ruleEvaluationServices.Logger.Debug("EcuCliqueExpression.Evaluate()", "ECU Clique: {0} Result: {1} [original rule: {2}]", ecuClique.CLIQUENKURZBEZEICHNUNG, flag, value);
                return flag;
            }

            foreach (IXepEcuVariants item2 in ecuVariantsByEcuCliquesId)
            {
                flag = VehicleHelper.GetECUbyECU_SGBD(vec, item2.Name) != null;
                if (flag)
                {
                    break;
                }
            }

            ruleEvaluationServices.Logger.Debug("EcuCliqueExpression.Evaluate()", "ECU Clique: {0} Result: {1} [original rule: {2}]", ecuClique.CLIQUENKURZBEZEICHNUNG, flag, value);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.EcuCliques.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.VALID;
            }

            if (ecus.UnknownEcuCliques.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.MISSING_VARIANT;
            }

            return EEvaluationResult.INVALID;
        }

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            List<long> list = new List<long>();
            if (ecus.UnknownEcuCliques.ToList().BinarySearch(value) >= 0)
            {
                list.Add(value);
            }

            return list;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(12);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "EcuClique=" + value.ToString(CultureInfo.InvariantCulture);
        }

        [PreserveSource(Added = true)]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.EcuClique ecuClique = ClientContext.GetDatabase(this.vecInfo)?.GetEcuClique(this.value.ToString(CultureInfo.InvariantCulture));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"EcuClique\", ");
            stringBuilder.Append("\"");
            if (ecuClique != null)
            {
                stringBuilder.Append(ecuClique.CliqueName);
            }

            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}