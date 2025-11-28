using System;

namespace PsdzClient.Programming
{
    [Flags]
    public enum EcuScheduledState
    {
        NothingScheduled = 0,
        ProgrammingScheduledByLogistic = 1,
        ProgrammingScheduledByUser = 2,
        CodingScheduledByLogistic = 4,
        CodingScheduledByUser = 8
    }
}