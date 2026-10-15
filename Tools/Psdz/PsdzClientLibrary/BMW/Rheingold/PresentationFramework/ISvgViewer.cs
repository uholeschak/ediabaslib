namespace BMW.Rheingold.PresentationFramework
{
    public interface ISvgViewer
    {
        bool CanZoomIn { get; }

        bool CanZoomOut { get; }

        bool HasComponentList { get; }

        event DictionaryEventHandler HotSpotClicked;

        void CleanUp();

        void Open(string filename);

        void ZoomIn();

        void ZoomOut();

        void ToggleComponentList(bool isVisible);

        void ExpandHotspots(bool canExpand);
    }
}
