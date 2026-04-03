using PsdzRpcServer.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient;

public class PsdzRpcServerStarter
{
    public const string ServerExeName = "PsdzRpcServer.exe";

    private readonly TextWriter _output;

    public PsdzRpcServerStarter(TextWriter output = null)
    {
        _output = output;
    }

    public async Task<bool> ConnectClient(string serverExe, PsdzRpcClient client, CancellationTokenSource cts)
    {
        Process serverProcess = null;
        if (!StartServerIfNeeded(serverExe, out serverProcess))
        {
            _output?.WriteLine("No server available. Exiting.");
            return false;
        }

        _output?.WriteLine("Starting PsdzJsonRpcClient...");
        Task clientTask = client.ConnectAsync(null, cts.Token);

        for (int i = 0; i < 3; i++)
        {
            Task delayTask = Task.Delay(2000, cts.Token);
            await Task.WhenAny(clientTask, delayTask);
            if (clientTask.IsCompleted)
            {
                break;
            }

            _output?.WriteLine("Try to restart server...");
            if (!StartServerIfNeeded(serverExe, out serverProcess))
            {
                _output?.WriteLine("No server available. Exiting.");
                return false;
            }
        }

        if (!clientTask.IsCompleted)
        {
            _output?.WriteLine("Failed to connect to server after multiple attempts. Exiting.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prüft ob die Named Pipe des Servers existiert.
    /// </summary>
    public bool IsPipeAvailable()
    {
        return File.Exists($@"\\.\pipe\{PsdzRpcServiceConstants.PipeName}");
    }

    /// <summary>
    /// Startet den Server-Prozess falls ein Pfad angegeben ist.
    /// Wartet nur bis der Prozess gestartet ist, nicht bis die Pipe verfügbar ist.
    /// </summary>
    public bool StartServerIfNeeded(string serverExe, out Process serverProcess)
    {
        serverProcess = null;

        // Prüfen ob der Server eventuell schon läuft (Pipe könnte noch nicht bereit sein)
        if (IsPipeAvailable())
        {
            _output?.WriteLine("Server pipe detected, server is already running.");
            return true;
        }

        if (string.IsNullOrEmpty(serverExe))
        {
            _output?.WriteLine("No server executable specified.");
            return false;
        }

        string serverExeFullPath = Path.IsPathRooted(serverExe)
            ? serverExe
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, serverExe));

        if (!File.Exists(serverExeFullPath))
        {
            _output?.WriteLine($"Server executable not found: {serverExeFullPath}");
            return false;
        }

        _output?.WriteLine($"Starting server: {serverExeFullPath}");
        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = serverExeFullPath,
            WorkingDirectory = Path.GetDirectoryName(serverExeFullPath) ?? Environment.CurrentDirectory,
            UseShellExecute = true,
        });

        if (process == null || process.HasExited)
        {
            _output?.WriteLine("Failed to start server process.");
            return false;
        }

        _output?.WriteLine($"Server process started (PID: {process.Id}).");
        serverProcess = process;
        return true;
    }

    public static string DetectServerLocation()
    {
        string assemblyDir = AssemblyDirectory;
        if (string.IsNullOrEmpty(assemblyDir))
        {
            return null;
        }

        string rootDir = Path.Combine(assemblyDir, "..", "..", "..", "..","..", "PsdzRpcServer", "artifacts", "bin", "PsdzRpcServer");
#if DEBUG
        string prefix = "debug";
#else
        string prefix = "release";
#endif
        string serverDir = Path.Combine(rootDir, prefix + "net10.0-windows10.0.26100.0");
        string serverExe = Path.Combine(serverDir, ServerExeName);
        if (File.Exists(serverExe))
        {
            return serverExe;
        }

        serverDir = Path.Combine(rootDir, prefix + "debug_net481");
        serverExe = Path.Combine(serverDir, ServerExeName);
        if (File.Exists(serverExe))
        {
            return serverExe;
        }

        return null;
    }

    public static string AssemblyDirectory
    {
        get
        {
#if NET
            string location = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(location) || !File.Exists(location))
            {
                return null;
            }
            return Path.GetDirectoryName(location);
#else
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            return Path.GetDirectoryName(path);
#endif
        }
    }
}