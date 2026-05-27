using System;

namespace PsdzRpcServer.Shared
{
    public static class PsdzRpcTools
    {
#if WINDOWS || NETFRAMEWORK
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const uint ENABLE_QUICK_EDIT = 0x0040;
        public static void DisableQuickEdit()
        {
            IntPtr handle = GetStdHandle(-10); // STD_INPUT_HANDLE
            if (GetConsoleMode(handle, out uint mode))
            {
                mode &= ~ENABLE_QUICK_EDIT; // ENABLE_QUICK_EDIT_MODE entfernen
                SetConsoleMode(handle, mode);
            }
        }
#endif
    }
}
