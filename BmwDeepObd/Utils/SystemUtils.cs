using Android.OS;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BmwDeepObd
{
    public static class SystemUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(SystemUtils).FullName;
#endif

        public static bool IsCpuStatisticsSupported()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return false;
            }
            return true;
        }

        public static List<int> GetCpuUsageStatistic()
        {
            if (!IsCpuStatisticsSupported())
            {
                return null;
            }

            try
            {
                string tempString = ExecuteTop();
                if (tempString == null)
                {
                    return null;
                }
                MatchCollection matches = Regex.Matches(tempString, "User +(\\d+)%, +System +(\\d+)%, +IOW +(\\d+)%, +IRQ +(\\d+)%", RegexOptions.IgnoreCase);
                if ((matches.Count != 1) || (matches[0].Groups.Count != 5))
                {
                    return null;
                }
                List<int> resultList = new List<int>();
                int index = 0;
                foreach (Group group in matches[0].Groups)
                {
                    if (index > 0)
                    {
                        if (Int32.TryParse(group.Value, out int value))
                        {
                            resultList.Add(value);
                        }
                    }
                    index++;
                }
                return resultList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> ExecuteCommand(string command)
        {
            Java.Lang.Process process = null;
            Java.IO.BufferedReader input = null;
            try
            {
                process = Java.Lang.Runtime.GetRuntime()?.Exec(command);
                if (process == null)
                {
                    return null;
                }

                input = new Java.IO.BufferedReader(new Java.IO.InputStreamReader(process.InputStream));
                if (process.WaitFor() != 0)
                {
                    return null;
                }

                List<string> resultList = new List<string>();
                for (; ; )
                {
                    string line = input.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (line.Length > 0)
                    {
                        resultList.Add(line);
                    }
                }
                return resultList;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                try
                {
                    input?.Close();
                    process?.Destroy();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static string ExecuteTop()
        {
            List<string> resultList = ExecuteCommand("top -n 1");
            if (resultList != null && resultList.Count > 0)
            {
                return resultList[0];
            }

            return null;
        }

        public static List<string> GetCurrentAbiDirs()
        {
            bool isArm = false;
            string abi = null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                if (Build.SupportedAbis != null && Build.SupportedAbis.Count > 0)
                {
                    abi = Build.SupportedAbis[0];
                }
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                abi = Build.CpuAbi;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(abi))
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("GetCurrentAbiDirs ABI={0}", abi));
#endif
                if (abi.Contains("arm", StringComparison.OrdinalIgnoreCase))
                {
                    isArm = true;
                }
            }

            try
            {
                string soArch = Java.Lang.JavaSystem.GetProperty("os.arch");
                if (!string.IsNullOrEmpty(soArch))
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GetCurrentAbiDirs Arch={0}", soArch));
#endif
                    if (soArch.Contains("arm", StringComparison.OrdinalIgnoreCase))
                    {
                        isArm = true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            bool is64Bit = IntPtr.Size == 8;
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetCurrentAbiDirs ARM={0}, 64Bit={1}", isArm, is64Bit));
#endif
            if (isArm)
            {
                // the release version uses _ and debug uses -
                return is64Bit ? ["arm64", "arm64_v8a", "arm64-v8a", "aarch64"] : ["armeabi_v7a", "armeabi-v7a", "arm"];
            }

            // only the _ variant is used for release and debug
            return is64Bit ? ["x86_64", "x86-64"] : ["x86", "i686"];
        }
    }
}
