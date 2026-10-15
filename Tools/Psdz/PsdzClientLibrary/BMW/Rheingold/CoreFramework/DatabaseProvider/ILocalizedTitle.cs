namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface ILocalizedTitle
    {
        string Title { get; }

        string GetLocalizedTitle(string language);
    }
}
