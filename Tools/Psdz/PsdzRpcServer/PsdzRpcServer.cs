using PsdzRpcServer.Shared;
using StreamJsonRpc;
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer
{
    public class PsdzRpcServer
    {
        private readonly string _dealerId;
        private readonly TextWriter _output;
        private int _clientCount;
        private bool _hadClients;
        private readonly TaskCompletionSource<bool> _allClientsDisconnected = new TaskCompletionSource<bool>();

        /// <summary>
        /// Wird ausgelöst wenn der letzte Client die Verbindung trennt.
        /// </summary>
        public Task AllClientsDisconnected => _allClientsDisconnected.Task;

        public PsdzRpcServer(string dealerId, TextWriter output = null)
        {
            _dealerId = dealerId;
            _output = output;
            _clientCount = 0;
            _hadClients = false;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream pipeServer = CreatePipeServer();

                try
                {
                    _output?.WriteLine("Wait for client connection...");
                    await pipeServer.WaitForConnectionAsync(ct).ConfigureAwait(false);

                    int count = Interlocked.Increment(ref _clientCount);
                    _hadClients = true;
                    _output?.WriteLine($"Client connected. Active clients: {count}");

                    // JsonRpc for this connection
                    _ = HandleClientAsync(pipeServer);
                }
                catch (OperationCanceledException)
                {
                    pipeServer.Dispose();
                    break;
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

        private async Task HandleClientAsync(NamedPipeServerStream pipeServer)
        {
            try
            {
                using JsonRpc jsonRpc = new JsonRpc(pipeServer);

                IPsdzRpcServiceCallback callback = jsonRpc.Attach<IPsdzRpcServiceCallback>();
                PsdzRpcService service = new PsdzRpcService(callback, _dealerId);
                jsonRpc.AddLocalRpcTarget(service);

                jsonRpc.StartListening();
                await jsonRpc.Completion.ConfigureAwait(false);

                _output?.WriteLine("Client disconnected.");
            }
            finally
            {
                pipeServer.Dispose();

                int count = Interlocked.Decrement(ref _clientCount);
                _output?.WriteLine($"Active clients: {count}");

                if (count == 0 && _hadClients)
                {
                    _allClientsDisconnected.TrySetResult(true);
                }
            }
        }
    }
}
