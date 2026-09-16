using BmwFileReader;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClientLibrary;

namespace PsdzClient.Core;

public class DiagnosticObject
{
    private readonly IFFMDynamicResolver ffmResolver;

    private readonly Vehicle vehicle;

    private readonly XEP_DIAGNOSISOBJECTSEX diagnosisObject;

    private ICollection<IXepInfoObject> infoObjects;

    public IFFMDynamicResolver FFMResolver => ffmResolver;

    public string Title
    {
        get
        {
            string text;
            switch (ConfigSettings.CurrentUICulture)
            {
                case "de-DE":
                    text = diagnosisObject.Title_dede;
                    break;
                case "en-GB":
                    text = diagnosisObject.Title_engb;
                    break;
                case "en-US":
                    text = diagnosisObject.Title_enus;
                    break;
                case "fr-FR":
                    text = diagnosisObject.Title_fr;
                    break;
                case "es-ES":
                    text = diagnosisObject.Title_es;
                    break;
                case "th-TH":
                    text = diagnosisObject.Title_th;
                    break;
                case "tr-TR":
                    text = diagnosisObject.Title_tr;
                    break;
                case "el-GR":
                    text = diagnosisObject.Title_el;
                    break;
                case "ja-JP":
                    text = diagnosisObject.Title_ja;
                    break;
                case "ru-RU":
                    text = diagnosisObject.Title_ru;
                    break;
                case "it-IT":
                    text = diagnosisObject.Title_it;
                    break;
                case "nl-NL":
                    text = diagnosisObject.Title_nl;
                    break;
                case "pl-PL":
                    text = diagnosisObject.Title_plpl;
                    break;
                case "cs-CZ":
                    text = diagnosisObject.Title_cscz;
                    break;
                case "pt-PT":
                    text = diagnosisObject.Title_pt;
                    break;
                case "sv-SE":
                    text = diagnosisObject.Title_sv;
                    break;
                case "zh-CN":
                    text = diagnosisObject.Title_zhcn;
                    break;
                case "zh-TW":
                    text = diagnosisObject.Title_zhtw;
                    break;
                case "ko-KR":
                    text = diagnosisObject.Title_ko;
                    break;
                default:
                    Log.Warning("DiagnosticObject.get_Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                    text = diagnosisObject.Title_engb;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                return diagnosisObject.Title_engb;
            }
            return text;
        }
    }

    public Vehicle Vehicle => vehicle;

    public decimal Id => diagnosisObject.Id;

    public decimal? ControlId => diagnosisObject.ControlId;

    public DiagnosticObject()
    {
    }

    public DiagnosticObject(XEP_DIAGNOSISOBJECTSEX diagnosticObjectContainer, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
    {
        this.vehicle = vehicle;
        ffmResolver = ffmDynamicResolver;
        diagnosisObject = new XEP_DIAGNOSISOBJECTSEX(diagnosticObjectContainer);
    }

    public ICollection<IXepInfoObject> GetAttachedInfoObjects()
    {
        if (infoObjects != null)
        {
            return infoObjects;
        }
        //[-] infoObjects = DatabaseProviderFactory.Instance.GetInfoObjectsForDiagObject(GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
        //[+] infoObjects = ClientContext.GetClientContext(vehicle).Database.GetInfoObjectsForDiagObject(GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
        infoObjects = ClientContext.GetClientContext(vehicle).Database.GetInfoObjectsForDiagObject(GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
        return infoObjects;
    }

    public XEP_DIAGNOSISOBJECTSEX GetXepDiagnosisObject()
    {
        return new XEP_DIAGNOSISOBJECTSEX(diagnosisObject);
    }
}
