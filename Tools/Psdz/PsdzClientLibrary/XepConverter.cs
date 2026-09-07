using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BmwFileReader;
using PsdzClient;

namespace PsdzClientLibrary;

[PreserveSource(Hint = "Custom code", SuppressWarning = true)]
public static class XepConverter
{
    public static XEP_SALAPAS Convert(PsdzDatabase.SaLaPa saLaPa)
    {
        if (saLaPa == null)
        {
            return null;
        }

        XEP_SALAPAS xepSaLaPa = new XEP_SALAPAS();
        xepSaLaPa.Id = saLaPa.Id.ConvertToInt();
        xepSaLaPa.Name = saLaPa.Name;
        xepSaLaPa.ProductType = saLaPa.ProductType;
        xepSaLaPa.Title_dede = saLaPa.EcuTranslation.TextDe;
        xepSaLaPa.Title_engb = saLaPa.EcuTranslation.TextEn;
        xepSaLaPa.Title_enus = saLaPa.EcuTranslation.TextUs;
        xepSaLaPa.Title_fr = saLaPa.EcuTranslation.TextFr;
        xepSaLaPa.Title_th = saLaPa.EcuTranslation.TextTh;
        xepSaLaPa.Title_sv = saLaPa.EcuTranslation.TextSv;
        xepSaLaPa.Title_it = saLaPa.EcuTranslation.TextIt;
        xepSaLaPa.Title_es = saLaPa.EcuTranslation.TextEs;
        xepSaLaPa.Title_id = saLaPa.EcuTranslation.TextId;
        xepSaLaPa.Title_ko = saLaPa.EcuTranslation.TextKo;
        xepSaLaPa.Title_el = saLaPa.EcuTranslation.TextEl;
        xepSaLaPa.Title_tr = saLaPa.EcuTranslation.TextTr;
        xepSaLaPa.Title_zhcn = saLaPa.EcuTranslation.TextZh;
        xepSaLaPa.Title_zhtw = saLaPa.EcuTranslation.TextZh;
        xepSaLaPa.Title_ru = saLaPa.EcuTranslation.TextRu;
        xepSaLaPa.Title_nl = saLaPa.EcuTranslation.TextNl;
        xepSaLaPa.Title_pt = saLaPa.EcuTranslation.TextPt;
        xepSaLaPa.Title_ja = saLaPa.EcuTranslation.TextJa;
        xepSaLaPa.Title_cscz = saLaPa.EcuTranslation.TextCs;
        xepSaLaPa.Title_plpl = saLaPa.EcuTranslation.TextPl;
        return xepSaLaPa;
    }

    public static List<XEP_SALAPAS> Convert(List<PsdzDatabase.SaLaPa> saLaPaList)
    {
        if (saLaPaList == null)
        {
            return null;
        }

        List<XEP_SALAPAS> xepSaLaPaList = new List<XEP_SALAPAS>();
        foreach (PsdzDatabase.SaLaPa saLaPa in saLaPaList)
        {
            xepSaLaPaList.Add(Convert(saLaPa));
        }
        return xepSaLaPaList;
    }
}
