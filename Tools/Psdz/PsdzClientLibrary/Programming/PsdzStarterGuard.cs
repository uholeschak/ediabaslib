using PsdzClient.Core;
using System;
using System.Reflection;

namespace PsdzClient.Programming
{
    public sealed class PsdzStarterGuard
    {
        private static readonly Lazy<PsdzStarterGuard> _instance = new Lazy<PsdzStarterGuard>(() => new PsdzStarterGuard());
        private bool _isInitialized;
        private bool _isInitializationError;
        private static readonly object _lock = new object ();
        public static PsdzStarterGuard Instance => _instance.Value;

        [PreserveSource(Hint = "Modified")]
        private PsdzStarterGuard()
        {
            _isExecutable = CheckIsExecutable();
        }

        [PreserveSource(Hint = "IsExecutable added for IIS")]
        public bool IsInitializationAlreadyAttempted()
        {
            if (!_isExecutable)
            {
                Log.Info(Log.CurrentMethod(), "IsInitializationAlreadyAttempted ignoring");
                return false;
            }

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

        [PreserveSource(Hint = "Added")]
        private bool _isExecutable;
        [PreserveSource(Hint = "Added")]
        private bool CheckIsExecutable()
        {
            try
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    Log.Info(Log.CurrentMethod(), "IsExecutable true");
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.ErrorException(Log.CurrentMethod(), "IsExecutable", e);
            }

            Log.Info(Log.CurrentMethod(), "IsExecutable false");
            return false;
        }
    }
}