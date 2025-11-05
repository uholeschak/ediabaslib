using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    [DataContract]
    public class LocalizedText : ICloneable
    {
        private string textItem;
        private string language;
        [DataMember]
        public string TextItem
        {
            get
            {
                return textItem;
            }

            set
            {
                textItem = value;
            }
        }

        [DataMember]
        public string Language
        {
            get
            {
                return language;
            }

            set
            {
                language = value;
            }
        }

        public LocalizedText(string textItem, string language)
        {
            this.textItem = textItem;
            this.language = language;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LocalizedText))
            {
                return false;
            }

            LocalizedText localizedText = (LocalizedText)obj;
            if (Language == localizedText.Language)
            {
                return TextItem == localizedText.TextItem;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Language + TextItem).GetHashCode();
        }

        public object Clone()
        {
            return new LocalizedText(TextItem, Language);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0},{1}]", Language, TextItem);
        }
    }
}