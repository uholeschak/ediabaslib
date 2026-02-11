using System;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            await using var client = new PsdzRpcClient();
            await client.ConnectAsync();

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
