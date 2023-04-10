using BMW.Rheingold.CoreFramework;
namespace PsdzClient.Core
{
    public interface IParameters
    {
        HardenedStringObjectDictionary Parameter { get; }

        void cloneParameters(ParameterContainer cloneParams);

        object getParameter(string name);

        object getParameter(string name, object defaultValue);

        void setParameter(string name, object parameter);

        void clearParameter(string name);

        void clearParameters();
    }
}
