namespace BMW.Rheingold.CoreFramework.Feedback
{
    public interface IFeedbackViewHeaderTitleHelper
    {
        string Title1 { get; }

        string Title2 { get; }

        string Title3 { get; }

        void SetDocumentTitle(string title);

        void SetDocumentSubTitle(string title, string version);

        void SetGeneralUiTitle(string title);

        void SetGeneralUiSubTitle(string title);

        void StoreSubTitleForMinimizedServiceProgram();

        void RestoreSubTitleForMinimizedServiceProgram();
    }
}
