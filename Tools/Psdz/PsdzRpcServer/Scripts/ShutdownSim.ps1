param(
    # Send WM_ENDSESSION after WM_QUERYENDSESSION, this stops the server
    [switch]$EndSession
)

if (-not ('ShutdownSim' -as [type]))
{
Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class ShutdownSim
{
    delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr hWnd, StringBuilder sb, int max);
    [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern bool ShutdownBlockReasonQuery(IntPtr hWnd, StringBuilder sb, ref uint len);

    public static IntPtr Find()
    {
        IntPtr found = IntPtr.Zero;
        EnumWindows((h, l) =>
        {
            StringBuilder sb = new StringBuilder(256);
            GetClassName(h, sb, sb.Capacity);
            if (sb.ToString().StartsWith("PsdzRpcServerShutdownBlocker_")) { found = h; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    public static string Reason(IntPtr h)
    {
        uint len = 256;
        StringBuilder sb = new StringBuilder((int)len);
        return ShutdownBlockReasonQuery(h, sb, ref len) ? sb.ToString() : "<none>";
    }
}
"@
}

$hwnd = [ShutdownSim]::Find()
if ($hwnd -eq [IntPtr]::Zero)
{
    "ShutdownBlocker window not found"; return 
}
"Window: 0x{0:X}" -f $hwnd.ToInt64()
"Block reason: " + [ShutdownSim]::Reason($hwnd)

# WM_QUERYENDSESSION (0x11): 0 = shutdown blocked, 1 = allowed
$r = [ShutdownSim]::SendMessage($hwnd, 0x11, [IntPtr]::Zero, [IntPtr]::Zero)
"WM_QUERYENDSESSION result: $r"

# WM_ENDSESSION (0x16) with wParam=1: session ends -> server stops
if ($EndSession)
{
    if ($r -eq [IntPtr]::Zero)
    {
        "Shutdown blocked, WM_ENDSESSION not sent"
        return
    }

    [void][ShutdownSim]::SendMessage($hwnd, 0x16, [IntPtr]1, [IntPtr]::Zero)
    "WM_ENDSESSION sent"
}
