using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class ProgrammingTaskEventArgs : ProgrammingEventArgs
    {
        public ProgrammingTaskEventArgs(DateTime timestamp, string taskName, double progress, double timeLeftSec, bool isTaskFinished) : base(timestamp)
        {
            this.taskName = taskName;
            this.progress = progress;
            this.timeLeftSec = timeLeftSec;
            this.isTaskFinished = isTaskFinished;
        }

        public bool IsTaskFinished
        {
            get
            {
                return this.isTaskFinished;
            }
        }

        public double Progress
        {
            get
            {
                return this.progress;
            }
        }

        public string TaskName
        {
            get
            {
                return this.taskName;
            }
        }

        public double TimeLeftSec
        {
            get
            {
                return this.timeLeftSec;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Task: {0} - Progress: {1} - Time left [s]: {2} - Task finished: {3}", new object[]
            {
                this.taskName ?? "<null>",
                this.progress,
                this.timeLeftSec,
                this.isTaskFinished
            });
        }

        private readonly bool isTaskFinished;

        private readonly double progress;

        private readonly string taskName;

        private readonly double timeLeftSec;
    }
}
