using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

namespace VehicleTestServer
{
    class Program
    {
        static int Main(string[] args)
        {
            TcpListenerTest();
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            return 0;
        }

        static void TcpListenerTest()
        {

            var uri = new Uri("http://127.0.0.1:8080");

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
                    Console.WriteLine($"Remote Address: {r.RemoteIpEndPoint.Address}");
                    Console.WriteLine($"Remote Port: {r.RemoteIpEndPoint.Port}");
                    Console.WriteLine("--------------***-------------");
                })
                // Send reply to browser
                .Select(r => Observable.FromAsync(() => SendResponseAsync(r, httpSender)))
                .Concat()
                .Subscribe(r =>
                    {
                        Console.WriteLine("Reply sent.");
                    },
                    ex =>
                    {
                        Console.WriteLine($"Exception: {ex}");
                    },
                    () =>
                    {
                        Console.WriteLine("Completed.");
                    });
        }

        static async Task SendResponseAsync(IHttpRequestResponse request, HttpSender httpSender)
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
                    Console.WriteLine("GET {0}", request.QueryString);
                    queryString = request.QueryString;
                }

                if (string.Compare(request.Method, "POST", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (request.Body != null)
                    {
                        Console.WriteLine("POST FormData={0}, UrlEncoded={1}", formData, urlEncoded);
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
                                Console.WriteLine("Multipart Exception={0}", e.Message);
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
                                Console.WriteLine("POST Body Exception={0}", e.Message);
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
                        Console.WriteLine("POST Exception={0}", e.Message);
                        valid = false;
                    }
                }

                if (valid)
                {

                    if (!string.IsNullOrEmpty(idString))
                    {
                        sbBody.Append($" <request id=\"{System.Web.HttpUtility.HtmlEncode(idString)}\" />\r\n");
                    }

                    if (!string.IsNullOrEmpty(connectString))
                    {
                        sbBody.Append($" <status connect=\"{System.Web.HttpUtility.HtmlEncode(connectString)}\" />\r\n");
                    }

                    if (!string.IsNullOrEmpty(disconnectString))
                    {
                        sbBody.Append($" <status disconnect=\"{System.Web.HttpUtility.HtmlEncode(disconnectString)}\" />\r\n");
                    }

                    if (!string.IsNullOrEmpty(dataString))
                    {
                        sbBody.Append($" <data response=\"{System.Web.HttpUtility.HtmlEncode(dataString)}\" />\r\n");
                    }
                }

                sbBody.Append("</vehicle_info>\r\n");
                var response = new HttpResponse
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
    }
}
