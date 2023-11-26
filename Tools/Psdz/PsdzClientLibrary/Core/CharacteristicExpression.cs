using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
	[Serializable]
	public class CharacteristicExpression : RuleExpression
	{
        [DataContract]
        public enum EnumBrand
        {
            [DataMember]
            BMWBMWiMINI,
            [DataMember]
            BMWBMWi,
            [DataMember]
            BMWiMINI,
            [DataMember]
            BMWMINI,
            [DataMember]
            BMWPKW,
            [DataMember]
            Mini,
            [DataMember]
            RollsRoyce,
            [DataMember]
            BMWMotorrad,
            [DataMember]
            BMWi,
            [DataMember]
            TOYOTA,
            [DataMember]
            Unknown
        }

        public CharacteristicExpression(long dataclassId, long datavalueId, Vehicle vec)
		{
			this.dataclassId = dataclassId;
			this.datavalueId = datavalueId;
            this.vecInfo = vec;
            this.CharacteristicRoot = this.GetCharacteristicRootFromDb();
            this.CharacteristicValue = this.GetCharacteristicValueFromDb();
        }

		public long DataClassId
		{
			get
			{
				return this.dataclassId;
			}
		}

		public long DataValueId
		{
			get
			{
				return this.datavalueId;
			}
		}

		private string CharacteristicRoot
		{
			get
			{
				if (string.IsNullOrEmpty(this.characteristicRoot))
				{
					this.CharacteristicRoot = this.GetCharacteristicRootFromDb();
				}
				return this.characteristicRoot;
			}
			set
			{
				this.characteristicRoot = value;
			}
		}

		private string CharacteristicValue
		{
			get
			{
				if (string.IsNullOrEmpty(this.characteristicValue))
				{
					this.CharacteristicValue = this.GetCharacteristicValueFromDb();
					return this.characteristicValue;
				}
				return this.characteristicValue;
			}
			set
			{
				this.characteristicValue = value;
			}
		}

		public new static CharacteristicExpression Deserialize(Stream ms, Vehicle vec)
		{
			byte[] array = new byte[16];
			ms.Read(array, 0, 16);
			long num = BitConverter.ToInt64(array, 0);
			long num2 = BitConverter.ToInt64(array, 8);
			return new CharacteristicExpression(num, num2, vec);
		}

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            string value = null;
            bool flag;
            if (vec != null && vec.VCI != null && vec.VCI.VCIType != VCIDeviceType.UNKNOWN)
            {
                flag = vec.getISTACharacteristics(dataclassId, out value, datavalueId, internalResult, vec);
            }
            else if (CharacteristicRoot.Equals("Marke"))
            {
                string text;
                switch (ClientContext.GetBrand(vec))
                {
                    default:
                        text = "-";
                        break;
                    case EnumBrand.BMWBMWiMINI:
                        text = "BMW/BMW I/MINI";
                        break;
                    case EnumBrand.BMWBMWi:
                        text = "BMW/BMW I";
                        break;
                    case EnumBrand.BMWiMINI:
                        text = "BMW I/MINI";
                        break;
                    case EnumBrand.BMWMINI:
                        text = "BMW/MINI";
                        break;
                    case EnumBrand.BMWPKW:
                        text = "BMW PKW";
                        break;
                    case EnumBrand.Mini:
                        text = "MINI PKW";
                        break;
                    case EnumBrand.RollsRoyce:
                        text = "ROLLS-ROYCE PKW";
                        break;
                    case EnumBrand.BMWMotorrad:
                        text = "BMW MOTORRAD";
                        break;
                    case EnumBrand.BMWi:
                        text = "BMW I";
                        break;
                    case EnumBrand.TOYOTA:
                        text = "TOYOTA";
                        break;
                }
                value = text;
                flag = string.Compare(text, CharacteristicValue, StringComparison.OrdinalIgnoreCase) == 0 || ((CharacteristicValue == "MINI PKW" || CharacteristicValue == "BMW PKW") && string.Compare(text, "BMW/MINI", StringComparison.OrdinalIgnoreCase) == 0) || (CharacteristicValue.Equals("BMW I", StringComparison.OrdinalIgnoreCase) && string.Compare(text, "BMW/MINI", StringComparison.OrdinalIgnoreCase) == 0) || ((CharacteristicValue == "MINI PKW" || CharacteristicValue == "BMW PKW" || CharacteristicValue.Equals("BMW I", StringComparison.OrdinalIgnoreCase)) && string.Compare(text, "BMW/BMW I/MINI", StringComparison.OrdinalIgnoreCase) == 0) || ((CharacteristicValue == "BMW PKW" || CharacteristicValue.Equals("BMW I", StringComparison.OrdinalIgnoreCase)) && string.Compare(text, "BMW/BMW I", StringComparison.OrdinalIgnoreCase) == 0) || (((CharacteristicValue.Equals("BMW I", StringComparison.OrdinalIgnoreCase) || CharacteristicValue == "MINI PKW") && string.Compare(text, "BMW I/MINI", StringComparison.OrdinalIgnoreCase) == 0) ? true : false);
            }
            else if (!"Sicherheitsrelevant".Equals(CharacteristicRoot, StringComparison.OrdinalIgnoreCase) && !"Sicherheitsfahrzeug".Equals(CharacteristicRoot, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("CharacteristicExpression.Evaluate()", "Failed to evaluate {0} without vehcile context; will answer true", CharacteristicRoot);
                flag = true;
            }
            else
            {
                Log.Info("CharacteristicExpression.Evaluate()", "Failed to evaluate {0} without vehcile context; will answer false for 'Sicherheitsrelevant'", CharacteristicRoot);
                flag = false;
            }
            Log.Debug("CharacteristicExpression.Evaluate()", "rule: {0}={1} result: {2} (session context: {3}) [original rule: {4}={5}]", CharacteristicRoot, CharacteristicValue, flag, value, dataclassId, datavalueId);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			long num;
			if (!baseConfiguration.Characteristics.TryGetValue(this.dataclassId, out num))
			{
				return EEvaluationResult.MISSING_CHARACTERISTIC;
			}
			if (num == this.datavalueId)
			{
				return EEvaluationResult.VALID;
			}
			return EEvaluationResult.INVALID;
		}

		public override long GetExpressionCount()
		{
			return 1L;
		}

		public override long GetMemorySize()
		{
			return 24L;
		}

		public override IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
		{
			List<long> list = new List<long>();
			if (!baseConfiguration.Characteristics.ContainsKey(this.dataclassId))
			{
				list.Add(this.dataclassId);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(17);
			ms.Write(BitConverter.GetBytes(this.dataclassId), 0, 8);
			ms.Write(BitConverter.GetBytes(this.datavalueId), 0, 8);
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"");

            stringBuilder.Append(this.CharacteristicRoot);
            stringBuilder.Append("\", ");

            stringBuilder.Append("\"");
            stringBuilder.Append(this.CharacteristicValue);
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.CharacteristicRoot,
				"=",
				this.CharacteristicValue,
				" [",
				this.dataclassId.ToString(CultureInfo.InvariantCulture),
				"=",
				this.datavalueId.ToString(CultureInfo.InvariantCulture),
				"]"
			});
		}

		private string GetCharacteristicRootFromDb()
		{
			string result = string.Empty;
            PsdzDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetDatabase(this.vecInfo)?.GetCharacteristicRootsById(this.dataclassId.ToString(CultureInfo.InvariantCulture));
			if (characteristicRootsById != null && !string.IsNullOrEmpty(characteristicRootsById.EcuTranslation.TextDe))
			{
				result = characteristicRootsById.EcuTranslation.TextDe;
			}
			return result;
		}

		private string GetCharacteristicValueFromDb()
		{
			return ClientContext.GetDatabase(this.vecInfo)?.LookupVehicleCharDeDeById(this.datavalueId.ToString(CultureInfo.InvariantCulture));
		}

		private readonly long dataclassId;

		private readonly long datavalueId;

		private string characteristicRoot;

		private string characteristicValue;

        private Vehicle vecInfo;
    }
}
