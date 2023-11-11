using System.Xml.Linq;
using System;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
    internal static class XNodeExtension
    {
        public static string PrintPlainText(this XText node, bool removeWhiteSpace = true)
        {
            string text = node.Value;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = ((text == null || removeWhiteSpace) ? string.Empty : " ");
            }
            return text;
        }

        public static string Print(this XNode node, bool removeWhiteSpace = true)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            string text = node.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
            if (string.IsNullOrWhiteSpace(text))
            {
                Log.Info("XNodeExtension.Print()", "Print white spaces of \"{0}\" with parameter removeWhiteSpace={1}.", node.Parent?.Name?.LocalName, removeWhiteSpace);
                if (!(text == null || removeWhiteSpace))
                {
                    return " ";
                }
                return string.Empty;
            }
            return text;
        }
    }
}