using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XmlGenSharp.Lib.Utility
{
  public class XmlDocumentLoader
  {
    // Public Static Methods

    #region LoadWithIncludes(string filename)
    public static XmlDocument LoadWithIncludes(string filename)
    {
      XmlDocument xml = new XmlDocument();

      xml.Load(filename);
      ProcessIncludeNodes(xml, xml.DocumentElement, Path.GetDirectoryName(filename));

      return xml;
    }
    #endregion

    // Private Static Methods

    #region ProcessIncludeNodes(XmlDocument xml, XmlNode parent, string basePath)
    public static void ProcessIncludeNodes(XmlDocument xml, XmlNode parent, string basePath)
    {
      List<XmlNode> removeList = new List<XmlNode>();

      foreach (XmlNode node in parent.ChildNodes)
      {
        if (node.Name.ToLower() == "include")
        {
          string filename = node.Attributes["filename"].Value as string;
          if (!Path.IsPathRooted(filename))
            filename = Path.Combine(basePath, filename);

          XmlDocument include = XmlDocumentLoader.LoadWithIncludes(filename);

          foreach (XmlNode inner in include.DocumentElement.ChildNodes)
          {
            XmlNode imported = xml.ImportNode(inner, true);
            parent.InsertBefore(imported, node);
          }

          removeList.Add(node);
        }
        else
          ProcessIncludeNodes(xml, node, basePath);
      }

      foreach (XmlNode node in removeList)
        parent.RemoveChild(node);
    }
    #endregion
  }
}
