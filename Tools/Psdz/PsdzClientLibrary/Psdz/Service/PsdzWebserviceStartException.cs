using System;
using BMW.Rheingold.Psdz;

namespace BMW.Rheingold.Psdz
{
    public class PsdzWebserviceStartException : Exception
    {
        private const string JavaInstallationErrorMessage = "Java runtime validation failed: java.exe not found. Correct JDK required.";

        public PsdzWebserviceStartFailureReason Reason { get; }

        public string ServiceCode { get; }

        private PsdzWebserviceStartException(PsdzWebserviceStartFailureReason reason, string message, string serviceCode, Exception inner = null)
            : base(message, inner)
        {
            Reason = reason;
            ServiceCode = serviceCode;
        }

        public static PsdzWebserviceStartException Create(PsdzWebserviceStartFailureReason reason)
        {
            string serviceCode = MapReasonToServiceCode(reason);
            string message = MapReasonToDefaultMessage(reason);
            return new PsdzWebserviceStartException(reason, message, serviceCode);
        }

        public static string MapReasonToServiceCode(PsdzWebserviceStartFailureReason reason)
        {
            switch (reason)
            {
                case PsdzWebserviceStartFailureReason.PortInUse:
                    return "PWS06_PsdzWebservicePortInUse_nu_LF";
                case PsdzWebserviceStartFailureReason.JarMissingOrCorrupt:
                    return "PWS07_PsdzWebserviceJarMissingOrCorrupt_nu_LF";
                case PsdzWebserviceStartFailureReason.JavaRuntimeFaulty:
                    return "PWS03_JavaInitializationValidationError_nu_LF";
                case PsdzWebserviceStartFailureReason.SpringBootStartupError:
                    return "PWS11_PsdzWebserviceSpringBootError_nu_LF";
                case PsdzWebserviceStartFailureReason.Timeout:
                    return "PWS10_PsdzWebserviceStartupTimeout_nu_LF";
                case PsdzWebserviceStartFailureReason.AccessDenied:
                    return "PWS08_PsdzWebserviceJarAccessDenied_nu_LF";
                case PsdzWebserviceStartFailureReason.MissingRights:
                    return "PWS9_PsdzWebserviceLogDirNoWrite_nu_LF";
                case PsdzWebserviceStartFailureReason.SdpFaulty:
                    return "PWS12_PsdzWebserviceSdpFaulty_nu_LF";
                case PsdzWebserviceStartFailureReason.JavaRuntimeInstallationFaulty:
                case PsdzWebserviceStartFailureReason.JavaVersionError:
                    return "PWS02_JavaInstalationValidationError_nu_LF";
                default:
                    return "PWS05_WebserviceStartRuntimeError_nu_LF";
            }
        }

        private static string MapReasonToDefaultMessage(PsdzWebserviceStartFailureReason reason)
        {
            switch (reason)
            {
                case PsdzWebserviceStartFailureReason.PortInUse:
                    return "Webservice port already in use.";
                case PsdzWebserviceStartFailureReason.JarMissingOrCorrupt:
                    return "Required webservice JAR missing or corrupt.";
                case PsdzWebserviceStartFailureReason.JavaRuntimeFaulty:
                    return "Java runtime initialization failed.";
                case PsdzWebserviceStartFailureReason.SpringBootStartupError:
                    return "Spring Boot startup error detected.";
                case PsdzWebserviceStartFailureReason.Timeout:
                    return "Webservice startup timed out.";
                case PsdzWebserviceStartFailureReason.AccessDenied:
                    return "Access denied to webservice JAR.";
                case PsdzWebserviceStartFailureReason.MissingRights:
                    return "Insufficient rights for log directory.";
                case PsdzWebserviceStartFailureReason.SdpFaulty:
                    return "SDP data faulty.";
                case PsdzWebserviceStartFailureReason.JavaVersionError:
                    return "Wrong Java version found.";
                case PsdzWebserviceStartFailureReason.JavaRuntimeInstallationFaulty:
                    return "Java runtime validation failed: java.exe not found. Correct JDK required.";
                default:
                    return "Webservice runtime error.";
            }
        }
    }
}