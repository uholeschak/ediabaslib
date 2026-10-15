namespace BMW.ISPI.TRIC.ISTA.MultisourceLogic
{
    public interface IValueValidator
    {
        bool IsValid<T>(string propertyName, object value);
    }
}
