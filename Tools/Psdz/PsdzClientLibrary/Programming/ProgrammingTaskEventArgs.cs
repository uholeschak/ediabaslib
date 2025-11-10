using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class ProgrammingTaskEventArgs : ProgrammingEventArgs
    {
        private readonly bool isTaskFinished;
        private readonly double progress;
        private readonly string taskName;
        private readonly double timeLeftSec;
        public bool IsTaskFinished => isTaskFinished;
        public double Progress => progress;
        public string TaskName => taskName;
        public double TimeLeftSec => timeLeftSec;

        public ProgrammingTaskEventArgs(DateTime timestamp, string taskName, double progress, double timeLeftSec, bool isTaskFinished) : base(timestamp)
        {
            this.taskName = taskName;
            this.progress = progress;
            this.timeLeftSec = timeLeftSec;
            this.isTaskFinished = isTaskFinished;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Task: {0} - Progress: {1} - Time left [s]: {2} - Task finished: {3}", taskName ?? "<null>", progress, timeLeftSec, isTaskFinished);
        }
    }
}