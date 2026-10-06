using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.xVM.ENET;

#pragma warning disable CS0618
namespace BMW.Rheingold.xVM
{
    public class ConnectionManagerDeviceService : IDeviceService, IDisposable
    {
        private readonly HashSet<VCIDevice> devices;
        private readonly ILogic logic;
        private readonly List<IDeviceService> registeredDeviceServices;
        private bool disposed;
        private bool isServiceRunning;
        private Regex filter;
        private string filterAsString;
        private IEnumerable<VCIDevice> SimulationData
        {
            get
            {
                List<VCIDevice> list = new List<VCIDevice>();
                if (logic.VciConnType == EnumVCIConnectionType.sim || logic.VciConnType == EnumVCIConnectionType.ptt || logic.VciConnType == EnumVCIConnectionType.ediabas)
                {
                    VCIDevice vCIDevice = ConnectedDevices.FirstOrDefault((VCIDevice x) => x.VCIType == VCIDeviceType.SIM);
                    if (vCIDevice != null)
                    {
                        list.Add(vCIDevice);
                    }
                    else
                    {
                        list = logic.FindConnections().ToList();
                        list.ForEach((VCIDevice x) =>
                        {
                            x.DeviceState = DeviceState.Free;
                        });
                    }
                }

                return list;
            }
        }

        private IEnumerable<VCIDevice> ConnectedDevices
        {
            get
            {
                List<VCIDevice> list = new List<VCIDevice>();
                VCIDevice vCIDevice = logic.VecInfo?.MIB;
                if (vCIDevice != null)
                {
                    list.AddIfNotContains(vCIDevice);
                }

                VCIDevice vCIDevice2 = logic.VecInfo?.VCI;
                if (vCIDevice2 != null)
                {
                    list.AddIfNotContains(vCIDevice2);
                }

                return list;
            }
        }

        public IList<VCIDevice> Devices => devices.ToList();

        public Regex Filter
        {
            get
            {
                return filter;
            }

            set
            {
                filter = value;
                foreach (IDeviceService registeredDeviceService in registeredDeviceServices)
                {
                    registeredDeviceService.Filter = value;
                }
            }
        }

        public string FilterAsString
        {
            get
            {
                return filterAsString;
            }

            set
            {
                filterAsString = value;
                foreach (IDeviceService registeredDeviceService in registeredDeviceServices)
                {
                    registeredDeviceService.FilterAsString = value;
                }
            }
        }

        public event EventHandler<NotifyCollectionChangedEventArgs> DevicesChanged;
        public ConnectionManagerDeviceService(ILogic logic, bool showAlreadyConnectedDeviceTypes, string vciTypesToShow, bool excludeAlreadyConnectedDevices = false, bool includeEnetDevices = true)
        {
            devices = new HashSet<VCIDevice>();
            registeredDeviceServices = new List<IDeviceService>();
            this.logic = logic;
            IDeviceService deviceService = new VciDeviceService(this.logic.EcuKomInterface);
            if (ConfigSettings.getConfigStringAsBoolean("UseOldVciDeviceService", defaultValue: false))
            {
                (deviceService as IDisposable)?.Dispose();
                deviceService = new VciDeviceServiceOld();
            }

            deviceService.DevicesChanged += DeviceDevicesChanged;
            registeredDeviceServices.Add(deviceService);
            if (includeEnetDevices)
            {
                IDeviceService deviceService2 = new VCIEnetHsfzService(logic);
                deviceService2.DevicesChanged += DeviceDevicesChanged;
                registeredDeviceServices.Add(deviceService2);
                IDeviceService deviceService3 = new VCIEnetDoIpService(logic);
                deviceService3.DevicesChanged += DeviceDevicesChanged;
                registeredDeviceServices.Add(deviceService3);
            }

            FilterAsString = CreateFilter(ConnectedDevices, vciTypesToShow, showAlreadyConnectedDeviceTypes);
            if (vciTypesToShow != "MIB" && !excludeAlreadyConnectedDevices)
            {
                foreach (VCIDevice connectedDevice in ConnectedDevices)
                {
                    if (!string.IsNullOrEmpty(connectedDevice.DevType))
                    {
                        devices.AddIfNotContains(connectedDevice);
                    }
                }
            }

            devices.AddRange(SimulationData);
        }

        public VciDeviceService GetRegisteredVciDeviceService()
        {
            IDeviceService deviceService = registeredDeviceServices.FirstOrDefault((IDeviceService ser) => ser is VciDeviceService);
            if (deviceService == null)
            {
                return null;
            }

            return deviceService as VciDeviceService;
        }

        public void Start(IPAddress address)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            registeredDeviceServices.ForEach((IDeviceService x) =>
            {
                x.Start();
            });
            isServiceRunning = true;
        }

        public void Stop()
        {
            registeredDeviceServices.ForEach((IDeviceService x) =>
            {
                x.Stop();
            });
            isServiceRunning = false;
        }

