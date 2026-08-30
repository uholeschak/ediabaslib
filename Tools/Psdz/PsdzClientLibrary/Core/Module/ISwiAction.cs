using System.ComponentModel;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;

#pragma warning disable CS8632
namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ISwiAction : INotifyPropertyChanged
    {
        [PreserveSource(Hint = "XEP_SWIACTION", Placeholder = true)]
        PlaceholderType XepSwiAction { get; }

        decimal Id { get; }

        bool IsSelected { get; set; }

        bool IsDisabled { get; set; }

        bool IsPlanned { get; set; }

        bool IsHidden { get; }

        [PreserveSource(Placeholder = true)]
        PsdzDatabase.SwiRegister? Register { get; }

        string EcuId { get; }

        SwiActionType Type { get; }

        ISwiAction Data { get; }

        [PreserveSource(Placeholder = true)]
        void ExecuteServiceProgramms(SwiActionLinkType type, IProgrammingSessionExt session);
    }
}