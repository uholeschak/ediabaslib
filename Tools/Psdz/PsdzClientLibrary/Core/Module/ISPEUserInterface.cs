using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface ISPEUserInterface
    {
        bool ContinueButtonEnabled { get; set; }

        void DisplayWaitCursor(bool bWaitCursor);

        void SetNextButtonEnabled(bool nButtonEnabled);

        void WaitForContinueButton();

        void WaitForCancelButton();

        void SetFooterInfo(ITextContent message);
    }
}
