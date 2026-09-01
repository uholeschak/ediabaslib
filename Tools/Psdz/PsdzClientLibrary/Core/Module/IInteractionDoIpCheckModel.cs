using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    public interface IInteractionDoIpCheckModel : IInteractionModel, INotifyPropertyChanged
    {
        int SourceOperationPid { get; set; }
    }
}
