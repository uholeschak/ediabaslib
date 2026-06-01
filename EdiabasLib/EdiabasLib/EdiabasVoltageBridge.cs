#if NETFRAMEWORK
using System;

namespace EdiabasLib
{
    /// <summary>
    /// Generic, opt-in hook that lets an external in-process consumer receive the clamp
    /// (KL15/KL30) state sampled from the active interface (see <see cref="EdiabasVoltageSampler"/>).
    ///
    /// The contract uses only corelib delegate types, on purpose: a consumer can register by
    /// reflection, with NO compile-time reference to EdiabasLib and NO assembly loading on either
    /// side (so it is immune to how EdiabasLib is packed/merged into api32/api64.dll). EdiabasLib
    /// itself loads nothing and references no consumer.
    ///
    /// The field is static, i.e. one consumer per process. This scopes the bridge to the hosting
    /// application (e.g. ISTA, whose patched process injects the consumer DLL) and never to another
    /// EdiabasLib instance running in a different process (a foreign app's instance has its own
    /// null hook and does nothing).
    /// </summary>
    public static class EdiabasVoltageBridge
    {
        public const int Version = 1;

        // EdiabasLib's nominal voltages when no measurement / per-interface config override exists.
        // Exposed so the in-process consumer can seed a sensible "auto" display value BEFORE the
        // first sample, without hardcoding the same number on its side. Matches EdInterfaceEnet's
        // protected defaults (12000 mV).
        public static int NominalIgnitionMv = 12000;
        public static int NominalBatteryMv = 12000;

        // onSample(connected, clampOn, ignitionNominalMv, batteryNominalMv, connectionId)
        //   clampOn: true = KL15 on, false = off, null = unknown (consumer decides hold-last)
        //   *NominalMv: configured nominal (EnetIgnitionVoltage / OBD equivalent), not a measurement
        //   connectionId: stable per-interface id (remote host / COM port) for disambiguation
        private static volatile Action<bool, bool?, int, int, string> _onSample;
        private static volatile Action<string> _onDetached;

        public static bool TryRegister(int consumerVersion,
            Action<bool, bool?, int, int, string> onSample, Action<string> onDetached)
        {
            if (consumerVersion != Version || onSample == null)
            {
                return false;
            }
            _onSample = onSample;
            _onDetached = onDetached;
            EdiabasVoltageSampler.OnSinkRegistered();
            return true;
        }

        public static void Unregister(Action<bool, bool?, int, int, string> onSample)
        {
            if (ReferenceEquals(_onSample, onSample))
            {
                _onSample = null;
                _onDetached = null;
            }
        }

        internal static bool HasSink => _onSample != null;

        internal static void Sample(bool connected, bool? clampOn, int ignNomMv, int battNomMv, string connectionId)
        {
            try { _onSample?.Invoke(connected, clampOn, ignNomMv, battNomMv, connectionId); }
            catch { /* a consumer fault must never affect diagnostics */ }
        }

        internal static void Detached(string connectionId)
        {
            try { _onDetached?.Invoke(connectionId); }
            catch { }
        }
    }
}
#endif
