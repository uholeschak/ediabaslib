using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BMW.Rheingold.Module.ISTA
{
    internal class DtcAnzeigeDynUi : UserControl, IComponentConnector
    {
        internal ScrollViewer scrollViewerBottom;

        internal ListView lvError;

        private bool _contentLoaded;

        public DtcAnzeigeDynUi()
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
                Uri resourceLocator = new Uri("/RheingoldISTACoreFramework;component/servicedialoge/dtc_anzeige_dyn/dtcanzeigedynui.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "10.0.9.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    scrollViewerBottom = (ScrollViewer)target;
                    break;
                case 2:
                    lvError = (ListView)target;
                    break;
                default:
                    _contentLoaded = true;
                    break;
            }
        }
    }
}
