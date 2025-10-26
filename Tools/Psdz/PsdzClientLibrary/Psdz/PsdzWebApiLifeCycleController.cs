using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal class PsdzWebApiLifeCycleController : IDisposable
    {
        private readonly IDictionary<ILifeCycleDependencyProvider, int> _lifeCycleDependencies = new Dictionary<ILifeCycleDependencyProvider, int>();

        private readonly object _threadLock = new object();

        private bool _isShutdownRequested;

        public bool IsIdle { get; private set; }

        public event EventHandler<EventArgs> SwitchToIdle;

        public event EventHandler<EventArgs> Shutdown;

        internal PsdzWebApiLifeCycleController(IEnumerable<ILifeCycleDependencyProvider> lifeCycleDependencyProviders)
        {
            if (lifeCycleDependencyProviders == null)
            {
                throw new ArgumentNullException("lifeCycleDependencyProviders");
            }
            foreach (ILifeCycleDependencyProvider lifeCycleDependencyProvider in lifeCycleDependencyProviders)
            {
                RegisterLifeCycleDependencyProvider(lifeCycleDependencyProvider);
            }
            IsIdle = true;
        }

        public void RequestShutdown()
        {
            Log.Info(Log.CurrentMethod(), "called.");
            _isShutdownRequested = true;
            if (IsIdle)
            {
                Log.Info(Log.CurrentMethod(), "PSdZ Web API is idle. Shutdown initiated.");
                this.Shutdown?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            lock (_threadLock)
            {
                while (_lifeCycleDependencies.Any())
                {
                    ILifeCycleDependencyProvider lifeCycleDependencyProvider = _lifeCycleDependencies.Keys.First();
                    UnregisterLifeCycleDependencyProvider(lifeCycleDependencyProvider);
                }
            }
        }

        private void OnActiveDependencyCountChanged(object sender, DependencyCountChangedEventArgs itemCountChangedEventArgs)
        {
            int itemCount = itemCountChangedEventArgs.ItemCount;
            ILifeCycleDependencyProvider lifeCycleDependencyProvider = (ILifeCycleDependencyProvider)sender;
            Log.Debug(Log.CurrentMethod(), $"Provider: '{lifeCycleDependencyProvider.Name}' - Count: {itemCount}");
            lock (_threadLock)
            {
                _lifeCycleDependencies[lifeCycleDependencyProvider] = itemCount;
                bool isIdle = IsIdle;
                IsIdle = _lifeCycleDependencies.Values.All((int dependencyCount) => dependencyCount <= 0);
                if (isIdle != IsIdle)
                {
                    Log.Debug(Log.CurrentMethod(), "PSdZ Web API State changed: [" + ConvertStateToString(isIdle) + " -> " + ConvertStateToString(IsIdle) + "]");
                }
                if (!isIdle && IsIdle)
                {
                    if (_isShutdownRequested)
                    {
                        Log.Debug(Log.CurrentMethod(), "Shutdown initiated.");
                        this.Shutdown?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        this.SwitchToIdle?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private string ConvertStateToString(bool isIdle)
        {
            if (!isIdle)
            {
                return "busy";
            }
            return "idle";
        }

        private void RegisterLifeCycleDependencyProvider(ILifeCycleDependencyProvider lifeCycleDependencyProvider)
        {
            if (lifeCycleDependencyProvider == null)
            {
                throw new ArgumentNullException("lifeCycleDependencyProvider");
            }
            if (_lifeCycleDependencies.ContainsKey(lifeCycleDependencyProvider))
            {
                Log.Warning(Log.CurrentMethod(), "Provider (Name: '" + lifeCycleDependencyProvider.Name + "') is already registered.");
                return;
            }
            _lifeCycleDependencies.Add(lifeCycleDependencyProvider, 0);
            lifeCycleDependencyProvider.ActiveDependencyCountChanged += OnActiveDependencyCountChanged;
            Log.Debug(Log.CurrentMethod(), "Name: '" + lifeCycleDependencyProvider.Name + "' (" + lifeCycleDependencyProvider.Description + ")");
        }

        private void UnregisterLifeCycleDependencyProvider(ILifeCycleDependencyProvider lifeCycleDependencyProvider)
        {
            bool flag = _lifeCycleDependencies.Remove(lifeCycleDependencyProvider);
            if (flag)
            {
                lifeCycleDependencyProvider.ActiveDependencyCountChanged -= OnActiveDependencyCountChanged;
            }
            Log.Debug(Log.CurrentMethod(), $"Name: '{lifeCycleDependencyProvider.Name}' Removed: {flag}");
        }
    }
}