        public VCIDevice FindVci(string serialOfVci)
        {
            bool flag = false;
            try
            {
                if (!isServiceRunning)
                {
                    Start();
                    flag = true;
                }

                return GetVCIDevice(serialOfVci);
            }
            finally
            {
                if (isServiceRunning & flag)
                {
                    Stop();
                }
            }
        }

        private VCIDevice GetVCIDevice(string serialNr)
        {
            int num = ConfigSettings.getConfigint("Bmw.Rheingold.ConnectionManager.SearchLimit", 5) * 1000 / 500;
            int num2 = 0;
            while (num2 < num)
            {
                VCIDevice vCIDevice = devices.FirstOrDefault((VCIDevice x) => string.Equals(x.Serial, serialNr, StringComparison.OrdinalIgnoreCase));
                if (vCIDevice != null)
                {
                    return vCIDevice;
                }

                num2++;
                Thread.Sleep(500);
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private string CreateFilter(IEnumerable<VCIDevice> conDevs, string vciDeviceTypeToShow, bool showAlreadyConnectedDeviceTypes)
        {
            string text = string.Empty;
            bool flag = false;
            bool flag2 = false;
            if (!showAlreadyConnectedDeviceTypes && vciDeviceTypeToShow != "MIB" && ConnectedDevices.Any())
            {
                foreach (VCIDevice item in conDevs.Where((VCIDevice x) => x.VCIType != VCIDeviceType.UNKNOWN && x.VCIType != VCIDeviceType.INFOSESSION))
                {
                    if (!string.IsNullOrEmpty(item.Serial))
                    {
                        text += string.Format("(Serial={0})|", item.Serial.Replace("-", "\\-"));
                    }

                    if (item.VCIType == VCIDeviceType.IMIB)
                    {
                        VCIDevice vCIDevice = logic.VecInfo?.MIB;
                        if (int.Parse(vCIDevice.State) != 4 && logic.WasImibConnected)
                        {
                            flag = true;
                        }

                        if (int.Parse(item.State) == 4 && logic.WasImibConnected && item.Serial == vCIDevice.Serial)
                        {
                            text = string.Empty;
                        }
                    }
                    else
                    {
                        flag2 = true;
                    }
                }

                if (!flag && (vciDeviceTypeToShow == "MIB" || vciDeviceTypeToShow == "ALL"))
                {
                    text += "(DevType=IMIB)|";
                }

                if (!flag2 && (vciDeviceTypeToShow == "VCI" || vciDeviceTypeToShow == "ALL"))
                {
                    text += "(DevType=ICOM|ENET|ICOM-Next)|";
                }
            }
            else
            {
                if (vciDeviceTypeToShow == "MIB" || vciDeviceTypeToShow == "ALL")
                {
                    text += "(DevType=IMIB)|";
                }

                if (vciDeviceTypeToShow == "VCI" || vciDeviceTypeToShow == "ALL")
                {
                    text += "(DevType=ICOM|ENET|ICOM-Next)|";
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                text = text.TrimEnd('|');
            }

            return text;
        }

        private void OnDevicesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            DevicesChanged?.Invoke(sender, args);
        }

        private void DeviceDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace || e.Action == NotifyCollectionChangedAction.Remove)
            {
                devices.RemoveWhere((VCIDevice x) => e.OldItems.Contains(x));
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (VCIDevice vciDevice in e.NewItems)
                {
                    if (ConnectedDevices.Contains(vciDevice))
                    {
                        if (vciDevice.VCIType == VCIDeviceType.IMIB)
                        {
                            VCIDevice vCIDevice = ConnectedDevices.FirstOrDefault((VCIDevice x) => x.Serial == vciDevice.Serial);
                            if (vCIDevice != null && vCIDevice.State != vciDevice.State)
                            {
                                vCIDevice.State = vciDevice.State;
                                if (int.Parse(vciDevice.State) == 4)
                                {
                                    UpdateFilter(vCIDevice.Serial);
                                }
                            }
                        }

                        vciDevice.IsConnected = true;
                    }

                    devices.Add(vciDevice);
                }
            }

            OnDevicesChanged(sender, e);
        }

        private void UpdateFilter(string serial)
        {
            string input = FilterAsString;
            try
            {
                input = Regex.Replace(input, "\\(Serial=[^)]*\\)", "(DevType=IMIB)");
                Filter = new Regex(input);
            }
            catch (Exception ex)
            {
                Log.Error(Log.CurrentMethod(), "Update of ConnectionManager filter failed:", ex.Message);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (IDeviceService registeredDeviceService in registeredDeviceServices)
                {
                    registeredDeviceService.DevicesChanged -= DeviceDevicesChanged;
                    if (registeredDeviceService is IDisposable)
                    {
                        ((IDisposable)registeredDeviceService).Dispose();
                    }
                }

                registeredDeviceServices.Clear();
            }

            disposed = true;
        }
    }
}