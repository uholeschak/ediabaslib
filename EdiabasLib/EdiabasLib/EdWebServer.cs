using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpMultipartParser;
using ISimpleHttpListener.Rx.Enum;
using ISimpleHttpListener.Rx.Model;
using SimpleHttpListener.Rx.Extension;
using SimpleHttpListener.Rx.Model;
using SimpleHttpListener.Rx.Service;

namespace EdiabasLib
{
    public class EdWebServer: IDisposable
    {
        public delegate void MessageDelegate(string message);

        private bool _disposed;
        private object _ediabasLock = new object();
        private object _requestLock = new object();
        private EdiabasNet _ediabas;
        private MessageDelegate _messageHandler;
        private bool _ediabasAbort;
        private UInt64 _requestId;

        public EdiabasNet Ediabas
        {
            get
            {
                lock (_ediabasLock)
                {
                    return _ediabas;
                }
            }
        }

        public EdWebServer(EdiabasNet ediabas, MessageDelegate messageHandler)
        {
            _ediabas = ediabas;
            _ediabas.AbortJobFunc = AbortEdiabasJob;
            _messageHandler = messageHandler;
            _ediabasAbort = false;
            _requestId = 0;
        }

        public void EdiabasDispose()
        {
            lock (_ediabasLock)
            {
                if (_ediabas != null)
                {
                    _ediabas.Dispose();
                    _ediabas = null;
                }
            }
        }

        public bool AbortEdiabasJob()
        {
            return _ediabasAbort;
        }

        public bool EdiabasConnect()
        {
            lock (_ediabasLock)
            {
                try
                {
                    _ediabasAbort = false;
                    if (_ediabas.EdInterfaceClass.InterfaceConnect())
                    {
                        _messageHandler?.Invoke($"Ediabas connected");
                        return true;
                    }

                    _messageHandler?.Invoke($"Ediabas connect failed");
                    return false;
                }
                catch (Exception ex)
                {
                    _messageHandler?.Invoke($"Ediabas connect Exception: {ex.Message}");
                    return false;
                }
            }
        }

