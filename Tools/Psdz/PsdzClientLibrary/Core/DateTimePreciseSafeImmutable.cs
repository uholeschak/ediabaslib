using System;

namespace PsdzClientLibrary.Core
{
    internal sealed class DateTimePreciseSafeImmutable
    {
        internal readonly DateTime t_observed;

        internal readonly DateTime t_base;

        internal readonly long s_observed;

        internal readonly long stopWatchFrequency;

        internal DateTimePreciseSafeImmutable(DateTime t_observed, DateTime t_base, long s_observed, long stopWatchFrequency)
        {
            this.t_observed = t_observed;
            this.t_base = t_base;
            this.s_observed = s_observed;
            this.stopWatchFrequency = stopWatchFrequency;
        }
    }
}
