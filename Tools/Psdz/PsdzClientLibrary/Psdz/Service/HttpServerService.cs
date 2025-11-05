using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using RestSharp;
using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
{
    internal class HttpServerService : IHttpServerService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string _endpointService = "httpserver";
        private CancellationTokenSource _statusTokenSource;
        private Task _checkStatusTask;
        public HttpServerService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public bool Start()
        {
            try
            {
                StartStatusCheckTask();
                return _webCallHandler.ExecuteRequest<bool>(_endpointService, "start", Method.Post).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public bool Stop()
        {
            try
            {
                StopStatusCheckTaskIfRunning();
                return _webCallHandler.ExecuteRequest<bool>(_endpointService, "stop", Method.Post).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        private ServerStatus GetStatus()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<ServerStatus>(_endpointService, "getstatus", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        private void StartStatusCheckTask()
        {
            try
            {
                Task checkStatusTask = _checkStatusTask;
                if (checkStatusTask == null || checkStatusTask.Status != TaskStatus.Running)
                {
                    _statusTokenSource = new CancellationTokenSource();
                    _checkStatusTask = new Task(CheckStatusLoop, _statusTokenSource.Token);
                    _checkStatusTask.Start();
                }
            }
            catch (Exception exception)
            {
                Log.Error(Log.CurrentMethod(), "Starting status check task failed.");
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private async void CheckStatusLoop()
        {
            ServerStatus status;
            try
            {
                status = GetStatus();
            }
            catch
            {
                status = ServerStatus.STOPPED;
            }

            while (!_statusTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(60.0));
                try
                {
                    ServerStatus status2 = GetStatus();
                    if (status2 != status)
                    {
                        Log.Info(Log.CurrentMethod(), $"Detected http server status change. Old status: {status}. Server status: {status2}.");
                    }

                    if (status2 != ServerStatus.RUNNING)
                    {
                        Log.Info(Log.CurrentMethod(), $"Current status is {status2}. Restarting http server.");
                        Start();
                        status2 = GetStatus();
                        if (status2 != ServerStatus.RUNNING)
                        {
                            Log.Info(Log.CurrentMethod(), $"After restart server is not in RUNNING status. Server status {status}");
                        }
                    }

                    status = status2;
                }
                catch (Exception exception)
                {
                    Log.Info(Log.CurrentMethod(), "Restarting http host loop failed");
                    Log.ErrorException(Log.CurrentMethod(), exception);
                }
            }
        }

        private void StopStatusCheckTaskIfRunning()
        {
            try
            {
                if (_statusTokenSource != null && !_statusTokenSource.IsCancellationRequested)
                {
                    _statusTokenSource.Cancel();
                }
            }
            catch (Exception exception)
            {
                Log.Error(Log.CurrentMethod(), "Stopping status check task failed.");
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }
    }
}