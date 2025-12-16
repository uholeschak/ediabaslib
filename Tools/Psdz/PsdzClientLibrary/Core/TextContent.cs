using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using System;
using System.Web.UI;

namespace PsdzClient.Core
{
    public class TextContent : SPELocator, ITextContent, ISPELocator
    {
        private readonly string[] elementWithAttributeXmlSpace = new string[4]
        {
            "PARAGRAPH",
            "HINT",
            "CAUTION",
            "WARNING"
        };
        private string myText = string.Empty;
        private bool toStringAsPlainText;
        private string localPlainText = string.Empty;
        private IList<LocalizedText> locText;
        private static XslCompiledTransform transformer;
        public IList<LocalizedText> TextLocalized => locText;
        private bool IsLocalized => locText != null;

        public string FormattedText
        {
            get
            {
                if (IsLocalized)
                {
                    return locText[0].TextItem;
                }

                return myText;
            }
        }

        public string PlainText => BuildPlainText(FormattedText);
        public string Text => FormattedText;

        private static XslCompiledTransform CompiledTransformer
        {
            get
            {
                if (transformer == null)
                {
                    transformer = CreateTransformer();
                }

                return transformer;
            }
        }

        public TextContent(IList<LocalizedText> text)
        {
            locText = new List<LocalizedText>();
            locText.AddRange(text.Select((LocalizedText x) => new LocalizedText(AsignFormattedText(x.TextItem), x.Language)));
        }

        public TextContent(FormatedData text, IList<string> lang)
        {
            TextContent textContent = this;
            locText = new List<LocalizedText>();
            locText.AddRange(lang.Select((string x) => new LocalizedText(textContent.AsignFormattedText(text.Localize(new CultureInfo(x))), x)));
        }

        public TextContent(string text, bool toStringAsPlainText = false)
        {
            myText = AsignFormattedText(text);
            this.toStringAsPlainText = toStringAsPlainText;
        }

        private string AsignFormattedText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return TextLocator.Empty.TextContent.FormattedText;
            }

