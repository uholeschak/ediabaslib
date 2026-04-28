using EdiabasLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using PsdzRpcServer.Shared;

namespace PsdzRpcClient;

public class EdiabasProxyClient : IDisposable
{
    private class VehicleRequest
    {
        public enum VehicleRequestType
        {
            Connect,
            Disconnect,
            Transmit
        }

        public VehicleRequest(VehicleRequestType requestType, ulong id, byte[] data = null)
        {
            RequestType = requestType;
            Id = id;
            Data = data;
        }

        public VehicleRequestType RequestType { get; }
        public ulong Id { get; }
        public byte[] Data { get; }
    }

    private bool _disposed;
    private EdiabasNet _ediabas;
    private volatile bool _ediabasJobAbort;
    private Thread _ediabasThread;
    private AutoResetEvent _ediabasThreadWakeEvent;
    private object _ediabasThreadLock = new object();
    private object _ediabasLock = new object();
    private object _requestLock = new object();
    private Queue<VehicleRequest> _requestQueue = new Queue<VehicleRequest>();

    public EdiabasProxyClient(EdiabasNet ediabas)
    {
        _ediabas = ediabas;
        _ediabasThreadWakeEvent = new AutoResetEvent(false);
    }

    private bool EnqueueVehicleRequest(VehicleRequest vehicleRequest)
    {
        lock (_requestLock)
        {
            if (_requestQueue.Count > 0)
            {
                return false;
            }

            _requestQueue.Enqueue(vehicleRequest);
            _ediabasThreadWakeEvent.Set();
        }

        return true;
    }

    private bool StartEdiabasThread()
    {
        if (IsEdiabasThreadRunning())
        {
            return true;
        }

        _ediabasJobAbort = false;
        _ediabasThreadWakeEvent.Reset();
        lock (_ediabasThreadLock)
        {
            _ediabasThread = new Thread(EdiabasThread);
            _ediabasThread.Start();
        }

        return true;
    }

    private bool StopEdiabasThread()
    {
        _ediabasJobAbort = true;
        _ediabasThreadWakeEvent.Set();
        if (IsEdiabasThreadRunning())
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _ediabasThread?.Join();
            // clear thread pointer
            IsEdiabasThreadRunning();
        }

        return true;
    }

    private bool IsEdiabasThreadRunning()
    {
        lock (_ediabasThreadLock)
        {
            if (_ediabasThread == null)
            {
                return false;
            }
            if (_ediabasThread.IsAlive)
            {
                return true;
            }
            _ediabasThread = null;
        }

        return false;
    }

    private bool AbortEdiabasJob()
    {
        if (_ediabasJobAbort)
        {
            return true;
        }
        return false;
    }

    public bool EdiabasConnect(ulong id)
    {
        lock (_ediabasLock)
        {
            if (_ediabas == null)
            {
                return false;
            }

            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas connect, Id={0}", id);

            try
            {
                if (_ediabas.EdInterfaceClass.InterfaceConnect())
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Ediabas connected");
                    return true;
                }

                _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Ediabas connect failed");
                return false;
            }
            catch (Exception ex)
            {
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas connect Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }
    }

    public bool EdiabasDisconnect(ulong id)
    {
        lock (_ediabasLock)
        {
            if (_ediabas == null)
            {
                return false;
            }

            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas disconnect, Id={0}", id);

            try
            {
                _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Ediabas disconnect");
                return _ediabas.EdInterfaceClass.InterfaceDisconnect();
            }
            catch (Exception ex)
            {
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas disconnect Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }
    }

    public bool IsEdiabasConnected()
    {
        lock (_ediabasLock)
        {
            if (_ediabas == null)
            {
                return false;
            }

            return _ediabas.EdInterfaceClass.Connected;
        }
    }

    public List<byte[]> EdiabasTransmit(ulong id, byte[] requestData)
    {
        List<byte[]> responseList = new List<byte[]>();
        if (requestData == null || requestData.Length < 3)
        {
            return responseList;
        }

        lock (_ediabasLock)
        {
            byte[] sendData = requestData;
            bool funcAddress = (sendData[0] & 0xC0) == 0xC0;     // functional address

            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas transmit, Id={0}, Func={1}", id, funcAddress);

            for (; ; )
            {
                bool dataReceived = false;

                if (_ediabas == null)
                {
                    break;
                }

                try
                {
                    if (_ediabas.EdInterfaceClass.TransmitData(sendData, out byte[] receiveData))
                    {
                        if (receiveData.Length > 0)
                        {
                            byte[] responseData = new byte[receiveData.Length - 1];
                            Array.Copy(receiveData, responseData, responseData.Length);
                            responseList.Add(responseData);
                        }

                        dataReceived = true;
                    }
                    else
                    {
                        if (!funcAddress)
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No response");
                        }
                    }
                }
                catch (Exception)
                {
                }

                if (!funcAddress || !dataReceived)
                {
                    break;
                }

                if (AbortEdiabasJob())
                {
                    break;
                }

                sendData = Array.Empty<byte>();
            }
        }

        return responseList;
    }

    private void EdiabasThread()
    {
        for (; ; )
        {
            _ediabasThreadWakeEvent.WaitOne(100);
            if (_ediabasJobAbort)
            {
                break;
            }

            VehicleRequest vehicleRequest = null;
            lock (_requestLock)
            {
                if (_requestQueue.Count > 0)
                {
                    vehicleRequest = _requestQueue.Dequeue();
                }
            }

            if (vehicleRequest != null)
            {
                PsdzVehicleResponse vehicleResponse = new PsdzVehicleResponse(vehicleRequest.Id);
                bool valid = true;
                switch (vehicleRequest.RequestType)
                {
                    case VehicleRequest.VehicleRequestType.Connect:
                        {
                            EdiabasDisconnect(vehicleRequest.Id);
                            bool isConnected = EdiabasConnect(vehicleRequest.Id);
                            break;
                        }

                    case VehicleRequest.VehicleRequestType.Disconnect:
                        EdiabasDisconnect(vehicleRequest.Id);
                        break;

                    case VehicleRequest.VehicleRequestType.Transmit:
                        {
                            if (vehicleRequest.Data == null)
                            {
                                valid = false;
                                break;
                            }

                            byte[] requestData = vehicleRequest.Data;
                            vehicleResponse.Request = requestData;
                            List<byte[]> responseList = EdiabasTransmit(vehicleRequest.Id, requestData);
                            vehicleResponse.ResponseList = responseList;
                            break;
                        }
                }

                vehicleResponse.Valid = valid;
                bool connected = IsEdiabasConnected();
                vehicleResponse.Connected = connected;

                //SendVehicleResponseThread(vehicleResponse);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                StopEdiabasThread();
                if (_ediabasThreadWakeEvent != null)
                {
                    _ediabasThreadWakeEvent.Dispose();
                    _ediabasThreadWakeEvent = null;
                }

                lock (_ediabasLock)
                {
                    if (_ediabas != null)
                    {
                        _ediabas.Dispose();
                        _ediabas = null;
                    }
                }
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }

}
