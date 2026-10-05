using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;

namespace BMW.Rheingold.CoreFramework
{
    public static class ITextContentExtensions
    {
        public static IList<LocalizedText> GetTextForUI(this ITextContent textContent, IList<string> lang)
        {
            if (textContent == null)
            {
                throw new ArgumentNullException("textContent");
            }
            TextContent textContent2 = (TextContent)textContent;
            textContent2.ChangeToLocalizedText(lang);
            IList<LocalizedText> list = new List<LocalizedText>();
            for (int i = 0; i < lang.Count; i++)
            {
                list.Add(new LocalizedText(TextContent.TransformSpeTextItem2Html(textContent2.TextLocalized[i].TextItem, textContent2.TextLocalized[i].Language), textContent2.TextLocalized[i].Language));
            }
            return list;
        }
    }
}