            try
            {
                XElement xElement = null;
                if (text.TrimStart().StartsWith("<"))
                {
                    xElement = ParseXml(text);
                    return xElement.Print(removeWhiteSpace: false);
                }

                string plainText = EscapeXmlElementContent(text);
                xElement = ParseXml(Create(plainText));
                return xElement.Print();
            }
            catch (Exception ex)
            {
                string text2 = EscapeXmlElementContent(text);
                Log.Warning("TextContent.AsignFormattedText()", "Failed to assign formatted text \"{0}\". Assign paragraph with plain text \"{1}\" instead. {2}", text, text2, ex.Message);
                return ParseXml(Create(text2)).Print();
            }
        }

        public static string EscapeXmlElementContent(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                return content.Replace("&", "&amp;").Replace("<", "&lt;");
            }

            return content;
        }

        public TextContent CreateParagraph(string text)
        {
            XElement xElement = ParseXml(TextLocator.Empty.TextContent.FormattedText);
            if (xElement.LastNode is XElement xElement2 && xElement2.Name.LocalName.Equals("PARAGRAPH"))
            {
                xElement2.Add(new XText(text));
            }

            return new TextContent(xElement.Print());
        }

        public string GetFormattedText(string language)
        {
            if (IsLocalized)
            {
                foreach (LocalizedText item in locText)
                {
                    if (item.Language.Equals(language))
                    {
                        return item.TextItem;
                    }
                }

                throw new ArgumentException("Unsupported language \"" + language + "\".");
            }

            return myText;
        }

        internal void ChangeToLocalizedText(IList<string> lang)
        {
            if (!IsLocalized)
            {
                IList<LocalizedText> collection = new List<LocalizedText>();
                collection.AddRange(lang.Select((string x) => new LocalizedText(myText, x)));
                locText = collection;
            }
        }

        public IList<LocalizedText> CreatePlainText(IList<string> lang)
        {
            IList<LocalizedText> list = new List<LocalizedText>();
            for (int i = 0; i < lang.Count; i++)
            {
                string text = (IsLocalized ? locText[i].TextItem : myText);
                list.Add(new LocalizedText(BuildPlainText(text), lang[i]));
            }

            return list;
        }

        internal string BuildPlainText(string text)
        {
            XElement root = ParseXml(text);
            return BuildPlainText(root);
        }

        internal string BuildPlainText(XElement root)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (root != null)
            {
                bool flag;
                if (elementWithAttributeXmlSpace.Contains(root.Name.LocalName))
                {
                    XAttribute xAttribute = root.Attribute(XName.Get("space", "http://www.w3.org/XML/1998/namespace"));
                    flag = xAttribute == null || !"preserve".Equals(xAttribute.Value);
                }
                else
                {
                    flag = false;
                }

                foreach (XNode item in root.Nodes())
                {
                    if (item.NodeType == XmlNodeType.Text)
                    {
                        if (!(item is XText node))
                        {
                            Log.Error("BuildPlainText()", "Node with type \"XmlNodeType.Text\" is no XText (Parent name \"{0}\"). Using method Print() instead of PrintPlainText().", item.Parent?.Name?.LocalName);
                            stringBuilder.Append(item.Print(flag));
                        }
                        else
                        {
                            stringBuilder.Append(node.PrintPlainText(flag));
                        }
                    }
                    else if (item.NodeType == XmlNodeType.Element)
                    {
                        XElement xElement = item as XElement;
                        if (!HandleValueUnitSymbol(xElement, stringBuilder))
                        {
                            stringBuilder.Append(BuildPlainText(xElement));
                        }
                    }
                }

                if (flag)
                {
                    return stringBuilder.ToString().Trim();
                }
            }

            return stringBuilder.ToString();
        }

        private bool HandleValueUnitSymbol(XElement element, StringBuilder result)
        {
            if (element == null || element.Name == null)
            {
                return false;
            }

            string localName = element.Name.LocalName;
            if ("UNIT".Equals(localName) || "SYMBOL".Equals(localName))
            {
                string attibuteValue = GetAttibuteValue("REF", element);
                if (attibuteValue != null)
                {
                    result.Append(attibuteValue);
                }

                return true;
            }

            if ("VALUEUNIT".Equals(localName))
            {
                string attibuteValue = GetAttibuteValue("VALUE", element);
                if (attibuteValue != null)
                {
                    result.Append(attibuteValue);
                }

                attibuteValue = GetAttibuteValue("UNIT", element);
                if (attibuteValue != null)
                {
                    result.Append(" ");
                    result.Append(attibuteValue);
                }

                return true;
            }

            if ("PARAMETER".Equals(localName) && !element.HasElements)
            {
                string attibuteValue = GetAttibuteValue("ID", element);
                if (attibuteValue != null)
                {
                    result.Append(attibuteValue);
                }

                attibuteValue = GetAttibuteValue("UNIT", element, logMissing: false);
                if (attibuteValue != null)
                {
                    result.Append(" ");
                    result.Append(attibuteValue);
                }

                return true;
            }

            return false;
        }

        private string GetAttibuteValue(string name, XElement element, bool logMissing = true)
        {
            XAttribute xAttribute = element.Attribute(XName.Get(name));
            if (xAttribute == null)
            {
                if (logMissing)
                {
                    Log.Warning("TextContentManager.GetAttibuteValue()", "Element \"{0}\" has no attribute \"{1}\", retuning null.", element?.Name?.LocalName, name);
                }

                return null;
            }

            return xAttribute.Value;
        }

        internal static XslCompiledTransform CreateTransformer()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("BMW.Rheingold.CoreFramework.DatabaseProvider.Text.Spe_Text_2.0.xsl"))
            {
                using (XmlReader stylesheet = XmlReader.Create(input))
                {
                    xslCompiledTransform.Load(stylesheet);
                    return xslCompiledTransform;
                }
            }
        }

        internal static string TransformSpeTextItem2Html(string textItem, string language, bool fullHtml = false)
        {
            XsltArgumentList xsltArgumentList = new XsltArgumentList();
            if (fullHtml)
            {
                xsltArgumentList.AddParam("fullHtml", string.Empty, true);
            }

            xsltArgumentList.AddParam("lang", string.Empty, language);
            using (StringReader input = new StringReader(textItem))
            {
                using (XmlReader input2 = XmlReader.Create(input))
                {
                    using (StringWriter stringWriter = new StringWriter())
                    {
                        CompiledTransformer.Transform(input2, xsltArgumentList, stringWriter);
                        return stringWriter.ToString().Replace(" xmlns:spe=\"http://bmw.com/2014/Spe_Text_2.0\"", "");
                    }
                }
            }
        }

        [PreserveSource(Hint = "database replaced")]
        public static string ReplaceTextReferences(string xmlText, PsdzDatabase database, string language)
        {
            string result = xmlText;
            XDocument xDocument = null;
            if (string.IsNullOrEmpty(xmlText))
            {
                result = "<TextItem/>";
            }
            else
            {
                try
                {
                    if (Regex.IsMatch(xmlText.Trim(), "^<.+"))
                    {
                        if (xmlText.StartsWith("<TextItem", StringComparison.Ordinal))
                        {
                            try
                            {
                                xDocument = XDocument.Parse(xmlText);
                            }
                            catch (Exception)
                            {
                                xDocument = null;
                            }
                        }

                        if (xDocument == null)
                        {
                            xDocument = XDocument.Parse(string.Format(CultureInfo.InvariantCulture, "<TextItem>{0}</TextItem>", xmlText));
                        }
                    }
                    else
                    {
                        if (!Regex.IsMatch(xmlText.Trim(), "^\\d+$"))
                        {
                            Log.Info("TextContentOld.ReplaceTextReferences()", "found native text!? no replacement required for: {0}", xmlText);
                            return string.Format(CultureInfo.InvariantCulture, "<TextItem SchemaVersion=\"1.0.0\">{0}</TextItem>", xmlText);
                        }

                        // [UH] [IGNORE] database replaced
                        string textItem = database.GetTextById(xmlText, new string[1] { language })[0].TextItem;
                        if (!string.IsNullOrEmpty(textItem))
                        {
                            result = ReplaceTextReferences(textItem, database, language);
                        }
                    }

                    if (xDocument != null)
                    {
                        while (xDocument.Descendants("TextReference").FirstOrDefault() != null)
                        {
                            XElement xElement = xDocument.Descendants("TextReference").FirstOrDefault();
                            if (xElement == null)
                            {
                                continue;
                            }

                            XAttribute xAttribute = xElement.Attribute("Path");
                            if (xAttribute != null && !string.IsNullOrEmpty(xAttribute.Value))
                            {
                                string value = xAttribute.Value;
                                Log.Info("TextContentOld.ReplaceTextReferences()", "Found referenced text: {0} Path: {1}", xElement, xAttribute.Value);
                                // [UH] [IGNORE] database replaced
                                PsdzDatabase.EcuTranslation ecuTranslation = database.GetSpTextItemsByControlId(value);
                                string localizedXmlValue = null;
                                if (ecuTranslation != null)
                                {
                                    localizedXmlValue = ecuTranslation.GetTitle(language);
                                }

                                XElement content;
                                if (string.IsNullOrEmpty(localizedXmlValue))
                                {
                                    Log.Error("TextContentOld.ReplaceTextReferences()", "Failed to get the localized text for ID {0}.", value);
                                    content = XElement.Parse("<TextItem>###" + value + "###</TextItem>");
                                }
                                else
                                {
                                    localizedXmlValue = ReplaceTextReferences(localizedXmlValue, database, language);
                                    content = XElement.Parse(localizedXmlValue);
                                }

                                xElement.ReplaceWith(content);
                            }
                        }

                        result = xDocument.ToString(SaveOptions.DisableFormatting);
                    }
                }
                catch (Exception ex2)
                {
                    Log.Warning("TextContentOld.ReplaceTextReferences()", "error parsing xmlText {0} : {1}", xmlText, ex2.ToString());
                }
            }

            return result;
        }

        public ITextContent Concat(ITextContent theTextContent)
        {
            if (!(theTextContent is TextContent textContent))
            {
                throw new ArgumentNullException("textContent");
            }

            if (IsLocalized)
            {
                if (textContent.IsLocalized && locText.Count == textContent.TextLocalized.Count())
                {
                    for (int i = 0; i < locText.Count; i++)
                    {
                        LocalizedText localizedText = locText[i];
                        localizedText.TextItem = ConcatFormattedText(localizedText.TextItem, textContent.TextLocalized[i].TextItem);
                    }

                    return new TextContent(new List<LocalizedText>(locText));
                }

                return Concat(textContent.FormattedText);
            }

            if (textContent.IsLocalized)
            {
                IList<LocalizedText> list = new List<LocalizedText>();
                string formattedText = FormattedText;
                for (int j = 0; j < textContent.TextLocalized.Count; j++)
                {
                    LocalizedText localizedText2 = textContent.TextLocalized[j];
                    list.Add(new LocalizedText(ConcatFormattedText(formattedText, localizedText2.TextItem), localizedText2.Language));
                }

                myText = string.Empty;
                locText = list;
                return new TextContent(new List<LocalizedText>(locText));
            }

            return Concat(textContent.FormattedText);
        }

        public ITextContent Concat(string add)
        {
            if (locText != null)
            {
                locText.ForEach(delegate (LocalizedText x)
                {
                    x.TextItem = ConcatFormattedText(x.TextItem, add);
                });
                return new TextContent(locText);
            }

            myText = ConcatFormattedText(myText, add);
            return new TextContent(myText.Clone() as string);
        }

        public TextContent ConcatPlainText(IList<LocalizedText> plainText, bool inFront = false)
        {
            IList<LocalizedText> list = new List<LocalizedText>();
            if (locText != null)
            {
                for (int i = 0; i < locText.Count; i++)
                {
                    list.Add(new LocalizedText(ConcatPlainText(locText[i].TextItem, plainText[i].TextItem, inFront), locText[i].Language));
                }
            }
            else
            {
                for (int j = 0; j < plainText.Count; j++)
                {
                    string textItem = (inFront ? (plainText[j].TextItem + myText) : (myText + plainText[j].TextItem));
                    list.Add(new LocalizedText(textItem, plainText[j].Language));
                }
            }

            return new TextContent(list);
        }

        private string ConcatPlainText(string formattedText, string appendPlain, bool inFront = false)
        {
            XElement xElement = ParseXml(formattedText);
            if (inFront)
            {
                if (xElement.FirstNode is XElement xElement2)
                {
                    if (xElement2.Name.LocalName.Equals("PARAGRAPH"))
                    {
                        xElement2.AddFirst(new XText(appendPlain));
                    }
                    else if (xElement2.Name.LocalName.Equals("STANDARDTEXT"))
                    {
                        XElement lastElementNode = GetLastElementNode(xElement2);
                        if (lastElementNode != null)
                        {
                            lastElementNode.Value = appendPlain + lastElementNode.Value;
                        }
                    }
                }
                else
                {
                    XElement xElement3 = new XElement(XName.Get("PARAGRAPH", "http://bmw.com/2014/Spe_Text_2.0"));
                    xElement3.Add(new XText(appendPlain));
                    xElement.AddFirst(xElement3);
                }
            }
            else if (xElement.LastNode is XElement xElement4 && xElement4.Name.LocalName.Equals("PARAGRAPH"))
            {
                xElement4.Add(new XText(appendPlain));
            }
            else
            {
                XElement xElement5 = new XElement(XName.Get("PARAGRAPH", "http://bmw.com/2014/Spe_Text_2.0"));
                xElement5.Add(new XText(appendPlain));
                xElement.Add(xElement5);
            }

            return xElement.Print();
        }

        private XElement GetLastElementNode(XElement xElement)
        {
            XElement xElement2 = xElement;
            while (xElement2 != null && xElement2.NodeType != XmlNodeType.Text)
            {
                XNode firstNode = xElement2.FirstNode;
                if (firstNode.NodeType != XmlNodeType.Element)
                {
                    break;
                }

                xElement2 = firstNode as XElement;
            }

            return xElement2;
        }

        internal static string Create(string plainText)
        {
            return $"<spe:TEXTITEM xmlns:spe=\"http://bmw.com/2014/Spe_Text_2.0\"><spe:PARAGRAPH>{plainText}</spe:PARAGRAPH></spe:TEXTITEM>";
        }

        private XElement ParseXml(string xml)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreWhitespace = true;
            return XElement.Load(XmlReader.Create(new StringReader(xml), xmlReaderSettings));
        }

        private string ConcatFormattedText(string text, string append)
        {
            XElement xElement = ParseXml(text);
            try
            {
                XElement xElement2 = XElement.Load(XmlReader.Create(new StringReader(append)));
                xElement.Add(xElement2.Elements());
            }
            catch (Exception ex)
            {
                Log.Info("TextContent.ConcatText()", "Text to append \"{0}\"could not be parsed as XML, thus add it as plain text. {1}", append, ex.Message);
                if (xElement.LastNode is XElement xElement3 && xElement3.Name.LocalName.Equals("PARAGRAPH"))
                {
                    xElement3.Add(new XText(append));
                }
                else
                {
                    XElement xElement4 = new XElement(XName.Get("PARAGRAPH", "http://bmw.com/2014/Spe_Text_2.0"));
                    xElement4.Add(new XText(append));
                    xElement.Add(xElement4);
                }
            }

            return xElement.Print();
        }

        public ITextContent Concat(double theNewValue)
        {
            return Concat(theNewValue, null);
        }

        public ITextContent Concat(double theNewValue, string theMetaInformation)
        {
            string text = theNewValue.ToString(CultureInfo.InvariantCulture);
            if (theMetaInformation != null)
            {
                text += theMetaInformation;
            }

            return Concat(text);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TextContent) && !(obj is ITextContent))
            {
                return false;
            }

            if (!(obj is TextContent textContent))
            {
                return false;
            }

            if (textContent.myText != myText)
            {
                return false;
            }

            if (textContent.locText != null)
            {
                if (locText == null || locText.Count != textContent.locText.Count)
                {
                    return false;
                }

                for (int i = 0; i < locText.Count; i++)
                {
                    if (!locText.Equals(textContent.locText))
                    {
                        return false;
                    }
                }
            }
            else if (locText != null)
            {
                return false;
            }

            if (textContent.Children != base.Children)
            {
                return false;
            }

            if (textContent.DataClassName != base.DataClassName)
            {
                return false;
            }

            if (textContent.Exception != base.Exception)
            {
                return false;
            }

            if (textContent.FormattedText != FormattedText)
            {
                return false;
            }

            if (textContent.HasException != base.HasException)
            {
                return false;
            }

            if (textContent.Id != base.Id)
            {
                return false;
            }

            if (textContent.IncomingLinkNames != base.IncomingLinkNames)
            {
                return false;
            }

            if (textContent.OutgoingLinkNames != base.OutgoingLinkNames)
            {
                return false;
            }

            if (textContent.Parents != base.Parents)
            {
                return false;
            }

            if (textContent.PlainText != PlainText)
            {
                return false;
            }

            if (textContent.SignedId != base.SignedId)
            {
                return false;
            }

            if (textContent.Text != Text)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (FormattedText + base.Children?.ToString() + base.DataClassName + base.Exception?.ToString() + FormattedText + base.HasException + base.Id + base.IncomingLinkNames?.ToString() + base.OutgoingLinkNames?.ToString() + base.Parents?.ToString() + PlainText + base.SignedId + Text).GetHashCode();
        }

        public override string ToString()
        {
            if (toStringAsPlainText)
            {
                return PlainText;
            }

            return FormattedText;
        }
    }
}