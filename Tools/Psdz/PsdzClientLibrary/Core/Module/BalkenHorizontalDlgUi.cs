using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BMW.Rheingold.Module.ISTA
{
    public class BalkenHorizontalDlgUi : UserControl, IComponentConnector
    {
        private bool _contentLoaded;

        public BalkenHorizontalDlgUi()
        {
            InitializeComponent();
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "10.0.9.0")]
        public void InitializeComponent()
        {
            if (!_contentLoaded)
            {
                _contentLoaded = true;
                Uri resourceLocator = new Uri("/RheingoldISTACoreFramework;component/servicedialoge/balkenhorizontal/balkenhorizontaldlgui.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "10.0.9.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }
}
