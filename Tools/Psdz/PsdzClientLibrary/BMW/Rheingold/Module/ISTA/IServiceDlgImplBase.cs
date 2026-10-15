using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;

namespace BMW.Rheingold.Module.ISTA
{
    internal interface IServiceDlgImplBase<out TModel> where TModel : ServiceDialogModelBase
    {
        TModel Model { get; }
    }
}
