namespace PsdzClientLibrary.Core
{
    public interface IServiceLocator
    {
        T GetService<T>() where T : class;

        bool TryGetService<T>(out T service) where T : class;

        void AddService<T>(T service) where T : class;

        void TryAddService<T>(T service) where T : class;

        void RemoveService<T>() where T : class;
    }
}
