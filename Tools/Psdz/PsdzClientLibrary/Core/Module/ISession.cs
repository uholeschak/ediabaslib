using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Feedback;

namespace BMW.Rheingold.CoreFramework
{
    public interface ISession : INotifyPropertyChanged
    {
        Vehicle VecInfo { get; }

        IFFMDynamicResolver FFMResolver { get; }

        IFasta2Service Fasta2Service { get; }

        IFeedbackViewHeaderTitleHelper FeedbackViewHeaderTitleHelper { get; }
    }
}
