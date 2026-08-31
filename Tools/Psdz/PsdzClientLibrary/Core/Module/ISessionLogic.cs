namespace BMW.Rheingold.CoreFramework.Contracts
{
    public interface ISessionLogic
    {
        void ShowVciLossConnectionInEcuKomServiceDlg();

        void StartWatchDogTimer();

        void StopWatchDogTimer();
    }
}
