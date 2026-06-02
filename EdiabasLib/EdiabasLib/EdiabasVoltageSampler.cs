#if NETFRAMEWORK
using System;
using System.Threading;

namespace EdiabasLib
{
    /// <summary>
    /// Passive 1 s sampler of the clamp (KL15/KL30) state for the active interface, pushed to a
    /// consumer registered via <see cref="EdiabasVoltageBridge"/>. Reads only a control line
    /// (DSR on OBD) or the HSFZ control channel (ENET) — no traffic on the diagnostic bus.
    ///
    /// The timer runs ONLY while the interface is connected AND a consumer is registered, so a
    /// plain EdiabasLib instance with no consumer does nothing (no timer, no allocations, no IPC).
    /// Only the raw state is reported; presentation policy (e.g. holding the last value on an
    /// unknown read) lives in the consumer.
    ///
    /// Thread-safety: the timer thread MUST NOT touch the transport directly when the transport
    /// has a dedicated owner thread (OBD CommThread, future ENET equivalent). In that case the
    /// interface passes a non-null <c>requestSample</c> action that enqueues a fresh read on the
    /// owner thread; the <c>clamp</c> delegate then just returns the latest published snapshot.
    /// For interfaces without an owner thread (raw COM DSR is a hardware line, immune to
    /// concurrency), <c>requestSample</c> may be null and <c>clamp</c> reads directly.
    /// </summary>
    internal static class EdiabasVoltageSampler
    {
        private static readonly object Lock = new object();
        private static Timer _timer;
        private static bool _armed;                 // interface connected
        private static string _connectionId;
        private static Func<bool> _connected;
        private static Action _requestSample;
        private static Func<bool?> _clamp;
        private static Func<int> _ignNom;
        private static Func<int> _battNom;

        /// <summary>Arms sampling for a connected interface; the timer starts only if a consumer is present.</summary>
        public static void Start(string connectionId, Func<bool> connected, Func<bool?> clamp, Func<int> ignNomMv, Func<int> battNomMv, Action requestSample = null)
        {
            lock (Lock)
            {
                StopTimer();
                _armed = true;
                _connectionId = connectionId;
                _connected = connected;
                _requestSample = requestSample;
                _clamp = clamp;
                _ignNom = ignNomMv;
                _battNom = battNomMv;

                if (EdiabasVoltageBridge.HasSink)
                {
                    _timer = new Timer(OnTick, null, 0, 1000);
                }
            }
        }

        /// <summary>Disarms sampling (interface disconnect) and notifies the consumer.</summary>
        public static void Stop(string connectionId)
        {
            lock (Lock)
            {
                _armed = false;
                StopTimer();
                _connected = null;
                _requestSample = null;
                _clamp = null;
                _ignNom = null;
                _battNom = null;
            }

            EdiabasVoltageBridge.Detached(connectionId);
        }

        // Called by the bridge when a consumer registers, to start sampling if already connected.
        internal static void OnSinkRegistered()
        {
            lock (Lock)
            {
                if (_armed && _timer == null && EdiabasVoltageBridge.HasSink)
                {
                    _timer = new Timer(OnTick, null, 0, 1000);
                }
            }
        }

        private static void StopTimer()
        {
            try
            {
                _timer?.Dispose();
            }
            catch
            {
                // ignored
            }

            _timer = null;
        }

        private static void OnTick(object state)
        {
            string connectionId;
            Func<bool> connected;
            Action requestSample;
            Func<bool?> clamp;
            Func<int> ignNom;
            Func<int> battNom;
            lock (Lock)
            {
                if (!_armed || !EdiabasVoltageBridge.HasSink)
                {
                    return;
                }
                connectionId = _connectionId;
                connected = _connected;
                requestSample = _requestSample;
                clamp = _clamp;
                ignNom = _ignNom;
                battNom = _battNom;
            }

            try
            {
                if (connected == null || !connected())
                {
                    EdiabasVoltageBridge.Sample(false, null, 0, 0, connectionId);
                    return;
                }
                // Owner-thread model: enqueue a fresh read; clamp() returns the previous snapshot.
                // The 1 s latency is acceptable for a slow-changing clamp signal.
                if (requestSample != null)
                {
                    try
                    {
                        requestSample();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                bool? clampOn = clamp != null ? clamp() : (bool?)null;
                EdiabasVoltageBridge.Sample(true, clampOn, Safe(ignNom), Safe(battNom), connectionId);
            }
            catch
            {
                // Never let an exception escape: diagnostics take priority.
            }
        }

        private static int Safe(Func<int> f)
        {
            try
            {
                return f != null ? f() : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
#endif
