namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public static class DocumentTypeConverter
    {
        public static DocumentType GetEnumDocumentType(string documentType)
        {
            if (documentType == null)
            {
                return DocumentType.UNKNOWN;
            }
            if (documentType.ToUpperInvariant().Contains("PDF"))
            {
                return DocumentType.PDF;
            }
            if (documentType.ToUpperInvariant().Contains("XML"))
            {
                return DocumentType.XML;
            }
            if (documentType.ToUpperInvariant().Contains("SVG"))
            {
                return DocumentType.SVG;
            }
            if (documentType.ToUpperInvariant().Contains("NONE"))
            {
                return DocumentType.NONE;
            }
            if (documentType.ToUpperInvariant().Contains("XPS"))
            {
                return DocumentType.XPS;
            }
            Log.Warning("DocumentTypeConverter.GetEnumDocumentType()", "Unknown document type '{0}' was given!", documentType);
            return DocumentType.UNKNOWN;
        }
    }
}
