using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer
{
    public class PsdzRpcServer
    {
        public const int DefaultTcpPort = 31285;

        private readonly string _dealerId;
        private readonly TextWriter _output;
        private readonly int? _tcpPort; // null = Pipe, sonst TCP
        private int _clientCount;
        private bool _hadClients;
        private readonly TaskCompletionSource<bool> _allClientsDisconnected = new TaskCompletionSource<bool>();

        /// <summary>
        /// Wird ausgelöst wenn der letzte Client die Verbindung trennt.
        /// </summary>
        public Task AllClientsDisconnected => _allClientsDisconnected.Task;

        /// <param name="tcpPort">Wenn angegeben, wird TCP statt Named Pipe verwendet.</param>
        public PsdzRpcServer(string dealerId, TextWriter output = null, int? tcpPort = null)
        {
            _dealerId = dealerId;
            _output = output;
            _tcpPort = tcpPort;
            _clientCount = 0;
            _hadClients = false;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            if (_tcpPort.HasValue)
            {
                await StartTcpAsync(_tcpPort.Value, ct).ConfigureAwait(false);
            }
            else
            {
                await StartPipeAsync(ct).ConfigureAwait(false);
            }
        }

        private async Task StartPipeAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream pipeServer = CreatePipeServer();
                try
                {
                    _output?.WriteLine("Wait for client connection (Pipe)...");
                    await pipeServer.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    OnClientAccepted(pipeServer);
                }
                catch (OperationCanceledException)
                {
                    pipeServer.Dispose();
                    break;
                }
            }
        }

        private async Task StartTcpAsync(int port, CancellationToken ct)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            _output?.WriteLine($"TCP server listening on port {port}...");
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // TcpListener hat kein natives CancellationToken, daher mit Task.WhenAny abbrechen
                    Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                    Task cancelTask = Task.Delay(Timeout.Infinite, ct);
                    Task completed = await Task.WhenAny(acceptTask, cancelTask).ConfigureAwait(false);
                    if (completed == cancelTask)
                    {
                        break;
                    }

                    TcpClient tcpClient = await acceptTask.ConfigureAwait(false);
                    tcpClient.NoDelay = true;
                    OnClientAccepted(tcpClient.GetStream(), tcpClient);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                listener.Stop();
            }
        }

        private void OnClientAccepted(Stream stream, IDisposable owner = null)
        {
            int count = Interlocked.Increment(ref _clientCount);
            _hadClients = true;
            _output?.WriteLine($"Client connected. Active clients: {count}");
            _ = HandleClientAsync(stream, owner);
        }

        private async Task HandleClientAsync(Stream stream, IDisposable owner = null)
        {
            try
            {
                using JsonRpc jsonRpc = new JsonRpc(stream);
                using PsdzRpcService service = new PsdzRpcService(jsonRpc.Attach<IPsdzRpcServiceCallback>(), _dealerId);

                jsonRpc.AddLocalRpcTarget(service);
                jsonRpc.StartListening();
                await jsonRpc.Completion.ConfigureAwait(false);

                _output?.WriteLine("Client disconnected.");
            }
            finally
            {
                owner?.Dispose(); // TcpClient oder null
                stream.Dispose();

                int count = Interlocked.Decrement(ref _clientCount);
                _output?.WriteLine($"Active clients: {count}");

                if (count == 0 && _hadClients)
                {
                    _allClientsDisconnected.TrySetResult(true);
                }
            }
        }

        private NamedPipeServerStream CreatePipeServer()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();

            // Aktueller Benutzer (Server-Prozess) darf alles
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                WindowsIdentity.GetCurrent().User,
                PipeAccessRights.FullControl,
                AccessControlType.Allow));

            pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));

            // IIS App Pool Identitäten sind NICHT in AuthenticatedUsers enthalten
            // → Everyone (World) hinzufügen für lokale IPC
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));

#if NET
            return NamedPipeServerStreamAcl.Create(
                PsdzRpcServiceConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                pipeSecurity);
#else
            return new NamedPipeServerStream(
                PsdzRpcServiceConstants.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                pipeSecurity);
#endif
        }

        public int ClientCount => _clientCount;
    }
}
