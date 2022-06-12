using System;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [Flags]
    public enum ProgrammingTaskFlags
    {
        Mount = 1,
        Unmount = 2,
        Replace = 4,
        Flash = 16,
        Code = 32,
        DataRecovery = 64,
        Fsc = 128,
        EnforceCoding = 256,
        PrepareForCarSharing = 512,
        RetroFitSa620 = 1024
    }
    
    public interface IProgrammingTask
    {
        ProgrammingTaskFlags Flags { get; }

        string Id { get; }

        string Name { get; }
    }
}