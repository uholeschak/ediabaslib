using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
    public class PsdzProgressListener : IPsdzProgressListener
    {
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
            this.durationInMilliseconds = milliseconds;
        }

        public void SetElapsedTime(long milliseconds)
        {
            double num = (this.durationInMilliseconds > 0L) ? ((double)milliseconds / (double)this.durationInMilliseconds) : 0.0;
            double num2 = (double)(this.durationInMilliseconds - milliseconds) / 1000.0;
            this.eventManager.OnProgressChanged(this.task, (num > 1.0) ? 1.0 : num, (num2 < 0.0) ? 0.0 : num2, false);
        }

        public void SetFinished()
        {
            this.eventManager.OnProgressChanged(this.task, 1.0, 0.0, true);
        }

        private readonly ProgrammingEventManager eventManager;

        private long durationInMilliseconds;

        private string task;
    }
}
