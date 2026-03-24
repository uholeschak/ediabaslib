using PsdzRpcServer.Shared;
using System;
using System.Diagnostics;
using System.IO;

namespace PsdzRpcClient;

public static class PsdzRpcServerStarter
{
    /// <summary>
    /// Prüft ob die Named Pipe des Servers existiert.
    /// </summary>
    public static bool IsPipeAvailable()
    {
        return File.Exists($@"\\.\pipe\{PsdzRpcServiceConstants.PipeName}");
    }

    /// <summary>
    /// Startet den Server-Prozess falls ein Pfad angegeben ist.
    /// Wartet nur bis der Prozess gestartet ist, nicht bis die Pipe verfügbar ist.
    /// </summary>
    public static bool StartServerIfNeeded(string serverExe, out Process serverProcess)
    {
        serverProcess = null;

        // Prüfen ob der Server eventuell schon läuft (Pipe könnte noch nicht bereit sein)
        if (IsPipeAvailable())
        {
            Console.WriteLine("Server pipe detected, server is already running.");
            return true;
        }

        if (string.IsNullOrEmpty(serverExe))
        {
            Console.WriteLine("No server executable specified.");
            return false;
        }

        string serverExeFullPath = Path.IsPathRooted(serverExe)
            ? serverExe
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, serverExe));

        if (!File.Exists(serverExeFullPath))
        {
            Console.WriteLine($"Server executable not found: {serverExeFullPath}");
            return false;
        }

        Console.WriteLine($"Starting server: {serverExeFullPath}");
        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = serverExeFullPath,
            WorkingDirectory = Path.GetDirectoryName(serverExeFullPath) ?? Environment.CurrentDirectory,
            UseShellExecute = true,
        });

        if (process == null || process.HasExited)
        {
            Console.WriteLine("Failed to start server process.");
            return false;
        }

        Console.WriteLine($"Server process started (PID: {process.Id}).");
        serverProcess = process;
        return true;
    }

}