        public bool EdiabasDisconnect()
        {
            lock (_ediabasLock)
            {
                try
                {
                    _messageHandler?.Invoke($"Ediabas disconnected");
                    _ediabasAbort = true;
                    return _ediabas.EdInterfaceClass.InterfaceDisconnect();
                }
                catch (Exception)
                {
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

        public List<byte[]> EdiabasTransmit(byte[] requestData)
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

                for (; ; )
                {
                    bool dataReceived = false;

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
                    }
                    catch (Exception)
                    {
                        // ignored
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

        public int StartTcpListener(string url)
        {

            var uri = new Uri(url);

            var tcpListener = new TcpListener(uri.Host.GetIPv4Address(), uri.Port)
            {
                ExclusiveAddressUse = false
            };

            var httpSender = new HttpSender();

            var cts = new CancellationTokenSource();

            var disposable = tcpListener
                .ToHttpListenerObservable(cts.Token)
                .Do(r =>
                {
                    _messageHandler?.Invoke($"Remote Address: {r.RemoteIpEndPoint.Address}");
                    _messageHandler?.Invoke($"Remote Port: {r.RemoteIpEndPoint.Port}");
                    _messageHandler?.Invoke("--------------***-------------");
                })
                // Send reply to browser
                .Select(r => Observable.FromAsync(() => SendResponseAsync(r, httpSender)))
                .Concat()
                .Subscribe(r =>
                {
                    _messageHandler?.Invoke($"Reply sent.");
                },
                    ex =>
                    {
                        _messageHandler?.Invoke($"Exception: {ex.Message}");
                    },
                    () =>
                    {
                        _messageHandler?.Invoke($"Completed.");
                    });

            int port = -1;
            IPEndPoint ipEndPoint = tcpListener.LocalEndpoint as IPEndPoint;
            if (ipEndPoint != null)
            {
                port = ipEndPoint.Port;
            }

            return port;
        }

        private async Task SendResponseAsync(IHttpRequestResponse request, HttpSender httpSender)
        {
            if (request.RequestType == RequestType.TCP)
            {
                if (!request.Headers.TryGetValue("CONTENT-TYPE", out string contentType))
                {
                    contentType = string.Empty;
                }

                bool urlEncoded = contentType.Contains("application/x-www-form-urlencoded");
                bool formData = contentType.Contains("multipart/form-data");

                StringBuilder sbBody = new StringBuilder();
                sbBody.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
                sbBody.Append("<vehicle_info>\r\n");

                string queryString = null;
                string idString = null;
                string connectString = null;
                string disconnectString = null;
                string dataString = null;
                bool valid = true;

                if (string.Compare(request.Method, "GET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _messageHandler?.Invoke($"GET {request.QueryString}");
                    queryString = request.QueryString;
                }

                if (string.Compare(request.Method, "POST", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (request.Body != null)
                    {
                        _messageHandler?.Invoke($"POST FormData={formData}, UrlEncoded={urlEncoded}");
                        if (formData)
                        {
                            try
                            {
                                request.Body.Seek(0, SeekOrigin.Begin);
                                MultipartFormDataParser parser = await MultipartFormDataParser.ParseAsync(request.Body).ConfigureAwait(false);
                                idString = parser.GetParameterValue("id");
                                connectString = parser.GetParameterValue("connect");
                                disconnectString = parser.GetParameterValue("disconnect");
                                dataString = parser.GetParameterValue("data");
                            }
                            catch (Exception e)
                            {
                                _messageHandler?.Invoke($"Multipart Exception={e.Message}");
                                valid = false;
                            }
                        }
                        else if (urlEncoded)
                        {
                            try
                            {
                                request.Body.Seek(0, SeekOrigin.Begin);
                                StreamReader sr = new StreamReader(request.Body);
                                queryString = await sr.ReadToEndAsync().ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                _messageHandler?.Invoke($"POST Body Exception={e.Message}");
                                valid = false;
                            }
                        }
                    }
                }

                if (valid && !string.IsNullOrWhiteSpace(queryString))
                {
                    try
                    {
                        NameValueCollection queryCollection = System.Web.HttpUtility.ParseQueryString(queryString);
                        idString = queryCollection.Get("id");
                        connectString = queryCollection.Get("connect");
                        disconnectString = queryCollection.Get("disconnect");
                        dataString = queryCollection.Get("data");
                    }
                    catch (Exception e)
                    {
                        _messageHandler?.Invoke($"POST Exception={e.Message}");
                        valid = false;
                    }
                }

                UInt64? requestId = null;
                if (valid)
                {
                    if (!string.IsNullOrEmpty(idString))
                    {
                        if (UInt64.TryParse(idString, out UInt64 idValue))
                        {
                            requestId = idValue;
                        }
                    }
                }

                if (!requestId.HasValue)
                {
                    _messageHandler?.Invoke($"No request ID");
                    valid = false;
                }

                bool checkId = true;
                if (valid)
                {
                    if (!string.IsNullOrEmpty(connectString))
                    {
                        if (int.TryParse(connectString, out int connectValue))
                        {
                            if (connectValue > 0)
                            {
                                EdiabasDisconnect();
                                EdiabasConnect();
                                _requestId = requestId.Value;
                                checkId = false;
                            }
                        }
                    }
                }

                if (valid)
                {
                    lock (_requestLock)
                    {
                        if (checkId && requestId.Value <= _requestId)
                        {
                            _messageHandler?.Invoke($"Ignoring request ID: {requestId.Value}");
                            valid = false;
                        }
                        else
                        {
                            _requestId = requestId.Value;
                        }
                    }
                }

                Stopwatch stopwatchOperation = new Stopwatch();
                stopwatchOperation.Start();
                string validReport = valid ? "1" : "0";
                string idReport = idString ?? string.Empty;
                sbBody.Append($" <request valid=\"{System.Web.HttpUtility.HtmlEncode(validReport)}\" id=\"{System.Web.HttpUtility.HtmlEncode(idReport)}\" />\r\n");
                if (valid)
                {
                    if (!string.IsNullOrEmpty(disconnectString))
                    {
                        if (int.TryParse(disconnectString, out int disconnectValue))
                        {
                            if (disconnectValue > 0)
                            {
                                EdiabasDisconnect();
                            }
                        }
                    }

                    bool connected = IsEdiabasConnected();
                    string connectedState = connected ? "1" : "0";
                    sbBody.Append($" <status");
                    sbBody.Append($" connected=\"{System.Web.HttpUtility.HtmlEncode(connectedState)}\"");
                    sbBody.Append(" />\r\n");

                    if (!string.IsNullOrEmpty(dataString))
                    {
                        string requestString = dataString.Replace(" ", "");
                        byte[] requestData = EdiabasNet.HexToByteArray(requestString);
                        sbBody.Append($" <data request=\"{System.Web.HttpUtility.HtmlEncode(requestString)}\" />\r\n");
                        List<byte[]> responseList = EdiabasTransmit(requestData);
                        foreach (byte[] responseData in responseList)
                        {
                            if (responseData.Length > 0)
                            {
                                string responseReport = BitConverter.ToString(responseData).Replace("-", "");
                                sbBody.Append($" <data response=\"{System.Web.HttpUtility.HtmlEncode(responseReport)}\" />\r\n");
                            }
                        }
                    }
                }

                stopwatchOperation.Stop();
                string processingInfo = string.Format(CultureInfo.InvariantCulture, "Time: {0} ms", stopwatchOperation.ElapsedMilliseconds);
                sbBody.Append($" <info");
                sbBody.Append($" message=\"{System.Web.HttpUtility.HtmlEncode(processingInfo)}\"");
                sbBody.Append(" />\r\n");

                sbBody.Append("</vehicle_info>\r\n");
                HttpResponse response = new HttpResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    ResponseReason = HttpStatusCode.OK.ToString(),
                    Headers = new Dictionary<string, string>
                    {
                        {"Date", System.Web.HttpUtility.HtmlEncode(DateTime.UtcNow.ToString("r"))},
                        {"Content-Type", "text/xml" },
                        {"Access-Control-Allow-Origin", "*" },
                    },
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(sbBody.ToString()))
                };

                await httpSender.SendTcpResponseAsync(request, response).ConfigureAwait(false);
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
                EdiabasDisconnect();
                EdiabasDispose();

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}