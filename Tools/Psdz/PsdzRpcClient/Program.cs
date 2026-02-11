using System;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            await using var client = new PsdzRpcClient();
            client.CallbackHandler.ProgressChanged += (s, e) =>
            {
                Console.WriteLine($"[{e.Percent}%] {e.Message}");
            };

            client.CallbackHandler.OperationCompleted += (s, success) =>
            {
                Console.WriteLine($"Operation finished: {(success ? "Success" : "Error")}");
            };

            await client.ConnectAsync(cts.Token);

            if (client.RpcService != null)
            {
                bool resultConnect = await client.RpcService.Connect("Connect");
                Console.WriteLine($"Connect = {resultConnect}");

                bool resultDisconnect = await client.RpcService.Disconnect("Disconnect");
                Console.WriteLine($"Disconnect = {resultDisconnect}");
            }

            return  0;
        }
    }
}
