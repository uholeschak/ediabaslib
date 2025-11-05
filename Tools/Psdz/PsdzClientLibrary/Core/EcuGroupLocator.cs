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
		public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup)
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

		public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
		{
			this.ecuGroup = ecuGroup;
			//this.children = new ISPELocator[0];
            this.parents = null;
			this.vecInfo = vecInfo;
			this.ffmResolver = ffmResolver;
		}
        // [UH] removed
#if false
        public ISPELocator[] Children
        {
	        get
	        {
                PsdzDatabase database = ClientContext.GetDatabase(this.vecInfo);
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
                    PsdzDatabase.EcuVar ecuVariantByName = database.GetEcuVariantByName(eCUbyECU_GRUPPE.VARIANTE);
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

        public string GetDataValue(string name)
        {
            if (ecuGroup != null && !string.IsNullOrEmpty(name))
            {
                switch (name.ToUpperInvariant())
                {
                    case "FAULTMEMORYDELETEWAITINGTIME":
                        return ecuGroup.FaultMemDelWaitTime;
                    case "NODECLASS":
                        return "5717890";
                    case "ID":
                        return ecuGroup.Id.ToString(CultureInfo.InvariantCulture);
                    case "NAME":
                        return ecuGroup.Name;
                    case "OBDIDENTIFICATION":
                        return ecuGroup.ObdIdent;
                    case "VIRTUELL":
                        return ecuGroup.Virt;
                    case "VALIDFROM":
                        return ecuGroup.ValidFrom;
                    case "FAULTMEMORYDELETEIDENTIFICATIO":
                        return ecuGroup.FaultMemDelIdent;
                    case "SICHERHEITSRELEVANT":
                        return ecuGroup.SafetyRelevant;
                    case "VALIDTO":
                        return ecuGroup.ValidTo;
                    default:
                        return string.Empty;
                }
            }
            return null;
        }
        
        public ISPELocator[] GetIncomingLinks()
		{
			return new ISPELocator[0];
		}

		public ISPELocator[] GetIncomingLinks(string incomingLinkName)
		{
			return this.parents;
		}

        // [UH] removed
#if false
		public ISPELocator[] GetOutgoingLinks()
		{
			return this.children;
		}

		public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
		{
			return this.children;
		}
#endif
        public T GetDataValue<T>(string name)
		{
			throw new NotImplementedException();
		}

		private readonly PsdzDatabase.EcuGroup ecuGroup;

		private readonly Vehicle vecInfo;

		private readonly IFFMDynamicResolver ffmResolver;

        // [UH] removed
        //private ISPELocator[] children;

        private ISPELocator[] parents;
	}
}
