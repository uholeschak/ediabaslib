using System.Runtime.InteropServices;
using System.Threading;

namespace BMW.Rheingold.CoreFramework.Media
{
    public class VolumeChanger
    {
        private const byte VK_VOLUME_DOWN = 174;

        private const byte VK_VOLUME_UP = 175;

        private const uint KEYEVENTF_EXTENDEDKEY = 1u;

        private const uint KEYEVENTF_KEYUP = 2u;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern byte MapVirtualKey(uint uCode, uint uMapType);

        public static void SetVolume(int level)
        {
            Mute();
            uint num = NormalizeVolume(level) / 2;
            for (int i = 0; i < num; i++)
            {
                VolumeUp();
            }
            Thread.Sleep(1000);
        }

        private static uint NormalizeVolume(int level)
        {
            if (level < 0)
            {
                return 0u;
            }
            if (level > 100)
            {
                return 100u;
            }
            return (uint)level;
        }

        private static void VolumeUp()
        {
            keybd_event(175, MapVirtualKey(175u, 0u), 1u, 0u);
            keybd_event(175, MapVirtualKey(175u, 0u), 3u, 0u);
        }

        private static void VolumeDown()
        {
            keybd_event(174, MapVirtualKey(174u, 0u), 1u, 0u);
            keybd_event(174, MapVirtualKey(174u, 0u), 3u, 0u);
        }

        private static void Mute()
        {
            for (int i = 0; i < 50; i++)
            {
                VolumeDown();
            }
        }
    }
}
