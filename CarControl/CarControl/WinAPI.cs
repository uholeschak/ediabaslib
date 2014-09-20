using System;

using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CarControl
{
    public class WinAPI
    {
        [DllImport("coredll.dll", EntryPoint = "CreateWindowEx")]
        public static extern IntPtr CeCreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int width, int height, IntPtr hWndParent,
            int hMenu, IntPtr hInstance, string lpParam);

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx")]
        public static extern IntPtr W32CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int width, int height, IntPtr hWndParent,
            int hMenu, IntPtr hInstance, string lpParam);

        [DllImport("coredll.dll", EntryPoint = "DestroyWindow")]
        public static extern bool CeDestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow")]
        public static extern bool W32DestroyWindow(IntPtr hWnd);

        [DllImport("coredll.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr CeSendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr W32SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("coredll.dll", EntryPoint = "SendMessage")]
        public static extern int CeSendMessageStr(IntPtr hWnd, int message, int data, string s);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int W32SendMessageStr(IntPtr hWnd, int message, int data, string s);

        [DllImport("coredll.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr CeWindowFromPoint(int xPoint, int yPoint);

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr W32WindowFromPoint(int xPoint, int yPoint);

        [DllImport("coredll.dll", EntryPoint = "GetWindowLong")]
        public static extern int CeGetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int W32GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("coredll.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr CeGetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr W32GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr W64GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("coredll.dll", EntryPoint = "SetWindowLong")]
        public static extern void CeSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern void W32SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr W64SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("coredll.dll", EntryPoint = "SetWindowLong")]
        public static extern void CeSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern void W32SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("coredll.dll", EntryPoint = "MoveWindow")]
        public static extern bool CeMoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool W32MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("coredll.dll", EntryPoint = "SetWindowText")]
        public static extern bool CeSetWindowText(IntPtr hWnd, String lpString);

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool W32SetWindowText(IntPtr hWnd, String lpString);

        [DllImport("coredll.dll", EntryPoint = "SetParent")]
        public static extern IntPtr CeSetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "SetParent")]
        public static extern IntPtr W32SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("coredll.dll", EntryPoint = "CallWindowProc")]
        public static extern IntPtr CeCallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "CallWindowProc")]
        public static extern IntPtr W32CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("coredll", EntryPoint = "ExitWindowsEx")]
        public static extern int CeExitWindowsEx(ExitFlags flags, int reserved);

        [DllImport("user32", EntryPoint = "ExitWindowsEx")]
        public static extern int W32ExitWindowsEx(ExitFlags flags, int reserved);

        [DllImport("Coredll.dll", EntryPoint = "CreateFile", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CeCreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", EntryPoint = "CreateFile", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr W32CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("Coredll.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CeCloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool W32CloseHandle(IntPtr hObject);

        [DllImport("coredll.dll", EntryPoint = "RegFlushKey")]
        public static extern int CeRegFlushKey(IntPtr hKey);

        [DllImport("Advapi32.dll", EntryPoint = "RegFlushKey")]
        public static extern int W32RegFlushKey(IntPtr hKey);

        [DllImport("coredll.dll", EntryPoint = "GwesPowerOffSystem")]
        public extern static void CeGwesPowerOffSystem();

        [DllImport("coredll.dll", EntryPoint = "SignalStarted")]
        public extern static void CeSignalStarted(uint dword);

        // window messages
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_ENABLE = 0x000A;
        public const int WM_COMMAND = 0x0111;
        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;

        // User Button Notification Codes
        public const int BN_CLICKED = 0;
        public const int BN_PAINT = 1;
        public const int BN_HILITE = 2;
        public const int BN_UNHILITE = 3;
        public const int BN_DISABLE =  4;
        public const int BN_DOUBLECLICKED = 5;
        public const int BN_PUSHED = BN_HILITE;
        public const int BN_UNPUSHED = BN_UNHILITE;
        public const int BN_DBLCLK = BN_DOUBLECLICKED;
        public const int BN_SETFOCUS = 6;
        public const int BN_KILLFOCUS = 7;

        // Button Control Messages
        public const int BM_GETCHECK = 0x00F0;
        public const int BM_SETCHECK = 0x00F1;
        public const int BM_GETSTATE = 0x00F2;
        public const int BM_SETSTATE = 0x00F3;
        public const int BM_SETSTYLE = 0x00F4;
        public const int BM_CLICK = 0x00F5;
        public const int BM_GETIMAGE = 0x00F6;
        public const int BM_SETIMAGE = 0x00F7;
        public const int BM_SETDONTCLICK = 0x00F8;
        public const int BST_UNCHECKED = 0x0000;
        public const int BST_CHECKED = 0x0001;
        public const int BST_INDETERMINATE = 0x0002;
        public const int BST_PUSHED = 0x0004;
        public const int BST_FOCUS = 0x0008;

        // Window styles
        public const int WS_OVERLAPPED = 0x00000000;
        public const int WS_TABSTOP = 0x00010000;
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_GROUP = 0x00020000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_SYSMENU = 0x00080000;
        public const int WS_HSCROLL = 0x00100000;
        public const int WS_VSCROLL = 0x00200000;
        public const int WS_BORDER = 0x00800000;
        public const int WS_CAPTION = 0x00C00000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_CLIPCHILDREN = 0x02000000;
        public const int WS_CLIPSIBLINGS = 0x04000000;
        public const int WS_DISABLED = 0x08000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_POPUP = unchecked((int)0x80000000);

        // Extended Window styles
        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_WINDOWEDGE = 0x00000100;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
        public const int WS_EX_CONTEXTHELP = 0x00000400;
        public const int WS_EX_STATICEDGE = 0x00020000;
        public const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);

        // GetWindowLong()
        public const int GWL_WNDPROC = (-4);
        public const int GWL_HINSTANCE = (-6);
        public const int GWL_HWNDPARENT = (-8);
        public const int GWL_STYLE = (-16);
        public const int GWL_EXSTYLE = (-20);
        public const int GWL_USERDATA = (-21);
        public const int GWL_ID = (-12);

        // Button styles
        public const int BS_PUSHBUTTON = 0x00000000;
        public const int BS_DEFPUSHBUTTON = 0x00000001;
        public const int BS_CHECKBOX = 0x00000002;
        public const int BS_AUTOCHECKBOX = 0x00000003;
        public const int BS_RADIOBUTTON = 0x00000004;
        public const int BS_3STATE = 0x00000005;
        public const int BS_AUTO3STATE = 0x00000006;
        public const int BS_GROUPBOX = 0x00000007;
        public const int BS_USERBUTTON = 0x00000008;
        public const int BS_AUTORADIOBUTTON = 0x00000009;
        public const int BS_PUSHBOX = 0x0000000A;
        public const int BS_OWNERDRAW = 0x0000000B;
        public const int BS_SPLITBUTTON = 0x0000000C;
        public const int BS_TYPEMASK = 0x0000000F;
        public const int BS_LEFTTEXT = 0x00000020;
        public const int BS_LEFT = 0x00000100;
        public const int BS_RIGHT = 0x00000200;
        public const int BS_PUSHLIKE = 0x00001000;
        public const int BS_RIGHTBUTTON = BS_LEFTTEXT;

        // Tab control styles
        public const int TCS_SCROLLOPPOSITE = 0x0001;
        public const int TCS_BOTTOM = 0x0002;
        public const int TCS_RIGHT = 0x0002;
        public const int TCS_MULTISELECT = 0x0004;
        public const int TCS_FLATBUTTONS = 0x0008;
        public const int TCS_FORCEICONLEFT = 0x0010;
        public const int TCS_FORCELABELLEFT = 0x0020;
        public const int TCS_VERTICAL = 0x0080;
        public const int TCS_BUTTONS = 0x0100;
        public const int TCS_SINGLELINE = 0x0000;
        public const int TCS_MULTILINE = 0x0200;
        public const int TCS_RIGHTJUSTIFY = 0x0000;
        public const int TCS_FIXEDWIDTH = 0x0400;
        public const int TCS_RAGGEDRIGHT = 0x0800;
        public const int TCS_FOCUSONBUTTONDOWN = 0x1000;
        public const int TCS_OWNERDRAWFIXED = 0x2000;
        public const int TCS_FOCUSNEVER = 0x8000;
        public const int TCS_EX_FLATSEPARATORS = 0x00000001;

        public const UInt32 HKEY_CLASSES_ROOT = 0x80000000;
        public const UInt32 HKEY_CURRENT_USER = 0x80000001;
        public const UInt32 HKEY_LOCAL_MACHINE = 0x80000002;
        public const UInt32 HKEY_USERS = 0x80000003;

        // CreateFile
        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 1;
        public const uint FILE_SHARE_WRITE = 2;
        public const uint FILE_SHARE_DELETE = 4;
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;

        [Flags]
        public enum ExitFlags
        {
            Reboot = 0x02,
            PowerOff = 0x08
        }

        public static IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int width, int height, IntPtr hWndParent,
            int hMenu, IntPtr hInstance, string lpParam)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeCreateWindowEx(
                    dwExStyle, lpClassName, lpWindowName,
                    dwStyle, x, y, width, height, hWndParent,
                    hMenu, hInstance, lpParam);
            }
            return W32CreateWindowEx(
                dwExStyle, lpClassName, lpWindowName,
                dwStyle, x, y, width, height, hWndParent,
                hMenu, hInstance, lpParam);
        }

        public static bool DestroyWindow(IntPtr hWnd)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeDestroyWindow(hWnd);
            }
            return W32DestroyWindow(hWnd);
        }

        public static IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeSendMessage(hWnd, Msg, wParam, lParam);
            }
            return W32SendMessage(hWnd, Msg, wParam, lParam);
        }

        public static int SendMessageStr(IntPtr hWnd, int message, int data, string s)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeSendMessageStr(hWnd, message, data, s);
            }
            return W32SendMessageStr(hWnd, message, data, s);
        }

        public static IntPtr WindowFromPoint(int xPoint, int yPoint)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeWindowFromPoint(xPoint, yPoint);
            }
            return W32WindowFromPoint(xPoint, yPoint);
        }

        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeGetWindowLong(hWnd, nIndex);
            }
            return W32GetWindowLong(hWnd, nIndex);
        }

        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeGetWindowLongPtr(hWnd, nIndex);
            }
            if (IntPtr.Size == 8)
            {
                return W64GetWindowLongPtr(hWnd, nIndex);
            }
            return W32GetWindowLongPtr(hWnd, nIndex);
        }

        public static void SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                CeSetWindowLong(hWnd, nIndex, dwNewLong);
                return;
            }
            W32SetWindowLong(hWnd, nIndex, dwNewLong);
        }

        public static void SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                CeSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                return;
            }
            if (IntPtr.Size == 8)
            {
                W64SetWindowLongPtr(hWnd, nIndex, dwNewLong);
                return;
            }
            W32SetWindowLongPtr(hWnd, nIndex, dwNewLong);
        }

        public static bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeMoveWindow( hWnd, X, Y, nWidth, nHeight, bRepaint);
            }
            return W32MoveWindow(hWnd, X, Y, nWidth, nHeight, bRepaint);
        }

        public static bool SetWindowText(IntPtr hWnd, String lpString)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeSetWindowText(hWnd, lpString);
            }
            return W32SetWindowText(hWnd, lpString);
        }

        public static IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeSetParent(hWndChild, hWndNewParent);
            }
            return W32SetParent(hWndChild, hWndNewParent);
        }

        public static IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeCallWindowProc(lpPrevWndFunc, hWnd, Msg, wParam, lParam);
            }
            return W32CallWindowProc(lpPrevWndFunc, hWnd, Msg, wParam, lParam);
        }

        public static int ExitWindowsEx(ExitFlags flags, int reserved)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeExitWindowsEx(flags, reserved);
            }
            return W32ExitWindowsEx(flags, reserved);
        }


        public static IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            )
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeCreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            }
            return W32CreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        }

        public static bool CloseHandle(IntPtr hObject)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeCloseHandle(hObject);
            }
            return W32CloseHandle(hObject);
        }

        public static int RegFlushKey(IntPtr hKey)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return CeRegFlushKey(hKey);
            }
            return W32RegFlushKey(hKey);
        }

        public static void GwesPowerOffSystem()
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                CeGwesPowerOffSystem();
                return;
            }
        }

        public static void SignalStarted(uint dword)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                CeSignalStarted(dword);
                return;
            }
        }

        public static string CharsToString(char[] c)
        {
            int nLength = 0;

            while (nLength < c.Length && c[nLength] != '\0')
            nLength++;

            return new string(c, 0, nLength);
        }
    }
}
