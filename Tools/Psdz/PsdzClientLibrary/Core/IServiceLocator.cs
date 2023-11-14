namespace PsdzClientLibrary.Core
{
    public interface IServiceLocator
    {
        T GetService<T>() where T : class;

        void AddService<T>(T service) where T : class;

        void TryAddService<T>(T service) where T : class;

        void RemoveService<T>() where T : class;
    }
}
