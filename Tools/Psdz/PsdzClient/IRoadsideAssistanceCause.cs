using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IRoadsideAssistanceCause : INotifyPropertyChanged
    {
        uint? Cause { get; }

        uint? TowedAway { get; }

        uint? Repaired { get; }

        uint? Completed { get; }
    }
}
