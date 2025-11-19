using BmwFileReader;
using PsdzClientLibrary;
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
        [PreserveSource(Hint = "ecuGroup modified")]
		public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup)
		{
			this.ecuGroup = ecuGroup;
			this.children = new ISPELocator[0];
            this.parents = null;
        }

        [PreserveSource(Hint = "Unmodified")]
		public EcuGroupLocator(decimal id, Vehicle vec, IFFMDynamicResolver ffmResolver)
		{
			this.ecuGroup = ClientContext.GetDatabase(vec)?.GetEcuGroupById(id.ToString(CultureInfo.InvariantCulture));
			this.children = new ISPELocator[0];
            this.parents = null;
			this.vecInfo = vec;
			this.ffmResolver = ffmResolver;
		}

        [PreserveSource(Hint = "ecuGroup modified")]
		public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
		{
			this.ecuGroup = ecuGroup;
			this.children = new ISPELocator[0];
            this.parents = null;
			this.vecInfo = vecInfo;
			this.ffmResolver = ffmResolver;
		}

		[PreserveSource(Hint = "Cleaned")]
        public ISPELocator[] Children
        {
            get
            {
                return children;
            }
        }

        [PreserveSource(Hint = "Modified")]
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

        [PreserveSource(Hint = "Modified")]
        public decimal SignedId
        {
            get
            {
                if (ecuGroup == null)
                {
                    return -1m;
                }
                return ecuGroup.Id.ConvertToInt();
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

        [PreserveSource(Hint = "Modified")]
		private readonly PsdzDatabase.EcuGroup ecuGroup;

		private readonly Vehicle vecInfo;

		private readonly IFFMDynamicResolver ffmResolver;

        private ISPELocator[] children;

        private ISPELocator[] parents;
	}
}
