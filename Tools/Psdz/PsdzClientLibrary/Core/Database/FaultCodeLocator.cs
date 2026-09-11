using System;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class FaultCodeLocator : IFaultCodeLocator, ISPELocator
    {
        private FaultCode faultCode;

        private Vehicle vecInfo;

        private IFFMDynamicResolver ffmResolver;

        public ISPELocator[] Children => faultCode.Children;

        public string DataClassName => faultCode.DataClassName;

        public string[] DataValueNames => faultCode.DataValueNames;

        public Exception Exception => faultCode.Exception;

        public bool HasException => faultCode.HasException;

        public string Id => faultCode.Id;

        public string[] IncomingLinkNames => faultCode.IncomingLinkNames;

        public string[] OutgoingLinkNames => faultCode.OutgoingLinkNames;

        public ISPELocator[] Parents => faultCode.Parents;

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

        public ITextContent TextContent
        {
            get
            {
                if (faultCode != null && !faultCode.IsVirtualDTC)
                {
                    //[-] XEP_FAULTLABELS xepFaultLabelByFaultCodeId = DatabaseProviderFactory.Instance.GetXepFaultLabelByFaultCodeId(faultCode.ID);
                    //[+] XEP_FAULTLABELS xepFaultLabelByFaultCodeId = null;
                    XEP_FAULTLABELS xepFaultLabelByFaultCodeId = null;
                    if (xepFaultLabelByFaultCodeId != null)
                    {
                        return new TextContent(xepFaultLabelByFaultCodeId.Title);
                    }
                }
                if (faultCode != null && faultCode.IsVirtualDTC)
                {
                    //[-] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = DatabaseProviderFactory.Instance.GetXepVirtualFaultLabelsByVirtualFaultCodeId(faultCode.ID);
                    //[+] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                    XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                    if (xepVirtualFaultLabelsByVirtualFaultCodeId != null)
                    {
                        return new TextContent(xepVirtualFaultLabelsByVirtualFaultCodeId.Title);
                    }
                    //[-] XEP_COMBIFAULTLABELS xepCombiFaultLabelById = DatabaseProviderFactory.Instance.GetXepCombiFaultLabelById(faultCode.ID);
                    //[+] XEP_COMBIFAULTLABELS xepCombiFaultLabelById = null;
                    XEP_COMBIFAULTLABELS xepCombiFaultLabelById = null;
                    if (xepCombiFaultLabelById != null)
                    {
                        return new TextContent(xepCombiFaultLabelById.Title);
                    }
                }
                return new TextContent("na");
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

        public FaultCodeLocator(FaultCode faultCode, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            this.faultCode = faultCode;
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        public string GetDataValue(string name)
        {
            if (faultCode == null)
            {
                return null;
            }
            return faultCode.GetDataValue(name);
        }

        public T GetDataValue<T>(string name)
        {
            return faultCode.GetDataValue<T>(name);
        }

        public IDocumentLocator GetDocument()
        {
            try
            {
                return GetDocument("FKB-FKB-XML-1");
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCodeLocator.GetDocument()", exception);
            }
            return null;
        }

        public IDocumentLocator GetDocument(string docType)
        {
            try
            {
                return faultCode.GetDocument(docType);
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCodeLocator.GetDocument()", exception);
            }
            return null;
        }

        public ISPELocator[] GetIncomingLinks()
        {
            return faultCode.GetIncomingLinks();
        }

        public ISPELocator[] GetIncomingLinks(string incomingLinkName)
        {
            return faultCode.GetIncomingLinks(incomingLinkName);
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            return faultCode.GetOutgoingLinks();
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            return faultCode.GetOutgoingLinks(outgoingLinkName);
        }

        public void Set()
        {
            if (faultCode != null)
            {
                faultCode.Set();
            }
        }
    }
}
