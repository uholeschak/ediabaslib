using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ProgrammingActionState
    {
        ActionPlanned = 4,
        ActionInProcess = 32,
        ActionSuccessful = 1,
        ActionFailed = 16,
        MissingPrerequisitesForAction = 8,
        ActionWarning = 2
    }

    [Flags]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ProgrammingActionType
    {
        Programming = 2,
        BootloaderProgramming = 4,
        DataProgramming = 8,
        Coding = 0x10,
        Unmounting = 0x20,
        Mounting = 0x40,
        Replacement = 0x80,
        FscBakup = 0x100,
        FscStore = 0x400,
        FscActivate = 0x800,
        FscDeactivate = 0x1000,
        HddUpdate = 0x2000,
        IdSave = 0x4000,
        IdRestore = 0x8000,
        IbaDeploy = 0x10000,
        SFAWrite = 0x20000,
        SFADelete = 0x40000,
        SFAVerfy = 0x80000,
        HDDUpdateAndroid = 0x100000
    }

    public interface IEcuProgrammingInfo : INotifyPropertyChanged
    {
        IEcu Ecu { get; }

        IEnumerable<IProgrammingAction> ProgrammingActions { get; }

        double ProgressValue { get; }

        ProgrammingActionState? State { get; }

        bool IsCodingDisabled { get; set; }

        bool IsProgrammingDisabled { get; set; }

        bool IsProgrammingSelectionDisabled { get; set; }

        bool IsCodingSelectionDisabled { get; set; }

        bool IsCodingScheduled { get; set; }

        bool IsProgrammingScheduled { get; set; }

        IStandardSvk SvkCurrent { get; }

        IStandardSvk SvkTarget { get; }

        bool IsExchangeScheduled { get; set; }

        bool IsExchangeDone { get; set; }

        string EcuIdentifier { get; }

        int FlashOrder { get; }

        IProgrammingAction GetProgrammingAction(ProgrammingActionType type);

        IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter);
    }
}
