using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

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
            WIESMANN,
            [DataMember]
            MORGAN,
            [DataMember]
            RODING,
            [DataMember]
            PGO,
            [DataMember]
            GIBBS,
            [DataMember]
            BMWi,
            [DataMember]
            TOYOTA,
            [DataMember]
            CAMPAGNA,
            [DataMember]
            ZINORO,
            [DataMember]
            HUSQVARNA,
            [DataMember]
            Brilliance,
            [DataMember]
            YANMAR,
            [DataMember]
            KARMA,
            [DataMember]
            TORQEEDO,
            [DataMember]
            WORKHORSE,
            [DataMember]
            Unknown
        }

        public CharacteristicExpression(long dataclassId, long datavalueId)
		{
			this.dataclassId = dataclassId;
			this.datavalueId = datavalueId;
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

		public new static CharacteristicExpression Deserialize(Stream ms)
		{
			byte[] array = new byte[16];
			ms.Read(array, 0, 16);
			long num = BitConverter.ToInt64(array, 0);
			long num2 = BitConverter.ToInt64(array, 8);
			return new CharacteristicExpression(num, num2);
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			string text = null;
			bool flag;

			if (vec != null && vec.VCI != null)
			{
				if (vec.VCI.VCIType != VCIDeviceType.UNKNOWN)
				{
					flag = vec.getISTACharacteristics(this.dataclassId, out text, this.datavalueId, internalResult);
					return flag;
				}
			}

			if (this.CharacteristicRoot.Equals("Marke"))
			{
				string text2;
				switch (ClientContext.SelectedBrand)
				{
					case EnumBrand.BMWBMWiMINI:
						text2 = "BMW/BMW I/MINI";
						goto IL_133;
					case EnumBrand.BMWBMWi:
						text2 = "BMW/BMW I";
						goto IL_133;
					case EnumBrand.BMWiMINI:
						text2 = "BMW I/MINI";
						goto IL_133;
					case EnumBrand.BMWMINI:
						text2 = "BMW/MINI";
						goto IL_133;
					case EnumBrand.BMWPKW:
						text2 = "BMW PKW";
						goto IL_133;
					case EnumBrand.Mini:
						text2 = "MINI PKW";
						goto IL_133;
					case EnumBrand.RollsRoyce:
						text2 = "ROLLS-ROYCE PKW";
						goto IL_133;
					case EnumBrand.BMWMotorrad:
						text2 = "BMW MOTORRAD";
						goto IL_133;
					case EnumBrand.WIESMANN:
						text2 = "WIESMANN";
						goto IL_133;
					case EnumBrand.MORGAN:
						text2 = "MORGAN";
						goto IL_133;
					case EnumBrand.RODING:
						text2 = "RODING";
						goto IL_133;
					case EnumBrand.PGO:
						text2 = "PGO";
						goto IL_133;
					case EnumBrand.GIBBS:
						text2 = "GIBBS";
						goto IL_133;
					case EnumBrand.BMWi:
						text2 = "BMW I";
						goto IL_133;
					case EnumBrand.TOYOTA:
						text2 = "TOYOTA";
						goto IL_133;
					case EnumBrand.ZINORO:
						text2 = "ZINORO";
						goto IL_133;
				}
				text2 = "-";
				IL_133:
				text = text2;
				if (string.Compare(text2, this.CharacteristicValue, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
				}
				else if ((this.CharacteristicValue == "MINI PKW" || this.CharacteristicValue == "BMW PKW") && string.Compare(text2, "BMW/MINI", StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
				}
				else if (this.CharacteristicValue.Equals("BMW I", StringComparison.OrdinalIgnoreCase) && string.Compare(text2, "BMW/MINI", StringComparison.OrdinalIgnoreCase) == 0 /*&& Dealer.Instance != null && Dealer.Instance.HasLicenseForBrand(new BrandName?(BrandName.BMWi))*/)
				{
					flag = true;
				}
				else
				{
					if ((this.CharacteristicValue == "MINI PKW" || this.CharacteristicValue == "BMW PKW" || this.CharacteristicValue == "BMW I") && string.Compare(text2, "BMW/BMW I/MINI", StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
					if ((this.CharacteristicValue == "BMW PKW" || this.CharacteristicValue == "BMW I") && string.Compare(text2, "BMW/BMW I", StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
					if ((this.CharacteristicValue == "BMW I" || this.CharacteristicValue == "MINI PKW") && string.Compare(text2, "BMW I/MINI", StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
					flag = false;
				}
			}
			else if (!"Sicherheitsrelevant".Equals(this.CharacteristicRoot, StringComparison.OrdinalIgnoreCase) && !"Sicherheitsfahrzeug".Equals(this.CharacteristicRoot, StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
			}
			else
			{
				flag = false;
			}
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
            PdszDatabase.CharacteristicRoots characteristicRootsById = ClientContext.Database?.GetCharacteristicRootsById(this.dataclassId.ToString(CultureInfo.InvariantCulture));
			if (characteristicRootsById != null && !string.IsNullOrEmpty(characteristicRootsById.EcuTranslation.TextDe))
			{
				result = characteristicRootsById.EcuTranslation.TextDe;
			}
			return result;
		}

		private string GetCharacteristicValueFromDb()
		{
			return ClientContext.Database?.LookupVehicleCharDeDeById(this.datavalueId.ToString(CultureInfo.InvariantCulture));
		}

		private readonly long dataclassId;

		private readonly long datavalueId;

		private string characteristicRoot;

		private string characteristicValue;
	}
}
