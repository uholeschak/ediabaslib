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

		public EcuGroupLocator(decimal id, Vehicle vec, IFFMDynamicResolver ffmResolver)
		{
			this.ecuGroup = ClientContext.GetDatabase(vec)?.GetEcuGroupById(id.ToString(CultureInfo.InvariantCulture));
			//this.children = new ISPELocator[0];
            this.parents = null;
			this.vecInfo = vec;
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
                PdszDatabase database = ClientContext.GetDatabase(this.vecInfo);
                if (database == null)
                {
                    return null;
                }

                if (children != null && children.Length != 0)
		        {
			        return children;
		        }
		        List<ISPELocator> list = new List<ISPELocator>();
		        ECU eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE(ecuGroup.Name);
		        if (eCUbyECU_GRUPPE != null && !string.IsNullOrEmpty(eCUbyECU_GRUPPE.VARIANTE))
		        {
                    PdszDatabase.EcuVar ecuVariantByName = database.GetEcuVariantByName(eCUbyECU_GRUPPE.VARIANTE);
                    if (ecuVariantByName != null)
                    {
                        list.Add(new EcuVariantLocator(ecuVariantByName));
                        return list.ToArray();
                    }
		        }
				ICollection<XEP_ECUVARIANTS> ecuVariantsByEcuGroupId = database.GetEcuVariantsByEcuGroupId(ecuGroup.Id, vecInfo, ffmResolver);
		        if (ecuVariantsByEcuGroupId != null)
		        {
			        foreach (XEP_ECUVARIANTS item in ecuVariantsByEcuGroupId)
			        {
				        if (vecInfo != null && vecInfo.ECU != null && vecInfo.ECU.Count > 0)
				        {
					        if (vecInfo.getECUbyECU_SGBD(item.Name) != null)
					        {
						        list.Add(new EcuVariantLocator(item));
						        return list.ToArray();
					        }
				        }
				        else if (database.EvaluateXepRulesById(item.Id, vecInfo, ffmResolver))
				        {
					        list.Add(new EcuVariantLocator(item));
				        }
			        }
		        }
		        children = list.ToArray();
		        return children;
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
