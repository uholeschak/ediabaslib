using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using SimpleWifi;
using SimpleWifi.Win32.Interop;

using NotifCodeACM = SimpleWifi.Win32.Interop.WlanNotificationCodeAcm;
using NotifCodeMSM = SimpleWifi.Win32.Interop.WlanNotificationCodeMsm;

namespace EdiabasLibConfigTool
{
    public class WifiMod
    {
        public event EventHandler<WifiStatusEventArgsMod> ConnectionStatusChanged;

        private WlanClient _client;
        private WifiStatus _connectionStatus;
        private bool _isConnectionStatusSet = false;
        public bool NoWifiAvailable = false;

        public WifiMod()
        {
            _client = new WlanClient();
            NoWifiAvailable = _client.NoWifiAvailable;
            if (_client.NoWifiAvailable)
                return;

            foreach (var inte in _client.Interfaces)
                inte.WlanNotification += inte_WlanNotification;
        }

        /// <summary>
        /// Returns a list over all available access points
        /// </summary>
        public List<AccessPoint> GetAccessPoints()
        {
            List<AccessPoint> accessPoints = new List<AccessPoint>();
            if (_client.NoWifiAvailable)
                return accessPoints;

            foreach (WlanInterface wlanIface in _client.Interfaces)
            {
                try
                {
                    WlanAvailableNetwork[] rawNetworks = wlanIface.GetAvailableNetworkList(0);
                    List<WlanAvailableNetwork> networks = new List<WlanAvailableNetwork>();

                    // Remove network entries without profile name if one exist with a profile name.
                    foreach (WlanAvailableNetwork network in rawNetworks)
                    {
                        bool hasProfileName = !string.IsNullOrEmpty(network.profileName);
                        bool anotherInstanceWithProfileExists = rawNetworks.Where(n => n.Equals(network) && !string.IsNullOrEmpty(n.profileName)).Any();

                        if (!anotherInstanceWithProfileExists || hasProfileName)
                            networks.Add(network);
                    }

                    foreach (WlanAvailableNetwork network in networks)
                    {
                        accessPoints.Add(CreateInstance<AccessPoint>(wlanIface, network));
                        //accessPoints.Add(new AccessPoint(wlanIface, network));
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return accessPoints;
        }

        /// <summary>
        /// Disconnect all wifi interfaces
        /// </summary>
        public void Disconnect()
        {
            if (_client.NoWifiAvailable)
                return;

            foreach (WlanInterface wlanIface in _client.Interfaces)
            {
                wlanIface.Disconnect();
            }
        }

        public WifiStatus ConnectionStatus
        {
            get
            {
                if (!_isConnectionStatusSet)
                    ConnectionStatus = GetForcedConnectionStatus();

                return _connectionStatus;
            }
            private set
            {
                _isConnectionStatusSet = true;
                _connectionStatus = value;
            }
        }

        private static T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }

        private void inte_WlanNotification(WlanNotificationData notifyData)
        {
        if (notifyData.notificationSource == WlanNotificationSource.ACM && (NotifCodeACM)notifyData.NotificationCode == NotifCodeACM.Disconnected)
            OnConnectionStatusChanged(WifiStatus.Disconnected);
        else if (notifyData.notificationSource == WlanNotificationSource.MSM && (NotifCodeMSM)notifyData.NotificationCode == NotifCodeMSM.Connected)
            OnConnectionStatusChanged(WifiStatus.Connected);
        }

        private void OnConnectionStatusChanged(WifiStatus newStatus)
        {
            ConnectionStatus = newStatus;

            if (ConnectionStatusChanged != null) 
                ConnectionStatusChanged(this, new WifiStatusEventArgsMod(newStatus));
        }

        // I don't like this method, it's slow, ugly and should be refactored ASAP.
        private WifiStatus GetForcedConnectionStatus()
        {
            if (NoWifiAvailable)
                return WifiStatus.Disconnected;

            bool connected = false;

            foreach (var i in _client.Interfaces)
            {
                try
                {
                    var a = i.CurrentConnection; // Current connection throws an exception if disconnected.
                    connected = true;
                }
                catch {	}
            }

            if (connected)
                return WifiStatus.Connected;
            else
                return WifiStatus.Disconnected;
        }
    }

    public class WifiStatusEventArgsMod : EventArgs
    {
        public WifiStatus NewStatus { get; private set; }

        public WifiStatusEventArgsMod(WifiStatus status) : base()
        {
            this.NewStatus = status;
        }
    }
}
