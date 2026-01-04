using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PsdzClient.Programming
{
    internal class EscalationStep : IEscalationStep
    {
        private readonly ICollection<IProgrammingFailure> errors;

        public ProgrammingActionState State { get; internal set; }

        public int Step { get; internal set; }

        public DateTime StartTime { get; internal set; }

        public DateTime EndTime { get; internal set; }

        public IEnumerable<IProgrammingFailure> Errors => errors;

        public EscalationStep()
        {
            errors = new List<IProgrammingFailure>();
        }

        internal void AddErrorList(IEnumerable<IPsdzTalLine> talLines)
        {
            foreach (IPsdzTalLine talLine in talLines)
            {
                if (!talLine.HasFailureCauses)
                {
                    continue;
                }
                foreach (IPsdzFailureCause failureCause in talLine.FailureCauses)
                {
                    ProgrammingFailure programmingFailure = new ProgrammingFailure();
                    programmingFailure.Id = failureCause.Id;
                    programmingFailure.MessageId = failureCause.MessageId.ToString(CultureInfo.InvariantCulture);
                    string message = failureCause.Message;
                    message = ((message.Length > 255) ? $"{message.Substring(0, 252)}..." : message);
                    programmingFailure.Message = message;
                    errors.Add(programmingFailure);
                }
            }
        }
    }
}