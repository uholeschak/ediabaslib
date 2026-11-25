using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.TableDataHelper
{
    public static class TableDataHelper
    {
        [PreserveSource(Hint = "IDatabaseProvider replaced", SignatureModified = true)]
        public static Dictionary<string, TableData> GetTableData(ParameterContainer inParam, IList<string> languages, Vehicle vehicle, PsdzDatabase databaseProvider, IFFMDynamicResolver fFMDynamicResolver)
        {
            List<int> list = (inParam.getParameter("eFuse_ID") as List<int>) ?? new List<int>();
            IEnumerable<string> ids = list.Select((int id) => "EF" + id);
            ICollection<XEP_EFUSE> eFusesById = databaseProvider.GetEFusesById(ids, vehicle, fFMDynamicResolver);
            string infoTitle = inParam.getKeyValuePair("TableName_C")?.Value.ToString();
            Dictionary<string, List<string>> columnHeaders = GetColumnHeaders(inParam, languages);
            Dictionary<string, List<List<object>>> tableDataRows = GetTableDataRows(inParam, languages, eFusesById, list);
            Dictionary<string, TableData> dictionary = new Dictionary<string, TableData>();
            foreach (string language in languages)
            {
                List<string> columnHeaders2 = (columnHeaders.ContainsKey(language) ? columnHeaders[language] : new List<string>());
                List<TableDataRow> rows = (tableDataRows.ContainsKey(language) ? tableDataRows[language].Select((List<object> values) => new TableDataRow(values)).ToList() : new List<TableDataRow>());
                RemoveEmptyColumns(ref columnHeaders2, ref rows);
                TableData tableData = new TableData(columnHeaders2, MergeTableDataRows(rows))
                {
                    InfoTitle = infoTitle
                };
                SetBeforeAndAfterText(inParam, tableData, new List<string> { language });
                dictionary[language] = tableData;
            }

            return dictionary;
        }

        private static void RemoveEmptyColumns(ref List<string> columnHeaders, ref List<TableDataRow> rows)
        {
            for (int num = columnHeaders.Count - 1; num >= 0; num--)
            {
                if (string.IsNullOrEmpty(columnHeaders[num]) && (rows[num].Cells == null || rows[num].Cells.Count == 0))
                {
                    columnHeaders.RemoveAt(num);
                    rows.RemoveAt(num);
                }
            }
        }

        private static void SetBeforeAndAfterText(ParameterContainer inParam, TableData tableData, IList<string> lang)
        {
            ITextLocator textLocator = (ITextLocator)inParam.getParameter("Vortext", TextLocator.Empty);
            tableData.BeforeText = textLocator.TextContent.GetTextForUI(lang)[0].TextItem;
            ITextLocator textLocator2 = (ITextLocator)inParam.getParameter("Nachtext", TextLocator.Empty);
            tableData.AfterText = textLocator2.TextContent.GetTextForUI(lang)[0].TextItem;
        }

        private static List<TableDataRow> MergeTableDataRows(List<TableDataRow> rows)
        {
            List<TableDataRow> list = new List<TableDataRow>();
            int num = (rows.Any() ? rows.Max((TableDataRow r) => r.Cells.Count) : 0);
            foreach (TableDataRow row in rows)
            {
                int count = list.Count;
                for (int num2 = 0; num2 < num; num2++)
                {
                    if (num2 < count)
                    {
                        if (num2 >= row.Cells.Count)
                        {
                            list[num2].Cells.Add("");
                        }
                        else
                        {
                            list[num2].Cells.Add(row.Cells[num2]);
                        }
                    }
                    else
                    {
                        TableDataRow item = ((row.Cells.Count != 0) ? new TableDataRow(new List<object> { row.Cells[num2] }) : new TableDataRow(new List<object> { "" }));
                        list.Add(item);
                    }
                }
            }

            return list;
        }

        private static Dictionary<string, List<List<object>>> GetTableDataRows(ParameterContainer inParam, IList<string> lang, ICollection<XEP_EFUSE> eFuseResults, List<int> originalIds)
        {
            Dictionary<string, List<List<object>>> dictionary = new Dictionary<string, List<List<object>>>();
            foreach (string item in lang)
            {
                dictionary[item] = new List<List<object>>
                {
                    GetValueAndConvert(inParam.getParameter("eFuse_ID") as List<int>),
                    GetValueAndConvert(inParam.getParameter("eFuseNennwert") as List<double>),
                    GetValueAndConvert(inParam.getParameter("eFuseVerbraucher") as List<ITextLocator>, item),
                    GetValueAndConvert(inParam.getParameter("Grobzeichen") as List<string>, item, eFuseResults, originalIds),
                    GetValueAndConvert(inParam.getParameter("eFuseSOLL") as List<ITextLocator>, item),
                    GetValueAndConvert(inParam.getParameter("eFuseIST") as List<ITextLocator>, item)
                };
            }

            return dictionary;
        }

        public static Dictionary<string, List<string>> GetColumnHeaders(ParameterContainer inParam, IList<string> languages)
        {
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            foreach (string language in languages)
            {
                List<string> value = new List<string>
                {
                    GetLocalizedPlainText((ITextLocator)inParam.getParameter("TableName_A", TextLocator.Empty), language),
                    GetLocalizedPlainText((ITextLocator)inParam.getParameter("TableName_B", TextLocator.Empty), language),
                    GetLocalizedPlainText((ITextLocator)inParam.getParameter("TableName_C", TextLocator.Empty), language),
                    GetLocalizedPlainText((ITextLocator)inParam.getParameter("TableName_D", TextLocator.Empty), language),
                    GetLocalizedPlainText((ITextLocator)inParam.getParameter("TableName_E", TextLocator.Empty), language),
                    GetLocalizedPlainText((ITextLocator)inParam.getParameter("TableName_F", TextLocator.Empty), language)
                };
                dictionary[language] = value;
            }

            return dictionary;
        }

        private static List<object> GetValueAndConvert<T>(List<T> valuesList, string lang = null, ICollection<XEP_EFUSE> eFuseResults = null, List<int> originalIds = null)
        {
            if (valuesList == null)
            {
                return new List<object>();
            }

            Type typeFromHandle = typeof(T);
            if (typeFromHandle == typeof(double))
            {
                return (
                    from double value in valuesList
                    select (value % 1.0 != 0.0) ? value.ToString("F1") : value.ToString("F0")).Cast<object>().ToList();
            }

            if (typeFromHandle == typeof(int) || typeFromHandle == typeof(string) || typeFromHandle == typeof(ITextLocator))
            {
                if (typeFromHandle == typeof(ITextLocator))
                {
                    return valuesList.Cast<ITextLocator>().Select((ITextLocator item) =>
                    {
                        string localizedPlainText = GetLocalizedPlainText(item, lang);
                        return (!string.IsNullOrEmpty(localizedPlainText)) ? localizedPlainText : string.Empty;
                    }).Cast<object>().ToList();
                }

                if (typeFromHandle == typeof(string))
                {
                    Dictionary<string, string> translationMap = BuildLocalizedTextMap(eFuseResults, lang);
                    return (
                        from string text in valuesList
                        select GetLocalizedPlainText(text, lang, translationMap)).Cast<object>().ToList();
                }

                return valuesList.Cast<object>().ToList();
            }

            return new List<object>();
        }

        private static string GetLocalizedPlainText(ITextLocator textLocator, string lang)
        {
            TextContent textContent = textLocator.TextContent as TextContent;
            string result = string.Empty;
            if (textContent.TextLocalized != null)
            {
                result = new TextContent((
                    from y in textContent.TextLocalized
                    where y.Language == lang
                    select y.TextItem).FirstOrDefault()).PlainText;
            }

            return result;
        }

        private static string GetLocalizedPlainText(string stringLocator, string targetLang, Dictionary<string, string> translationMap)
        {
            if (string.IsNullOrWhiteSpace(stringLocator))
            {
                return string.Empty;
            }

            if (!translationMap.TryGetValue(stringLocator.Trim(), out var value))
            {
                return stringLocator;
            }

            return value;
        }

        private static Dictionary<string, string> BuildLocalizedTextMap(ICollection<XEP_EFUSE> eFuseResults, string targetLang)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (eFuseResults == null)
            {
                return dictionary;
            }

            foreach (XEP_EFUSE eFuseResult in eFuseResults)
            {
                if (eFuseResult?.Entries == null)
                {
                    continue;
                }

                Dictionary<string, string> dictionary2 = eFuseResult.Entries.ToDictionary((LocalizedTitleItem e) => e.Language, (LocalizedTitleItem e) => e.Value);
                if (!dictionary2.TryGetValue(targetLang, out var value))
                {
                    continue;
                }

                foreach (KeyValuePair<string, string> item in dictionary2)
                {
                    string text = item.Value?.Trim();
                    if (!string.IsNullOrEmpty(text) && !dictionary.ContainsKey(text))
                    {
                        dictionary[text] = value;
                    }
                }
            }

            return dictionary;
        }
    }
}