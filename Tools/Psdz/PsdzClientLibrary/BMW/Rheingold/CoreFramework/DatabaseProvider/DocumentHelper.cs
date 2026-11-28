using BMW.Rheingold.CoreFramework.DatabaseProvider.SmartMaintenance;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;
using Windows.UI.Text;
using BMW.Rheingold.CoreFramework.Operation;
using PsdzClientLibrary.Properties;
using Saxon.Api;

#pragma warning disable CS0618
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class DocumentHelper : InfoObjectTransformationStrategie
    {
        private const string JAVASCRIPT_RG_PICTURE = "function rg_showPicture(id) { window.chrome.webview.hostObjects.sync.external.RG_ShowPicture(id) };";
        private const string JAVASCRIPT_RG_SHOW = "function rg_showLink(id, isNews) { window.chrome.webview.hostObjects.sync.external.RG_Show(id, isNews) };";
        private const string JAVASCRIPT_RG_SVGLINK = "function rg_openSvgLink(grobzeichen)\r\n                                                        { \r\n                                                            try \r\n                                                            { \r\n                                                                window.chrome.webview.hostObjects.sync.external.RG_SvgLinkClicked(grobzeichen); \r\n                                                            } \r\n                                                            catch(err)\r\n                                                            {\r\n                                                                try \r\n                                                                { \r\n                                                                    window.parent.postMessage({ type: 'SVG_LINK_CLICK', data: grobzeichen }, '*') \r\n                                                                } \r\n                                                                catch(err) {}  \r\n                                                            }\r\n                                                        };";
        private static readonly bool newXsltTransformationEngineEnabled = ConfigSettings.GetFeatureEnabledStatus("NewDocTransformation").IsActive;
        private static readonly Lazy<Processor> xsltProcessor = new Lazy<Processor>(() => new Processor(), isThreadSafe: true);
        public void DoDocumentTransformation(InfoObject infoObj)
        {
            if (infoObj.Content.IsDocumentTransformationValid && !ConfigSettings.getConfigStringAsBoolean("SpoolHtmlDocs"))
            {
                return;
            }

            if (CoreFramework.DebugLevel > 1)
            {
                string tempFolder = ConfigSettings.GetTempFolder();
                string fileName = Path.Combine(tempFolder, "Document.xml");
                StreamToFile(fileName, infoObj.Content.Doc);
                if (!string.IsNullOrEmpty(infoObj.Content.StyleSheet))
                {
                    string fileName2 = Path.Combine(tempFolder, "Transformator.xslt");
                    StreamToFile(fileName2, infoObj.Content.StyleSheet);
                }
            }

            infoObj.Content.IsDocumentTransformationValid = true;
            switch (DocumentTypeConverter.GetEnumDocumentType(infoObj.DocumentType))
            {
                case DocumentType.XML:
                    TransformXml(infoObj);
                    return;
                case DocumentType.SVG:
                    TransformSvg(infoObj);
                    return;
                case DocumentType.PDF:
                    TransformPdf(infoObj);
                    return;
                case DocumentType.XPS:
                    TransformXps(infoObj);
                    return;
                case DocumentType.NONE:
                    infoObj.Content.TransformedDocument = infoObj.Content.Doc;
                    return;
            }

            Log.Warning("Document.doDocumentTransformation", "Unknown format: {0}", infoObj.XepInfoObject.DocumentType);
            infoObj.Content.TransformedDocument = "Unknown format!";
        }

        public byte[] GetDocumentUtf8(InfoObject infoObj, bool disableLinks, bool disablePictureZoom)
        {
            DoDocumentTransformation(infoObj);
            try
            {
                string text = infoObj.Content.TransformedDocument;
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Document.HtmlTextWriter.Enabled"))
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        Log.Error("Logic.GetDocumentUtf8()", "Retuning null, because document=\"{0}\".", text);
                        return null;
                    }

                    return Encoding.UTF8.GetBytes(DisableLinksAndZoom(disableLinks, disablePictureZoom, text));
                }

                if (!string.IsNullOrEmpty(text) && text.StartsWith("<html", StringComparison.Ordinal))
                {
                    int num = text.IndexOf(">", 2, StringComparison.Ordinal);
                    if (num > 0)
                    {
                        text = text.Insert(num + 1, "<meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" />");
                    }

                    text = DisableLinksAndZoom(disableLinks, disablePictureZoom, text);
                }

                return Encoding.UTF8.GetBytes(text);
            }
            catch (Exception ex)
            {
                Log.Error("Logic.GetDocumentUtf8()", "Retuning null, because getting UTF-8 failed: {0}", ex);
                return null;
            }
        }

        private string DisableLinksAndZoom(bool disableLinks, bool disablePictureZoom, string doc)
        {
            string text = doc;
            if (disableLinks)
            {
                text = text.Replace("function rg_showLink(id, isNews) { window.chrome.webview.hostObjects.sync.external.RG_Show(id, isNews) };", "void(0);");
                string tempFolder = ConfigSettings.GetTempFolder();
                text = text.Replace(tempFolder, string.Empty);
            }

            if (disablePictureZoom)
            {
                text = text.Replace("function rg_showPicture(id) { window.chrome.webview.hostObjects.sync.external.RG_ShowPicture(id) };", "void(0);");
            }

            return text;
        }

        private string SetHrefAttribute(bool isNews, IEnumerable<LinkType> links)
        {
            string text = string.Join(",", links.Select((LinkType item) => item.InfoObjectControlId));
            return "javascript:rg_showLink('" + text + "', " + (int)(isNews ? 1 : 0) + ")";
        }

        private void TraceDocumentTransformation(decimal? controlId, string name, string data)
        {
            if (ConfigSettings.getConfigStringAsBoolean("SpoolHtmlDocs"))
            {
                string text = "XmlTextWriter";
                string tempFolder = ConfigSettings.GetTempFolder();
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Document.HtmlTextWriter.Enabled"))
                {
                    text = "HtmlTextWriter";
                }

                StreamToFile($"{tempFolder}\\doc\\{controlId}_{text}_{name}", data);
            }
        }

        private bool StreamToFile(string fileName, string docStream)
        {
            try
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(docStream);
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Close();
                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.StreamToFile()", exception);
            }

            return false;
        }

        private bool StreamToFile(string fileName, byte[] byteArray)
        {
            bool result = false;
            if (byteArray == null)
            {
                Log.Warning("Document.StreamToFile()", "byteArray was null");
                return false;
            }

            try
            {
                using (FileStream fileStream = File.Create(fileName))
                {
                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                    binaryWriter.Write(byteArray);
                    binaryWriter.Close();
                    fileStream.Close();
                    result = true;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.StreamToFile()", exception);
            }

            return result;
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private void CreateLinkCollection(XmlTextWriter writer, string svg, ICollection<XEP_QUERYOBJECTSEX> links)
        {
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("slave");
            writer.WriteStartElement("links");
            foreach (XEP_QUERYOBJECTSEX link in links)
            {
                writer.WriteStartElement("link");
                writer.WriteAttributeString("linkid", link.LinkId);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteRaw(svg);
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private string DoXsltTransformation(InfoObject infoObj, string dataToTransform)
        {
            string text = null;
            try
            {
                string styleSheet = infoObj.Content.StyleSheet;
                TraceDocumentTransformation(infoObj.XepInfoObject.ControlId, "3_stylesheet.xsl", styleSheet);
                string text2 = FormatConverter.ISTAXmlShaper(styleSheet);
                TraceDocumentTransformation(infoObj.XepInfoObject.ControlId, "4_stylesheet.xsl", text2);
                string text3 = FormatConverter.ISTAXmlShaper(dataToTransform);
                TraceDocumentTransformation(infoObj.XepInfoObject.ControlId, "5_document.xml", text3);
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Document.HtmlTextWriter.Enabled"))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(text3);
                    if (infoObj.InfoType == "FKB")
                    {
                        RemoveCcmTitlePlaceholders(xmlDocument);
                    }

                    if (newXsltTransformationEngineEnabled)
                    {
                        XsltTransformer xsltTransformer = xsltProcessor.Value.NewXsltCompiler().Compile(new StringReader(text2)).Load();
                        xsltTransformer.InitialContextNode = xsltProcessor.Value.NewDocumentBuilder().Build(xmlDocument);
                        AddXsltTransformationArguments(xsltTransformer, xsltProcessor.Value, infoObj);
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            Serializer serializer = xsltProcessor.Value.NewSerializer(stringWriter);
                            serializer.SetOutputProperty(Serializer.METHOD, "html");
                            xsltTransformer.Run(serializer);
                            text = stringWriter.ToString();
                        }
                    }
                    else
                    {
                        XslCompiledTransform xslCompiledTransform = new XslCompiledTransform(enableDebug: true);
                        xslCompiledTransform.Load(new XmlTextReader(new StringReader(text2)));
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            UTF8Encoding uTF8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                            using (HtmlTextWriter results = new HtmlTextWriter(new StreamWriter(memoryStream, uTF8Encoding)))
                            {
                                xslCompiledTransform.Transform(xmlDocument, GetXsltTransformationArguments(infoObj), results);
                            }

                            text = uTF8Encoding.GetString(memoryStream.ToArray());
                            int num = text.IndexOf('<');
                            if (num > 0)
                            {
                                text = text.Substring(num);
                            }
                        }
                    }
                }
                else
                {
                    using (StringWriter stringWriter2 = new StringWriter())
                    {
                        using (XmlWriter xmlWriter = new XmlTextWriter(stringWriter2))
                        {
                            using (StringReader input = new StringReader(text3))
                            {
                                using (XmlReader input2 = new XmlTextReader(input))
                                {
                                    XslCompiledTransform xslCompiledTransform2 = new XslCompiledTransform(enableDebug: true);
                                    xslCompiledTransform2.Load(new XmlTextReader(new StringReader(text2)));
                                    xslCompiledTransform2.Transform(input2, GetXsltTransformationArguments(infoObj), xmlWriter);
                                    xmlWriter.Close();
                                    text = stringWriter2.ToString();
                                    int num2 = text.IndexOf('<');
                                    if (num2 > 0)
                                    {
                                        text = text.Substring(num2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.DoXsltTransformation()", exception);
            }

            if (text == null)
            {
                Log.Error("Document.DoXsltTransformation()", "Failed to transform document, thus returning XML document.");
                text = dataToTransform;
            }

            return text;
        }

        private void RemoveCcmTitlePlaceholders(XmlDocument doc)
        {
            XmlNodeList xmlNodeList = doc.SelectNodes("//CCMELDUNGLIST/CCM");
            if (xmlNodeList == null)
            {
                return;
            }

            foreach (XmlNode item in xmlNodeList)
            {
                item.InnerText = CheckControlMessage.TitlePlaceholderRegex.Replace(item.InnerText, string.Empty).Trim();
            }
        }

        private string AddLinksToXmlDocument(InfoObject infoObj, string dataToTransform)
        {
            try
            {
                XDocument xDocument = XDocument.Parse(dataToTransform);
                Dictionary<string, DocumentLink> dictionary = new Dictionary<string, DocumentLink>();
                foreach (XElement item in xDocument.Descendants())
                {
                    string localName = item.Name.LocalName;
                    if (localName.Equals("REFERENCE", StringComparison.OrdinalIgnoreCase) || localName.Equals("HOTSPOT", StringComparison.OrdinalIgnoreCase))
                    {
                        XAttribute xAttribute = item.Attribute("LINKID");
                        if (xAttribute == null || string.IsNullOrWhiteSpace(xAttribute.Value))
                        {
                            Log.Error("Document.AddLinksToXmlDocument()", "Missing attribute LINKID in elemnent {0} with value {1} (Document ID {2}, Title {3}).", localName, item.Value, infoObj.Id, infoObj.XepInfoObject.Title_enus);
                            continue;
                        }

                        string linkId = xAttribute.Value;
                        if (!dictionary.ContainsKey(linkId))
                        {
                            IEnumerable<LinkType> enumerable = infoObj.Content.ListLinks.Where((LinkType item) => item.LinkId.Equals(linkId, StringComparison.OrdinalIgnoreCase)).ToList();
                            if (enumerable.Any())
                            {
                                dictionary.Add(linkId, new DocumentLink(linkId, SetHrefAttribute(infoObj.XepInfoObject.IsNews, enumerable)));
                                continue;
                            }

                            Log.Error("Document.AddLinksToXmlDocument()", "Missing link of element {4} to {0} with ID {1} (Document ID {2}, Title {3}).", item.Value, linkId, infoObj.Id, infoObj.XepInfoObject.Title_enus, localName);
                        }
                    }
                    else
                    {
                        if (!localName.Equals("GRAPHIC", StringComparison.OrdinalIgnoreCase) && !localName.Equals("SYMBOL", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        XAttribute xAttribute2 = item.Attribute("LINKID");
                        if (xAttribute2 == null || string.IsNullOrWhiteSpace(xAttribute2.Value))
                        {
                            Log.Error("Document.AddLinksToXmlDocument()", "Missing attribute LINKID in element {0} with value {1} (Document ID {2}, Title {3}).", localName, item.Value, infoObj.Id, infoObj.XepInfoObject.Title_enus);
                            continue;
                        }

                        string linkId2 = xAttribute2.Value;
                        if (dictionary.ContainsKey(linkId2))
                        {
                            continue;
                        }

                        GraphicsType graphicsType = infoObj.Content.ListGraphics.FirstOrDefault((GraphicsType item) => item.LinkId.Equals(linkId2, StringComparison.OrdinalIgnoreCase));
                        if (graphicsType == null)
                        {
                            XAttribute xAttribute3 = item.Attribute("SRC");
                            string text = ((xAttribute3 != null && !string.IsNullOrWhiteSpace(xAttribute3.Value)) ? xAttribute3.Value : "unknown");
                            Log.Error("Document.AddLinksToXmlDocument()", "Missing link of element {4} to {0} with ID {1} (Document ID {2}, Title {3}).", text, linkId2, infoObj.Id, infoObj.XepInfoObject.Title_enus, localName);
                            continue;
                        }

                        switch (GraphicsType.GetSuffixByFormat(graphicsType.Type))
                        {
                            case "svg":
                            {
                                byte[] byteArray = TransformSvgToUseJsOnLinkClick(graphicsType);
                                string graphicsPath3 = InfoObjectUtils.GetGraphicsPath(infoObj.Id, graphicsType.LinkId, graphicsType.Type);
                                dictionary.Add(linkId2, new DocumentLink(linkId2, graphicsPath3));
                                StreamToFile(graphicsPath3, byteArray);
                                break;
                            }

                            case "wmv":
                            case "mp4":
                            case "mp3":
                            {
                                string graphicsPath2 = InfoObjectUtils.GetGraphicsPath(infoObj.Id, graphicsType.LinkId, graphicsType.Type);
                                dictionary.Add(linkId2, new DocumentLink(linkId2, graphicsPath2));
                                StreamToFile(graphicsPath2, graphicsType.Graphic);
                                break;
                            }

                            default:
                            {
                                string graphicsPath = InfoObjectUtils.GetGraphicsPath(infoObj.Id, graphicsType.LinkId, graphicsType.Type);
                                string zoomUrl = ((!graphicsType.Name.StartsWith("GRA-SYM", StringComparison.OrdinalIgnoreCase) && !graphicsType.Name.StartsWith("GRC", StringComparison.OrdinalIgnoreCase)) ? $"javascript:rg_showPicture('{linkId2}');" : null);
                                dictionary.Add(linkId2, new DocumentLink(linkId2, graphicsPath, zoomUrl));
                                StreamToFile(graphicsPath, graphicsType.Graphic);
                                break;
                            }
                        }
                    }
                }

                XElement xElement = new XElement("view");
                XElement xElement2 = new XElement("slave");
                xElement.Add(xElement2);
                xElement2.Add(xDocument.Root);
                if (dictionary.Count > 0)
                {
                    XElement xElement3 = new XElement("links");
                    xElement2.Add(xElement3);
                    foreach (DocumentLink value in dictionary.Values)
                    {
                        xElement3.Add(value.ToXml());
                    }
                }

                XElement xElement4 = new XElement("script");
                xElement4.Add(new XAttribute("content", "function rg_showLink(id, isNews) { window.chrome.webview.hostObjects.sync.external.RG_Show(id, isNews) };function rg_showPicture(id) { window.chrome.webview.hostObjects.sync.external.RG_ShowPicture(id) };"));
                xElement2.Add(xElement4);
                return xElement.ToString();
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.AddLinksToXmlDocument()", exception);
                return dataToTransform;
            }
        }

        private byte[] TransformSvgToUseJsOnLinkClick(GraphicsType graphic)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                MemoryStream inStream = new MemoryStream(graphic.Graphic);
                xmlDocument.Load(inStream);
                AddJsFunctionToSvg(xmlDocument);
                ReplaceHrefsToCallJsFunction(xmlDocument);
                return Encoding.Default.GetBytes(xmlDocument.OuterXml);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), "Error during transformation of SVG attachment with LinkId " + graphic?.LinkId, exception);
                return graphic.Graphic;
            }
        }

        private static void AddJsFunctionToSvg(XmlDocument xmlDocument)
        {
            XmlNode documentElement = xmlDocument.DocumentElement;
            XmlElement xmlElement = xmlDocument.CreateElement("script", documentElement.NamespaceURI);
            xmlElement.InnerText = "function rg_openSvgLink(grobzeichen)\r\n                                                        { \r\n                                                            try \r\n                                                            { \r\n                                                                window.chrome.webview.hostObjects.sync.external.RG_SvgLinkClicked(grobzeichen); \r\n                                                            } \r\n                                                            catch(err)\r\n                                                            {\r\n                                                                try \r\n                                                                { \r\n                                                                    window.parent.postMessage({ type: 'SVG_LINK_CLICK', data: grobzeichen }, '*') \r\n                                                                } \r\n                                                                catch(err) {}  \r\n                                                            }\r\n                                                        };";
            documentElement.PrependChild(xmlElement);
        }

        private static void ReplaceHrefsToCallJsFunction(XmlDocument xmlDocument)
        {
            foreach (XmlNode item in xmlDocument.GetElementsByTagName("a"))
            {
                XmlAttributeCollection attributes = item.Attributes;
                if (attributes != null)
                {
                    XmlNode namedItem = attributes.GetNamedItem("xlink:href");
                    if (namedItem != null && !string.IsNullOrEmpty(namedItem.Value))
                    {
                        string value = namedItem.Value;
                        namedItem.Value = "javascript:rg_openSvgLink('" + value + "')";
                    }
                }
            }
        }

        private void SaveMediaToTempPathForESLDocuments(InfoObject infoObject)
        {
            try
            {
                if (infoObject == null || !infoObject.IsESLError)
                {
                    return;
                }

                EslDocumentIsta eslDocument = infoObject.EslDocument;
                if (eslDocument != null && eslDocument.LinkedMedia?.Count == 0)
                {
                    return;
                }

                foreach (LinkedMedia linkedMedium in infoObject.EslDocument.LinkedMedia)
                {
                    if (linkedMedium.ContentInBytes != null && !string.IsNullOrEmpty(linkedMedium.Name) && !string.IsNullOrEmpty(linkedMedium.Type))
                    {
                        string tempFolder = ConfigSettings.GetTempFolder();
                        StreamToFile(Path.Combine(tempFolder, linkedMedium.Name + "." + linkedMedium.Type), linkedMedium.ContentInBytes);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        private void TransformPdf(InfoObject infoObj)
        {
            try
            {
                if (string.IsNullOrEmpty(infoObj.Title))
                {
                    infoObj.XepInfoObjectCasted.Title_engb = "Information";
                }

                string tempFolder = ConfigSettings.GetTempFolder();
                string text = tempFolder + infoObj.XepInfoObject.ControlId + ".pdf";
                string text2 = tempFolder + infoObj.XepInfoObject.ControlId + ".html";
                using (FileStream fileStream = new FileStream(text, FileMode.Create))
                {
                    fileStream.Write(infoObj.Content.BinaryDocument, 0, infoObj.Content.BinaryDocument.Length);
                    fileStream.Close();
                }

                string s = string.Format("<!-- saved from url=(0016)http://localhost -->\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:svg=\"http://www.w3.org/2000/svg\"><body><embed src=\"file:///{0}#toolbar=0&navpanes=0&scrollbar=0\" width=\"100%\" height=\"100%\"/></body></html>", text.Replace("\\", "/"));
                using (FileStream fileStream2 = new FileStream(text2, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(s);
                    fileStream2.Write(bytes, 0, bytes.Length);
                    fileStream2.Close();
                }

                infoObj.Content.FilenameOfContent = text;
                infoObj.Content.UriOfContent = string.Format("file:///{0}", text2.Replace("\\", "/"));
                infoObj.Content.TransformedDocument = string.Format("<!-- saved from url=(0016)http://localhost -->\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:svg=\"http://www.w3.org/2000/svg\"><body><iframe src=\"file:///{0}\" frameborder=\"0\" width=\"100%\" height=\"100%\"/></body></html>", text2.Replace("\\", "/"));
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.TransformPdf()", exception);
            }
        }

        private void TransformSvg(InfoObject infoObj)
        {
            try
            {
                if (string.IsNullOrEmpty(infoObj.Title))
                {
                    infoObj.XepInfoObjectCasted.Title_engb = "Wiring diagram";
                    infoObj.XepInfoObjectCasted.Title_dede = "Schaltplan";
                    infoObj.XepInfoObjectCasted.Title_fr = "Schéma électrique";
                    infoObj.XepInfoObjectCasted.Title_th = "แผนผ\u0e31งการเด\u0e34นสาย";
                    infoObj.XepInfoObjectCasted.Title_sv = "Kopplingsschema";
                    infoObj.XepInfoObjectCasted.Title_it = "Schema elettrico";
                    infoObj.XepInfoObjectCasted.Title_es = "Esquema eléctrico";
                    infoObj.XepInfoObjectCasted.Title_id = infoObj.XepInfoObject.Title_engb;
                    infoObj.XepInfoObjectCasted.Title_ko = "회로도";
                    infoObj.XepInfoObjectCasted.Title_el = "Διάγραμμα συνδεσμολογίας";
                    infoObj.XepInfoObjectCasted.Title_tr = "Devre planı";
                    infoObj.XepInfoObjectCasted.Title_zhcn = "电路图";
                    infoObj.XepInfoObjectCasted.Title_ru = "Электрическая схема";
                    infoObj.XepInfoObjectCasted.Title_nl = "Elektrisch schema";
                    infoObj.XepInfoObjectCasted.Title_pt = "Esquema de ligações";
                    infoObj.XepInfoObjectCasted.Title_zhtw = "開關面板";
                    infoObj.XepInfoObjectCasted.Title_ja = "配線図";
                    infoObj.XepInfoObjectCasted.Title_cscz = "Elektrické schéma";
                    infoObj.XepInfoObjectCasted.Title_plpl = "Schemat połączeń";
                }

                if (!ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Document.showSVGNative", defaultValue: false))
                {
                    string tempFolder = ConfigSettings.GetTempFolder();
                    string text = Path.Combine(tempFolder, $"ext{infoObj.XepInfoObject.ControlId}.svg");
                    string text2 = Path.Combine(tempFolder, $"{infoObj.XepInfoObject.ControlId}.svg");
                    DeleteFileIfExists(text);
                    DeleteFileIfExists(text2);
                    string name = "Schematic";
                    if (ConfigSettings.getConfigString("BMW.Rheingold.SvgViewer", "SharpVectorSvgViewer") == "SharpVectorSvgViewer")
                    {
                        name = "SchematicNew";
                    }

                    using (XmlReader stylesheet = XmlReader.Create(new StringReader(Resources.ResourceManager.GetString(name))))
                    {
                        XslCompiledTransform xslCompiledTransform = new XslCompiledTransform(enableDebug: true);
                        xslCompiledTransform.Load(stylesheet);
                        using (XmlTextWriter xmlTextWriter = new XmlTextWriter(text, null))
                        {
                            CreateLinkCollection(xmlTextWriter, infoObj.Content.Doc, infoObj.Content.ListSvgLinks);
                            xmlTextWriter.Close();
                        }

                        using (XmlReader input = new XmlTextReader(text))
                        {
                            using (XmlTextWriter xmlTextWriter2 = new XmlTextWriter(text2, null))
                            {
                                xmlTextWriter2.Formatting = Formatting.Indented;
                                xslCompiledTransform.Transform(input, xmlTextWriter2);
                                xmlTextWriter2.Close();
                            }
                        }
                    }

                    DeleteFileIfExists(text);
                    using (StreamReader streamReader = new StreamReader(text2))
                    {
                        infoObj.Content.TransformedDocument = streamReader.ReadToEnd();
                    }

                    infoObj.Content.FilenameOfContent = text2;
                    infoObj.Content.UriOfContent = string.Format("file:///{0}", text2.Replace("\\", "/"));
                }
                else
                {
                    infoObj.Content.TransformedDocument = infoObj.Content.Doc;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.TransformSvg()", exception);
            }
        }

        private void TransformXml(InfoObject infoObj)
        {
            try
            {
                if (string.IsNullOrEmpty(infoObj.Content.StyleSheet))
                {
                    Log.Error("Document.TransformXml()", "Using XML as transformed document, because style sheet is null or empty.");
                    infoObj.Content.TransformedDocument = infoObj.Content.Doc;
                    return;
                }

                infoObj.Content.IsDocumentTransformationValid = true;
                string text = infoObj.Content.Doc;
                TraceDocumentTransformation(infoObj.XepInfoObject.ControlId, "1_document-from-database.xml", text);
                if (string.IsNullOrEmpty(text) || "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>".Equals(text))
                {
                    Log.Warning("Document.TransformXml()", "Data to transform was null or empty (no xml content which can be transformed): {0}", text);
                }
                else
                {
                    text = AddLinksToXmlDocument(infoObj, text);
                    SaveMediaToTempPathForESLDocuments(infoObj);
                    TraceDocumentTransformation(infoObj.XepInfoObject.ControlId, "2_document-wiht-links.xml", text);
                    text = DoXsltTransformation(infoObj, text);
                    TraceDocumentTransformation(infoObj.XepInfoObject.ControlId, "6_document-transformed.html", text);
                }

                infoObj.Content.TransformedDocument = text;
            }
            catch (Exception ex)
            {
                Log.Error("Document.TransformXml()", "Using XML as transformed documnet, because: {0}", ex);
                infoObj.Content.TransformedDocument = infoObj.Content.Doc;
            }
        }

        private void TransformXps(InfoObject infoObj)
        {
            try
            {
                string text = ConfigSettings.GetTempFolder() + infoObj.XepInfoObject.ControlId + ".xps";
                using (FileStream fileStream = new FileStream(text, FileMode.Create))
                {
                    fileStream.Write(infoObj.Content.BinaryDocument, 0, infoObj.Content.BinaryDocument.Length);
                    fileStream.Close();
                }

                infoObj.Content.FilenameOfContent = text;
                infoObj.Content.UriOfContent = string.Format("{0}", text.Replace("\\", "/"));
                infoObj.Content.TransformedDocument = infoObj.Content.UriOfContent;
            }
            catch (Exception exception)
            {
                Log.WarningException("Document.TransformXps()", exception);
            }
        }

        private XsltArgumentList GetXsltTransformationArguments(InfoObject infoObj)
        {
            if (infoObj != null && infoObj.Content != null && infoObj.Content.ListIncludes != null && infoObj.Content.ListIncludes.Count > 0)
            {
                XsltArgumentList xsltArgumentList = new XsltArgumentList();
                string text = string.Join("|", infoObj.Content.ListIncludes);
                xsltArgumentList.AddParam(parameter: "|" + text + "|", name: "VALIDITYIDS", namespaceUri: string.Empty);
                return xsltArgumentList;
            }

            if (infoObj != null && infoObj.IsESLError)
            {
                XsltArgumentList xsltArgumentList2 = new XsltArgumentList();
                xsltArgumentList2.AddParam("LANGUAGE", string.Empty, ConfigSettings.CurrentUICulture);
                XmlDocument xmlDocument = new XmlDocument();
                //[-] xmlDocument.LoadXml(Resources.dictionaries);
                xsltArgumentList2.AddParam("DICT_DOC", string.Empty, xmlDocument);
                xsltArgumentList2.AddParam(parameter: ConfigSettings.GetTempFolder(), name: "ATT_FOLDER", namespaceUri: string.Empty);
                return xsltArgumentList2;
            }

            return null;
        }

        private void AddXsltTransformationArguments(XsltTransformer transformer, Processor processor, InfoObject infoObj)
        {
            if (infoObj != null && infoObj.Content != null && infoObj.Content.ListIncludes != null && infoObj.Content.ListIncludes.Count > 0)
            {
                string text = string.Join("|", infoObj.Content.ListIncludes);
                text = "|" + text + "|";
                transformer.SetParameter(new QName("VALIDITYIDS"), new XdmAtomicValue(text));
            }
            else if (infoObj != null && infoObj.IsESLError)
            {
                transformer.SetParameter(new QName("LANGUAGE"), new XdmAtomicValue(ConfigSettings.CurrentUICulture));
                XmlDocument xmlDocument = new XmlDocument();
                //[-] xmlDocument.LoadXml(Resources.dictionaries);
                XdmNode value = processor.NewDocumentBuilder().Build(xmlDocument);
                transformer.SetParameter(new QName("DICT_DOC"), value);
                string tempFolder = ConfigSettings.GetTempFolder();
                transformer.SetParameter(new QName("ATT_FOLDER"), new XdmAtomicValue(tempFolder));
            }
        }
    }
}