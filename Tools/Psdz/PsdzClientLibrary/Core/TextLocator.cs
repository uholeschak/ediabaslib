using PsdzClient.Core.Container;
using System.Collections.Generic;
using System;

namespace PsdzClient.Core
{
    public class TextLocator : SPELocator, ITextLocator, ISPELocator
    {
        private readonly TextContent textContent;

        public static ITextLocator Empty => new TextLocator("<spe:TEXTITEM  xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:PARAGRAPH/></spe:TEXTITEM>");

        public string Text
        {
            get
            {
                if (textContent != null)
                {
                    return textContent.Text;
                }
                return null;
            }
        }

        ITextContent ITextLocator.TextContent
        {
            get
            {
                return textContent;
            }
            set
            {
            }
        }

        public TextLocator(IList<LocalizedText> text)
        {
            textContent = new TextContent(text);
        }

        private TextLocator(TextContent textContent)
        {
            this.textContent = textContent;
        }

        public TextLocator()
            : this(string.Empty)
        {
        }

        public TextLocator(string text)
        {
            textContent = new TextContent(text);
        }

        public ITextLocator Concat(ITextLocator theTextLocator)
        {
            return Concat(theTextLocator, theAddAfterLineBreak: false);
        }

        public ITextLocator Concat(ITextLocator theTextLocator, bool theAddAfterLineBreak)
        {
            try
            {
                if (theTextLocator != null)
                {
                    if (theAddAfterLineBreak)
                    {
                        textContent.Concat(Empty.Text);
                    }
                    textContent.Concat(theTextLocator.TextContent);
                }
                return new TextLocator(textContent);
            }
            catch (Exception)
            {
                //Log.WarningException("TextLocator.Concat(ITextLocator theTextLocator, bool theAddAfterLineBreak)", exception);
                return this;
            }
        }

        public ITextLocator Concat(IEnumerable<ITextLocator> theTextLocator)
        {
            return Concat(theTextLocator, theAddAfterLineBreak: false);
        }

        public ITextLocator Concat(IEnumerable<ITextLocator> theTextLocator, bool theAddAfterLineBreak)
        {
            if (theTextLocator != null)
            {
                foreach (ITextLocator item in theTextLocator)
                {
                    Concat(item, theAddAfterLineBreak);
                }
            }
            return new TextLocator(textContent);
        }

        public override string ToString()
        {
            if (textContent != null)
            {
                return textContent.PlainText;
            }
            return string.Empty;
        }
    }
}
