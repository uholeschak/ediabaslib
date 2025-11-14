using System;

namespace PsdzClient.Core
{
    [Obsolete("It has been moved to Authroing API.")]
    public enum OperationalMode
    {
        ISTA,
        ISTA_PLUS,
        ISTA_LIGHT,
        ISTA_POWERTRAIN,
        RITA,
        ISTAHV,
        [Obsolete("This OperationalMode is only used by Testmodules. ISTA does not have anymore TELESERVICE")]
        TELESERVICE,
        [Obsolete("This OperationalMode is only used by Testmodules. ISTA does not have anymore TeleServiceConsole")]
        TeleServiceConsole
    }
}