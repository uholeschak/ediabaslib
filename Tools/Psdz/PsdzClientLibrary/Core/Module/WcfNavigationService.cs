using PsdzClient;
using PsdzClient.Core;

namespace BMW.ISPI.IstaOperation.Impl
{
    [PreserveSource(Hint = "Simplified")]
    public class WcfNavigationService : INavigationService
    {
        public TabName CurrentTab
        {
            get; set;
        }

        public TabName TabRequest
        {
            get; set;
        }

        public WcfNavigationService()
        {
            CurrentTab = TabName.None;
            TabRequest = TabName.None;
        }

        public void NavigateTo(TabName targetPage)
        {
            Log.Info("WcfNavigationService.NavigateTo()", $"Navigate to tab '{targetPage}'");
            TabRequest = targetPage;
        }

        public void NavigateToLastPage()
        {
            TabName? previewsTab = TabName.None;
            if (previewsTab.HasValue)
            {
                Log.Info("WcfNavigationService.NavigateToLastPage()", $"Navigate back to tab '{previewsTab}'");
                TabRequest = previewsTab.Value;
            }
            else
            {
                Log.Error("WcfNavigationService.NavigateToLastPage()", "No previews tab existing.");
            }
        }

        public void SetTabToOriginCache(IstaNavigationOriginCacheKey origin)
        {
        }

        public void SetTabToOriginCache(IstaNavigationOriginCacheKey origin, TabName tab)
        {
        }

        public TabName GetTabFromOriginCache(IstaNavigationOriginCacheKey origin)
        {
            return TabRequest;
        }
    }
}
