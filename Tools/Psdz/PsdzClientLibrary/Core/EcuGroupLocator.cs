using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class EcuGroupLocator : ISPELocator, IEcuGroupLocator
	{
		public EcuGroupLocator(PdszDatabase.EcuGroup ecuGroup)
		{
			this.ecuGroup = ecuGroup;
			//this.children = new ISPELocator[0];
            this.parents = null;
        }

		public EcuGroupLocator(decimal id, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
		{
			this.ecuGroup = ClientContext.Database?.GetEcuGroupById(id.ToString(CultureInfo.InvariantCulture));
			//this.children = new ISPELocator[0];
            this.parents = null;
			this.vecInfo = vecInfo;
			this.ffmResolver = ffmResolver;
		}

		public EcuGroupLocator(PdszDatabase.EcuGroup ecuGroup, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
		{
			this.ecuGroup = ecuGroup;
			//this.children = new ISPELocator[0];
            this.parents = null;
			this.vecInfo = vecInfo;
			this.ffmResolver = ffmResolver;
		}
#if false
		public ISPELocator[] Children
		{
			get
			{
				if (this.children != null && this.children.Length != 0)
				{
					return this.children;
				}
				List<ISPELocator> list = new List<ISPELocator>();
				ECU ecubyECU_GRUPPE = this.vecInfo.getECUbyECU_GRUPPE(this.ecuGroup.Name);
				if (ecubyECU_GRUPPE != null && !string.IsNullOrEmpty(ecubyECU_GRUPPE.VARIANTE))
				{
					PdszDatabase.EcuVar ecuVariantByName = ClientContext.Database?.GetEcuVariantByName(ecubyECU_GRUPPE.VARIANTE);
					if (ecuVariantByName != null)
					{
						list.Add(new EcuVariantLocator(ecuVariantByName));
						return list.ToArray();
					}
				}
				List<PdszDatabase.EcuVar> ecuVariantsByEcuGroupId = ClientContext.Database?.GetEcuVariantsByEcuGroupId(this.ecuGroup.Id, this.vecInfo, this.ffmResolver);
				if (ecuVariantsByEcuGroupId != null)
				{
					using (IEnumerator<XEP_ECUVARIANTS> enumerator = ecuVariantsByEcuGroupId.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							XEP_ECUVARIANTS xep_ECUVARIANTS = enumerator.Current;
							if (this.vecInfo != null && this.vecInfo.ECU != null && this.vecInfo.ECU.Count > 0)
							{
								if (this.vecInfo.getECUbyECU_SGBD(xep_ECUVARIANTS.Name) != null)
								{
									list.Add(new EcuVariantLocator(xep_ECUVARIANTS));
									return list.ToArray();
								}
							}
							else if (DatabaseProviderFactory.Instance.EvaluateXepRulesById(xep_ECUVARIANTS.Id, this.vecInfo, this.ffmResolver, null))
							{
								list.Add(new EcuVariantLocator(xep_ECUVARIANTS));
							}
						}
						goto IL_14B;
					}
				}
				IL_14B:
				this.children = list.ToArray();
				return this.children;
			}
		}
#endif
		public string Id
		{
			get
			{
				return this.ecuGroup.Id.ToString(CultureInfo.InvariantCulture);
			}
		}

		public ISPELocator[] Parents
		{
			get
			{
				if (this.parents != null)
				{
					return this.parents;
				}
				return this.parents;
			}
		}

		public string DataClassName
		{
			get
			{
				return "ECUGroup";
			}
		}

		public string[] OutgoingLinkNames
		{
			get
			{
				return new string[0];
			}
		}

		public string[] IncomingLinkNames
		{
			get
			{
				return new string[0];
			}
		}

		public string[] DataValueNames
		{
			get
			{
				return new string[]
				{
					"ID",
					"OBDIDENTIFICATION",
					"FAULTMEMORYDELETEIDENTIFICATIO",
					"FAULTMEMORYDELETEWAITINGTIME",
					"NAME",
					"VIRTUELL",
					"SICHERHEITSRELEVANT",
					"VALIDTO",
					"VALIDFROM"
				};
			}
		}

		public string SignedId
		{
			get
			{
				if (this.ecuGroup == null)
				{
					return string.Empty;
				}
				return this.ecuGroup.Id;
			}
		}

		public Exception Exception
		{
			get
			{
				return null;
			}
		}

		public bool HasException
		{
			get
			{
				return false;
			}
		}
#if false
		public string GetDataValue(string name)
		{
			if (this.ecuGroup != null && !string.IsNullOrEmpty(name))
			{
				string text = name.ToUpperInvariant();
				if (text != null)
				{
					uint num = < PrivateImplementationDetails >.ComputeStringHash(text);
					if (num <= 1458105184U)
					{
						if (num <= 1035818431U)
						{
							if (num != 410075207U)
							{
								if (num == 1035818431U)
								{
									if (text == "FAULTMEMORYDELETEWAITINGTIME")
									{
										return this.ecuGroup.FaultMemoryDeleteWaitingTime.ToString(CultureInfo.InvariantCulture);
									}
								}
							}
							else if (text == "NODECLASS")
							{
								return "5717890";
							}
						}
						else if (num != 1304193470U)
						{
							if (num != 1387956774U)
							{
								if (num == 1458105184U)
								{
									if (text == "ID")
									{
										return this.ecuGroup.Id.ToString(CultureInfo.InvariantCulture);
									}
								}
							}
							else if (text == "NAME")
							{
								return this.ecuGroup.Name;
							}
						}
						else if (text == "OBDIDENTIFICATION")
						{
							return this.ecuGroup.ObdIdentification.ToString(CultureInfo.InvariantCulture);
						}
					}
					else if (num <= 2641724604U)
					{
						if (num != 1944305449U)
						{
							if (num == 2641724604U)
							{
								if (text == "VIRTUELL")
								{
									return this.ecuGroup.Virtuell.ToString(CultureInfo.InvariantCulture);
								}
							}
						}
						else if (text == "VALIDFROM")
						{
							return this.ecuGroup.ValidFrom.ToString();
						}
					}
					else if (num != 2726887280U)
					{
						if (num != 2819690657U)
						{
							if (num == 3895374777U)
							{
								if (text == "FAULTMEMORYDELETEIDENTIFICATIO")
								{
									return this.ecuGroup.FaultMemoryDeleteIdentificatio.ToString(CultureInfo.InvariantCulture);
								}
							}
						}
						else if (text == "SICHERHEITSRELEVANT")
						{
							return this.ecuGroup.Sicherheitsrelevant.ToString(CultureInfo.InvariantCulture);
						}
					}
					else if (text == "VALIDTO")
					{
						return this.ecuGroup.ValidTo.ToString();
					}
				}
				return string.Empty;
			}
			return null;
		}
#endif
		public ISPELocator[] GetIncomingLinks()
		{
			return new ISPELocator[0];
		}

		public ISPELocator[] GetIncomingLinks(string incomingLinkName)
		{
			return this.parents;
		}
#if false
		public ISPELocator[] GetOutgoingLinks()
		{
			return this.children;
		}

		public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
		{
			return this.children;
		}

		public T GetDataValue<T>(string name)
		{
			throw new NotImplementedException();
		}
#endif
		private readonly PdszDatabase.EcuGroup ecuGroup;

		private readonly Vehicle vecInfo;

		private readonly IFFMDynamicResolver ffmResolver;

		//private ISPELocator[] children;

		private ISPELocator[] parents;
	}
}
