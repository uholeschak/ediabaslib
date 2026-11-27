namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface InfoObjectTransformationStrategie
    {
        void DoDocumentTransformation(InfoObject infoObj);

        byte[] GetDocumentUtf8(InfoObject infoObj, bool disableLinks, bool disablePictureZoom);
    }
}
