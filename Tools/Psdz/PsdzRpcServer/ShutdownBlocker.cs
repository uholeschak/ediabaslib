using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace PsdzRpcServer
{
    // Blocks Windows shutdown/logoff while clients are connected.
    // Uses a hidden top level window, because only windows receive WM_QUERYENDSESSION.
    internal sealed class ShutdownBlocker : IDisposable
    {
        private const int WM_DESTROY = 0x0002;
        private const int WM_CLOSE = 0x0010;
        private const int WM_QUERYENDSESSION = 0x0011;
        private const int WM_ENDSESSION = 0x0016;
        private const int WM_TIMER = 0x0113;
        private const int TimerId = 1;
        private const uint TimerPeriodMs = 1000;

        private readonly Func<int> _clientCountFunc;
        private readonly Action _endSessionAction;
        private readonly Thread _thread;
        private readonly ManualResetEventSlim _windowCreated = new ManualResetEventSlim(false);
        private readonly string _className = "PsdzRpcServerShutdownBlocker_" + Guid.NewGuid().ToString("N");
        // Keep delegate alive, it is called from native code
        private readonly WndProcDelegate _wndProc;
        private IntPtr _hwnd;
        private bool _blockReasonActive;
        private volatile bool _sessionEnding;
        private bool _disposed;

        public ShutdownBlocker(Func<int> clientCountFunc, Action endSessionAction)
        {
            _clientCountFunc = clientCountFunc ?? throw new ArgumentNullException(nameof(clientCountFunc));
            _endSessionAction = endSessionAction;
            _wndProc = WndProc;

            _thread = new Thread(MessageLoop)
            {
                IsBackground = true,
                Name = nameof(ShutdownBlocker)
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            _windowCreated.Wait(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            IntPtr hwnd = _hwnd;
            if (hwnd != IntPtr.Zero)
            {
                PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                // The window thread is blocked in WM_ENDSESSION waiting for this cleanup
                if (!_sessionEnding)
                {
                    _thread.Join(TimeSpan.FromSeconds(5));
                }
            }

            if (!_sessionEnding)
            {
                _windowCreated.Dispose();
            }
        }

        private void MessageLoop()
        {
            try
            {
                IntPtr hInstance = GetModuleHandle(null);
                WNDCLASSEX wndClass = new WNDCLASSEX
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
                    hInstance = hInstance,
                    lpszClassName = _className
                };

                if (RegisterClassEx(ref wndClass) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Hidden top level window (no HWND_MESSAGE, those don't receive WM_QUERYENDSESSION)
                _hwnd = CreateWindowEx(0, _className, "PsdzRpcServer", 0, 0, 0, 0, 0,
                    IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
                if (_hwnd == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                SetTimer(_hwnd, (UIntPtr)TimerId, TimerPeriodMs, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShutdownBlocker init failed: {ex.Message}");
                _hwnd = IntPtr.Zero;
                _windowCreated.Set();
                return;
            }

            _windowCreated.Set();

            while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            UnregisterClass(_className, GetModuleHandle(null));
        }

        private void UpdateBlockReason()
        {
            int clientCount = _clientCountFunc();
            if (clientCount > 0)
            {
                // Always update to show the current client count
                ShutdownBlockReasonCreate(_hwnd, $"PsdzRpcServer: {clientCount} client(s) connected");
                _blockReasonActive = true;
            }
            else if (_blockReasonActive)
            {
                ShutdownBlockReasonDestroy(_hwnd);
                _blockReasonActive = false;
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_TIMER:
                    UpdateBlockReason();
                    return IntPtr.Zero;

                case WM_QUERYENDSESSION:
                    UpdateBlockReason();
                    // FALSE: block shutdown while clients are connected
                    return _blockReasonActive ? IntPtr.Zero : new IntPtr(1);

                case WM_ENDSESSION:
                    if (wParam != IntPtr.Zero)
                    {
                        // Session is ending, the process is terminated after returning
                        _sessionEnding = true;
                        _endSessionAction?.Invoke();
                    }
                    return IntPtr.Zero;

                case WM_CLOSE:
                    KillTimer(hWnd, (UIntPtr)TimerId);
                    if (_blockReasonActive)
                    {
                        ShutdownBlockReasonDestroy(hWnd);
                        _blockReasonActive = false;
                    }
                    DestroyWindow(hWnd);
                    return IntPtr.Zero;

                case WM_DESTROY:
                    _hwnd = IntPtr.Zero;
                    PostQuitMessage(0);
                    return IntPtr.Zero;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        private static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, IntPtr lpTimerFunc);

        [DllImport("user32.dll")]
        private static extern bool KillTimer(IntPtr hWnd, UIntPtr uIDEvent);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, string pwszReason);

        [DllImport("user32.dll")]
        private static extern bool ShutdownBlockReasonDestroy(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
