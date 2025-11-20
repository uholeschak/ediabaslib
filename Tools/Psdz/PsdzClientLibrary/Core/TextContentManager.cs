using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using BmwFileReader;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public class TextContentManager : ITextContentManager
    {
        private IList<string> lang;

        [PreserveSource(Hint = "Database modified")]
        private readonly PsdzDatabase db;

        private readonly bool old;

        private XmlNamespaceManager namespaceManager;

        private readonly ITextLocator serviceProgramCollection;

        private IList<XElement> serviceProgramCollectionRoot;

        [PreserveSource(Hint = "Database modified")]
        private readonly PsdzDatabase.SwiInfoObj xepInfoObj;

        private const string DefaultParameterValue = "";

        public IList<string> Langs => lang;

        private ITextLocator ServiceProgramCollection
        {
            get
            {
                if (serviceProgramCollection == null)
                {
                    throw new ArgumentException($"No text collection available.");
                }
                return serviceProgramCollection;
            }
        }

        [PreserveSource(Hint = "Database modified")]
        public static ITextContentManager Create(PsdzDatabase databaseProvider, IList<string> lang, PsdzDatabase.SwiInfoObj xepInfoObj, string serviceDialogName = null)
        {
            if (databaseProvider == null)
            {
                throw new ArgumentNullException("databaseProvider");
            }
            if (lang == null)
            {
                throw new ArgumentNullException("lang");
            }
            if (xepInfoObj != null && !(xepInfoObj.Id.ConvertToInt(-1) == -1))
            {
                return new TextContentManager(databaseProvider, lang, xepInfoObj, serviceDialogName);
            }
            Log.Info("TextContentManager.Create()", "Text collection not available, because of missing info object: {0}{1}.", (serviceDialogName == null) ? "" : ("\"" + serviceDialogName + "\" "), (xepInfoObj == null) ? "null" : (xepInfoObj.Identification + "(" + xepInfoObj.ControlId + ")"));
            return new TextContentManagerDummy();
        }

        [PreserveSource(Hint = "Database modified")]
        private TextContentManager(PsdzDatabase databaseProvider, IList<string> lang, PsdzDatabase.SwiInfoObj xepInfoObj, string serviceDialogName = null)
        {
            if (databaseProvider == null)
            {
                throw new ArgumentNullException("databaseProvider");
            }
            if (lang == null)
            {
                throw new ArgumentNullException("lang");
            }
            string num2 = xepInfoObj.Id;
            if (num2.ConvertToInt(-1) == 0m)
            {
                string text = xepInfoObj.Identifier;
                if (text.StartsWith("ABL_"))
                {
                    text = new Regex(Regex.Escape("_")).Replace(text, "-", 2);
                }
                num2 = databaseProvider.GetInfoObjectIdByIdentifier(text);
            }
            db = databaseProvider;
            this.lang = lang;
            old = false;
            this.xepInfoObj = xepInfoObj;
            serviceProgramCollection = ReadTextCollection(num2);
            serviceProgramCollectionRoot = null;
            Log.Info("TextContentManager.TextContentManager()", "Text collection {0}available for {1}\"{2}\" ({3}).", (serviceProgramCollection == null) ? "not " : "", (serviceDialogName == null) ? "" : ("\"" + serviceDialogName + "\" "), xepInfoObj.Identification, xepInfoObj.ControlId);
        }

        [PreserveSource(Hint = "Database modified")]
        internal TextContentManager(PsdzDatabase databaseProvider, IList<string> lang, string textCollection)
        {
            if (databaseProvider == null)
            {
                throw new ArgumentNullException("databaseProvider");
            }
            if (lang == null)
            {
                throw new ArgumentNullException("lang");
            }
            if (textCollection == null)
            {
                throw new ArgumentNullException("textCollection");
            }
            db = databaseProvider;
            this.lang = lang;
            old = false;
            List<LocalizedText> list = new List<LocalizedText>();
            list.AddRange(lang.Select((string x) => new LocalizedText(textCollection, x)));
            serviceProgramCollection = new TextLocator(list);
        }

        [PreserveSource(Hint = "Database modified")]
        internal TextContentManager(PsdzDatabase databaseProvider, IList<string> lang)
        {
            if (databaseProvider == null)
            {
                throw new ArgumentNullException("databaseProvider");
            }
            if (lang == null)
            {
                throw new ArgumentNullException("lang");
            }
            db = databaseProvider;
            this.lang = lang;
            old = true;
        }

        [PreserveSource(Hint = "Database modified")]
        public ITextLocator __StandardText(decimal value, __TextParameter[] paramArray)
        {
            IList<LocalizedText> list = new List<LocalizedText>();
            try
            {
                PsdzDatabase.EcuTranslation o = db.GetSpTextItemsByControlId(value.ToString(CultureInfo.InvariantCulture));
                if (o != null)
                {
                    list.AddRange(lang.Select((string x) => new LocalizedText(o.GetTitle(x), x)));
                }
            }
            catch (Exception ex)
            {
                list.AddRange(lang.Select((string x) => new LocalizedText(string.Format(CultureInfo.InvariantCulture, "<<<{0}>>>", value), x)));
                Log.Error("TextContentManager.__StandardText()", "No valid standard text found for control id \"{0}\", returning \"{1}\". {2}", value, list[0].TextItem, ex);
            }
            return ReplaceTextReferencesAndHandleParameter(list, paramArray);
        }

        public ITextLocator __Text()
        {
            return __Text(string.Empty);
        }

        public ITextLocator __Text(string value)
        {
            return __Text(value, null);
        }

        public ITextLocator __Text(string value, __TextParameter[] paramArray)
        {
            return CreateText(value, paramArray);
        }

        public IList<string> CreateTextItemIdList()
        {
            List<string> list = new List<string>();
            foreach (XElement item in ParseTextCollection(ServiceProgramCollection.Text).XPathSelectElements("spe:TEXTITEMS/spe:TEXTITEM", namespaceManager))
            {
                list.Add(item.Attribute(XName.Get("ID")).Value);
            }
            return list;
        }

        public IList<string> CreateTextIdList()
        {
            List<string> list = new List<string>();
            foreach (XElement item in ParseTextCollection(ServiceProgramCollection.Text).XPathSelectElements("spe:TEXTITEMS/spe:TEXTITEM", namespaceManager))
            {
                list.Add(item.Attribute(XName.Get("ID")).Value + " -- " + item.Attribute(XName.Get("NAME")).Value);
            }
            return list;
        }

        public IList<LocalizedText> GetTextItem(string textItemId, __TextParameter[] paramArray)
        {
            serviceProgramCollectionRoot = new List<XElement>();
            for (int i = 0; i < lang.Count; i++)
            {
                serviceProgramCollectionRoot.Add(ParseTextCollection(((TextContent)ServiceProgramCollection.TextContent).TextLocalized[i].TextItem));
            }
            IList<LocalizedText> list = new List<LocalizedText>();
            for (int j = 0; j < lang.Count; j++)
            {
                IEnumerable<XElement> source = serviceProgramCollectionRoot[j].XPathSelectElements("spe:TEXTITEMS/spe:TEXTITEM[@ID='" + textItemId + "']", namespaceManager);
                int num2 = source.Count();
                LocalizedText item;
                if (num2 == 0)
                {
                    item = new LocalizedText("<spe:TEXTITEM xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:PARAGRAPH>### " + textItemId + " ###</spe:PARAGRAPH></spe:TEXTITEM>", lang[j]);
                }
                else
                {
                    XElement xElement = source.First();
                    ReplaceParameter(xElement, paramArray, lang[j]);
                    item = new LocalizedText(xElement.Print(), lang[j]);
                }
                list.Add(item);
            }
            return list;
        }

        private XElement ParseTextCollection(string text)
        {
            return ParseXml(text);
        }

        private XElement ParseXml(string text)
        {
            try
            {
                XmlReader xmlReader = XmlReader.Create(new StringReader(text));
                XmlNameTable xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable == null)
                {
                    xmlNameTable = new NameTable();
                }
                namespaceManager = new XmlNamespaceManager(xmlNameTable);
                namespaceManager.AddNamespace("spe", "http://bmw.com/2014/Spe_Text_2.0");
                return XElement.Load(xmlReader);
            }
            catch (Exception ex)
            {
                Log.Error("TextContentManager.ParseXml()", "Failed with \"{0}\" while parsing XML: {1}", ex.Message, text);
                throw;
            }
        }

        [PreserveSource(Hint = "Database modified")]
        private XElement ParseSpeXml(string xml, string language, PsdzDatabase database)
        {
            XElement xElement = ParseXml(xml);
            AppendStandardText(xElement, namespaceManager, database, language, 0);
            AppendDiagcode(xElement, namespaceManager, database);
            return xElement;
        }

        [PreserveSource(Hint = "Modified")]
        private ITextLocator ReadTextCollection(string idInfoObject)
        {
            IList<LocalizedText> textCollectionById = db.GetTextCollectionById(idInfoObject, lang);
            if (textCollectionById == null)
            {
                return null;
            }
            IList<string> langList = new List<string>();
            IList<LocalizedText> list = new List<LocalizedText>();
            foreach (LocalizedText item in textCollectionById)
            {
                XElement node = ParseSpeXml(item.TextItem, item.Language, db);
                list.Add(new LocalizedText(node.Print(removeWhiteSpace: false), item.Language));
                langList.Add(item.Language);
            }

            // [UH] [IGNORE] update lang list
            lang = langList;
            return new TextLocator(list);
        }

        private object GetParameter(__TextParameter[] paramArray, string id)
        {
            string text = "";
            if (paramArray != null)
            {
                for (int i = 0; i < paramArray.Length; i++)
                {
                    __TextParameter _TextParameter = paramArray[i];
                    if (_TextParameter.Name.Equals(id))
                    {
                        if (_TextParameter.Value == null)
                        {
                            Log.Error("TextContentManager.GetParameter()", "Parameter with name \"{0}\" has a value==null. Returning \"{1}\" instead.", id, text);
                            return text;
                        }
                        return _TextParameter.Value;
                    }
                }
            }
            Log.Error("TextContentManager.GetParameter()", "Parameter with name \"{0}\" missing. Returning \"{1}\" instead.", id, text);
            return text;
        }

        private void ReplaceParameter(XElement textItem, __TextParameter[] paramArray, string language)
        {
            IEnumerable<XElement> enumerable = textItem.XPathSelectElements(".//spe:PARAMETER[@ID and not(@done)]|.//spe:TEXTPARAMETER[@ID]", namespaceManager);
            bool flag = true;
            foreach (XElement item in enumerable)
            {
                flag = false;
                XAttribute xAttribute = item.Attribute(XName.Get("ID"));
                string value = xAttribute.Value;
                object parameter = GetParameter(paramArray, value);
                string text = null;
                if (parameter is TextLocator)
                {
                    text = ((TextContent)((ITextLocator)parameter).TextContent).GetFormattedText(language);
                }
                else if (parameter is TextContent)
                {
                    text = ((TextContent)parameter).GetFormattedText(language);
                }
                else if (parameter is string)
                {
                    text = (string)parameter;
                    if (!text.Contains("/>") && !text.Contains("</"))
                    {
                        text = null;
                    }
                }
                if (text != null)
                {
                    ReplaceParameterXml(text, item, xAttribute, language, textItem);
                }
                else
                {
                    ReplaceParameterSimpleType(parameter, item, xAttribute, language);
                }
            }
            if (!flag)
            {
                ReplaceParameter(textItem, paramArray, language);
            }
        }

        private void ReplaceParameterSimpleType(object paramValue, XElement parameterElement, XAttribute attParameterId, string language)
        {
            string format = parameterElement.Attribute(XName.Get("FORMAT"))?.Value;
            string text = Format(paramValue, format, language);
            if (parameterElement.Name.LocalName.Equals("PARAMETER"))
            {
                attParameterId.SetValue(text);
                parameterElement.Add(new XAttribute(XName.Get("done"), true));
                return;
            }
            Log.Error("TextContentManager.ReplaceParameterSimpleType()", "The value \"{0}\" of the TEXTPARAMETER with ID \"{1}\" is a simple type. It will be wrapped into a TEXTITEM.", text, attParameterId.Value);
            XElement xElement = ParseSpeXml("<spe:TEXTITEM xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:PARAGRAPH>" + text + "</spe:PARAGRAPH></spe:TEXTITEM>", language, db);
            parameterElement.ReplaceWith(xElement.Element(XName.Get("PARAGRAPH", "http://bmw.com/2014/Spe_Text_2.0")));
        }

        private void ReplaceParameterXml(string parameterFormattedText, XElement parameterElement, XAttribute attParameterId, string language, XElement textItem)
        {
            XElement xElement = ParseSpeXml(parameterFormattedText, language, db);
            int num2 = xElement.Elements().Count();
            if (parameterElement.Name.LocalName.Equals("TEXTPARAMETER"))
            {
                parameterElement.AddAfterSelf(xElement.Elements());
                parameterElement.Remove();
            }
            else if (textItem.XPathSelectElements(".//spe:PARAGRAPH/spe:PARAMETER[@ID]", namespaceManager).Contains(parameterElement) && (num2 > 1 || xElement.Element(XName.Get("PARAGRAPH", "http://bmw.com/2014/Spe_Text_2.0")) == null))
            {
                IEnumerable<XNode> enumerable = parameterElement.NodesAfterSelf();
                XElement xElement2 = new XElement(XName.Get("PARAGRAPH", "http://bmw.com/2014/Spe_Text_2.0"));
                xElement2.AddFirst(enumerable);
                enumerable.ForEach(delegate (XNode x)
                {
                    x.Remove();
                });
                parameterElement.Parent.AddAfterSelf(xElement2);
                parameterElement.Parent.AddAfterSelf(xElement.Elements());
                parameterElement.Remove();
            }
            else
            {
                string value = new TextContent(string.Empty).BuildPlainText(xElement);
                attParameterId.SetValue(value);
                parameterElement.Add(new XAttribute(XName.Get("done"), true));
                if (num2 > 1)
                {
                    parameterElement.SetAttributeValue(XName.Get("STATIC"), true);
                }
            }
        }

        private string Format(object paramValue, string format, string language)
        {
            string text = format ?? "#0.0";
            try
            {
                if (!(paramValue is double num2))
                {
                    if (!(paramValue is float num3))
                    {
                        if (!(paramValue is short num4))
                        {
                            if (!(paramValue is ushort num5))
                            {
                                if (!(paramValue is int num6))
                                {
                                    if (!(paramValue is uint num7))
                                    {
                                        if (!(paramValue is byte b))
                                        {
                                            if (!(paramValue is sbyte b2))
                                            {
                                                if (!(paramValue is char c))
                                                {
                                                    return paramValue.ToString();
                                                }
                                                return c.ToString(CultureInfo.InvariantCulture);
                                            }
                                            return b2.ToString(format, CultureInfo.InvariantCulture);
                                        }
                                        return b.ToString(format, CultureInfo.InvariantCulture);
                                    }
                                    return num7.ToString(format, CultureInfo.InvariantCulture);
                                }
                                return num6.ToString(format, CultureInfo.InvariantCulture);
                            }
                            return num5.ToString(format, CultureInfo.InvariantCulture);
                        }
                        return num4.ToString(format, CultureInfo.InvariantCulture);
                    }
                    return num3.ToString(text);
                }
                return num2.ToString(text);
            }
            catch (Exception exception)
            {
                Log.ErrorException("TextContentManager.Format()", exception);
                Log.Error("TextContentManager.Format()", "Invalid format \"{0}\" was being tried to be applied. Unformatted value is returned.", text);
                return paramValue.ToString();
            }
        }

        [PreserveSource(Hint = "Database modified")]
        private void AppendDiagcode(XElement textCollectionRoot, XmlNamespaceManager namespaceManager, PsdzDatabase db)
        {
            IEnumerable<XElement> enumerable = textCollectionRoot.XPathSelectElements("//spe:DIAGCODE[not(spe:CONTENT)]", namespaceManager);
            if (enumerable == null)
            {
                return;
            }
            foreach (XElement item in enumerable)
            {
                string value = item.Attribute(XName.Get("ID")).Value;
                string diagnosisCode = this.db.GetDiagnosisCode(value);
                if (!string.IsNullOrEmpty(diagnosisCode))
                {
                    XElement xElement = new XElement(XName.Get("CONTENT", "http://bmw.com/2014/Spe_Text_2.0"));
                    xElement.Add(diagnosisCode);
                    item.Add(xElement);
                }
            }
        }

        [PreserveSource(Hint = "Database modified")]
        private void AppendStandardText(XElement textCollectionRoot, XmlNamespaceManager namespaceManager, PsdzDatabase db, string language, int repeat)
        {
            IEnumerable<XElement> enumerable = textCollectionRoot.XPathSelectElements("//spe:STANDARDTEXT[not(spe:CONTENT)]", namespaceManager);
            if (enumerable != null)
            {
                int items = 0;
                foreach (XElement item in enumerable)
                {
                    string value = item.Attribute(XName.Get("ID")).Value;
                    PsdzDatabase.EcuTranslation ecuTranslation = db.GetSpTextItemsByControlId(value);
                    string localizedXmlValue = null;
                    if (ecuTranslation != null)
                    {
                        localizedXmlValue = ecuTranslation.GetTitle(language);
                    }

                    if (string.IsNullOrEmpty(localizedXmlValue))
                    {
                        continue;
                    }

                    XElement xElement = new XElement(XName.Get("CONTENT", "http://bmw.com/2014/Spe_Text_2.0"));
                    xElement.Add(ParseStandardTextItem(localizedXmlValue));
                    item.Add(xElement);
                    items++;
                }

                // [IGNORE] enumerable.Any is not working after enumeration!
                if (items > 0)
                {
                    if (repeat == 10)
                    {
                        Log.Error("TextContentManager.AppendStandardText()", "Abort recursive replacement of spe:STANDARDTEXT (elementsToBeReplaced.Count={0}).", enumerable.Count());
                    }
                    else
                    {
                        repeat++;
                        AppendStandardText(textCollectionRoot, namespaceManager, db, language, repeat);
                    }
                }
            }
        }

        private ITextLocator CreateTextLocator(string text)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            string spe = $"<spe:TEXTITEM xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:PARAGRAPH>{text}</spe:PARAGRAPH></spe:TEXTITEM>";
            spe = text;
            Extensions.AddRange(list, lang.Select((string x) => new LocalizedText(spe, x)));
            return new TextLocator(list);
        }

        private XElement ParseStandardTextItem(string xml)
        {
            return XElement.Load(XmlReader.Create(new StringReader(xml)));
        }

        private ITextLocator CreateText(string xmlText, __TextParameter[] paramArray)
        {
            if (old)
            {
                if (!Regex.IsMatch(xmlText.Trim(), "^\\d+$"))
                {
                    string tmp = string.Format(CultureInfo.CurrentCulture, "<TextItem>{0}</TextItem>", xmlText);
                    List<LocalizedText> list = new List<LocalizedText>();
                    Extensions.AddRange(list, lang.Select((string x) => new LocalizedText(tmp, x)));
                    return new TextLocator(list);
                }
                IList<LocalizedText> textById = db.GetTextById(xmlText, lang);
                return ReplaceTextReferencesAndHandleParameter(textById, paramArray);
            }
            if (Regex.IsMatch(xmlText.Trim(), "^\\d+$"))
            {
                return new TextLocator(GetTextItem(xmlText, paramArray));
            }
            string xml = xmlText;
            if (!xmlText.StartsWith("<?xml", StringComparison.Ordinal) && !xmlText.StartsWith("<spe:", StringComparison.Ordinal))
            {
                xml = $"<spe:TEXTITEM xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:PARAGRAPH>{xmlText}</spe:PARAGRAPH></spe:TEXTITEM>";
            }
            try
            {
                string text = ParseSpeXml(xml, lang[0], db).Print();
                return CreateTextLocator(text);
            }
            catch (Exception ex)
            {
                Log.Error("", "Failed to parse text item \"{0}\", returning empty text. {1}", xmlText, ex);
                return CreateTextLocator(string.Empty);
            }
        }

        private ITextLocator ReplaceTextReferencesAndHandleParameter(IList<LocalizedText> locText, __TextParameter[] paramArray)
        {
            IList<LocalizedText> list = new List<LocalizedText>();
            foreach (LocalizedText item in locText)
            {
                XElement xElement;
                try
                {
                    xElement = XElement.Parse(TextContent.ReplaceTextReferences(item.TextItem, db, item.Language));
                }
                catch (Exception ex)
                {
                    string text = $"<TextItem>{item}</TextItem>";
                    Log.Warning("TextContentManager.HandleParameter()", "Failed to parse \"{0}\". Try to parse \"{1}\" instead: {2}", item, text, ex);
                    xElement = XElement.Parse(text);
                }
                if (paramArray != null)
                {
                    for (int i = 0; i < paramArray.Length; i++)
                    {
                        __TextParameter _TextParameter = paramArray[i];
                        __TextParameter p = _TextParameter;
                        List<XElement> list2 = new List<XElement>(from el in xElement.DescendantsAndSelf("Parameter")
                                                                  where (string)el.Attribute("ID") == p.Name
                                                                  select el);
                        if (_TextParameter.Value is TextLocator)
                        {
                            TextLocator textLocator = (TextLocator)_TextParameter.Value;
                            string text2 = textLocator.Text;
                            string text3 = "undefined";
                            object obj;
                            try
                            {
                                try
                                {
                                    obj = XElement.Parse(text2);
                                }
                                catch (Exception ex2)
                                {
                                    text3 = $"<TextItem>{text2}</TextItem>";
                                    Log.Warning("TextContentManager.HandleParameter()", "Failed to parse parameter \"{0}\". Try to parse \"{1}\" instead: {2}", text2, text3, ex2);
                                    obj = XElement.Parse(text3);
                                }
                            }
                            catch (Exception ex3)
                            {
                                obj = new TextContent(textLocator.Text).PlainText;
                                Log.Error("TextContentManager.HandleParameter()", "Failed to parse \"{0}\", use \"{1}\" instead: {2}", text3, obj, ex3);
                            }
                            foreach (XElement item2 in list2)
                            {
                                item2.ReplaceWith(obj);
                            }
                            continue;
                        }
                        foreach (XElement item3 in list2)
                        {
                            XAttribute xAttribute = item3.Attribute("Format");
                            if (xAttribute != null)
                            {
                                if (_TextParameter.Value != null)
                                {
                                    Log.Info("TextContentManager.HandleParameter()", "Parameter: {0} Type: {1} Format: {2}", _TextParameter.Name, _TextParameter.Value.GetType(), xAttribute.Value);
                                    try
                                    {
                                        if (_TextParameter.Value is double)
                                        {
                                            item3.ReplaceWith(((double)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is float)
                                        {
                                            item3.ReplaceWith(((float)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is short)
                                        {
                                            item3.ReplaceWith(((short)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is ushort)
                                        {
                                            item3.ReplaceWith(((ushort)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is int)
                                        {
                                            item3.ReplaceWith(XElement.Parse($"<TextItem>{((int)_TextParameter.Value).ToString(xAttribute.Value)} </TextItem>"));
                                        }
                                        else if (_TextParameter.Value is uint)
                                        {
                                            item3.ReplaceWith(((uint)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is byte)
                                        {
                                            item3.ReplaceWith(((byte)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is sbyte)
                                        {
                                            item3.ReplaceWith(((sbyte)_TextParameter.Value).ToString(xAttribute.Value));
                                        }
                                        else if (_TextParameter.Value is char)
                                        {
                                            item3.ReplaceWith(((char)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                        }
                                        else
                                        {
                                            item3.ReplaceWith(_TextParameter.Value.ToString());
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Log.WarningException("TextContentManager.HandleParameter()", exception);
                                        item3.ReplaceWith(_TextParameter.Value.ToString());
                                    }
                                }
                                else
                                {
                                    Log.Info("TextContentManager.HandleParameter()", "Parameter: {0} Type: null Format: {1}", _TextParameter.Name, xAttribute.Value);
                                }
                            }
                            else
                            {
                                if (_TextParameter.Value == null)
                                {
                                    continue;
                                }
                                if (!_TextParameter.Value.ToString().Contains("/>") && !_TextParameter.Value.ToString().Contains("</"))
                                {
                                    if (_TextParameter.Value is double)
                                    {
                                        item3.ReplaceWith(((double)_TextParameter.Value).ToString("#0.0"));
                                    }
                                    else if (_TextParameter.Value is float)
                                    {
                                        item3.ReplaceWith(((float)_TextParameter.Value).ToString("#0.0"));
                                    }
                                    else if (_TextParameter.Value is short)
                                    {
                                        item3.ReplaceWith(((short)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                    }
                                    else if (_TextParameter.Value is ushort)
                                    {
                                        item3.ReplaceWith(((ushort)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                    }
                                    else if (_TextParameter.Value is int)
                                    {
                                        item3.ReplaceWith(XElement.Parse($"<TextItem>{((int)_TextParameter.Value).ToString(CultureInfo.InvariantCulture)} </TextItem>"));
                                    }
                                    else if (_TextParameter.Value is uint)
                                    {
                                        item3.ReplaceWith(((uint)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                    }
                                    else if (_TextParameter.Value is byte)
                                    {
                                        item3.ReplaceWith(((byte)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                    }
                                    else if (_TextParameter.Value is sbyte)
                                    {
                                        item3.ReplaceWith(((sbyte)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                    }
                                    else if (_TextParameter.Value is char)
                                    {
                                        item3.ReplaceWith(((char)_TextParameter.Value).ToString(CultureInfo.InvariantCulture));
                                    }
                                    else
                                    {
                                        item3.ReplaceWith(_TextParameter.Value.ToString());
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        string text4 = _TextParameter.Value.ToString();
                                        text4 = text4.Replace("&nbsp;", " ");
                                        XElement content = XElement.Parse("<TextItem>" + text4 + "</TextItem>");
                                        item3.ReplaceWith(content);
                                    }
                                    catch (Exception exception2)
                                    {
                                        Log.WarningException("TextContentManager.HandleParameter()", exception2);
                                        item3.ReplaceWith(_TextParameter.Value.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                list.Add(new LocalizedText(xElement.ToString(), item.Language));
            }
            return new TextLocator(list);
        }
    }
}
