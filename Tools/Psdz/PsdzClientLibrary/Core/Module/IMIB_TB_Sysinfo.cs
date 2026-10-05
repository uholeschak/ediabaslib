using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Measurement.Common.Contract;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.Module.ISTA
{
    public class IMIB_TB_Sysinfo : ISTAModule
    {
        public string Ergebnis;

        public ITextLocator txtMeldung;

        internal ParameterContainer fastaParameterContainer = new ParameterContainer();

        private string methodName;

        private DateTime startTime;

        public IMIB_TB_Sysinfo(ParameterContainer InParameter)
        {
            methodName = (InParameter.getParameter("methodname") as string) ?? "n/a";
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            Ergebnis = "";
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        private void AddMessageToFasta(int elementNumber, string meldungsText, string antwort)
        {
            AddMessageToFasta("IMIB_SysInfo", elementNumber, meldungsText, antwort);
        }

        private void AddMessageToFasta(string art, int elementNumber, string meldungsText, string antwort)
        {
            if (FastaProtocoler != null)
            {
                IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram(art, methodName);
                action.StartTime = startTime;
                action.SpecialAction.Display = false;
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText(meldungsText, x)));
                action.SpecialAction.CreateAndAddMessageText(list);
                List<LocalizedText> list2 = new List<LocalizedText>();
                list2.AddRange(logic.Lang.Select((string x) => new LocalizedText(antwort, x)));
                action.SpecialAction.AddAnswer(list2, null);
            }
            else
            {
                Log.Error("IMIB_TB_Sysinfo.AddMessageToFasta()", "No FASTA available.");
            }
        }

        private bool UnloadPlugIn(IDeviceGeneric traceBuffer, ref int IMIB_Status_Nr, ref double HW_IMIB_BIOS, ref double HW_IMIB_MB_FW, ref bool HW_Bluetooth_Installed, ref bool HW_WLAN_Installed, ref double FileList_Version, ref double PlugIn_Version)
        {
            bool status = traceBuffer.LoadPlugin(null, currentDomain: false).Status;
            if (!status)
            {
                AddMessageToFasta(2308, "IMIB Fehler:", "-1");
                LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "IMIB_SysInfo", "IMIB Fehler:", "-1");
                IMIB_Status_Nr = -1;
                HW_IMIB_BIOS = -1.0;
                HW_IMIB_MB_FW = -1.0;
                HW_Bluetooth_Installed = false;
                HW_WLAN_Installed = false;
                FileList_Version = -1.0;
                PlugIn_Version = -1.0;
            }
            return status;
        }

        private void CallMessageServiceDlg(string _methodName)
        {
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer outParam = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            parameterContainer.setParameter("txtParam", __Text("70011869707"));
            parameterContainer.setParameter("Quittierung", false);
            parameterContainer.setParameter("TIMEOUT", 0);
            parameterContainer.setParameter("Protocol", false);
            parameterContainer.setParameter("Display", true);
            base.Factory.CreateServiceDialog(this, _methodName, "51915403", _globalTabModuleISTA, 85, parameterContainer, parameterContainer2).Invoke("InitializeDialog", parameterContainer, outParam, parameterContainer2);
        }

        private void CallImibReserveAdapter(string _methodName)
        {
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer outParam = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            base.Factory.CreateServiceDialog(this, _methodName, "51695499", _globalTabModuleISTA, 3145, parameterContainer, parameterContainer2).Invoke("ReserveIMIBAdapter", parameterContainer, outParam, parameterContainer2);
        }

        public virtual void Sysinfo(string FileList_Name, string PlugIn_Name, string Hardware_Name, ref int IMIB_Status_Nr, ref double HW_IMIB_BIOS, ref double HW_IMIB_MB_FW, ref bool HW_Bluetooth_Installed, ref bool HW_WLAN_Installed, ref double FileList_Version, ref double PlugIn_Version, ref double Hardware_Version)
        {
            startTime = DateTime.Now;
            base.SPEUserInterface.DisplayWaitCursor(bWaitCursor: true);
            Logger.WriteInformation("Sysinfocalled");
            if (FileList_Name == null)
            {
                FileList_Name = "dummy";
            }
            if (PlugIn_Name == null || "sysinfo".Equals(PlugIn_Name, StringComparison.OrdinalIgnoreCase))
            {
                PlugIn_Name = "sys_info";
            }
            if (Hardware_Name == null)
            {
                Hardware_Name = "IMIB_TYPE";
            }
            CallMessageServiceDlg("Sysinfo");
            CallImibReserveAdapter("Sysinfo");
            base.SPEUserInterface.DisplayWaitCursor(bWaitCursor: true);
            IDeviceImib deviceImib = base.MeasurementLauncher.ReserveMeasurementDevice();
            IDeviceGeneric deviceGeneric = null;
            if (deviceImib != null)
            {
                deviceGeneric = deviceImib.VirtualDevice;
            }
            if (deviceGeneric == null)
            {
                throw new ArgumentNullException("The generic device is not available.");
            }
            UnloadPlugIn(deviceGeneric, ref IMIB_Status_Nr, ref HW_IMIB_BIOS, ref HW_IMIB_MB_FW, ref HW_Bluetooth_Installed, ref HW_WLAN_Installed, ref FileList_Version, ref PlugIn_Version);
            Sleep(1000);
            if (deviceGeneric == null || !deviceGeneric.LoadPlugin("sysinfo", currentDomain: false).Status)
            {
                AddMessageToFasta(26637, "IMIB Fehler:", "-2");
                LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "IMIB_SysInfo", "IMIB Fehler:", Convert.ToString(-2));
                IMIB_Status_Nr = -2;
                HW_IMIB_BIOS = -1.0;
                HW_IMIB_MB_FW = -1.0;
                HW_Bluetooth_Installed = false;
                HW_WLAN_Installed = false;
                FileList_Version = -1.0;
                PlugIn_Version = -1.0;
                Logger.WriteInformation("_ExitIndex is: 0");
                return;
            }
            IMIB_Status_Nr = 1;
            int num = 0;
            Ergebnis = string.Empty;
            int num2 = 6;
            do
            {
                try
                {
                    Sleep(1500);
                    Ergebnis = deviceGeneric.GetStatus().AnswerStatus;
                    Log.Info("IMIB_TB_Sysinfo.Sysinfo()", "GetStatus() result was: {0}", Ergebnis);
                }
                catch (Exception exception)
                {
                    Log.ErrorException("IMIB_TB_Sysinfo.Sysinfo()", exception);
                }
            }
            while (num++ < num2 && (string.IsNullOrEmpty(Ergebnis) || Ergebnis.Count() < 10));
            if (string.IsNullOrEmpty(Ergebnis))
            {
                Log.Error("IMIB_TB_Sysinfo.Sysinfo()", "Got no status from the device.");
            }
            AddMessageToFasta(26638, "TraceBuffer: SysInfo", Ergebnis);
            LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "IMIB_SysInfo", "TraceBuffer: SysInfo", Ergebnis);
            IDictionary<string, GenericDeviceStatusInfo> dictionary = GenericDeviceReadResult.ParseStatusResult(Ergebnis);
            deviceGeneric.LoadPlugin("", currentDomain: false);
            string searchString = "IMIB_BIOS";
            HW_IMIB_BIOS = ((!dictionary.ContainsKey(searchString)) ? (-1) : ((dictionary[searchString] != null) ? dictionary[searchString].VersionAsNumber : (-1)));
            searchString = "IMIB_MB_FW";
            HW_IMIB_MB_FW = ((!dictionary.ContainsKey(searchString)) ? (-1) : ((dictionary[searchString] != null) ? dictionary[searchString].VersionAsNumber : (-1)));
            searchString = "IMIB_Bluetooth";
            HW_Bluetooth_Installed = dictionary.ContainsKey(searchString) && "TRUE".Equals((dictionary[searchString] != null) ? dictionary[searchString].Value : "FALSE", StringComparison.OrdinalIgnoreCase);
            searchString = "IMIB_WLAN";
            HW_WLAN_Installed = dictionary.ContainsKey(searchString) && "TRUE".Equals((dictionary[searchString] != null) ? dictionary[searchString].Value : "FALSE", StringComparison.OrdinalIgnoreCase);
            searchString = FileList_Name;
            KeyValuePair<string, GenericDeviceStatusInfo> keyValuePair = dictionary.SingleOrDefault((KeyValuePair<string, GenericDeviceStatusInfo> x) => x.Key.Equals(searchString, StringComparison.OrdinalIgnoreCase));
            if (keyValuePair.Value != null)
            {
                FileList_Version = keyValuePair.Value.VersionAsNumber;
            }
            AddMessageToFasta(26639, FileList_Name, FileList_Version.ToString(CultureInfo.InvariantCulture));
            LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "IMIB_SysInfo", FileList_Name, FileList_Version);
            searchString = PlugIn_Name;
            keyValuePair = dictionary.SingleOrDefault((KeyValuePair<string, GenericDeviceStatusInfo> x) => x.Key.Equals(searchString, StringComparison.OrdinalIgnoreCase));
            if (keyValuePair.Value != null)
            {
                PlugIn_Version = keyValuePair.Value.VersionAsNumber;
            }
            AddMessageToFasta(26640, PlugIn_Name, PlugIn_Version.ToString(CultureInfo.InvariantCulture));
            LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "IMIB_SysInfo", PlugIn_Name, PlugIn_Version);
            if ("IMIB_TYPE".Equals(Hardware_Name, StringComparison.OrdinalIgnoreCase) && dictionary.ContainsKey("IMIB_TYPE") && dictionary["IMIB_TYPE"] != null)
            {
                Hardware_Version = dictionary["IMIB_TYPE"].VersionAsNumber;
            }
            else
            {
                searchString = Hardware_Name;
                keyValuePair = dictionary.SingleOrDefault((KeyValuePair<string, GenericDeviceStatusInfo> x) => x.Key.Equals(searchString, StringComparison.OrdinalIgnoreCase));
                Hardware_Version = ((keyValuePair.Value != null) ? ((double)keyValuePair.Value.VersionAsNumber) : 0.0);
            }
            AddMessageToFasta(26642, Hardware_Name, Hardware_Version.ToString(CultureInfo.InvariantCulture));
            LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "IMIB_SysInfo", Hardware_Name, Hardware_Version);
            base.SPEUserInterface.DisplayWaitCursor(bWaitCursor: false);
        }
    }
}
