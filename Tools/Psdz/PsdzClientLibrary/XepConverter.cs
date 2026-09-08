using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BmwFileReader;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;

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

    public static XEP_ECUGROUPS Convert(PsdzDatabase.EcuGroup ecuGroup)
    {
        if (ecuGroup == null)
        {
            return null;
        }

        XEP_ECUGROUPS xepEcuGroup = new XEP_ECUGROUPS();
        xepEcuGroup.Id = ecuGroup.Id.ConvertToInt();
        xepEcuGroup.ObdIdentification = ecuGroup.ObdIdent.ConvertToInt();
        xepEcuGroup.FaultMemoryDeleteIdentificatio = ecuGroup.FaultMemDelIdent.ConvertToInt();
        xepEcuGroup.FaultMemoryDeleteWaitingTime = ecuGroup.FaultMemDelWaitTime.ConvertToInt();
        xepEcuGroup.Name = ecuGroup.Name;
        xepEcuGroup.Virtuell = ecuGroup.Virt.ConvertToInt();
        xepEcuGroup.Sicherheitsrelevant = ecuGroup.SafetyRelevant.ConvertToInt();
        xepEcuGroup.ValidFrom = ConvertToDateTime(ecuGroup.ValidFrom);
        xepEcuGroup.ValidTo = ConvertToDateTime(ecuGroup.ValidTo);
        xepEcuGroup.DiagnosticAddress = ecuGroup.DiagAddr.ConvertToInt();

        return xepEcuGroup;
    }

    public static List<XEP_ECUGROUPS> Convert(List<PsdzDatabase.EcuGroup> ecuGroupList)
    {
        if (ecuGroupList == null)
        {
            return null;
        }

        List<XEP_ECUGROUPS> xepEcuGroupList = new List<XEP_ECUGROUPS>();
        foreach (PsdzDatabase.EcuGroup ecuGroup in ecuGroupList)
        {
            xepEcuGroupList.Add(Convert(ecuGroup));
        }
        return xepEcuGroupList;
    }

    public static XEP_ECUVARIANTS Convert(PsdzDatabase.EcuVar ecuVar)
    {
        if (ecuVar == null)
        {
            return null;
        }

        XEP_ECUVARIANTS xepEcuVariant = new XEP_ECUVARIANTS();
        xepEcuVariant.Id = ecuVar.Id.ConvertToInt();
        xepEcuVariant.FaultMemoryDeleteWaitingTime = ecuVar.FaultMemDelWaitTime.ConvertToInt();
        xepEcuVariant.Name = ecuVar.Name;
        xepEcuVariant.EcuGroupId = ecuVar.EcuGroupId.ConvertToInt();
        xepEcuVariant.ValidFrom = ConvertToDateTime(ecuVar.ValidFrom);
        xepEcuVariant.ValidTo = ConvertToDateTime(ecuVar.ValidTo);
        xepEcuVariant.Sicherheitsrelevant = ecuVar.SafetyRelevant.ConvertToInt();
        xepEcuVariant.EcuGroupId = ecuVar.EcuGroupId.ConvertToInt();
        xepEcuVariant.Sort = ecuVar.Sort.ConvertToInt();

        xepEcuVariant.Title_dede = ecuVar.EcuTranslation.TextDe;
        xepEcuVariant.Title_engb = ecuVar.EcuTranslation.TextEn;
        xepEcuVariant.Title_enus = ecuVar.EcuTranslation.TextUs;
        xepEcuVariant.Title_fr = ecuVar.EcuTranslation.TextFr;
        xepEcuVariant.Title_th = ecuVar.EcuTranslation.TextTh;
        xepEcuVariant.Title_sv = ecuVar.EcuTranslation.TextSv;
        xepEcuVariant.Title_it = ecuVar.EcuTranslation.TextIt;
        xepEcuVariant.Title_es = ecuVar.EcuTranslation.TextEs;
        xepEcuVariant.Title_id = ecuVar.EcuTranslation.TextId;
        xepEcuVariant.Title_ko = ecuVar.EcuTranslation.TextKo;
        xepEcuVariant.Title_el = ecuVar.EcuTranslation.TextEl;
        xepEcuVariant.Title_tr = ecuVar.EcuTranslation.TextTr;
        xepEcuVariant.Title_zhcn = ecuVar.EcuTranslation.TextZh;
        xepEcuVariant.Title_zhtw = ecuVar.EcuTranslation.TextZh;
        xepEcuVariant.Title_ru = ecuVar.EcuTranslation.TextRu;
        xepEcuVariant.Title_nl = ecuVar.EcuTranslation.TextNl;
        xepEcuVariant.Title_pt = ecuVar.EcuTranslation.TextPt;
        xepEcuVariant.Title_ja = ecuVar.EcuTranslation.TextJa;
        xepEcuVariant.Title_cscz = ecuVar.EcuTranslation.TextCs;
        xepEcuVariant.Title_plpl = ecuVar.EcuTranslation.TextPl;
        return xepEcuVariant;
    }

    public static List<XEP_ECUVARIANTS> Convert(List<PsdzDatabase.EcuVar> ecuVarList)
    {
        if (ecuVarList == null)
        {
            return null;
        }

        List<XEP_ECUVARIANTS> xepEcuVariantList = new List<XEP_ECUVARIANTS>();
        foreach (PsdzDatabase.EcuVar ecuVar in ecuVarList)
        {
            xepEcuVariantList.Add(Convert(ecuVar));
        }
        return xepEcuVariantList;
    }

    private static DateTime ConvertToDateTime(string text, DateTime? defaultValue = null)
    {
        if (!string.IsNullOrWhiteSpace(text) &&
            DateTime.TryParse(text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
        {
            return result;
        }

        return defaultValue ?? DateTime.MinValue;
    }
}
