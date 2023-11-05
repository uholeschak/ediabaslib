namespace PsdzClientLibrary.Core
{
    public interface IValueValidator
    {
        bool IsValid<T>(string propertyName, object value);
    }
}
