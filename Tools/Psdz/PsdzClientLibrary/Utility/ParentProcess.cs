using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace PsdzClient.Utility
{
    namespace ParentProcess
    {
        public class ParentProcess
        {
            public static Process ParentProcessObject
            {
                get { return GetParentProcess(); }
            }

            public static String ProcessName
            {
                get { return GetParentProcess().ProcessName; }
            }

            public static int ProcessId
            {
                get { return GetParentProcess().Id; }
            }

            public static String FullPath
            {
                get
                {
                    return GetParentProcess().MainModule.FileName;
                }
            }

            public static String FileName
            {
                get
                {
                    return System.IO.Path.GetFileName(GetParentProcess().MainModule.FileName);
                }
            }

            public static String DirectoryName
            {
                get
                {
                    return System.IO.Path.GetDirectoryName(GetParentProcess().MainModule.FileName);
                }
            }

            private static Process GetParentProcess()
            {
                int iParentPid = 0;
                int iCurrentPid = Process.GetCurrentProcess().Id;

                IntPtr oHnd = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

                if (oHnd == IntPtr.Zero)
                    return null;

                PROCESSENTRY32 oProcInfo = new PROCESSENTRY32();

                oProcInfo.dwSize =
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(PROCESSENTRY32));

                if (Process32First(oHnd, ref oProcInfo) == false)
                    return null;

                do
                {
                    if (iCurrentPid == oProcInfo.th32ProcessID)
                        iParentPid = (int)oProcInfo.th32ParentProcessID;
                }
                while (iParentPid == 0 && Process32Next(oHnd, ref oProcInfo));

                if (iParentPid > 0)
                    return Process.GetProcessById(iParentPid);
                else
                    return null;
            }

            static uint TH32CS_SNAPPROCESS = 2;

            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESSENTRY32
            {
                public uint dwSize;
                public uint cntUsage;
                public uint th32ProcessID;
                public IntPtr th32DefaultHeapID;
                public uint th32ModuleID;
                public uint cntThreads;
                public uint th32ParentProcessID;
                public int pcPriClassBase;
                public uint dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szExeFile;
            };

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

            [DllImport("kernel32.dll")]
            static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

            [DllImport("kernel32.dll")]
            static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
        }
    }
}
