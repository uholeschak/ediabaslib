using BMW.Rheingold.CoreFramework.Navigation;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    public interface INavigationService
    {
        TabName CurrentTab { get; }

        void NavigateTo(TabName target);
        void NavigateToLastPage();
        void SetTabToOriginCache(IstaNavigationOriginCacheKey origin);
        void SetTabToOriginCache(IstaNavigationOriginCacheKey origin, TabName tab);
        TabName GetTabFromOriginCache(IstaNavigationOriginCacheKey origin);
    }
}