using BMW.Rheingold.CoreFramework.Module;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BMW.Rheingold.ISTA.CoreFramework.Module
{
    public class InputListener : IInputListener, IDisposable
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 513,
            WM_LBUTTONUP = 514,
            WM_MOUSEMOVE = 512,
            WM_MOUSEWHEEL = 522,
            WM_RBUTTONDOWN = 516,
            WM_RBUTTONUP = 517
        }

        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;

            public uint mouseData;

            public uint flags;

            public uint time;

            public IntPtr dwExtraInfo;
        }

        private struct POINT
        {
            public int x;

            public int y;
        }

        private const int WH_KEYBOARD_LL = 13;

        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 256;

        private static IntPtr keyboardHookID = IntPtr.Zero;

        private static IntPtr mouseHookID = IntPtr.Zero;

        private readonly Logic logic;

        private LowLevelKeyboardProc keyboardProc;

        private LowLevelMouseProc mouseProc;

        public bool InputReceived => logic.HasInputBeenDetectedInModule;

        public bool IsListening => logic.IsInputListenerActive;

        public InputListener(Logic logic)
        {
            this.logic = logic;
            if (logic == null)
            {
                throw new ArgumentException("logic cannot be null.");
            }
            mouseProc = MouseHookCallback;
            keyboardProc = KeyboardHookCallback;
        }

        public void Dispose()
        {
            ClearEventHandlers();
        }

        public void Reset()
        {
            logic.HasInputBeenDetectedInModule = false;
            if (IsListening)
            {
                StartListening();
            }
        }

        public void StartListening()
        {
            if (!InputReceived)
            {
                RemoveEventHandlers();
                if (!Debugger.IsAttached)
                {
                    keyboardHookID = SetHook(keyboardProc);
                    mouseHookID = SetHook(mouseProc);
                    logic.IsInputListenerActive = true;
                    Log.Info("InputListener.StartListening()", "Start detecting keyboard/mouse input events.");
                }
            }
        }

        public void StopListening()
        {
            logic.IsInputListenerActive = false;
            ClearEventHandlers();
            Log.Info("InputListener.StopListening()", "Stop detecting keyboard/mouse input events.");
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule processModule = process.MainModule)
                {
                    return SetWindowsHookEx(13, proc, GetModuleHandle(processModule.ModuleName), 0u);
                }
            }
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule processModule = process.MainModule)
                {
                    return SetWindowsHookEx(14, proc, GetModuleHandle(processModule.ModuleName), 0u);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        private void ClearEventHandlers()
        {
            mouseHookID = IntPtr.Zero;
            keyboardHookID = IntPtr.Zero;
            RemoveEventHandlers();
        }

        private void InputDetected()
        {
            RemoveEventHandlers();
            logic.HasInputBeenDetectedInModule = true;
            Log.Info("InputListener.InputDetected()", $"InputReceived set to {InputReceived}.");
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)256)
            {
                InputDetected();
            }
            return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && ((int)wParam == 513 || (int)wParam == 516))
            {
                InputDetected();
            }
            return CallNextHookEx(mouseHookID, nCode, wParam, lParam);
        }

        private void RemoveEventHandlers()
        {
            _ = keyboardHookID;
            UnhookWindowsHookEx(keyboardHookID);
            _ = mouseHookID;
            UnhookWindowsHookEx(mouseHookID);
        }
    }
}
