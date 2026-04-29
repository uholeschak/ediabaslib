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

    public delegate bool VehicleResponseDelegate(PsdzVehicleResponse vehicleResponse);
    public delegate void ErrorMessageDelegate(string message);
    public delegate void InfoMessageDelegate(string message);
    public event VehicleResponseDelegate VehicleResponseEvent;
    public event ErrorMessageDelegate ErrorMessageEvent;
    public event InfoMessageDelegate InfoMessageEvent;

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

    public bool VehicleConnect(ulong id)
    {
        return EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Connect, id));
    }

    public bool VehicleDisconnect(ulong id)
    {
        return EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Disconnect, id));
    }

    public bool VehicleSend(ulong id, byte[] data)
    {
        return EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Transmit, id, data));
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

    public bool StartEdiabasThread()
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

    public bool StopEdiabasThread()
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

            InfoMessageEvent?.Invoke($"Ediabas connect, Id={id}");
            try
            {
                if (_ediabas.EdInterfaceClass.InterfaceConnect())
                {
                    InfoMessageEvent?.Invoke("Ediabas connected");
                    return true;
                }

                ErrorMessageEvent?.Invoke("Ediabas connect failed");
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessageEvent?.Invoke($"Ediabas connect Exception: {EdiabasNet.GetExceptionText(ex)}");
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

            InfoMessageEvent?.Invoke($"Ediabas disconnect, Id={id}");

            try
            {
                InfoMessageEvent?.Invoke("Ediabas disconnect");
                return _ediabas.EdInterfaceClass.InterfaceDisconnect();
            }
            catch (Exception ex)
            {
                ErrorMessageEvent?.Invoke($"Ediabas disconnect Exception: {EdiabasNet.GetExceptionText(ex)}");
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

            InfoMessageEvent?.Invoke($"Ediabas transmit, Id={id}, Func={funcAddress}");

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
                            ErrorMessageEvent?.Invoke("*** No response");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessageEvent?.Invoke($"Ediabas transmit Exception: {EdiabasNet.GetExceptionText(ex)}");
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
                bool connected = false;
                switch (vehicleRequest.RequestType)
                {
                    case VehicleRequest.VehicleRequestType.Connect:
                        {
                            EdiabasDisconnect(vehicleRequest.Id);
                            if (EdiabasConnect(vehicleRequest.Id))
                            {
                                connected = IsEdiabasConnected();
                            }
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
                            connected = IsEdiabasConnected();
                            break;
                        }
                }

                vehicleResponse.Valid = valid;
                vehicleResponse.Connected = connected;

                VehicleResponseEvent?.Invoke(vehicleResponse);
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
