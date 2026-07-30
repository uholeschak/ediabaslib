using BmwFileReader;
using System.Collections.Generic;

namespace PsdzClient.Core;

public class DiagnosticObject
{
    private readonly IFFMDynamicResolver ffmResolver;

    private readonly Vehicle vehicle;

    [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
    private readonly PsdzDatabase.SwiDiagObj diagnosisObject;

    [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
    private ICollection<PsdzDatabase.SwiInfoObj> infoObjects;

    public IFFMDynamicResolver FFMResolver => ffmResolver;

    public string Title
    {
        get
        {
            string text;
            switch (ConfigSettings.CurrentUICulture)
            {
                case "de-DE":
                    //[-] text = diagnosisObject.Title_dede;
                    //[+] text = diagnosisObject.EcuTranslation.TextDe;
                    text = diagnosisObject.EcuTranslation.TextDe;
                    break;
                case "en-GB":
                    //[-] text = diagnosisObject.Title_engb;
                    //[+] text = diagnosisObject.EcuTranslation.TextEn;
                    text = diagnosisObject.EcuTranslation.TextEn;
                    break;
                case "en-US":
                    //[-] text = diagnosisObject.Title_enus;
                    //[+] text = diagnosisObject.EcuTranslation.TextUs;
                    text = diagnosisObject.EcuTranslation.TextUs;
                    break;
                case "fr-FR":
                    //[-] text = diagnosisObject.Title_fr;
                    //[+] text = diagnosisObject.EcuTranslation.TextFr;
                    text = diagnosisObject.EcuTranslation.TextFr;
                    break;
                case "es-ES":
                    //[-] text = diagnosisObject.Title_es;
                    //[+] text = diagnosisObject.EcuTranslation.TextEs;
                    text = diagnosisObject.EcuTranslation.TextEs;
                    break;
                case "th-TH":
                    //[-] text = diagnosisObject.Title_th;
                    //[+] text = diagnosisObject.EcuTranslation.TextTh;
                    text = diagnosisObject.EcuTranslation.TextTh;
                    break;
                case "tr-TR":
                    //[-] text = diagnosisObject.Title_tr;
                    //[+] text = diagnosisObject.EcuTranslation.TextTr;
                    text = diagnosisObject.EcuTranslation.TextTr;
                    break;
                case "el-GR":
                    //[-] text = diagnosisObject.Title_el;
                    //[+] text = diagnosisObject.EcuTranslation.TextEl;
                    text = diagnosisObject.EcuTranslation.TextEl;
                    break;
                case "ja-JP":
                    //[-] text = diagnosisObject.Title_ja;
                    //[+] text = diagnosisObject.EcuTranslation.TextJa;
                    text = diagnosisObject.EcuTranslation.TextJa;
                    break;
                case "ru-RU":
                    //[-] text = diagnosisObject.Title_ru;
                    //[+] text = diagnosisObject.EcuTranslation.TextRu;
                    text = diagnosisObject.EcuTranslation.TextRu;
                    break;
                case "it-IT":
                    //[-] text = diagnosisObject.Title_it;
                    //[+] text = diagnosisObject.EcuTranslation.TextIt;
                    text = diagnosisObject.EcuTranslation.TextIt;
                    break;
                case "nl-NL":
                    //[-] text = diagnosisObject.Title_nl;
                    //[+] text = diagnosisObject.EcuTranslation.TextNl;
                    text = diagnosisObject.EcuTranslation.TextNl;
                    break;
                case "pl-PL":
                    //[-] text = diagnosisObject.Title_plpl;
                    //[+] text = diagnosisObject.EcuTranslation.TextPl;
                    text = diagnosisObject.EcuTranslation.TextPl;
                    break;
                case "cs-CZ":
                    //[-] text = diagnosisObject.Title_cscz;
                    //[+] text = diagnosisObject.EcuTranslation.TextCs;
                    text = diagnosisObject.EcuTranslation.TextCs;
                    break;
                case "pt-PT":
                    //[-] text = diagnosisObject.Title_pt;
                    //[+] text = diagnosisObject.EcuTranslation.TextPt;
                    text = diagnosisObject.EcuTranslation.TextPt;
                    break;
                case "sv-SE":
                    //[-] text = diagnosisObject.Title_sv;
                    //[+] text = diagnosisObject.EcuTranslation.TextSv;
                    text = diagnosisObject.EcuTranslation.TextSv;
                    break;
                case "zh-CN":
                    //[-] text = diagnosisObject.Title_zhcn;
                    //[+] text = diagnosisObject.EcuTranslation.TextZh;
                    text = diagnosisObject.EcuTranslation.TextZh;
                    break;
                case "zh-TW":
                    //[-] text = diagnosisObject.Title_zhtw;
                    //[+] text = diagnosisObject.EcuTranslation.TextZh;
                    text = diagnosisObject.EcuTranslation.TextZh;
                    break;
                case "ko-KR":
                    //[-] text = diagnosisObject.Title_ko;
                    //[+] text = diagnosisObject.EcuTranslation.TextKo;
                    text = diagnosisObject.EcuTranslation.TextKo;
                    break;
                default:
                    Log.Warning("DiagnosticObject.get_Title", "CurrentUICulture {0} not available - language set to enGB", ConfigSettings.CurrentUICulture);
                    //[-] text = diagnosisObject.Title_engb;
                    //[+] text = diagnosisObject.EcuTranslation.TextEn;
                    text = diagnosisObject.EcuTranslation.TextEn;
                    break;
            }
            if (string.IsNullOrEmpty(text))
            {
                //[-] return diagnosisObject.Title_engb;
                //[+] return diagnosisObject.EcuTranslation.TextEn;
                return diagnosisObject.EcuTranslation.TextEn;
            }
            return text;
        }
    }

    public Vehicle Vehicle => vehicle;

    [PreserveSource(Hint = "ConvertToInt", Placeholder = true)]
    public decimal Id => diagnosisObject.Id.ConvertToInt();

    [PreserveSource(Hint = "ConvertToInt", Placeholder = true)]
    public decimal? ControlId => diagnosisObject.ControlId.ConvertToInt();

    [PreserveSource(Hint = "No Change", SignatureModified = true)]
    public DiagnosticObject()
    {
    }

    [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", SignatureModified = true)]
    public DiagnosticObject(PsdzDatabase.SwiDiagObj diagnosticObjectContainer, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
    {
        this.vehicle = vehicle;
        ffmResolver = ffmDynamicResolver;
        diagnosisObject = new PsdzDatabase.SwiDiagObj(diagnosticObjectContainer);
    }

    [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
    public ICollection<PsdzDatabase.SwiInfoObj> GetAttachedInfoObjects()
    {
        if (infoObjects != null)
        {
            return infoObjects;
        }
        //[-] infoObjects = DatabaseProviderFactory.Instance.GetInfoObjectsForDiagObject(GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
        infoObjects = ClientContext.GetClientContext(vehicle).Database.GetInfoObjectsForDiagObject(GetXepDiagnosisObject(), Vehicle, FFMResolver, getHidden: true);
        return infoObjects;
    }

    [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
    public PsdzDatabase.SwiDiagObj GetXepDiagnosisObject()
    {
        return new PsdzDatabase.SwiDiagObj(diagnosisObject);
    }
}
