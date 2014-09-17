using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CarControl
{
    public partial class GroupBox : Control
    {
        public IntPtr WindowHandle
        {
            get;
            private set;
        }

        public GroupBox()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (Environment.OSVersion.Platform != PlatformID.WinCE)
            {   // force paint of background
                int style = WinAPI.GetWindowLong(Handle, WinAPI.GWL_STYLE);
                WinAPI.SetWindowLong(Handle, WinAPI.GWL_STYLE, style & ~WinAPI.WS_CLIPCHILDREN);
            }

            WindowHandle = WinAPI.CreateWindowEx(0, "Button", Text,
                WinAPI.WS_VISIBLE | WinAPI.WS_CHILD | WinAPI.BS_GROUPBOX,
                0, 0,
                Width, Height, Handle,
                0, (IntPtr)0, null);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (WindowHandle != (System.IntPtr)0)
            {
                WinAPI.DestroyWindow(WindowHandle);
                WindowHandle = (System.IntPtr)0;
            }
            base.OnHandleDestroyed(e);
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new GroupBoxControlCollection(this);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (WindowHandle != (IntPtr)0)
            {
                WinAPI.SendMessage(WindowHandle, WinAPI.WM_ENABLE, (IntPtr)(Enabled ? 1 : 0), (IntPtr)0);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (WindowHandle != (IntPtr)0)
            {
                WinAPI.SetWindowText(WindowHandle, Text);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowHandle != (IntPtr)0)
            {
                WinAPI.MoveWindow(WindowHandle, 0, 0, Width, Height, true);
            }
        }
    }

    public class GroupBoxControlCollection : Control.ControlCollection
    {
        private Control ownerControl;

        public GroupBoxControlCollection(Control owner)
            : base(owner)
        {
            ownerControl = owner;
        }

        public override void Add(Control value)
        {
            base.Add(value);

            GroupBox ownerGroupBox = ownerControl as GroupBox;
            if (ownerGroupBox != null)
            {
                WinAPI.SetParent(value.Handle, ownerGroupBox.WindowHandle);
            }
        }
    }

}
