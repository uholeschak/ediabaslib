using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
    internal class PsdzProgressListener : IPsdzProgressListener
    {
        private readonly ProgrammingEventManager eventManager;
        private long durationInMilliseconds;
        private string task;
        public PsdzProgressListener(ProgrammingEventManager eventManager)
        {
            this.eventManager = eventManager;
        }

        public void BeginTask(string task)
        {
            this.task = task;
        }

        public void SetDuration(long milliseconds)
        {
            durationInMilliseconds = milliseconds;
        }

        public void SetElapsedTime(long milliseconds)
        {
            double num = ((durationInMilliseconds > 0) ? ((double)milliseconds / (double)durationInMilliseconds) : 0.0);
            double num2 = (double)(durationInMilliseconds - milliseconds) / 1000.0;
            eventManager.OnProgressChanged(task, (num > 1.0) ? 1.0 : num, (num2 < 0.0) ? 0.0 : num2, isTaskFinished: false);
        }

        public void SetFinished()
        {
            eventManager.OnProgressChanged(task, 1.0, 0.0, isTaskFinished: true);
        }
    }
}