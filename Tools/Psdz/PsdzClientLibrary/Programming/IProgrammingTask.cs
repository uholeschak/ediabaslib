using System;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [Flags]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ProgrammingTaskFlags
    {
        Mount = 1,
        Unmount = 2,
        Replace = 4,
        Flash = 0x10,
        Code = 0x20,
        DataRecovery = 0x40,
        Fsc = 0x80,
        EnforceCoding = 0x100,
        PrepareForCarSharing = 0x200,
        RetroFitSa620 = 0x400
    }

    public interface IProgrammingTask
    {
        ProgrammingTaskFlags Flags { get; }

        string Id { get; }

        string Name { get; }
    }
}