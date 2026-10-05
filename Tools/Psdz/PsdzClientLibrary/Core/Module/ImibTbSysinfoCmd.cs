using System;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class ImibTbSysinfoCmd : ServiceDialogCmdBase
    {
        public ImibTbSysinfoCmd(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo) : base(callingModule, methodName, path, globalTabModuleISTA, elementNo)
        {
        }

        public override void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            Log.Info("ImibTbSysinfoCmd.CreateDialog()", "IMIB_TB_Sysinfo init started.");
            Display = false;
        }

        public override void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CallingModule == null)
            {
                Log.Error("ImibTbSysinfoCmd.DoInvoke()", "Failed to invoke method {0}, because calling module is null.", method);
                return;
            }

            ModuleParameter value = CallingModule.__RheinGoldCoreModuleParameters__.Clone();
            inParam.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            inParam.Parameter.Add("__RheinGoldTabModuleISTA__", CallingModule.GlobalTabModuleISTA);
            inParam.Parameter.Add("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            if (method == "Sysinfo")
            {
                string fileList_Name = inParam.getParameter("FileList_Name") as string;
                string plugIn_Name = inParam.getParameter("PlugIn_Name") as string;
                string hardware_Name = inParam.getParameter("Hardware_Name") as string;
                int IMIB_Status_Nr = 0;
                double HW_IMIB_BIOS = 0.0;
                double HW_IMIB_MB_FW = 0.0;
                bool HW_Bluetooth_Installed = false;
                bool HW_WLAN_Installed = false;
                double FileList_Version = 0.0;
                double PlugIn_Version = 0.0;
                double Hardware_Version = 0.0;
                try
                {
                    IMIB_TB_Sysinfo iMIB_TB_Sysinfo = new IMIB_TB_Sysinfo(inParam);
                    iMIB_TB_Sysinfo.Sysinfo(fileList_Name, plugIn_Name, hardware_Name, ref IMIB_Status_Nr, ref HW_IMIB_BIOS, ref HW_IMIB_MB_FW, ref HW_Bluetooth_Installed, ref HW_WLAN_Installed, ref FileList_Version, ref PlugIn_Version, ref Hardware_Version);
                    outParam.setParameter("_FASTA", iMIB_TB_Sysinfo.fastaParameterContainer);
                }
                catch (Exception exception)
                {
                    Log.WarningException("ImibTbSysinfoCmd.DoInvoke()", exception);
                }

                outParam.setParameter("IMIB_Status_Nr", IMIB_Status_Nr);
                outParam.setParameter("HW_IMIB_BIOS", HW_IMIB_BIOS);
                outParam.setParameter("HW_IMIB_MB_FW", HW_IMIB_MB_FW);
                outParam.setParameter("HW_Bluetooth_Installed", HW_Bluetooth_Installed);
                outParam.setParameter("HW_WLAN_Installed", HW_WLAN_Installed);
                outParam.setParameter("FileList_Version", FileList_Version);
                outParam.setParameter("PlugIn_Version", PlugIn_Version);
                outParam.setParameter("Hardware_Version", Hardware_Version);
            }
            else
            {
                Log.Error("ImibTbSysinfoCmd.DoInvoke()", "Unsupported method {0} will be ignored.", method);
            }
        }
    }
}