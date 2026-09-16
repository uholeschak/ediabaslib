using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BmwFileReader;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;

namespace PsdzClient.Core;

public class DiagnosticObjectLocator : IDiagnosticObjectLocator, ISPELocator
{
    private DiagnosticObject diagnosticObjectContainer;

    private ISPELocator[] children;

    private ISPELocator[] parents;

    public ISPELocator[] Children
    {
        get
        {
            if (children != null && children.Length != 0)
            {
                return children;
            }
            //[-] ICollection<XEP_DIAGNOSISOBJECTSEX> childDiagObjects = DatabaseProviderFactory.Instance.GetChildDiagObjects(diagnosticObjectContainer.GetXepDiagnosisObject(), diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver, getHidden: true);
            //[+] ICollection<XEP_DIAGNOSISOBJECTSEX> childDiagObjects = XepConverter.Convert(clientContext?.Database?.GetChildDiagObjects(diagnosticObjectContainer.GetXepDiagnosisObject(), diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver, getHidden: true));
            ICollection<XEP_DIAGNOSISOBJECTSEX> childDiagObjects = XepConverter.Convert(clientContext?.Database?.GetChildDiagObjects(diagnosticObjectContainer.GetXepDiagnosisObject(), diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver, getHidden: true));
            int num = 0;
            children = new ISPELocator[childDiagObjects.Count];
            foreach (XEP_DIAGNOSISOBJECTSEX item in childDiagObjects)
            {
                DiagnosticObject diagObj = new DiagnosticObject(item, diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver);
                children[num++] = new DiagnosticObjectLocator(diagObj);
            }
            return children;
        }
    }

    public string Id => diagnosticObjectContainer.GetXepDiagnosisObject().Id.ToString(CultureInfo.InvariantCulture);

    public ISPELocator[] Parents
    {
        get
        {
            if (parents != null && parents.Length != 0)
            {
                return parents;
            }
            int num = 0;
            //[-] ICollection<XEP_DIAGNOSISOBJECTSEX> parentDiagObjects = DatabaseProviderFactory.Instance.GetParentDiagObjects(diagnosticObjectContainer.GetXepDiagnosisObject(), diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver, getHidden: true);
            //[+] ICollection<XEP_DIAGNOSISOBJECTSEX> parentDiagObjects = XepConverter.Convert(clientContext?.Database?.GetParentDiagObjects(diagnosticObjectContainer.GetXepDiagnosisObject(), diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver, getHidden: true));
            ICollection<XEP_DIAGNOSISOBJECTSEX> parentDiagObjects = XepConverter.Convert(clientContext?.Database?.GetParentDiagObjects(diagnosticObjectContainer.GetXepDiagnosisObject(), diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver, getHidden: true));
            parents = new ISPELocator[parentDiagObjects.Count];
            foreach (XEP_DIAGNOSISOBJECTSEX item in parentDiagObjects)
            {
                if (item != null)
                {
                    DiagnosticObject diagObj = new DiagnosticObject(item, diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver);
                    parents[num++] = new DiagnosticObjectLocator(diagObj);
                }
                else
                {
                    Log.Warning("DiagnosticObjectLocator.Parents", "a parent DiagObject was null!");
                }
            }
            return parents;
        }
    }

    public string DataClassName
    {
        get
        {
            XEP_DIAGNOSISOBJECTSEX xepDiagnosisObject = diagnosticObjectContainer.GetXepDiagnosisObject();
            if (xepDiagnosisObject.Nodeclass.HasValue)
            {
                //[-] return DatabaseProviderFactory.Instance.GetXepNodeClassNameById(xepDiagnosisObject.Nodeclass.Value);
                //[+] return clientContext?.Database?.GetXepNodeClassNameById(xepDiagnosisObject.Nodeclass.Value.ToString(CultureInfo.InvariantCulture));
                return clientContext?.Database?.GetXepNodeClassNameById(xepDiagnosisObject.Nodeclass.Value.ToString(CultureInfo.InvariantCulture));
            }
            return string.Empty;
        }
    }

