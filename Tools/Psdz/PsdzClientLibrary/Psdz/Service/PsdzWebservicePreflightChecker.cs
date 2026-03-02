using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.Psdz
{
    public class PsdzWebservicePreflightChecker : IPsdzWebservicePreflightChecker
    {
        private const string JavaInstallationErrorMessage = "{0} Validation - {1}";

        private const int JavaMajorVersion = 17;

        private const int JavaLaunchProbeTimeoutSeconds = 10;

        private readonly string _logDir;

        private readonly Func<string, string, Process> _createBaseProcess;

        private readonly Func<string, string, Process> _createMonitoredProcess;

        public PsdzWebservicePreflightChecker(string logDir, Func<string, string, Process> createBaseProcess, Func<string, string, Process> createMonitoredProcess)
        {
            _logDir = logDir;
            _createBaseProcess = createBaseProcess;
            _createMonitoredProcess = createMonitoredProcess;
        }

        public void Execute(int sessionWebservicePort, string jarPath, string javaExePath)
        {
            ValidateJavaRuntime(javaExePath);
            ValidateJavaVersion(javaExePath);
            ValidateJarExistenceAndIntegrity(jarPath);
            ProbeJarLaunch(javaExePath, jarPath);
            ValidatePortAvailable(sessionWebservicePort);
            ValidateJarReadable(jarPath);
            ValidateLogDirectoryWritable();
        }

        private void ValidateJavaRuntime(string javaExePath)
        {
            if (!File.Exists(javaExePath))
            {
                string msg = string.Format("{0} Validation - {1}", "Java Executable", $"java.exe not found at {javaExePath}. JDK {17} is required for the PSdZ Webservice.");
                Log.Error(Log.CurrentMethod(), msg);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JavaRuntimeInstallationFaulty);
            }
        }

        private void ValidateJavaVersion(string javaExePath)
        {
            string javaVersionOutput = GetJavaVersionOutput(javaExePath);
            AssertRequiredJavaVersion(javaVersionOutput);
        }

        private string GetJavaVersionOutput(string javaExePath)
        {
            try
            {
                Process process = _createBaseProcess(javaExePath, "-version");
                process.Start();
                string text = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Log.Info(Log.CurrentMethod(), "JDK Version Info:\n{0}", text);
                return text;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JavaRuntimeFaulty);
            }
        }

        private void AssertRequiredJavaVersion(string versionOutput)
        {
            if (!TryValidateJavaVersion(versionOutput, out var _))
            {
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JavaVersionError);
            }
        }

        private bool TryValidateJavaVersion(string checkVersionProcessOutput, out string error)
        {
            Match match = Regex.Match(checkVersionProcessOutput, "\\b(\\d+)(?:\\.\\d+\\.\\d+)?");
            if (match.Success)
            {
                int num = Convert.ToInt32(match.Groups[1]?.Value);
                if (num < 17)
                {
                    error = string.Format("{0} Validation - {1}", "Java Version", $"Wrong Java major version installed: {num} - Expected {17}.");
                    Log.Error(Log.CurrentMethod(), error);
                    return false;
                }
            }
            error = null;
            return true;
        }

        private void ValidateJarExistenceAndIntegrity(string jarPath)
        {
            if (jarPath == null || !File.Exists(jarPath))
            {
                string msg = string.Format("{0} Validation - {1}", "Jar Path", "Path " + (jarPath ?? "<null>") + " for the .jar file not found!");
                Log.Error(Log.CurrentMethod(), msg);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JarMissingOrCorrupt);
            }
            FileInfo fileInfo = new FileInfo(jarPath);
            if (fileInfo.Length < 1024)
            {
                string msg2 = $"PSdZ Webservice JAR appears too small ({fileInfo.Length} bytes).";
                Log.Error(Log.CurrentMethod(), msg2);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JarMissingOrCorrupt);
            }
        }

        private void ProbeJarLaunch(string javaExePath, string jarPath)
        {
            string arg = "-jar \"" + jarPath + "\" --spring.main.web-application-type=none --spring.main.lazy-initialization=true --probe.launch=true";
            Process process = _createMonitoredProcess(javaExePath, arg);
            process.EnableRaisingEvents = false;
            try
            {
                process.Start();
                if (!process.WaitForExit(10000))
                {
                    TryKillTree(process);
                    Log.Error(Log.CurrentMethod(), $"Launch probe exceeded {10}s timeout.");
                    throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.Timeout);
                }
            }
            catch (PsdzWebserviceStartException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), $"Launch probe exceeded {10}s timeout.", exception);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JavaRuntimeFaulty);
            }
            finally
            {
                try
                {
                    process.Dispose();
                }
                catch
                {
                }
            }
        }

        private void ValidatePortAvailable(int port)
        {
            if (!NetUtils.CanBindTcpPort(port))
            {
                Log.Error(Log.CurrentMethod(), $"Port {port} is already in use.");
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.PortInUse);
            }
        }

        private void ValidateJarReadable(string jarPath)
        {
            try
            {
                using (File.OpenRead(jarPath))
                {
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                Log.ErrorException(Log.CurrentMethod(), "Access denied to JAR '" + jarPath + "'", exception);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.AccessDenied);
            }
        }

        private void ValidateLogDirectoryWritable()
        {
            try
            {
                string path = Path.Combine(_logDir, "write_probe.tmp");
                File.WriteAllText(path, "probe");
                File.Delete(path);
            }
            catch (UnauthorizedAccessException exception)
            {
                Log.ErrorException(Log.CurrentMethod(), "Write permission missing in log directory '" + _logDir + "'", exception);
                throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.MissingRights);
            }
        }

        private static bool TryKillTree(Process proc, int timeoutMs = 10000)
        {
            if (proc == null)
            {
                return true;
            }
            try
            {
                proc.Refresh();
                if (proc.HasExited)
                {
                    return true;
                }
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/PID {proc.Id} /T /F",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }))
                {
                    process?.WaitForExit(timeoutMs);
                }
                proc.Refresh();
                if (proc.HasExited)
                {
                    return true;
                }
                proc.Kill();
                return proc.WaitForExit(Math.Max(3000, timeoutMs / 3));
            }
            catch
            {
                try
                {
                    proc.Refresh();
                    return proc.HasExited;
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                try
                {
                    proc.Close();
                }
                catch
                {
                }
            }
        }
    }
}