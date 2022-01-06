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
                StringBuilder sbBody = new StringBuilder();
                sbBody.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
                sbBody.Append("<vehicle_info>\r\n");
                if (!string.IsNullOrEmpty(request.QueryString))
                {
                    NameValueCollection queryCollection = System.Web.HttpUtility.ParseQueryString(request.QueryString);
                    string connectString = queryCollection.Get("connect");
                    string disconnectString = queryCollection.Get("disconnect");
                    string dataString = queryCollection.Get("data");
                    sbBody.Append(" <data");
                    if (!string.IsNullOrEmpty(connectString))
                    {
                        sbBody.Append($" connect =\"{connectString}\"");
                    }

                    if (!string.IsNullOrEmpty(disconnectString))
                    {
                        sbBody.Append($" disconnect =\"{disconnectString}\"");
                    }

                    if (!string.IsNullOrEmpty(dataString))
                    {
                        sbBody.Append($" data =\"{dataString}\"");
                    }
                    sbBody.Append(" />\r\n");
                }
                sbBody.Append("</vehicle_info>\r\n");
                var response = new HttpResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    ResponseReason = HttpStatusCode.OK.ToString(),
                    Headers = new Dictionary<string, string>
                    {
                        {"Date", DateTime.UtcNow.ToString("r")},
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
