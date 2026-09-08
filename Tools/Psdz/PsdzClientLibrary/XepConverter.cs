using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BmwFileReader;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

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

        CopyEcuTranslation(saLaPa.EcuTranslation, xepSaLaPa);
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

        CopyEcuTranslation(ecuVar.EcuTranslation, xepEcuVariant);
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

    private static readonly (string TitleProperty, string TextProperty)[] TitleMapping =
    {
        ("Title_dede", nameof(PsdzDatabase.EcuTranslation.TextDe)),
        ("Title_engb", nameof(PsdzDatabase.EcuTranslation.TextEn)),
        ("Title_enus", nameof(PsdzDatabase.EcuTranslation.TextUs)),
        ("Title_fr", nameof(PsdzDatabase.EcuTranslation.TextFr)),
        ("Title_th", nameof(PsdzDatabase.EcuTranslation.TextTh)),
        ("Title_sv", nameof(PsdzDatabase.EcuTranslation.TextSv)),
        ("Title_it", nameof(PsdzDatabase.EcuTranslation.TextIt)),
        ("Title_es", nameof(PsdzDatabase.EcuTranslation.TextEs)),
        ("Title_id", nameof(PsdzDatabase.EcuTranslation.TextId)),
        ("Title_ko", nameof(PsdzDatabase.EcuTranslation.TextKo)),
        ("Title_el", nameof(PsdzDatabase.EcuTranslation.TextEl)),
        ("Title_tr", nameof(PsdzDatabase.EcuTranslation.TextTr)),
        ("Title_zhcn", nameof(PsdzDatabase.EcuTranslation.TextZh)),
        ("Title_zhtw", nameof(PsdzDatabase.EcuTranslation.TextZh)),
        ("Title_ru", nameof(PsdzDatabase.EcuTranslation.TextRu)),
        ("Title_nl", nameof(PsdzDatabase.EcuTranslation.TextNl)),
        ("Title_pt", nameof(PsdzDatabase.EcuTranslation.TextPt)),
        ("Title_ja", nameof(PsdzDatabase.EcuTranslation.TextJa)),
        ("Title_cscz", nameof(PsdzDatabase.EcuTranslation.TextCs)),
        ("Title_plpl", nameof(PsdzDatabase.EcuTranslation.TextPl)),
    };

    private static class TitleCopier<T> where T : class
    {
        // Built once per XEP type, then reused. Reflection is only used during construction.
        internal static readonly Action<PsdzDatabase.EcuTranslation, T> Copy = BuildCopyAction();

        private static Action<PsdzDatabase.EcuTranslation, T> BuildCopyAction()
        {
            Type translationType = typeof(PsdzDatabase.EcuTranslation);
            ParameterExpression translationParam = Expression.Parameter(translationType, "ecuTranslation");
            ParameterExpression targetParam = Expression.Parameter(typeof(T), "xepObject");
            List<Expression> assignments = new List<Expression>();

            foreach ((string titleName, string textName) in TitleMapping)
            {
                PropertyInfo titleProperty = typeof(T).GetProperty(titleName, BindingFlags.Public | BindingFlags.Instance);
                if (titleProperty == null || !titleProperty.CanWrite || titleProperty.PropertyType != typeof(string))
                {
                    continue;
                }

                PropertyInfo textProperty = translationType.GetProperty(textName, BindingFlags.Public | BindingFlags.Instance);
                if (textProperty == null || textProperty.PropertyType != typeof(string))
                {
                    continue;
                }

                assignments.Add(Expression.Assign(
                    Expression.Property(targetParam, titleProperty),
                    Expression.Property(translationParam, textProperty)));
            }

            if (assignments.Count == 0)
            {
                return (ecuTranslation, xepObject) => { };
            }

            return Expression.Lambda<Action<PsdzDatabase.EcuTranslation, T>>(
                Expression.Block(assignments), translationParam, targetParam).Compile();
        }
    }

    private static void CopyEcuTranslation<T>(PsdzDatabase.EcuTranslation ecuTranslation, T xepObject) where T : class
    {
        if (ecuTranslation == null || xepObject == null)
        {
            return;
        }

        TitleCopier<T>.Copy(ecuTranslation, xepObject);
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