    public string[] OutgoingLinkNames => new string[0];

    public string[] IncomingLinkNames => new string[0];

    public string[] DataValueNames => new string[34]
    {
        "ID", "NODECLASS", "TITLEID", "TITLE_DEDE", "TITLE_ENGB", "TITLE_ENUS", "TITLE_FR", "TITLE_TH", "TITLE_SV", "TITLE_IT",
        "TITLE_ES", "TITLE_ID", "TITLE_KO", "TITLE_EL", "TITLE_TR", "TITLE_ZHCN", "TITLE_RU", "TITLE_NL", "TITLE_PT", "TITLE_ZHTW",
        "TITLE_JA", "TITLE_CSCZ", "TITLE_PLPL", "VERSIONNUMBER", "NAME", "FAILUREWEIGHT", "VERSTECKT", "VALIDFROM", "VALIDTO", "SICHERHEITSRELEVANT",
        "GROBZEICHEN", "HG_NUMMER", "HGUG_NUMMER", "CONTROLID"
    };

    public decimal SignedId
    {
        get
        {
            if (diagnosticObjectContainer == null)
            {
                return -1m;
            }
            return diagnosticObjectContainer.Id;
        }
    }

    public Exception Exception => null;

    public bool HasException => false;

    [PreserveSource(Hint = "No change", SignatureModified = true)]
    public DiagnosticObjectLocator(DiagnosticObject diagObj)
    {
        diagnosticObjectContainer = diagObj;
    }

    [PreserveSource(Hint = "ClientContext added", SignatureModified = true)]
    public DiagnosticObjectLocator(DiagnosticObject diagObj, ICollection<XEP_DIAGNOSISOBJECTSEX> diagChildren, ClientContext clientContext)
    {
        diagnosticObjectContainer = diagObj;
        //[+] this.clientContext = clientContext;
        this.clientContext = clientContext;
        children = new ISPELocator[diagChildren.Count];
        if (diagChildren != null && diagChildren.Count > 0)
        {
            int num = 0;
            children = new ISPELocator[diagChildren.Count];
            {
                foreach (XEP_DIAGNOSISOBJECTSEX diagChild in diagChildren)
                {
                    DiagnosticObject diagObj2 = new DiagnosticObject(diagChild, diagObj.Vehicle, diagObj.FFMResolver);
                    children[num++] = new DiagnosticObjectLocator(diagObj2);
                }
                return;
            }
        }
        children = new ISPELocator[0];
    }

