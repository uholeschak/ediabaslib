using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CarControl
{
    public partial class PushButton : Control
    {
        private IntPtr _oldWndProc;
        private WndProcDelegate wndProcDelegate;
        private bool _mouseDown;
        private bool _ignoreClick;

        delegate IntPtr WndProcDelegate(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);

        public IntPtr WindowHandle
        {
            get;
            private set;
        }

        public bool ButtonState
        {
            get
            {
                _ignoreClick = true;
                int result = (int) WinAPI.SendMessage(WindowHandle, WinAPI.BM_GETCHECK, (IntPtr)0, (IntPtr)0);
                _ignoreClick = false;
                if ((result & WinAPI.BST_CHECKED) != 0)
                {
                    return true;
                }
                return false;
            }
            set
            {
                WinAPI.SendMessage(WindowHandle, WinAPI.BM_SETCHECK, (IntPtr)(value ? WinAPI.BST_CHECKED : WinAPI.BST_UNCHECKED), (IntPtr)0);
            }
        }

        public PushButton()
        {
            _mouseDown = false;
            _ignoreClick = false;
            InitializeComponent();
        }

        IntPtr NewWndProc(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam)
        {
            switch (message)
            {
                case WinAPI.WM_LBUTTONDOWN:
                    _mouseDown = true;
                    break;

                case WinAPI.WM_LBUTTONUP:
                    {
                        IntPtr result = WinAPI.CallWindowProc(_oldWndProc, handle, message, wParam, lParam);
                        if (_mouseDown && !_ignoreClick)
                        {
                            Point screenPoint = PointToScreen(new Point((int)lParam & 0xFFFF, (int)lParam >> 16));
                            if (WinAPI.WindowFromPoint(screenPoint.X, screenPoint.Y) == WindowHandle)
                            {
                                OnClick(EventArgs.Empty);
                            }
                        }
                        _mouseDown = false;
                        return result;
                    }
            }
            return WinAPI.CallWindowProc(_oldWndProc, handle, message, wParam, lParam);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            WindowHandle = WinAPI.CreateWindowEx(0, "Button", Text,
                WinAPI.WS_VISIBLE | WinAPI.WS_CHILD | WinAPI.WS_TABSTOP | WinAPI.BS_AUTOCHECKBOX | WinAPI.BS_PUSHLIKE,
                0, 0,
                Width, Height, Handle,
                0, (IntPtr)0, null);
            wndProcDelegate = new WndProcDelegate(NewWndProc);
            _oldWndProc = WinAPI.GetWindowLongPtr(WindowHandle, WinAPI.GWL_WNDPROC);
            WinAPI.SetWindowLongPtr(WindowHandle, WinAPI.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(wndProcDelegate));
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (WindowHandle != (System.IntPtr)0)
            {
                if (_oldWndProc != null)
                {
                    WinAPI.SetWindowLongPtr(WindowHandle, WinAPI.GWL_WNDPROC, _oldWndProc);
                }
                WinAPI.DestroyWindow(WindowHandle);
                WindowHandle = (System.IntPtr)0;
            }
            base.OnHandleDestroyed(e);
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
}
