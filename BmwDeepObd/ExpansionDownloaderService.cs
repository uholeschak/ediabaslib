using System;
using Android.Content;
using Android.OS;
using Google.Android.Vending.Expansion.Downloader;
using Java.Interop;

namespace BmwDeepObd
{
    [Android.App.Service(
        Name = ActivityCommon.AppNameSpace + "." + nameof(ExpansionDownloaderService)
        )]
    public class ExpansionDownloaderService : CustomDownloaderService
    {
        /// <summary>
        /// This public key comes from your Android Market publisher account, and it
        /// used by the LVL to validate responses from Market on your behalf.
        /// </summary>
        public override string PublicKey => @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwH+sp+AE5s8WueyH2XkIYuLHKmbANxahV0s7/FlUg8OTmaF5Q3L8xWheL+x0V2lzuM33C8sSJW8kpUKUleFhcI5ky1S6PalAWwUO/6UF6MDNbyWPCK9wJdn2WsZlY/XgbHKtqjUIZNeO+q0hxpy0IxlOhxsFcgJkXwtoixRPdwhLuPTChE6n/q1AXGY/xEohRx0iN/VGD8h7flwxrYsd1aS2LsNichlXxJl8tPwt7H+xkYIQJfzFGeT8OFjQP6miZYHoHM9dOQ17Ys2x538XNJYfsIqO+dT6Lfh35ix1jYRsR+2DX+zHexMfpXFyTFSHwaACs9PSiwIyGncySOLxiwIDAQAB";

        /// <summary>
        /// This is used by the preference obfuscater to make sure that your
        /// obfuscated preferences are different than the ones used by other
        /// applications.
        /// </summary>
        public override byte[] GetSalt() => new byte[] { 208, 118, 151, 140, 7, 53, 67, 126, 150, 28, 97, 17, 132, 18, 199, 108, 40, 7, 119, 241 };

        /// <summary>
        /// Fill this in with the class name for your alarm receiver. We do this
        /// because receivers must be unique across all of Android (it's a good idea
        /// to make sure that your receiver is in your unique package)
        /// </summary>
        public override string AlarmReceiverClassName => ActivityCommon.AppNameSpace + "." + nameof(ExpansionAlarmReceiver);

        [Export(SuperArgumentsString = "\"LVLDownloadService\"")]
        public ExpansionDownloaderService()
        {
        }
    }
}
