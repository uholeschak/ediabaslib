#if NETFRAMEWORK || WINDOWS
using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace EdiabasLib
{
    /// <summary>
    /// "Auto" bridge: publishes the real clamp state (ignition via the DSR line) into a named
    /// shared memory region, read by the ISTAVoltageControl injection DLL hosted in
    /// IstaOperation.exe. Reproduces the behaviour of the legacy obd32.dll (continuous line
    /// monitoring), but without any traffic on the bus: DsrHolding is just a control-line read
    /// (GetCommModemStatus).
    ///
    /// Passive sampling on a 1 s timer, started/stopped by EdInterfaceConnect/Disconnect.
    /// No influence on diagnostics: on any error it publishes "disconnected".
    ///
    /// Layout (MUST match the AUTO block of ISTAVoltageControl.SharedVoltageState):
    ///   20 uint AutoMagic | 24 int AutoFlags(bit0 connected) | 28 int IgnMv | 32 int BattMv | 36 int TickMs
    /// </summary>
    internal static class EdiabasVoltagePublisher
    {
        private const string MapName = "ISTAVoltageControlSharedState";
        private const int Capacity = 64;
        private const uint AutoMagic = 0x4E415554u; // "NAUT"
        private const int FlagConnected = 0x1;

        private const int OffAutoMagic = 20;
        private const int OffAutoFlags = 24;
        private const int OffAutoIgn = 28;
        private const int OffAutoBatt = 32;
        private const int OffAutoTick = 36;

        private static readonly object Lock = new object();
        private static MemoryMappedFile _mmf;
        private static Timer _timer;
        private static Func<bool> _connected;
        private static Func<bool?> _dsr;
        private static Func<int> _ignNom;
        private static Func<int> _battNom;
        // Last published clamp values: the configured nominals (EnetIgnitionVoltage/
        // EnetBatteryVoltage, or the OBD equivalents) when the clamp was last read ON, 0 when
        // OFF. NOT a measurement (ENET only reports on/off). Held when a sample is momentarily
        // unreadable so a transient read failure does not collapse the display to 0 V = "off".
        private static int _lastIgnMv;
        private static int _lastBattMv;

        /// <summary>Starts sampling (called when the interface connects).</summary>
        public static void Start(Func<bool> connected, Func<bool?> dsr, Func<int> ignNomMv, Func<int> battNomMv)
        {
            lock (Lock)
            {
                StopInternal();
                try
                {
                    _connected = connected;
                    _dsr = dsr;
                    _ignNom = ignNomMv;
                    _battNom = battNomMv;
                    _mmf = MemoryMappedFile.CreateOrOpen(MapName, Capacity, MemoryMappedFileAccess.ReadWrite);
                    _timer = new Timer(OnTick, null, 0, 1000);
                }
                catch
                {
                    StopInternal();
                }
            }
        }

        /// <summary>Stops sampling and publishes "disconnected" (called when the interface disconnects).</summary>
        public static void Stop()
        {
            lock (Lock)
            {
                try
                {
                    PublishUnsafe(false, 0, 0);
                }
                catch
                {
                }
                StopInternal();
            }
        }

        private static void StopInternal()
        {
            try { _timer?.Dispose(); } catch { }
            try { _mmf?.Dispose(); } catch { }
            _timer = null;
            _mmf = null;
            _connected = null;
            _dsr = null;
            _ignNom = null;
            _battNom = null;
            _lastIgnMv = 0;
            _lastBattMv = 0;
        }

        private static void OnTick(object state)
        {
            // Snapshot the delegates under lock (Stop may run concurrently).
            Func<bool> connected;
            Func<bool?> dsr;
            Func<int> ignNom;
            Func<int> battNom;
            lock (Lock)
            {
                connected = _connected;
                dsr = _dsr;
                ignNom = _ignNom;
                battNom = _battNom;
                if (_mmf == null)
                {
                    return;
                }
            }

            try
            {
                if (connected == null || !connected())
                {
                    Publish(false, 0, 0);
                    return;
                }

                bool? d = dsr != null ? dsr() : (bool?)null;
                if (d == null)
                {
                    // State momentarily unreadable (e.g. ENET control channel busy, or DSR not
                    // available): do NOT collapse to 0 V (that looks like "clamp off" and breaks
                    // the "GUI closed == before GUI" behaviour). Re-publish the last configured
                    // nominal (the value Api64.dll/EdiabasLib.config injects, not a measurement).
                    // A genuine clamp-off arrives as d == false below and still drops to 0 V.
                    Publish(true, _lastIgnMv, _lastBattMv);
                    return;
                }

                bool ignitionOn = d.Value;
                int ign = ignitionOn ? Safe(ignNom) : 0;
                int batt = ignitionOn ? Safe(battNom) : 0;
                _lastIgnMv = ign;
                _lastBattMv = batt;
                Publish(true, ign, batt);
            }
            catch
            {
                // Never let an exception escape: diagnostics take priority.
            }
        }

        private static int Safe(Func<int> f)
        {
            try { return f != null ? f() : 0; }
            catch { return 0; }
        }

        private static void Publish(bool connected, int ignMv, int battMv)
        {
            lock (Lock)
            {
                PublishUnsafe(connected, ignMv, battMv);
            }
        }

        private static void PublishUnsafe(bool connected, int ignMv, int battMv)
        {
            if (_mmf == null)
            {
                return;
            }
            try
            {
                using (var a = _mmf.CreateViewAccessor(0, Capacity, MemoryMappedFileAccess.Write))
                {
                    a.Write(OffAutoFlags, connected ? FlagConnected : 0);
                    a.Write(OffAutoIgn, ignMv);
                    a.Write(OffAutoBatt, battMv);
                    a.Write(OffAutoTick, Environment.TickCount);
                    a.Write(OffAutoMagic, AutoMagic); // magic written last = block complete
                }
            }
            catch
            {
            }
        }
    }
}
#endif
