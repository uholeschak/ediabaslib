using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core.Container;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.PresentationFramework
{
    public interface IDiagnosticsModuleCoreTab : IModuleExecutionParent
    {
        object SelectedTab { set; }

        string AttributWert { get; set; }

        CommandBase CommandZoomIn { get; }

        CommandBase CommandZoomOut { get; }

        ICommand CommandToggleKeyboard { get; }

        bool CanGoForward { get; }

        bool CannotGoForward { get; }

        bool NavigatingDenied { get; set; }

        new bool IsKeyboardEnabled { get; set; }

        bool IsAblTabsFullscreenEnabled { get; set; }

        bool IsKeyboardVisible { get; set; }

        DockPanel FrameDock { get; }

        ItemsControl FaultCodeControl { get; }

        Button CustomButton0 { get; }

        Button CustomButton1 { get; }

        Button CustomButton2 { get; }

        Button ReloadButton { get; }

        Button KeyboardButton { get; }

        Button UpdateDetailsButton { get; }

        TabControl DocTabs { get; }

        bool NextButtonCloseModule { set; }

        bool IsReloadButtonEnabled { set; }

        bool IsDocTabSelected { set; }

        bool IsBackEnabled { set; }

        bool CanShowSvgComponentList { get; set; }

        bool IsScreenModeSelectionEnabled { set; }

        IEcuKom EcuKom { get; }

        Logic MyLogic { get; }

        Vehicle VecInfo { get; }

        ModuleParameter ModuleParameters { get; }

        string TabTitel { get; }

        IAbortable Abortable { set; }

        ISvgViewer SvgViewer { get; }

        [Obsolete("static function")]
        string CreateHeading(InfoObject doc);

        [Obsolete("static function")]
        string CreatePageTitle(InfoObject document);

        void OpenDocumentViewer(InfoObject document);

        void OnPropertyChanged(string info);

        new void SetScreenMode(uint mode);

        new void ResetPageTitle();

        void SetPageTitle(string title);

        new uint GetScreenMode();

        void SetScrollBarEnabledState(bool enabled);

        void Close();

        new void Close(bool abort);

        void SetFastaEndzeitpunkt();

        void UpdateButtonLine();
    }
}
