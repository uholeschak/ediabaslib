using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Events;
using PsdzClient.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Timers;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class EventManagerService : IEventManagerService
    {
        private readonly IWebCallHandler webCallHandler;
        private readonly string endpointService = "eventmanagement";
        private readonly System.Timers.Timer pollingTimer = new System.Timers.Timer(1000.0);
        private readonly ConnectionLossEventListener connectionLossEventListener = new ConnectionLossEventListener();
        private readonly List<IPsdzEventListener> registeredEventListeners = new List<IPsdzEventListener>();
        private readonly ConcurrentQueue<IPsdzEvent> eventsToBeHandled = new ConcurrentQueue<IPsdzEvent>();
        private readonly object eventFiringLock = new object ();
        private readonly object eventQueuingLock = new object ();
        private readonly string clientId;
        public bool Listening
        {
            get
            {
                return pollingTimer.Enabled;
            }

            private set
            {
                pollingTimer.Enabled = value;
            }
        }

        public EventManagerService(IWebCallHandler webCallHandler, string clientId)
        {
            this.webCallHandler = webCallHandler;
            this.clientId = clientId;
            pollingTimer.Elapsed += PollingTimer_Elapsed;
        }

        public void PrepareListening()
        {
            try
            {
                Log.Debug(Log.CurrentMethod(), "Set Client-ID as Event-ID: '{0}'", clientId);
                webCallHandler.ExecuteRequest(endpointService, "seteventid/" + clientId, HttpMethod.Post);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void StartListening()
        {
            try
            {
                if (Listening)
                {
                    Log.Debug(Log.CurrentMethod(), "Attempting to start listening even though listening is already ongoing. EventId: " + clientId);
                }

                EventListeningRequestModel requestBodyObject = new EventListeningRequestModel
                {
                    PsdZEventTypes = null
                };
                webCallHandler.ExecuteRequest(endpointService, "startlistening/" + clientId, HttpMethod.Post, requestBodyObject);
                Listening = true;
                Log.Debug(Log.CurrentMethod(), "Commenced listening. ClientId: " + clientId);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void StopListening()
        {
            try
            {
                if (!Listening)
                {
                    Log.Debug(Log.CurrentMethod(), "Attempting to stop listening even though listening is not started. EventId: " + clientId);
                }

                webCallHandler.ExecuteRequest(endpointService, "stoplistening/" + clientId, HttpMethod.Post);
                Listening = false;
                Log.Debug(Log.CurrentMethod(), "Ceased listening. ClientId: " + clientId);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void SendInternalEvent(IPsdzEvent psdzEvent)
        {
            Log.Debug(Log.CurrentMethod(), "Sending internal event. ClientId: " + clientId + " Message: " + psdzEvent.Message);
            HandleEvents(psdzEvent);
        }

        public IConnectionLossEventListener AddPsdzEventListenerForConnectionLoss()
        {
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Psdz.DetectEthernetConnectionLoss", defaultValue: false))
            {
                Log.Debug(Log.CurrentMethod(), "Adding connection loss listener. ClientId: " + clientId);
                AddEventListener(connectionLossEventListener);
                return connectionLossEventListener;
            }

            return null;
        }

        public void RemovePsdzEventListenerForConnectionLoss()
        {
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Psdz.DetectEthernetConnectionLoss", defaultValue: false))
            {
                Log.Debug(Log.CurrentMethod(), "Removing connection loss listener. ClientId: " + clientId);
                RemoveEventListener(connectionLossEventListener);
            }
        }

        public void AddEventListener(IPsdzEventListener psdzEventListener)
        {
            if (psdzEventListener == null)
            {
                Log.Debug(Log.CurrentMethod(), "Caller attempted to add a null listener. ClientId: " + clientId);
                return;
            }

            lock (registeredEventListeners)
            {
                if (registeredEventListeners.Contains(psdzEventListener))
                {
                    Log.Debug(Log.CurrentMethod(), "Caller attempted to add a listener that was already added. ClientId: " + clientId);
                    return;
                }

                registeredEventListeners.Add(psdzEventListener);
                Log.Debug(Log.CurrentMethod(), "Listener added. ClientId: " + clientId);
                if (!Listening)
                {
                    StartListening();
                }
            }
        }

        public void RemoveEventListener(IPsdzEventListener psdzEventListener)
        {
            if (psdzEventListener == null)
            {
                Log.Debug(Log.CurrentMethod(), "Caller attempted to remove a null listener. ClientId: " + clientId);
                return;
            }

            lock (registeredEventListeners)
            {
                if (!registeredEventListeners.Contains(psdzEventListener))
                {
                    Log.Debug(Log.CurrentMethod(), "Caller attempted to remove a listener, but alas 'twas not there to be removed. ClientId: " + clientId);
                    return;
                }

                registeredEventListeners.Remove(psdzEventListener);
                Log.Debug(Log.CurrentMethod(), "Listener removed. ClientId: " + clientId);
                if (!registeredEventListeners.Any() && Listening)
                {
                    StopListening();
                }
            }
        }

        public void RemoveAllEventListeners()
        {
            lock (registeredEventListeners)
            {
                Log.Debug(Log.CurrentMethod(), "Removing all listeners as requested by the caller. ClientId: " + clientId);
                registeredEventListeners.Clear();
                if (Listening)
                {
                    StopListening();
                }
            }
        }

        private void HandleEvents(IPsdzEvent eventToAppend = null)
        {
            lock (eventQueuingLock)
            {
                IEnumerable<IPsdzEvent> enumerable = GetEventsFromJavaSide()?.Where((IPsdzEvent x) => x != null);
                if (enumerable != null)
                {
                    foreach (IPsdzEvent item in enumerable)
                    {
                        eventsToBeHandled.Enqueue(item);
                    }
                }

                if (eventToAppend != null)
                {
                    eventsToBeHandled.Enqueue(eventToAppend);
                }
            }

            FireEventHandlers();
        }

        private IEnumerable<IPsdzEvent> GetEventsFromJavaSide()
        {
            try
            {
                return webCallHandler.ExecuteRequest<IList<EventModel>>(endpointService, "getevents/" + clientId, HttpMethod.Get)?.Data?.Select(EventMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        private void FireEventHandlers()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(eventFiringLock, ref lockTaken);
                if (lockTaken)
                {
                    IPsdzEvent result;
                    while (eventsToBeHandled.TryDequeue(out result))
                    {
                        foreach (IPsdzEventListener item in registeredEventListeners.ToList())
                        {
                            try
                            {
                                item.SetPsdzEvent(result);
                            }
                            catch (Exception exception)
                            {
                                Log.ErrorException(Log.CurrentMethod(), exception);
                            }
                        }
                    }
                }
                else
                {
                    Log.Debug(Log.CurrentMethod(), "Event handling is already underway, so it will not be started again.");
                }
            }
            catch (Exception exception2)
            {
                Log.ErrorException(Log.CurrentMethod(), exception2);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(eventFiringLock);
                }
            }
        }

        private void PollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HandleEvents();
        }
    }
}