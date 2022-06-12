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

    [AuthorAPI(SelectableTypeDeclaration = true)]
    [Flags]
    public enum ProgrammingActionType
    {
        Programming = 2,
        BootloaderProgramming = 4,
        DataProgramming = 8,
        Coding = 16,
        Unmounting = 32,
        Mounting = 64,
        Replacement = 128,
        FscBakup = 256,
        FscStore = 1024,
        FscActivate = 2048,
        FscDeactivate = 4096,
        HddUpdate = 8192,
        IdSave = 16384,
        IdRestore = 32768,
        IbaDeploy = 65536,
        SFAWrite = 131072,
        SFADelete = 262144,
        SFAVerfy = 524288
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
