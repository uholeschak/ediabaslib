using System.Diagnostics;
using System;

namespace PsdzClient.Core
{
    public class DateTimePrecise
    {
        private const long ClockTickFrequency = 10000000L;
        private Stopwatch stopwatch;
        private long synchronizePeriodStopwatchTicks;
        private long synchronizePeriodSeconds;
        private long synchronizePeriodClockTicks;
        private DateTimePreciseSafeImmutable immutable;
        public DateTime UtcNow
        {
            get
            {
                long elapsedTicks = stopwatch.ElapsedTicks;
                DateTimePreciseSafeImmutable dateTimePreciseSafeImmutable = immutable;
                if (elapsedTicks < dateTimePreciseSafeImmutable.s_observed + synchronizePeriodStopwatchTicks)
                {
                    return dateTimePreciseSafeImmutable.t_base.AddTicks((elapsedTicks - dateTimePreciseSafeImmutable.s_observed) * 10000000 / dateTimePreciseSafeImmutable.stopWatchFrequency);
                }

                DateTime utcNow = DateTime.UtcNow;
                DateTime dateTime = dateTimePreciseSafeImmutable.t_base.AddTicks((elapsedTicks - dateTimePreciseSafeImmutable.s_observed) * 10000000 / dateTimePreciseSafeImmutable.stopWatchFrequency);
                immutable = new DateTimePreciseSafeImmutable(utcNow, dateTime, elapsedTicks, (elapsedTicks - dateTimePreciseSafeImmutable.s_observed) * 10000000 * 2 / (utcNow.Ticks - dateTimePreciseSafeImmutable.t_observed.Ticks + utcNow.Ticks + utcNow.Ticks - dateTime.Ticks - dateTimePreciseSafeImmutable.t_observed.Ticks));
                return dateTime;
            }
        }

        public DateTime Now => UtcNow.ToLocalTime();

        public Stopwatch Stopwatch
        {
            get
            {
                return stopwatch;
            }

            set
            {
                stopwatch = value;
            }
        }

        public DateTimePrecise(long synchronizePeriodSeconds)
        {
            stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            DateTime utcNow = DateTime.UtcNow;
            immutable = new DateTimePreciseSafeImmutable(utcNow, utcNow, stopwatch.ElapsedTicks, Stopwatch.Frequency);
            this.synchronizePeriodSeconds = synchronizePeriodSeconds;
            synchronizePeriodStopwatchTicks = synchronizePeriodSeconds * Stopwatch.Frequency;
            synchronizePeriodClockTicks = synchronizePeriodSeconds * 10000000;
        }
    }
}