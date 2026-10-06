using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using BMW.Rheingold.CoreFramework.Interaction.Models;
using BMW.Rheingold.CoreFramework.Interaction.Responses;

namespace BMW.Rheingold.xVM
{
    public static class ConnectionManagerHandler
    {
        public static InteractionConnectionManagerResponse ShowConnectionManager(Dispatcher dispatcher, ILogic logic, IInteractionService interactionService, IVciDevice connectedVci, IVciDevice connectedImib, ConnectionTargetTypes vciTypesToShow = ConnectionTargetTypes.ALL, bool shouldLogin = true)
        {
            if (interactionService == null)
            {
                throw new ArgumentNullException("interactionService");
            }

            InteractionConnectionManagerModel connectionManagerModel = new InteractionConnectionManagerModel(logic, connectedVci, connectedImib, vciTypesToShow, shouldLogin);
            ConnectionManagerDeviceService connectionManagerDeviceService = new ConnectionManagerDeviceService(logic, showAlreadyConnectedDeviceTypes: false, vciTypesToShow.ToString());
            connectionManagerModel.Devices.AddRange(connectionManagerDeviceService.Devices);
            connectionManagerModel.IsCloseButtonEnabled = true;
            connectionManagerModel.ResponseCloseButton = new InteractionConnectionManagerResponse(ConnectionManagerResponseAction.Cancel, null);
            Task<InteractionConnectionManagerResponse> task = interactionService.RegisterAsync(connectionManagerModel);
            if (dispatcher == null)
            {
                connectionManagerDeviceService.DevicesChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    DeviceServiceDevicesChanged(e, connectionManagerModel.Devices);
                };
            }
            else
            {
                connectionManagerDeviceService.DevicesChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    dispatcher.InvokeAsync(() =>
                    {
                        DeviceServiceDevicesChanged(e, connectionManagerModel.Devices);
                    });
                };
            }

            connectionManagerDeviceService.Start();
            task.Wait();
            connectionManagerDeviceService.Stop();
            connectionManagerDeviceService.Dispose();
            return task.Result;
        }

        private static InteractionConnectionManagerResponse ConnectIMIB(Dispatcher dispatcher, ILogic logic, IInteractionService interactionService, IVciDevice connectedVci, IVciDevice connectedImib)
        {
            if (interactionService == null)
            {
                throw new ArgumentNullException("interactionService");
            }

            InteractionIMIBConnectionModel imibConnectionModel = new InteractionIMIBConnectionModel(GetLocalIPAddresses());
            ConnectionManagerDeviceService connectionManagerDeviceService = new ConnectionManagerDeviceService(logic, showAlreadyConnectedDeviceTypes: false, ConnectionTargetTypes.MIB.ToString());
            imibConnectionModel.Devices.AddRange(connectionManagerDeviceService.Devices);
            imibConnectionModel.IsCloseButtonEnabled = true;
            imibConnectionModel.ResponseCloseButton = new InteractionConnectionManagerResponse(ConnectionManagerResponseAction.Cancel, null);
            Task<InteractionConnectionManagerResponse> task = interactionService.RegisterAsync(imibConnectionModel);
            if (dispatcher == null)
            {
                connectionManagerDeviceService.DevicesChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    DeviceServiceDevicesChanged(e, imibConnectionModel.Devices);
                };
            }
            else
            {
                connectionManagerDeviceService.DevicesChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    dispatcher.InvokeAsync(() =>
                    {
                        DeviceServiceDevicesChanged(e, imibConnectionModel.Devices);
                    });
                };
            }

            connectionManagerDeviceService.Start();
            task.Wait();
            connectionManagerDeviceService.Stop();
            connectionManagerDeviceService.Dispose();
            return task.Result;
        }

        private static List<string> GetLocalIPAddresses()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            return (
                from ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                where ip.AddressFamily == AddressFamily.InterNetwork
                select ip.ToString()).ToList();
        }

        private static void DeviceServiceDevicesChanged(NotifyCollectionChangedEventArgs e, ICollection<IVciDevice> devices)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (IVciDevice item in e.NewItems.OfType<IVciDevice>())
                    {
                        devices.AddIfNotContains(item);
                        Log.Info("ConnectionManagerHandler.DeviceServiceDevicesChanged()", "Device with DevId {0} added ", item.DevId);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (IVciDevice vciDevice3 in e.OldItems.OfType<IVciDevice>())
                    {
                        foreach (IVciDevice item2 in (IEnumerable<IVciDevice>)devices.Where((IVciDevice x) => x.Equals(vciDevice3)).ToList())
                        {
                            devices.Remove(item2);
                            Log.Info("ConnectionManagerHandler.DeviceServiceDevicesChanged()", "Device with DevId {0} removed ", vciDevice3.DevId);
                        }
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (IVciDevice vciDevice in e.NewItems.OfType<IVciDevice>())
                    {
                        IVciDevice vciDevice2 = devices.SingleOrDefault((IVciDevice x) => x.Equals(vciDevice));
                        if (vciDevice2 != null)
                        {
                            vciDevice2.State = vciDevice.State;
                            vciDevice2.Kl15Voltage = vciDevice.Kl15Voltage;
                            vciDevice2.Kl30Voltage = vciDevice.Kl30Voltage;
                            vciDevice2.AccuCapacity = vciDevice.AccuCapacity;
                            vciDevice2.VIN = vciDevice.VIN;
                            vciDevice2.SignalStrength = vciDevice.SignalStrength;
                            vciDevice2.NetworkType = vciDevice.NetworkType;
                            vciDevice2.VciChannels = vciDevice.VciChannels;
                            vciDevice2.IPAddress = vciDevice.IPAddress;
                            Log.Info("ConnectionManagerHandler.DeviceServiceDevicesChanged()", "Device with DevId {0} updated ", vciDevice.DevId);
                        }
                    }

                    break;
                }
            }
        }
    }
}