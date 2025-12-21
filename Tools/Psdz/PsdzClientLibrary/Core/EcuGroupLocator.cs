using BmwFileReader;
using PsdzClient;
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
        [PreserveSource(Hint = "Cleaned", OriginalHash = "23F7E4420C13B2429379BCD46C3EAADC")]
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

        [PreserveSource(Hint = "Use ConvertToInt", OriginalHash = "4465D41838B9D67581BD145743BEDB74")]
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

        [PreserveSource(Hint = "ecuGroup modified", OriginalHash = "06BD86D992742E8CEB3DF15DD20F7425")]
        public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup)
        {
            this.ecuGroup = ecuGroup;
            this.children = new ISPELocator[0];
        }

        [PreserveSource(Hint = "Database modified", OriginalHash = "062B06F28CEEF71621BB52FF3194BAB9")]
        public EcuGroupLocator(decimal id, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            this.ecuGroup = ClientContext.GetDatabase(vecInfo)?.GetEcuGroupById(id.ToString(CultureInfo.InvariantCulture));
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "ecuGroup modified", OriginalHash = "8EDAAF275E011A7D60865AA2A4E4C118")]
        public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup, Vehicle vecInfo, IFFMDynamicResolverRuleEvaluation ffmResolver)
        {
            this.ecuGroup = ecuGroup;
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "Modified", OriginalHash = "457AEE8771A8A4D2D0BD097120D8DA14")]
        public string GetDataValue(string name)
        {
            if (ecuGroup == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return ecuGroup.Id.ToString(CultureInfo.InvariantCulture);
                case "NODECLASS":
                    return "5717890";
                case "OBDIDENTIFICATION":
                    return ecuGroup.ObdIdent;
                case "FAULTMEMORYDELETEIDENTIFICATIO":
                    return ecuGroup.FaultMemDelIdent;
                case "FAULTMEMORYDELETEWAITINGTIME":
                    return ecuGroup.FaultMemDelWaitTime;
                case "NAME":
                    return ecuGroup.Name;
                case "VIRTUELL":
                    return ecuGroup.Virt;
                case "VALIDFROM":
                    return ecuGroup.ValidFrom;
                case "VALIDTO":
                    return ecuGroup.ValidTo;
                case "SICHERHEITSRELEVANT":
                    return ecuGroup.SafetyRelevant;
                default:
                    return string.Empty;
            }
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