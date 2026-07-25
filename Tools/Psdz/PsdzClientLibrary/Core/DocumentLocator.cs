using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System;
using System.Globalization;

namespace PsdzClient.Core;

public class DocumentLocator : IDocumentLocator, ISPELocator
{
    private readonly InfoObject infoObject;

    public ISPELocator[] Children
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public ISPELocator[] Parents
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public decimal SignedId
    {
        get
        {
            //[-] if (infoObject.XepInfoObject == null)
            {
                return -1m;
            }
            //[-] return infoObject.XepInfoObject.Id;
        }
    }

    public string DataClassName
    {
        get
        {
            //[-] if (infoObject.XepInfoObject.Nodeclass.HasValue)
            //[-] {
            //[-] return DatabaseProviderFactory.Instance.GetXepNodeClassNameById(infoObject.XepInfoObject.Nodeclass.Value);
            //[-] }
            return string.Empty;
        }
    }

    public string[] OutgoingLinkNames
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public string[] IncomingLinkNames
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public string Id => SignedId.ToString(CultureInfo.InvariantCulture);

    public string[] DataValueNames
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public Exception Exception
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public bool HasException
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public DocumentLocator(InfoObject infoObject)
    {
        this.infoObject = infoObject;
        if (infoObject == null)
        {
            throw new ArgumentNullException("infoObject");
        }
    }

    public ISPELocator[] GetIncomingLinks()
    {
        throw new NotImplementedException();
    }

    public ISPELocator[] GetIncomingLinks(string incomingLinkName)
    {
        throw new NotImplementedException();
    }

    public ISPELocator[] GetOutgoingLinks()
    {
        throw new NotImplementedException();
    }

    public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
    {
        throw new NotImplementedException();
    }

    public string GetDataValue(string name)
    {
        throw new NotImplementedException();
    }

    public T GetDataValue<T>(string name)
    {
        return default(T);
    }

    public InfoObject GetDocument()
    {
        return infoObject;
    }
}
