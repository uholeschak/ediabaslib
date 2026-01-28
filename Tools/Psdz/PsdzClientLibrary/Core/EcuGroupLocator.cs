using BmwFileReader;
using PsdzClient;
using System;
using System.Globalization;

#pragma warning disable CS0649
namespace PsdzClient.Core
{
    public class EcuGroupLocator : IEcuGroupLocator, ISPELocator
    {
        [PreserveSource(Hint = "Database replaced")]
        private readonly PsdzDatabase.EcuGroup ecuGroup;
        private readonly Vehicle vecInfo;
        private readonly IFFMDynamicResolverRuleEvaluation ffmResolver;
        private readonly ISPELocator[] parents;
        private ISPELocator[] children;
        [PreserveSource(Cleaned = true, OriginalHash = "23F7E4420C13B2429379BCD46C3EAADC")]
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

        public decimal SignedId
        {
            get
            {
                if (ecuGroup == null)
                {
                    return -1m;
                }

                //[-] return ecuGroup.Id;
                //[+] return ecuGroup.Id.ConvertToInt();
                return ecuGroup.Id.ConvertToInt();
            }
        }

        public Exception Exception => null;
        public bool HasException => false;

        [PreserveSource(Hint = "ecuGroup modified", SignatureModified = true)]
        public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup)
        {
            this.ecuGroup = ecuGroup;
            children = new ISPELocator[0];
        }

        [PreserveSource(Hint = "Database modified", SignatureModified = true)]
        public EcuGroupLocator(decimal id, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            //[-] ecuGroup = DatabaseProviderFactory.Instance.GetEcuGroupById(id);
            //[+] ecuGroup = ClientContext.GetDatabase(vecInfo)?.GetEcuGroupById(id.ToString(CultureInfo.InvariantCulture));
            ecuGroup = ClientContext.GetDatabase(vecInfo)?.GetEcuGroupById(id.ToString(CultureInfo.InvariantCulture));
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "ecuGroup modified", SignatureModified = true)]
        public EcuGroupLocator(PsdzDatabase.EcuGroup ecuGroup, Vehicle vecInfo, IFFMDynamicResolverRuleEvaluation ffmResolver)
        {
            this.ecuGroup = ecuGroup;
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

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
                    //[-] return ecuGroup.ObdIdentification.ToString(CultureInfo.InvariantCulture);
                    //[+] return ecuGroup.ObdIdent;
                    return ecuGroup.ObdIdent;
                case "FAULTMEMORYDELETEIDENTIFICATIO":
                    //[-] return ecuGroup.FaultMemoryDeleteIdentificatio.ToString(CultureInfo.InvariantCulture);
                    //[+] return ecuGroup.FaultMemDelIdent;
                    return ecuGroup.FaultMemDelIdent;
                case "FAULTMEMORYDELETEWAITINGTIME":
                    //[-] return ecuGroup.FaultMemoryDeleteWaitingTime.ToString(CultureInfo.InvariantCulture);
                    //[+] return ecuGroup.FaultMemDelWaitTime;
                    return ecuGroup.FaultMemDelWaitTime;
                case "NAME":
                    return ecuGroup.Name;
                case "VIRTUELL":
                    //[-] return ecuGroup.Virtuell.ToString(CultureInfo.InvariantCulture);
                    //[+] return ecuGroup.Virt;
                    return ecuGroup.Virt;
                case "VALIDFROM":
                    //[-] return ecuGroup.ValidFrom.ToString();
                    //[+] return ecuGroup.ValidFrom;
                    return ecuGroup.ValidFrom;
                case "VALIDTO":
                    //[-] return ecuGroup.ValidTo.ToString();
                    //[+] return ecuGroup.ValidTo;
                    return ecuGroup.ValidTo;
                case "SICHERHEITSRELEVANT":
                    //[-] return ecuGroup.Sicherheitsrelevant.ToString(CultureInfo.InvariantCulture);
                    //[+] return ecuGroup.SafetyRelevant;
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

        [PreserveSource(Cleaned = true)]
        public T GetDataValue<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}