    public string GetDataValue(string name)
    {
        XEP_DIAGNOSISOBJECTSEX xepDiagnosisObject = diagnosticObjectContainer.GetXepDiagnosisObject();
        if (xepDiagnosisObject == null || string.IsNullOrEmpty(name))
        {
            return null;
        }
        switch (name.ToUpperInvariant())
        {
            case "ID":
                return xepDiagnosisObject.Id.ToString(CultureInfo.InvariantCulture);
            case "NODECLASS":
                return xepDiagnosisObject.Nodeclass.ToString();
            case "TITLEID":
                return xepDiagnosisObject.TitleId.ToString();
            case "TITLE_DEDE":
                return xepDiagnosisObject.Title_dede;
            case "TITLE_ENGB":
                return xepDiagnosisObject.Title_engb;
            case "TITLE_ENUS":
                return xepDiagnosisObject.Title_enus;
            case "TITLE_FR":
                return xepDiagnosisObject.Title_fr;
            case "TITLE_TH":
                return xepDiagnosisObject.Title_th;
            case "TITLE_SV":
                return xepDiagnosisObject.Title_sv;
            case "TITLE_IT":
                return xepDiagnosisObject.Title_it;
            case "TITLE_ES":
                return xepDiagnosisObject.Title_es;
            case "TITLE_ID":
                return xepDiagnosisObject.Title_id;
            case "TITLE_KO":
                return xepDiagnosisObject.Title_ko;
            case "TITLE_EL":
                return xepDiagnosisObject.Title_el;
            case "TITLE_TR":
                return xepDiagnosisObject.Title_tr;
            case "TITLE_ZHCN":
                return xepDiagnosisObject.Title_zhcn;
            case "TITLE_RU":
                return xepDiagnosisObject.Title_ru;
            case "TITLE_NL":
                return xepDiagnosisObject.Title_nl;
            case "TITLE_PT":
                return xepDiagnosisObject.Title_pt;
            case "TITLE_ZHTW":
                return xepDiagnosisObject.Title_zhtw;
            case "TITLE_JA":
                return xepDiagnosisObject.Title_ja;
            case "TITLE_CSCZ":
                return xepDiagnosisObject.Title_cscz;
            case "TITLE_PLPL":
                return xepDiagnosisObject.Title_plpl;
            case "VERSIONNUMBER":
                return xepDiagnosisObject.VersionNumber.ToString();
            case "NAME":
                return xepDiagnosisObject.Name;
            case "FAILUREWEIGHT":
                return xepDiagnosisObject.FailureWeight.ToString();
            case "VERSTECKT":
                return xepDiagnosisObject.Versteckt.ToString();
            case "VALIDFROM":
                return xepDiagnosisObject.ValidFrom.ToString();
            case "VALIDTO":
                return xepDiagnosisObject.ValidTo.ToString();
            case "SICHERHEITSRELEVANT":
                return xepDiagnosisObject.SicherheitsRelevant.ToString();
            case "GROBZEICHEN":
                return xepDiagnosisObject.Grobzeichen;
            case "HG_NUMMER":
                return xepDiagnosisObject.Hg_Nummer;
            case "HGUG_NUMMER":
                return xepDiagnosisObject.Hgug_Nummer;
            case "CONTROLID":
                return xepDiagnosisObject.ControlId.ToString();
            case "TITLE":
                return xepDiagnosisObject.Title;
            default:
                return string.Empty;
        }
    }

    [PreserveSource(Hint = "No change", SignatureModified = true)]
    public ISPELocator[] GetIncomingLinks()
    {
        //[-] ICollection<FaultCode> incomingFaultCodesForDiagObject = DatabaseProviderFactory.Instance.GetIncomingFaultCodesForDiagObject(diagnosticObjectContainer.GetXepDiagnosisObject().Id, diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver);
        List<ISPELocator> list = new List<ISPELocator>();
        //[-] if (incomingFaultCodesForDiagObject != null)
        //[-] {
        //[-] foreach (FaultCode item in incomingFaultCodesForDiagObject)
        //[-] {
        //[-] list.Add(new FaultCodeLocator(item, diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver));
        //[-] }
        //[-] }
        return list.ToArray();
    }

    [PreserveSource(Hint = "No change", SignatureModified = true)]
    public ISPELocator[] GetIncomingLinks(string incomingLinkName)
    {
        if ("SUSPICIONLINK".Equals(incomingLinkName, StringComparison.OrdinalIgnoreCase))
        {
            //[-] ICollection<FaultCode> incomingFaultCodesForDiagObject = DatabaseProviderFactory.Instance.GetIncomingFaultCodesForDiagObject(diagnosticObjectContainer.GetXepDiagnosisObject().Id, diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver);
            List<ISPELocator> list = new List<ISPELocator>();
            //[-] foreach (FaultCode item in incomingFaultCodesForDiagObject)
            //[-] {
            //[-] list.Add(new FaultCodeLocator(item, diagnosticObjectContainer.Vehicle, diagnosticObjectContainer.FFMResolver));
            //[-] }
            return list.ToArray();
        }
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

    [PreserveSource(Added = true)]
    private readonly ClientContext clientContext;
}
