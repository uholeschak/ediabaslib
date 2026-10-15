namespace BMW.Rheingold.CoreFramework.Programming.Error
{
    public interface IError
    {
        string Code { get; }

        string Description { get; }
    }
}