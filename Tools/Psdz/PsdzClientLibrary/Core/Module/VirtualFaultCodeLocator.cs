using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using System;
using System.Globalization;
using System.Linq;

#pragma warning disable CS0649
namespace BMW.Rheingold.CoreFramework
{
    public class VirtualFaultCodeLocator : IVirtualFaultCodeLocator, IFaultCodeLocator, ISPELocator
    {
        private FaultCode faultCode;

        private bool hasException;

        private Vehicle vehicle;

        private IXepInfoObject rootModule;

        public bool HasException => hasException;

        public ITextContent TextContent
        {
            get
            {
                if (faultCode != null)
                {
                    //[-] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = DatabaseProviderFactory.Instance.GetXepVirtualFaultLabelsByVirtualFaultCodeId(faultCode.ID);
                    //[+] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                    XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                    if (xepVirtualFaultLabelsByVirtualFaultCodeId != null)
                    {
                        return new TextContent(xepVirtualFaultLabelsByVirtualFaultCodeId.Title);
                    }
                }
                return null;
            }
        }

        public ISPELocator[] Children => new SPELocator[0];

        public string DataClassName => "VirtualFaultCode";

        public string[] DataValueNames => new string[8] { "ID", "CODE", "ECUNOANSWER", "VALIDFROM", "VALIDTO", "SICHERHEITSRELEVANT", "WEIGHTING", "PARENTID" };

        public Exception Exception => null;

        public string Id
        {
            get
            {
                if (faultCode == null)
                {
                    return "-1";
                }
                return faultCode.Id;
            }
        }

        public string[] IncomingLinkNames => new string[0];

        public string[] OutgoingLinkNames => new string[0];

        public ISPELocator[] Parents => new ISPELocator[0];

        public decimal SignedId
        {
            get
            {
                if (faultCode == null)
                {
                    return -1m;
                }
                return faultCode.ID;
            }
        }

        public string Code
        {
            get
            {
                if (faultCode == null)
                {
                    return null;
                }
                return faultCode.CODE;
            }
        }

        public VirtualFaultCodeLocator(FaultCode faultCode, Vehicle vehicle, IXepInfoObject rootModule)
        {
            this.faultCode = faultCode;
            this.vehicle = vehicle;
            this.rootModule = rootModule;
        }

        public IDocumentLocator GetDocument()
        {
            try
            {
                return faultCode.GetDocument("FKB-FKB-XML-1");
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCodeLocator.GetDocument()", exception);
            }
            return null;
        }

        public IDocumentLocator GetDocument(string docType)
        {
            if (faultCode != null)
            {
                return faultCode.GetDocument(docType);
            }
            return null;
        }

        public void Set()
        {
            if (faultCode != null && faultCode.Set())
            {
                MarkVirtualFaultForEcu();
            }
        }

        public string GetDataValue(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return SignedId.ToString(CultureInfo.InvariantCulture);
                case "F_SELEKT_CODE":
                case "CODE":
                    return Code;
                case "WEIGHTING":
                    if (faultCode == null || !faultCode.WEIGHTING.HasValue)
                    {
                        return null;
                    }
                    return faultCode.WEIGHTING.ToString();
                case "SICHERHEITSRELEVANT":
                    if (faultCode == null || !faultCode.SICHERHEITSRELEVANT.HasValue)
                    {
                        return null;
                    }
                    return faultCode.SICHERHEITSRELEVANT.ToString();
                case "VALIDTO":
                    if (faultCode == null || !faultCode.VALIDTO.HasValue)
                    {
                        return null;
                    }
                    return faultCode.VALIDTO.ToString();
                case "VALIDFROM":
                    if (faultCode == null || !faultCode.VALIDFROM.HasValue)
                    {
                        return null;
                    }
                    return faultCode.VALIDFROM.ToString();
                case "PARENTID":
                    if (faultCode == null || !faultCode.PARENTID.HasValue)
                    {
                        return null;
                    }
                    return faultCode.PARENTID.ToString();
                default:
                    return string.Empty;
            }
        }

        public T GetDataValue<T>(string name)
        {
            throw new NotImplementedException();
        }

        public ISPELocator[] GetIncomingLinks()
        {
            return new ISPELocator[0];
        }

        public ISPELocator[] GetIncomingLinks(string incomingLinkName)
        {
            return new ISPELocator[0];
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            return new ISPELocator[0];
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            return new ISPELocator[0];
        }

        private void MarkVirtualFaultForEcu()
        {
            if (vehicle.VirtualFaultInfoList.All((VirtualFaultInfo vf) => vf.ServiceProgram.Id != rootModule.Id || vf.AffectedECU != faultCode.ECU))
            {
                vehicle.VirtualFaultInfoList.Add(new VirtualFaultInfo(faultCode.ECU, rootModule));
            }
        }
    }

}
