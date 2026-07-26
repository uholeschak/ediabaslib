using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Resources;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class InfoObjectContentTransformed : InfoObjectContent
    {
        private readonly InfoObject parent;
        private string styleSheetField;
        private string transformedDocumentField;
        private bool isDocumentTransformationValid;
        [XmlIgnore]
        public string FilenameOfContent { get; set; }

        [XmlIgnore]
        public string UriOfContent { get; set; }

        [XmlIgnore]
        public string StyleSheet
        {
            get
            {
                //[-] if (!string.IsNullOrEmpty(styleSheetField))
                //[-] {
                //[-] return styleSheetField;
                //[-] }
                //[-] if (!string.IsNullOrEmpty(parent.XepInfoObject.DocumentType))
                //[-] {
                //[-] StyleSheet = DatabaseProviderFactory.Instance.GetStyleSheet(parent.XepInfoObject.DocumentType);
                //[-] return styleSheetField;
                //[-] }
                //[-] if (parent.InfoType == "ESL")
                //[-] {
                //[-] StyleSheet = Resources.ESL_Stylesheet;
                //[-] return styleSheetField;
                //[-] }
                return string.Empty;
            }

            set
            {
                IsDocumentTransformationValid = false;
                if (styleSheetField != null)
                {
                    if (!styleSheetField.Equals(value))
                    {
                        styleSheetField = value;
                        OnPropertyChanged("StyleSheet");
                    }
                }
                else
                {
                    styleSheetField = value;
                    OnPropertyChanged("StyleSheet");
                }
            }
        }

        [XmlIgnore]
        public string TransformedDocument
        {
            get
            {
                //[-] if (!IsDocumentTransformationValid)
                //[-] {
                //[-] parent.DoDocumentTransformation();
                //[-] }
                //[-] if (string.IsNullOrEmpty(transformedDocumentField))
                //[-] {
                //[-] return parent.Content.Doc;
                //[-] }
                return transformedDocumentField;
            }

            set
            {
                if (transformedDocumentField != null)
                {
                    if (!transformedDocumentField.Equals(value))
                    {
                        transformedDocumentField = value;
                        OnPropertyChanged("TransformedDocument");
                    }
                }
                else
                {
                    transformedDocumentField = value;
                    OnPropertyChanged("TransformedDocument");
                }
            }
        }

        [XmlIgnore]
        internal bool IsDocumentTransformationValid
        {
            get
            {
                return isDocumentTransformationValid;
            }

            set
            {
                isDocumentTransformationValid = value;
            }
        }

        public InfoObjectContentTransformed(InfoObject infoObject)
        {
            parent = infoObject;
        }

        private void OnPropertyChanged(string property)
        {
        }
    }
}