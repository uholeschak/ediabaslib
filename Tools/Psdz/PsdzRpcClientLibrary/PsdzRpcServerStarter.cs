using PsdzRpcServer.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient;

public class PsdzRpcServerStarter
{
    public const string ServerExeName = "PsdzRpcServer.exe";
    public const string ServerDirName = "PsdzRpcServer";

    private readonly TextWriter _output;

    public PsdzRpcServerStarter(TextWriter output = null)
    {
        _output = output;
    }

    public async Task<bool> ConnectClient(string serverExe, string serviceName, ProcessWindowStyle windowStyle, PsdzRpcClient client, CancellationTokenSource cts,
            string userName = null, string password = null)
    {
        if (!StartServerIfNeeded(serverExe, serviceName, windowStyle, userName, password))
        {
            _output?.WriteLine("No server available. Exiting.");
            return false;
        }

        _output?.WriteLine("Starting PsdzJsonRpcClient...");
        Task clientTask = client.ConnectAsync(null, cts.Token);

        for (int i = 0; i < 3; i++)
        {
            Task delayTask = Task.Delay(2000, cts.Token);
            await Task.WhenAny(clientTask, delayTask).ConfigureAwait(false);
            if (clientTask.IsCompleted)
            {
                break;
            }

            _output?.WriteLine("Try to restart server...");
            if (!StartServerIfNeeded(serverExe, serviceName, windowStyle, userName, password))
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
    /// Startet den Server-Prozess, falls ein Pfad angegeben ist.
    /// Wartet nur bis der Prozess gestartet ist, nicht bis die Pipe verfügbar ist.
    /// </summary>
    public bool StartServerIfNeeded(string serverExe, string serviceName, ProcessWindowStyle windowStyle, string userName, string password)
    {
        if (IsPipeAvailable())
        {
            _output?.WriteLine("Server pipe detected, server is already running.");
            return true;
        }

        // Zuerst versuchen den Windows-Dienst zu starten
        if (TryStartWindowsService(serviceName))
        {
            return true;
        }

        // Fallback: Prozess direkt starten
        return StartServerProcess(serverExe, windowStyle, userName, password);
    }

    private bool TryStartWindowsService(string serviceName)
    {
        try
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return false;
            }

            using ServiceController sc = new ServiceController(serviceName);
            ServiceControllerStatus status = sc.Status;

            if (status == ServiceControllerStatus.Running)
            {
                _output?.WriteLine($"Service '{serviceName}' is already running.");
                return true;
            }

            if (status == ServiceControllerStatus.Stopped ||
                status == ServiceControllerStatus.Paused)
            {
                _output?.WriteLine($"Starting service '{serviceName}'...");
                sc.Start();
                sc.WaitForStatus(
                    ServiceControllerStatus.Running,
                    TimeSpan.FromSeconds(30));
                _output?.WriteLine($"Service '{serviceName}' started.");
                return true;
            }

            _output?.WriteLine($"Service '{serviceName}' status: {status}");
            return false;
        }
        catch (System.ServiceProcess.TimeoutException)
        {
            _output?.WriteLine($"Timeout waiting for service '{serviceName}' to start.");
            return false;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            // Access denied (5) → IIS App Pool hat keine Rechte zum Starten von Diensten
            _output?.WriteLine($"Service '{serviceName}' access denied (Win32 error {ex.NativeErrorCode}): {ex.Message}");
            _output?.WriteLine($"Falling back to direct process start.");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            // Dienst nicht installiert oder Zugriff verweigert
            _output?.WriteLine($"Service '{serviceName}' not available: {ex.Message}");
            _output?.WriteLine($"Falling back to direct process start.");
            return false;
        }
        catch (Exception ex)
        {
            _output?.WriteLine($"TryStartWindowsService Exception: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private bool StartServerProcess(string serverExe, ProcessWindowStyle windowStyle, string userName, string password)
    {
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
        bool hasCredentials = !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password);
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = serverExeFullPath,
            WorkingDirectory = Path.GetDirectoryName(serverExeFullPath) ?? Environment.CurrentDirectory,
        };

        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = windowStyle == ProcessWindowStyle.Hidden || windowStyle == ProcessWindowStyle.Minimized;
        processStartInfo.LoadUserProfile = true;

        if (hasCredentials)
        {
            processStartInfo.UserName = userName;
            processStartInfo.Password = CreateSecureString(password);
        }

        Process process = Process.Start(processStartInfo);
        if (process == null || process.HasExited)
        {
            _output?.WriteLine("Failed to start server process.");
            return false;
        }

        _output?.WriteLine($"Server process started (PID: {process.Id}).");
        return true;
    }

    public static string DetectServerLocation(bool preferNet481 = false)
    {
        string assemblyDir = AssemblyDirectory;
        if (string.IsNullOrEmpty(assemblyDir))
        {
            return null;
        }

#if DEBUG
        string prefix = "debug";
#else
        string prefix = "release";
#endif

        string rootDir = assemblyDir;
        for (int i = 0; i < 5; i++)
        {
            rootDir = Directory.GetParent(rootDir)?.FullName;
            if (rootDir == null)
            {
                break;
            }

            string artifactsBinDir = Path.Combine(rootDir, ServerDirName, "artifacts", "bin", ServerDirName);
            if (Directory.Exists(artifactsBinDir))
            {
                string serverDirNet10 = Path.Combine(artifactsBinDir, prefix + "_net10.0-windows10.0.26100.0");
                string serverDirNet481 = Path.Combine(artifactsBinDir, prefix + "_net481");
                string serverExeNet10 = Path.Combine(serverDirNet10, ServerExeName);
                string serverExeNet481 = Path.Combine(serverDirNet481, ServerExeName);

                if (preferNet481)
                {
                    if (File.Exists(serverExeNet481))
                    {
                        return serverExeNet481;
                    }

                    if (File.Exists(serverExeNet10))
                    {
                        return serverExeNet10;
                    }
                }
                else
                {
                    if (File.Exists(serverExeNet10))
                    {
                        return serverExeNet10;
                    }

                    if (File.Exists(serverExeNet481))
                    {
                        return serverExeNet481;
                    }
                }
            }

            string serverExe = Path.Combine(rootDir, ServerDirName, ServerExeName);
            if (File.Exists(serverExe))
            {
                return serverExe;
            }
        }

        return null;
    }

    public static string GetProgramsServerLocation()
    {
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (string.IsNullOrEmpty(programFiles))
        {
            return null;
        }

        string serverExe = Path.Combine(programFiles, ServerDirName, ServerExeName);
        if (File.Exists(serverExe))
        {
            return serverExe;
        }

        return null;
    }

    public static SecureString CreateSecureString(string plainText)
    {
        SecureString secure = new SecureString();
        foreach (char c in plainText)
        {
            secure.AppendChar(c);
        }
        secure.MakeReadOnly();
        return secure;
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