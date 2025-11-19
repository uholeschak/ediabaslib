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
    public class EcuGroupLocator : IEcuGroupLocator, ISPELocator
    {
        [PreserveSource(Hint = "Modified")]
        private readonly PsdzDatabase.EcuGroup ecuGroup;
        private readonly Vehicle vecInfo;
        private readonly IFFMDynamicResolverRuleEvaluation ffmResolver;
        private readonly ISPELocator[] parents;
        private ISPELocator[] children;
        [PreserveSource(Hint = "Cleaned")]
        public ISPELocator[] Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Id => ecuGroup.Id.ToString(CultureInfo.InvariantCulture);

        public ISPELocator[] Parents
        {
            get
            {
                if (parents != null)
                {
                    _ = parents.LongLength;
                    return parents;
                }

                return parents;
            }
        }

        public string DataClassName => "ECUGroup";
        public string[] OutgoingLinkNames => new string[0];
        public string[] IncomingLinkNames => new string[0];
        public string[] DataValueNames => new string[9]
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

        public Exception Exception => null;
        public bool HasException => false;

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
        public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup, Vehicle vecInfo, IFFMDynamicResolverRuleEvaluation ffmResolver)
        {
            this.ecuGroup = ecuGroup;
            this.children = new ISPELocator[0];
            this.parents = null;
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "Modified")]
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
            return parents;
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            return children;
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            return children;
        }

        public T GetDataValue<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}