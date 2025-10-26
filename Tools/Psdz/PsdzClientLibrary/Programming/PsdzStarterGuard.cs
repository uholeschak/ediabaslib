using PsdzClient.Core;
using System;

namespace PsdzClient.Programming
{
    public sealed class PsdzStarterGuard
    {
        private static readonly Lazy<PsdzStarterGuard> _instance = new Lazy<PsdzStarterGuard>(() => new PsdzStarterGuard());

        private bool _isInitialized;

        private bool _isInitializationError;

        private static readonly object _lock = new object();

        public static PsdzStarterGuard Instance => _instance.Value;

        private PsdzStarterGuard()
        {
        }

        public bool IsInitializationAlreadyAttempted()
        {
            if (!_isInitialized)
            {
                return _isInitializationError;
            }
            return true;
        }

        public bool CanCheckAvailability()
        {
            return !_isInitializationError;
        }

        public void ResetInitialization()
        {
            lock (_lock)
            {
                _isInitialized = false;
                _isInitializationError = false;
            }
        }

        public bool TryInitialize(Func<bool> initializationMethod)
        {
            lock (_lock)
            {
                if (IsInitializationAlreadyAttempted())
                {
                    Log.Info(Log.CurrentMethod(), "Initialization was already attempted. Skipping...");
                    return false;
                }
                try
                {
                    _isInitialized = initializationMethod();
                }
                catch (Exception ex)
                {
                    Log.Error(Log.CurrentMethod(), "Initialization failed due to exception.", ex);
                    _isInitialized = false;
                }
                _isInitializationError = !_isInitialized;
                Log.Info(Log.CurrentMethod(), $"Initialization result: {_isInitialized}");
                return _isInitialized;
            }
        }
    }
}